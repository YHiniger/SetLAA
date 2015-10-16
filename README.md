# SetLAA
Sets (or clears) the Large Address-Aware flag of an application, given its file name.

## Requirements
.NET Framework 4.5

### Usage
SetLAA[.exe] appFileName[.exe] [state]
- appFileName: the name of the target application file.
- state: optional, 0 to clear the LAA flag, 1 to set it; defaults to 1.