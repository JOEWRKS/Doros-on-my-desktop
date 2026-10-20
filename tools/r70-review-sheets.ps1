param([string]$Run='D:/JOEWRKS/DororongDesktopPet/artifacts/snack-scale-20261018-r70')
Add-Type -AssemblyName System.Drawing
$reviewDir=Join-Path $Run 'review'
New-Item -ItemType Directory -Path $reviewDir -Force | Out-Null
$font=New-Object System.Drawing.Font('Consolas',11)
foreach($scale in @('1.00','1.10','1.25','1.35','1.50')){
    $files=Get-ChildItem -LiteralPath (Join-Path $Run 'frames') -Filter "$scale-*.png" | Sort-Object @{Expression={if($_.Name -match '-eating'){1}else{0}}},Name
    $rows=[int][Math]::Ceiling($files.Count/5)
    $sheet=New-Object System.Drawing.Bitmap(1000,(200*$rows))
    $g=[System.Drawing.Graphics]::FromImage($sheet);$g.Clear([System.Drawing.Color]::FromArgb(44,48,54))
    $g.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
    $g.PixelOffsetMode=[System.Drawing.Drawing2D.PixelOffsetMode]::Half
    for($i=0;$i -lt $files.Count;$i++){
        $img=[System.Drawing.Bitmap]::FromFile($files[$i].FullName)
        $x=($i%5)*200;$y=[int][Math]::Floor($i/5)*200
        $s=[double]::Parse($scale,[Globalization.CultureInfo]::InvariantCulture)
        $src=New-Object System.Drawing.RectangleF([single](80*$s),[single](80*$s),[single](100*$s),[single](105*$s))
        $dest=New-Object System.Drawing.RectangleF([single]($x+12),[single]($y+30),$src.Width,$src.Height)
        $g.DrawImage($img,$dest,$src,[System.Drawing.GraphicsUnit]::Pixel)
        $g.DrawString($files[$i].BaseName,$font,[System.Drawing.Brushes]::White,([single]($x+6)),([single]($y+5)))
        $img.Dispose()
    }
    $sheet.Save((Join-Path $reviewDir "$scale.png"),[System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose();$sheet.Dispose()
}
$font.Dispose()
