using System.Windows.Media;
using Dororong.App.Runtime;

namespace Dororong.App.Tests.Runtime;

public sealed class CompositionFrameTimerTests
{
    [Fact]
    public void Starts_once_and_ticks_once_per_composition_timestamp()
    {
        var source = new Source();
        var timer = new CompositionFrameTimer(source.Attach, source.Detach);
        var ticks = 0;
        timer.Tick += (_, _) => ticks++;
        source.Raise(0);
        timer.Start(); timer.Start();
        Assert.Equal(1, source.Attached);
        for (var i = 0; i < 100; i++) { source.Raise(i * 16); source.Raise(i * 16); }
        Assert.Equal(100, ticks);
        timer.Stop(); timer.Stop();
        source.Raise(1600);
        Assert.Equal(100, ticks);
        Assert.Equal(1, source.Detached);
    }

    [Fact]
    public void Stop_inside_tick_detaches_and_restart_accepts_first_frame()
    {
        var source = new Source();
        var timer = new CompositionFrameTimer(source.Attach, source.Detach);
        var ticks = 0;
        timer.Tick += (_, _) => { ticks++; timer.Stop(); };
        timer.Start(); source.Raise(16); source.Raise(32);
        Assert.Equal(1, ticks);
        Assert.False(source.HasHandler);
        timer.Start(); source.Raise(16);
        Assert.Equal(2, ticks);
        Assert.False(source.HasHandler);
    }

    [Fact]
    public void Nested_dispatch_does_not_run_a_second_simulation()
    {
        var source = new Source();
        var timer = new CompositionFrameTimer(source.Attach, source.Detach);
        var ticks = 0;
        timer.Tick += (_, _) => { ticks++; source.Raise(100); };
        timer.Start(); source.Raise(16);
        Assert.Equal(1, ticks);
        source.Raise(32);
        Assert.Equal(2, ticks);
        timer.Stop();
    }

    [Fact]
    public void Throwing_handler_does_not_leave_timer_reentrancy_locked()
    {
        var source = new Source();
        var timer = new CompositionFrameTimer(source.Attach, source.Detach);
        var ticks = 0;
        timer.Tick += (_, _) => { if (++ticks == 1) throw new InvalidOperationException(); };
        timer.Start();
        Assert.Throws<InvalidOperationException>(() => source.Raise(16));
        source.Raise(32);
        Assert.Equal(2, ticks);
        timer.Stop();
    }

    private sealed class Source
    {
        private EventHandler? handler;
        internal int Attached, Detached;
        internal bool HasHandler => handler is not null;
        internal void Attach(EventHandler value) { Attached++; handler += value; }
        internal void Detach(EventHandler value) { Detached++; handler -= value; }
        internal void Raise(int ms) => handler?.Invoke(this, (RenderingEventArgs)Activator.CreateInstance(
            typeof(RenderingEventArgs), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
            null, new object[] { TimeSpan.FromMilliseconds(ms) }, null)!);
    }
}
