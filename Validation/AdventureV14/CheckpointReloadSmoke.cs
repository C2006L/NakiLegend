using System;
using System.IO;
using System.Reflection;
using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using NakiLegend.Prototype;
public static class NakiCheckpointReloadSmoke
{
 const BindingFlags B=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
 const string Key="Naki.Checkpoint.ReloadV14",Out="D:/NakiLegend/Repository/Validation/AdventureV14/runtime/";
 static NakiPrototype p;static int ticks;
 [InitializeOnLoadMethod]static void Reload(){if(SessionState.GetBool(Key,false))Attach();}
 static void Attach(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;Application.logMessageReceived-=Log;Application.logMessageReceived+=Log;}
 static object Get(string n){var f=typeof(NakiPrototype).GetField(n,B);return f!=null?f.GetValue(p):typeof(NakiPrototype).GetProperty(n,B).GetValue(p,null);}
 static void Set(string n,object v){typeof(NakiPrototype).GetField(n,B).SetValue(p,v);}
 static object Call(string n,params object[] a){return typeof(NakiPrototype).GetMethod(n,B).Invoke(p,a);}
 static void Check(bool ok,string text){if(!ok)throw new Exception(text);SessionState.SetInt(Key+".checks",SessionState.GetInt(Key+".checks",0)+1);}
 static void Log(string s,string trace,LogType t){if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert){SessionState.SetBool(Key+".error",true);File.AppendAllText(Out+"checkpoint-reload-errors.txt",s+"\n"+trace+"\n");}}
 public static void Run(){Directory.CreateDirectory(Out);File.WriteAllText(Out+"checkpoint-reload-errors.txt","");EditorSceneManager.OpenScene("Assets/Scenes/NakiTest.unity");UnityEngine.Object.FindObjectOfType<NakiPrototype>().showOpeningOnStart=false;SessionState.SetBool(Key,true);SessionState.SetBool(Key+".error",false);SessionState.SetInt(Key+".step",0);SessionState.SetInt(Key+".checks",0);Attach();EditorApplication.isPlaying=true;}
 static void Quiet(){Call("StopOpeningCinematic");Set("storyLines",null);Set("paused",false);Set("inventoryMenu",false);Set("mapMenu",false);}
 static void Ready(){Check((float)Get("health")==4,"Health restored automatically");Check(Get("origin").ToString()=="Dark"&&(float)Get("darkPower")==67,"Dark origin and reserve restored automatically");Check(((IList)Get("journeyRooms")).Count==44&&(int)Get("currentRoom")==39&&(bool)Get("GatehouseActive"),"Appended canal checkpoint restored automatically");Check(((IDictionary)Get("heroFrames")).Count==7,"All animation caches rebuilt");Check(((int[])Get("carriedWeapons"))[0]==2,"Bow equipment restored");Check(!SessionState.GetBool(Key+".error",false),"No runtime errors during domain reload");Quiet();}
 static void Request(int step){SessionState.SetInt(Key+".step",step);ticks=0;EditorUtility.RequestScriptReload();}
 static void Tick()
 {
  if(!EditorApplication.isPlaying||EditorApplication.isCompiling||++ticks<80)return;p=UnityEngine.Object.FindObjectOfType<NakiPrototype>();if(p==null)return;
  try{
   int step=SessionState.GetInt(Key+".step",0);
   if(step==0){Quiet();Call("ChooseOrigin",1);Quiet();int end=((int[])Get("stageEnds"))[2];Call("EnterExplorationRoom",end,-1);Quiet();object boss=((IList)Get("enemies"))[0];Call("DamageEnemy",boss,100000f);Call("TickJourney",.01f);Call("EnterExplorationRoom",39,-1);Quiet();Set("allowBatchCheckpoints",true);Set("checkpointPathOverride",Out+"reload-checkpoint/journey.json");Set("health",4f);Set("darkPower",67f);Set("elementPower",41f);((int[])Get("carriedWeapons"))[0]=2;Check((bool)Call("SaveCheckpoint",false),"Save forty-four-room dark checkpoint");Set("health",0f);Call("UpdatePixelHero",Vector2.zero);Request(1);return;}
   if(step==1){Ready();Request(2);return;}
   if(step==2){Ready();Set("health",0f);for(int i=0;i<240;i++)Call("Update");Check(Get("currentHeroAction").ToString()=="Defeated","Death still renders after restored-domain reload");Check((bool)Call("LoadCheckpoint",true),"Reload checkpoint after death");Ready();Finish("PASS "+SessionState.GetInt(Key+".checks",0)+" checks; actual dead/alive assembly reload resumes a dark-origin forty-four-room canal checkpoint, inventory and resources; death and manual reload remain valid.",0);}
  }catch(Exception e){Finish("FAIL "+e,1);}
 }
 static void Finish(string text,int code){File.WriteAllText(Out+"checkpoint-reload-result.txt",text+"\n");SessionState.SetBool(Key,false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;EditorApplication.Exit(code);}
}
