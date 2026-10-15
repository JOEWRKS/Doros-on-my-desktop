using System.IO;
using Microsoft.Win32;

namespace Dororong.App.Product;

/// <summary>User preference, retained across updates/removal like other user data.</summary>
internal sealed class BehaviorPreferences
{
    internal const string KeyPath = @"Software\JOEWRKS\Dororong\Preferences";
    internal const string ValueName = "AutomaticSleepEnabled";
    private readonly Func<object?> read;
    private readonly Action<int> write;

    internal BehaviorPreferences() : this(Read, Write) { }
    internal BehaviorPreferences(Func<object?> read, Action<int> write)
    {
        this.read = read;
        this.write = write;
    }

    internal bool AutomaticSleepEnabled => read() switch
    {
        null => true,
        int value when value is 0 or 1 => value == 1,
        _ => throw new IOException("자동 눕기 설정 형식이 올바르지 않습니다.")
    };

    internal void SetAutomaticSleepEnabled(bool enabled)
    {
        // Refuse to silently overwrite an unreadable/foreign value.
        _ = AutomaticSleepEnabled;
        write(enabled ? 1 : 0);
        if (AutomaticSleepEnabled != enabled)
            throw new IOException("자동 눕기 설정이 저장되지 않았습니다.");
    }

    private static object? Read()
    {
        using var key = Registry.CurrentUser.OpenSubKey(KeyPath, false);
        return key?.GetValue(ValueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
    }

    private static void Write(int value)
    {
        using var key = Registry.CurrentUser.CreateSubKey(KeyPath, true)
            ?? throw new IOException("자동 눕기 설정을 열 수 없습니다.");
        key.SetValue(ValueName, value, RegistryValueKind.DWord);
    }
}
