using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;

namespace Dororong.App.Tests.Controls;

public sealed class HighResolutionParityProofTests
{
    [Fact]
    public void Export_source_resolution_hunt_and_cheek_against_product_renderer()=>CheekProductTests.Sta(()=>
    {
        var output=Environment.GetEnvironmentVariable("DORORONG_R54_OUTPUT");
        if(output is null)return;
        Directory.CreateDirectory(output);var rows=new List<object>();
        var high=new HighResolutionGrabRaster();var legacy=new HuntRenderer();
        void Save(BitmapSource b,string file){using var s=File.Create(Path.Combine(output,file));var enc=new PngBitmapEncoder();enc.Frames.Add(BitmapFrame.Create(b));enc.Save(s);}
        var sequence=Environment.GetEnvironmentVariable("DORORONG_R54_SEQUENCE")=="1";
        foreach(var scale in sequence?new[]{1d,1.125,1.25,1.375,1.5}:new[]{1d,1.5}){
            foreach(var frame in sequence&&scale is 1 or 1.5?Enumerable.Range(0,157):new[]{0,18,24,30,42,60,75,87,99,114,123,135,150,156}){
                foreach(var roll in sequence?new[]{0d}:new[]{0d,.24,-.24}){
                    var gaze=new HuntGazePose(0,0,roll);var name=$"hunt-{scale:F2}-{frame:D3}-{roll:F2}";
                    var face=new HighResolutionFacePose(HuntRenderer.HeadTransform(frame,gaze),0,0,0,0,0,HuntPose.At(frame/60d).Amount>0?frame:-1);
                    Save(high.Render(0,Matrix.Identity,new(37.5,28.25),default,default,0,scale,face),name+"-new.png");
                    Save(legacy.Render(frame,gaze),name+"-old.png");rows.Add(new{name,scale,frame,roll,kind="hunt"});
                }
            }
            foreach(var frame in new[]{0,75})foreach(var pull in new[]{0d,2,4,8,12,16,20,-5}){
                var gaze=new HuntGazePose(0,0,0);var name=$"cheek-{scale:F2}-{frame:D3}-{pull:F0}";
                var face=new HighResolutionFacePose(HuntRenderer.HeadTransform(frame,gaze),0,0,pull,Math.Max(0,pull),Math.Max(0,pull),frame==0?-1:frame);
                Save(high.Render(0,Matrix.Identity,new(37.5,28.25),default,default,0,scale,face),name+"-new.png");
                Save(legacy.Render(frame,gaze,cheekPull:pull,eyePull:Math.Max(0,pull),hairPull:Math.Max(0,pull)),name+"-old.png");rows.Add(new{name,scale,frame,pull,kind="cheek"});
            }
        }
        File.WriteAllText(Path.Combine(output,"pairs.json"),JsonSerializer.Serialize(rows));
    });
}
