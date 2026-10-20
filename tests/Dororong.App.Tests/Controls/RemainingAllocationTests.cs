using System.Reflection;
using Dororong.App.Controls;
using Dororong.App.Interop;
using Dororong.Core.Platforms;
using Xunit.Abstractions;

namespace Dororong.App.Tests.Controls;

public sealed class RemainingAllocationTests(ITestOutputHelper output)
{
    [Fact]
    public void Repeated_sleep_and_walk_do_not_replace_large_body_slots(){
        var bank=new HighResolutionLocomotion();
        var method=typeof(HighResolutionLocomotion).GetMethod("BorrowEffect",BindingFlags.Instance|BindingFlags.NonPublic);
        var effect=method is null ? bank.Get : method.CreateDelegate<Func<int,float[]>>(bank);
        void Cycle(){
            for(var frame=706;frame<=770;frame++){bank.BorrowBody(frame,1.5);effect(frame+65);}
            for(var frame=225;frame<=256;frame++)bank.BorrowBody(frame,1.5);
        }
        Cycle();Cycle();
        var before=GC.GetAllocatedBytesForCurrentThread();Cycle();
        var allocated=GC.GetAllocatedBytesForCurrentThread()-before;
        output.WriteLine($"One warm sleep/walk cycle allocated {allocated} bytes.");
        Assert.True(allocated<4_000_000,$"Warm sleep/walk allocated {allocated} bytes.");
    }
    [Fact]
    public void Repeated_native_snapshots_keep_identity_without_dictionary_churn(){
        var windows=Enumerable.Range(1,200).Select(i=>new NativeWindow(i,7,new(0,0,100,100))).ToArray();
        var reader=new Metadata(new([new(1,new(0,0,1000,1000))],windows));
        var native=new DesktopSceneNative(reader);
        var first=native.Capture(TimeSpan.Zero)!;
        native.Capture(TimeSpan.FromSeconds(1));
        var before=GC.GetAllocatedBytesForCurrentThread();
        for(var i=0;i<100;i++)native.Capture(TimeSpan.FromSeconds(i+2));
        var allocated=GC.GetAllocatedBytesForCurrentThread()-before;
        output.WriteLine($"100 snapshots allocated {allocated} bytes.");
        Assert.True(allocated<2_100_000,$"Snapshots allocated {allocated} bytes.");
        Assert.Equal(first.Windows.Select(w=>w.Key),native.Capture(TimeSpan.FromSeconds(102))!.Windows.Select(w=>w.Key));
    }
    private sealed class Metadata(NativeDesktop desktop):IDesktopMetadataReader{
        public NativeDesktop? Read()=>desktop;
    }
    [Theory]
    [InlineData(1)] [InlineData(1.1)] [InlineData(1.25)] [InlineData(1.35)] [InlineData(1.5)]
    public void Borrowed_effects_match_all_immutable_sleep_frames_without_touching_body(double scale){
        var bank=new HighResolutionLocomotion();var reference=new HighResolutionLocomotion();
        var immutable=bank.Get(790);var retained=(float[])immutable.Clone();
        float[]? slot=null;
        for(var frame=706;frame<=770;frame++){
            var body=bank.BorrowBody(frame,scale);
            var effect=bank.BorrowEffect(frame+65);
            if(slot is not null)Assert.Same(slot,effect);slot=effect;
            Assert.Equal(reference.Get(frame+65),effect);
            Assert.Equal(reference.GetBody(frame,scale),body);
            Assert.Equal(RasterAlphaBounds.Get(reference.Get(frame+65),225),RasterAlphaBounds.Get(effect,225));
            Assert.Same(effect,bank.BorrowEffect(frame+65));
        }
        Assert.Equal(retained,immutable);
    }
    [Fact]
    public void Bounds_invalidation_keeps_weak_entry_but_recomputes_immutable_result(){
        var pixels=new float[16*16*4];pixels[3]=255;
        var first=RasterAlphaBounds.Get(pixels,16);
        var field=typeof(RasterAlphaBounds).GetField("cache",BindingFlags.Static|BindingFlags.NonPublic)!;
        var table=field.GetValue(null)!;var lookup=table.GetType().GetMethod("TryGetValue")!;
        object Entry(){object?[] args=[pixels,null];Assert.True((bool)lookup.Invoke(table,args)!);return args[1]!;}
        var entry=Entry();
        for(var i=1;i<30;i++){
            Array.Clear(pixels);pixels[(i%256)*4+3]=255;RasterAlphaBounds.Invalidate(pixels);
            var actual=RasterAlphaBounds.Get(pixels,16);
            Assert.Same(entry,Entry());Assert.Equal(i%16,actual.Left);Assert.Equal(i/16,actual.Top);
        }
        Assert.Equal(0,first.Left);Assert.Equal(0,first.Top);
    }
}
