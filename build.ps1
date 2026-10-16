param([switch]$Test)
$ErrorActionPreference='Stop'
& "$PSScriptRoot/scripts/restore.ps1"
& "$PSScriptRoot/app/src/build.ps1" -Test:$Test
