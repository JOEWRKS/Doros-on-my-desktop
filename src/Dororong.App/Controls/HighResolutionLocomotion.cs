using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Windows;

namespace Dororong.App.Controls;

// Source-resolution parts, never enlarged legacy motion frames.
internal sealed class HighResolutionLocomotion
{
    private readonly byte[] bytes;
    private readonly (int Offset,int Length,Point Head)[] entries=new (int,int,Point)[837];
    private readonly Dictionary<(int Frame,double Scale),float[]> cache=new();
    private readonly Queue<(int Frame,double Scale)> cacheOrder=new();
    private readonly HashSet<(int Frame,double Scale)> reusable=new();
    private float[]? closedHead;
    private float[]? bodyScratch;
    private float[]? effectScratch;
    private int effectFrame=-1;
    internal HighResolutionLocomotion(){
        using var s=typeof(HighResolutionLocomotion).Assembly.GetManifestResourceStream("Dororong.App.Assets.high-resolution-locomotion.bank")!;
        using var m=new MemoryStream();s.CopyTo(m);bytes=m.ToArray();using var r=new BinaryReader(new MemoryStream(bytes));
        if(new string(r.ReadChars(4))!="DLM2"||r.ReadInt32()!=entries.Length)throw new InvalidDataException();
        for(var i=0;i<entries.Length;i++){
            var head=new Point(r.ReadDouble(),r.ReadDouble());var n=r.ReadInt32();
            if(n<=0||r.BaseStream.Position+n>bytes.Length)throw new InvalidDataException();
            entries[i]=((int)r.BaseStream.Position,n,head);r.BaseStream.Position+=n;
        }
    }
    internal Point Head(int frame)=>entries[frame].Head;
    internal float[] Get(int frame){
        if(frame==0)return closedHead??=Read(0);
        if(cache.TryGetValue((frame,1),out var found)){reusable.Remove((frame,1));return found;}
        // A full run cycle has48 keys (walk32). Four entries decompressed and
        // reallocated almost every frame of every lap. Retain a bounded full
        // cycle plus transition room; frames stay immutable while borrowed.
        var result=Read(frame);Store((frame,1),result);return result;
    }
    internal float[] GetBody(int frame,double scale){
        if(scale<=1)return Get(frame);
        if(cache.TryGetValue((frame,scale),out var found)){reusable.Remove((frame,scale));return found;}
        // Retain the ready-to-sample body, not both its raw 810KB source and
        // another 810KB stroke-adjusted copy for every cached motion frame.
        bodyScratch=Read(frame,bodyScratch);
        var result=(float[])HighResolutionBodyInk.AtScale(bodyScratch,225,scale).Clone();
        Store((frame,scale),result);return result;
    }
    // Effects are consumed synchronously alongside a borrowed body. Keep a
    // separate slot: immutable Get(effect) entries in the body FIFO displaced
    // reusable body slots, forcing another 810KB allocation on later misses.
    // General Get callers retain their existing immutable ownership contract.
    internal float[] BorrowEffect(int frame){
        if(effectFrame==frame&&effectScratch is not null)return effectScratch;
        effectScratch=Read(frame,effectScratch);effectFrame=frame;return effectScratch;
    }
    // Render-only loan: valid until the next BorrowBody call. The synchronous
    // raster joins all row workers before another render starts. General Get /
    // GetBody still return immutable retained frames, including when upgrading
    // a frame previously produced through this loan path.
    internal float[] BorrowBody(int frame,double scale){
        if(frame==0)return Get(0);
        var key=(frame,Math.Max(1,scale));
        if(cache.TryGetValue(key,out var found))return found;
        float[]? result=null;
        if(cache.Count>=64){
            var expired=cacheOrder.Dequeue();var old=cache[expired];cache.Remove(expired);
            if(reusable.Remove(expired))result=old;
        }
        if(scale<=1)result=Read(frame,result);
        else{
            bodyScratch=Read(frame,bodyScratch);
            var prepared=HighResolutionBodyInk.AtScale(bodyScratch,225,scale);
            result??=new float[prepared.Length];
            HighResolutionBodyInk.Invalidate(result);
            Array.Copy(prepared,result,prepared.Length);
        }
        Store(key,result,true);return result;
    }
    private void Store((int Frame,double Scale) key,float[] pixels,bool canReuse=false){
        // Dictionary enumeration reuses deleted slots; Keys.First() can evict
        // the newest insertion on every miss instead of the oldest frame.
        if(cache.Count>=64){var expired=cacheOrder.Dequeue();cache.Remove(expired);reusable.Remove(expired);}
        cache[key]=pixels;cacheOrder.Enqueue(key);
        if(canReuse)reusable.Add(key);
    }
    private float[] Read(int frame,float[]? reuse=null){
        var e=entries[frame];using var s=new GZipStream(new MemoryStream(bytes,e.Offset,e.Length),CompressionMode.Decompress);
        // Head/body/effect may be borrowed together by the current render.
        // Do not overwrite an evicted array while another layer still uses it.
        var result=reuse??new float[225*225*4];HighResolutionBodyInk.Invalidate(result);
        s.ReadExactly(MemoryMarshal.AsBytes(result.AsSpan()));return result;
    }
    internal static int Walk(double amount,double distance){
        var level=(int)Math.Round(Math.Clamp(amount,0,1)*8);if(level==0)return -1;
        var phase=(int)Math.Round(((distance%10+10)%10)/10*32)%32;
        return 1+(level-1)*32+phase;
    }
    internal static int Sit(double amount)=>257+(int)Math.Round(Math.Clamp(amount,0,1)*64);
    internal static int Run(double amount,double distance){
        var level=(int)Math.Round(Math.Clamp(amount,0,1)*8);if(level==0)return -1;
        var stride=SnackRunFrames.Stride;
        var phase=(int)Math.Round(((distance%stride+stride)%stride)/stride*48)%48;
        return 322+(level-1)*48+phase;
    }
    internal static int Sleep(double amount)=>706+(int)Math.Round(Math.Clamp(amount,0,1)*64);
    internal static int SleepEffect(int bodyFrame)=>bodyFrame+65;
    internal const int Perch=836;
}
