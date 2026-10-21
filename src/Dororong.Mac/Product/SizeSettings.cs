using System.Text.Json;
namespace Dororong.Mac.Product;
// Separate Mac-only setting; never reads or rewrites the Windows profile.
internal sealed class SizeSettings(string path) {
 internal int Read(){if(!File.Exists(path))return 100;if(new FileInfo(path).Length>1024)throw new IOException("Invalid size settings.");using var doc=JsonDocument.Parse(File.ReadAllText(path));var p=doc.RootElement.GetProperty("percent").GetInt32();if(p is <100 or >150)throw new IOException("Invalid size.");return p;}
 internal void Save(int percent){if(percent is <100 or >150)throw new ArgumentOutOfRangeException(nameof(percent));var full=Path.GetFullPath(path);Directory.CreateDirectory(Path.GetDirectoryName(full)!);var temp=full+"."+Guid.NewGuid().ToString("N")+".tmp";try{using(var f=new FileStream(temp,FileMode.CreateNew,FileAccess.Write,FileShare.None)){JsonSerializer.Serialize(f,new{version=1,percent});f.Flush(true);}File.Move(temp,full,true);}finally{if(File.Exists(temp))File.Delete(temp);}}
}
