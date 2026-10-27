using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NakiLegend.Prototype;

public static class NakiGatehouseSmoke
{
 const BindingFlags B=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
 const string Flag="Naki.GatehouseQA";
 const string Out="D:/NakiLegend/Repository/Validation/GatehouseV13/runtime/";
 static NakiPrototype p;static int ticks,stage,checks;static List<string> errors=new List<string>();
 [InitializeOnLoadMethod]static void Reload(){if(SessionState.GetBool(Flag,false))Attach();}
 public static void BuildAndRun(){NakiLegend.Prototype.Editor.NakiSampleScenes.BuildPracticeScenes();NakiLegend.Prototype.Editor.NakiSampleScenes.BuildGatehouseScene();Run();}
 static void Attach(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;Application.logMessageReceived-=Log;Application.logMessageReceived+=Log;}
 public static void Run(){Directory.CreateDirectory(Out);EditorSceneManager.OpenScene("Assets/Scenes/NakiGatehouseSample.unity");Debug.Log("GATE_PRE_PLAY mode="+UnityEngine.Object.FindObjectOfType<NakiPrototype>().gatehouseSampleMode+" scene="+EditorSceneManager.GetActiveScene().path);var w=EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));w.Show();SetSize(w);SessionState.SetBool(Flag,true);Attach();EditorApplication.isPlaying=true;}
 static void SetSize(EditorWindow w){var a=typeof(Editor).Assembly;var t=a.GetType("UnityEditor.GameViewSizes");var singleton=typeof(ScriptableSingleton<>).MakeGenericType(t).GetProperty("instance",BindingFlags.Public|BindingFlags.Static).GetValue(null,null);var group=t.GetMethod("GetGroup").Invoke(singleton,new[]{Enum.Parse(a.GetType("UnityEditor.GameViewSizeGroupType"),"Standalone")});var size=Activator.CreateInstance(a.GetType("UnityEditor.GameViewSize"),new object[]{Enum.Parse(a.GetType("UnityEditor.GameViewSizeType"),"FixedResolution"),1920,1080,"Gatehouse QA"});group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});w.GetType().GetProperty("selectedSizeIndex",B).SetValue(w,(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null)-1,null);}
 static void Log(string s,string trace,LogType t){if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert)errors.Add(s);}
 static object Get(object o,string n){return o.GetType().GetField(n,B).GetValue(o);}static object Get(string n){return Get(p,n);}static void Set(string n,object v){typeof(NakiPrototype).GetField(n,B).SetValue(p,v);}static void Set(object o,string n,object v){o.GetType().GetField(n,B).SetValue(o,v);}
 static object Call(string n,params object[] v){return typeof(NakiPrototype).GetMethod(n,B).Invoke(p,v);}static IList List(string n){return (IList)Get(n);}static float F(string n){return (float)Get(n);}static object Room(int id){return List("journeyRooms")[id];}
 static void Check(bool ok,string why){if(!ok)throw new Exception(why);checks++;Debug.Log("GATEHOUSE_PASS "+why);}
 static void Quiet(){Set("storyLines",null);Set("paused",false);Set("inventoryMenu",false);Set("mapMenu",false);Set("rewardMenu",false);Set("roomBannerUntil",0f);}
 static void Fresh(){Call("BeginJourney",false);Quiet();}
 static void Enter(int id,int d=-1){Call("EnterExplorationRoom",id,d);Quiet();}
 static void Go(int d)
 {
  Set("position",Call("DoorPoint",d,1.12f));Set("clock",F("clock")+2);Call("TryAutomaticExit");Check(F("doorTransition")>0,"Actual door starts a transition");
  for(int i=0;i<7;i++)Call("TickDoorTransition",.05f);Quiet();Check((bool)Call("SafeFooting",Get("position"),.34f),"Transition arrives on a movable safe point");
 }
 static void KillWave(){Set("clock",F("clock")+1);Call("TickPendingSpawns");for(int i=List("enemies").Count-1;i>=0;i--)Call("DamageEnemy",List("enemies")[i],10000f);Call("TickJourney",.01f);}
 static void ClearGate(){KillWave();Check(!(bool)Get(Room(1),"cleared"),"First wave does not open combat doors");Set("clock",F("clock")+1.2f);Call("TickJourney",.01f);Check((int)Get("currentWave")==2,"Second wave is queued after delay");KillWave();Check((bool)Get(Room(1),"cleared"),"Two waves clear gatehouse");}
 static void Action(int kind){var pts=(Vector2[])typeof(NakiPrototype).GetField("GateActionPoints",BindingFlags.Static|BindingFlags.NonPublic).GetValue(null);Set("position",pts[kind]+(kind==2||kind==4?Vector2.left*.85f:Vector2.zero));Check((bool)Call("TryRoomInteraction"),"F interaction reaches intended prop");}
 static void Finish(){for(int i=0;i<60;i++)Call("TickGatehouseInteraction",.05f);}
 static int EventCrystals(){int n=0;foreach(object d in (IList)Get(Room(1),"essence"))if((float)Get(d,"value")==20)n++;return n;}
 static void Functional()
 {
  Fresh();Check(p.gatehouseSampleMode&&List("journeyRooms").Count==5,"Dedicated scene uses five fixed connected rooms; mode="+p.gatehouseSampleMode+" count="+List("journeyRooms").Count+" scene="+p.gameObject.scene.path);
  Check((bool)Get("orbLearned")&&(bool)Get("bladeLearned")&&(bool)Get("waveLearned"),"Sample explicitly opens three skills");
  Enter(1,1);Check(List("pendingSpawns").Count==4,"First wave contains four warned enemies");
  foreach(object s in List("pendingSpawns")){var at=(Vector2)Get(s,"at");Check(Vector2.Distance(at,(Vector2)Get("position"))>=3.5f&&!(bool)Call("Blocked",at,.38f)&&!(bool)Call("IntersectsTransit",at,Vector2.one),"Reinforcement avoids arrival and solid cover");}
  Action(0);Check(F("gateWorkRemaining")==0,"Combat prevents repair interaction");ClearGate();
  Check((int)Get("clearedCombatRooms")==1,"Repair sample counts one combat clear");
  Check((bool)Call("GatehouseDoorLocked",1,2)&&!(bool)Call("GatehouseDoorLocked",1,0),"Shortcut locked while normal north path remains open");
  Check(!(bool)Call("CanStandInExploration",new Vector2(9.1f,0),.3f),"Locked shortcut blocks movement");
  Check((bool)Call("ProjectilePathBlocked",new Vector2(7,0),new Vector2(9,0),.02f),"Locked shortcut blocks projectiles");
  Go(0);Check((int)Get("currentRoom")==2,"Can leave without repairing");Go(2);Check((int)Get("currentRoom")==4,"Unrepaired shortcut has an alternate physical path");Go(1);Check((int)Get("currentRoom")==3,"Alternate route reaches other endpoint");KillWave();Check((bool)Get(Room(3),"cleared")&&(bool)Call("GatehouseDoorLocked",3,3),"Clearing far side alone does not unlock an unrepaired gate");
  Fresh();Enter(1,1);ClearGate();
  Action(2);Check(!(bool)Get("gateRepairCommitted"),"Wheel cannot operate before sand is cleared");
  Action(0);float clock=F("clock"),ep=F("elementPower"),light=F("energy");Call("Update");Check(F("clock")==clock&&F("elementPower")==ep&&F("energy")==light,"Work overlay pauses gameplay clocks and resources");Call("FinishGatehouseInteraction",false);Check(!((bool[])Get("gateSandCleared"))[0],"Cancel before commit leaves sand untouched");
  Action(0);Finish();Enter(2);Enter(1);Check(((bool[])Get("gateSandCleared"))[0]&&!((bool[])Get("gateSandCleared"))[1],"Partial repair persists on revisiting");
  Check(List("enemies").Count==0&&List("pendingSpawns").Count==0,"Cleared room does not respawn enemies on revisit");
  Action(1);Finish();Set("elementPower",100f);Set("energy",64f);Set("darkPower",27f);Action(2);
  Check((GameObject)Get("gateRotor")!=null,"Handwheel has an independently animated rotor");Check((bool)Get("gateRepairCommitted")&&(bool)Get("gateRewardCreated")&&EventCrystals()==1,"Wheel commits one persistent twenty-point reward");
  for(int i=0;i<20;i++)Call("TryRoomInteraction");Check(EventCrystals()==1,"Rapid repeated interaction cannot duplicate reward");
  Call("FinishGatehouseInteraction",true);Check((bool)Call("GatehouseDoorLocked",1,2),"Repair waits for far-end combat to clear");
  Check(F("elementPower")==100&&F("energy")==64&&F("darkPower")==27,"Repair itself consumes no resource and grants no light or dark");
  Call("TickEssence",.02f);Check(EventCrystals()==1,"Full elemental pool leaves event reward on floor");
  Go(0);Action(4);Quiet();Check((bool)Get("gateTotalUnlocked"),"Master valve unlock persists separately from local repair");
  Go(2);Action(5);Quiet();Check((bool)Get("gateRainReturned"),"Rain preview advances only after master unlock");
  Go(1);KillWave();Check(!(bool)Call("GatehouseDoorLocked",3,3),"Gate opens after repair and both endpoint clears");
  Check((bool)Call("CanStandInExploration",new Vector2(-9.1f,0),.3f),"Opened west shortcut is walkable");Go(3);
  Check((int)Get("currentRoom")==1&&EventCrystals()==1,"Real shortcut returns to repaired room and uncollected reward");
  int active=0;foreach(GameObject g in List("gateWaterVisuals"))if(g.activeSelf)active++;Check(active==9&&List("gateRainVisuals").Count==28,"Wet canal and gentle rain reconstruct on revisit");
  Check(!(bool)Call("ProjectilePathBlocked",new Vector2(7,0),new Vector2(9,0),.02f),"Open shortcut removes its projectile seal");
  int count=List("projectileObstacles").Count;for(int i=0;i<100;i++)Call("UpdateExplorationDoors");Check(List("projectileObstacles").Count==count,"Repeated door refresh does not accumulate collision blocks");
  object reward=null;foreach(object d in List("essenceDrops"))if((float)Get(Get(d,"data"),"value")==20)reward=d;
  Set("position",Get(Get(reward,"data"),"at"));Set("elementPower",40f);Set("clock",F("clock")+1);Call("TickEssence",.01f);
  Check(F("elementPower")==60&&EventCrystals()==0,"Event crystal collects exactly twenty elemental power");
  Enter(2);Enter(1);Check(EventCrystals()==0&&(bool)Get("gateRewardCreated"),"Collected event reward cannot return on revisiting");
  foreach(int id in new[]{0,1,2,3,4})foreach(int link in (List<int>)Get(Room(id),"links")){int d=(int)Call("NeighborDirection",Room(id),Room(link));Enter(id,d);Check((bool)Call("SafeFooting",Get("position"),.34f),"Each actual linked-door arrival remains safe");}
  Fresh();Check(!(bool)Get("gateRepairCommitted")&&!((bool[])Get("gateSandCleared"))[0]&&EventCrystals()==0,"R restart clears run-specific repair and reward state");
  int seed=(int)Get("activeSeed");Call("BeginJourney",true);Quiet();Check((int)Get("activeSeed")!=seed&&List("journeyRooms").Count==5,"N restarts sample with a new seed and fixed layout");
  Set("health",0f);for(int i=0;i<90;i++)Call("Update");Check(Get("currentHeroAction").ToString()=="Defeated","Sample death holds without animation errors");
  Call("ResetRoom");Quiet();Check(F("health")==6,"Restart after sample death remains playable");
 }
 static void Pose(bool wet)
 {
  Fresh();Set(Room(1),"cleared",true);Set(Room(3),"cleared",true);
  if(wet){((bool[])Get("gateSandCleared"))[0]=true;((bool[])Get("gateSandCleared"))[1]=true;Set("gateRepairCommitted",true);Set("gateTotalUnlocked",true);Set("gateRainReturned",true);}
  Enter(1,1);Set("position",new Vector2(-1.6f,1.8f));Call("UpdatePixelHero",Vector2.zero);Set("invulnerableUntil",0f);Set("roomBannerUntil",0f);Quiet();
 }
 static void Tick()
 {
  if(!EditorApplication.isPlaying||EditorApplication.isCompiling||++ticks<40)return;p=UnityEngine.Object.FindObjectOfType<NakiPrototype>();if(p==null||Get("world")==null)return;
  try{
   if(stage==0){p.enabled=false;Functional();Check(errors.Count==0,"Functional checks have no runtime errors");Pose(false);p.enabled=true;stage++;ticks=0;return;}
   if(stage==1){ScreenCapture.CaptureScreenshot(Out+"dry-gatehouse.png");stage++;ticks=0;return;}
   if(stage==2){Pose(true);stage++;ticks=0;return;}
   if(stage==3){ScreenCapture.CaptureScreenshot(Out+"restored-gatehouse.png");stage++;ticks=0;return;}
   if(stage==4){foreach(object n in List("journeyRooms")){Set(n,"discovered",true);Set(n,"visited",true);}Set("mapMenu",true);stage++;ticks=0;return;}
   if(stage==5){ScreenCapture.CaptureScreenshot(Out+"sample-map.png");stage++;ticks=0;return;}
   Check(errors.Count==0,"Real Game View renders sample HUD, map and restored environment");File.WriteAllText(Out+"smoke-result.txt","PASS "+checks+" checks: real five-room transitions; two waves; safe arrivals; optional repair; cancellation; repeated interaction; full-resource reward, collection and revisits; two-end gate safety; master valve and rain state; restart and death; 1920x1080 screenshots.\n");End(0);
  }catch(Exception e){Debug.LogException(e);File.WriteAllText(Out+"smoke-result.txt","FAIL after "+checks+" checks\n"+e+"\n"+string.Join("\n",errors));End(1);}
 }
 static void End(int code){SessionState.SetBool(Flag,false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;EditorApplication.Exit(code);}
}
