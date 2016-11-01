net use z: \\spdevnas01.sp.local\Development\Shared\ReadiMon

xcopy bin\debug\Pipeline z:\Pipeline /S /Y
xcopy bin\debug\ChangeLog.txt z:\ /Y
xcopy bin\debug\ICSharpCode.AvalonEdit.dll z:\ /Y
xcopy bin\debug\CommandLine.dll z:\ /Y
xcopy bin\debug\ReadiMon.exe z:\ /Y
xcopy bin\debug\ReadiMon.exe.config z:\ /Y
xcopy bin\debug\ReadiMon.HostView.dll z:\ /Y
xcopy bin\debug\ReadiMon.Shared.dll z:\ /Y
xcopy bin\debug\System.Windows.Controls.Input.Toolkit.dll z:\ /Y
xcopy bin\debug\System.Windows.Controls.Layout.Toolkit.dll z:\ /Y
xcopy bin\debug\System.Windows.Interactivity.dll z:\ /Y
xcopy bin\debug\WPFToolkit.dll z:\ /Y

net use z: /delete /yes