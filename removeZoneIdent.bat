@echo off
echo zip,dll,exeファイルのZone識別子を削除します。
pause

echo on
FOR %%a in (*.zip *.dll *.exe) do (echo . > %%a:Zone.Identifier)
FOR %%a in (compiler\*.zip compiler\*.dll compiler\*.exe) do (echo . > %%a:Zone.Identifier)
FOR %%a in (PCMTool\*.zip PCMTool\*.dll PCMTool\*.exe) do (echo . > %%a:Zone.Identifier)
FOR %%a in (player\*.zip player\*.dll player\*.exe) do (echo . > %%a:Zone.Identifier)
FOR %%a in (toVgm\*.zip toVgm\*.dll toVgm\*.exe) do (echo . > %%a:Zone.Identifier)
FOR %%a in (toWav\*.zip toWav\*.dll toWav\*.exe) do (echo . > %%a:Zone.Identifier)

FOR %%a in (NET5\compiler\*.zip NET5\compiler\*.dll NET5\compiler\*.exe) do (echo . > %%a:Zone.Identifier)
FOR %%a in (NET5\PCMTool\*.zip NET5\PCMTool\*.dll NET5\PCMTool\*.exe) do (echo . > %%a:Zone.Identifier)
FOR %%a in (NET5\toVgm\*.zip NET5\toVgm\*.dll NET5\toVgm\*.exe) do (echo . > %%a:Zone.Identifier)
FOR %%a in (NET5\toWav\*.zip NET5\toWav\*.dll NET5\toWav\*.exe) do (echo . > %%a:Zone.Identifier)

FOR %%a in (playerx64\*.zip playerx64\*.dll playerx64\*.exe) do (echo . > %%a:Zone.Identifier)
@echo off

echo 完了しました。
pause
echo on
