echo mml2vgm

del /Q .\output\*.*
del /Q .\output\compiler\*.*
del /Q .\output\player\*.*
del /Q .\output\toWav\*.*
xcopy .\mucomDotNETConsole.Net4\bin\Release\net472\*.* .\output\compiler\ /E /R /Y /I /K
xcopy .\mucomDotNETPlayer\bin\Release\net472\*.* .\output\player\ /E /R /Y /I /K
xcopy .\Wav.Net4\bin\Release\net472\*.* .\output\toWav\ /E /R /Y /I /K
del /Q .\output\*.pdb
del /Q .\output\*.config
del /Q .\output\compiler\*.pdb
del /Q .\output\compiler\*.config
del /Q .\output\player\*.pdb
del /Q .\output\player\*.config
del /Q .\output\toWav\*.pdb
del /Q .\output\toWav\*.config
del /Q .\output\bin.zip
copy .\CHANGE.txt .\output\
copy .\README.md .\output\
copy .\usage.txt .\output\
copy .\MML.txt .\output\
copy .\compile.bat .\output\
copy .\play.bat .\output\
copy .\playOnGIMIC.bat .\output\
copy .\playOnSCCI.bat .\output\
copy .\toWav.bat .\output\

pause
