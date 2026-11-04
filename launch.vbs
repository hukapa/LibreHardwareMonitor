Set fso = CreateObject("Scripting.FileSystemObject")
strDir = fso.GetParentFolderName(WScript.ScriptFullName)

exePublish = strDir & "\bin\Publish\LibreHardwareMonitor.Windows.Forms.exe"
exeSingle = strDir & "\bin\Publish-SingleFile\LibreHardwareMonitor.Windows.Forms.exe"
exeRel = strDir & "\bin\Release\net8.0-windows\LibreHardwareMonitor.Windows.Forms.exe"

If fso.FileExists(exePublish) Then
    exePath = exePublish
    workDir = strDir & "\bin\Publish"
ElseIf fso.FileExists(exeSingle) Then
    exePath = exeSingle
    workDir = strDir & "\bin\Publish-SingleFile"
Else
    exePath = exeRel
    workDir = strDir & "\bin\Release\net8.0-windows"
End If

Set shell = CreateObject("Shell.Application")
shell.ShellExecute exePath, "", workDir, "runas", 1
