using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using Dororong.App.Controls;
using Dororong.Core.Behavior;
internal static class Program {
 [STAThread] static void Main(string[] args){
  #if !BASELINE
  if(args.Length==2&&args[1]=="memory"){MemoryRun.Run(args[0]);return;}
#endif
  var output=Path.GetFullPath(args[0]);Directory.CreateDirectory(output);
  var r=new HighResolutionGrabRaster();var presenter=new DororongPresenter();
  var flags=BindingFlags.NonPublic|BindingFlags.Instance;
  typeof(DororongPresenter).GetField("_hrRaster",flags)!.SetValue(presenter,r);
  var apply=typeof(DororongPresenter).GetMethod("ApplyIdleActivity",flags)!;
  var faceField=typeof(DororongPresenter).GetField("_hrFace",flags)!;
  var rows=new List<object>();byte[]? bytes=null;
  foreach(var scale in new[]{1d,1.25,1.5})foreach(var left in new[]{false,true}){
   var inverse=Matrix.Identity;if(left)inverse.ScaleAt(-1,1,48,48);
   void Frame(string name,HighResolutionFacePose face,double density=1){
    var extent=(int)Math.Ceiling(256*scale*density);var length=extent*extent*4;
#if BASELINE
    bytes=r.RenderRetainedPixels(0,inverse,default,default,default,0,scale*density,face,scale,bytes,true);
#else
    bytes=r.RenderOwnedPixels(0,inverse,default,default,default,0,scale*density,face,scale,bytes,true);
#endif
    var id=$"{name}-{scale}-{left}-{density}";
    rows.Add(new{id,hash=Convert.ToHexString(SHA256.HashData(bytes.AsSpan(0,length)))});
    if(name.EndsWith("-60")||name=="Groom-110"||name=="Tap-30")File.WriteAllBytes(Path.Combine(output,id+".bgra"),bytes.AsSpan(0,length).ToArray());
   }
   var head=Matrix.Identity;var offset=r.MotionOffset(321);head.Translate(offset.X,offset.Y);
   var seated=HighResolutionFacePose.Neutral with{Head=head,MotionFrame=321};
   foreach(var kind in new[]{SeatedReaction.Tap,SeatedReaction.Shake}){
    var count=kind==SeatedReaction.Tap?63:57;var aim=new SeatedInteraction();
    for(var i=0;i<=count;i++){
     aim.Advance(1/60d,true,false,new Point(0,73+35*Math.Sin(i/63d*Math.PI*2-Math.PI/2)));
     var f=SeatedInteraction.Pose(seated,kind,i/60d,aim.AimAngle);
     Frame($"{kind}-{i}",f);if(i%15==0)Frame($"{kind}-{i}",f,2);
    }
   }
   foreach(var kind in new[]{IdleActivityKind.Groom,IdleActivityKind.Stretch,IdleActivityKind.Beg}){
    var duration=new IdleActivity(kind,0).Duration;
    for(var i=0;i<=(int)Math.Round(duration*60);i++){
     faceField.SetValue(presenter,HighResolutionFacePose.Neutral);
     var pet=new PetSnapshot(PetState.Idle,default,FacingDirection.Right,0,false,null){IdleActivity=new(kind,i/60d)};
     apply.Invoke(presenter,new object[]{pet});var f=(HighResolutionFacePose)faceField.GetValue(presenter)!;
     Frame($"{kind}-{i}",f);if(i%30==0)Frame($"{kind}-{i}",f,2);
    }
   }
   for(var i=0;i<=60;i++){
    var h=Matrix.Identity;h.RotateAt(DoroQuestionMotion.HeadRoll(i/60d)*180/Math.PI,43,66);
    Frame($"Question-{i}",HighResolutionFacePose.Neutral with{Head=h});
   }
   foreach(var i in new[]{0,5,11,24,39,60,90,120,160}){
    for(var streak=1;streak<=5;streak++)Frame($"Angry{streak}-{i}",HighResolutionGrabRaster.SnackReaction(i/60d,streak));
    Frame($"Chuckle-{i}",HighResolutionGrabRaster.SnackChuckle(i/60d));
   }
   Console.WriteLine($"scale={scale},left={left},frames={rows.Count}");
  }
  File.WriteAllText(Path.Combine(output,"frames.json"),JsonSerializer.Serialize(rows,new JsonSerializerOptions{WriteIndented=true}));
#if BASELINE
  var ink=Brushes.Black;var text=new FormattedText("DORO?",System.Globalization.CultureInfo.InvariantCulture,FlowDirection.LeftToRight,new Typeface(new FontFamily("Segoe UI"),FontStyles.Normal,FontWeights.SemiBold,FontStretches.Normal),13,ink,1);
  File.WriteAllText(Path.Combine(output,"question-glyph.path"),text.BuildGeometry(new((64-text.Width)/2,(27-text.Height)/2)).GetFlattenedPathGeometry(.001,ToleranceType.Absolute).ToString(System.Globalization.CultureInfo.InvariantCulture));
#endif
 }
}
