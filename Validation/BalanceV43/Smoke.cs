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
public static partial class NakiBalanceV43Smoke
{
 const BindingFlags B=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
 const string Out="D:/NakiLegend/Validation/BalanceV43/runtime/",Flag="Naki.BalanceV43";
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
  var rows=File.ReadAllLines("D:/NakiLegend/Validation/FeedbackV42/runtime/bosses-21.csv");var result=new List<string>{"island,stage,kind,old_hp,new_hp,multiplier"};
  for(int island=0;island<7;island++){
   BeginAuditIsland(island);
   for(int floor=0;floor<3;floor++){
    Set("clearedCombatRooms",3+floor*6);Set("clock",100f);Enter(((int[])Get("stageEnds"))[floor]);Quiet();
    object boss=List("enemies").Cast<object>().First(e=>(bool)Get(e,"boss"));var baseline=rows[1+island*3+floor].Split(',');float hp=float.Parse(baseline[4],System.Globalization.CultureInfo.InvariantCulture),factor=island<4?1.10f:1.18f;
    Check(Mathf.Abs((float)Get(boss,"maximum")-hp*factor)<.05f,"Boss HP ratio exactly "+factor+" / "+island+" / "+floor);
    Check(Mathf.Abs((float)Get(boss,"hp")-(float)Get(boss,"maximum"))<.01f,"Boss starts at full HP");
    Check(Mathf.Abs((float)Call("BossActiveSeconds",boss)-float.Parse(baseline[5],System.Globalization.CultureInfo.InvariantCulture))<.01f,"Attack phase duration unchanged");
    Check(Mathf.Abs(F("LightMaximum")-float.Parse(baseline[6],System.Globalization.CultureInfo.InvariantCulture))<.01f,"Light capacity unchanged");
    Check(Mathf.Abs((float)Call("OriginDamage",22f)-float.Parse(baseline[7],System.Globalization.CultureInfo.InvariantCulture))<.01f,"Skill damage unchanged");
    result.Add(string.Join(",",island+1,floor+1,Get(boss,"kind"),hp,Get(boss,"maximum"),factor));
   }
  }
  File.WriteAllLines(Out+"boss-hp.csv",result);
  BeginAuditIsland(1);Enter(1);Clear();
  foreach(bool talent in new[]{false,true})foreach(bool elite in new[]{false,true})foreach(bool summoned in new[]{false,true}){
   Set("learnedTalents",talent?new[]{7}:new int[0]);object enemy=Spawn("StormGuard",Vector2.zero);Set(enemy,"boss",false);Set(enemy,"elite",elite);Set(enemy,"summoned",summoned);
   float light=F("energy"),stone=F("darkPower");Call("SpawnEssence",enemy);var drops=(IList)Get(Room((int)Get("currentRoom")),"essence");object drop=drops[drops.Count-1];Check((float)Get(drop,"value")==2f,"Every non-boss kill is exactly +2 elemental, including elite/summon/talent");Check(F("energy")==light&&F("darkPower")==stone,"Element drop does not modify light or stone");
  }
  foreach(bool talent in new[]{false,true}){Set("learnedTalents",talent?new[]{7}:new int[0]);object boss=Spawn("CopperWarden",Vector2.zero);Call("SpawnEssence",boss);var drops=(IList)Get(Room((int)Get("currentRoom")),"essence");Check((float)Get(drops[drops.Count-1],"value")== (talent?50f:40f),"Boss elemental reward preserved");}
  BeginAuditIsland(4);Call("ChooseProfession",1);Set("chapterStone",true);Set("chapterLampGiven",true);Set("chapterRanger",true);Set("chapterEnding",true);Enter(0);Clear();Quiet();
  var all=((GameObject)Get("chapterFocus")).GetComponentsInChildren<SpriteRenderer>();Check(!all.Any(x=>x.name=="Seedlings by the harbor path"),"No ending seedbed on harbor water");Check(all.Any(x=>x.name=="Collector returned safely to A Ling"),"Returning collector retained");
  Sprite chest=(Sprite)Call("WorldPropSprite",6);Check(chest.rect.xMin>948&&chest.rect.xMin<951,"Chest atlas excludes neighboring boat without clipping box");Check(Mathf.Abs(chest.pixelsPerUnit-443.5f)<.01f,"Chest world size unchanged");Check(Mathf.Abs(chest.rect.xMin+chest.pivot.x-1108.75f)<.1f,"Chest anchor unchanged after trimming");
  Set("position",new Vector2(-2,0));Call("UpdatePixelHero",Vector2.zero);Call("ConfigureJourneyView");((Camera)Get("view")).transform.position=(Vector3)Get("JourneyViewCenter");
 }
 static void Tick(){
  if(!EditorApplication.isPlaying||EditorApplication.isCompiling||++ticks<25)return;p=UnityEngine.Object.FindObjectOfType<NakiPrototype>();if(p==null||Get("world")==null)return;
  try{
   if(stage==0){p.enabled=false;Tests();var proxy=new GameObject("QA HUD").AddComponent<CapitalHudProxy>();proxy.target=p;Time.timeScale=0;stage=1;ticks=0;return;}
   if(stage==1){ScreenCapture.CaptureScreenshot(Out+"harbor-fixed.png");stage=2;ticks=0;return;}
   Check(errors.Count==0,"No Unity runtime errors");File.WriteAllText(Out+"smoke-result.txt","PASS "+checks+" native checks\n");End(0);
  }catch(Exception e){Debug.LogException(e);File.WriteAllText(Out+"smoke-result.txt","FAIL after "+checks+"\n"+e+"\n"+string.Join("\n",errors));End(1);}
 }
 static void SetSize(int width,int height){var a=typeof(Editor).Assembly;var w=EditorWindow.GetWindow(a.GetType("UnityEditor.GameView"));w.Show();var t=a.GetType("UnityEditor.GameViewSizes");var singleton=typeof(ScriptableSingleton<>).MakeGenericType(t).GetProperty("instance",BindingFlags.Public|BindingFlags.Static).GetValue(null,null);var group=t.GetMethod("GetGroup").Invoke(singleton,new[]{Enum.Parse(a.GetType("UnityEditor.GameViewSizeGroupType"),"Standalone")});var size=Activator.CreateInstance(a.GetType("UnityEditor.GameViewSize"),new object[]{Enum.Parse(a.GetType("UnityEditor.GameViewSizeType"),"FixedResolution"),width,height,"Capital QA"});group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});w.GetType().GetProperty("selectedSizeIndex",B).SetValue(w,(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null)-1,null);}
 static void End(int code){Time.timeScale=1;SessionState.SetBool(Flag,false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;EditorApplication.Exit(code);}
}
