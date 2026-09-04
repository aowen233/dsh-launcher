$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$trackedCandidates = Get-ChildItem -LiteralPath $root -Recurse -File -Force |
    Where-Object {
        $_.FullName -notmatch '[\\/](\.git|bin|obj|artifacts|runtime|\.tools)[\\/]'
    }

$forbiddenExtensions = @('.pfx', '.p12', '.key', '.exe', '.msi')
$forbidden = $trackedCandidates | Where-Object { $_.Extension -in $forbiddenExtensions }
if ($forbidden) {
    throw "Forbidden generated or secret-bearing files found: $($forbidden.FullName -join ', ')"
}

$requiredBilingualDocuments = @(
    'README',
    'ASSET_LICENSE',
    'CHANGELOG',
    'CODE_OF_CONDUCT',
    'CODE_SIGNING_POLICY',
    'CONTRIBUTING',
    'PRIVACY',
    'SECURITY',
    'SUPPORT',
    'THIRD_PARTY_NOTICES',
    'docs\ARCHITECTURE',
    'docs\COMPATIBILITY',
    'docs\RELEASING',
    'docs\REPOSITORY-SETUP',
    'docs\THREAT-MODEL'
)
foreach ($document in $requiredBilingualDocuments) {
    $english = Join-Path $root "$document.md"
    $chinese = Join-Path $root "$document.zh-CN.md"
    if (-not (Test-Path -LiteralPath $english) -or -not (Test-Path -LiteralPath $chinese)) {
        throw "Required bilingual documentation pair is incomplete: $document"
    }
}

$manifestPath = Join-Path $root 'src\DshLauncher\Resources\dependencies.json'
$manifest = Get-Content -Raw -LiteralPath $manifestPath | ConvertFrom-Json
foreach ($dependency in @($manifest.node.x64, $manifest.node.arm64)) {
    if ($dependency.url -notmatch '^https://nodejs\.org/') {
        throw "Node.js URL is not on the allow-listed official host: $($dependency.url)"
    }
    if ($dependency.sha256 -notmatch '^[0-9a-f]{64}$') {
        throw "Invalid SHA-256 value for $($dependency.url)"
    }
}

$appProjectPath = Join-Path $root 'src\DshLauncher\DshLauncher.csproj'
[xml]$appProject = Get-Content -Raw -LiteralPath $appProjectPath
$packageReferences = @($appProject.Project.ItemGroup.PackageReference)
if ($packageReferences.Count -ne 1 -or
    $packageReferences[0].Include -ne 'System.Management' -or
    $packageReferences[0].Version -ne '10.0.11') {
    throw 'The direct NuGet dependency allow-list or exact version pin has changed.'
}

if ($manifest.whaleWidget.author -ne 'MeteorNOX' -or
    $manifest.whaleWidget.repository -ne 'https://github.com/MeteorNOX/DeepSeek-Balance-Whale-Widget' -or
    $manifest.whaleWidget.commit -notmatch '^[0-9a-f]{40}$') {
    throw 'Whale Widget attribution or pin is invalid.'
}

$translation = Join-Path $root 'installer\translations\ChineseSimplified.isl'
$translationLicense = Join-Path $root 'installer\translations\LICENSE-kira-96.txt'
$translationHash = 'bf0751fa176569c6faa2f6e17ed2734617bef325d5cc06eae030fdd0258ee778'
if (-not (Test-Path -LiteralPath $translation) -or
    -not (Test-Path -LiteralPath $translationLicense) -or
    (Get-FileHash -LiteralPath $translation -Algorithm SHA256).Hash.ToLowerInvariant() -ne $translationHash) {
    throw 'Pinned Simplified Chinese installer translation or its license is invalid.'
}

$secretPattern = '(?i)(-----BEGIN (RSA |EC |OPENSSH )?PRIVATE KEY-----|ghp_[A-Za-z0-9]{20,}|github_pat_[A-Za-z0-9_]{20,}|sk-[A-Za-z0-9]{20,})'
$secretHits = $trackedCandidates |
    Where-Object { $_.Extension -in @('.cs', '.json', '.yml', '.yaml', '.md', '.ps1', '.iss') } |
    Select-String -Pattern $secretPattern
if ($secretHits) {
    throw "Potential secret found: $($secretHits.Path -join ', ')"
}

Write-Host 'Repository validation passed.'
