using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NakiLegend.Prototype;

public static class NakiThunderSmoke
{
 const BindingFlags B=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
 const string Out="D:/NakiLegend/Repository/Validation/ThunderPrototypeS1/runtime/",Flag="Naki.ThunderS1QA";
 static NakiPrototype p;static int ticks,stage,checks;static readonly List<string> errors=new List<string>();
 [InitializeOnLoadMethod]static void Reload(){if(SessionState.GetBool(Flag,false))Attach();}
 public static void Run(){Directory.CreateDirectory(Out);AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);foreach(string name in new[]{"floor.png","rock.png","node.png","moth.png","guard.png","atlas.json"}){string path="Assets/NakiPrototype/Resources/NakiArt/ThunderS1/"+name;AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate|ImportAssetOptions.ForceSynchronousImport);}NakiLegend.Prototype.Editor.NakiSampleScenes.BuildThunderScene();EditorSceneManager.OpenScene(NakiLegend.Prototype.Editor.NakiSampleScenes.ThunderPath);var w=EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));w.Show();SetSize(w);SessionState.SetBool(Flag,true);Attach();EditorApplication.isPlaying=true;}
 static void SetSize(EditorWindow w){var a=typeof(Editor).Assembly;var t=a.GetType("UnityEditor.GameViewSizes");var singleton=typeof(ScriptableSingleton<>).MakeGenericType(t).GetProperty("instance",BindingFlags.Public|BindingFlags.Static).GetValue(null,null);var group=t.GetMethod("GetGroup").Invoke(singleton,new[]{Enum.Parse(a.GetType("UnityEditor.GameViewSizeGroupType"),"Standalone")});var size=Activator.CreateInstance(a.GetType("UnityEditor.GameViewSize"),new object[]{Enum.Parse(a.GetType("UnityEditor.GameViewSizeType"),"FixedResolution"),1920,1080,"Thunder QA"});group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});w.GetType().GetProperty("selectedSizeIndex",B).SetValue(w,(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null)-1,null);}
 static void Attach(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;Application.logMessageReceived-=Log;Application.logMessageReceived+=Log;}
 static void Log(string s,string trace,LogType t){if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert)errors.Add(s);}
 static object Get(object o,string n){var f=o.GetType().GetField(n,B);return f!=null?f.GetValue(o):o.GetType().GetProperty(n,B).GetValue(o,null);}static object Get(string n){return Get(p,n);}static void Set(string n,object v){typeof(NakiPrototype).GetField(n,B).SetValue(p,v);}static void Set(object o,string n,object v){o.GetType().GetField(n,B).SetValue(o,v);}
 static object Call(string n,params object[] v){return typeof(NakiPrototype).GetMethod(n,B).Invoke(p,v);}static IList List(string n){return (IList)Get(n);}static float F(string n){return (float)Get(n);}static object Room(int id){return List("journeyRooms")[id];}
 static object E(string name){return Enum.Parse(typeof(NakiPrototype).GetNestedType("EnemyKind",BindingFlags.NonPublic),name);}
 static void Check(bool yes,string why){if(!yes)throw new Exception(why);checks++;Debug.Log("THUNDER_PASS "+why);}
 static void Quiet(){Set("storyLines",null);Set("thunderSetupMenu",false);Set("paused",false);Set("inventoryMenu",false);Set("mapMenu",false);Set("rewardMenu",false);Set("roomBannerUntil",0f);}
 static void Fresh(NakiPrototype.ThunderPreset preset=NakiPrototype.ThunderPreset.Light,int room=0){p.thunderPrototypeMode=true;p.showOpeningOnStart=false;Call("BeginThunderSample",preset,room);Quiet();}
 static void Step(float seconds){for(int i=0;i<Mathf.CeilToInt(seconds/.05f);i++){Set("clock",F("clock")+.05f);Call("TickJourney",.05f);}}
 static void Wave(){Step(2.6f);for(int i=List("enemies").Count-1;i>=0;i--)Call("DamageEnemy",List("enemies")[i],10000f);Step(.05f);}
 static void Clear(){int count=0;while(!(bool)Get(Room((int)Get("currentRoom")),"cleared")&&count++<5){Wave();Step(1.2f);}Check((bool)Get(Room((int)Get("currentRoom")),"cleared"),"Configured wave budget clears room");}
 static void Go(int d,int target){Set("position",Call("DoorPoint",d,1.12f));Set("clock",F("clock")+2);Call("TryAutomaticExit");Check(F("doorTransition")>0,"Actual doorway begins transition");for(int i=0;i<7;i++)Call("TickDoorTransition",.05f);Quiet();Check((int)Get("currentRoom")==target&&(bool)Call("SafeFooting",Get("position"),.34f),"Actual doorway arrives safely");}
 static void RunChecks()
 {
  Check(p.thunderPrototypeMode&&!(bool)Get("CheckpointsEnabled"),"Native scene enables isolated mode and disables normal checkpoint access");

  foreach(string sprite in new[]{"thunderFloor","thunderRock","thunderNode"})Check(Get(sprite)!=null,"Imported production sprite "+sprite);
  var art=(IDictionary)Get("enemyActionArt");Check(((Sprite[])art[E("ChargeMoth")]).Length==14&&((Sprite[])art[E("StormGuard")]).Length==12,"Real irregular atlases load 14 moth and 12 guard frames");
  foreach(NakiPrototype.ThunderPreset preset in Enum.GetValues(typeof(NakiPrototype.ThunderPreset)))foreach(int start in new[]{0,1})
  {
   Fresh(preset,start);Check(List("journeyRooms").Count==2&&(int)Get("currentRoom")==start,"Preset and direct starting room are applied");
   Check(!(bool)Get("OriginChoiceReady")&&!(bool)Get("openingActive"),"Prototype avoids desert origin and opening overlays");
   if(preset==NakiPrototype.ThunderPreset.Dark)Check(Get("origin").ToString()=="Dark"&&F("darkPower")==24,"Dark preset has its own reserve");
   if(preset==NakiPrototype.ThunderPreset.BasicWeapons)Check(!(bool)Get("orbLearned")&&!(bool)Get("bladeLearned")&&!(bool)Get("waveLearned")&&F("elementPower")==0,"Basic preset starts with no skills and zero elemental power");
   int expected=start==0?8:6;Clear();Check((int)Get("defeated")==expected,"Exact wave roster: eight grass / six mechanism enemies");
   Check(!(bool)Get("pendingBossClear")&&!(bool)Get("journeyComplete")&&List("journeyRooms").Count==2,"New kinds never count as bosses or append canal rooms");
   Go(start==0?0:1,1-start);Clear();Go(start==0?1:0,start);
  }
  Fresh();Clear();Go(0,1);Clear();Set("position",new Vector2(3.7f,2.1f));Set("elementPower",100f);Set("energy",63f);Set("darkPower",17f);
  Check((bool)Call("TryRoomInteraction")&&F("thunderWorkRemaining")>0,"F starts the visible lever interaction");
  for(int i=0;i<9;i++)Call("TickThunderOverlay",.05f);Call("CancelThunderRepair");
  Check(!(bool)Get("thunderRepaired")&&!(bool)Get("thunderRewardCreated"),"Cancelled repair leaves no state or reward");Call("TryThunderInteraction");for(int i=0;i<26;i++)Call("TickThunderOverlay",.05f);Quiet();
  Check((bool)Get("thunderRepaired")&&(bool)Get("thunderRewardCreated"),"Finishing repair commits state and reward together");
  Check(F("elementPower")==100&&F("energy")==63&&F("darkPower")==17,"Repair is free and does not alter primary powers");
  int rewards=0;foreach(object e in (IList)Get(Room(1),"essence"))if((float)Get(e,"value")==20)rewards++;
  Check(rewards==1,"Repair leaves exactly one twenty-element crystal when full");Call("TryRoomInteraction");Go(1,0);Go(0,1);
  int again=0;foreach(object e in (IList)Get(Room(1),"essence"))if((float)Get(e,"value")==20)again++;
  Check(again==1&&(bool)Get("thunderRepaired"),"Revisit preserves repair and does not duplicate reward");
  foreach(object o in List("thunderFlow"))Check(((GameObject)o).activeSelf,"Rerouted current is visible after repair");
  PhysicsAndElements();Persistence();
 }
 static void PhysicsAndElements()
 {
  Fresh(NakiPrototype.ThunderPreset.BasicWeapons);Step(2.6f);foreach(object old in List("enemies"))Set(old,"position",new Vector2(-7,3));Set("position",Vector2.zero);object target=Call("SpawnJourneyEnemy",E("ChargeMoth"),new Vector2(.9f,0),0);float hp=(float)Get(target,"hp");Call("UseEquippedWeapon");Set("clock",F("clock")+.12f);Call("UpdateSwordImpact");Check((float)Get(target,"hp")<hp&&F("elementPower")==0,"Free starter sword really damages a new enemy with zero elemental power");
  Fresh();Call("EnterExplorationRoom",0,-1);Quiet();
  foreach(Vector2 at in new[]{new Vector2(-3.7f,-.1f),new Vector2(3.3f,-1.1f)})foreach(Vector2 dir in new[]{Vector2.right,Vector2.left,Vector2.up,Vector2.down,new Vector2(1,1).normalized})Check((bool)Call("ProjectilePathBlocked",at-dir*2,at+dir*2,.11f),"Low stone blocks full projectile path from cardinal and diagonal approaches");
  foreach(string name in new[]{"ChargeMoth","StormGuard"})
  {
   object e=Call("SpawnJourneyEnemy",E(name),new Vector2(20,20),0);Check(!(bool)Get(e,"boss")&&Get(e,"affinity").ToString()=="Lightning","New enemy has lightning affinity and ordinary kill behavior");
   Set("position",new Vector2(30,0));for(int i=0;i<60;i++){Set("clock",F("clock")+.05f);Call("UpdateJourneyEnemy",e,.05f);}
   Vector2 p0=(Vector2)Get(e,"position");Rect b=(Rect)Get("CurrentRoomBounds");float rad=(float)Get(e,"radius");Check(p0.x<b.xMax-rad&&p0.y<b.yMax-rad,"Chasing enemy stays within full-body room bounds");
   Set(e,"position",new Vector2(0,1));Set(e,"windup",2f);Set(e,"recoveryUntil",0f);Set(e,"hp",1000f);Set(e,"maximum",1000f);
   object wind=Enum.Parse(typeof(NakiPrototype).GetNestedType("Element",BindingFlags.NonPublic),"Wind");
   Call("ApplyElementDamage",e,10f,wind);Call("ApplyElementDamage",e,10f,wind);Call("ApplyElementDamage",e,10f,wind);
   Check(Mathf.Abs((float)Get(e,"hp")-961)<.01f&&(float)Get(e,"windup")==0,"Wind deals 1.3 damage and third hit breaks a charging enemy");
  }
  Fresh();Set("position",Vector2.zero);Set("invulnerableUntil",0f);Set("shield",4f);Set("windGuardUntil",0f);Call("QueueThunderStrike",Vector2.zero);
  Call("SpawnShot",new Vector2(.2f,0),Vector2.left,true,null,new Vector2(.2f,0));
  Call("CastWindWave");Check(List("shots").Count==0&&List("thunderStrikes").Count==1,"Wind wave dispels ordinary bullets but leaves ground warning");
  Set("clock",F("clock")+.91f);Call("TickThunderStrikes");Check(Mathf.Abs(F("shield")-3.3f)<.01f,"Ground lightning still hits with temporary thirty-percent reduction");
  Call("QueueThunderStrike",Vector2.zero);Call("ClearJourneyRoom");Check(List("thunderStrikes").Count==0,"Room clear removes pending ground hazards");
 }
 static void Persistence()
 {
  Set("allowBatchCheckpoints",true);Set("thunderCheckpointPathOverride",Out+"save/thunder.json");Set("checkpointPathOverride",Out+"save/normal-sentinel.json");Directory.CreateDirectory(Out+"save");File.WriteAllText(Out+"save/normal-sentinel.json","NORMAL_UNTOUCHED");
  Fresh(NakiPrototype.ThunderPreset.Dark);Clear();Set("darkPower",46f);Set("health",4f);Set("elementPower",39f);Set("bladeReadyAt",F("clock")+3);List("reserveWeapons").Add(2);Call("StoreGroundWeapon",4);
  Check((bool)Call("SaveThunderCheckpoint",false),"Safe prototype checkpoint writes a real file");string file=(string)Get("ThunderCheckpointPath"),saved=File.ReadAllText(file);
  Go(0,1);Check(!(bool)Call("SaveThunderCheckpoint",false)&&File.ReadAllText(file)==saved,"Combat cannot overwrite the safe prototype checkpoint");
  Set("health",1f);Set("darkPower",0f);Check((bool)Call("LoadThunderCheckpoint",false)&&F("health")==4&&F("darkPower")==46&&F("elementPower")==39,"Loading restores dark preset, resources and safe room");
  Check(List("reserveWeapons").Count==1&&((IList)Get(Room(0),"drops")).Count==1&&F("bladeReadyAt")>F("clock"),"Reserve weapons, ground loot and remaining cooldown persist");
  Check(File.ReadAllText(Out+"save/normal-sentinel.json")=="NORMAL_UNTOUCHED"&&!(bool)Call("LoadCheckpoint",false),"Normal journey save is neither read nor modified");
  Call("SaveThunderCheckpoint",false);File.WriteAllText(file,"{broken");Check((bool)Call("LoadThunderCheckpoint",false),"Broken primary prototype file recovers backup");
  File.WriteAllText(file,"{broken");File.WriteAllText(file+".bak","{broken");float old=F("health");Check(!(bool)Call("LoadThunderCheckpoint",false)&&F("health")==old,"Both broken files leave current state intact");
  Set("allowBatchCheckpoints",false);Fresh();Check(!(bool)Get("thunderRepaired")&&!(bool)Get("thunderRewardCreated"),"Restart resets mechanism and one-time reward");
 }
 static void CaptureActor(string kind,bool left)
 {
  Fresh();Clear();object e=Call("SpawnJourneyEnemy",E(kind),Vector2.zero,0);Set("position",new Vector2(left?-3:3,0));
  Camera cam=(Camera)Get("view");Vector3 oldPosition=cam.transform.position;float oldSize=cam.orthographicSize;var previous=RenderTexture.active;var old=cam.targetTexture;var rt=new RenderTexture(384,320,24);var tex=new Texture2D(384,320,TextureFormat.RGB24,false);
  try{cam.targetTexture=rt;cam.orthographicSize=1.1f;cam.aspect=1.2f;cam.transform.position=new Vector3(0,.45f,-10);
   for(int i=0;i<6;i++){Set("clock",200f+i/9f);Set(e,"velocity",Vector2.right);Set(e,"hurtUntil",0f);Set(e,"recoveryUntil",0f);Set(e,"windup",0f);Call("UpdateThunderPose",e);cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,384,320),0,0);tex.Apply();File.WriteAllBytes(Out+kind+(left?"-left-":"-right-")+i+".png",tex.EncodeToPNG());}}
  finally{cam.targetTexture=old;cam.ResetAspect();cam.transform.position=oldPosition;cam.orthographicSize=oldSize;RenderTexture.active=previous;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);}
 }
 static void Tick()
 {
  if(!EditorApplication.isPlaying||EditorApplication.isCompiling||++ticks<35)return;p=UnityEngine.Object.FindObjectOfType<NakiPrototype>();if(p==null||Get("world")==null)return;
  try{
   if(stage==0){p.enabled=false;RunChecks();CaptureActor("StormGuard",false);CaptureActor("StormGuard",true);CaptureActor("ChargeMoth",false);Fresh();Set("thunderSetupMenu",true);p.enabled=true;stage++;ticks=0;return;}
   if(stage==1){ScreenCapture.CaptureScreenshot(Out+"setup.png");stage++;ticks=0;return;}
   if(stage==2){p.enabled=false;Fresh();Step(2.6f);Set("clock",F("clock")+.4f);foreach(object e in List("enemies"))Call("UpdateJourneyEnemy",e,.05f);Set("roomBannerUntil",0f);Set("invulnerableUntil",F("clock")+1000);p.enabled=true;stage++;ticks=0;return;}
   if(stage==3){ScreenCapture.CaptureScreenshot(Out+"grass-combat.png");stage++;ticks=0;return;}
   if(stage==4){p.enabled=false;Fresh(NakiPrototype.ThunderPreset.Dark,1);Step(2.6f);Set("roomBannerUntil",0f);Call("QueueThunderStrike",new Vector2(-1,0));Set("invulnerableUntil",F("clock")+1000);p.enabled=true;stage++;ticks=0;return;}
   if(stage==5){ScreenCapture.CaptureScreenshot(Out+"mechanism-combat.png");stage++;ticks=0;return;}
   if(stage==6){p.enabled=false;Clear();Set("position",new Vector2(3.7f,2.1f));Set("elementPower",100f);Call("TryThunderInteraction");for(int i=0;i<26;i++)Call("TickThunderOverlay",.05f);Quiet();Call("UpdatePixelHero",Vector2.zero);p.enabled=true;stage++;ticks=0;return;}
   if(stage==7){ScreenCapture.CaptureScreenshot(Out+"mechanism-repaired.png");stage++;ticks=0;return;}
   Check(errors.Count==0,"Runtime, setup, game HUD, actors and repair render without exceptions");File.WriteAllText(Out+"smoke-result.txt","PASS "+checks+" checks: two-room scene and presets, real doors and wave rosters, elemental enemies, wind and lightning, cover, free repair and one-time rewards, isolated real saves and corruption recovery; native HD screenshots and actor animation captures.\n");End(0);
  }catch(Exception e){Debug.LogException(e);File.WriteAllText(Out+"smoke-result.txt","FAIL after "+checks+"\n"+e+"\n"+string.Join("\n",errors));End(1);}
 }
 static void End(int code){SessionState.SetBool(Flag,false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;EditorApplication.Exit(code);}
}
