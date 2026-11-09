using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace NakiLegend.Prototype.Editor
{
 public static class NakiCapitalScenes
 {
  public const string Adventure="Assets/Scenes/NakiRoyalCapital.unity";
  [MenuItem("Tools/Naki Legend/Play/7 Royal Capital Adventure")] public static void OpenAdventure(){Open(-1);}
  [MenuItem("Tools/Naki Legend/Boss Practice/7 Harbour Commander")] public static void OpenCommander(){Open(0);}
  [MenuItem("Tools/Naki Legend/Boss Practice/7B Royal Magistrate")] public static void OpenMagistrate(){Open(1);}
  [MenuItem("Tools/Naki Legend/Boss Practice/7C The King")] public static void OpenKing(){Open(2);}
  private static string ScenePath(int boss){return boss<0?Adventure:"Assets/Scenes/NakiCapitalBoss"+boss+"Practice.unity";}
  private static void Open(int boss)
  {
   if(EditorApplication.isPlaying){Debug.Log("先停止 Play，再打开王都场景；首领试炼不覆盖正式存档。");return;}
   if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
   if(!System.IO.File.Exists(ScenePath(boss)))BuildScenes();EditorSceneManager.OpenScene(ScenePath(boss));
  }
  public static void BuildScenes()
  {
   string previous=EditorSceneManager.GetActiveScene().path;
   for(int boss=-1;boss<3;boss++){
    var scene=EditorSceneManager.OpenScene("Assets/Scenes/NakiTest.unity");var p=Object.FindObjectOfType<NakiPrototype>();
    p.name=boss<0?"Royal Capital Adventure":"Royal Capital Practice "+boss;p.capitalMode=p.thunderChapterMode=true;
    p.manorCourtyardMode=p.fireChapterMode=p.iceSampleMode=p.woodlandMode=p.thunderPrototypeMode=p.gatehouseSampleMode=p.bossPracticeMode=p.lightningTrialMode=false;
    p.enableDesertJourney=true;p.showOpeningOnStart=false;p.thunderChapterPracticeBoss=boss;p.thunderChapterStartFloor=0;p.journeySeed=71001;
    EditorUtility.SetDirty(p);EditorSceneManager.MarkSceneDirty(scene);
    if(!EditorSceneManager.SaveScene(scene,ScenePath(boss)))throw new System.InvalidOperationException("Cannot save capital scene");
   }
   if(!string.IsNullOrEmpty(previous)&&System.IO.File.Exists(previous))EditorSceneManager.OpenScene(previous);
  }
 }
}
