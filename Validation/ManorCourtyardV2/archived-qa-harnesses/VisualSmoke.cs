using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NakiLegend.Prototype;
public static class NakiV9Visual {
const BindingFlags B=BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Instance;const string Flag="NakiV9.Visual";static int ticks,stage;static NakiPrototype p;static string Out="D:/NakiLegend/Repository/Validation/ExplorationV9/runtime/";
[InitializeOnLoadMethod]static void Reload(){if(SessionState.GetBool(Flag,false))EditorApplication.update+=Tick;}
public static void Run(){EditorSceneManager.OpenScene("Assets/Scenes/NakiTest.unity");Type t=typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");EditorWindow.GetWindow(t).Show();SessionState.SetBool(Flag,true);EditorApplication.update-=Tick;EditorApplication.update+=Tick;EditorApplication.isPlaying=true;}
static void Set(string k,object v){typeof(NakiPrototype).GetField(k,B).SetValue(p,v);}static object Get(string k){return typeof(NakiPrototype).GetField(k,B).GetValue(p);}static void Call(string k,params object[] a){typeof(NakiPrototype).GetMethod(k,B).Invoke(p,a);}
static void Tick(){if(!EditorApplication.isPlaying||EditorApplication.isCompiling||++ticks<40)return;p=UnityEngine.Object.FindObjectOfType<NakiPrototype>();if(p==null||Get("world")==null)return;try{
if(stage==0){Set("storyLines",null);Set("paused",false);Set("orbLearned",true);Set("bladeLearned",true);Set("waveLearned",true);Set("mapMenu",false);ScreenCapture.CaptureScreenshot(Out+"game-hud.png");stage++;ticks=0;return;}
if(stage==1){if(Get("journeyTitleStyle")==null)throw new Exception("Actual Game View OnGUI has not executed");Set("mapMenu",true);ScreenCapture.CaptureScreenshot(Out+"game-map.png");stage++;ticks=0;return;}
if(stage==2){Set("mapMenu",false);Set("rewardMenu",true);ScreenCapture.CaptureScreenshot(Out+"game-reward.png");stage++;ticks=0;return;}
if(stage==3){Set("rewardMenu",false);Call("TellStory",new object[]{new string[]{"少年：风曾把海上的云带过山口。我会让它重新认得回家的路，为枯井与树根带来甘泽。"}});ScreenCapture.CaptureScreenshot(Out+"game-dialogue.png");stage++;ticks=0;return;}
if(stage==4){if(!File.Exists(Out+"game-dialogue.png"))throw new Exception("Game View screenshot missing");File.WriteAllText(Out+"visual-result.txt","PASS: Actual Game View OnGUI executed; HUD, fog map, reward choices and CJK dialogue screenshots captured.\n");Finish(0);}
}catch(Exception e){File.WriteAllText(Out+"visual-result.txt","FAIL "+e);Debug.LogException(e);Finish(1);}}
static void Finish(int c){SessionState.SetBool(Flag,false);EditorApplication.update-=Tick;EditorApplication.Exit(c);}
}
