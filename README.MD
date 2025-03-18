# DllChecker

DllChecker is a Windows Forms application written in C# that scans a selected folder (including its subdirectories) for DLL and EXE files. The tool analyzes each file to determine its architecture (AnyCPU, 32-bit only, or 64-bit only) and displays the results in a color-coded interface with a summary of statistics. It also saves your last selected folder for convenience in future sessions.

## Features

- ✅ **Folder Selection:**  
  Easily select a folder to scan. The application remembers your last folder selection using persistent settings.

- ✅ **File Scanning:**  
  Recursively scans for both DLL and EXE files in the selected directory.

- ✅ **Architecture Detection:**  
  Analyzes each file to determine:
  
  - **AnyCPU:** Managed assemblies that can run on both 32-bit and 64-bit systems.
  - **32-bit Only:** Files that run exclusively on 32-bit systems.
  - **64-bit Only:** Files that run exclusively on 64-bit systems.
  - **Native PE Files:** Reads the PE header for non-managed files to determine their architecture.

- ✅ **Color-Coded Output:**  
  Displays detailed scanning results in a RichTextBox:
  
  - DLL file results appear in green.
  - EXE file results use default text color, with red highlights for architecture restrictions.

- ✅ **Summary Statistics:**  
  Provides a count summary of:
  
  - AnyCPU files.
  - 32-bit only files.
  - 64-bit only files.
  - Other/Unknown architecture files.

## Usage

1. **Select Folder:**  
   Click the "Select Folder" button to choose the directory containing your DLL and EXE files. The selected folder path is displayed on the form and saved for future sessions.

2. **Scan Files:**  
   Click the "Scan Files" button to begin scanning. The application will:
   
   - Clear previous results.
   - Scan and display DLL files with their relative paths and architecture details.
   - Scan and display EXE files, highlighting any architecture restrictions in red.
   - Show a summary of the results at the end.

3. **Review Results:**  
   Examine the console-style output in the RichTextBox to view detailed information and summary statistics about the scanned files.

## Configuration Files

- **User Settings:**  
  The last selected folder is saved in `Properties.Settings.Default.LastFolder` to streamline future use.

## Troubleshooting

- **No Files Found:**  
  Ensure that the selected folder contains DLL or EXE files.
- **Invalid File Format:**  
  Files that are not valid managed assemblies or native PE files will be marked accordingly.
- **Permission Issues:**  
  Verify that you have the necessary permissions to access files in the selected folder.

## License

This project is licensed under the MIT License.

## Contact & Support

If you have any issues or feature requests, please open an issue on GitHub.
