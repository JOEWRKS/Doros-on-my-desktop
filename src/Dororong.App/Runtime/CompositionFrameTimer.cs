using System.Windows.Media;

namespace Dororong.App.Runtime;

// Advance immediately before a new WPF frame, not 16 ms after the previous
// simulation/render finishes. Physics still uses PetLoop's elapsed clock.
// Rendering can be raised more than once for one composition timestamp.
internal sealed class CompositionFrameTimer
{
    private readonly Action<EventHandler> attach, detach;
    private bool running, ticking;
    private TimeSpan? lastFrame;
    internal event EventHandler? Tick;

    internal CompositionFrameTimer()
        : this(h => CompositionTarget.Rendering += h, h => CompositionTarget.Rendering -= h) { }

    internal CompositionFrameTimer(Action<EventHandler> attach, Action<EventHandler> detach)
    {
        this.attach = attach;
        this.detach = detach;
    }

    internal void Start() => Start(false);
    internal void Start(bool preserveTimestamp)
    {
        if (running) return;
        if (!preserveTimestamp) lastFrame = null;
        running = true;
        attach(OnRendering);
    }

    internal void Stop()
    {
        if (!running) return;
        running = false;
        detach(OnRendering);
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        if (!running || ticking || e is not RenderingEventArgs frame || lastFrame == frame.RenderingTime) return;
        lastFrame = frame.RenderingTime;
        ticking = true;
        try { Tick?.Invoke(this, EventArgs.Empty); }
        finally { ticking = false; }
    }
}
