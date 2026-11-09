using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NakiLegend.Prototype;
public sealed class CapitalHudProxy:MonoBehaviour
{public NakiPrototype target;void OnGUI(){if(target!=null&&!target.enabled)typeof(NakiPrototype).GetMethod("OnGUI",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(target,null);}}
public static partial class NakiReturnV44Smoke
{
 const BindingFlags B=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
 const string Out="D:/NakiLegend/Validation/ReturnV44/runtime/",Flag="Naki.ReturnV44";
 static NakiPrototype p;static int ticks,stage,checks;static List<string> errors=new List<string>();static string baseline;
 [InitializeOnLoadMethod]static void Reload(){if(SessionState.GetBool(Flag,false))Attach();}
 public static void Run(){Directory.CreateDirectory(Out);NakiLegend.Prototype.Editor.NakiCapitalScenes.BuildScenes();PlayerSettings.companyName="NakiValidation";PlayerSettings.productName="CapitalQA"+DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");EditorSceneManager.OpenScene(NakiLegend.Prototype.Editor.NakiCapitalScenes.Adventure);SetSize(1600,900);SessionState.SetBool(Flag,true);Attach();EditorApplication.isPlaying=true;}
 static void Attach(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;Application.logMessageReceived-=Log;Application.logMessageReceived+=Log;}
 static void Log(string s,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(s+"\n"+stack);}
 static object Get(object o,string n){var f=o.GetType().GetField(n,B);return f!=null?f.GetValue(o):o.GetType().GetProperty(n,B).GetValue(o,null);}static object Get(string n){return Get(p,n);}static void Set(string n,object v){typeof(NakiPrototype).GetField(n,B).SetValue(p,v);}static void Set(object o,string n,object v){o.GetType().GetField(n,B).SetValue(o,v);}static object Call(string n,params object[] a){return typeof(NakiPrototype).GetMethod(n,B).Invoke(p,a);}static IList List(string n){return (IList)Get(n);}static object Room(int i){return List("journeyRooms")[i];}static float F(string n){return (float)Get(n);}static object E(string type,string name){return Enum.Parse(typeof(NakiPrototype).GetNestedType(type,BindingFlags.NonPublic|BindingFlags.Public),name);}
 static void Check(bool yes,string why){if(!yes)throw new Exception(why);checks++;Debug.Log("CAPITAL_PASS "+why);}
 static void Quiet(){Call("StopOpeningCinematic");Set("storyLines",null);Set("queuedOriginChapter",0);Set("originAbilityMenu",false);Set("voyagerJournal",false);Set("worldAtlasMenu",false);Set("atlasConfirm",false);Set("challengeMenu",false);Set("paused",false);Set("mapMenu",false);Set("rewardMenu",false);Set("inventoryMenu",false);Set("showRainCG",false);Set("chapterSetup",false);Set("chapterCg",false);Set("chapterSkillsMenu",false);Set("roomBannerUntil",0f);Set("audioSettingsMenu",false);Set("invulnerableUntil",100000f);}
 static void Clear(){foreach(object e in List("enemies")){UnityEngine.Object.Destroy((GameObject)Get(e,"root"));UnityEngine.Object.Destroy((GameObject)Get(e,"shadow"));if(Get(e,"warning")!=null)UnityEngine.Object.Destroy((GameObject)Get(e,"warning"));}List("enemies").Clear();foreach(object e in List("pendingSpawns"))UnityEngine.Object.Destroy((GameObject)Get(e,"mark"));List("pendingSpawns").Clear();ClearShots();Call("ClearJourneyEffects");}
 static void ClearShots(){foreach(object s in List("shots"))UnityEngine.Object.Destroy((GameObject)Get(s,"root"));List("shots").Clear();}
 static void Enter(int room){Call("EnterExplorationRoom",room,-1);Quiet();Set("capitalCg",-1);Call("ConfigureJourneyView");((Camera)Get("view")).transform.position=(Vector3)Get("JourneyViewCenter");}
 static void Start(int boss=-1){Call("SetAtlasIsland",6);p.thunderChapterPracticeBoss=boss;p.thunderChapterStartFloor=0;Set("thunderPreset",E("ThunderPreset","Light"));Call("BeginCapital",false);Call("ChooseProfession",0);Quiet();Set("capitalCg",-1);Set("allowBatchCheckpoints",true);Set("chapterBatchSaveDirectory",Out+"save");Set("chapterCheckpointPathOverride",Out+"save/capital.json");}
 static object Spawn(string kind,Vector2 at){return Call("SpawnJourneyEnemy",E("EnemyKind",kind),at,26);}
 static void KillRoom(int room){Enter(room);for(int pass=0;pass<12&&!(bool)Get(Room(room),"cleared");pass++){
  foreach(object pending in List("pendingSpawns"))Set(pending,"when",0f);Call("TickPendingSpawns");
  foreach(object enemy in new ArrayList(List("enemies")))if(List("enemies").Contains(enemy))Call("DamageEnemy",enemy,100000f);
  if((bool)Get("pendingBossClear")){Set("pendingBossClear",false);Call("CompleteJourneyBoss");}
  else if(List("enemies").Count==0&&List("pendingSpawns").Count==0){if((int)Get("currentWave")<(int)Get(Room(room),"waves"))Call("SpawnJourneyWave");else Call("ClearJourneyRoom");}
  Set("clock",F("clock")+1f);
 }Check((bool)Get(Room(room),"cleared"),"Actual waves complete room "+room);Quiet();}
 static void Read(int index){Call("CapitalAction",1010+index);Check((int)Get("capitalLetter")==index,"Open parchment document "+index);Call("AdvanceCapitalLetter");Check((int)Get("capitalLetter")==index,"First page does not auto-complete document "+index);Call("AdvanceCapitalLetter");Check((int)Get("capitalLetter")==-1&&((int)Get("capitalLetters")&(1<<index))!=0,"Read full document records evidence "+index);}
 static void BeginAuditIsland(int island){Call("SetAtlasIsland",island);if(island==0)Call("BeginJourney",false);else Call("BeginThunderChapter",false);Quiet();Set("capitalCg",-1);Set("allowBatchCheckpoints",false);Set("chapterCheckpointPathOverride",null);}


 static void Tests(){
  BeginAuditIsland(5);Set("allowBatchCheckpoints",true);Set("chapterBatchSaveDirectory",Out+"saves");Set("chapterCheckpointPathOverride",null);Set("journeyTestProfile","return-path-"+DateTime.UtcNow.ToString("HHmmssfff"));Call("ChooseProfession",1);Quiet();Set("origin",E("Origin","Light"));
  Directory.CreateDirectory((string)Get("JourneySaveDirectory"));for(int i=0;i<5;i++)File.WriteAllText((string)Call("AtlasSavePath",i),"{\"version\":1,\"ending\":true,\"canalComplete\":true,\"rooms\":[]}");
  Enter(44);Clear();Check(!(bool)Get("ManorReturnReady"),"No return before finale");Set("interactionNeedsRelease",false);Set("worldInteractionReady",0f);Call("RequestManorReturn");Check((int)Get("pendingIceVoyage")==-1,"Premature boarding refused");
  foreach(object n in List("journeyRooms")){Set(n,"cleared",true);Set(n,"storyRead",true);}Set("chapterWaymarks",3);Set("manorIntro",false);Set("chapterLampGiven",true);Set("manorProgress",7);Set("manorLetters",7);Set("manorVictorySeen",true);Set("manorVictoryCg",false);Set("chapterStone",true);Call("BuildChapterActions");
  for(int i=0;i<3;i++){Call("ManorFinaleAction",720+i);if(i<2){Check((int)Get("manorEnding")==0,"First two seals do not skip finale");Quiet();}}
  Check((int)Get("manorEnding")==1&&(bool)Get("chapterCg"),"Third seal starts original light ending CG");Call("FinishManorEnding");Check((bool)Get("chapterEnding")&&(bool)Get("ManorReturnReady"),"CG conclusion unlocks return");Check((bool)Get("StoryActive"),"CG leads into departure narrative");Check(((string)Call("ManorFullObjective")).Contains("按 F 登船"),"Objective explicitly explains boarding");
  Quiet();Call("BuildChapterActions");Call("UpdateFlowGuide");Check((int)Get("StoryGuideGoal")==44&&Get("flowGuide")!=null,"Post-finale story guide points to boarding, not a dead-end");
  Check(List("chapterActions").Cast<object>().Count(x=>(int)Get(x,"kind")==724)==1,"Exactly one return interaction after all seals");
  Set("interactionNeedsRelease",false);Set("worldInteractionReady",0f);Call("RequestManorReturn");Check((int)Get("pendingIceVoyage")==724&&!(bool)Get("voyageArmed"),"Boarding requires fresh confirmation, never held F");Check(((string)Get("VoyageTitle")).Contains("本土王都")&&((string)Get("VoyageDescription")).Contains("六片源石"),"Voyage title and story identify destination");Call("CancelIceVoyage");Check((int)Get("pendingIceVoyage")==-1&&(bool)Get("manorCourtyardMode"),"Cancel keeps player in Manor");
  Check((bool)Call("SaveChapterCheckpoint",false),"Finished Manor with return saves safely");Check((bool)Call("LoadChapterCheckpoint",false),"Existing completed save reloads");Quiet();Call("BuildChapterActions");Check(List("chapterActions").Cast<object>().Any(x=>(int)Get(x,"kind")==724),"Return entry survives reload without new schema or reset");
  Enter(0);Clear();Check(List("chapterActions").Cast<object>().Count(x=>(int)Get(x,"kind")==724)==1,"Returning to outer harbor also permits boarding");Enter(44);Clear();
  Set("health",4f);Set("shield",2f);Set("elementPower",53f);Set("energy",65f);Set("darkPower",77f);int[] slots=(int[])Get("chapterSkillSlots");slots[0]=4;slots[1]=6;Call("CaptureSkillLoadout");int[] weapons=((int[])Get("carriedWeapons")).ToArray();
  Set("interactionNeedsRelease",false);Set("worldInteractionReady",0f);Call("RequestManorReturn");Set("voyageOpened",Time.unscaledTime-1f);Call("ConfirmIceVoyage");
  Check((bool)Get("capitalMode")&&(int)Get("currentRoom")==0,"Confirm goes directly to capital arrival without G menu");Check((int)Get("capitalCg")==0,"Fresh capital arrival starts existing homecoming CG");Check(!((bool)Get("worldAtlasMenu")),"No atlas selection required");Check(F("health")==4&&F("shield")==2&&F("elementPower")==53&&F("energy")==65&&F("darkPower")==0,"Actual health and resource balances travel unchanged");Check(((int[])Get("chapterSkillSlots")).SequenceEqual(new[]{4,6})&&((int[])Get("carriedWeapons")).SequenceEqual(weapons),"Selected skills and both weapons retained");
  Check(File.Exists((string)Call("AtlasSavePath",5))&&File.Exists((string)Call("AtlasSavePath",6)),"Both chapter saves preserved");
  // Reload completed Manor once more, then revisit the existing capital checkpoint.
  Call("SetAtlasIsland",5);Check((bool)Call("LoadChapterCheckpoint",false),"Return to original completed Manor checkpoint");Quiet();Set("interactionNeedsRelease",false);Set("worldInteractionReady",0f);Call("RequestManorReturn");Set("voyageOpened",Time.unscaledTime-1f);Call("ConfirmIceVoyage");Check((bool)Get("capitalMode")&&(int)Get("currentRoom")==0,"Repeat return resumes existing capital instead of resetting it");
  CapitalRevisionTests();
  Call("SetAtlasIsland",5);Call("LoadChapterCheckpoint",false);Quiet();Call("BuildChapterActions");Call("UpdateFlowGuide");var a=List("chapterActions").Cast<object>().First(x=>(int)Get(x,"kind")==724);Set("position",(Vector2)Get(a,"at")+Vector2.down*.8f);Call("UpdatePixelHero",Vector2.zero);Set("interactionNeedsRelease",false);Set("worldInteractionReady",0f);Call("RequestManorReturn");

 }

 static void CapitalRevisionTests(){
  Start();Set("manorIntro",false);Set("chapterBatchSaveDirectory",Out+"saves");Set("chapterCheckpointPathOverride",null);Quiet();Call("CloseCapitalCg");Quiet();
  for(int room=0;room<List("journeyRooms").Count;room++){
   Enter(room);Clear();Set(Room(room),"cleared",true);Call("BuildChapterActions");
   var rendered=((GameObject)Get("chapterFocus")).GetComponentsInChildren<SpriteRenderer>();int count=rendered.Count(x=>x.sprite==(Sprite)Call("CapitalProp",4));
   Check(count==(room==21?1:0),"Exactly one narrative desk, in the study only / room "+room);
   Check(!List("chapterActions").Cast<object>().Any(x=>(int)Get(x,"kind")==1006),"No redundant skill-management desk / "+room);
  }
  Set(Room(26),"cleared",false);Set("capitalFlags",7);Set("capitalLetters",0);Enter(21);Clear();Call("BuildChapterActions");Check(List("chapterActions").Cast<object>().Count(x=>(int)Get(x,"kind")>=1010&&(int)Get(x,"kind")<=1012)==1,"One bound archive interaction replaces three desks");
  Call("CapitalAction",1010);Call("CloseCapitalLetter",false);Check((int)Get("capitalLetters")==0,"Closing unread archive never unlocks plot");
  Enter(25);Check(((int)Get("capitalFlags")&8)==0&&!(bool)Call("CapitalGateReady",26),"Throne approach cannot skip unread archive");
  Enter(21);Clear();Call("CapitalAction",1010);
  for(int doc=0;doc<3;doc++){Check((int)Get("capitalLetter")==doc,"Bound archive advances in story order / "+doc);Call("AdvanceCapitalLetter");Check((int)Get("capitalLetters")==((1<<doc)-1),"First page alone never records full document / "+doc);Call("AdvanceCapitalLetter");}
  Check((int)Get("capitalLetters")==7&&(int)Get("capitalLetter")==-1,"All three complete documents recorded, then parchment closes");Quiet();Call("EnterExplorationRoom",25,-1);Check(((int)Get("capitalFlags")&8)!=0&&(int)Get("capitalCg")==2,"Entering preparation room automatically connects archive to heart CG without another desk");Call("CloseCapitalCg");Check((bool)Call("CapitalGateReady",26),"Throne unlocks after evidence and reflection");
  Check((bool)Call("SaveChapterCheckpoint",false),"Consolidated archive and reflection save");Check((bool)Call("LoadChapterCheckpoint",false)&&(int)Get("capitalLetters")==7,"Existing letter-bit save schema retained");Quiet();
  foreach(string kind in new[]{"CapitalCommander","CapitalMagistrate","CapitalKing"}){
   Enter(kind=="CapitalCommander"?8:kind=="CapitalMagistrate"?17:26);Clear();Call("ClearBossSignatures");Call("ClearManorImpacts");Set("clock",100f);Set("position",new Vector2(0,-2));object boss=Spawn(kind,Vector2.zero);Set(boss,"attackDirection",Vector2.down);Set(boss,"difficultyPhase",1);object state=Call("CapitalState",boss);Set(state,"move",kind=="CapitalMagistrate"?2:1);Call("ResolveCapitalAttack",boss,state);
   var zones=List("signatureZones").Cast<object>().Where(x=>Get(x,"owner")==boss&&(int)Get(x,"mode")==4).ToArray();Check((bool)Call("RoyalRangedActive",boss,state),"Existing ground pattern must finish before another move can start");
   Check(zones.Length>=1&&zones.Length<=(kind=="CapitalKing"?3:2),"New ground denial is present and bounded / "+kind);
   foreach(object zone in zones){Check(((Color)Call("SignatureZoneTint",zone)).r>((Color)Call("SignatureZoneTint",zone)).g,"Royal danger uses warm hostile color instead of green healing ring");Check(Mathf.Abs((float)Get(zone,"active")-101.1f)<.01f,"Ground danger has full 1.1s warning");Check((float)Get(zone,"end")-(float)Get(zone,"active")>=2.8f,"Ground danger persists long enough to force movement");}
   int initial=zones.Length;for(int i=0;i<8;i++)Call("CapitalControlGround",boss,kind=="CapitalMagistrate"?2:1);Check(List("signatureZones").Cast<object>().Count(x=>Get(x,"owner")==boss&&(int)Get(x,"mode")==4)<= (kind=="CapitalKing"?3:2),"Repeated casts cannot fill entire arena");
   object first=zones[0];Set("position",Get(first,"at"));Set("invulnerableUntil",0f);Set("shield",6f);Set("health",6f);Call("TickBossSignatures",.05f);Check(F("shield")==6f,"Warning phase cannot damage player");Set("clock",101.2f);Call("TickBossSignatures",.05f);Check(F("shield")<6f,"Active ground seal actually damages a stationary player");
   Set("clock",110f);Call("TickBossSignatures",.05f);Check(!List("signatureZones").Cast<object>().Any(x=>Get(x,"owner")==boss&&(int)Get(x,"mode")==4),"Ground seals expire instead of permanently blocking arena");
   Call("ClearBossSignatures");Set(state,"move",kind=="CapitalMagistrate"?1:2);Set("clock",120f);Call("ResolveCapitalAttack",boss,state);if(kind!="CapitalCommander")Check(!List("signatureZones").Cast<object>().Any(x=>(int)Get(x,"mode")==4),"No simultaneous ground seals during boss laser / "+kind);
   if(kind=="CapitalKing"){Call("ClearBossSignatures");Set(state,"move",0);Set(boss,"difficultyPhase",0);Call("ResolveCapitalAttack",boss,state);Check((int)Get(state,"royalCombo")==2,"King opening phase now has two committed sword follow-ups");Set(boss,"difficultyPhase",2);Call("ResolveCapitalAttack",boss,state);Check((int)Get(state,"royalCombo")==3,"King late phase escalates to three sword follow-ups");Check(Mathf.Abs((float)Get(boss,"maximum")-6490f)<.1f,"King HP remains at previously approved V43 value");}
   Clear();Call("ClearBossSignatures");Call("ClearManorImpacts");Set("clock",200f);boss=Spawn(kind,Vector2.zero);Set(boss,"nextAttack",200f);float resting=0;var seen=new HashSet<object>();
   for(int tick=0;tick<1200;tick++){
    Set("clock",200f+tick*.05f);Set("invulnerableUntil",100000f);Set("position",new Vector2(Mathf.Sin(tick*.015f)*3,-1.6f+Mathf.Cos(tick*.01f)*.7f));Call("ConfigureJourneyView");((Camera)Get("view")).transform.position=(Vector3)Get("JourneyViewCenter");
    foreach(object enemy in new ArrayList(List("enemies")))if(List("enemies").Contains(enemy))Call("UpdateTacticalEnemy",enemy,.05f);
    Call("TickBossSignatures",.05f);Call("TickManorImpacts");foreach(object zone in List("signatureZones"))if(Get(zone,"owner")==boss&&(int)Get(zone,"mode")==4)seen.Add(zone);
    for(int i=List("shots").Count-1;i>=0;i--)Call("UpdateShot",i,.05f);if((bool)Call("RecoveryWindow",boss))resting+=.05f;
   }
   Check(resting>=3,"Full moving recovery window still occurs in 60-second simulation / "+kind);Check((int)Get(boss,"attackNumber")>=4&&seen.Count>=2,"Boss cycles attacks and ground denial instead of stalling / "+kind);
   File.AppendAllText(Out+"cadence.txt",kind+", attacks="+Get(boss,"attackNumber")+", seals="+seen.Count+", recovery_seconds="+resting+"\n");

  }
 }
 static void Tick(){
  if(!EditorApplication.isPlaying||EditorApplication.isCompiling||++ticks<25)return;p=UnityEngine.Object.FindObjectOfType<NakiPrototype>();if(p==null||Get("world")==null)return;
  try{
   if(stage==0){p.enabled=false;Tests();var proxy=new GameObject("QA HUD").AddComponent<CapitalHudProxy>();proxy.target=p;Time.timeScale=0;stage=1;ticks=0;return;}
   if(stage==1){ScreenCapture.CaptureScreenshot(Out+"return-boarding.png");stage=2;ticks=0;return;}
   if(stage==2){BeginAuditIsland(6);Call("ChooseProfession",1);Enter(21);Clear();Set("position",new Vector2(0,-1));Call("UpdatePixelHero",Vector2.zero);((Camera)Get("view")).transform.position=new Vector3(0,1,-10);stage=3;ticks=0;return;}
   if(stage==3){ScreenCapture.CaptureScreenshot(Out+"single-study.png");stage=4;ticks=0;return;}
   if(stage==4){Enter(26);Clear();Call("ClearBossSignatures");Set("clock",100f);Set("position",new Vector2(0,-2));object king=Spawn("CapitalKing",Vector2.zero);Set(king,"attackDirection",Vector2.down);Set(king,"difficultyPhase",1);object state=Call("CapitalState",king);Set(state,"move",3);Call("ResolveCapitalAttack",king,state);Set("clock",101.2f);Call("TickBossSignatures",.05f);Call("UpdateJourneyEnemyPose",king,false);Call("UpdatePixelHero",Vector2.zero);((Camera)Get("view")).transform.position=new Vector3(0,1,-10);stage=5;ticks=0;return;}
   if(stage==5){ScreenCapture.CaptureScreenshot(Out+"king-ground-seals.png");stage=6;ticks=0;return;}
   Check(errors.Count==0,"No Unity runtime errors");File.WriteAllText(Out+"smoke-result.txt","PASS "+checks+" native checks\n");End(0);
  }catch(Exception e){Debug.LogException(e);File.WriteAllText(Out+"smoke-result.txt","FAIL after "+checks+"\n"+e+"\n"+string.Join("\n",errors));End(1);}
 }
 static void SetSize(int width,int height){var a=typeof(Editor).Assembly;var w=EditorWindow.GetWindow(a.GetType("UnityEditor.GameView"));w.Show();var t=a.GetType("UnityEditor.GameViewSizes");var singleton=typeof(ScriptableSingleton<>).MakeGenericType(t).GetProperty("instance",BindingFlags.Public|BindingFlags.Static).GetValue(null,null);var group=t.GetMethod("GetGroup").Invoke(singleton,new[]{Enum.Parse(a.GetType("UnityEditor.GameViewSizeGroupType"),"Standalone")});var size=Activator.CreateInstance(a.GetType("UnityEditor.GameViewSize"),new object[]{Enum.Parse(a.GetType("UnityEditor.GameViewSizeType"),"FixedResolution"),width,height,"Capital QA"});group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});w.GetType().GetProperty("selectedSizeIndex",B).SetValue(w,(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null)-1,null);}
 static void End(int code){Time.timeScale=1;SessionState.SetBool(Flag,false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;EditorApplication.Exit(code);}
}
