using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;
using Dororong.App.Interaction;
using Dororong.Core.Behavior;

internal static class Program
{
    // Compiles against either accepted or candidate binaries. Existing motion
    // API only: independent processes cannot accidentally load the other build.
    [STAThread] static void Main(string[] args){
        var rows=new List<object>();
        foreach(var size in new[]{100,125,150})foreach(var facing in new[]{FacingDirection.Left,FacingDirection.Right}){
            var p=new DororongPresenter();p.SetArtSize(size);
            void Capture(string name,PetSnapshot pet){
                p.Render(pet,DirectInteractionSnapshot.None,TimeSpan.Zero);
                p.Measure(new(p.Width,p.Height));p.Arrange(new(0,0,p.Width,p.Height));p.UpdateLayout();
                var image=new RenderTargetBitmap((int)p.Width,(int)p.Height,96,96,PixelFormats.Pbgra32);image.Render(p);
                var pixels=new byte[image.PixelWidth*image.PixelHeight*4];image.CopyPixels(pixels,image.PixelWidth*4,0);
                rows.Add(new{size,facing=facing.ToString(),name,hash=Convert.ToHexString(SHA256.HashData(pixels)),sole=p.MeasurePlatformContact()?.SoleY});
            }
            var pet=new PetSnapshot(PetState.Idle,default,facing,0,false,null);
            Capture("idle",pet);
            foreach(var phase in new[]{0d,.25,.5,.75,1})Capture($"question-{phase}",pet with{DoroQuestionPhase=phase});
            foreach(var phase in new[]{SnackPhase.Angry,SnackPhase.Eating,SnackPhase.Happy,SnackPhase.Chuckling})
            foreach(var age in new[]{0d,.08,.19,.4,.65,1.1,1.5}){
                var max=phase==SnackPhase.Angry?5:1;
                for(var streak=1;streak<=max;streak++){
                    p.SetSnack(new(phase,facing,0,0,age,0){WithdrawalStreak=streak},PointerSample.Unavailable);
                    Capture($"{phase}-{streak}-{age}",pet);
                }
            }
            p.SetSnack(default,PointerSample.Unavailable);
            foreach(var phase in new[]{0d,.1,.2,.6})Capture($"sleep-{phase}",pet with{State=PetState.Sleep,Phase=phase});
        }
        var path=Path.GetFullPath(args[0]);Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path,JsonSerializer.Serialize(rows,new JsonSerializerOptions{WriteIndented=true}));
        Console.WriteLine($"Rendered {rows.Count} existing-motion samples: {path}");
    }
}
