using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Raster;
internal static class Program {
 const BindingFlags B=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
 static object? Get(object o,string n)=>o.GetType().GetProperty(n,B)?.GetValue(o)??o.GetType().GetField(n,B)?.GetValue(o);
 static object? Call(object o,string n,params object?[] args){var methods=o.GetType().GetMethods(B).Where(m=>m.Name==n&&m.GetParameters().Length==args.Length).ToArray();return methods.Single().Invoke(o,args);}
 static object None(Assembly a)=>a.GetType("Dororong.App.Interaction.DirectInteractionSnapshot")!.GetProperty("None",B)!.GetValue(null)!;
 [STAThread] static int Main(string[] args){
  if(args.Length!=2)throw new ArgumentException("Accepted DLL and fresh output JSON required.");
  if(Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(args[0])) )!="374666338E0AA082F84B49B8EAD1E4BF0397F0CD08658BC24F336F78F882B322")throw new InvalidOperationException("Not accepted.");
  _=new Application{ShutdownMode=ShutdownMode.OnExplicitShutdown};
  var reference=Assembly.LoadFrom(Path.GetFullPath(args[0]));var portable=typeof(R104Raster).Assembly;var rows=new List<object>();var mismatches=0;
  foreach(var percent in new[]{100,125,150}){
   var a=Activator.CreateInstance(reference.GetType("Dororong.App.Controls.DororongPresenter")!)!;
   var b=Activator.CreateInstance(portable.GetType("Dororong.App.Controls.DororongPresenter")!,true)!;
   Call(a,"SetArtSize",percent);Call(b,"SetArtSize",percent);
   var element=(FrameworkElement)a;var grid=new Grid{Width=144*percent/100d+192,Height=144*percent/100d+192};
   grid.Children.Add(element);grid.Measure(new Size(grid.Width,grid.Height));grid.Arrange(new Rect(0,0,grid.Width,grid.Height));grid.UpdateLayout();
   var pet=new PetBrain(BehaviorTuning.Default,new R(),new(100,100)).Current;
   var da=None(reference);var db=None(portable);
   void Frame(string name,PetSnapshot p,TimeSpan dt,bool tracking=false){
    if(tracking){Call(a,"UpdateHuntingWithPounce",p,da,new PointerSample(true,new(330,220)),dt,false);Call(b,"UpdateHuntingWithPounce",p,db,new PointerSample(true,new(330,220)),dt,false);}
    Call(a,"RenderDesktop",p,da,dt);Call(b,"RenderDesktop",p,db,dt);
    var fa=Get(a,"_hrFace")!;var fb=Get(b,"_hrFace")!;var errors=new List<string>();
    foreach(var property in fa.GetType().GetProperties(B).Where(p=>p.GetIndexParameters().Length==0&&p.Name!="Neutral")){
     var va=property.GetValue(fa);var vb=Get(fb,property.Name);
     if(va is double d&&vb is double e){if(Math.Abs(d-e)>1e-8)errors.Add(property.Name);}
     else if(property.Name=="Head"){foreach(var n in new[]{"M11","M12","M21","M22","OffsetX","OffsetY"})if(Math.Abs(Convert.ToDouble(Get(va!,n))-Convert.ToDouble(Get(vb!,n)))>1e-8)errors.Add("Head."+n);}
     else if(!Equals(va,vb))errors.Add(property.Name);
    }
    foreach(var n in new[]{"_hrProgress","_hrAngle"})if(Math.Abs(Convert.ToDouble(Get(a,n))-Convert.ToDouble(Get(b,n)))>1e-8)errors.Add(n);
    if(errors.Count>0)mismatches++;rows.Add(new{percent,name,errors});
   }
   Frame("neutral",pet,TimeSpan.Zero);
   for(var i=0;i<16;i++)Frame("tracking-"+i,pet,TimeSpan.FromMilliseconds(16),true);
   Call(a,"SetSittingRequested",true);Call(b,"SetSittingRequested",true);
   for(var i=0;i<12;i++)Frame("sitting-"+i,pet,TimeSpan.FromMilliseconds(100),true);
   Call(a,"SetSittingRequested",false);Call(b,"SetSittingRequested",false);
   Frame("sleep-entry",pet with {State=PetState.Sleep,Phase=.1},TimeSpan.FromMilliseconds(16));
   Frame("sleep-closed",pet with {State=PetState.Sleep,Phase=.5},TimeSpan.FromMilliseconds(16));
   Frame("sleep-wrap",pet with {State=PetState.Sleep,Phase=.001},TimeSpan.FromMilliseconds(16));
   for(var i=0;i<15;i++)Frame("wake-"+i,pet,TimeSpan.FromMilliseconds(16));
   foreach(var phase in new[]{SnackPhase.Chasing,SnackPhase.Eating,SnackPhase.Happy,SnackPhase.Settling}){
    for(var i=0;i<8;i++){var snack=new SnackPose(phase,FacingDirection.Right,0,0,i*.06,i*5){RunAmount=.75};Call(a,"SetSnack",snack,PointerSample.Unavailable);Call(b,"SetSnack",snack,PointerSample.Unavailable);Frame(phase+"-"+i,pet,TimeSpan.FromMilliseconds(60));}
   }
  }
  Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(args[1]))!);using var output=new FileStream(args[1],FileMode.CreateNew);JsonSerializer.Serialize(output,new{samples=rows.Count,mismatches,nativeMacExecution="NOT_TESTED",rows},new JsonSerializerOptions{WriteIndented=true});
  Console.WriteLine($"Pose sequence samples={rows.Count}, mismatches={mismatches}");return mismatches==0?0:2;
 }
 sealed class R:IRandomSource{public double NextUnit()=>.5;}
}
