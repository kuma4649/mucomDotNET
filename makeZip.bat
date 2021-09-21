echo mml2vgm

mkdir output
mkdir output\compiler
mkdir output\player
mkdir output\toWav
mkdir output\toVgm
mkdir output\PCMTool
mkdir output\Samples
mkdir output\NET5
mkdir output\NET5\compiler
mkdir output\NET5\toWav
mkdir output\NET5\toVgm
mkdir output\NET5\PCMTool

del /Q .\output\*.*
del /Q .\output\compiler\*.*
del /Q .\output\player\*.*
del /Q .\output\toWav\*.*
del /Q .\output\toVgm\*.*
del /Q .\output\Samples\*.*
del /Q .\output\NET5\*.*
del /Q .\output\NET5\compiler\*.*
del /Q .\output\NET5\toWav\*.*
del /Q .\output\NET5\toVgm\*.*

xcopy .\mucomDotNETConsole.Net4\bin\Release\net472\*.* .\output\compiler\ /E /R /Y /I /K
xcopy .\mucomDotNETPlayer\bin\Release\net472\*.* .\output\player\ /E /R /Y /I /K
xcopy .\Wav.Net4\bin\Release\net472\*.* .\output\toWav\ /E /R /Y /I /K
xcopy .\Vgm.Net4\bin\Release\net472\*.* .\output\toVgm\ /E /R /Y /I /K
xcopy .\PCMTool.Net4\bin\Release\net472\*.* .\output\PCMTool\ /E /R /Y /I /K
xcopy .\Samples\*.* .\output\Samples\ /E /R /Y /I /K

xcopy .\Compiler_NET5\bin\Release\net5.0\*.* .\output\NET5\compiler\ /E /R /Y /I /K
xcopy .\Wav_NET5\bin\Release\net5.0\*.* .\output\NET5\toWav\ /E /R /Y /I /K
xcopy .\Vgm_NET5\bin\Release\net5.0\*.* .\output\NET5\toVgm\ /E /R /Y /I /K
xcopy .\PCMTool_NET5\bin\Release\net5.0\*.* .\output\NET5\PCMTool\ /E /R /Y /I /K

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

del /Q .\output\NET5\compiler\*.pdb
del /Q .\output\NET5\compiler\*.config
del /Q .\output\NET5\toWav\*.pdb
del /Q .\output\NET5\toWav\*.config
del /Q .\output\NET5\toVgm\*.pdb
del /Q .\output\NET5\toVgm\*.config

copy .\CHANGE.txt .\output\
copy .\README.md .\output\
copy .\usage.txt .\output\
copy .\MML.txt .\output\
copy .\ExtendFormat.txt .\output\
copy .\compile.bat .\output\
copy .\play.bat .\output\
copy .\playOnGIMIC.bat .\output\
copy .\playOnSCCI.bat .\output\
copy .\toVgm.bat .\output\
copy .\toWav.bat .\output\
copy .\PCMtool.bat .\output\
copy .\test.pxt .\output\
copy .\removeZoneIdent.bat .\output\

pause
