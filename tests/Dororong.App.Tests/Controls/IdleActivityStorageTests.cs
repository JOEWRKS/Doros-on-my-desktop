using System.Collections;
using System.Reflection;
using System.Windows;
using Dororong.App.Controls;
using Dororong.App.Interaction;
using Dororong.Core.Behavior;

namespace Dororong.App.Tests.Controls;

public sealed class IdleActivityStorageTests
{
    [Fact]
    public void Repeated_motions_and_sizes_reuse_bounded_frame_and_body_storage()=>CheekProductTests.Sta(()=>{
        var p=new DororongPresenter();p.SetArtSize(150);
        var pet=new PetSnapshot(PetState.Idle,default,FacingDirection.Right,0,false,null);
        static object Field(object target,string name)=>target.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(target)!;
        HashSet<object>? warmFrames=null,warmBodies=null;
        for(var cycle=0;cycle<8;cycle++){
            foreach(var scale in new[]{150,100,125}){
                p.SetArtSize(scale);
                foreach(var kind in new[]{IdleActivityKind.Stretch,IdleActivityKind.Beg,IdleActivityKind.Groom}){
                    var duration=new IdleActivity(kind,0).Duration;
                    for(var i=0;i<=28;i++){
                        p.Render(pet with{IdleActivity=new(kind,duration*i/28)},DirectInteractionSnapshot.None,TimeSpan.Zero);
                        var active=(IDictionary)Field(p,"_hrFrames");var spare=(IEnumerable)Field(p,"_hrSpareFrames");
                        var frames=active.Values.Cast<object>().Concat(spare.Cast<object>()).ToArray();
                        Assert.InRange(frames.Length,1,12);
                        Assert.Equal(frames.Length,frames.Distinct(ReferenceEqualityComparer.Instance).Count());
                        if(warmFrames is not null)Assert.All(frames,f=>Assert.Contains(f,warmFrames));
                        var raster=Field(p,"_hrRaster");
                        var motion=raster.GetType().GetField("locomotion",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(raster);
                        if(motion is not null){
                            var bodies=(IDictionary)Field(motion,"cache");Assert.InRange(bodies.Count,0,64);
                            if(warmBodies is not null)Assert.All(bodies.Values.Cast<object>(),b=>Assert.Contains(b,warmBodies));
                        }
                    }
                }
            }
            // Full cycle visits >64 keys across three sizes; all reusable body
            // slots and the12 frame slots are warm before identity assertions.
            if(cycle==1){
                var active=(IDictionary)Field(p,"_hrFrames");var spare=(IEnumerable)Field(p,"_hrSpareFrames");
                warmFrames=new(active.Values.Cast<object>().Concat(spare.Cast<object>()),ReferenceEqualityComparer.Instance);
                warmBodies=new(((IDictionary)Field(Field(Field(p,"_hrRaster"),"locomotion"),"cache")).Values.Cast<object>(),ReferenceEqualityComparer.Instance);
            }
        }
    });
}
