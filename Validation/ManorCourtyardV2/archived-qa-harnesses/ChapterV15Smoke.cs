using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NakiLegend.Prototype;

public static class NakiChapterV15Smoke
{
 const BindingFlags B=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
 const string Out="D:/NakiLegend/Repository/Validation/AdventureV15/chapter-runtime/",Flag="Naki.ChapterV15QA";
 static NakiPrototype p;static int ticks,stage,checks;static List<string> errors=new List<string>();
 [InitializeOnLoadMethod]static void Reload(){if(SessionState.GetBool(Flag,false))Attach();}
 public static void Run(){Directory.CreateDirectory(Out);AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);NakiLegend.Prototype.Editor.NakiSampleScenes.BuildThunderChapterScenes();EditorSceneManager.OpenScene(NakiLegend.Prototype.Editor.NakiSampleScenes.ChapterPath);var w=EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));w.Show();SetSize(w);SessionState.SetBool(Flag,true);Attach();EditorApplication.isPlaying=true;}
 static void SetSize(EditorWindow w){var a=typeof(Editor).Assembly;var t=a.GetType("UnityEditor.GameViewSizes");var singleton=typeof(ScriptableSingleton<>).MakeGenericType(t).GetProperty("instance",BindingFlags.Public|BindingFlags.Static).GetValue(null,null);var group=t.GetMethod("GetGroup").Invoke(singleton,new[]{Enum.Parse(a.GetType("UnityEditor.GameViewSizeGroupType"),"Standalone")});var size=Activator.CreateInstance(a.GetType("UnityEditor.GameViewSize"),new object[]{Enum.Parse(a.GetType("UnityEditor.GameViewSizeType"),"FixedResolution"),1920,1080,"Chapter QA"});group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});w.GetType().GetProperty("selectedSizeIndex",B).SetValue(w,(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null)-1,null);}
 static void Attach(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;Application.logMessageReceived-=Log;Application.logMessageReceived+=Log;}
 static void Log(string s,string trace,LogType t){if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert)errors.Add(s);}
 static object Get(object o,string n){var f=o.GetType().GetField(n,B);return f!=null?f.GetValue(o):o.GetType().GetProperty(n,B).GetValue(o,null);}static object Get(string n){return Get(p,n);}static void Set(string n,object v){typeof(NakiPrototype).GetField(n,B).SetValue(p,v);}static void Set(object o,string n,object v){o.GetType().GetField(n,B).SetValue(o,v);}
 static object Call(string n,params object[] v){return typeof(NakiPrototype).GetMethod(n,B).Invoke(p,v);}static IList List(string n){return (IList)Get(n);}static float F(string n){return (float)Get(n);}static object Room(int id){return List("journeyRooms")[id];}
 static object E(string name){return Enum.Parse(typeof(NakiPrototype).GetNestedType("EnemyKind",BindingFlags.NonPublic),name);}static object Element(string name){return Enum.Parse(typeof(NakiPrototype).GetNestedType("Element",BindingFlags.NonPublic),name);}
 static void Check(bool yes,string why){if(!yes)throw new Exception(why);checks++;Debug.Log("CHAPTER_PASS "+why);}
 static void Quiet(){Set("storyLines",null);Set("chapterSetup",false);Set("chapterSkillsMenu",false);Set("chapterRewardMenu",false);Set("chapterCg",false);Set("paused",false);Set("inventoryMenu",false);Set("mapMenu",false);Set("rewardMenu",false);Set("roomBannerUntil",0f);}
 static void Fresh(int floor=0,int boss=-1,NakiPrototype.ThunderPreset preset=NakiPrototype.ThunderPreset.Light){p.thunderChapterMode=true;p.thunderPrototypeMode=false;p.showOpeningOnStart=false;p.thunderChapterStartFloor=floor;p.thunderChapterPracticeBoss=boss;p.thunderPreset=preset;Call("BeginThunderChapter",false);Quiet();Set("invulnerableUntil",100000f);}
 static void Step(float seconds){for(int i=0;i<Mathf.CeilToInt(seconds/.05f);i++){Set("clock",F("clock")+.05f);Call("TickJourney",.05f);}}
 static void Clear(){int count=0;Quiet();while(!(bool)Get(Room((int)Get("currentRoom")),"cleared")&&count++<9){Step(2.5f);for(int i=List("enemies").Count-1;i>=0;i--)Call("DamageEnemy",List("enemies")[i],10000f);Step(1.3f);Quiet();}Check((bool)Get(Room((int)Get("currentRoom")),"cleared"),"Actual waves and death callbacks clear room "+Get("currentRoom"));}
 static void Enter(int id,int entry=-1){Call("EnterExplorationRoom",id,entry);Quiet();Set("invulnerableUntil",100000f);}
 static void WalkDoor(int from,int to)
 {
  object a=Room(from),b=Room(to);int d=(int)Call("NeighborDirection",a,b);Set("position",Call("DoorPoint",d,1.12f));Set("clock",F("clock")+2);Call("TryAutomaticExit");Check(F("doorTransition")>0,"Real chapter doorway starts transition");for(int i=0;i<7;i++)Call("TickDoorTransition",.05f);Quiet();Check((int)Get("currentRoom")==to&&(bool)Call("SafeFooting",Get("position"),.34f),"Door transition reaches safe target room");
 }
 static void Interact(Vector2 at){Set("position",at);Check((bool)Call("TryChapterInteraction"),"F finds chapter interaction");Quiet();}
 static void GraphAndCover()
 {
  for(int seed=1;seed<=8;seed++)
  {
   Set("activeSeed",seed);Fresh();Check(List("journeyRooms").Count==39,"39 room graph seed "+seed);int edges=0;var seen=new HashSet<int>();var queue=new Queue<int>();queue.Enqueue(0);
   while(queue.Count>0){int id=queue.Dequeue();if(!seen.Add(id))continue;object n=Room(id);foreach(int link in (IList)Get(n,"links")){queue.Enqueue(link);edges++;Vector2Int a=(Vector2Int)Get(n,"cell"),b=(Vector2Int)Get(Room(link),"cell");Check(Mathf.Abs(a.x-b.x)+Mathf.Abs(a.y-b.y)==1,"Rotated map links retain cardinal doors");}int stair=(int)Get(n,"stairsTo");if(stair>=0)queue.Enqueue(stair);}
   Check(seen.Count==39&&edges==82,"All floors connected with 41 corridors plus stairs");
   if(seed<=2)for(int id=0;id<39;id++)
   {
    object n=Room(id);var arrivals=new List<int>{-1};foreach(int link in (IList)Get(n,"links"))arrivals.Add((int)Call("NeighborDirection",n,Room(link)));
    foreach(int d in arrivals){Enter(id,d);Vector2 at=(Vector2)Get("position");Check((bool)Call("SafeFooting",at,.34f),"Safe chapter landing "+id+" / "+d);Vector2 inward=d>=0?-(Vector2)Call("DoorNormal",d):Vector2.up;Vector2 moved=(Vector2)Call("MoveInExploration",at,inward*.09f,.3f);Check((moved-at).magnitude>.04f,"Arrival can move away from closed gate");}
   }
  }
  Fresh();var art=(IDictionary)Get("chapterArt");foreach(string key in new[]{"warden","sentinel","stag","roller"})Check(((Sprite[])art[key]).Length==8,"Eight real animation poses "+key);
  var floors=(Sprite[])Get("chapterFloors");Check(floors[0]!=null&&floors[1]!=null&&floors[2]!=null&&floors[0].texture!=floors[1].texture&&floors[1].texture!=floors[2].texture,"Three floors load distinct terrain artwork");Check(((Array)Get("weaponCatalog")).Length==14,"Six chapter weapons extend original eight");
 }
 static void MechanismsAndSkills()
 {
  Fresh(1);Enter(14);Clear();Set("energy",53f);Set("darkPower",17f);Set("elementPower",100f);Interact(new Vector2(4.4f,2.1f));Check(F("chapterWork")>0,"Node repair begins without resource cost");Call("TickChapterOverlay",.5f);Set("chapterWork",0f);Check(!(bool)Get("chapterWest"),"Unfinished repair does not commit state");Interact(new Vector2(4.4f,2.1f));for(int i=0;i<25;i++)Call("TickChapterOverlay",.05f);
  Check((bool)Get("chapterWest")&&F("energy")==53&&F("darkPower")==17&&F("elementPower")==100,"Node repair commits free and preserves all power pools");int count=0;foreach(object e in (IList)Get(Room(14),"essence"))if((float)Get(e,"value")==20)count++;Check(count==1,"Node leaves one twenty-element reward");
  int gateDirection=(int)Call("NeighborDirection",Room(17),Room(18));Check((bool)Call("ChapterDoorLocked",17,gateDirection),"Sentinel route blocked until both nodes repaired");Enter(16);Clear();Interact(new Vector2(4.4f,2.1f));for(int i=0;i<25;i++)Call("TickChapterOverlay",.05f);Quiet();Check((bool)Get("chapterEast")&&!(bool)Call("ChapterDoorLocked",17,gateDirection),"Both nodes open summit route");
  Interact(new Vector2(-2.2f,-.8f));Check((bool)Get("chapterArcKnown"),"East node teaches arc without replacing current slots");int[] slots=(int[])Get("chapterSkillSlots");Check(slots[0]==0&&slots[1]==1,"Learning keeps existing wind configuration");Set("chapterArcReady",F("clock")+7);Call("EquipChapterSkill",3,0);Check(slots[0]==3&&F("chapterArcReady")>F("clock"),"Equipping arc preserves cooldown");Call("EquipChapterSkill",1,0);Check(slots[0]==1&&slots[1]==3,"Duplicate equip swaps the other slot");
  Set("chapterMarkKnown",true);Call("EquipChapterSkill",2,0);Set("chapterMarkReady",0f);Set("position",Vector2.zero);List("projectileObstacles").Clear();float ep=F("elementPower");Call("CastChapterSkill",0);Check(F("elementPower")==ep&&List("chapterMarks").Count==0,"No-target mark does not charge energy");
  object target=Call("SpawnJourneyEnemy",E("StormGuard"),new Vector2(1,0),0);Set(target,"hp",1000f);Set(target,"maximum",1000f);Call("CastChapterSkill",0);Check(List("chapterMarks").Count==1&&F("elementPower")==ep-8,"Mark costs eight elemental power and locks old position");Set(target,"position",new Vector2(4,0));Set("clock",F("clock")+1);Call("TickChapterSkills");Check((float)Get(target,"hp")==1000,"Moving out of locked mark avoids damage");Call("ClearThunderHazards");List("enemies").Clear();
  Enter(16);List("projectileObstacles").Clear();Set("position",Vector2.zero);Set("chapterArcKnown",true);Call("EquipChapterSkill",3,0);Set("chapterArcReady",0f);Set("elementPower",100f);
  object first=Call("SpawnJourneyEnemy",E("StormGuard"),new Vector2(1,0),0),second=Call("SpawnJourneyEnemy",E("StormGuard"),new Vector2(2,.8f),0),third=Call("SpawnJourneyEnemy",E("StormGuard"),new Vector2(2.4f,.1f),0);foreach(object e in List("enemies")){Set(e,"hp",1000f);Set(e,"maximum",1000f);}
  Call("CastChapterSkill",0);Check(List("shots").Count==1&&F("elementPower")==90,"Arc creates one projectile and spends ten elemental power");Call("UpdateShot",0,.05f);Check(Mathf.Abs((float)Get(first,"hp")-977)<.01f&&Mathf.Abs((float)Get(second,"hp")-986.2f)<.01f&&(float)Get(third,"hp")==1000,"Actual strengthened arc hits first target and leaps exactly once");
  List("projectileObstacles").Add(new Rect(1.4f,-1,.2f,3));Set("chapterArcReady",0f);Call("CastChapterSkill",0);Call("UpdateShot",0,.05f);Check(Mathf.Abs((float)Get(first,"hp")-954)<.01f&&Mathf.Abs((float)Get(second,"hp")-986.2f)<.01f&&(float)Get(third,"hp")==1000,"Solid cover blocks secondary arc targets");
  Enter(9);Clear();foreach(float x in new[]{-3f,0,3f}){Vector2 at=x==-3?new Vector2(-3,1):x==0?new Vector2(0,1.4f):new Vector2(3,-.8f);Interact(at);}Check((int)Get("chapterWaymarks")==7&&(bool)Get(Room(9),"claimed"),"Three waymarkers persist as one completed event");
  Enter(11);Call("ClaimChapterWeapon",1);Check((bool)Get(Room(11),"claimed")&&((IList)Get(Room(11),"drops")).Count==1,"Reward places selected weapon once");Call("ClaimChapterWeapon",0);Check(((IList)Get(Room(11),"drops")).Count==1,"Reward cannot be duplicated");
 }
 static void BossesAndPersistence()
 {
  foreach(string name in new[]{"CopperWarden","ThunderSentinel","StormStag"})
  {
   int id=name=="CopperWarden"?0:name=="ThunderSentinel"?1:2;Fresh(0,id,NakiPrototype.ThunderPreset.Dark);object e=List("enemies")[0];Check((bool)Get(e,"boss")&&Get(e,"kind").ToString()==name,"Practice starts requested boss");
   Check((float)Get(e,"maximum")==new[]{360f,620,1250}[id],"Stronger second-island boss health budget");
   for(int move=0;move<3;move++){Call("BeginChapterBossMove",e);Check((float)Get(e,"windup")>=.7f,"Boss move has readable windup");Call("ResolveChapterBossMove",e);}
   Set(e,"hp",(float)Get(e,"maximum")*.4f);Call("UpdateChapterBoss",e,.05f);Check((bool)Get(e,"secondPhase")&&(float)Get(e,"phaseUntil")>F("clock"),"Threshold enters protected second-phase transition");
   Set(e,"hp",1000f);Set(e,"maximum",1000f);Set(e,"phaseUntil",0f);Set(e,"windup",2f);float before=(float)Get(e,"hp");for(int j=0;j<3;j++)Call("ApplyElementDamage",e,10f,Element("Wind"));Check(Mathf.Abs((float)Get(e,"hp")-(before-39))<.01f&&(float)Get(e,"windup")==0,"Wind multiplier and third-hit break work on chapter boss");
   float soul=F("darkPower");Call("DamageEnemy",e,10000f);Step(.1f);Quiet();Check(List("enemies").Count==0&&List("shots").Count==0&&List("thunderStrikes").Count==0,"Boss defeat clears hostile remnants");if(id==2)Check((bool)Get("chapterStone")&&F("darkPower")==soul,"Living guardian yields one thunder fragment and no soul charge");
  }
  Set("allowBatchCheckpoints",true);Set("chapterCheckpointPathOverride",Out+"save/chapter.json");Set("checkpointPathOverride",Out+"save/desert.json");Set("thunderCheckpointPathOverride",Out+"save/prototype.json");Directory.CreateDirectory(Out+"save");File.WriteAllText(Out+"save/desert.json","DESERT_UNTOUCHED");File.WriteAllText(Out+"save/prototype.json","PROTOTYPE_UNTOUCHED");
  Fresh(2,-1,NakiPrototype.ThunderPreset.Dark);Set("health",4f);Set("darkPower",47f);Set("elementPower",39f);Set("chapterArcReady",F("clock")+3);List("reserveWeapons").Add(13);Call("ChapterDrop",12,new Vector2(1,-1));Check((bool)Call("SaveChapterCheckpoint",false),"Real full-chapter checkpoint writes safely");string path=(string)Get("ChapterCheckpointPath"),original=File.ReadAllText(path);Enter(26);Check(!(bool)Call("SaveChapterCheckpoint",false)&&File.ReadAllText(path)==original,"Combat cannot overwrite checkpoint");
  Check((bool)Call("LoadChapterCheckpoint",false)&&F("health")==4&&F("darkPower")==47&&F("elementPower")==39&&(int)Get("currentRoom")==25,"Full save restores floor, dark origin and independent resources");Check((bool)Get("chapterWest")&&(bool)Get("chapterEast")&&(bool)Get("chapterArcKnown")&&List("reserveWeapons").Count==1&&((IList)Get(Room(25),"drops")).Count==1&&F("chapterArcReady")>F("clock"),"Nodes, skill knowledge, weapons, ground loot and cooldown persist");
  Call("SaveChapterCheckpoint",false);File.WriteAllText(path,"{broken");Check((bool)Call("LoadChapterCheckpoint",false),"Corrupt primary checkpoint recovers backup");File.WriteAllText(path,"{broken");File.WriteAllText(path+".bak","{broken");float health=F("health");Check(!(bool)Call("LoadChapterCheckpoint",false)&&F("health")==health,"Invalid files do not mutate current game");
  Fresh(0,2);Check(!(bool)Call("SaveChapterCheckpoint",false),"Boss practice cannot overwrite full chapter checkpoint");Check(File.ReadAllText(Out+"save/desert.json")=="DESERT_UNTOUCHED"&&File.ReadAllText(Out+"save/prototype.json")=="PROTOTYPE_UNTOUCHED","Desert and S1 saves stay untouched");Set("allowBatchCheckpoints",false);
 }
 static void FullJourney()
 {
  Fresh();for(int id=0;id<39;id++){Enter(id);Clear();if(id==14||id==16){Interact(new Vector2(4.4f,2.1f));for(int j=0;j<25;j++)Call("TickChapterOverlay",.05f);Quiet();}}
  Check((bool)Get("chapterRanger")&&(bool)Get("chapterStone")&&List("journeyRooms").Count==39,"All 39 rooms and three bosses complete without appending desert content");Enter(31);Interact(new Vector2(-2.5f,-1.5f));Check((int)Get("currentRoom")==0,"Return windbell travels back to lampkeeper");Set("position",new Vector2(-3.4f,1.4f));Call("TryChapterInteraction");Check((bool)Get("chapterCg"),"Fragment return triggers chapter ending CG");
  Call("FinishChapterEnding");Check((bool)Get("chapterEnding")&&!(bool)Get("chapterCg"),"Finishing CG enables post-chapter return action");
  for(int id=0;id<39;id++)
  {
   Enter(id);foreach(int link in new List<int>((List<int>)Get(Room(id),"links")))
   {
    int d=(int)Call("NeighborDirection",Room(id),Room(link));if((bool)Call("ChapterDoorLocked",id,d))continue;WalkDoor(id,link);WalkDoor(link,id);
   }
   int stair=(int)Get(Room(id),"stairsTo");if(stair>=0){Set("position",Get("stairPosition"));Check((bool)Call("TryRoomInteraction"),"F uses actual chapter staircase");for(int i=0;i<7;i++)Call("TickDoorTransition",.05f);Quiet();Check((int)Get("currentRoom")==stair,"Chapter staircase goes to correct floor");}
  }
 }
 static void CrossIsland()
 {
  p.thunderChapterMode=p.thunderPrototypeMode=p.bossPracticeMode=p.gatehouseSampleMode=p.lightningTrialMode=false;Set("allowBatchCheckpoints",true);Set("checkpointPathOverride",Out+"travel/desert.json");Set("chapterCheckpointPathOverride",Out+"travel/chapter.json");Call("BeginJourney",false);Call("ChooseOrigin",1);Quiet();foreach(object n in List("journeyRooms"))Set(n,"cleared",true);Call("UnlockCanalEpilogue",Room(31));Set("gateRepairCommitted",true);Set("gateTotalUnlocked",true);Set("canalEpilogueComplete",true);Set("gateRainReturned",true);Enter(43);Set("health",5f);Set("darkPower",52f);Set("elementPower",44f);Set("swordDamage",F("initialSwordDamage")+4);Set("spellDamage",F("initialSpellDamage")+6);int[] carried=(int[])Get("carriedWeapons");carried[0]=5;carried[1]=6;Set("orbLearned",true);Set("bladeLearned",true);Set("waveLearned",true);
  Check((bool)Call("SaveCheckpoint",false),"Desert departure saves prior island first");Check(((Transform)Get("world")).Find("Boat to Thunder Plateau")!=null,"Completed canal lookout contains departure marker");Set("position",new Vector2(-4.8f,-1.5f));Check((bool)Call("TryRoomInteraction"),"F at lookout windbell boards second-island boat");Quiet();
  Check(p.thunderChapterMode&&List("journeyRooms").Count==39&&(int)Get("currentRoom")==0,"Actual boat enters full thunder chapter");Check(Get("origin").ToString()=="Dark"&&F("health")==5&&F("darkPower")==52&&F("elementPower")==44,"Boat carries original origin and resources");Check(((int[])Get("carriedWeapons"))[0]==5&&F("swordDamage")==F("initialSwordDamage")+4&&F("spellDamage")==F("initialSpellDamage")+6,"Boat carries equipped weapons and upgrade bonuses");
  Check((bool)Call("SaveChapterCheckpoint",false),"Arrival writes independent chapter checkpoint");Call("ReturnToDesertCheckpoint");Quiet();Check(!p.thunderChapterMode&&List("journeyRooms").Count==44&&(int)Get("currentRoom")==43&&(bool)Get("canalEpilogueComplete"),"Return restores completed desert checkpoint");Call("EmbarkThunderChapter");Quiet();Check(p.thunderChapterMode&&F("darkPower")==52&&((int[])Get("carriedWeapons"))[0]==5,"Second boarding resumes chapter checkpoint");Set("allowBatchCheckpoints",false);
 }
 static void Tick()
 {
  if(!EditorApplication.isPlaying||EditorApplication.isCompiling||++ticks<30)return;p=UnityEngine.Object.FindObjectOfType<NakiPrototype>();if(p==null||Get("world")==null)return;
  try
  {
   if(stage==0){p.enabled=false;GraphAndCover();MechanismsAndSkills();BossesAndPersistence();FullJourney();CrossIsland();Fresh();Set("chapterSetup",true);p.enabled=true;stage++;ticks=0;return;}
   if(stage==1){ScreenCapture.CaptureScreenshot(Out+"chapter-setup.png");stage++;ticks=0;return;}
   if(stage==2){p.enabled=false;Fresh();Call("UpdatePixelHero",Vector2.zero);p.enabled=true;stage++;ticks=0;return;}
   if(stage==3){ScreenCapture.CaptureScreenshot(Out+"floor1-lampkeeper.png");stage++;ticks=0;return;}
   if(stage==4){p.enabled=false;Fresh(1);Enter(14);Step(2.5f);Call("UpdatePixelHero",Vector2.zero);p.enabled=true;stage++;ticks=0;return;}
   if(stage==5){ScreenCapture.CaptureScreenshot(Out+"floor2-nodes.png");stage++;ticks=0;return;}
   if(stage==6){p.enabled=false;Fresh(0,2);object e=List("enemies")[0];Call("UpdateChapterEnemyPose",e);Call("UpdatePixelHero",Vector2.zero);p.enabled=true;stage++;ticks=0;return;}
   if(stage==7){ScreenCapture.CaptureScreenshot(Out+"floor3-stag.png");stage++;ticks=0;return;}
   if(stage==8){p.enabled=false;Fresh();Set("chapterCg",true);p.enabled=true;stage++;ticks=0;return;}
   if(stage==9){ScreenCapture.CaptureScreenshot(Out+"ending-cg.png");stage++;ticks=0;return;}
   if(stage==10){p.enabled=false;Fresh(2);Set("chapterSkillsMenu",true);p.enabled=true;stage++;ticks=0;return;}
   if(stage==11){ScreenCapture.CaptureScreenshot(Out+"skills.png");stage++;ticks=0;return;}
   Check(errors.Count==0,"Native scenes, game HUD, dialogue, skills and ending render without exceptions");File.WriteAllText(Out+"smoke-result.txt","PASS "+checks+" checks: 39-room seeded graphs and arrivals, three terrain sets, real actor frames, free two-node repair, gate prerequisites, rewards, skills and cooldowns, three phased bosses, elemental break, guardian survival, isolated JSON checkpoints and corruption recovery, full chapter progression and native 1920x1080 captures.\n");End(0);
  }catch(Exception e){Debug.LogException(e);File.WriteAllText(Out+"smoke-result.txt","FAIL after "+checks+"\n"+e+"\n"+string.Join("\n",errors));End(1);}
 }
 static void End(int code){SessionState.SetBool(Flag,false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;EditorApplication.Exit(code);}
}
