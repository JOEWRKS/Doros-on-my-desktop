param(
    [string]$Source = (Join-Path $PSScriptRoot '..\src\Dororong.App\Assets\dororong-head-source.png'),
    [string]$Output = (Join-Path $PSScriptRoot '..\src\Dororong.App\Assets\dororong.ico'),
    [string]$Preview = (Join-Path $PSScriptRoot '..\src\Dororong.App\Assets\dororong-head.png')
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
# Format conversion of the existing separated head, not newly painted artwork.
# Fit nontransparent bounds; preserve aspect ratio and transparent padding.
$sourceBitmap = [Drawing.Bitmap]::new((Resolve-Path -LiteralPath $Source).Path)
$frames = [Collections.Generic.List[object]]::new()
try {
    $left=$sourceBitmap.Width; $top=$sourceBitmap.Height; $right=-1; $bottom=-1
    for($y=0;$y-lt $sourceBitmap.Height;$y++) {
        for($x=0;$x-lt $sourceBitmap.Width;$x++) {
            if($sourceBitmap.GetPixel($x,$y).A-eq 0){continue}
            $left=[Math]::Min($left,$x);$top=[Math]::Min($top,$y)
            $right=[Math]::Max($right,$x);$bottom=[Math]::Max($bottom,$y)
        }
    }
    if($right-lt $left){throw 'Icon source has no visible pixels.'}
    $width=$right-$left+1;$height=$bottom-$top+1
    foreach($size in @(16,20,24,32,40,48,64,96,128,256)) {
        $bitmap=[Drawing.Bitmap]::new($size,$size,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $graphics=[Drawing.Graphics]::FromImage($bitmap)
        $attributes=[Drawing.Imaging.ImageAttributes]::new()
        $stream=[IO.MemoryStream]::new()
        try {
            $graphics.Clear([Drawing.Color]::Transparent)
            $graphics.CompositingMode=[Drawing.Drawing2D.CompositingMode]::SourceCopy
            $graphics.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::Half
            $padding=[Math]::Max(1,[Math]::Round($size/32))
            $scale=($size-2*$padding)/[Math]::Max($width,$height)
            $dw=[int][Math]::Round($width*$scale);$dh=[int][Math]::Round($height*$scale)
            $graphics.InterpolationMode=if($scale-gt 1){[Drawing.Drawing2D.InterpolationMode]::NearestNeighbor}else{[Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic}
            $attributes.SetWrapMode([Drawing.Drawing2D.WrapMode]::TileFlipXY)
            $destination=[Drawing.Rectangle]::new([int][Math]::Floor(($size-$dw)/2),[int][Math]::Floor(($size-$dh)/2),$dw,$dh)
            $graphics.DrawImage($sourceBitmap,$destination,$left,$top,$width,$height,[Drawing.GraphicsUnit]::Pixel,$attributes)
            $bitmap.Save($stream,[Drawing.Imaging.ImageFormat]::Png)
            $frames.Add([pscustomobject]@{Size=$size;Bytes=$stream.ToArray()})
            if($size-eq 96){$bitmap.Save([IO.Path]::GetFullPath($Preview),[Drawing.Imaging.ImageFormat]::Png)}
        } finally { $stream.Dispose();$attributes.Dispose();$graphics.Dispose();$bitmap.Dispose() }
    }
} finally { $sourceBitmap.Dispose() }
$memory=[IO.MemoryStream]::new();$writer=[IO.BinaryWriter]::new($memory)
try {
    $writer.Write([uint16]0);$writer.Write([uint16]1);$writer.Write([uint16]$frames.Count)
    $offset=6+16*$frames.Count
    foreach($frame in $frames) {
        $dimension=[byte]$(if($frame.Size-eq 256){0}else{$frame.Size})
        $writer.Write($dimension);$writer.Write($dimension);$writer.Write([byte]0);$writer.Write([byte]0)
        $writer.Write([uint16]1);$writer.Write([uint16]32)
        $writer.Write([uint32]$frame.Bytes.Length);$writer.Write([uint32]$offset)
        $offset+=$frame.Bytes.Length
    }
    foreach($frame in $frames){$writer.Write([byte[]]$frame.Bytes)}
    $writer.Flush();[IO.File]::WriteAllBytes([IO.Path]::GetFullPath($Output),$memory.ToArray())
} finally { $writer.Dispose();$memory.Dispose() }
Write-Output "Head icon generated: 16/20/24/32/40/48/64/96/128/256; source bounds $left,$top,$width,$height."
