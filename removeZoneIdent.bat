@echo off
echo zip,dll,exe�t�@�C����Zone���ʎq���폜���܂��B
pause

echo on
FOR %%a in (*.zip *.dll *.exe) do (echo . > %%a:Zone.Identifier)
FOR %%a in (NET6\*.zip NET6\*.dll NET6\*.exe) do (echo . > %%a:Zone.Identifier)
FOR %%a in (NET6\playerx86\*.zip NET6\playerx86\*.dll NET6\playerx86\*.exe) do (echo . > %%a:Zone.Identifier)
FOR %%a in (NetCoreApp31\*.zip NetCoreApp31\*.dll NetCoreApp31\*.exe) do (echo . > %%a:Zone.Identifier)
FOR %%a in (NetStandard21\*.zip NetStandard21\*.dll NetStandard21\*.exe) do (echo . > %%a:Zone.Identifier)

@echo off

echo �������܂����B
pause
echo on
