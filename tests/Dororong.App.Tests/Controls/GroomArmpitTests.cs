using Dororong.App.Controls;

namespace Dororong.App.Tests.Controls;

public sealed class GroomArmpitTests
{
    [Theory][InlineData(1)][InlineData(1.25)][InlineData(1.5)]
    public void Grooming_removes_the_branch_through_raise_wipe_and_lower(double scale)
    {
        var body=new HighResolutionLocomotion().BorrowBody(HighResolutionLocomotion.Sit(1),scale);
        Span<double> pixel=stackalloc double[4];Span<double> scratch=stackalloc double[4];
        // Include the upper branch rejected by the user, not only its low tip.
        var originalInkSamples=0;
        foreach(var cover in new[]{.004,.02,.1,.3,.6,1d}){
        var warp=new HighResolutionGrabRaster.ChuckleWarp(HighResolutionFacePose.Neutral with{ChuckleCover=cover,GroomingPaw=true},scale);
        for(var row=185;row<=200;row++)for(var col=70;col<=72;col++){
            var x=(col+.5)*96/225;var y=(row+.5)*96/225;
            for(var c=0;c<4;c++)pixel[c]=body[(row*225+col)*4+c];
            if(pixel[3]>20&&pixel[0]<pixel[3]*.6)originalInkSamples++;
            warp.RemovePaw([],x,y,pixel,scratch);
            Assert.Equal(0,pixel[3]);
        }
        }
        Assert.True(originalInkSamples>0,"Tail test must include real source ink, not just empty pixels.");
    }

    [Theory][InlineData(1)][InlineData(1.25)][InlineData(1.5)]
    public void Grooming_removal_never_weakens_the_upper_crease_at_any_cover(double scale)
    {
        var body=new HighResolutionLocomotion().BorrowBody(HighResolutionLocomotion.Sit(1),scale);
        var raster=new HighResolutionGrabRaster();
        var standing=(float[])typeof(HighResolutionGrabRaster).GetMethod("Body",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!.Invoke(raster,[0d])!;
        standing=HighResolutionBodyInk.AtScale(standing,375,scale);
        Span<double> a=stackalloc double[4];Span<double> b=stackalloc double[4];Span<double> scratch=stackalloc double[4];
        // Only the top fork is a required junction. Rows183..192 in the old
        // test incorrectly protected the downward branch the user rejected.
        foreach(var cover in new[]{.01,.1,.3,.6,1d}){
            var pose=HighResolutionFacePose.Neutral with{ChuckleCover=cover};
            var accepted=new HighResolutionGrabRaster.ChuckleWarp(pose,scale);
            var corrected=new HighResolutionGrabRaster.ChuckleWarp(pose with{GroomingPaw=true},scale);
            for(var row=179;row<=182;row++)for(var col=70;col<=74;col++){
                var x=(col+.5)*96/225;var y=(row+.5)*96/225;
                for(var c=0;c<4;c++)a[c]=b[c]=body[(row*225+col)*4+c];
                accepted.RemovePaw(standing,x,y,a,scratch);corrected.RemovePaw(standing,x,y,b,scratch);
                Assert.True(b[3]>=a[3],$"Armpit weakened: {cover} at {x},{y}");
            }
        }
    }

    [Theory][InlineData(1)][InlineData(1.25)][InlineData(1.5)]
    public void Exposed_seated_junction_retains_source_ink_and_opacity(double scale)
    {
        var body=new HighResolutionLocomotion().BorrowBody(HighResolutionLocomotion.Sit(1),scale);
        var warp=new HighResolutionGrabRaster.ChuckleWarp(HighResolutionFacePose.Neutral with{ChuckleCover=1,GroomingPaw=true,GroomSweep=1},scale);
        Span<double> pixel=stackalloc double[4];Span<double> scratch=stackalloc double[4];
        var ink=0;
        // Real seated outline exposed when the wiping paw rotates away.
        // The old standing-paw mask retained only partial opacity here.
        foreach(var (col,row) in new[]{(69,179),(70,182),(77,187)}){
            var x=(col+.5)*96/225;var y=(row+.5)*96/225;var at=(row*225+col)*4;
            for(var c=0;c<4;c++)pixel[c]=body[at+c];
            if(pixel[3]>200&&pixel[0]<pixel[3]*.7)ink++;
            warp.RemovePaw([],x,y,pixel,scratch);
            for(var c=0;c<4;c++)Assert.Equal((double)body[at+c],pixel[c]);
        }
        Assert.True(ink>0,"Junction regression must protect actual dark source ink.");
    }

    [Fact]
    public void Cleanup_preserves_far_body_and_is_not_enabled_for_chuckle()
    {
        var normal=new HighResolutionGrabRaster.ChuckleWarp(HighResolutionFacePose.Neutral with{ChuckleCover=1});
        var groom=new HighResolutionGrabRaster.ChuckleWarp(HighResolutionFacePose.Neutral with{ChuckleCover=1,GroomingPaw=true});
        Span<double> a=stackalloc double[4];Span<double> b=stackalloc double[4];Span<double> scratch=stackalloc double[4];
        foreach(var x in new[]{33d,35,40,50})foreach(var y in new[]{78d,82,88,92}){
            for(var c=0;c<4;c++)a[c]=b[c]=c==3?255:60;
            normal.RemovePaw([],x,y,a,scratch);groom.RemovePaw([],x,y,b,scratch);
            Assert.True(a.SequenceEqual(b));
        }
        a.Fill(255);normal.RemovePaw([],31,80,a,scratch);
        Assert.InRange(a[3],188,190); // Accepted chuckle path unchanged.
    }
}
