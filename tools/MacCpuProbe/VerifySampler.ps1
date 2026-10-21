$ErrorActionPreference='Stop'
$root=Resolve-Path (Join-Path $PSScriptRoot '../..')
$source=(Get-Content -LiteralPath (Join-Path $root 'src/Dororong.App/Controls/HighResolutionGrabRaster.cs') -Raw).Replace("`r`n","`n").TrimEnd()
$mac=(Get-Content -LiteralPath (Join-Path $root 'src/Dororong.Raster/HighResolutionGrabRaster.cs') -Raw).Replace("`r`n","`n").TrimEnd()
$header="// Mac-only r104 sampler copy. Arithmetic is identical to the Windows source;`n// only retained-region clearing and destination-loop clipping are added here.`n"
$clear=@'
else if(_renderRegion is {} dirty){
            for(var y=dirty.Top;y<dirty.Bottom;y++)Array.Clear(outputBuffer,(y*extent+dirty.Left)*4,(dirty.Right-dirty.Left)*4);
        }else Array.Clear(outputBuffer,0,length);
'@
$clip=@'
        if(_renderRegion is {} region){
            minX=Math.Max(minX,region.Left);maxX=Math.Min(maxX,region.Right);
            minY=Math.Max(minY,region.Top);maxY=Math.Min(maxY,region.Bottom);
        }
        if(!soleOnly)LastRenderedPixelCount=(long)Math.Max(0,maxX-minX)*Math.Max(0,maxY-minY);
'@
$expected=$header+$source.Replace('else Array.Clear(outputBuffer,0,length);',$clear.Replace("`r`n","`n")).Replace('        float[]? low=null;',$clip.Replace("`r`n","`n")+"`n        float[]? low=null;")
if($mac -cne $expected){throw 'Mac sampler differs beyond the two reviewed clear/loop hooks'}
'Sampler arithmetic identical: only region clearing, clipping and diagnostic pixel count added.'
