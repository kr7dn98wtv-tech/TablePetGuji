param()
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$deps=Join-Path $root 'build-deps/net46'
$refs=Join-Path $deps 'package/build/.NETFramework/v4.6'
if(Test-Path (Join-Path $refs 'mscorlib.dll')){return}
New-Item -ItemType Directory -Path $deps -Force | Out-Null
$env:TEMP=Join-Path $root 'app/work';$env:TMP=$env:TEMP
New-Item -ItemType Directory -Path $env:TEMP -Force | Out-Null
[Net.ServicePointManager]::SecurityProtocol=[Net.SecurityProtocolType]::Tls12
$archive=Join-Path $deps 'reference-assemblies.nupkg'
Invoke-WebRequest -UseBasicParsing -Uri 'https://api.nuget.org/v3-flatcontainer/microsoft.netframework.referenceassemblies.net46/1.0.3/microsoft.netframework.referenceassemblies.net46.1.0.3.nupkg' -OutFile $archive
$expected='DCBB79BB3868DBFBB64C643116FAA63D888EE9ECCBA0C4E965E9992ED7C4E35D'
if((Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash -ne $expected){throw 'Reference package checksum mismatch'}
Add-Type -AssemblyName System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::ExtractToDirectory($archive,(Join-Path $deps 'package'))
if(-not (Test-Path (Join-Path $refs 'mscorlib.dll'))){throw 'Reference assemblies were not restored'}
