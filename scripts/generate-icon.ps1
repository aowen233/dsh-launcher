param([switch]$Verify)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$root = Split-Path -Parent $PSScriptRoot
$source = Join-Path $root 'assets\dsh-launcher-source.png'
$output = Join-Path $root 'assets\dsh-launcher.ico'
$sourceSha256 = '17a89bc3dd0f09e57bd3803e1698591f38e20674f3c324aa50dd29580d77b8eb'
$committedSha256 = '851c307655f7811bf03c77e10841032485cd07cb10d7ff7c53b6a559858e6e71'

if (-not (Test-Path -LiteralPath $source)) {
    throw 'Reviewed icon source image is missing.'
}

$actualSource = (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash.ToLowerInvariant()
if ($sourceSha256 -ne $actualSource) {
    throw 'Icon source image hash does not match the reviewed asset.'
}

if ($Verify) {
    if (-not (Test-Path -LiteralPath $output)) {
        throw 'Committed icon is missing. Run scripts/generate-icon.ps1.'
    }
    $actual = (Get-FileHash -LiteralPath $output -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($committedSha256 -ne $actual) {
        throw 'Committed icon hash does not match the reviewed icon.'
    }
    Write-Host 'Committed icon source and multi-resolution ICO hashes are valid.'
    exit 0
}

$sizes = @(16, 20, 24, 32, 40, 48, 64, 128, 256)
$sourceImage = [Drawing.Image]::FromFile($source)
$entries = [Collections.Generic.List[byte[]]]::new()

try {
    foreach ($size in $sizes) {
        $bitmap = [Drawing.Bitmap]::new($size, $size, [Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $graphics = [Drawing.Graphics]::FromImage($bitmap)
        $stream = [IO.MemoryStream]::new()
        try {
            $graphics.CompositingMode = [Drawing.Drawing2D.CompositingMode]::SourceCopy
            $graphics.CompositingQuality = [Drawing.Drawing2D.CompositingQuality]::HighQuality
            $graphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $graphics.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::HighQuality
            $graphics.PixelOffsetMode = [Drawing.Drawing2D.PixelOffsetMode]::HighQuality
            $graphics.DrawImage($sourceImage, [Drawing.Rectangle]::new(0, 0, $size, $size))
            $bitmap.Save($stream, [Drawing.Imaging.ImageFormat]::Png)
            $entries.Add($stream.ToArray())
        }
        finally {
            $stream.Dispose()
            $graphics.Dispose()
            $bitmap.Dispose()
        }
    }
}
finally {
    $sourceImage.Dispose()
}

$temporary = Join-Path ([IO.Path]::GetTempPath()) "dsh-launcher-icon-$PID.ico"
$file = [IO.File]::Open($temporary, [IO.FileMode]::Create, [IO.FileAccess]::Write, [IO.FileShare]::None)
$writer = [IO.BinaryWriter]::new($file)
try {
    $writer.Write([uint16]0)
    $writer.Write([uint16]1)
    $writer.Write([uint16]$entries.Count)

    $offset = 6 + (16 * $entries.Count)
    for ($index = 0; $index -lt $entries.Count; $index++) {
        $size = $sizes[$index]
        $dimension = if ($size -eq 256) { 0 } else { $size }
        $writer.Write([byte]$dimension)
        $writer.Write([byte]$dimension)
        $writer.Write([byte]0)
        $writer.Write([byte]0)
        $writer.Write([uint16]1)
        $writer.Write([uint16]32)
        $writer.Write([uint32]$entries[$index].Length)
        $writer.Write([uint32]$offset)
        $offset += $entries[$index].Length
    }

    foreach ($entry in $entries) {
        $writer.Write($entry)
    }
}
finally {
    $writer.Dispose()
}

Move-Item -LiteralPath $temporary -Destination $output -Force
Write-Host "Wrote multi-resolution icon to $output"
