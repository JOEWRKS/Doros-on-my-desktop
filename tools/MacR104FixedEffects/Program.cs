using System.IO;using System.IO.Compression;using System.Reflection;using System.Security.Cryptography;
internal static class Program {
 [STAThread] static int Main(string[] args){
  if(args.Length is <2 or >3)throw new ArgumentException("Accepted DLL, fresh output bank, optional eating mode.");
  var bytes=File.ReadAllBytes(args[0]);
  if(Convert.ToHexString(SHA256.HashData(bytes))!="374666338E0AA082F84B49B8EAD1E4BF0397F0CD08658BC24F336F78F882B322")throw new InvalidOperationException("Not r104.");
  _=new System.Windows.Application{ShutdownMode=System.Windows.ShutdownMode.OnExplicitShutdown};
  var a=Assembly.LoadFrom(Path.GetFullPath(args[0]));var flags=BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Static;
  Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(args[1]))!);
  using var f=new FileStream(args[1],FileMode.CreateNew);using var gz=new GZipStream(f,CompressionLevel.SmallestSize);using var w=new BinaryWriter(gz);
  if(args.Length==3&&args[2]=="eating"){
   w.Write(104);w.Write(103);
   var render=a.GetType("Dororong.App.Controls.HighResolutionSnackEffects",true)!.GetMethod("Render",flags)!;
   for(var frame=0;frame<103;frame++){
    var pixels=(float[])render.Invoke(null,new object[]{frame/60d,-1d})!;
    var left=225;var top=225;var right=-1;var bottom=-1;
    for(var y=0;y<225;y++)for(var x=0;x<225;x++)if(pixels[(y*225+x)*4+3]>0){left=Math.Min(left,x);top=Math.Min(top,y);right=Math.Max(right,x);bottom=Math.Max(bottom,y);}
    if(right<0){w.Write(0);w.Write(0);w.Write(0);w.Write(0);continue;}
    w.Write(left);w.Write(top);w.Write(right-left+1);w.Write(bottom-top+1);
    for(var y=top;y<=bottom;y++)for(var x=left;x<=right;x++)for(var c=0;c<4;c++)w.Write(checked((byte)pixels[(y*225+x)*4+c]));
   }return 0;
  }
  w.Write(104);w.Write(2);
  var method=a.GetType("Dororong.App.Controls.HighResolutionGrabRaster+FlutterLayers",true)!.GetMethod("PawTexture",flags)!;
  foreach(var far in new[]{false,true}){var p=(float[])method.Invoke(null,new object[]{far})!;w.Write(p.Length);foreach(var value in p){if(value!=Math.Round(value)||value<0||value>255)throw new InvalidDataException();w.Write((byte)value);}}
  return 0;
 }
}
