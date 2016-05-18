param([String]$ToolsPath)

if (Test-Path "$ToolsPath") { remove-item -Recurse -Force "$ToolsPath" }
mkdir .dotnetcli | Out-Null

wget https://raw.githubusercontent.com/dotnet/cli/rel/1.0.0/scripts/obtain/install.ps1 -OutFile "$ToolsPath\install.ps1"

.\.dotnetcli\install.ps1 -Channel preview -version Latest -InstallDir "$ToolsPath" -NoPath
