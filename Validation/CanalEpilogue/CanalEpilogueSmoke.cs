using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NakiLegend.Prototype;

public static class NakiCanalEpilogueSmoke
{
 const BindingFlags B=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
 const string Flag="Naki.CanalEpilogueQA";
 const string Out="D:/NakiLegend/Repository/Validation/CanalEpilogue/runtime/";
 static NakiPrototype p;static int ticks,stage,checks;static List<string> errors=new List<string>();
 [InitializeOnLoadMethod]static void Reload(){if(SessionState.GetBool(Flag,false))Attach();}
 public static void BuildAndRun(){NakiLegend.Prototype.Editor.NakiSampleScenes.BuildPracticeScenes();NakiLegend.Prototype.Editor.NakiSampleScenes.BuildGatehouseScene();Run();}
 static void Attach(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;Application.logMessageReceived-=Log;Application.logMessageReceived+=Log;}
 public static void Run(){Directory.CreateDirectory(Out);EditorSceneManager.OpenScene("Assets/Scenes/NakiTest.unity");Debug.Log("GATE_PRE_PLAY mode="+UnityEngine.Object.FindObjectOfType<NakiPrototype>().gatehouseSampleMode+" scene="+EditorSceneManager.GetActiveScene().path);var w=EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));w.Show();SetSize(w);SessionState.SetBool(Flag,true);Attach();EditorApplication.isPlaying=true;}
 static void SetSize(EditorWindow w){var a=typeof(Editor).Assembly;var t=a.GetType("UnityEditor.GameViewSizes");var singleton=typeof(ScriptableSingleton<>).MakeGenericType(t).GetProperty("instance",BindingFlags.Public|BindingFlags.Static).GetValue(null,null);var group=t.GetMethod("GetGroup").Invoke(singleton,new[]{Enum.Parse(a.GetType("UnityEditor.GameViewSizeGroupType"),"Standalone")});var size=Activator.CreateInstance(a.GetType("UnityEditor.GameViewSize"),new object[]{Enum.Parse(a.GetType("UnityEditor.GameViewSizeType"),"FixedResolution"),1920,1080,"Gatehouse QA"});group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});w.GetType().GetProperty("selectedSizeIndex",B).SetValue(w,(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null)-1,null);}
 static void Log(string s,string trace,LogType t){if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert)errors.Add(s);}
 static object Get(object o,string n){var f=o.GetType().GetField(n,B);return f!=null?f.GetValue(o):o.GetType().GetProperty(n,B).GetValue(o,null);}static object Get(string n){return Get(p,n);}static void Set(string n,object v){typeof(NakiPrototype).GetField(n,B).SetValue(p,v);}static void Set(object o,string n,object v){o.GetType().GetField(n,B).SetValue(o,v);}
 static object Call(string n,params object[] v){return typeof(NakiPrototype).GetMethod(n,B).Invoke(p,v);}static IList List(string n){return (IList)Get(n);}static float F(string n){return (float)Get(n);}static object Room(int id){return List("journeyRooms")[id];}
 static void Check(bool ok,string why){if(!ok)throw new Exception(why);checks++;Debug.Log("GATEHOUSE_PASS "+why);}
 static void Quiet(){Set("storyLines",null);Set("paused",false);Set("inventoryMenu",false);Set("mapMenu",false);Set("rewardMenu",false);Set("roomBannerUntil",0f);}
 static void Fresh(){p.gatehouseSampleMode=false;p.bossPracticeMode=false;p.lightningTrialMode=false;p.showOpeningOnStart=false;Call("BeginJourney",false);Call("ChooseOrigin",0);Quiet();}
 static void Enter(int id,int d=-1){Call("EnterExplorationRoom",id,d);Quiet();}
 static void Go(int d)
 {
  Set("position",Call("DoorPoint",d,1.12f));Set("clock",F("clock")+2);Call("TryAutomaticExit");Check(F("doorTransition")>0,"Actual door starts a transition");
  for(int i=0;i<7;i++)Call("TickDoorTransition",.05f);Quiet();Check((bool)Call("SafeFooting",Get("position"),.34f),"Transition arrives on a movable safe point");
 }
 static void KillWave(){Set("clock",F("clock")+1);Call("TickPendingSpawns");for(int i=List("enemies").Count-1;i>=0;i--)Call("DamageEnemy",List("enemies")[i],10000f);Call("TickJourney",.01f);}
 static void ClearGate(){KillWave();Check(!(bool)Get(Room((int)Get("canalEpilogueStart")+1),"cleared"),"First wave does not open combat doors");Set("clock",F("clock")+1.2f);Call("TickJourney",.01f);Check((int)Get("currentWave")==2,"Second wave is queued after delay");KillWave();Check((bool)Get(Room((int)Get("canalEpilogueStart")+1),"cleared"),"Two waves clear gatehouse");}
 static void Action(int kind){var pts=(Vector2[])typeof(NakiPrototype).GetField("GateActionPoints",BindingFlags.Static|BindingFlags.NonPublic).GetValue(null);Set("position",pts[kind]+(kind==2||kind==4?Vector2.left*.85f:Vector2.zero));Check((bool)Call("TryRoomInteraction"),"F interaction reaches intended prop");}
 static void Finish(){for(int i=0;i<60;i++)Call("TickGatehouseInteraction",.05f);}
 static int EventCrystals(){int n=0;foreach(object d in (IList)Get(Room((int)Get("canalEpilogueStart")+1),"essence"))if((float)Get(d,"value")==20)n++;return n;}

 static int Start{get{return (int)Get("canalEpilogueStart");}}
 static void Stairs(){Set("position",Get("stairPosition"));Check((bool)Call("TryRoomInteraction"),"F uses actual connecting stairs");for(int i=0;i<7;i++)Call("TickDoorTransition",.05f);Quiet();Check((bool)Call("SafeFooting",Get("position"),.34f),"Stair arrival is safe");}
 static void DefeatFinal(int variant)
 {
  p.bossOverride=variant;Fresh();Check(List("journeyRooms").Count==39,"Normal journey begins with original thirty-nine rooms");
  foreach(object n in List("journeyRooms"))Check((int)Get(n,"gatehouseIndex")==-1,"Pre-boss room has no canal identity");
  Enter(((int[])Get("stageEnds"))[2]);Set("health",4f);Set("shield",2f);Set("energy",47f);Set("elementPower",14f);Set("darkPower",19f);
  Set("orbLearned",true);Set("bladeLearned",false);Set("waveLearned",false);((int[])Get("carriedWeapons"))[0]=5;List("reserveWeapons").Add(2);
  object boss=List("enemies")[0];Check((int)Get(boss,"kind")==5+variant,"Actual selected final boss spawned");Call("DamageEnemy",boss,100000f);Call("TickJourney",.01f);
  Check(List("journeyRooms").Count==44&&Start==39,"Final boss appends five real canal rooms once");
  Check((bool)Get("desertStoneRecovered")&&(bool)Get("windEmpowered"),"Wind stone is recovered and wind empowered at boss defeat");
  Check(!(bool)Get("journeyComplete")&&!(bool)Get("pendingRainCG")&&!(bool)Get("showRainCG"),"Boss defeat does not finish chapter or prematurely play rain CG");
  Check(F("health")==4&&F("shield")==2&&F("energy")==47&&F("elementPower")==44&&F("darkPower")==19,"Boss reward and independent resources remain correct");
  Check(((int[])Get("carriedWeapons"))[0]==5&&List("reserveWeapons").Count==1,"Inventory is preserved at story boundary");
  Check(((string)Call("StairDestination")).Contains("归渠水道")&&Get("stairs")!=null,"Boss arena exposes a named physical canal entrance");
  Call("AdvanceJourneyStory",true);Check(!(bool)Get("showRainCG"),"Skipping boss dialogue still cannot play the final CG");
  float ep=F("elementPower");Call("CompleteJourneyBoss");Quiet();Check(List("journeyRooms").Count==44&&F("elementPower")==ep,"Repeated boss completion cannot duplicate rooms or reward");
  Stairs();Check((int)Get("currentRoom")==Start&&!(bool)Get("gatehouseSampleMode")&&(bool)Get("GatehouseActive"),"Normal gameplay enters canal without switching to sample mode");
  Check(F("energy")==47&&F("elementPower")==44&&F("darkPower")==19&&F("health")==4,"Transition keeps current player resources");
  Check((bool)Get("orbLearned")&&!(bool)Get("bladeLearned")&&!(bool)Get("waveLearned"),"Sample-only skill unlocks are not granted by integration");
  var world=(Transform)Get("world");Check(world.Find("Camp rug")==null&&world.Find("Camp wind emblem")==null,"Ambiguous blue rug and ring are removed");
  Check(((string)Call("StairDestination")).Contains("古渠核心"),"Canal entrance can return to the boss arena");
  Stairs();Check((int)Get("currentRoom")==((int[])Get("stageEnds"))[2]&&List("enemies").Count==0,"Returning never respawns boss");Stairs();
 }
 static void FullFlow(int variant)
 {
  DefeatFinal(variant);Go(0);Check((int)Get("currentRoom")==Start+1,"Normal walk reaches canal gatehouse");ClearGate();
  Go(0);Action(4);Quiet();Go(2);Action(5);Quiet();
  Check(!(bool)Get("journeyComplete")&&!(bool)Get("pendingRainCG"),"Bell rejects chapter completion while local canal remains clogged");
  Go(1);KillWave();Check((bool)Get(Room(Start+3),"cleared"),"Maintenance yard is cleared through the physical bypass");
  Check((bool)Call("GatehouseDoorLocked",Start+3,3),"Unrepaired shortcut remains closed even after yard clear");
  Go(0);Go(3);Go(1);Action(0);Finish();Action(1);Finish();Action(2);Finish();
  Check((bool)Get("gateRepairCommitted")&&EventCrystals()==1,"Repair reward belongs to appended room rather than old room one");
  Check(!(bool)Call("GatehouseDoorLocked",Start+1,2)&&!(bool)Get("journeyComplete"),"Shortcut opens before final chapter confirmation");
  Set("allowBatchCheckpoints",true);Set("checkpointPathOverride",Out+"checkpoint-"+variant+"/journey.json");Check((bool)Call("SaveCheckpoint",false),"Forty-four-room ending-area checkpoint is writable");
  Set("allowBatchCheckpoints",false);Fresh();Set("allowBatchCheckpoints",true);Check((bool)Call("LoadCheckpoint",false),"Ending-area checkpoint loads into a new normal run");Quiet();
  Check(List("journeyRooms").Count==44&&Start==39&&(bool)Get("gateRepairCommitted")&&EventCrystals()==1,"Canal graph, repair and uncollected reward survive disk save");Set("allowBatchCheckpoints",false);
  Go(2);Go(0);Action(5);
  Check((bool)Get("journeyComplete")&&(bool)Get("canalEpilogueComplete")&&(bool)Get("gateRainReturned"),"Wind stone plus repair, master valve and yard lead to chapter completion");
  Check((bool)Get("pendingRainCG")&&!(bool)Get("showRainCG"),"Final rain CG is queued after canal completion");
  Call("AdvanceJourneyStory",false);Check(!(bool)Get("showRainCG"),"First closing line precedes CG");Call("AdvanceJourneyStory",false);Check((bool)Get("showRainCG"),"CG plays after the rain story beat");
  Call("AdvanceJourneyStory",true);Set("showRainCG",false);Quiet();int restored=(int)Get("restoredCanals");float ep=F("elementPower");
  Action(5);Check(!(bool)Get("pendingRainCG")&&!(bool)Get("showRainCG")&&(int)Get("restoredCanals")==restored&&F("elementPower")==ep,"Repeated bell cannot replay CG or grant rewards");
  Go(1);Go(3);Check((int)Get("currentRoom")==Start+1&&((bool[])Get("gateSandCleared"))[0],"After CG the restored gatehouse remains visitable");
  int active=0;foreach(GameObject go in List("gateWaterVisuals"))if(go.activeSelf)active++;Check(active==9&&List("gateRainVisuals").Count==28,"Water and rain reconstruct after story return");
  Call("ResetRoom");Quiet();Check(List("journeyRooms").Count==39&&Start==-1&&!(bool)Get("desertStoneRecovered")&&!(bool)Get("journeyComplete"),"R starts a clean normal run without carrying ending state");
 }
 static void PrepareCapture()
 {
  DefeatFinal(1);Stairs();Set("storyLines",null);Set("position",Get("stairPosition"));Call("UpdatePixelHero",Vector2.zero);Set("roomBannerUntil",0f);Quiet();
 }
 static void Tick()
 {
  if(!EditorApplication.isPlaying||EditorApplication.isCompiling||++ticks<40)return;p=UnityEngine.Object.FindObjectOfType<NakiPrototype>();if(p==null||Get("world")==null)return;
  try{
   if(stage==0){p.enabled=false;for(int v=0;v<3;v++)FullFlow(v);Check(errors.Count==0,"Three final bosses integrate without runtime exceptions");PrepareCapture();p.enabled=true;stage++;ticks=0;return;}
   if(stage==1){ScreenCapture.CaptureScreenshot(Out+"canal-entrance.png");stage++;ticks=0;return;}
   if(stage==2){Stairs();Set("roomBannerUntil",0f);stage++;ticks=0;return;}
   if(stage==3){ScreenCapture.CaptureScreenshot(Out+"normal-canal.png");stage++;ticks=0;return;}
   if(stage==4){p.enabled=false;for(int i=1;i<5;i++)Set(Room(Start+i),"cleared",true);((bool[])Get("gateSandCleared"))[0]=true;((bool[])Get("gateSandCleared"))[1]=true;Set("gateRepairCommitted",true);Set("gateTotalUnlocked",true);Enter(Start+4);Action(5);Call("AdvanceJourneyStory",false);Call("AdvanceJourneyStory",false);p.enabled=true;stage++;ticks=0;return;}
   if(stage==5){ScreenCapture.CaptureScreenshot(Out+"final-rain-cg.png");stage++;ticks=0;return;}
   Check(errors.Count==0,"Normal entrance, fourth-area HUD and ending CG render without errors");File.WriteAllText(Out+"smoke-result.txt","PASS "+checks+" checks; three normal final bosses; 39 to 44 room integration; actual stairs and doors; inventory and resources retained; repair and two-end shortcut; CG delayed until canal completion; one-time ending; return visits and restart; real HD screenshots.\n");End(0);
  }catch(Exception e){Debug.LogException(e);File.WriteAllText(Out+"smoke-result.txt","FAIL after "+checks+" checks\n"+e+"\n"+string.Join("\n",errors));End(1);}
 }
 static void End(int code){SessionState.SetBool(Flag,false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;EditorApplication.Exit(code);}
}
