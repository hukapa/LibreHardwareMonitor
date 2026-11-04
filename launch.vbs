Set fso = CreateObject("Scripting.FileSystemObject")
strDir = fso.GetParentFolderName(WScript.ScriptFullName)
Set shell = CreateObject("Shell.Application")
' Executa o launcher elevado oculto (0 = oculto, sem janela preta)
shell.ShellExecute strDir & "\Iniciar-Elevado.cmd", "", strDir, "runas", 0
