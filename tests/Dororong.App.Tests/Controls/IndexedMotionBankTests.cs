using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;
namespace Dororong.App.Tests.Controls;

public sealed class IndexedMotionBankTests
{
    private static byte[] Original(string name)=>Gzip(File.ReadAllBytes(Path.Combine(
        EdgePerchPresentationTests.ProjectRoot(),"src/Dororong.App/Assets",name+".pbgra.gz")));
    private static byte[] Gzip(byte[] bytes)
    {using var input=new MemoryStream(bytes);using var gzip=new GZipStream(input,CompressionMode.Decompress);using var output=new MemoryStream();gzip.CopyTo(output);return output.ToArray();}
    private static byte[] Pixels(BitmapSource image)
    {var bytes=new byte[image.PixelWidth*image.PixelHeight*4];image.CopyPixels(bytes,image.PixelWidth*4,0);Assert.True(image.IsFrozen);return bytes;}

    [Fact] public void Head_request_never_decodes_unrequested_bodies_and_reuses_captures()=>EdgePerchPresentationTests.Sta(()=>
    {
        var hunt=new HuntFrames();Assert.Equal(0,hunt.DecodedCount);
        var head=hunt.Head(0);Assert.Equal(1,hunt.DecodedCount);Assert.Equal(head,hunt.Head(0));
        var body=hunt.Body(60);Assert.Equal(2,hunt.DecodedCount);Assert.Same(body,hunt.Body(60));
        var big=new byte[384*384*4];hunt.CopyEnlargedHead(0,big);hunt.CopyEnlargedHead(0,big);Assert.Equal(3,hunt.DecodedCount);
        Assert.Equal(head,hunt.Head(0));Assert.Equal(3,hunt.DecodedCount);
    });
    [Fact] public void Locomotion_only_decodes_requested_frames()=>EdgePerchPresentationTests.Sta(()=>
    {
        var frames=new LocomotionFrames.LazyFrames();Assert.Equal(772,frames.Count);Assert.Equal(0,frames.DecodedCount);
        var frame=frames[65];Assert.Same(frame,frames[65]);Assert.Equal(1,frames.DecodedCount);
        Assert.True(LocomotionFrames.Contains(frame));Assert.Equal(96,frames[771].PixelWidth);Assert.Equal(2,frames.DecodedCount);
    });
    [Fact] public void All_933_blocks_equal_approved_pixels_exactly()=>EdgePerchPresentationTests.Sta(()=>
    {
        var root=EdgePerchPresentationTests.ProjectRoot();var hunt=new HuntFrames();var original=Original("hunting");var offset=20;
        var timings=new List<double>();
        for(var i=0;i<157;i++)
        {
            var watch=Stopwatch.StartNew();var frame=hunt.Body(i);watch.Stop();timings.Add(watch.Elapsed.TotalMilliseconds);
            var bytes=Pixels(frame);Assert.Equal(original.AsSpan(offset,bytes.Length).ToArray(),bytes);offset+=bytes.Length;
        }
        for(var i=0;i<2;i++)
        {
            var head=hunt.Head(i).ToArray();Assert.Equal(original.AsSpan(offset,head.Length).ToArray(),head);offset+=head.Length;
            var big=new byte[384*384*4];hunt.CopyEnlargedHead(i,big);Assert.Equal(original.AsSpan(offset,big.Length).ToArray(),big);offset+=big.Length;
        }
        Assert.Equal(original.Length,offset);Assert.Equal(161,hunt.DecodedCount);
        original=Original("locomotion");offset=20;var frames=new LocomotionFrames.LazyFrames();
        for(var i=0;i<frames.Count;i++)
        {
            var watch=Stopwatch.StartNew();var frame=frames[i];watch.Stop();timings.Add(watch.Elapsed.TotalMilliseconds);
            var bytes=Pixels(frame);Assert.Equal(original.AsSpan(offset,bytes.Length).ToArray(),bytes);offset+=bytes.Length;
        }
        Assert.Equal(original.Length,offset);Assert.Equal(772,frames.DecodedCount);
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"indexed-motion-timings.json"),JsonSerializer.Serialize(new{
            scope="Cold per-frame decode on this Windows test run, not boot or Mac evidence",frames=timings.Count,
            medianMs=timings.Order().ElementAt(timings.Count/2),p95Ms=timings.Order().ElementAt((int)(timings.Count*.95)),maxMs=timings.Max()}));
    });
    [Theory][InlineData(0)][InlineData(12)][InlineData(16)][InlineData(20)]
    public void Invalid_header_offset_length_and_size_fail_closed(int at)
    {
        var path=Path.Combine(EdgePerchPresentationTests.ProjectRoot(),"src/Dororong.App/Assets/locomotion.indexed");
        var bytes=File.ReadAllBytes(path);bytes[at]^=0x7f;
        Assert.Throws<InvalidDataException>(()=>new IndexedMotionBank(()=>new MemoryStream(bytes),Enumerable.Repeat(96*96*4,772).ToArray()));
    }
}
