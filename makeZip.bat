echo mml2vgm

mkdir output
mkdir output\compiler
mkdir output\player
mkdir output\toWav
mkdir output\toVgm
mkdir output\PCMTool

del /Q .\output\*.*
del /Q .\output\compiler\*.*
del /Q .\output\player\*.*
del /Q .\output\toWav\*.*
del /Q .\output\toVgm\*.*
xcopy .\mucomDotNETConsole.Net4\bin\Release\net472\*.* .\output\compiler\ /E /R /Y /I /K
xcopy .\mucomDotNETPlayer\bin\Release\net472\*.* .\output\player\ /E /R /Y /I /K
xcopy .\Wav.Net4\bin\Release\net472\*.* .\output\toWav\ /E /R /Y /I /K
xcopy .\Vgm.Net4\bin\Release\net472\*.* .\output\toVgm\ /E /R /Y /I /K
xcopy .\PCMTool\bin\Release\netcoreapp3.1\*.* .\output\PCMTool\ /E /R /Y /I /K
del /Q .\output\*.pdb
del /Q .\output\*.config
del /Q .\output\compiler\*.pdb
del /Q .\output\compiler\*.config
del /Q .\output\player\*.pdb
del /Q .\output\player\*.config
del /Q .\output\toWav\*.pdb
del /Q .\output\toWav\*.config
del /Q .\output\toVgm\*.pdb
del /Q .\output\toVgm\*.config
del /Q .\output\bin.zip
copy .\CHANGE.txt .\output\
copy .\README.md .\output\
copy .\usage.txt .\output\
copy .\MML.txt .\output\
copy .\ExtendFormat.txt .\output\
copy .\compile.bat .\output\
copy .\play.bat .\output\
copy .\playOnGIMIC.bat .\output\
copy .\playOnSCCI.bat .\output\
copy .\toWav.bat .\output\
copy .\PCMtool.bat .\output\
copy .\test.pxt .\output\
copy .\removeZoneIdent.bat .\output\

pause
