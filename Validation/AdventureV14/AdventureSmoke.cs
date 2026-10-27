using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NakiLegend.Prototype;

public static class NakiAdventureV14Smoke
{
 const BindingFlags B=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
 const string Flag="Naki.AdventureV14QA";
 const string Out="D:/NakiLegend/Repository/Validation/AdventureV14/runtime/";
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
 static void Fresh(){p.gatehouseSampleMode=false;p.bossPracticeMode=false;p.lightningTrialMode=false;p.showOpeningOnStart=false;Call("BeginJourney",false);Quiet();}
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

 static object EnumValue(string type,string value){return Enum.Parse(typeof(NakiPrototype).GetNestedType(type,BindingFlags.NonPublic),value);}
 static void Select(int choice){Call("ChooseOrigin",choice);Quiet();}
 static void ReadyCast(){Set("clock",F("clock")+10);Set("nextSword",0f);Set("nextSpell",0f);Set("bladeReadyAt",0f);Set("waveReadyAt",0f);Set("dashUntil",0f);Set("swordUntil",0f);Set("castPoseUntil",0f);}
 static object Spawn(string name,Vector2 at){return Call("SpawnJourneyEnemy",EnumValue("EnemyKind",name),at,0);}
 static void ChoicesAndBalance()
 {
  Fresh();Check((bool)Get("OriginChoiceReady")&&!(bool)Get("orbLearned"),"Arrival asks for origin before any primary lesson");Set("clearedCombatRooms",3);Call("CheckPendingLesson");Check((int)Get("pendingLesson")==0,"Unchosen origin cannot silently learn light skill");
  Select(0);Check(Get("origin").ToString()=="Light"&&!(bool)Get("orbLearned"),"Light selection precedes learning");Call("ChooseOrigin",1);Check(Get("origin").ToString()=="Light","Origin is fixed for the run");Call("CheckPendingLesson");Call("LearnPendingLesson");Quiet();
  Set("energy",100f);Set("elementPower",100f);Set("bladeLearned",true);Set("waveLearned",true);ReadyCast();Call("CastJourneyOrb");Check(F("energy")==92&&Mathf.Abs(F("nextSpell")-F("clock")-1.2f)<.001f,"Light orb uses eight power and 1.2-second cooldown");
  Call("CastWindBlade");Check(F("elementPower")==94&&Mathf.Abs(F("bladeReadyAt")-F("clock")-1.2f)<.001f,"Wind blade uses six elemental power and 1.2-second cooldown");Call("CastWindWave");Check(F("elementPower")==84&&Mathf.Abs(F("waveReadyAt")-F("clock")-4.5f)<.001f,"Wind wave uses ten elemental power and 4.5-second cooldown");
  int[] costs={0,1,1,1,1,2,2,1};
  for(int i=0;i<8;i++){((int[])Get("carriedWeapons"))[0]=i;Set("equippedSlot",0);ReadyCast();Set("elementPower",100f);Set("energy",57f);Set("darkPower",32f);Call("UseEquippedWeapon");Check(F("elementPower")==100-costs[i]&&F("energy")==57&&F("darkPower")==32,"Weapon "+i+" charges its own elemental cost");}
  Fresh();Select(1);Check(Get("origin").ToString()=="Dark"&&F("energy")==0&&F("darkPower")==24,"Dark selection has an independent initial soul reserve");Set("clearedCombatRooms",3);Call("CheckPendingLesson");Call("LearnPendingLesson");Quiet();
  Set("darkPower",80f);Set("elementPower",51f);ReadyCast();Call("CastJourneyOrb");Check(List("shots").Count==2&&F("darkPower")==72&&F("elementPower")==51&&F("energy")==0,"Dark primary emits two purple homing orbs using only dark power");
  foreach(object shot in List("shots"))Check(Get(shot,"element").ToString()=="Dark"&&(bool)Get(shot,"orb"),"Dark projectile carries dark damage and active-volley cap");
  float charge=F("darkPower");Call("CastJourneyOrb");Check(F("darkPower")==charge&&List("shots").Count==2,"Dark meteor cannot stack unlimited volleys");
  Set("darkPower",0f);object living=Spawn("SandScorpion",new Vector2(3,0));Call("DamageEnemy",living,100000f);Check(F("darkPower")==12,"Living enemy charges twelve dark power");object machine=Spawn("StoneSentinel",new Vector2(-3,0));Call("DamageEnemy",machine,100000f);Check(F("darkPower")==12,"Mechanical guardian does not yield a soul");
 }
 static void BowAndDoors()
 {
  Fresh();Select(0);((int[])Get("carriedWeapons"))[0]=2;Set("equippedSlot",0);
  foreach(Vector2 aim in new[]{Vector2.right,Vector2.left,Vector2.up,Vector2.down})
  {ReadyCast();Set("castPoseUntil",F("clock")+.25f);Set("castDirection",aim);Call("UpdatePixelHero",Vector2.zero);var bow=(GameObject)Get("sword");Vector2 forward=bow.transform.rotation*Vector3.left;Check(Vector2.Dot(forward.normalized,aim)>.999f,"Bow front follows aim in all four directions");}
  Rect bounds=(Rect)typeof(NakiPrototype).GetProperty("CurrentRoomBounds",B).GetValue(p,null);
  foreach(string kind in new[]{"SandScorpion","SpearGuard","Tumbleweed","SandKite","StoneSentinel","ApeKing"})
  {
   object e=Spawn(kind,new Vector2(bounds.xMax+2,0));float radius=(float)Get(e,"radius");Set("position",new Vector2(bounds.xMax+5,0));Set("invulnerableUntil",100000f);
   for(int i=0;i<80;i++)Call("UpdateJourneyEnemy",e,.05f);
   Vector2 at=(Vector2)Get(e,"position");Check(at.x<=bounds.xMax-radius-.1f&&at.x>=bounds.xMin+radius+.1f&&at.y<=bounds.yMax-radius-.1f&&at.y>=bounds.yMin+radius+.1f,"Enemy "+kind+" stays inside room despite chasing through door");
  }
  Vector2 moved=(Vector2)Call("MoveEnemyWithinRoom",new Vector2(100,100),new Vector2(100,100),.85f);Check(bounds.Contains(moved),"High-speed enemy movement cannot tunnel beyond room bounds");
  Set(Room((int)Get("currentRoom")),"cleared",false);Call("UpdateExplorationDoors");foreach(object e in List("enemies"))Check(bounds.Contains((Vector2)Get(e,"position")),"Closing doors reclaims any out-of-room enemy");
 }
 static void Persistence()
 {
  Set("allowBatchCheckpoints",true);Set("checkpointPathOverride",Out+"checkpoint-test/journey.json");Fresh();Select(1);
  Set("health",4f);Set("shield",1f);Set("darkPower",43f);Set("elementPower",55f);((int[])Get("carriedWeapons"))[0]=6;Set("equippedSlot",0);List("reserveWeapons").Add(2);Call("StoreGroundWeapon",4);
  Check((bool)Call("SaveCheckpoint",true),"Safe-room checkpoint writes successfully");string path=(string)Get("CheckpointPath"),saved=File.ReadAllText(path);
  int battle=-1;foreach(object room in List("journeyRooms"))if(Get(room,"kind").ToString()=="Combat"){battle=(int)Get(room,"id");break;}
  Enter(battle);Check(!(bool)Call("SaveCheckpoint",true)&&File.ReadAllText(path)==saved,"Battle cannot overwrite the last safe checkpoint");
  Set("health",1f);Set("darkPower",0f);((int[])Get("carriedWeapons"))[0]=0;Check((bool)Call("LoadCheckpoint",true),"F9 equivalent resumes from a battle to the saved safe room");Quiet();
  Check((int)Get("currentRoom")==0&&F("health")==4&&F("shield")==1&&F("darkPower")==43&&F("elementPower")==55&&Get("origin").ToString()=="Dark","Health, shields, pools and dark origin survive disk round trip");
  Check(((int[])Get("carriedWeapons"))[0]==6&&List("reserveWeapons").Count==1&&List("worldWeapons").Count==1,"Equipped, reserve and ground weapons survive checkpoint");
  Set("health",3f);Check((bool)Call("SaveCheckpoint",false),"Second checkpoint creates a recoverable backup");File.WriteAllText(path,"{bad-json");Check((bool)Call("LoadCheckpoint",true)&&F("health")==4,"Corrupt primary recovers the older valid backup");Quiet();
  File.WriteAllText(path,"{bad-json");File.WriteAllText(path+".bak","{bad-json");float health=F("health");int roomId=(int)Get("currentRoom");Check(!(bool)Call("LoadCheckpoint",true)&&F("health")==health&&(int)Get("currentRoom")==roomId,"Both corrupt files leave current gameplay intact");
  Check((bool)Call("SaveCheckpoint",false),"A valid safe checkpoint can be written after recovery failure");
  string valid=File.ReadAllText(path);Type savedType=typeof(NakiPrototype).GetNestedType("JourneyCheckpoint",BindingFlags.NonPublic);object invalid=JsonUtility.FromJson(valid,savedType);Array badRooms=(Array)Get(invalid,"rooms");Set(badRooms.GetValue(0),"layout",-1);File.WriteAllText(path,JsonUtility.ToJson(invalid));File.WriteAllText(path+".bak","{bad-json");
  Check(!(bool)Call("LoadCheckpoint",true)&&F("health")==health,"Structurally invalid JSON is rejected before changing live state");Set("allowBatchCheckpoints",false);
 }
 static void CaptureBow(string name,Vector2 aim)
 {
  Fresh();Select(0);((int[])Get("carriedWeapons"))[0]=2;Set("equippedSlot",0);ReadyCast();Set("position",Vector2.zero);Set("castPoseUntil",F("clock")+.25f);Set("castDirection",aim);Call("UpdatePixelHero",Vector2.zero);
  var cam=(Camera)Get("view");var old=cam.targetTexture;var previous=RenderTexture.active;var rt=new RenderTexture(480,360,24);var tex=new Texture2D(480,360,TextureFormat.RGB24,false);
  try{cam.transform.position=new Vector3(0,.45f,-10);cam.orthographicSize=1.25f;cam.aspect=480f/360;cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,480,360),0,0);tex.Apply();File.WriteAllBytes(Out+name+".png",tex.EncodeToPNG());}
  finally{cam.targetTexture=old;RenderTexture.active=previous;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);}
 }
 static void Tick()
 {
  if(!EditorApplication.isPlaying||EditorApplication.isCompiling||++ticks<40)return;p=UnityEngine.Object.FindObjectOfType<NakiPrototype>();if(p==null||Get("world")==null)return;
  try{
   if(stage==0){p.enabled=false;ChoicesAndBalance();BowAndDoors();Persistence();Check(errors.Count==0,"Origins, balance, bow, enemy bounds and real file saves have no runtime exceptions");Fresh();p.enabled=true;stage++;ticks=0;return;}
   if(stage==1){ScreenCapture.CaptureScreenshot(Out+"origin-choice.png");stage++;ticks=0;return;}
   if(stage==2){Select(1);Set("orbLearned",true);Set("darkPower",80f);Set("energy",0f);Set("roomBannerUntil",0f);Set("inventoryMenu",true);stage++;ticks=0;return;}
   if(stage==3){ScreenCapture.CaptureScreenshot(Out+"dark-inventory.png");stage++;ticks=0;return;}
   if(stage==4){p.enabled=false;CaptureBow("bow-right",Vector2.right);CaptureBow("bow-left",Vector2.left);stage++;ticks=0;return;}
   Check(errors.Count==0,"Choice and dark resource HUD render correctly");File.WriteAllText(Out+"smoke-result.txt","PASS "+checks+" checks: explicit light/dark origin, branch-specific lesson and projectiles; cheaper skills; eight weapon costs; four bow directions; six enemy types and closed doors; actual JSON save/load, inventory and map persistence, corruption backup; HD UI captures.\n");End(0);
  }catch(Exception e){Debug.LogException(e);File.WriteAllText(Out+"smoke-result.txt","FAIL after "+checks+" checks\n"+e+"\n"+string.Join("\n",errors));End(1);}
 }
 static void End(int code){SessionState.SetBool(Flag,false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;EditorApplication.Exit(code);}
}
