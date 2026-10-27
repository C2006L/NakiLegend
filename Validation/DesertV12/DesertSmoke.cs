using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using NakiLegend.Prototype;
public static class NakiDesertV12Smoke
{
 const BindingFlags B=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;const string Flag="NakiV12.Smoke";
 static readonly string Out="D:/NakiLegend/Repository/Validation/DesertV12/runtime/";static NakiPrototype p;static int ticks,checks,stage;static List<string> errors=new List<string>();
 [InitializeOnLoadMethod]static void Reload(){if(SessionState.GetBool(Flag,false)){EditorApplication.update+=Tick;Application.logMessageReceived+=Log;}}
 public static void Run(){Directory.CreateDirectory(Out);EditorSceneManager.OpenScene("Assets/Scenes/NakiTest.unity");UnityEngine.Object.FindObjectOfType<NakiPrototype>().showOpeningOnStart=false;var window=EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));window.Show();SetSize(window);SessionState.SetBool(Flag,true);EditorApplication.update-=Tick;EditorApplication.update+=Tick;Application.logMessageReceived+=Log;EditorApplication.isPlaying=true;}
 static void SetSize(EditorWindow window){Assembly a=typeof(Editor).Assembly;Type t=a.GetType("UnityEditor.GameViewSizes");object singleton=typeof(ScriptableSingleton<>).MakeGenericType(t).GetProperty("instance",BindingFlags.Static|BindingFlags.Public).GetValue(null,null);object group=t.GetMethod("GetGroup").Invoke(singleton,new[]{Enum.Parse(a.GetType("UnityEditor.GameViewSizeGroupType"),"Standalone")});object size=Activator.CreateInstance(a.GetType("UnityEditor.GameViewSize"),new object[]{Enum.Parse(a.GetType("UnityEditor.GameViewSizeType"),"FixedResolution"),1920,1080,"Naki V12 QA"});group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});window.GetType().GetProperty("selectedSizeIndex",B).SetValue(window,(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null)-1,null);}
 static void Log(string s,string trace,LogType t){if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert)errors.Add(s);}
 static object Get(object o,string n){return o.GetType().GetField(n,B).GetValue(o);}static object Get(string n){return Get(p,n);}static void Set(object o,string n,object v){o.GetType().GetField(n,B).SetValue(o,v);}static void Set(string n,object v){Set(p,n,v);}static object Call(string n,params object[] args){return typeof(NakiPrototype).GetMethod(n,B).Invoke(p,args);}static IList List(string n){return (IList)Get(n);}static float F(string n){return (float)Get(n);}static int I(string n){return (int)Get(n);}static object E(string type,string name){return Enum.Parse(typeof(NakiPrototype).GetNestedType(type,BindingFlags.NonPublic),name);}
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);checks++;Debug.Log("NAKI_V12_PASS "+message);}static void Quiet(){Call("StopOpeningCinematic");Set("storyLines",null);Set("mapMenu",false);Set("rewardMenu",false);Set("inventoryMenu",false);Set("showRainCG",false);Set("paused",false);}
 static void ClearWorld(){Transform old=(Transform)Get("world");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);}
 static void Enter(int id,int entry=-1){ClearWorld();Call("EnterExplorationRoom",id,entry);Quiet();}
 static void Fresh(){p.bossPracticeMode=false;p.lightningTrialMode=false;p.showOpeningOnStart=false;ClearWorld();Call("BeginJourney",false);Quiet();}
 static void TimeAt(float t){Set("clock",t);Set("invulnerableUntil",0f);Set("dashUntil",0f);Set("swordUntil",0f);Set("castPoseUntil",0f);Set("nextSword",0f);Set("nextSpell",0f);Set("bladeReadyAt",0f);Set("waveReadyAt",0f);}
 static void ClearColliders(){((IList)Get("obstacles")).Clear();((IList)Get("projectileObstacles")).Clear();}
 static bool Reach(Vector2 start,Vector2 goal)
 {
  const float step=.28f;var q=new Queue<Vector2Int>();var seen=new HashSet<Vector2Int>();q.Enqueue(Vector2Int.RoundToInt(start/step));int tries=0;
  while(q.Count>0&&tries++<24000){Vector2Int c=q.Dequeue();if(!seen.Add(c))continue;Vector2 at=(Vector2)c*step;if(Vector2.Distance(at,goal)<.38f)return true;foreach(var d in new[]{Vector2Int.up,Vector2Int.down,Vector2Int.right,Vector2Int.left}){Vector2Int next=c+d;Vector2 pt=(Vector2)next*step;if(!seen.Contains(next)&&(bool)Call("CanStandInExploration",pt,.3f)&&!(bool)Call("Blocked",pt,.3f))q.Enqueue(next);}}return false;
 }
 static void GraphAndArrivals()
 {
  bool connected=true,cardinal=true;var layouts=new HashSet<int>();int arrivals=0;
  for(int seed=1;seed<=50;seed++){Call("BuildSpatialGraph",new System.Random(seed));var seen=new HashSet<int>();var q=new Queue<int>();q.Enqueue(0);while(q.Count>0){int id=q.Dequeue();if(!seen.Add(id))continue;object room=List("journeyRooms")[id];foreach(int n in (List<int>)Get(room,"links")){q.Enqueue(n);Vector2Int a=(Vector2Int)Get(room,"cell"),b=(Vector2Int)Get(List("journeyRooms")[n],"cell");cardinal&=Mathf.Abs(a.x-b.x)+Mathf.Abs(a.y-b.y)==1;}int stair=(int)Get(room,"stairsTo");if(stair>=0)q.Enqueue(stair);}connected&=seen.Count==39;}
  Check(connected&&cardinal,"50 seeded graphs connect all 39 rooms and preserve cardinal doors");
  for(int seed=1;seed<=12;seed++)
  {
   Call("BuildSpatialGraph",new System.Random(seed));
   for(int id=0;id<39;id++)
   {
    object node=List("journeyRooms")[id];layouts.Add((int)Get(node,"layout"));var entries=new List<int>{-1};foreach(int link in (List<int>)Get(node,"links"))entries.Add((int)Call("NeighborDirection",node,List("journeyRooms")[link]));
    foreach(int entry in entries)
    {
     Enter(id,entry);Vector2 at=(Vector2)Get("position");
     Check((bool)Call("SafeFooting",at,.34f),"Safe actual arrival seed="+seed+" room="+id+" entry="+entry);
     Vector2 inward=entry>=0?-(Vector2)Call("DoorNormal",entry):Vector2.up;
     Vector2 moved=(Vector2)Call("MoveInExploration",at,inward*.09f,.3f);Check((moved-at).magnitude>.045f,"Player can move away from sealed arrival");arrivals++;
    }
    if(seed<=3)
    {
     Set(node,"cleared",true);Call("UpdateExplorationDoors");Vector2 at=(Vector2)Get("position");
     foreach(int link in (List<int>)Get(node,"links")){int d=(int)Call("NeighborDirection",node,List("journeyRooms")[link]);Check(Reach(at,(Vector2)Call("DoorPoint",d,1.1f)),"Open door is reachable from real arrival");}
     if(Get("stairs")!=null)Check(Reach(at,(Vector2)Get("stairPosition")),"Staircase landing is reachable");
    }
   }
  }
  Check(layouts.Count>=60,"Actual arrival coverage includes at least sixty layout variants");File.WriteAllText(Out+"navigation-summary.txt",arrivals+" actual arrival configurations across 12 seeds; 50 graph seeds; all gates/stairs traversed on 3 full graphs.\n");
 }
 static object Spawn(string kind,Vector2 at){return Call("SpawnJourneyEnemy",E("EnemyKind",kind),at,5);}
 static void PowersAndAim()
 {
  Fresh();ClearColliders();TimeAt(100);Set("position",Vector2.zero);Set("facing",Vector2.right);Set("energy",70f);Set("elementPower",60f);Set("darkPower",37f);Set("orbLearned",true);Set("bladeLearned",true);Set("waveLearned",true);
  object enemy=Spawn("SandScorpion",new Vector2(-3,0));Call("CastWindBlade");Check(F("elementPower")==44&&F("energy")==70&&F("darkPower")==37,"Wind blade consumes only sixteen elemental power");object shot=List("shots")[0];Check(Vector2.Dot((Vector2)Get(shot,"direction"),Vector2.left)>.99f,"Wind blade auto-aims behind walking direction");
  Call("CastWindWave");Check(F("elementPower")==20&&F("energy")==70&&F("darkPower")==37,"Wind wave consumes only twenty-four elemental power");Call("CastJourneyOrb");Check(F("energy")==50&&F("elementPower")==20,"Light orb has independent light cost");Check(((Vector2)Get(List("shots")[1],"direction")).x<0,"Light orb auto-targets across 360 degrees");
  float mana=F("energy");Call("CastJourneyOrb");Check(F("energy")==mana,"Existing single orb prevents another cost/projectile");
  int[] carried=(int[])Get("carriedWeapons");carried[0]=3;Set("equippedSlot",0);Set("energy",60f);Set("elementPower",50f);List("shots").Clear();TimeAt(120);Call("UseEquippedWeapon");Check(F("elementPower")==48&&F("energy")==60,"Basic staff spends elemental power rather than light");Check(((Vector2)Get(List("shots")[0],"direction")).x<-.99f,"Staff automatically aims behind player");
  carried[0]=5;TimeAt(125);Call("UseEquippedWeapon");Check(F("elementPower")==47&&F("energy")==60,"Heavy sword uses its stated elemental cost");
  carried[0]=0;Set("elementPower",0f);TimeAt(130);Call("UseEquippedWeapon");Check(F("swordUntil")>F("clock")&&F("elementPower")==0,"Free sword stays usable with no elemental power");
  Set("combatTarget",null);((IList)Get("projectileObstacles")).Add(new Rect(-2,-1,1,2));object visible=Spawn("SandKite",new Vector2(2,2));Check(ReferenceEquals(Call("AutoTarget",10f),visible),"Aim rejects targets through solid cover");ClearColliders();
  List("enemies").Clear();Set("combatTarget",null);Set("elementPower",40f);Set("energy",61f);Set("darkPower",29f);object killed=Spawn("SandScorpion",new Vector2(2,0));Call("DamageEnemy",killed,10000f);Check(List("essenceDrops").Count==1&&F("energy")==61&&F("darkPower")==29,"Enemy death drops a crystal without granting light or dark power");
  object drop=List("essenceDrops")[0];Set("position",Get(Get(drop,"data"),"at"));TimeAt(140);Call("TickEssence",.02f);Check(F("elementPower")==48&&List("essenceDrops").Count==0,"Nearby crystal automatically restores eight elemental power");Call("DamageEnemy",killed,1000f);Check(List("essenceDrops").Count==0,"Dead enemy cannot create duplicate crystals");
  Set("elementPower",100f);killed=Spawn("SandScorpion",new Vector2(2,1));Call("DamageEnemy",killed,10000f);int room=(int)Get("currentRoom");Check(List("essenceDrops").Count==1,"Full resource leaves crystal on floor");Enter(1);Enter(room);Check(List("essenceDrops").Count==1,"Uncollected crystals persist when revisiting room");
 }
 static void AddWeapon(int id){Call("StoreGroundWeapon",id);IList list=List("worldWeapons");Call("PickUpWeapon",list[list.Count-1]);}
 static void BackpackAndStairs()
 {
  Fresh();AddWeapon(3);Check(((int[])Get("carriedWeapons"))[1]==3,"Second pickup fills carried slot");foreach(int id in new[]{1,2,4,6})AddWeapon(id);Check(List("reserveWeapons").Count==4,"Four reserve weapons fit in backpack");AddWeapon(7);Check((bool)Get("inventoryMenu")&&Get("pendingWeaponPickup")!=null,"Full inventory requests explicit replacement");Call("ReplaceWithPendingPickup");Check(((int[])Get("carriedWeapons"))[1]==7&&List("worldWeapons").Count==1,"Replacing leaves previous equipped weapon on floor");
  Set("bagSelection",2);Call("EquipBagWeapon",0);Check(((int[])Get("carriedWeapons"))[0]==1&&List("reserveWeapons").Count==4,"Backpack equips into chosen carried slot and retains old item");Set("bagSelection",0);Call("DropBagWeapon");Check(((int[])Get("carriedWeapons"))[0]==-1,"Dropping a carried weapon frees its slot");AddWeapon(2);Check(((int[])Get("carriedWeapons"))[0]==2,"Pickup fills either empty carried slot");
  Set("health",3f);Set("healingSupplies",1);Call("UseSupply",0);Check(F("health")==5&&I("healingSupplies")==0,"Healing supply restores two health and consumes one item");Set("health",6f);Set("healingSupplies",1);Call("UseSupply",0);Check(I("healingSupplies")==1,"Full health does not waste potion");Set("elementPower",10f);Set("elementSupplies",1);Call("UseSupply",1);Check(F("elementPower")==50&&I("elementSupplies")==0,"Element supply restores forty elemental power");
  Set("inventoryMenu",true);Set("energy",52f);Set("clock",200f);Call("Update");Check(F("clock")==200&&F("energy")==52,"Backpack pauses combat and resource clocks");Set("inventoryMenu",false);
  int[] ends=(int[])Get("stageEnds");Enter(ends[0]);object n=List("journeyRooms")[ends[0]];Set(n,"cleared",true);Call("UpdateExplorationDoors");Check(Get("stairs")!=null&&((string)Call("StairDestination")).Contains("第 2 层"),"Guardian stair clearly names second floor");
  Set("position",Get("stairPosition"));Set("clock",300f);Call("TryAutomaticExit");Check(F("doorTransition")==0,"Walking near stairs alone does not change floors");Set("elementPower",33f);Set("energy",44f);Set("darkPower",22f);Call("TryRoomInteraction");Check(I("transitionTarget")==((int[])Get("stageStarts"))[1],"F at stairs starts second-floor transition");for(int i=0;i<7;i++)Call("TickDoorTransition",.05f);Check((int)Get(List("journeyRooms")[I("currentRoom")],"stage")==1&&(bool)Call("SafeFooting",Get("position"),.34f),"Second-floor arrival is safe and movable");Check(F("elementPower")==33&&F("energy")==44&&F("darkPower")==22,"Crossing floors preserves all independent resources");
 }
 static void Bosses()
 {
  for(int variant=0;variant<3;variant++)
  {
   Fresh();p.bossPracticeMode=true;p.bossOverride=variant;ClearWorld();Call("BeginJourney",false);Quiet();object e=List("enemies")[0];
   for(int a=0;a<3;a++){TimeAt(400+a*3);Set(e,"attackNumber",a);Call("BeginJourneyAttack",e);Check(Get(e,"warning")!=null&&!string.IsNullOrEmpty((string)Call("EnemyMoveName",e)),"Boss pattern has readable warning and move name");Call("ResolveJourneyAttack",e);}
   Set(e,"hp",(float)Get(e,"maximum")*.4f);TimeAt(420);Call("UpdateJourneyEnemy",e,.01f);Check((bool)Get(e,"secondPhase")&&(float)Get(e,"phaseUntil")>420,"Boss phase transition grants a readable pause");
   Call("DamageEnemy",e,100000f);Call("TickJourney",.01f);Check((bool)Get("journeyComplete")&&List("groundShocks").Count==0&&List("pendingSpawns").Count==0,"Boss death clears remaining hazards and queued summons");
  }
  Fresh();ClearColliders();TimeAt(500);object elite=Spawn("SandKite",new Vector2(3,0));Set(List("journeyRooms")[I("currentRoom")],"stage",1);Call("MakeChampion",elite);Check((bool)Get(elite,"elite")&&Get(elite,"affinity").ToString()=="Lightning","Late elite wind bird gains distinct lightning identity");Set(elite,"windup",1f);Set(elite,"hp",1000f);Call("ApplyElementDamage",elite,1f,E("Element","Wind"));Check((float)Get(elite,"windup")>0,"Elite resists ordinary single-hit stagger");for(int i=0;i<2;i++)Call("ApplyElementDamage",elite,1f,E("Element","Wind"));Check((float)Get(elite,"windup")==0,"Wind blade's elemental interaction can interrupt lightning elite");
 }
 static void RemoveFixtureEnemies(){foreach(object e in List("enemies")){foreach(string field in new[]{"root","shadow","warning","armorSigil"}){var go=Get(e,field) as GameObject;if(go!=null)UnityEngine.Object.DestroyImmediate(go);}}List("enemies").Clear();List("pendingSpawns").Clear();}
 static void MapPose(int floor){Quiet();p.bossPracticeMode=false;Fresh();int id=((int[])Get("stageStarts"))[floor];ScenePose(id);RemoveFixtureEnemies();Set(List("journeyRooms")[id],"cleared",true);Call("UpdateExplorationDoors");Set("nextWaveAt",-1f);Set("position",Vector2.zero);Call("UpdatePixelHero",Vector2.zero);Set("paused",false);}
 static void DeathAndRecovery()
 {
  Fresh();TimeAt(600);Set("shield",0f);Call("TakeJourneyDamage",100f,E("Element","Physical"));Check(F("health")==0,"Actual lethal damage reaches defeated state");Quiet();
  foreach(Vector2 face in new[]{Vector2.up,Vector2.down,Vector2.left,Vector2.right}){Set("facing",face);for(int i=0;i<90;i++)Call("Update");Check(Get("currentHeroAction").ToString()=="Defeated","Death animation persists without exceptions in each direction");}
  Call("ResetRoom");Quiet();Check(F("health")==6&&Get("currentHeroAction").ToString()=="Idle","Restart after death restores playable hero");Check(F("flashUntil")==0,"Restart clears old-clock damage tint");
  Set("health",3f);Set("clock",625f);Call("OnEnable");Check(F("health")==3&&F("clock")==625,"Ordinary component re-enable preserves healthy runtime state");
  ((IDictionary)Get("heroFrames")).Clear();List("journeyRooms").Clear();Set("weaponCatalog",null);Set("health",0f);Call("OnEnable");Quiet();
  Check(((IDictionary)Get("heroFrames")).Count==7&&List("journeyRooms").Count==39&&Get("weaponCatalog")!=null,"Lost animation, room and weapon caches rebuild together");Check(F("health")==6,"Editor reload recovery restarts the incomplete run explicitly");
  for(int i=0;i<90;i++)Call("Update");Check(errors.Count==0,"Death, restart and missing-cache recovery have no runtime exceptions");
 }
 static void ScenePose(int room){p.enabled=false;Enter(room);Set("position",Vector2.zero);Set("facing",Vector2.right);Call("UpdatePixelHero",Vector2.zero);Set("roomBannerUntil",0f);p.enabled=true;Set("paused",true);}
 static void Tick()
 {
  if(!EditorApplication.isPlaying||EditorApplication.isCompiling||++ticks<40)return;p=UnityEngine.Object.FindObjectOfType<NakiPrototype>();if(p==null||Get("world")==null)return;
  try
  {
   if(stage==0){p.enabled=false;Quiet();Check(errors.Count==0,"V12 scene initializes without runtime errors");GraphAndArrivals();PowersAndAim();BackpackAndStairs();Bosses();DeathAndRecovery();Fresh();ScenePose(0);Set("paused",false);Set("storyLines",new[]{"古井碑：让内心的光在掌心汇聚，化作一颗元气弹。"});stage=1;ticks=0;return;}
   if(stage==1){ScreenCapture.CaptureScreenshot(Out+"hud-dialogue.png");stage++;ticks=0;return;}
   if(stage==2){Quiet();Set("inventoryMenu",true);Set("energy",65f);Set("elementPower",42f);int[] ids=(int[])Get("carriedWeapons");ids[0]=0;ids[1]=6;List("reserveWeapons").Clear();foreach(int n in new[]{1,2,4,5})List("reserveWeapons").Add(n);Set("bagSelection",4);stage++;ticks=0;return;}
   if(stage==3){ScreenCapture.CaptureScreenshot(Out+"backpack.png");stage++;ticks=0;return;}
   if(stage==4){Quiet();ScenePose(((int[])Get("stageStarts"))[1]);Set("paused",false);Set("mapMenu",true);stage++;ticks=0;return;}
   if(stage==5){ScreenCapture.CaptureScreenshot(Out+"map-floor2.png");stage++;ticks=0;return;}
   if(stage==6){Quiet();int id=((int[])Get("stageEnds"))[0];ScenePose(id);RemoveFixtureEnemies();Set(List("journeyRooms")[id],"cleared",true);Call("UpdateExplorationDoors");Set("position",(Vector2)Get("stairPosition")+Vector2.down*.7f);Call("UpdatePixelHero",Vector2.zero);Set("paused",false);Set("storyLines",new[]{"石阶已开启。靠近按 F，前往第2层 · 断渠遗迹。"});stage++;ticks=0;return;}
   if(stage==7){ScreenCapture.CaptureScreenshot(Out+"stairs.png");stage++;ticks=0;return;}
   if(stage==8){Quiet();p.bossPracticeMode=true;p.bossOverride=1;ClearWorld();Call("BeginJourney",false);Quiet();Set("position",new Vector2(0,-2));object e=List("enemies")[0];Set(e,"position",new Vector2(0,1));Set(e,"attackNumber",0);Call("BeginJourneyAttack",e);Call("UpdateJourneyEnemy",e,0f);Set("inventoryMenu",true);stage++;ticks=0;return;}
   if(stage==9){Set("inventoryMenu",false);Set("storyLines",new[]{"巨像抬起重锤，震波将从脚下向外扩散。利用掩体或闪避穿过震波。"});Call("UpdatePixelHero",Vector2.zero);stage++;ticks=0;return;}
   if(stage==10){ScreenCapture.CaptureScreenshot(Out+"boss-warning.png");stage++;ticks=0;return;}
   if(stage==11){MapPose(0);stage++;ticks=0;return;}
   if(stage==12){ScreenCapture.CaptureScreenshot(Out+"floor1.png");stage++;ticks=0;return;}
   if(stage==13){MapPose(1);stage++;ticks=0;return;}
   if(stage==14){ScreenCapture.CaptureScreenshot(Out+"floor2.png");stage++;ticks=0;return;}
   if(stage==15){MapPose(2);stage++;ticks=0;return;}
   if(stage==16){ScreenCapture.CaptureScreenshot(Out+"floor3.png");stage++;ticks=0;return;}
   Check(errors.Count==0,"Actual resource HUD, backpack, map, stairs and boss warnings render without GUI errors");File.WriteAllText(Out+"smoke-result.txt","PASS "+checks+" checks. Unity 2022.3.62f3 / URP.\n50 map seeds; actual sealed arrivals over 12 full graphs; all exits/stairs traversed on 3 graphs; separate light/element/dark accounting; crystal drops and revisits; automatic aim and cover; six weapon slots, full-bag replacement, supplies; explicit F floor transition; three boss moves, phases and post-death cleanup; lethal player damage in four directions, restart and missing-cache recovery. Real 1920x1080 Game View HUD/bag/map/stair/boss screenshots exported.\n");Finish(0);
  }
  catch(Exception e){Debug.LogException(e);File.WriteAllText(Out+"smoke-result.txt","FAIL after "+checks+" checks\n"+e+"\n"+string.Join("\n",errors));Finish(1);}
 }
 static void Finish(int code){SessionState.SetBool(Flag,false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;EditorApplication.Exit(code);}
}
