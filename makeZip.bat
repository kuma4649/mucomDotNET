echo mml2vgm

del /Q .\output\*.*
del /Q .\output\compiler\*.*
del /Q .\output\player\*.*
xcopy .\mucomDotNETConsole\bin\Release\*.* .\output\compiler\ /E /R /Y /I /K
xcopy .\mucomDotNETPlayer\bin\Release\*.* .\output\player\ /E /R /Y /I /K
del /Q .\output\*.pdb
del /Q .\output\*.config
del /Q .\output\compiler\*.pdb
del /Q .\output\compiler\*.config
del /Q .\output\player\*.pdb
del /Q .\output\player\*.config
del /Q .\output\bin.zip
copy .\CHANGE.txt .\output\
copy .\README.md .\output\
copy .\usage.txt .\output\

pause
