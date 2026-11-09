from pathlib import Path
import shutil
r=Path('D:/NakiLegend/Repository');o=r/'Validation/ManorFullV11';o.mkdir(exist_ok=True);(o/'runtime').mkdir(exist_ok=True)
s=(r/'Validation/NaturalRecoveryV10/Smoke.cs').read_text(encoding='utf-8-sig').replace('NakiNaturalRecoveryV10Smoke','NakiManorFullV11Smoke').replace('Naki.NaturalRecoveryV10','Naki.ManorFullV11').replace('Validation/NaturalRecoveryV10/runtime/','Validation/ManorFullV11/runtime/').replace('NaturalRecoveryV10Tests();','ManorFullV11Tests();')
extra=''' static void SafeManor(int id){Set(Room(id),"cleared",true);Enter(id);Quiet();Set("chapterLampGiven",true);}
 static void DefeatManor(){object boss=List("enemies").Cast<object>().First(e=>(bool)Get(e,"boss"));Call("DamageEnemy",boss,999999f);if((int)Get("currentRoom")==14)Call("CompleteManorCaptain");else Call("CompleteManorLaterBoss");Set("pendingBossClear",false);Quiet();}
 static void ManorFullV11Tests(){
 ManorPose(0);Set("chapterLampGiven",true);Set("allowBatchCheckpoints",true);Set("chapterCheckpointPathOverride",Out+"manor.json");
 Check(List("journeyRooms").Count==45&&((string[])Get("chapterNames")).Length==45,"Manor now has three floors and forty-five named rooms");
 foreach(object room in List("journeyRooms")){int id=(int)Get(room,"id");Check((int)Get(room,"stage")==id/15,"Stable room-to-floor mapping "+id);foreach(int link in (IList)Get(room,"links"))Check(((IList)Get(Room(link),"links")).Contains(id),"Room link reciprocal "+id+" / "+link);}
 Check((int)Get("ChapterRoomCount")==45,"Save count matches expanded graph");
 Check((int[])Get("stageStarts")!=null&&((int[])Get("stageStarts"))[2]==30,"Atlas third-floor entrance is real");
 Check(List("weaponCatalog").Count==135&&List("weaponIdentities").Count==135,"Two new dedicated weapons have matching unique identities");Check(Call("AudioAsset","weapon_011_cast")!=null,"Seal hammer has a real hammer sound asset");
 Set("chapterWaymarks",3);Set(Room(4),"cleared",true);Set(Room(13),"cleared",true);Enter(14);DefeatManor();Check((bool)Get("chapterStone")&&!Get("chapterEnding").Equals(true),"Captain still ends only the outer court");
 Check((bool)Call("SaveChapterCheckpoint",false),"Existing outer court journey saves with expanded chapter");
 object old=JsonUtility.FromJson(File.ReadAllText(Out+"manor.json"),typeof(NakiPrototype).GetNestedType("ChapterCheckpoint",BindingFlags.NonPublic));Array prior=(Array)Get(old,"rooms");Array first=Array.CreateInstance(prior.GetType().GetElementType(),15);Array.Copy(prior,first,15);Set(old,"rooms",first);Set(old,"map","manor-courtyard-v2");Call("MigrateManorCheckpoint",old);Call("ValidateChapterCheckpoint",old);Check(((Array)Get(old,"rooms")).Length==45&&((string)Get(old,"map"))=="manor-three-v3","Legacy fifteen-room save migrates without losing completed captain");
 Call("ManorAction",606);Quiet();Check((int)Get("currentRoom")==15,"Old village door continues the existing journey");
 SafeManor(17);Call("ManorAction",611);Quiet();Check(((int)Get("manorProgress")&1)==0,"Food delivery cannot skip held granary");Set(Room(18),"cleared",true);Call("ManorAction",611);Quiet();Check(((int)Get("manorProgress")&1)!=0,"Cleared granary opens real village food task");
 SafeManor(19);Call("ManorAction",612);Quiet();Check(((int)Get("manorProgress")&2)==0,"Medical delivery requires transfer depot");Set(Room(20),"cleared",true);Call("ManorAction",612);Quiet();SafeManor(24);Call("ManorAction",613);Quiet();Check((int)Get("manorProgress")==7,"Food medicine and workshop evidence persist independently");
 int dir=(int)Call("NeighborDirection",Room(24),Room(29));Check(!(bool)Call("ManorGateLocked",24,dir),"Seal guardian gate opens after helping village and reading evidence");
 foreach(int room in new[]{29,44}){
 if(room==44){Call("ManorAction",615);Quiet();Check((int)Get("currentRoom")==30,"Seal exit enters no-day inner court");}
 Enter(room);object boss=List("enemies").Cast<object>().First(e=>(bool)Get(e,"boss"));Check(Get(boss,"kind").ToString()==(room==29?"ManorSeal":"ManorKnight"),"Correct distinct later-floor boss spawns");Check(Mathf.Abs((float)Get(boss,"maximum")-(room==29?4160:4420))<.1f,"Later boss health is only slightly above captain");
 object state=Activator.CreateInstance(typeof(NakiPrototype).GetNestedType("ManorCaptainState",BindingFlags.NonPublic));((IDictionary)Get("manorCaptains"))[boss]=state;
 for(int move=0;move<5;move++){Set(boss,"attackNumber",move);Set(boss,"strikePoint",new Vector2(2,1));Set(boss,"difficultyPhase",1);Call("ResolveManorCaptain",boss,state);Check((float)Get(boss,"recoveryUntil")>=F("clock"),"Later boss attack executes "+room+" / "+move);}
 Check((int)Get(state,"bursts")>0,"Later boss has real queued follow-up fire");Set(state,"bursts",0);Set(state,"shieldUntil",0f);Set(boss,"windup",0f);Set(boss,"chargeUntil",0f);Call("ClearBossSignatures");Call("ClearManorImpacts");Call("TickBossBreathing",boss,.01f);Set("clock",F("clock")+7.6f);Check((bool)Call("TickBossBreathing",boss,.1f),"New boss uses natural rest window");
 DefeatManor();Check((bool)Get(Room(room),"cleared"),"Later boss room completes without ending whole chapter prematurely");int expected=room==29?133:134;Check(((IList)Get(Room(room),"drops")).Cast<object>().Count(d=>(int)Get(d,"id")==expected)==1,"Unique later boss weapon awarded once");Call("RewardBossWeapon",Room(room));Check(((IList)Get(Room(room),"drops")).Cast<object>().Count(d=>(int)Get(d,"id")==expected)==1,"Unique weapon cannot duplicate");
 }
 Set("origin",EnumVal("Origin","Light"));for(int i=0;i<2;i++){Call("ManorAction",720+i);Quiet();Check((int)Get("manorEnding")==0,"Light ending waits for all contract nodes");}Call("ManorAction",722);Check((int)Get("manorEnding")==1&&(bool)Get("chapterCg")&&!(bool)Get("chapterEnding"),"Light contract release starts its CG before completion");Call("FinishChapterEnding");Check((bool)Get("chapterEnding")&&(bool)Get("manorEndingSeen"),"Light CG completes the manor chapter");Check((bool)Call("LoadChapterCheckpoint",false)&&(int)Get("manorEnding")==1,"Light completion survives reload");
 // Create a preserved pre-choice safe point for the Philosopher Stone endings.
 Set("chapterEnding",false);Set("chapterCg",false);Set("manorEnding",0);Set("manorEndingSeen",false);Set("origin",EnumVal("Origin","Dark"));Set("darkPower",807f);Check((bool)Call("SaveChapterCheckpoint",false),"Pre-choice save preserves full stone reserve");string beforeChoice=File.ReadAllText(Out+"manor.json");Call("ManorAction",723);Check((int)Get("manorChoiceMode")==1&&(int)Get("manorEnding")==0,"Interacting does not immediately sacrifice anyone");
 foreach(int ending in new[]{2,3}){Set("manorChoiceMode",0);Call("BeginManorEnding",ending);Check(F("darkPower")==807,"Ending does not reset accumulated stone energy");Call("FinishChapterEnding");Check((bool)Get("manorEndingSeen")&&(int)Get("manorEnding")==ending,"Distinct tragic ending rendered and recorded");Check(!(bool)Call("SaveChapterCheckpoint",false)&&File.ReadAllText(Out+"manor.json")==beforeChoice,"Tragic ending cannot overwrite reversible decision save");Check(File.Exists(Out+"manor.json.ending-"+ending+".json"),"Ending journal is stored separately");Check((bool)Call("LoadChapterCheckpoint",false)&&(int)Get("manorEnding")==0&&F("darkPower")==807,"Reload returns to pre-choice progress and reserve");}
 Check(Resources.Load<Texture2D>("NakiArt/ManorFinaleV3/endings")!=null&&Resources.Load<Texture2D>("NakiArt/ManorFinaleV3/bosses")!=null,"Final cinematic and both new boss artworks are available");
 }
'''
s=s.replace(' static void Tick()',extra+' static void Tick()').replace('natural volley completion, retained projectiles and hazards, independent minions and boss-only rest.','three-floor Manor, legacy saves, food/medicine gates, distinct bosses, unique drops and three reversible ending branches.')
s=s.replace('Time.timeScale=0;ForestPose(6);','Time.timeScale=0;ManorPose(19);').replace('if(stage==2){ForestPose(1);','if(stage==2){ManorPose(29);').replace('p.enabled=false;DivinePose();','p.enabled=false;ManorPose(44);').replace('forest-boss-corrected.png','manor-village.png').replace('forest-room-cover.png','manor-seal.png').replace('divine-light-three-pulses.png','manor-knight.png')
(o/'Smoke.cs').write_text(s,encoding='utf-8');qa=r.parent/'Validation/DesertV2/UnitySmokeProject'
for p in (qa/'Assets/NakiPrototype/Editor').glob('*Smoke.cs'):p.rename(p.with_suffix('.cs.v11-visual-disabled'))
for src in (r/'Assets/NakiPrototype').rglob('*'):
 if src.is_file():
  dst=qa/'Assets/NakiPrototype'/src.relative_to(r/'Assets/NakiPrototype')
  if not dst.exists() or src.read_bytes()!=dst.read_bytes():dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dst)
shutil.copy2(o/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/ManorFullV11Smoke.cs')

