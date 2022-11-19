
param ([bool]$isProd = $false)
$projectPath = "C:\code\ICan\ICan\ICan.Jobs.Runner";
$binPath = "$projectPath\bin";
$releasePath = "$binPath\Release\net5.0\win-x86\publish";

$settingsPath = "C:\backup\yamogu\opt\jobs\";

if ($isProd -eq $true) {
    $settingsPath = "C:\backup\yamogu\prod\jobs\";
}


Set-Location -path $projectPath
if (Test-Path $binPath) {
    Remove-Item  -path $binPath  -recurse
}
dotnet publish -c Release  -r win-x86
 
Set-Location -path $settingsPath
Copy-Item  -Path ./appsettings* -Destination $releasePath -Recurse -force
Copy-Item  -Path ./root.crt -Destination $releasePath -Recurse -force
Copy-Item  -Path ./nlog* -Destination $releasePath -Recurse -force
Copy-Item  -Path ./web.config -Destination $releasePath -Recurse -force


$7zipPath = "$env:ProgramFiles\7-Zip\7z.exe"
Set-Alias 7zip $7zipPath

Set-Location -path $releasePath

7zip a "publish.zip" -tzip '*'

start $releasePath




