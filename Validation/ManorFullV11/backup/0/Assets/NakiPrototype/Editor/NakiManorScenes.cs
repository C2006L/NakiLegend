using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace NakiLegend.Prototype.Editor
{
 public static class NakiManorScenes
 {
  public const string Adventure="Assets/Scenes/NakiManorCourtyard.unity",Practice="Assets/Scenes/NakiManorCaptainPractice.unity";
  [MenuItem("Tools/Naki Legend/Play/6 Manor Courtyard Adventure")]
  public static void OpenAdventure(){Open(false);}
  [MenuItem("Tools/Naki Legend/Boss Practice/6 Oath Captain")]
  public static void OpenCaptain(){Open(true);}
  private static void Open(bool practice)
  {
   if(EditorApplication.isPlaying){Debug.Log("先停止 Play，再切换府邸试玩场景；当前进度会保留在安全存档。");return;}
   if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
   string path=practice?Practice:Adventure;
   if(!System.IO.File.Exists(path))BuildScenes();
   EditorSceneManager.OpenScene(path);
  }
  public static void BuildScenes()
  {
   foreach(bool practice in new[]{false,true}){
    var scene=EditorSceneManager.OpenScene("Assets/Scenes/NakiTest.unity");var p=Object.FindObjectOfType<NakiPrototype>();
    p.name=practice?"Oath Captain Practice":"Manor Oath Courtyard";p.manorCourtyardMode=p.thunderChapterMode=true;
    p.fireChapterMode=p.iceSampleMode=p.woodlandMode=p.thunderPrototypeMode=p.gatehouseSampleMode=p.bossPracticeMode=p.lightningTrialMode=false;
    p.showOpeningOnStart=false;p.thunderChapterStartFloor=0;p.thunderChapterPracticeBoss=practice?0:-1;p.journeySeed=61001;
    EditorUtility.SetDirty(p);EditorSceneManager.MarkSceneDirty(scene);
    if(!EditorSceneManager.SaveScene(scene,practice?Practice:Adventure))throw new System.InvalidOperationException("Cannot save manor scene");
   }
  }
 }
}
