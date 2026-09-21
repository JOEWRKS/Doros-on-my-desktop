using System.Reflection;
using Dororong.App.Product;
using Forms = System.Windows.Forms;

namespace Dororong.SingleInstanceProbe;

internal static class TrayMenuProbe
{
    internal static int Run(string scenario, string receipt)
    {
        Exception? failure = null;
        var worker = new Thread(() =>
        {
            try { CheckScenario(scenario); }
            catch (Exception error) { failure = error; }
            finally { Forms.Application.ExitThread(); }
        });
        worker.SetApartmentState(ApartmentState.STA);
        worker.Start();
        worker.Join();
        File.WriteAllText(receipt, failure is null ? "PASS" : failure.ToString());
        return failure is null ? 0 : 1;
    }

    private static void Check(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
    }

    private static void CheckScenario(string scenario)
    {
        using var icon = new Forms.NotifyIcon();
        using var menu = new TestMenu();
        var logs = 0;
        var exits = 0;
        using var service = new TrayService(
            (System.Drawing.Icon)System.Drawing.SystemIcons.Application.Clone(), icon, menu,
            () => logs++, () => exits++, visible: false);
        var openings = 0;
        menu.Opened += (_, _) => openings++;
        void Left() => typeof(Forms.NotifyIcon).GetMethod("OnMouseUp", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(icon, [new Forms.MouseEventArgs(Forms.MouseButtons.Left, 1, 0, 0, 0)]);

        if (scenario == "right")
        {
            var message = Forms.Message.Create(IntPtr.Zero, 0x400 + 1024, IntPtr.Zero, new IntPtr(0x0205));
            typeof(Forms.NotifyIcon).GetMethod("WndProc", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(icon, [message]);
            Check(menu.Visible && openings == 1 && ReferenceEquals(menu, icon.ContextMenuStrip),
                "Native right-click must open the existing menu exactly once.");
            return;
        }

        Left();
        Check(menu.Visible, "Left click did not open the existing menu.");
        if (scenario == "repeat-escape")
        {
            var handle = menu.Handle;
            Left(); Left();
            Check(openings == 1 && handle == menu.Handle, "Repeated clicks duplicated/reopened the popup.");
            Check(menu.Escape() && !menu.Visible, "Escape did not dismiss the popup.");
            Left();
            Check(menu.Visible && openings == 2, "Popup could not be reopened.");
            menu.Items[1].PerformClick(); menu.Items[2].PerformClick();
            Check(logs == 1 && exits == 1, "Existing commands must execute once.");
        }
        else if (scenario == "outside")
        {
            using var outside = new OutsideWindow();
            Forms.ToolStripDropDownCloseReason? reason = null;
            menu.Closed += (_, e) => reason = e.CloseReason;
            var click = Forms.Message.Create(outside.Handle, 0x0201, new IntPtr(1), new IntPtr(0x00050005));
            Forms.Application.FilterMessage(ref click);
            Check(!menu.Visible && reason == Forms.ToolStripDropDownCloseReason.AppClicked,
                "Outside-window click did not dismiss the popup through the real menu filter.");
        }
        else throw new ArgumentException("Unknown tray scenario.", nameof(scenario));
    }

    private sealed class TestMenu : Forms.ContextMenuStrip
    {
        internal bool Escape() => ProcessDialogKey(Forms.Keys.Escape);
    }

    private sealed class OutsideWindow : Forms.NativeWindow, IDisposable
    {
        internal OutsideWindow() => CreateHandle(new Forms.CreateParams
        {
            Caption = "Tray test outside window", X = 800, Y = 400,
            Width = 200, Height = 150, Style = unchecked((int)0x80000000)
        });
        public void Dispose() => DestroyHandle();
    }
}
