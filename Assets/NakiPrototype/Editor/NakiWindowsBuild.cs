using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace NakiLegend.Prototype.Editor {
 public static class NakiWindowsBuild {
  public const string Version="0.46.0";
  [MenuItem("Tools/Naki Legend/Build/Windows portable")]
  public static void Build(){
   string destination=Path.GetFullPath("Builds/NakiLegend-Windows-x64");var args=Environment.GetCommandLineArgs();for(int i=0;i+1<args.Length;i++)if(args[i]=="-nakiOutput")destination=Path.GetFullPath(args[i+1]);Directory.CreateDirectory(destination);
   var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var p=new GameObject("Naki Legend Adventure").AddComponent<NakiPrototype>();
   p.enableDesertJourney=true;p.thunderChapterMode=p.capitalMode=p.manorCourtyardMode=p.woodlandMode=p.fireChapterMode=p.iceSampleMode=p.thunderPrototypeMode=p.bossPracticeMode=p.gatehouseSampleMode=p.lightningTrialMode=false;
   p.thunderChapterPracticeBoss=-1;p.thunderChapterStartFloor=0;p.journeyTestProfile="";p.showOpeningOnStart=true;p.journeySeed=0;
   Directory.CreateDirectory("Assets/Scenes");const string entry="Assets/Scenes/NakiPlayer.unity";if(!EditorSceneManager.SaveScene(scene,entry))throw new IOException("Cannot save player entry scene");
   // These sprites/materials are constructed at runtime, so explicitly keep their shaders in the player.
   Directory.CreateDirectory("Assets/Resources/NakiBuild");AssetDatabase.Refresh();int index=0;foreach(string name in new[]{"Sprites/Default","UI/Default","Unlit/Transparent"}){Shader shader=Shader.Find(name);if(shader==null)throw new InvalidOperationException("Missing shader "+name);string path="Assets/Resources/NakiBuild/shader-"+(index++)+".mat";if(AssetDatabase.LoadAssetAtPath<Material>(path)==null)AssetDatabase.CreateAsset(new Material(shader),path);}
   PlayerSettings.companyName="NakiLegendStudio";PlayerSettings.productName="Naki Legend";PlayerSettings.bundleVersion=Version;
   PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;PlayerSettings.fullScreenMode=FullScreenMode.FullScreenWindow;PlayerSettings.resizableWindow=true;PlayerSettings.allowFullscreenSwitch=true;PlayerSettings.runInBackground=false;
   UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset=null;int quality=QualitySettings.GetQualityLevel();for(int i=0;i<QualitySettings.names.Length;i++){QualitySettings.SetQualityLevel(i,false);QualitySettings.renderPipeline=null;}QualitySettings.SetQualityLevel(quality,false);
   PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone,ScriptingImplementation.Mono2x);PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Standalone,ManagedStrippingLevel.Disabled);
   EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(entry,true)};AssetDatabase.SaveAssets();
   var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{entry},locationPathName=Path.Combine(destination,"NakiLegend.exe"),target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
   File.WriteAllText(Path.Combine(destination,"build-report.txt"),"Version: "+Version+"\nResult: "+report.summary.result+"\nErrors: "+report.summary.totalErrors+"\nWarnings: "+report.summary.totalWarnings+"\nBytes: "+report.summary.totalSize+"\nDuration: "+report.summary.totalTime+"\n");
   if(report.summary.result!=BuildResult.Succeeded)throw new InvalidOperationException("Windows build failed: "+report.summary.result);
   Debug.Log("NAKI_WINDOWS_BUILD_OK "+destination);if(Application.isBatchMode)EditorApplication.Exit(0);
  }
 }
}
