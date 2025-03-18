using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace DllChecker
{
    public partial class frmMain : Form
    {
        private string selectedFolder = string.Empty;

        public frmMain()
        {
            InitializeComponent();
            // 如果有儲存上次選擇的資料夾，則載入之
            if (!string.IsNullOrEmpty(Properties.Settings.Default.LastFolder))
            {
                selectedFolder = Properties.Settings.Default.LastFolder;
                lblFolder.Text = selectedFolder;
            }
        }

        /// <summary>
        /// "Select Folder" 按鈕點擊事件：選取資料夾並記住選擇結果
        /// </summary>
        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (!string.IsNullOrEmpty(selectedFolder))
                {
                    fbd.SelectedPath = selectedFolder;
                }
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    selectedFolder = fbd.SelectedPath;
                    lblFolder.Text = selectedFolder;
                    // 儲存選擇的資料夾到使用者設定
                    Properties.Settings.Default.LastFolder = selectedFolder;
                    Properties.Settings.Default.Save();
                }
            }
        }

        /// <summary>
        /// "Scan Files" 按鈕點擊事件：掃描選定資料夾及其子資料夾中的 DLL 與 EXE 檔案，
        /// 並在最後輸出總結（統計各類型檔案數量）。
        /// </summary>
        private void btnScan_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFolder))
            {
                MessageBox.Show("Please select a folder first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            rtbResults.Clear();
            int counter = 1;
            int countAnyCPU = 0, count32 = 0, count64 = 0, countOther = 0;

            // 掃描 DLL 檔案（結果皆以綠色輸出）
            string[] dllFiles = Directory.GetFiles(selectedFolder, "*.dll", SearchOption.AllDirectories);
            if (dllFiles.Length > 0)
            {
                AppendResult("Scanning DLL files:" + Environment.NewLine, Color.Blue);
                foreach (string file in dllFiles)
                {
                    string relativePath = GetRelativePath(selectedFolder, file);
                    AppendResult($"{counter}. File: {relativePath}" + Environment.NewLine, Color.Green);
                    string result = GetFileArchitecture(file, out string procArchLine, out string typeLine);
                    AppendResult("     " + procArchLine + Environment.NewLine, Color.Green);
                    AppendResult("     " + typeLine + Environment.NewLine, Color.Green);
                    AppendResult("     Result: " + result + Environment.NewLine, Color.Green);
                    AppendResult(new string('-', 25) + Environment.NewLine, Color.Green);
                    counter++;
                    // 統計計數
                    if (result.IndexOf("AnyCPU", StringComparison.OrdinalIgnoreCase) >= 0)
                        countAnyCPU++;
                    else if (result.IndexOf("32-bit only", StringComparison.OrdinalIgnoreCase) >= 0)
                        count32++;
                    else if (result.IndexOf("64-bit only", StringComparison.OrdinalIgnoreCase) >= 0)
                        count64++;
                    else
                        countOther++;
                }
            }
            else
            {
                AppendResult("No DLL files found." + Environment.NewLine, Color.Green);
            }

            // 掃描 EXE 檔案（結果如果包含 "only" 則以紅色顯示，否則預設黑色）
            string[] exeFiles = Directory.GetFiles(selectedFolder, "*.exe", SearchOption.AllDirectories);
            if (exeFiles.Length > 0)
            {
                AppendResult(Environment.NewLine + "Scanning EXE files:" + Environment.NewLine, Color.Blue);
                foreach (string file in exeFiles)
                {
                    string relativePath = GetRelativePath(selectedFolder, file);
                    AppendResult($"{counter}. File: {relativePath}" + Environment.NewLine, SystemColors.ControlText);
                    string result = GetFileArchitecture(file, out string procArchLine, out string typeLine);
                    Color exeResultColor = result.ToLower().Contains("only") ? Color.Red : SystemColors.ControlText;
                    AppendResult("     " + procArchLine + Environment.NewLine, exeResultColor);
                    AppendResult("     " + typeLine + Environment.NewLine, exeResultColor);
                    AppendResult("     Result: " + result + Environment.NewLine, exeResultColor);
                    AppendResult(new string('-', 25) + Environment.NewLine, SystemColors.ControlText);
                    counter++;
                    // 統計計數
                    if (result.IndexOf("AnyCPU", StringComparison.OrdinalIgnoreCase) >= 0)
                        countAnyCPU++;
                    else if (result.IndexOf("32-bit only", StringComparison.OrdinalIgnoreCase) >= 0)
                        count32++;
                    else if (result.IndexOf("64-bit only", StringComparison.OrdinalIgnoreCase) >= 0)
                        count64++;
                    else
                        countOther++;
                }
            }
            else
            {
                AppendResult(Environment.NewLine + "No EXE files found." + Environment.NewLine, SystemColors.ControlText);
            }

            // 輸出總結
            AppendResult(Environment.NewLine + "Summary:" + Environment.NewLine, Color.Blue);
            AppendResult($"   AnyCPU: {countAnyCPU}" + Environment.NewLine, Color.Blue);
            AppendResult($"   64-bit only: {count64}" + Environment.NewLine, Color.Blue);
            AppendResult($"   32-bit only: {count32}" + Environment.NewLine, Color.Blue);
            if (countOther > 0)
                AppendResult($"   Other: {countOther}" + Environment.NewLine, Color.Blue);
        }

        /// <summary>
        /// 將指定文字以指定顏色附加到 RichTextBox 中
        /// </summary>
        private void AppendResult(string text, Color color)
        {
            rtbResults.SelectionStart = rtbResults.TextLength;
            rtbResults.SelectionLength = 0;
            rtbResults.SelectionColor = color;
            rtbResults.AppendText(text);
            rtbResults.SelectionColor = rtbResults.ForeColor; // 重設為預設顏色
        }

        /// <summary>
        /// 判斷檔案（DLL 或 EXE）的架構，並傳回判定依據。
        /// 對於 Managed 程式集，利用 AssemblyName 取得 ProcessorArchitecture。
        /// 對於原生 PE 檔案，則讀取 PE 標頭。
        /// 回傳描述性字串 (Result)，並透過 out 參數傳回：
        /// procArchLine：例如 "ProcessorArchitecture = MSIL."
        /// typeLine：例如 "Type: Managed assembly" 或 "Type: Native PE file"
        /// </summary>
        private string GetFileArchitecture(string filePath, out string procArchLine, out string typeLine)
        {
            try
            {
                // 嘗試以 Managed 程式集方式載入
                AssemblyName asmName = AssemblyName.GetAssemblyName(filePath);
                string archStr = asmName.ProcessorArchitecture.ToString();
                procArchLine = "ProcessorArchitecture = " + archStr + ".";
                typeLine = "Type: Managed assembly";
                var arch = asmName.ProcessorArchitecture;
                if (arch == ProcessorArchitecture.MSIL)
                    return "AnyCPU (runs on both 32-bit and 64-bit EXEs)";
                else if (arch == ProcessorArchitecture.X86)
                    return "32-bit only (runs only on 32-bit EXEs)";
                else if (arch == ProcessorArchitecture.Amd64)
                    return "64-bit only (runs only on 64-bit EXEs)";
                else
                    return "Unknown Managed Architecture";
            }
            catch
            {
                // 非 Managed 程式集，嘗試以原生 PE 檔案方式判斷
                try
                {
                    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    using (var br = new BinaryReader(fs))
                    {
                        // 檢查 DOS 標頭 "MZ"
                        ushort mz = br.ReadUInt16();
                        if (mz != 0x5A4D)
                        {
                            procArchLine = "Invalid PE file: Missing 'MZ' header.";
                            typeLine = "";
                            return "Invalid PE file";
                        }
                        fs.Seek(0x3C, SeekOrigin.Begin);
                        int peHeaderOffset = br.ReadInt32();
                        fs.Seek(peHeaderOffset, SeekOrigin.Begin);
                        uint peHeader = br.ReadUInt32();
                        if (peHeader != 0x00004550)
                        {
                            procArchLine = "Invalid PE file: Missing 'PE\\0\\0' header.";
                            typeLine = "";
                            return "Invalid PE file";
                        }
                        ushort machine = br.ReadUInt16();
                        procArchLine = "Machine type = 0x" + machine.ToString("X4") + ".";
                        typeLine = "Type: Native PE file";
                        if (machine == 0x014c)
                            return "32-bit only (native)";
                        else if (machine == 0x8664)
                            return "64-bit only (native)";
                        else
                            return "Unknown native architecture";
                    }
                }
                catch (Exception ex)
                {
                    procArchLine = "Detection failed: " + ex.Message;
                    typeLine = "";
                    return procArchLine;
                }
            }
        }

        /// <summary>
        /// 取得 fullPath 相對於 basePath 的相對路徑
        /// </summary>
        private string GetRelativePath(string basePath, string fullPath)
        {
            string basePathWithSep = EnsureTrailingSeparator(basePath);
            Uri baseUri = new Uri(basePathWithSep);
            Uri fullUri = new Uri(fullPath);
            string relativeUri = Uri.UnescapeDataString(baseUri.MakeRelativeUri(fullUri).ToString());
            return relativeUri.Replace('/', Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// 確保給定路徑以目錄分隔符號結尾
        /// </summary>
        private string EnsureTrailingSeparator(string path)
        {
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                return path + Path.DirectorySeparatorChar;
            }
            return path;
        }
    }
}
