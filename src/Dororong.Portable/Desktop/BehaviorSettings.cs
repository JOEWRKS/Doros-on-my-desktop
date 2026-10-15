using System.Text.Json;
using System.Text.Json.Nodes;

namespace Dororong.Portable.Desktop;

// Only current-user application data; no Windows registry or startup changes.
public sealed class BehaviorSettings(string path)
{
    private JsonObject Read()
    {
        if(!File.Exists(path))return new JsonObject { ["version"]=1,["automaticSleepEnabled"]=true };
        if(new FileInfo(path).Length>65536)throw new IOException("설정 파일이 너무 큽니다.");
        try
        {
            var root=JsonNode.Parse(File.ReadAllText(path)) as JsonObject??throw new IOException("설정 형식 오류");
            if(root["version"]?.GetValue<int>()!=1||root["automaticSleepEnabled"] is not JsonValue value||!value.TryGetValue<bool>(out _))
                throw new IOException("지원하지 않는 설정 형식입니다. 기존 파일을 보존했습니다.");
            return root;
        }
        catch(Exception e) when(e is JsonException or InvalidOperationException or FormatException)
        {throw new IOException("설정을 읽지 못했습니다. 기존 파일을 보존했습니다.",e);}
    }
    public bool AutomaticSleepEnabled=>Read()["automaticSleepEnabled"]!.GetValue<bool>();
    public void SetAutomaticSleepEnabled(bool enabled)
    {
        var root=Read();root["automaticSleepEnabled"]=enabled;
        var full=Path.GetFullPath(path);Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        var temp=full+"."+Guid.NewGuid().ToString("N")+".tmp";
        try
        {
            using(var stream=new FileStream(temp,FileMode.CreateNew,FileAccess.Write,FileShare.None))
            {JsonSerializer.Serialize(stream,root);stream.Flush(true);}
            File.Move(temp,full,true);
            if(AutomaticSleepEnabled!=enabled)throw new IOException("설정 저장을 확인하지 못했습니다.");
        }
        finally{if(File.Exists(temp))File.Delete(temp);}
    }
}
