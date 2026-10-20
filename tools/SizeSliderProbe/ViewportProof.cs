using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;
using Dororong.App.Interaction;
using Dororong.App.Runtime;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;

internal static class ViewportProof
{
    internal static void Run(string output)
    {
        Directory.CreateDirectory(output);
        var rows=new List<object>();
        foreach(var size in new[]{100,110,125,140,150})
        foreach(var facing in new[]{FacingDirection.Left,FacingDirection.Right})
        foreach(var angle in new[]{-88d,-45d,0d,45d,88d})
        {
            var p=new DororongPresenter();p.SetArtSize(size);
            var root=new Grid();root.Children.Add(p);
            var window=new Window{Content=root,WindowStyle=WindowStyle.None,AllowsTransparency=true,
                Background=Brushes.Transparent,ShowInTaskbar=false,ShowActivated=false};
            try {
                var viewport=new PetWindowViewport(window,p);window.Show();
                viewport.SetPosition(new(400,400));
                var pet=new PetSnapshot(PetState.Idle,new(400,400),facing,0,false,null);
                p.Render(pet,DirectInteractionSnapshot.None,TimeSpan.Zero);window.UpdateLayout();
                var press=new PointD(461.5,452.25);
                var direct=new DirectInteractionSnapshot(DirectInteractionTarget.Body,DirectInteractionPhase.BodyDragHold,
                    press,press,1,0,true){HeadSwingDegrees=angle,PressFacing=facing};
                p.Render(pet,direct,TimeSpan.Zero);window.UpdateLayout();
                if(root.ActualWidth<330 || p.HighResolutionImage is null)
                    throw new InvalidOperationException($"Live viewport did not finish layout: {size}/{facing}/{angle}: {root.ActualWidth}.");
                var bitmap=(BitmapSource)p.HighResolutionImage!.Source;
                var bytes=new byte[bitmap.PixelWidth*bitmap.PixelHeight*4];
                bitmap.CopyPixels(bytes,bitmap.PixelWidth*4,0);
                var origin=p.TranslatePoint(new Point(0,0),window);
                var geometry=p.MeasurePlatformGeometry();
                rows.Add(new{size,facing=facing.ToString(),angle,originX=origin.X,originY=origin.Y,
                    logicalWidth=root.ActualWidth,logicalHeight=root.ActualHeight,
                    frameHash=Convert.ToHexString(SHA256.HashData(bytes)),
                    diagnostic=p.HighResolutionDiagnostic,
                    bounds=geometry?.Bounds.ToString()});
                if(facing==FacingDirection.Right) {
                    using var stream=File.Create(Path.Combine(output,$"{size}-{angle}.png"));
                    var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));png.Save(stream);
                }
            } finally {window.Close();}
        }
        File.WriteAllText(Path.Combine(output,"viewport.json"),JsonSerializer.Serialize(rows,new JsonSerializerOptions{WriteIndented=true}));
    }
}
