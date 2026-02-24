param(
  [string]$Configuration = 'Release',
  [string]$RuntimeIdentifier = 'win-x64',
  [string]$CredentialFile,
  [string]$Version,
  [string]$Publisher = 'CN=00000000-0000-0000-0000-000000000000',
  [string]$PackageName = 'sametcn99.HTWind',
  [string]$DisplayName = 'HTWind',
  [string]$PublisherDisplayName = 'sametcn99',
  [string]$Description = 'HTWind desktop widget manager',
  [string]$CertificatePath,
  [string]$CertificatePassword,
  [switch]$CreateTestCertificate
)

$ErrorActionPreference = 'Stop'

function Get-ToolPath {
  param(
    [Parameter(Mandatory = $true)]
    [string]$FileName
  )

  $command = Get-Command $FileName -ErrorAction SilentlyContinue
  if ($command) {
    return $command.Source
  }

  $sdkBin = Join-Path ${env:ProgramFiles(x86)} 'Windows Kits\10\bin'
  if (-not (Test-Path $sdkBin)) {
    throw "Windows SDK bin directory not found: $sdkBin"
  }

  $tool = Get-ChildItem $sdkBin -Recurse -Filter $FileName -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -match '\\x64\\' } |
    Sort-Object FullName -Descending |
    Select-Object -First 1

  if (-not $tool) {
    throw "$FileName not found. Install Windows 10/11 SDK."
  }

  return $tool.FullName
}

function Get-ProjectVersion {
  param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectPath
  )

  [xml]$projectXml = Get-Content -Path $ProjectPath -Raw
  $versionNode = $projectXml.Project.PropertyGroup.Version | Select-Object -First 1

  if ([string]::IsNullOrWhiteSpace($versionNode)) {
    return '0.1.0.0'
  }

  $segments = $versionNode.Split('.')
  while ($segments.Count -lt 4) {
    $segments += '0'
  }

  return ($segments[0..3] -join '.')
}

function Ensure-Dir {
  param([string]$Path)

  if (-not (Test-Path $Path)) {
    New-Item -Path $Path -ItemType Directory -Force | Out-Null
  }
}

function New-ResizedPng {
  param(
    [Parameter(Mandatory = $true)]
    [string]$InputPath,
    [Parameter(Mandatory = $true)]
    [string]$OutputPath,
    [Parameter(Mandatory = $true)]
    [int]$Width,
    [Parameter(Mandatory = $true)]
    [int]$Height
  )

  Add-Type -AssemblyName System.Drawing

  $bitmap = New-Object System.Drawing.Bitmap $InputPath
  $resized = New-Object System.Drawing.Bitmap $Width, $Height
  $graphics = [System.Drawing.Graphics]::FromImage($resized)

  try {
    $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $graphics.DrawImage($bitmap, 0, 0, $Width, $Height)
    $resized.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)
  }
  finally {
    $graphics.Dispose()
    $resized.Dispose()
    $bitmap.Dispose()
  }
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot 'HTWind\HTWind.csproj'

if ([string]::IsNullOrWhiteSpace($CredentialFile)) {
  $CredentialFile = Join-Path $PSScriptRoot 'msix-store.credentials.json'
}

