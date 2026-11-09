using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace NakiLegend.Prototype.Editor {
 public static class NakiFullJourneyTest {
  public const string Scene="Assets/Scenes/NakiFullJourneyTest.unity";
  [MenuItem("Tools/Naki Legend/Play/0 Full Journey - New Test")]
  public static void NewTest(){
   if(EditorApplication.isPlaying){Debug.Log("先停止 Play，再开启完整旅程测试。");return;}
   if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
   var scene=EditorSceneManager.OpenScene("Assets/Scenes/NakiTest.unity");var p=UnityEngine.Object.FindObjectOfType<NakiPrototype>();
   p.journeyTestProfile=DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")+"-"+Guid.NewGuid().ToString("N").Substring(0,8);
   p.enableDesertJourney=true;p.thunderChapterMode=p.capitalMode=p.manorCourtyardMode=p.woodlandMode=p.fireChapterMode=p.iceSampleMode=p.thunderPrototypeMode=p.bossPracticeMode=p.gatehouseSampleMode=p.lightningTrialMode=false;
   p.thunderChapterPracticeBoss=-1;p.thunderChapterStartFloor=0;p.showOpeningOnStart=true;p.journeySeed=20261108;
   EditorUtility.SetDirty(p);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene,Scene);Debug.Log("完整旅程测试已就绪：点击 Play。从开场开始，测试存档独立保存；中途退出用 0 Full Journey - Continue Test 继续。");
  }
  [MenuItem("Tools/Naki Legend/Play/0 Full Journey - Continue Test")]
  public static void ContinueTest(){if(EditorApplication.isPlaying)return;if(!File.Exists(Scene)){Debug.Log("请先选择 0 Full Journey - New Test。");return;}if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())EditorSceneManager.OpenScene(Scene);}
 }
}
