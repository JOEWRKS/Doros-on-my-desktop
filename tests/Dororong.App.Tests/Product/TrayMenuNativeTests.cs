using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Dororong.App.Tests.Product;

public sealed class TrayMenuNativeTests
{
    [Theory]
    [InlineData("repeat-escape")]
    [InlineData("outside")]
    [InlineData("right")]
    public void Real_tray_popup_preserves_dismissal_reuse_and_native_commands(string scenario)
    {
        var output = new DirectoryInfo(AppContext.BaseDirectory);
        var bin = Ancestor(output, d => d.Name.Equals("bin", StringComparison.OrdinalIgnoreCase));
        var root = Ancestor(output, d => File.Exists(Path.Combine(d.FullName, "DororongDesktopPet.sln")));
        var probe = Path.Combine(root.FullName, "tests/support/Dororong.SingleInstanceProbe/bin",
            Path.GetRelativePath(bin.FullName, output.FullName), "Dororong.SingleInstanceProbe.exe");
        Assert.True(File.Exists(probe), $"Probe not built: {probe}");
        var receipts = Path.Combine(root.FullName, "tests/Dororong.App.Tests/TestResults");
        Directory.CreateDirectory(receipts);
        var receipt = Path.Combine(receipts, $"tray-{scenario}-{Guid.NewGuid():N}.txt");
        var desktopName = "DororongTrayTest-" + Guid.NewGuid().ToString("N");
        var desktop = CreateDesktop(desktopName, IntPtr.Zero, IntPtr.Zero, 0, 0x01FF, IntPtr.Zero);
        if (desktop == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());
        var process = new ProcessInformation();
        try
        {
            // Isolate the process: Windows IME helpers can outlive a test thread.
            var startup = new StartupInfo { Size = Marshal.SizeOf<StartupInfo>(), Desktop = desktopName };
            var arguments = new StringBuilder($"\"{probe}\" tray-menu {scenario} \"{receipt}\"");
            if (!CreateProcess(probe, arguments, IntPtr.Zero, IntPtr.Zero, false,
                    0x08000000, IntPtr.Zero, Path.GetDirectoryName(probe)!, ref startup, out process))
                throw new Win32Exception(Marshal.GetLastWin32Error());
            Assert.Equal(0u, WaitForSingleObject(process.Process, 15000));
            Assert.True(GetExitCodeProcess(process.Process, out var code));
            var result = File.Exists(receipt) ? File.ReadAllText(receipt) : "No probe receipt.";
            Assert.True(code == 0, result);
            Assert.Equal("PASS", result);
        }
        finally
        {
            if (process.Process != IntPtr.Zero)
            {
                if (WaitForSingleObject(process.Process, 0) == 0x102)
                {
                    // Only our freshly created isolated probe, never a pet.
                    TerminateProcess(process.Process, 137);
                    WaitForSingleObject(process.Process, 5000);
                }
                CloseHandle(process.Thread);
                CloseHandle(process.Process);
            }
            Assert.True(CloseDesktop(desktop), $"Test desktop cleanup failed: {Marshal.GetLastWin32Error()}");
        }
    }

    private static DirectoryInfo Ancestor(DirectoryInfo start, Func<DirectoryInfo, bool> match)
    {
        for (DirectoryInfo? current = start; current is not null; current = current.Parent)
            if (match(current)) return current;
        throw new InvalidOperationException("Test output/repository ancestor not found.");
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct StartupInfo
    {
        internal int Size;
        internal string? Reserved, Desktop, Title;
        internal int X, Y, XSize, YSize, XCountChars, YCountChars, FillAttribute, Flags;
        internal short ShowWindow, ReservedBytes;
        internal IntPtr ReservedPointer, StdInput, StdOutput, StdError;
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct ProcessInformation
    {
        internal IntPtr Process, Thread;
        internal uint ProcessId, ThreadId;
    }
    [DllImport("user32.dll", EntryPoint = "CreateDesktopW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateDesktop(string name, IntPtr device, IntPtr devMode, uint flags, uint access, IntPtr attributes);
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseDesktop(IntPtr desktop);
    [DllImport("kernel32.dll", EntryPoint = "CreateProcessW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CreateProcess(string application, StringBuilder arguments, IntPtr processAttributes,
        IntPtr threadAttributes, bool inheritHandles, uint flags, IntPtr environment, string directory,
        ref StartupInfo startup, out ProcessInformation process);
    [DllImport("kernel32.dll")]
    private static extern uint WaitForSingleObject(IntPtr handle, uint milliseconds);
    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetExitCodeProcess(IntPtr process, out uint code);
    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool TerminateProcess(IntPtr process, uint code);
    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr handle);
}