if (Test-Path $CredentialFile) {
  try {
    $credentialConfig = Get-Content -Path $CredentialFile -Raw | ConvertFrom-Json

    if (-not $PSBoundParameters.ContainsKey('Publisher') -and -not [string]::IsNullOrWhiteSpace($credentialConfig.Publisher)) {
      $Publisher = [string]$credentialConfig.Publisher
    }

    if (-not $PSBoundParameters.ContainsKey('PackageName') -and -not [string]::IsNullOrWhiteSpace($credentialConfig.PackageName)) {
      $PackageName = [string]$credentialConfig.PackageName
    }

    if (-not $PSBoundParameters.ContainsKey('PublisherDisplayName') -and -not [string]::IsNullOrWhiteSpace($credentialConfig.PublisherDisplayName)) {
      $PublisherDisplayName = [string]$credentialConfig.PublisherDisplayName
    }

    if (-not $PSBoundParameters.ContainsKey('DisplayName') -and -not [string]::IsNullOrWhiteSpace($credentialConfig.DisplayName)) {
      $DisplayName = [string]$credentialConfig.DisplayName
    }

    if (-not $PSBoundParameters.ContainsKey('Description') -and -not [string]::IsNullOrWhiteSpace($credentialConfig.Description)) {
      $Description = [string]$credentialConfig.Description
    }

    if (-not $PSBoundParameters.ContainsKey('CertificatePath') -and -not [string]::IsNullOrWhiteSpace($credentialConfig.CertificatePath)) {
      $CertificatePath = [string]$credentialConfig.CertificatePath
    }

    if (-not $PSBoundParameters.ContainsKey('CertificatePassword') -and -not [string]::IsNullOrWhiteSpace($credentialConfig.CertificatePassword)) {
      $CertificatePassword = [string]$credentialConfig.CertificatePassword
    }

    Write-Host "Loaded packaging credentials: $CredentialFile"
  }
  catch {
    throw "Credential file could not be parsed: $CredentialFile"
  }
}

if (-not (Test-Path $projectPath)) {
  throw "Project not found: $projectPath"
}

if ([string]::IsNullOrWhiteSpace($Version)) {
  $Version = Get-ProjectVersion -ProjectPath $projectPath
}

if ($PackageName -match 'PUT_PACKAGE_IDENTITY_NAME_HERE' -or $Publisher -match 'PUT_PACKAGE_IDENTITY_PUBLISHER_HERE' -or $PublisherDisplayName -match 'PUT_PUBLISHER_DISPLAY_NAME_HERE') {
  throw 'Credential file contains placeholder values. Update scripts/msix-store.credentials.json with Partner Center identity values before packaging.'
}

$ridLabel = $RuntimeIdentifier -replace '[^A-Za-z0-9\-]', '-'
$outRoot = Join-Path $repoRoot 'out\msix'
$publishDir = Join-Path $outRoot 'publish'
$stageDir = Join-Path $outRoot 'stage'
$assetDir = Join-Path $stageDir 'Assets'
$distDir = Join-Path $repoRoot 'dist'

if (Test-Path $outRoot) {
  Remove-Item $outRoot -Recurse -Force
}

Ensure-Dir -Path $publishDir
Ensure-Dir -Path $assetDir
Ensure-Dir -Path $distDir

Write-Host "Publishing app..."

dotnet publish $projectPath -c $Configuration -r $RuntimeIdentifier --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true -o $publishDir

Write-Host "Preparing MSIX staging directory..."
Copy-Item -Path (Join-Path $publishDir '*') -Destination $stageDir -Recurse -Force

$sourceLogo = Join-Path $repoRoot 'assets\android-chrome-512x512.png'
if (-not (Test-Path $sourceLogo)) {
  throw "Source logo not found: $sourceLogo"
}

New-ResizedPng -InputPath $sourceLogo -OutputPath (Join-Path $assetDir 'Square150x150Logo.png') -Width 150 -Height 150
New-ResizedPng -InputPath $sourceLogo -OutputPath (Join-Path $assetDir 'Square44x44Logo.png') -Width 44 -Height 44
New-ResizedPng -InputPath $sourceLogo -OutputPath (Join-Path $assetDir 'Wide310x150Logo.png') -Width 310 -Height 150

$manifestPath = Join-Path $stageDir 'AppxManifest.xml'
$manifest = @"
<?xml version="1.0" encoding="utf-8"?>
<Package
  xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10"
  xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10"
  xmlns:desktop="http://schemas.microsoft.com/appx/manifest/desktop/windows10"
  xmlns:rescap="http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities"
  IgnorableNamespaces="uap desktop rescap">
  <Identity Name="$PackageName" Publisher="$Publisher" Version="$Version" />
  <Properties>
    <DisplayName>$DisplayName</DisplayName>
    <PublisherDisplayName>$PublisherDisplayName</PublisherDisplayName>
    <Description>$Description</Description>
    <Logo>Assets\Square150x150Logo.png</Logo>
  </Properties>
  <Dependencies>
    <TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.19041.0" MaxVersionTested="10.0.26100.0" />
  </Dependencies>
  <Resources>
    <Resource Language="en-us" />
  </Resources>
  <Applications>
    <Application Id="HTWind" Executable="HTWind.exe" EntryPoint="Windows.FullTrustApplication">
      <uap:VisualElements
        DisplayName="$DisplayName"
        Description="$Description"
        Square150x150Logo="Assets\Square150x150Logo.png"
        Square44x44Logo="Assets\Square44x44Logo.png"
        BackgroundColor="transparent">
        <uap:DefaultTile Wide310x150Logo="Assets\Wide310x150Logo.png" />
      </uap:VisualElements>
      <Extensions>
        <desktop:Extension Category="windows.fullTrustProcess" Executable="HTWind.exe" />
      </Extensions>
    </Application>
  </Applications>
  <Capabilities>
    <rescap:Capability Name="runFullTrust" />
  </Capabilities>
