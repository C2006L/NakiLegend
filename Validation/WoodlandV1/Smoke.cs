using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NakiLegend.Prototype;
public static class NakiWoodlandV1Smoke
{
 const BindingFlags B=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
 const string Out="D:/NakiLegend/Repository/Validation/WoodlandV1/runtime/",Flag="Naki.WoodlandV1";
 static NakiPrototype p;static int ticks,stage,checks;static readonly List<string> errors=new List<string>();
 [InitializeOnLoadMethod]static void Reload(){if(SessionState.GetBool(Flag,false))Attach();}
 public static void Run(){Directory.CreateDirectory(Out);NakiLegend.Prototype.Editor.NakiSampleScenes.BuildWoodScenes();EditorSceneManager.OpenScene("Assets/Scenes/NakiTest.unity");UnityEngine.Object.FindObjectOfType<NakiPrototype>().showOpeningOnStart=false;SetSize(1920,1080);SessionState.SetBool(Flag,true);Attach();EditorApplication.isPlaying=true;}
 static void Attach(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;Application.logMessageReceived-=Log;Application.logMessageReceived+=Log;}
 static void Log(string s,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(s);}
 static object Get(object o,string n){var f=o.GetType().GetField(n,B);return f!=null?f.GetValue(o):o.GetType().GetProperty(n,B).GetValue(o,null);}static object Get(string n){return Get(p,n);}static void Set(string n,object v){typeof(NakiPrototype).GetField(n,B).SetValue(p,v);}static void Set(object o,string n,object v){o.GetType().GetField(n,B).SetValue(o,v);}static object Call(string n,params object[] a){return typeof(NakiPrototype).GetMethod(n,B).Invoke(p,a);}static IList List(string n){return (IList)Get(n);}static object Room(int i){return List("journeyRooms")[i];}static float F(string n){return (float)Get(n);}static object EnumVal(string type,string name){return Enum.Parse(typeof(NakiPrototype).GetNestedType(type,BindingFlags.NonPublic),name);}
 static void Check(bool yes,string why){if(!yes)throw new Exception(why);checks++;Debug.Log("EXPERIENCE_PASS "+why);}
 static void Quiet(){Call("StopOpeningCinematic");Set("storyLines",null);Set("paused",false);Set("mapMenu",false);Set("rewardMenu",false);Set("inventoryMenu",false);Set("showRainCG",false);Set("chapterSetup",false);Set("chapterCg",false);Set("chapterSkillsMenu",false);Set("roomBannerUntil",0f);}
 static string Story(){return string.Join("\n",(string[])Get("storyLines")??new string[0]);}
 static void Fresh(bool dark=true){p.thunderChapterMode=p.thunderPrototypeMode=p.bossPracticeMode=p.gatehouseSampleMode=p.lightningTrialMode=false;p.showOpeningOnStart=false;Call("BeginJourney",false);}
 static void Select(bool dark=true){Call("ChooseProfession",0);Call("ChooseOrigin",dark?1:0);Quiet();Set("invulnerableUntil",100000f);}
 static void Chapter(int floor=0,int boss=-1){p.thunderChapterMode=true;p.thunderPrototypeMode=false;p.thunderPreset=NakiPrototype.ThunderPreset.Light;p.thunderChapterStartFloor=floor;p.thunderChapterPracticeBoss=boss;Call("LoadThunderArt");Call("BeginThunderChapter",false);Quiet();Call("ChooseProfession",0);Set("invulnerableUntil",100000f);}
 static void Enter(int id){Call("EnterExplorationRoom",id,-1);Quiet();Set("invulnerableUntil",100000f);}
 static int Find(string kind){for(int i=0;i<List("journeyRooms").Count;i++)if(Get(Room(i),"kind").ToString()==kind)return i;throw new Exception(kind);}
 static object Spawn(string kind,Vector2 at){return Call("SpawnJourneyEnemy",EnumVal("EnemyKind",kind),at,0);}
 static void NarrativeAndPower()
 {
  Fresh();Check(Story().Contains("渡船")&&Story().Contains("木栈桥"),"Fresh landing explains boat and shore without requiring the long CG");Call("CompleteOpeningCinematic");Check(Story().Contains("渡船"),"Skipping opening still explains arrival by boat");Select();
  Fresh();Call("ChooseOrigin",1);Check(!Story().Contains("三间")&&!Story().Contains("战斗房")&&Story().Contains("右眼"),"Dark origin dialogue stays in story");Quiet();Set("clearedCombatRooms",3);Call("CheckPendingLesson");Check((int)Get("pendingLesson")==1,"Primary lesson still appears after early tutorial encounters");Call("LearnPendingLesson");Check((bool)Get("orbLearned")&&!Story().Contains("冷却")&&!Story().Contains("消耗"),"Skill discovery dialogue is separate from mechanical notices");Quiet();
  int rest=Find("Rest"),trace=Find("Event");Set("health",2f);Set("darkPower",51f);Set("energy",88f);Set("elementPower",30f);Call("EnterExplorationRoom",rest,-1);
  Check(F("energy")==0&&F("darkPower")==51&&F("health")==4&&F("elementPower")==70,"Dark rest heals and supplies elements without light or souls");Check(!Story().Contains("光之力"),"Dark rest never claims to restore light");Quiet();Enter(rest);Check(F("health")==4&&F("elementPower")==70,"Revisiting rest cannot farm rewards");Set("elementPower",30f);Call("EnterExplorationRoom",trace,-1);
  Check(!Story().Contains("水车")&&!Story().Contains("轮机")&&!Story().Contains("孩子")&&!(bool)Get("gateRepairCommitted")&&(int)Get("restoredCanals")==0,"Early trace rooms do not duplicate wheel repair or child scenes");Check(F("energy")==0&&F("darkPower")==51&&F("elementPower")==50,"Trace reward respects independent power pools");
  Fresh();Select(false);Set("energy",22f);Call("EnterExplorationRoom",Find("Rest"),-1);Check(F("energy")==100&&F("darkPower")==0,"Light rest still restores light");Quiet();
  Set("allowBatchCheckpoints",true);Set("checkpointPathOverride",Out+"save/legacy-dark.json");Fresh();Select();Set("energy",37f);Check((bool)Call("SaveCheckpoint",false),"Write historical dark save containing inactive light reserve");Set("energy",99f);Check((bool)Call("LoadCheckpoint",false)&&F("energy")==0,"Loading old dark save removes inactive light reserve");Set("allowBatchCheckpoints",false);
 }
 static void EndingGate()
 {
  Fresh();Select();int end=((int[])Get("stageEnds"))[2];Enter(end);object boss=List("enemies")[0];Call("DamageEnemy",boss,100000f);Call("TickJourney",.01f);Check(List("journeyRooms").Count==44&&!(bool)Get("pendingRainCG")&&!(bool)Get("showRainCG"),"Desert final boss unlocks canal instead of ending CG");
  Set("pendingRainCG",true);Call("AdvanceJourneyStory",true);Check(!(bool)Get("showRainCG")&&!(bool)Get("pendingRainCG"),"Premature ending request is rejected before wheel and total gate");int start=(int)Get("canalEpilogueStart");Enter(start+4);Call("TryCompleteCanalEpilogue");Check(!(bool)Get("canalEpilogueComplete"),"Entering lookout cannot bypass wheel repair");Set("gateRepairCommitted",true);Call("TryCompleteCanalEpilogue");Check(!(bool)Get("canalEpilogueComplete"),"Wheel repair alone cannot bypass total gate");Set("gateTotalUnlocked",true);Call("TryCompleteCanalEpilogue");Check(!(bool)Get("canalEpilogueComplete"),"Repair and total gate still require safe canal combat rooms");Set(Room(start+1),"cleared",true);Set(Room(start+3),"cleared",true);Call("TryCompleteCanalEpilogue");Check((bool)Get("DesertEndingReady"),"Repair, total gate and safe rooms enable real ending");Call("AdvanceJourneyStory",true);Check((bool)Get("showRainCG"),"Ending CG plays only after complete canal sequence");
  Fresh();p.bossPracticeMode=true;Call("BeginJourney",false);Quiet();Call("DamageEnemy",List("enemies")[0],100000f);Call("TickJourney",.01f);Check(!(bool)Get("pendingRainCG")&&!(bool)Get("DesertEndingReady"),"Boss practice does not falsely play the rain ending");
 }
 static void GrowthAndPressure()
 {
  Fresh();Select(false);Check(F("EffectiveMaximumShield")==4&&F("SkillStrength")==1,"First island retains tutorial shield and base skill strength");
  Chapter();Check(p.maximumShield==4&&F("EffectiveMaximumShield")==5&&F("shield")==5,"Second island adds one shield without modifying base tuning");
  Set("orbLearned",true);Set("energy",100f);Call("CastJourneyOrb");Check(Mathf.Abs((float)Get(List("shots")[0],"damage")-27.6f)<.001f,"Second-island light skill gains fifteen percent damage");List("shots").Clear();Set("bladeReadyAt",0f);Call("CastWindBlade");Check(Mathf.Abs((float)Get(List("shots")[0],"damage")-29.9f)<.001f,"Inherited empowered wind skill gains fifteen percent damage");
  Set("waveReadyAt",0f);Call("CastWindWave");Check(Mathf.Abs(F("windGuardUntil")-F("clock")-1.2f)<.001f,"Second-island wind guard lasts 1.2 seconds");Set("shield",5f);Set("invulnerableUntil",0f);Call("TakeJourneyDamage",2f,EnumVal("Element","Lightning"));Check(Mathf.Abs(F("shield")-3.7f)<.001f,"Strengthened guard reduces lightning damage by thirty-five percent");
  foreach(int floor in new[]{0,1,2})
  {
   Chapter(floor);object moth=Spawn("ChargeMoth",Vector2.zero),guard=Spawn("StormGuard",new Vector2(2,0));Check((float)Get(moth,"maximum")==42+floor*6&&(float)Get(guard,"maximum")==58+floor*8,"Second-island ordinary enemy durability grows by floor");
  }
  foreach(int id in new[]{0,1,2})
  {
   Chapter(0,id);object boss=List("enemies")[0];Check((float)Get(boss,"maximum")==new[]{360f,620,1250}[id],"Boss has distinct increased health budget");Set(boss,"attackNumber",1);Call("BeginChapterBossMove",boss);if(id==1){Call("ResolveChapterBossMove",boss);Check(List("shots").Count==7,"Sentinel first-phase fan has seven projectiles");}else if(id==2)Check(List("thunderStrikes").Count==2,"Stag's first lightning sequence already has two readable impacts");
   Call("ClearThunderHazards");foreach(object s in List("shots"))UnityEngine.Object.Destroy((GameObject)Get(s,"root"));List("shots").Clear();Set(boss,"secondPhase",true);Set(boss,"attackNumber",1);Call("BeginChapterBossMove",boss);Call("ResolveChapterBossMove",boss);int expected=id==0?7:id==1?9:0;if(expected>0){Check(List("shots").Count==expected,"Second phase increases fan pressure");foreach(object s in List("shots"))Check((float)Get(s,"damage")==2&&(float)Get(s,"speed")>=4,"Boss projectiles are stronger without losing warning time");}
  }
  Chapter();Set("allowBatchCheckpoints",true);Set("chapterCheckpointPathOverride",Out+"save/chapter.json");Set("shield",4f);Check((bool)Call("SaveChapterCheckpoint",false),"New chapter saves support raised shield limit");for(int i=0;i<3;i++){Call("LoadChapterCheckpoint",false);Check(F("EffectiveMaximumShield")==5&&F("shield")==4&&Mathf.Abs(F("SkillStrength")-1.15f)<.001f,"Repeated loads preserve deficit and never stack growth");}
  Set("origin",EnumVal("Origin","Dark"));Set("darkPower",53f);Set("energy",37f);Check((bool)Call("SaveChapterCheckpoint",false),"Legacy chapter may have a light preset but a dark origin and inactive light balance");Chapter();Check((bool)Call("LoadChapterCheckpoint",false)&&Get("origin").ToString()=="Dark"&&F("darkPower")==53&&F("energy")==0,"Loading uses saved origin before normalizing resources, independently of setup preset");Set("allowBatchCheckpoints",false);Fresh();Select();Check(F("EffectiveMaximumShield")==4,"Earlier island does not inherit duplicate chapter bonuses");
 }
 static void Layout()
 {
  foreach(Vector2 size in new[]{new Vector2(1280,720),new Vector2(1920,1080),new Vector2(2560,1440),new Vector2(1280,960),new Vector2(2560,1080)})
  {
   Rect r=(Rect)Call("JourneyViewport",size.x,size.y);float s=(float)Call("HudScaleFor",size.x,size.y);Check(Mathf.Abs(r.yMin*size.y-104*s)<.01f&&Mathf.Abs((1-r.yMax)*size.y-96*s)<.01f,"Both persistent HUD rails remain outside world viewport at "+size);
  }
  GUIStyle style=(GUIStyle)Get("journeySmallStyle");foreach(string line in new[]{"消耗 3 元素之力","风 · 伤害 1250","◆ 当前 2 沉铜破阵锤"})
  {Rect rect=new Rect(0,0,151,24);GUIStyle fit=(GUIStyle)Call("FitLabel",rect,line,style,false,TextAnchor.UpperLeft);Check(fit.CalcHeight(new GUIContent(line),rect.width)<=rect.height&&fit.CalcSize(new GUIContent(line)).x<=rect.width,"Weapon labels fit their actual glyph bounds");}
  Fresh();Select();Enter(Find("Combat"));Call("ConfigureJourneyView");Camera cam=(Camera)Get("view");Rect pixels=cam.pixelRect;foreach(Vector2 at in new[]{new Vector2(-5,3),new Vector2(5,3),new Vector2(-5,-3),new Vector2(5,-3)}){Vector3 pt=cam.WorldToScreenPoint(at);Check(pixels.Contains(pt),"Combat corner remains inside unobstructed camera viewport");}
 }
 static void ContinueAndEffects()
 {
  Chapter();Set("allowBatchCheckpoints",true);Set("chapterCheckpointPathOverride",Out+"save/continue.json");Set("darkPower",0f);Set("shield",3f);Set("elementPower",73f);
  Check((bool)Call("SaveChapterCheckpoint",false),"Write player checkpoint before testing menu");string before=File.ReadAllText(Out+"save/continue.json");Set("chapterSetup",true);Call("StartChapterFromSetup",false);
  Check((bool)Get("chapterConfirmRestart")&&(bool)Get("chapterSetup")&&File.ReadAllText(Out+"save/continue.json")==before,"New journey selection cannot silently overwrite existing save");
  Set("shield",0f);Check((bool)Call("LoadChapterCheckpoint",false)&&F("shield")==3&&F("elementPower")==73,"Continue restores original resources and room");Set("chapterConfirmRestart",false);Set("allowBatchCheckpoints",false);Quiet();
  int[] carried=(int[])Get("carriedWeapons");carried[0]=8;Set("elementPower",100f);Call("UseEquippedWeapon");object shot=List("shots")[List("shots").Count-1];GameObject bolt=(GameObject)Get(shot,"root");
  Check(bolt.transform.localScale.x>bolt.transform.localScale.y&&bolt.GetComponent<TrailRenderer>()!=null,"Crossbow fires a slim bolt with short physical trail");
  carried[0]=1;Set("clock",10f);Call("Swing");Set("clock",F("swordHitAt"));Call("UpdatePixelHero",Vector2.zero);GameObject trail=(GameObject)Get("weaponTrail");
  Check(!trail.activeSelf||trail.GetComponent<SpriteRenderer>().sprite==(Sprite)Get("square"),"Spear uses a straight thrust streak instead of sword arc");
  Set("orbLearned",true);Set("nextSpell",0f);Set("energy",100f);List("shots").Clear();Call("CastJourneyOrb");Check(((GameObject)Get(List("shots")[0],"root")).GetComponent<TrailRenderer>()!=null,"Light spell gets a distinct golden trail");
  Check(((Camera)Get("view")).rect.height>.77f,"Arena keeps more than seventy-seven percent of screen height at 16:9");
  Chapter();Set("position",Vector2.zero);carried=(int[])Get("carriedWeapons");carried[0]=1;object front=Spawn("StormGuard",new Vector2(1.4f,0)),side=Spawn("StormGuard",new Vector2(1.4f,.65f));float hp=(float)Get(side,"hp");Call("Swing");Set("clock",F("swordHitAt"));Call("UpdateSwordImpact");
  Check((float)Get(front,"hp")<hp&&(float)Get(side,"hp")==hp,"Spear hits its narrow forward line without damaging the adjacent flank");
  Chapter();Set("allowBatchCheckpoints",true);Set("chapterCheckpointPathOverride",Out+"save/complete.json");Set(Room(31),"cleared",true);Set("chapterStone",true);Set("chapterEnding",true);Set("journeyComplete",true);Check((bool)Call("SaveChapterCheckpoint",false),"Completed island writes a safe checkpoint");Call("RebuildRuntimeAfterReload");Quiet();
  Check((bool)Get("chapterStone")&&(bool)Get("chapterEnding")&&(bool)Get(Room(31),"cleared"),"Script rebuild preserves defeated final boss and completed ending");Set("allowBatchCheckpoints",false);
  Set("allowBatchCheckpoints",true);string completed=File.ReadAllText(Out+"save/complete.json");Call("StartChapterFromSetup",true);string[] archives=Directory.GetFiles(Out+"save","complete.json.*.previous");Check(archives.Length>0&&File.ReadAllText(archives[archives.Length-1])==completed,"Explicit new journey preserves the completed main save in a historical copy");Set("allowBatchCheckpoints",false);
 }
 static void ProfessionAndLightning()
 {
  Fresh();Quiet();int room=(int)Get("currentRoom");Call("ChooseProfession",1);Check(Get("profession").ToString()=="Mage"&&(int)Get("currentRoom")==room,"Old run selects mage without moving rooms");Check(((int[])Get("carriedWeapons"))[0]==0&&((int[])Get("carriedWeapons"))[1]==3,"Mage keeps sword and receives staff only in empty slot");
  Set("elementPower",100f);Call("SpendPower",EnumVal("PowerPool","Elemental"),10f);Check(Mathf.Abs(F("elementPower")-91.5f)<.001f,"Mage elemental discount is 15 percent");Set("energy",100f);Call("SpendPower",EnumVal("PowerPool","Light"),8f);Check(F("energy")==92,"Mage does not discount origin power");
  Select();Call("ChooseProfession",1);Set("allowBatchCheckpoints",true);Set("checkpointPathOverride",Out+"save/desert-profession.json");Check((bool)Call("SaveCheckpoint",false),"Desert profession saves at safe room");Set("profession",EnumVal("Profession","Warrior"));Call("LoadCheckpoint",false);Check(Get("profession").ToString()=="Mage","Desert load restores mage");Set("allowBatchCheckpoints",false);
  Chapter();Call("ChooseProfession",2);int[] carried=(int[])Get("carriedWeapons");carried[0]=2;Set("nextSword",0f);Call("UseEquippedWeapon");Check(Mathf.Abs(F("nextSword")-F("clock")-.5f)<.001f,"Archer bow rate is ten percent faster");Call("SwitchWeapon");Check(F("nextSword")>=F("clock")+.49f,"Switching does not bypass attack recovery");
  Chapter();Call("ChooseProfession",0);Set("position",Vector2.zero);object enemy=Spawn("StormGuard",new Vector2(.7f,0));float hp=(float)Get(enemy,"hp");Call("BeginDodge",Vector2.right);Set("position",new Vector2(.5f,0));Call("TickWarriorRam",Vector2.zero);Check((float)Get(enemy,"hp")==hp-12,"Warrior dash applies twelve damage");Call("TickWarriorRam",Vector2.zero);Check((float)Get(enemy,"hp")==hp-12,"Same dash cannot damage same enemy twice");
  Chapter();Call("ClearThunderHazards");Set("position",new Vector2(1.11f,0));Set("invulnerableUntil",0f);Set("clock",10f);Set("shield",5f);Call("QueueThunderStrike",Vector2.zero);object strike=List("thunderStrikes")[0];Check(Mathf.Abs((float)Get(strike,"when")-10.9f)<.001f,"Ground strike warns for 0.9 seconds");Check(Mathf.Abs(((GameObject)Get(strike,"root")).transform.localScale.x-2.2f)<.001f,"Warning diameter equals 2.2-unit damage diameter");Set("clock",10.45f);Call("TickThunderStrikes");Check(F("shield")==5&&List("thunderStrikes").Count==1,"Telegraph never causes early damage");Check(Mathf.Abs(((GameObject)Get(strike,"timer")).transform.localScale.x-.5f)<.01f,"Countdown ring is half-size halfway through warning");Set("clock",11f);Call("TickThunderStrikes");Check(F("shield")==5&&List("thunderStrikes").Count==0,"Outside warning edge is safe at impact");Check(List("fadingBodies").Count>=19,"Impact generates branched arcs ground sparks and boundary flash");
  Call("QueueThunderStrike",Vector2.zero);Set("position",Vector2.zero);Set("clock",12f);Call("TickThunderStrikes");Check(F("shield")<5,"Inside circle takes damage at impact");float shield=F("shield");Call("TickThunderStrikes");Check(F("shield")==shield,"Resolved strike cannot damage a second time");
  Chapter();Call("ChooseProfession",1);Set("chapterMarkKnown",true);((int[])Get("chapterSkillSlots"))[0]=2;Set("elementPower",6.8f);Spawn("StormGuard",new Vector2(2,0));Call("CastChapterSkill",0);Check(List("chapterMarks").Count==1&&F("elementPower")<.001f,"Mage can cast雷标 with exactly discounted energy");Call("ClearThunderHazards");List("chapterMarks").Clear();Chapter();Call("ChooseProfession",1);Set("allowBatchCheckpoints",true);Set("chapterCheckpointPathOverride",Out+"save/profession.json");Check((bool)Call("SaveChapterCheckpoint",false),"Mage chapter checkpoint saved");Set("profession",EnumVal("Profession","Archer"));Call("LoadChapterCheckpoint",false);Check(Get("profession").ToString()=="Mage","Chapter load restores selected profession");Set("allowBatchCheckpoints",false);
 }
 static void WoodTests(){p.woodlandMode=true;Chapter();Check(List("journeyRooms").Count==45,"Woodland graph has 45 rooms");Check((int)Get(Room(6),"stairsTo")==12&&(int)Get(Room(18),"stairsTo")==25,"Boss stairs connect all three floors");for(int i=0;i<45;i++){object room=Room(i);foreach(int link in (IList)Get(room,"links"))Check(((IList)Get(Room(link),"links")).Contains(i),"Door links are reciprocal");}
 Set("allowBatchCheckpoints",true);Set("chapterCheckpointPathOverride",Out+"save/wood.json");Call("ChooseProfession",1);Check((bool)Call("SaveChapterCheckpoint",false),"Wood save writes independently");Check((bool)Call("LoadChapterCheckpoint",false)&&List("journeyRooms").Count==45,"Wood 45-room save round-trips");
 foreach(int id in new[]{6,18,31}){Enter(id);foreach(object enemy in new ArrayList(List("enemies")))Call("DamageEnemy",enemy,100000f);Call("TickJourney",.01f);Quiet();Check((bool)Get(Room(id),"cleared"),"Wood boss resolves and clears room");}
 for(int id=0;id<45;id++){Enter(id);for(int pass=0;pass<8&&!(bool)Get(Room(id),"cleared");pass++){Set("clock",F("clock")+10);Call("TickPendingSpawns");foreach(object enemy in new ArrayList(List("enemies")))Call("DamageEnemy",enemy,100000f);Call("TickJourney",.01f);Quiet();}Check((bool)Get(Room(id),"cleared"),"All woodland room waves clear safely");}
 Check((bool)Get("chapterStone")&&!(bool)Get("chapterEnding"),"Boss does not immediately complete woodland ending");Call("WoodAction",206);Check((int)Get("currentRoom")==4,"Boss return reaches seedbed");Set("elementPower",0f);Call("WoodAction",207);Check((bool)Get("chapterLampGiven"),"Zero-resource planting succeeds");Call("WoodAction",207);Check((int)Get("currentRoom")==0,"Planted seedbed returns to dock");Call("WoodAction",201);Check((bool)Get("chapterEnding"),"Returning forest book completes ending");Quiet();Check((bool)Call("SaveChapterCheckpoint",false),"Completed wood story saves");Set("allowBatchCheckpoints",false);p.woodlandMode=false;Fresh();Select();}
 static void CombatPose()
 {
  Fresh();Select();Enter(Find("Combat"));Set("darkPower",67f);Set("orbLearned",true);Set("bladeLearned",true);Set("waveLearned",true);Set("invulnerableUntil",100000f);Set("clock",3f);Call("TickPendingSpawns");foreach(object e in List("enemies")){Set(e,"nextAttack",100000f);Set(e,"speed",0f);}Spawn("SpearGuard",new Vector2(-5,3));Spawn("SandScorpion",new Vector2(5,3));Call("UpdatePixelHero",Vector2.zero);
 }
 static void BagPose(){Chapter(2);p.thunderPreset=NakiPrototype.ThunderPreset.Dark;Call("BeginThunderChapter",false);Quiet();int[] carried=(int[])Get("carriedWeapons");carried[0]=11;carried[1]=13;List("reserveWeapons").Add(8);List("reserveWeapons").Add(9);List("reserveWeapons").Add(10);List("reserveWeapons").Add(12);Set("inventoryMenu",true);Call("UpdatePixelHero",Vector2.zero);}
 static void SetSize(int width,int height){var a=typeof(Editor).Assembly;var w=EditorWindow.GetWindow(a.GetType("UnityEditor.GameView"));w.Show();var t=a.GetType("UnityEditor.GameViewSizes");var singleton=typeof(ScriptableSingleton<>).MakeGenericType(t).GetProperty("instance",BindingFlags.Public|BindingFlags.Static).GetValue(null,null);var group=t.GetMethod("GetGroup").Invoke(singleton,new[]{Enum.Parse(a.GetType("UnityEditor.GameViewSizeGroupType"),"Standalone")});var size=Activator.CreateInstance(a.GetType("UnityEditor.GameViewSize"),new object[]{Enum.Parse(a.GetType("UnityEditor.GameViewSizeType"),"FixedResolution"),width,height,"Experience QA"});group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});w.GetType().GetProperty("selectedSizeIndex",B).SetValue(w,(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null)-1,null);}
 static void Next(){stage++;ticks=0;}
 static void Tick()
 {
  if(!EditorApplication.isPlaying||EditorApplication.isCompiling||++ticks<35)return;p=UnityEngine.Object.FindObjectOfType<NakiPrototype>();if(p==null||Get("world")==null)return;
  try
  {
   if(stage==0){p.enabled=false;NarrativeAndPower();EndingGate();GrowthAndPressure();Layout();ContinueAndEffects();ProfessionAndLightning();WoodTests();CombatPose();p.enabled=true;Next();return;}
   if(stage==1){ScreenCapture.CaptureScreenshot(Out+"combat-hd.png");Next();return;}
   if(stage==2){p.enabled=false;BagPose();p.enabled=true;Next();return;}
   if(stage==3){ScreenCapture.CaptureScreenshot(Out+"backpack-hd.png");Next();return;}
   if(stage==4){SetSize(2560,1440);Next();return;}
   if(stage==5){ScreenCapture.CaptureScreenshot(Out+"backpack-qhd.png");Next();return;}
   if(stage==6){p.enabled=false;SetSize(1280,960);CombatPose();p.enabled=true;Next();return;}
   if(stage==7){ScreenCapture.CaptureScreenshot(Out+"combat-4x3.png");Next();return;}
   if(stage==8){p.enabled=false;SetSize(1920,1080);Chapter(0,2);object e=List("enemies")[0];Set(e,"secondPhase",true);Set(e,"hp",500f);Set(e,"attackNumber",1);Call("BeginChapterBossMove",e);Call("UpdateChapterEnemyPose",e);Call("UpdatePixelHero",Vector2.zero);p.enabled=true;Next();return;}
   if(stage==9){ScreenCapture.CaptureScreenshot(Out+"boss-pressure.png");Next();return;}
   if(stage==10){p.enabled=false;Fresh();p.enabled=true;Next();return;}
   if(stage==11){ScreenCapture.CaptureScreenshot(Out+"boat-arrival.png");Next();return;}
   if(stage==12){p.enabled=false;Chapter(0,2);Set("position",new Vector2(-2,0));Call("LightningImpact",new Vector2(-3,0));Set("paused",false);Next();return;}
   if(stage==13){ScreenCapture.CaptureScreenshot(Out+"lightning-impact.png");Next();return;}
   if(stage==14){p.enabled=false;p.woodlandMode=true;Chapter();Quiet();Set("profession",EnumVal("Profession","Unchosen"));p.enabled=true;Next();return;}
   if(stage==15){ScreenCapture.CaptureScreenshot(Out+"profession-choice.png");Next();return;}
   if(stage==16){p.enabled=false;Call("ChooseProfession",1);Enter(2);Set("clock",F("clock")+3);Call("TickPendingSpawns");Set("invulnerableUntil",100000f);p.enabled=true;Next();return;}
   if(stage==17){ScreenCapture.CaptureScreenshot(Out+"wood-combat.png");Next();return;}
   Check(errors.Count==0,"Actual HUD, inventory, dialogue and boss scenes render without runtime errors");File.WriteAllText(Out+"smoke-result.txt","PASS "+checks+" checks: boat arrival; narrative skill discovery; no early wheel/child repair; origin-specific rest and old-save normalization; canal-only rain ending; tutorial enemies; second-island shield, skills, stronger phased bosses; repeated-load growth; five viewport aspect/size cases; fitted weapon text; native HD/QHD/4:3 captures.\n");End(0);
  }catch(Exception e){Debug.LogException(e);File.WriteAllText(Out+"smoke-result.txt","FAIL after "+checks+"\n"+e+"\n"+string.Join("\n",errors));End(1);}
 }
 static void End(int code){SessionState.SetBool(Flag,false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;EditorApplication.Exit(code);}
}
