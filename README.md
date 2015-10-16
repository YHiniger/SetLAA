# SetLAA
Sets (or clears) the Large Address-Aware flag of an application, given its file name.

## Requirements
.NET Framework 4.5

## Usage
SetLAA[.exe] appFileName.exe [state]
- appFileName.exe: the name of the target application (*.exe) file.
- state: optional, 0 to clear the LAA flag, 1 to set it; defaults to 1.
[] indicates an optional argument (or part of an argument

Or simply drag and drop the target application file on SetLAA.exe in the file explorer !

### Notes
- Make a backup of the original application before patching it. SetLAA won't do it for you.
- Beware that the target application is not running. This would simply cause SetLAA to fail.