</Package>
"@

Set-Content -Path $manifestPath -Value $manifest -Encoding UTF8

$makeAppxPath = Get-ToolPath -FileName 'makeappx.exe'
$signtoolPath = Get-ToolPath -FileName 'signtool.exe'

$packageBaseName = "HTWind-store-$Version-$ridLabel"
$msixPath = Join-Path $distDir "$packageBaseName.msix"
$msixUploadPath = Join-Path $distDir "$packageBaseName.msixupload"

if (Test-Path $msixPath) {
  Remove-Item $msixPath -Force
}

if (Test-Path $msixUploadPath) {
  Remove-Item $msixUploadPath -Force
}

Write-Host "Packing MSIX..."
& $makeAppxPath pack /d $stageDir /p $msixPath /o
if ($LASTEXITCODE -ne 0 -or -not (Test-Path $msixPath)) {
  throw 'MSIX packing failed. Check PackageName/Publisher values and AppxManifest validation details above.'
}

if (-not [string]::IsNullOrWhiteSpace($CertificatePath) -and -not (Test-Path $CertificatePath)) {
  throw "Certificate file not found: $CertificatePath"
}

if ($CreateTestCertificate -and [string]::IsNullOrWhiteSpace($CertificatePath)) {
  if ([string]::IsNullOrWhiteSpace($CertificatePassword)) {
    $CertificatePassword = "HTWind-Local-Test-123!"
  }

  $cert = New-SelfSignedCertificate -Type Custom -Subject $Publisher -KeyUsage DigitalSignature -FriendlyName 'HTWind MSIX Test Certificate' -CertStoreLocation 'Cert:\CurrentUser\My' -TextExtension @('2.5.29.37={text}1.3.6.1.5.5.7.3.3')

  $certificateOutPath = Join-Path $outRoot 'HTWind-Local-Test-Certificate.pfx'
  $securePwd = ConvertTo-SecureString -String $CertificatePassword -AsPlainText -Force
  Export-PfxCertificate -Cert $cert -FilePath $certificateOutPath -Password $securePwd | Out-Null
  $CertificatePath = $certificateOutPath

  Write-Host "Created test certificate: $CertificatePath"
}

if (-not [string]::IsNullOrWhiteSpace($CertificatePath)) {
  Write-Host "Signing MSIX..."

  $signArgs = @('sign', '/fd', 'SHA256', '/a', '/f', $CertificatePath)
  if (-not [string]::IsNullOrWhiteSpace($CertificatePassword)) {
    $signArgs += @('/p', $CertificatePassword)
  }
  $signArgs += $msixPath

  & $signtoolPath @signArgs
  if ($LASTEXITCODE -ne 0) {
    throw 'MSIX signing failed. Verify certificate path, password, and publisher match.'
  }
}
else {
  Write-Warning 'MSIX was not signed. Provide -CertificatePath or use -CreateTestCertificate for local install tests.'
}

Copy-Item -Path $msixPath -Destination $msixUploadPath -Force

Write-Host "MSIX package created: $msixPath"
Write-Host "MSIX upload package created: $msixUploadPath"
Write-Host "Publisher: $Publisher"
Write-Host "Package Name: $PackageName"
Write-Host "Credential file: $CredentialFile"
Write-Host 'Note: For Microsoft Store submission, Publisher and PackageName must match Partner Center reserved values.'
