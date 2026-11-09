using System;
using System.IO;
using System.Reflection;
using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using NakiLegend.Prototype;
public static class NakiChapterReloadSmoke
{
 const BindingFlags B=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
 const string Key="Naki.Chapter.ReloadV1",Out="D:/NakiLegend/Repository/Validation/ThunderChapterV1/runtime/";
 static NakiPrototype p;static int ticks;
 [InitializeOnLoadMethod]static void Reload(){if(SessionState.GetBool(Key,false))Attach();}
 static void Attach(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;Application.logMessageReceived-=Log;Application.logMessageReceived+=Log;}
 static object Get(string n){var f=typeof(NakiPrototype).GetField(n,B);return f!=null?f.GetValue(p):typeof(NakiPrototype).GetProperty(n,B).GetValue(p,null);}
 static void Set(string n,object v){typeof(NakiPrototype).GetField(n,B).SetValue(p,v);}static object Call(string n,params object[] a){return typeof(NakiPrototype).GetMethod(n,B).Invoke(p,a);}
 static void Check(bool ok,string why){if(!ok)throw new Exception(why);SessionState.SetInt(Key+".checks",SessionState.GetInt(Key+".checks",0)+1);}
 static void Log(string s,string trace,LogType t){if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert){SessionState.SetBool(Key+".error",true);File.AppendAllText(Out+"reload-errors.txt",s+"\n"+trace+"\n");}}
 public static void Run(){Directory.CreateDirectory(Out);File.WriteAllText(Out+"reload-errors.txt","");EditorSceneManager.OpenScene("Assets/Scenes/NakiThunderChapter.unity");SessionState.SetBool(Key,true);SessionState.SetBool(Key+".error",false);SessionState.SetInt(Key+".step",0);SessionState.SetInt(Key+".checks",0);Attach();EditorApplication.isPlaying=true;}
 static void Quiet(){Set("storyLines",null);Set("paused",false);Set("inventoryMenu",false);Set("mapMenu",false);Set("chapterSetup",false);}
 static void Ready()
 {
  Check(p.thunderChapterMode&&Get("origin").ToString()=="Dark","Dedicated mode and dark preset restore");
  Check((float)Get("health")==4&&(float)Get("darkPower")==67&&(float)Get("elementPower")==100,"Resources restore without cross-pool mixing");
  Check(((IList)Get("journeyRooms")).Count==39&&(int)Get("currentRoom")==25&&(bool)Get("chapterWest")&&(bool)Get("chapterEast"),"Repaired node flags and 39-room graph restore");
  Check(((IDictionary)Get("heroFrames")).Count==7&&((IDictionary)Get("enemyActionArt")).Count==15&&Get("thunderFloor")!=null,"Player, new enemy frames and environment caches rebuild");
  Check(!(bool)Get("CheckpointsEnabled")&&!SessionState.GetBool(Key+".error",false),"Normal save stays disabled and reload produces no runtime exceptions");Quiet();
 }
 static void Request(int step){SessionState.SetInt(Key+".step",step);ticks=0;EditorUtility.RequestScriptReload();}
 static void Tick()
 {
  if(!EditorApplication.isPlaying||EditorApplication.isCompiling||++ticks<80)return;p=UnityEngine.Object.FindObjectOfType<NakiPrototype>();if(p==null)return;
  try
  {
   int step=SessionState.GetInt(Key+".step",0);
   if(step==0)
   {
    p.enabled=false;p.thunderPreset=NakiPrototype.ThunderPreset.Dark;p.thunderChapterStartFloor=2;p.thunderChapterPracticeBoss=-1;Call("BeginThunderChapter",false);Quiet();
    Set("allowBatchCheckpoints",true);Set("chapterCheckpointPathOverride",Out+"reload/thunder.json");Set("health",4f);Set("darkPower",67f);Set("elementPower",100f);
    Check((bool)Call("SaveChapterCheckpoint",false),"Write completed chapter checkpoint before reload");Set("health",0f);Call("UpdatePixelHero",Vector2.zero);p.enabled=true;Request(1);return;
   }
   if(step==1){Ready();Request(2);return;}
   if(step==2){Ready();Set("health",0f);for(int i=0;i<240;i++)Call("Update");Check(Get("currentHeroAction").ToString()=="Defeated","Death pose still valid after new enemy cache reload");Check((bool)Call("LoadChapterCheckpoint",true),"Prototype checkpoint reloads after death");Ready();Finish("PASS "+SessionState.GetInt(Key+".checks",0)+" checks; real dead/alive script reload restores independent dark chapter, repaired nodes, resources, environment and enemy animation caches.",0);}
  }catch(Exception e){Finish("FAIL "+e,1);}
 }
 static void Finish(string text,int code){File.WriteAllText(Out+"reload-result.txt",text+"\n");SessionState.SetBool(Key,false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;EditorApplication.Exit(code);}
}
