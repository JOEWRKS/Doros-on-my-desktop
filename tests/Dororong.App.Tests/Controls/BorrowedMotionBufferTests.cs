using Dororong.App.Controls;
using Xunit.Abstractions;

namespace Dororong.App.Tests.Controls;

public sealed class BorrowedMotionBufferTests(ITestOutputHelper output)
{
    [Fact]
    public void Mixed_motion_warmup_retains_only_sixty_four_body_arrays()
    {
        var bank=new HighResolutionLocomotion();
        var slots=new HashSet<float[]>(ReferenceEqualityComparer.Instance);
        foreach(var frame in Enumerable.Range(225,32))slots.Add(bank.BorrowBody(frame,1.5));
        Assert.Equal(32,slots.Count);
        var walkBytes=slots.Sum(p=>p.LongLength*sizeof(float));
        foreach(var frame in Enumerable.Range(658,48))slots.Add(bank.BorrowBody(frame,1.5));
        Assert.Equal(64,slots.Count);
        var warmBytes=slots.Sum(p=>p.LongLength*sizeof(float));
        bank.Get(0);bank.BorrowEffect(771);
        var sequence=Enumerable.Range(225,32).Concat(Enumerable.Range(257,65))
            .Concat(Enumerable.Range(658,48)).Concat(Enumerable.Range(706,65)).ToArray();
        var before=GC.GetAllocatedBytesForCurrentThread();
        for(var cycle=0;cycle<5;cycle++)foreach(var frame in sequence)
        {
            var pixels=bank.BorrowBody(frame,1.5);
            Assert.True(slots.Contains(pixels),"Warm rendering allocated another retained body array.");
            if(frame is >=706 and <=770)bank.BorrowEffect(frame+65);
        }
        var allocated=GC.GetAllocatedBytesForCurrentThread()-before;
        Assert.Equal(25_920_000,walkBytes);Assert.Equal(51_840_000,warmBytes);
        Assert.True(allocated<40_000_000,$"Warm1050 mixed frames allocated {allocated} bytes.");
        output.WriteLine($"Body storage:32 frames={walkBytes} bytes,64 frames={warmBytes} bytes;1050 warm mixed frames reused the same64 arrays,temporary allocation={allocated} bytes. This is not wall-clock runtime evidence.");
    }

    [Theory]
    [InlineData(1)] [InlineData(1.1)] [InlineData(1.25)] [InlineData(1.35)] [InlineData(1.5)]
    public void Borrowed_frame_reuses_only_expired_owned_slots_and_matches_immutable_body(double scale){
        var bank=new HighResolutionLocomotion();var reference=new HighResolutionLocomotion();
        var slots=new HashSet<float[]>(ReferenceEqualityComparer.Instance);
        for(var frame=1;frame<=75;frame++){
            var borrowed=bank.BorrowBody(frame,scale);slots.Add(borrowed);
            Assert.True(borrowed.SequenceEqual(reference.GetBody(frame,scale)), $"frame {frame} scale {scale}");
            Assert.Equal(RasterAlphaBounds.Get(reference.GetBody(frame,scale),225),RasterAlphaBounds.Get(borrowed,225));
        }
        Assert.Equal(64,slots.Count);
        // Head/effect/general callers may retain their arrays indefinitely.
        var retained=bank.GetBody(75,scale);var retainedPixels=(float[])retained.Clone();
        var raw=bank.Get(74);var rawPixels=(float[])raw.Clone();
        for(var frame=76;frame<=150;frame++)bank.BorrowBody(frame,scale);
        Assert.True(retained.SequenceEqual(retainedPixels));Assert.True(raw.SequenceEqual(rawPixels));
        Assert.True(bank.Get(0).SequenceEqual(reference.Get(0)));
    }
    [Fact]
    public void Warm_owned_cache_does_not_allocate_large_pixel_arrays_on_misses(){
        var bank=new HighResolutionLocomotion();
        for(var frame=1;frame<=70;frame++)bank.BorrowBody(frame,1.5);
        var before=GC.GetAllocatedBytesForCurrentThread();
        for(var frame=71;frame<=110;frame++)bank.BorrowBody(frame,1.5);
        var allocated=GC.GetAllocatedBytesForCurrentThread()-before;
        Assert.True(allocated<4_000_000,$"40 misses allocated {allocated} bytes; expected no 810KB pixel clones.");
        var immutable=new HighResolutionLocomotion();
        for(var frame=1;frame<=70;frame++)immutable.GetBody(frame,1.5);
        before=GC.GetAllocatedBytesForCurrentThread();
        for(var frame=71;frame<=110;frame++)immutable.GetBody(frame,1.5);
        var originalAllocated=GC.GetAllocatedBytesForCurrentThread()-before;
        Assert.True(originalAllocated>30_000_000);
        output.WriteLine($"40 cache misses: immutable path {originalAllocated} bytes; owned render path {allocated} bytes.");
    }
    [Theory]
    [InlineData(1)] [InlineData(1.1)] [InlineData(1.25)] [InlineData(1.35)] [InlineData(1.5)]
    public void Sleep_effect_and_closed_head_do_not_overwrite_current_borrowed_body(double scale){
        var bank=new HighResolutionLocomotion();var reference=new HighResolutionLocomotion();
        for(var frame=1;frame<=70;frame++)bank.BorrowBody(frame,scale);
        foreach(var frame in new[]{321,706,707,735,770,720,836,225,706}){
            var body=bank.BorrowBody(frame,scale);
            var expected=reference.GetBody(frame,scale);
            var head=bank.Get(0);
            if(frame is >=706 and <=770){
                var effect=HighResolutionLocomotion.SleepEffect(frame);
                Assert.True(bank.Get(effect).SequenceEqual(reference.Get(effect)));
            }
            Assert.True(head.SequenceEqual(reference.Get(0)));
            Assert.True(body.SequenceEqual(expected),$"body changed while borrowing other layers: {frame}/{scale}");
        }
    }
}
