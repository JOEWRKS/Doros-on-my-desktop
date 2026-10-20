using System.IO;
using Microsoft.Win32;

namespace Dororong.App.Product;

internal sealed class PetSizePreferences
{
    internal const string ValueName = "SizePercent";
    private readonly Func<object?> read;
    private readonly Action<int> write;
    internal PetSizePreferences() : this(Read, Write) { }
    internal PetSizePreferences(Func<object?> read, Action<int> write)
    {
        this.read = read; this.write = write;
    }
    internal int? Percent => read() switch
    {
        null => null,
        int n when n is >= 100 and <= 150 => n,
        _ => throw new IOException("크기 설정 형식이 올바르지 않습니다.")
    };
    internal void Save(int percent)
    {
        if (percent is < 100 or > 150) throw new ArgumentOutOfRangeException(nameof(percent));
        if (Percent == percent) return;
        write(percent);
        if (Percent != percent) throw new IOException("크기 설정이 저장되지 않았습니다.");
    }
    private static object? Read()
    {
        using var key = Registry.CurrentUser.OpenSubKey(BehaviorPreferences.KeyPath);
        return key?.GetValue(ValueName);
    }
    private static void Write(int percent)
    {
        using var key = Registry.CurrentUser.CreateSubKey(BehaviorPreferences.KeyPath, true)
            ?? throw new IOException("크기 설정을 열 수 없습니다.");
        key.SetValue(ValueName, percent, RegistryValueKind.DWord);
    }
}
