param([string]$Run='D:/JOEWRKS/DororongDesktopPet/artifacts/seated-sleep-20261018-r69')
Add-Type -AssemblyName System.Drawing
$reviewDir=Join-Path $Run 'review'
New-Item -ItemType Directory -Path $reviewDir -Force | Out-Null
$font=New-Object System.Drawing.Font('Consolas',11)
foreach($scale in @('1.00','1.10','1.25','1.35','1.50')){
    foreach($group in @('sit','sleep')){
        $files=Get-ChildItem -LiteralPath (Join-Path $Run 'final-frames') -Filter "$scale-*.png" | Where-Object {if($group -eq 'sit'){$_.Name -match '-sit-'}else{$_.Name -match '-sleep-|-wake-'}} | Sort-Object Name
        $cell=210; $rowHeight=210; $cols=5; $rows=[int][Math]::Ceiling($files.Count/$cols)
        $sheet=New-Object System.Drawing.Bitmap(($cell*$cols),($rowHeight*$rows))
        $g=[System.Drawing.Graphics]::FromImage($sheet);$g.Clear([System.Drawing.Color]::FromArgb(44,48,54))
        $g.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
        $g.PixelOffsetMode=[System.Drawing.Drawing2D.PixelOffsetMode]::Half
        for($i=0;$i -lt $files.Count;$i++){
            $img=[System.Drawing.Bitmap]::FromFile($files[$i].FullName)
            $x=($i%$cols)*$cell;$y=[int][Math]::Floor($i/$cols)*$rowHeight
            # Crop only the empty render canvas margin; retain original display pixels.
            $s=[double]::Parse($scale,[Globalization.CultureInfo]::InvariantCulture)
            $src=New-Object System.Drawing.RectangleF([single](80*$s),[single](80*$s),[single](100*$s),[single](105*$s))
            $dest=New-Object System.Drawing.RectangleF([single]($x+12),[single]($y+30),$src.Width,$src.Height)
            $g.DrawImage($img,$dest,$src,[System.Drawing.GraphicsUnit]::Pixel)
            $label=$files[$i].BaseName.Substring(5)
            $g.DrawString($label,$font,[System.Drawing.Brushes]::White,([single]($x+6)),([single]($y+5)))
            $img.Dispose()
        }
        $sheet.Save((Join-Path $reviewDir "$scale-$group.png"),[System.Drawing.Imaging.ImageFormat]::Png)
        $g.Dispose();$sheet.Dispose()
    }
}
$font.Dispose()
