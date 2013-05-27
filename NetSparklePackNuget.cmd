@echo off

echo Cleanup NuGet environment
rmdir /S /Q Nuget\core\lib
rmdir /S /Q Nuget\core\content
rmdir /S /Q Nuget\core\tools
del \q Nuget\core\*.nupkg
rmdir /S /Q Nuget\tools\lib
rmdir /S /Q Nuget\tools\content
rmdir /S /Q Nuget\tools\tools
del \q Nuget\tools\*.nupkg

echo Create NuGet tree for core
mkdir Nuget\core\lib
mkdir Nuget\core\content
mkdir Nuget\core\tools

echo Create NuGet tree for tools
mkdir Nuget\tools\lib
mkdir Nuget\tools\content
mkdir Nuget\tools\content\Extras
mkdir Nuget\tools\tools

echo Copy Core Buildoutput to Nuget dir
xcopy /s /q NetSparkle\Release\lib\net40-full\* Nuget\core\lib\net40-full\
del /q Nuget\core\lib\net40-full\*.pdb

echo Copy Tools Buildoutput to Nuget dir
xcopy /s /q /y NetSparkleChecker\bin\Release\* Nuget\tools\tools\
xcopy /s /q /y NetSparkleDSAHelper\bin\Release Nuget\tools\tools\
xcopy /s /q /y Extras\* Nuget\tools\content\Extras\
del /q Nuget\tools\tools\*.config
del /q Nuget\tools\tools\*.pdb
del /q Nuget\tools\tools\*.xml
del /q Nuget\tools\tools\*.manifest
del /q Nuget\tools\tools\*.vshost.*

echo Moving to release directory
cd Nuget

echo Packing core nuget package
cd core
..\nuget pack NetSparkle.nuspec -Version %1
cd ..

echo Packing tools nuget package
cd tools
..\nuget pack NetSparkle.Tools.nuspec -Version %1
cd ..

echo Pushing nuget package 
rem nuget Push core\NetSparkle.%1.nupkg
rem nuget Push tools\NetSparkle.Tools.%1.nupkg

echo Leaving directories
cd ..