using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype {
 public sealed partial class NakiPrototype {
  // Optional noninteractive packaged-player check. Normal launches never enter this path.
  private void CheckPackagedPlayer(){
   if(Application.isEditor||!Application.isBatchMode)return;var args=Environment.GetCommandLineArgs();for(int i=0;i+1<args.Length;i++)if(args[i]=="-nakiSmokeReport"){StartCoroutine(CheckPackagedPlayerRoutine(Path.GetFullPath(args[i+1])));return;}
  }
  private IEnumerator CheckPackagedPlayerRoutine(string report){
   yield return null;yield return null;enabled=false;string failure=null;var observations=new List<string>();
   try{
    if(world==null||journeyRooms.Count==0||CurrentIsland.ordinal!=1||origin!=Origin.Unchosen)throw new InvalidOperationException("Player must start at first island origin selection");
    var shader=Shader.Find("Sprites/Default");if(shader==null||!shader.isSupported)throw new InvalidOperationException("Sprite shader missing from player");
    observations.Add("PASS: packaged entry starts fresh at first island; sprite shader supported");
    StopOpeningCinematic();storyLines=null;queuedOriginChapter=0;ChooseOrigin(0);ChooseProfession(1);storyLines=null;paused=true;
    if(!HandleReplayShortcut(true,false)||replayMenu!=1)throw new InvalidOperationException("Packaged pause replay menu unavailable");
    observations.Add("PASS: standalone pause replay menu opens");
    // Render the actual player camera offscreen as well; hidden batch windows may not present a swapchain.
    view.rect=new Rect(0,0,1,1);view.transform.position=new Vector3(0,0,-10);view.orthographicSize=6.8f;
    var target=RenderTexture.GetTemporary(1600,900,24);var previous=RenderTexture.active;view.targetTexture=target;view.Render();RenderTexture.active=target;
    var frame=new Texture2D(1600,900,TextureFormat.RGB24,false);frame.ReadPixels(new Rect(0,0,1600,900),0,0);frame.Apply();File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(report),"player-world.png"),frame.EncodeToPNG());Destroy(frame);view.targetTexture=null;RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);
    enabled=true;
   }catch(Exception e){failure=e.ToString();}
   yield return null;yield return null;
   if(failure==null)ScreenCapture.CaptureScreenshot(Path.ChangeExtension(report,".png"));
   yield return null;yield return null;yield return null;
   enabled=false;
   if(failure==null)try{
    replayMenu=0;paused=false;
    for(int island=0;island<7;island++){
     SetAtlasIsland(island);if(island==0)BeginJourney(false);else BeginThunderChapter(false);StopOpeningCinematic();storyLines=null;capitalCg=-1;chapterCg=false;chapterSetup=false;manorIntro=false;
     for(int floor=0;floor<3;floor++){
      EnterExplorationRoom(stageEnds[floor],-1);Enemy boss=null;foreach(var candidate in enemies)if(candidate.boss){boss=candidate;break;}
      if(boss==null||boss.maximum<=0||boss.body==null||boss.body.sprite==null)throw new InvalidOperationException("Packaged boss/assets missing: "+island+"/"+floor);
      observations.Add("PASS: island "+(island+1)+" boss "+(floor+1)+" "+boss.kind+" HP="+boss.maximum);
     }
    }
   }catch(Exception e){failure=e.ToString();}
   Directory.CreateDirectory(Path.GetDirectoryName(report));File.WriteAllText(report,(failure==null?"PASS\n":"FAIL\n"+failure+"\n")+string.Join("\n",observations));Application.Quit(failure==null?0:1);
  }
 }
}
