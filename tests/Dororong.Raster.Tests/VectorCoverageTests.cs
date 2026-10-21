using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Dororong.App.Controls;
using SkiaSharp;

namespace Dororong.Raster.Tests;

public sealed class VectorCoverageTests
{
    // Float-array hashes exported from the immutable accepted r104 DLL,
    // not blessed from the portable implementation. Includes both sides of
    // sharp neck bends: missing inner-rail tiles used to leave a small gap.
    [Theory]
    [InlineData(0,-20,"088205DB4EEB1A3CD47BB7EC50C1F4BDC6F6C380400A21B76123306A766A9956")]
    [InlineData(25,20,"FE87B96747C07F7CD3F9A09E270E1C32784BB81F6488732574166D9179A958A4")]
    [InlineData(60,0,"81CB530C6B2A83398C659258D7019426FD4D7AA9856D76B78A0371E8E888CE3E")]
    [InlineData(60,20,"4CB03ECEBBE1EF828FA1C751212490EF01D82D9D87998F77B0F59A602A2B2708")]
    [InlineData(140,-10,"A780BC7806C376E73249EC6558F61A601433F1543BD701E2018A2E3310A76A5E")]
    [InlineData(150,-4.583662361046586,"48947DDC930A4B92B53C1C83DC2D37ED9E0DDF2C3A11DC4BEC0C3F134566EDF1")]
    public void NeckMatchesReferenceIncludingRefinedCorners(int frame,double angle,string expected)
    {
        var head=HuntRenderer.HeadTransform(frame,new(0,0,angle*Math.PI/180));
        var buffer=new HighResolutionHuntNeck.Buffer();
        var face=HighResolutionFacePose.Neutral with { Head=head,BodyFrame=frame };
        var pixels=buffer.Get(face);
        Assert.Equal(expected,Hash(pixels));
        Assert.Same(pixels,buffer.Get(face));
        Assert.Same(pixels,buffer.Get(face with { BodyFrame=(frame+1)%157 }));
    }

    [Theory]
    [InlineData(0,"C3AD7D5993F8BC5DECD4B321C2FA45AB8DD32C2660CA1A4C198A3F1E31F4656F")]
    [InlineData(.7,"A2ACD082476239EA7E536F058D54C5656A6CDFE22BB60E8FDED789A2E53524D5")]
    [InlineData(10000.123,"B96CB918BADA243BA24DB95A8387B4BBA2A28072C720CB757E15767A0399CB41")]
    public void DroolMatchesReferenceWithoutTimeQuantization(double age,string expected)
    {
        var buffer=new HighResolutionSnackEffects.Buffer();var pixels=buffer.Get(-1,age);
        Assert.Equal(expected,Hash(pixels));
        Assert.Same(pixels,buffer.Get(-1,age+.013));
        Assert.Same(pixels,buffer.Get(11/60d,-1)); // switching to the fixed eating bank
        Assert.Same(pixels,buffer.Get(-1,age));
        Assert.Equal(expected,Hash(pixels)); // returning must not retain mouth pixels
    }

    private static string Hash(float[] pixels)=>Convert.ToHexString(SHA256.HashData(MemoryMarshal.AsBytes(pixels.AsSpan())));

    [Fact]
    public void CoverageScratchReusesItsHighWaterStorage()
    {
        var scratch=new MilCoverage.Scratch();var pixels=new byte[225*225*4];
        void Cycle()
        {
            for(int i=0;i<120;i++)
            {
                using var path=MilCoverage.Ellipse(72.65625,160+i/10d,3.046875,4.6875);
                using var stroke=MilCoverage.Widen(path,1.0546875,false,scratch);
                Array.Clear(pixels);MilCoverage.Fill(pixels,225,225,stroke,SKColors.White,scratch);
            }
        }
        int[] Capacities()=>[scratch.Pool.Capacity,scratch.Contours.Capacity,scratch.Samples.Capacity,scratch.Reverse.Capacity,scratch.Hits.Capacity,..scratch.Pool.Select(p=>p.Capacity)];
        Cycle();var row=scratch.Row;var pointStorage=scratch.Points;var highWater=Capacities();
        for(int repeat=0;repeat<4;repeat++)Cycle();
        Assert.Same(row,scratch.Row);Assert.Same(pointStorage,scratch.Points);Assert.Equal(highWater,Capacities());
    }
}
