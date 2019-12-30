echo mml2vgm

del /Q .\output\*.*
xcopy .\mucomDotNETConsole\bin\Release\*.* .\output\ /E /R /Y /I /K
del /Q .\output\*.pdb
del /Q .\output\*.config
del /Q .\output\bin.zip

pause
