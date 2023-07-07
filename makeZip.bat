echo mml2vgm

mkdir output
mkdir output\Samples
mkdir output\NetStandard21
mkdir output\NET6
mkdir output\NET6\playerx86
mkdir output\NetCoreApp31

del /Q .\output\*.*
del /Q .\output\Samples\*.*
del /Q .\output\NetStandard21\*.*
del /Q .\output\NET6\*.*
del /Q .\output\NET6\playerx86\*.*
del /Q .\output\NetCoreApp31\*.*

xcopy .\Samples\*.*                                        .\output\Samples\        /E /R /Y /I /K
xcopy .\mucomDotNETCommon\bin\Release\netstandard2.1\*.*   .\output\NetStandard21\  /E /R /Y /I /K
xcopy .\mucomDotNETCompiler\bin\Release\netstandard2.1\*.* .\output\NetStandard21\  /E /R /Y /I /K
xcopy .\mucomDotNETDriver\bin\Release\netstandard2.1\*.*   .\output\NetStandard21\  /E /R /Y /I /K
xcopy .\Common_NET5\bin\Release\net6.0\*.*                 .\output\NET6\           /E /R /Y /I /K
xcopy .\Compiler_NET5\bin\Release\net6.0\*.*               .\output\NET6\           /E /R /Y /I /K
xcopy .\Driver_NET5\bin\Release\net6.0\*.*                 .\output\NET6\           /E /R /Y /I /K
xcopy .\Console_NET5\bin\Release\net6.0\*.*                .\output\NET6\           /E /R /Y /I /K
xcopy .\PCMTool_NET5\bin\Release\net6.0\*.*                .\output\NET6\           /E /R /Y /I /K
xcopy .\mucomDotNETPlayer\bin\Release\net6.0-windows\*.*   .\output\NET6\playerx86\ /E /R /Y /I /K
xcopy .\Player64\bin\Release\net6.0-windows\*.*            .\output\NET6\           /E /R /Y /I /K
xcopy .\Vgm_NET5\bin\Release\net6.0\*.*                    .\output\NET6\           /E /R /Y /I /K
xcopy .\Wav_NET5\bin\Release\net6.0\*.*                    .\output\NET6\           /E /R /Y /I /K
xcopy .\mucomDotNETConsole\bin\Release\netcoreapp3.1\*.*   .\output\NetCoreApp31\   /E /R /Y /I /K
xcopy .\PCMTool\bin\Release\netcoreapp3.1\*.*              .\output\NetCoreApp31\   /E /R /Y /I /K
xcopy .\Vgm\bin\Release\netcoreapp3.1\*.*                  .\output\NetCoreApp31\   /E /R /Y /I /K
xcopy .\Wav\bin\Release\netcoreapp3.1\*.*                  .\output\NetCoreApp31\   /E /R /Y /I /K


del /Q .\output\*.pdb
del /Q .\output\*.config
del /Q .\output\NetStandard21\*.pdb
del /Q .\output\NetStandard21\*.config
del /Q .\output\NET6\*.pdb
del /Q .\output\NET6\*.config
del /Q .\output\NET6\playerx86\*.pdb
del /Q .\output\NET6\playerx86\*.config
del /Q .\output\NetCoreApp31\*.pdb
del /Q .\output\NetCoreApp31\*.config
del /Q .\output\bin.zip


copy .\CHANGE.txt          .\output\
copy .\README.md           .\output\
copy .\usage.txt           .\output\
copy .\MML.txt             .\output\
copy .\ExtendFormat.txt    .\output\
copy .\FolderStructure.txt .\output\
copy .\compile.bat         .\output\
copy .\play.bat            .\output\
copy .\playOnGIMIC.bat     .\output\
copy .\playOnSCCI.bat      .\output\
copy .\playx64.bat         .\output\
copy .\playx64OnGIMIC.bat  .\output\
copy .\playx64OnSCCI.bat   .\output\
copy .\toVgm.bat           .\output\
copy .\toWav.bat           .\output\
copy .\PCMtool.bat         .\output\
copy .\test.pxt            .\output\
copy .\removeZoneIdent.bat .\output\

pause
