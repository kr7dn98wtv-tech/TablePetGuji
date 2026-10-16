param([switch]$Test)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$env:TEMP=Join-Path $root 'work';$env:TMP=$env:TEMP
$f=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319'
if(-not (Test-Path (Join-Path $f 'csc.exe'))){$f=Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319'}
$refs=Join-Path (Split-Path -Parent $root) 'build-deps\net46\package\build\.NETFramework\v4.6'
if(-not (Test-Path (Join-Path $refs 'mscorlib.dll'))){throw 'Missing E-drive .NET 4.6 reference assemblies. Run build.ps1 from the repository root to restore dependencies.'}
$arguments=@('/noconfig','/nologo','/target:winexe','/platform:anycpu','/optimize+','/nowarn:0618','/nostdlib+',"/win32manifest:$PSScriptRoot\app.manifest","/out:$root\Guji.exe")
foreach($assembly in @('mscorlib','System','System.Core','System.Xml','PresentationFramework','PresentationCore','WindowsBase','System.Xaml','System.Windows.Forms','System.Drawing')){$arguments+="/reference:$refs\$assembly.dll"}
foreach($source in @('AssemblyInfo','Model','Art','PetWindow','Shell','Program')){$arguments+="$PSScriptRoot\$source.cs"}
& "$f\csc.exe" @arguments
if($LASTEXITCODE -ne 0){throw 'Build failed'}
if($Test){$p=Start-Process "$root\Guji.exe" -ArgumentList '--test' -WindowStyle Hidden -Wait -PassThru; Get-Content "$root\work\test-report.txt";if($p.ExitCode -ne 0){throw "Test failed: $($p.ExitCode)"}}
