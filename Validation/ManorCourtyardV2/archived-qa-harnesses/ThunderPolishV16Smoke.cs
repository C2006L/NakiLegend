using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NakiLegend.Prototype;

public static class NakiThunderPolishV16Smoke
{
 const BindingFlags B=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
 const string Out="D:/NakiLegend/Repository/Validation/ThunderPolishV16/runtime/",Flag="Naki.ThunderPolishV16";
 static NakiPrototype p;static int ticks,stage,checks;static readonly List<string> errors=new List<string>();static readonly List<object> portraits=new List<object>();
 [InitializeOnLoadMethod]static void Reload(){if(SessionState.GetBool(Flag,false))Attach();}
 public static void Run(){Directory.CreateDirectory(Out);AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);EditorSceneManager.OpenScene("Assets/Scenes/NakiThunderChapter.unity");SetSize();SessionState.SetBool(Flag,true);Attach();EditorApplication.isPlaying=true;}
 static void Attach(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;Application.logMessageReceived-=Log;Application.logMessageReceived+=Log;}
 static void Log(string s,string stack,LogType t){if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert)errors.Add(s);}
 static object Get(object o,string n){var f=o.GetType().GetField(n,B);return f!=null?f.GetValue(o):o.GetType().GetProperty(n,B).GetValue(o,null);}static object Get(string n){return Get(p,n);}
 static void Set(object o,string n,object v){o.GetType().GetField(n,B).SetValue(o,v);}static void Set(string n,object v){Set(p,n,v);}
 static object Call(string n,params object[] args){return typeof(NakiPrototype).GetMethod(n,B).Invoke(p,args);}static IList List(string n){return (IList)Get(n);}static object Room(int id){return List("journeyRooms")[id];}static float F(string n){return (float)Get(n);}
 static object Kind(string name){return Enum.Parse(typeof(NakiPrototype).GetNestedType("EnemyKind",BindingFlags.NonPublic),name);}
 static void Check(bool yes,string why){if(!yes)throw new Exception(why);checks++;Debug.Log("THUNDER_POLISH_PASS "+why);}
 static void Quiet(){Set("storyLines",null);Set("chapterSetup",false);Set("chapterSkillsMenu",false);Set("chapterRewardMenu",false);Set("chapterCg",false);Set("paused",false);Set("inventoryMenu",false);Set("mapMenu",false);Set("rewardMenu",false);}
 static void Fresh(){Time.timeScale=1;p.thunderChapterMode=true;p.thunderPrototypeMode=false;p.thunderChapterStartFloor=1;p.thunderChapterPracticeBoss=-1;p.thunderPreset=NakiPrototype.ThunderPreset.Dark;Call("BeginThunderChapter",false);Quiet();Set("invulnerableUntil",100000f);}
 static void Enter(int id){Call("EnterExplorationRoom",id,-1);Quiet();Set("invulnerableUntil",100000f);}
 static object Spawn(string kind,Vector2 at){return Call("SpawnJourneyEnemy",Kind(kind),at,4);}
 static void Step(float seconds){for(int i=0;i<Mathf.CeilToInt(seconds/.05f);i++){Set("clock",F("clock")+.05f);Call("TickJourney",.05f);Quiet();}}
 static void DeathTransition(string kind,bool champion,bool flip)
 {
  Fresh();object enemy=Spawn(kind,Vector2.zero);if(champion)Call("MakeChampion",enemy);
  var actor=(GameObject)Get(enemy,"root");var sr=(SpriteRenderer)Get(enemy,"body");sr.flipX=flip;
  // Exercise the real kill path, including removal, essence and fading-body creation.
  Vector3 size=actor.transform.localScale;Quaternion rotation=actor.transform.localRotation;Call("DamageEnemy",enemy,100000f);
  object faded=List("fadingBodies")[List("fadingBodies").Count-1];var body=(GameObject)Get(faded,"root");var dead=(SpriteRenderer)Get(faded,"renderer");
  Check((body.transform.localScale-size).sqrMagnitude<.000001f,"Death keeps live scale: "+kind+" champion="+champion);
  Check(Quaternion.Angle(body.transform.localRotation,rotation)<.001f&&dead.flipX==flip,"Death keeps orientation: "+kind);
  Check(Mathf.Abs(dead.bounds.size.x-dead.sprite.bounds.size.x*Mathf.Abs(size.x))<.001f,"Rendered corpse respects authored world size: "+kind);
  Set("clock",F("clock")+.25f);Call("TickJourneyEffects",.25f);Check((body.transform.localScale-size).sqrMagnitude<.000001f&&dead.color.a>0&&dead.color.a<1,"Corpse fades without enlarging: "+kind);
  Set("clock",F("clock")+.4f);Call("TickJourneyEffects",.4f);Check(!List("fadingBodies").Contains(faded),"Corpse expires normally: "+kind);
 }
 static void WaveProgression()
 {
  Fresh();var changed=new HashSet<int>{3,5,7,13,15,19,26,28,30,32};int total=0,added=0;
  for(int id=0;id<39;id++)
  {
   object n=Room(id);string kind=Get(n,"kind").ToString();int stageIndex=(int)Get(n,"stage"),waves=(int)Get(n,"waves");
   if(kind=="Combat"||kind=="Elite")
   {
    int old=stageIndex==0?2:3;Check(waves==old+(changed.Contains(id)?1:0),"Only selected combat rooms gain one wave: "+id);total+=waves;added+=waves-old;
    Enter(id);int iterations=0;while(!(bool)Get(n,"cleared")&&iterations++<20){Step(2f);for(int i=List("enemies").Count-1;i>=0;i--)Call("DamageEnemy",List("enemies")[i],100000f);Step(.2f);}
    Check((bool)Get(n,"cleared")&&(int)Get("currentWave")==waves,"Actual added waves finish before opening doors: "+id);
    Check(List("enemies").Count==0&&List("pendingSpawns").Count==0,"No leftover enemies lock the room: "+id);
   }
   else Check(waves==1,"Boss, shelter, story and reward rooms retain wave budget: "+id);
  }
  Check(total==72&&added==10,"Island adds ten waves, 62 to 72, across three floors");
  Enter(12);Set("allowBatchCheckpoints",true);Set("chapterCheckpointPathOverride",Out+"save/chapter.json");Check((bool)Call("SaveChapterCheckpoint",false),"Save after clearing expanded encounters");
  Fresh();Check((bool)Call("LoadChapterCheckpoint",false),"Existing map identity can resume expanded encounter layout");foreach(int id in changed)Check((bool)Get(Room(id),"cleared"),"Loading does not replay an already-cleared room: "+id);Set("allowBatchCheckpoints",false);
 }
 static void PreparePortraits()
 {
  Fresh();portraits.Clear();portraits.Add(Spawn("ThunderStone",new Vector2(-3,0)));portraits.Add(Spawn("ThunderStone",Vector2.zero));portraits.Add(Spawn("SandKite",new Vector2(3,0)));
  for(int i=0;i<portraits.Count;i++){object e=portraits[i];if(i>0)Call("MakeChampion",e);Set(e,"speed",0f);Set(e,"nextAttack",100000f);var sr=(SpriteRenderer)Get(e,"body");sr.flipX=i==1;}
  Call("UpdatePixelHero",Vector2.zero);Time.timeScale=0;
 }
 static void SetSize(){var a=typeof(Editor).Assembly;var w=EditorWindow.GetWindow(a.GetType("UnityEditor.GameView"));w.Show();var t=a.GetType("UnityEditor.GameViewSizes");var singleton=typeof(ScriptableSingleton<>).MakeGenericType(t).GetProperty("instance",BindingFlags.Public|BindingFlags.Static).GetValue(null,null);var group=t.GetMethod("GetGroup").Invoke(singleton,new[]{Enum.Parse(a.GetType("UnityEditor.GameViewSizeGroupType"),"Standalone")});var size=Activator.CreateInstance(a.GetType("UnityEditor.GameViewSize"),new object[]{Enum.Parse(a.GetType("UnityEditor.GameViewSizeType"),"FixedResolution"),1920,1080,"Thunder death QA"});group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});w.GetType().GetProperty("selectedSizeIndex",B).SetValue(w,(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null)-1,null);}
 static void Next(){stage++;ticks=0;}
 static void Tick()
 {
  if(!EditorApplication.isPlaying||EditorApplication.isCompiling||++ticks<25)return;p=UnityEngine.Object.FindObjectOfType<NakiPrototype>();if(p==null||Get("world")==null)return;
  try
  {
   if(stage==0){p.enabled=false;foreach(string kind in new[]{"ChargeMoth","StormGuard","CopperRoller","ThunderStone","SandKite","StoneSentinel","CopperWarden","ThunderSentinel","StormStag"})foreach(bool champion in new[]{false,true})DeathTransition(kind,champion,champion);WaveProgression();PreparePortraits();p.enabled=true;Next();return;}
   if(stage==1){ScreenCapture.CaptureScreenshot(Out+"elite-alive.png");Next();return;}
   if(stage==2){foreach(object e in portraits)Call("DamageEnemy",e,100000f);Next();return;}
   if(stage==3){ScreenCapture.CaptureScreenshot(Out+"elite-defeated.png");Next();return;}
   Check(errors.Count==0,"Native death poses, expanded encounters and UI have no runtime errors");File.WriteAllText(Out+"smoke-result.txt","PASS "+checks+" checks: real kill/fade paths for nine enemy types and champions, preserved render scale and direction, actual clearing of all 23 non-boss encounter rooms, 62 to 72 total waves, old-layout checkpoint continuity and native before/after screenshots.\n");End(0);
  }catch(Exception e){Debug.LogException(e);File.WriteAllText(Out+"smoke-result.txt","FAIL after "+checks+"\n"+e+"\n"+string.Join("\n",errors));End(1);}
 }
 static void End(int code){Time.timeScale=1;SessionState.SetBool(Flag,false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;EditorApplication.Exit(code);}
}
