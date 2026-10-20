using System.IO;
using Microsoft.Win32;

namespace Dororong.App.Product;

internal sealed class StartupRegistration
{
    internal const string KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    internal const string ValueName = "JOEWRKS.Dororong";
    private readonly Func<string?> read;
    private readonly Action<string?> write;
    internal string Command { get; }
    private readonly string legacyCommand;
    private bool Owned(string? value)=>string.Equals(value,Command,StringComparison.OrdinalIgnoreCase)||
        string.Equals(value,legacyCommand,StringComparison.OrdinalIgnoreCase);
    internal bool Enabled => Owned(read());

    internal StartupRegistration() : this(Environment.ProcessPath ?? throw new InvalidOperationException("실행 파일 경로를 찾을 수 없습니다."), Read, Write) { }
    internal StartupRegistration(string executable, Func<string?> read, Action<string?> write)
    {
        if (!Path.IsPathFullyQualified(executable) || !executable.EndsWith(".exe",StringComparison.OrdinalIgnoreCase) ||
            executable.IndexOfAny(['"','\r','\n']) >= 0)
            throw new ArgumentException("자동 실행에는 올바른 실행 파일 경로가 필요합니다.",nameof(executable));
        legacyCommand = $"\"{executable}\"";
        Command = legacyCommand+" "+LoginStartupDelay.Argument;
        this.read = read; this.write = write;
    }
    internal void SetEnabled(bool enabled)
    {
        var current=read();
        if (!string.IsNullOrEmpty(current) && !Owned(current))
            throw new InvalidOperationException("다른 경로의 도로롱 자동 실행이 등록되어 있습니다. 기존 등록을 해제한 뒤 다시 시도해 주세요.");
        write(enabled ? Command : null);
        if(enabled ? !string.Equals(read(),Command,StringComparison.OrdinalIgnoreCase) : Enabled)
            throw new IOException("자동 실행 설정이 저장되지 않았습니다.");
    }
    internal void UpgradeOwnedLegacy()
    {
        // Never opt in an OFF user or rewrite another executable/unknown args.
        if(string.Equals(read(),legacyCommand,StringComparison.OrdinalIgnoreCase))
        {
            write(Command);
            if(!string.Equals(read(),Command,StringComparison.OrdinalIgnoreCase))
                throw new IOException("자동 실행 지연 설정이 저장되지 않았습니다.");
        }
    }
    private static string? Read()
    {
        using var key=Registry.CurrentUser.OpenSubKey(KeyPath,false);
        var value=key?.GetValue(ValueName,null,RegistryValueOptions.DoNotExpandEnvironmentNames);
        if(value is not null && value is not string)throw new IOException("기존 자동 실행 항목 형식이 올바르지 않습니다. 임의로 덮어쓰지 않았습니다.");
        return value as string;
    }
    private static void Write(string? command)
    {
        using var key=Registry.CurrentUser.CreateSubKey(KeyPath,true)
            ?? throw new IOException("자동 실행 설정을 열 수 없습니다.");
        if(command is null)key.DeleteValue(ValueName,false);
        else key.SetValue(ValueName,command,RegistryValueKind.String);
    }
}
