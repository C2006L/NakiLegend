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
  [MenuItem("Tools/Naki Legend/Boss Practice/6B Royal Seal")]
  public static void OpenSeal(){OpenLater("Assets/Scenes/NakiManorSealPractice.unity");}
  [MenuItem("Tools/Naki Legend/Boss Practice/6C Dark Great Knight")]
  public static void OpenKnight(){OpenLater("Assets/Scenes/NakiManorKnightPractice.unity");}
  private static void OpenLater(string path){if(EditorApplication.isPlaying){Debug.Log("先停止 Play 再打开练习；练习不覆盖旅程存档。");return;}if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;if(!System.IO.File.Exists(path))BuildScenes();EditorSceneManager.OpenScene(path);}
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
   for(int boss=-1;boss<3;boss++){bool practice=boss>=0;
    var scene=EditorSceneManager.OpenScene("Assets/Scenes/NakiTest.unity");var p=Object.FindObjectOfType<NakiPrototype>();
    p.name=!practice?"Manor Full Chapter":boss==0?"Oath Captain Practice":boss==1?"Royal Seal Practice":"Dark Great Knight Practice";p.manorCourtyardMode=p.thunderChapterMode=true;
    p.fireChapterMode=p.iceSampleMode=p.woodlandMode=p.thunderPrototypeMode=p.gatehouseSampleMode=p.bossPracticeMode=p.lightningTrialMode=false;
    p.showOpeningOnStart=false;p.thunderChapterStartFloor=0;p.thunderChapterPracticeBoss=boss;p.journeySeed=61001;
    EditorUtility.SetDirty(p);EditorSceneManager.MarkSceneDirty(scene);
    string target=!practice?Adventure:boss==0?Practice:boss==1?"Assets/Scenes/NakiManorSealPractice.unity":"Assets/Scenes/NakiManorKnightPractice.unity";
    if(!EditorSceneManager.SaveScene(scene,target))throw new System.InvalidOperationException("Cannot save manor scene");
   }
  }
 }
}
