param([string]$Run='D:/JOEWRKS/DororongDesktopPet/artifacts/render-cost-20261018-r72', [string]$Candidate='after')
Add-Type -AssemblyName System.Drawing
$reviewDir=Join-Path $Run 'review'
New-Item -ItemType Directory -Path $reviewDir -Force | Out-Null
$font=New-Object System.Drawing.Font('Consolas',10)
foreach($scale in @('1.00','1.10','1.25','1.35','1.50')){
    $sheet=New-Object System.Drawing.Bitmap(1296,1320)
    $g=[System.Drawing.Graphics]::FromImage($sheet)
    $g.Clear([System.Drawing.Color]::FromArgb(44,48,54))
    $g.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
    $g.PixelOffsetMode=[System.Drawing.Drawing2D.PixelOffsetMode]::Half
    $row=0
    foreach($kind in @('grab','release','landing','tilted-landing','track','cheek')){
        $col=0
        foreach($index in @('00','05','11')){
            foreach($stage in @('before',$Candidate)){
                $name="$scale-$kind-$index"
                $img=[System.Drawing.Bitmap]::FromFile((Join-Path $Run "$stage/$name.png"))
                $s=[double]::Parse($scale,[Globalization.CultureInfo]::InvariantCulture)
                $src=New-Object System.Drawing.RectangleF([single](64*$s),[single](64*$s),[single](128*$s),[single](128*$s))
                $x=$col*216;$y=$row*220
                $dest=New-Object System.Drawing.RectangleF([single]($x+8),[single]($y+25),$src.Width,$src.Height)
                $g.DrawImage($img,$dest,$src,[System.Drawing.GraphicsUnit]::Pixel)
                $g.DrawString("$stage $kind $index",$font,[System.Drawing.Brushes]::White,([single]($x+3)),([single]($y+4)))
                $img.Dispose();$col++
            }
        }
        $row++
    }
    $sheet.Save((Join-Path $reviewDir "$scale.png"),[System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose();$sheet.Dispose()
}
$font.Dispose()
