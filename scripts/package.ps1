param([switch]$Test)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
& "$root/build.ps1" -Test:$Test
$version=[Diagnostics.FileVersionInfo]::GetVersionInfo("$root/app/Guji.exe").FileVersion
$stage=Join-Path $root "dist/Guji-$version"
if(Test-Path $stage){throw "Output already exists: $stage"}
New-Item -ItemType Directory -Path (Join-Path $stage 'assets') -Force | Out-Null
foreach($name in @('Guji.exe','Guji.exe.config','38道美食.txt')){Copy-Item -LiteralPath (Join-Path "$root/app" $name) -Destination $stage}
Copy-Item -LiteralPath "$root/docs/使用说明.txt" -Destination $stage
Get-ChildItem "$root/app/assets" -Filter '*.png' -File | Copy-Item -Destination "$stage/assets"
$zip=Join-Path "$root/dist" "Guji-$version-Windows.zip"
Compress-Archive -Path "$stage/Guji.exe","$stage/Guji.exe.config","$stage/38道美食.txt","$stage/使用说明.txt","$stage/assets" -DestinationPath $zip
Write-Output $zip
