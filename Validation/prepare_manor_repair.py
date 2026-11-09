from pathlib import Path
import shutil
r=Path('D:/NakiLegend/Repository');out=r/'Validation/ManorRepairV12';qa=r.parent/'Validation/DesertV2/UnitySmokeProject'
s=(r/'Validation/ManorFullV11/Smoke.cs').read_text(encoding='utf-8-sig').replace('NakiManorFullV11Smoke','NakiManorRepairV12Smoke').replace('Naki.ManorFullV11','Naki.ManorRepairV12').replace('Validation/ManorFullV11/runtime/','Validation/ManorRepairV12/runtime/')
tests='''
 static int Actions(params int[] kinds){return List("chapterActions").Cast<object>().Count(a=>kinds.Contains((int)Get(a,"kind")));}
 static void ManorRepairTests(){
  ManorPose(0);Set("chapterLampGiven",true);Set(Room(14),"cleared",true);Set("chapterStone",true);Set("chapterWaymarks",3);Set(Room(4),"cleared",true);Set(Room(13),"cleared",true);Set("manorProgress",7);Set(Room(29),"cleared",true);
  var atlas=Resources.Load<Texture2D>("NakiArt/ManorVillageV4/props");Check(atlas!=null&&atlas.width==1536&&atlas.height==1024&&atlas.GetPixel(0,0).a==0,"Native village atlas has six clean transparent cells");
  for(int cell=0;cell<6;cell++){var prop=(Sprite)Call("ManorVillageProp",cell,2f);Check(prop.texture==atlas&&prop.rect.width==512&&prop.rect.height==512,"Village prop uses its own atlas cell "+cell);}
  for(int id=0;id<45;id++){
   Enter(id);ClearEnemies();Set(Room(id),"cleared",true);Call("BuildChapterActions");Call("CreateStairsIfReady");
   int count=Actions(606,610,615,616);Check(count==((id==14||id==15||id==29||id==30)?1:0),"Exactly one authored cross-floor exit, only in boundary rooms "+id);
   Check(Get("stairs")==null,"No generic duplicate stair sprite in Manor room "+id);
   if(count==1){object a=List("chapterActions").Cast<object>().First(x=>new[]{606,610,615,616}.Contains((int)Get(x,"kind")));Vector2 at=(Vector2)Get(a,"at");Check((bool)Call("SafeFooting",at,.34f),"Passage interaction can be reached "+id);Check(((string)Get(a,"label")).Contains(id==14||id==30?"断粮旧村":id==15?"归功外庭":"无昼内庭"),"Passage label matches actual destination "+id);}
  }
  foreach(int id in new[]{17,19,23}){Enter(id);Vector2 door=new Vector2(-4.7f,.9f);Check((bool)Call("SafeFooting",door,.34f),"Villager remains reachable in front of building "+id);int blocks=List("obstacles").Count;Call("BuildChapterActions");Call("BuildChapterActions");Check(List("obstacles").Count==blocks,"Rebuilding interactions cannot stack building collision "+id);}
  Enter(14);Call("ManorAction",606);Quiet();Check((int)Get("currentRoom")==15,"Single village doorway still enters second floor");Call("ManorAction",610);Quiet();Check((int)Get("currentRoom")==14,"Village return doorway has correct destination");Enter(29);Call("ManorAction",615);Quiet();Check((int)Get("currentRoom")==30,"Single inner-court doorway enters third floor");Call("ManorAction",616);Quiet();Check((int)Get("currentRoom")==29,"Inner-court return doorway has correct destination");
  Set("manorProgress",0);Enter(15);Check((int)Get("FlowGoal")==17,"Already-cleared granary guides toward the grain keeper, not old captain");Set("manorProgress",1);Check((int)Get("FlowGoal")==19,"After food, guidance follows medicine delivery");Set("manorProgress",3);Check((int)Get("FlowGoal")==24,"After deliveries, guidance follows the archive");Set("manorProgress",7);Check((int)Get("FlowGoal")==44,"After seal boss, guidance continues toward the great knight");
  Enter(32);Call("ManorAction",617);string voyage=Story();Quiet();Enter(34);Call("ManorAction",617);string royal=Story();Quiet();Enter(41);Call("ManorAction",617);string appeal=Story();Check(voyage!=royal&&royal!=appeal&&appeal!=voyage,"Three investigation rooms reveal distinct evidence");Quiet();
  Enter(44);ClearEnemies();Set(Room(44),"cleared",true);Set("manorEnding",0);Set("origin",EnumVal("Origin","Light"));Set("manorProgress",7);Call("BuildChapterActions");Check(Actions(720,721,722)==3&&Actions(723)==0,"Light ending exposes only three royal seals");Set("manorProgress",15);Call("BuildChapterActions");Check(Actions(720,721,722)==2&&Actions(723)==0,"Completed light node never creates a stone-route choice");
  Set("origin",EnumVal("Origin","Dark"));Call("BuildChapterActions");Check(Actions(723)==1&&Actions(720,721,722)==0,"Philosopher Stone route actually exposes one decision interaction");
  Set("allowBatchCheckpoints",true);Set("chapterCheckpointPathOverride",Out+"repair-save.json");Check((bool)Call("SaveChapterCheckpoint",false),"Repaired chapter keeps existing safe-save format");Check((bool)Call("LoadChapterCheckpoint",false)&&Actions(723)==1&&Get("stairs")==null,"Reload restores correct choice and no duplicate exit");Quiet();
 }
 static void RepairPreview(int room){ManorPose(room);Set("manorIntro",false);Set("chapterLampGiven",true);Set(Room(14),"cleared",true);Set("chapterStone",true);Set("chapterWaymarks",3);Set(Room(4),"cleared",true);Set(Room(13),"cleared",true);Set("manorProgress",7);Set(Room(29),"cleared",true);ClearEnemies();Set(Room(room),"cleared",true);Call("BuildChapterActions");Set("position",new Vector2(0,1));Call("ConfigureJourneyView");((Camera)Get("view")).transform.position=(Vector3)Get("JourneyViewCenter");Call("UpdatePixelHero",Vector2.zero);Quiet();}
'''
s=s.replace(' static void Tick()\n',tests+' static void Tick()\n')
s=s.replace('ManorFullV11Tests();','ManorFullV11Tests();ManorRepairTests();')
s=s.replace('Time.timeScale=0;ManorPose(19);Set("position",new Vector2(5,-.5f));','Time.timeScale=0;RepairPreview(15);Set("position",new Vector2(0,1));')
s=s.replace('p.enabled=false;ForestPose(1);Set("position",new Vector2(0,-1));','p.enabled=false;RepairPreview(19);Set("position",new Vector2(-1,1));')
s=s.replace('p.enabled=false;ManorPose(44);p.enabled=true;','p.enabled=false;RepairPreview(29);p.enabled=true;')
s=s.replace('manor-village.png','village-gate.png').replace('manor-seal.png','village-clinic.png').replace('manor-knight.png','seal-exit.png')
s=s.replace('Native forest rendering has no runtime errors','Manor repair rendering has no runtime errors')
s=s.replace('three-floor Manor, legacy saves, food/medicine gates, distinct bosses, unique drops and three reversible ending branches.','Manor village assets, single exits, safe passage, progressive navigation, distinct evidence, both ending interactions and old-save compatibility.')
(out/'Smoke.cs').write_text(s,encoding='utf-8')
for p in (qa/'Assets/NakiPrototype/Editor').glob('*Smoke.cs'):p.rename(p.with_suffix('.cs.manor-repair-disabled'))
shutil.copy2(out/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/ManorRepairV12Smoke.cs')
for rel in (r/'Validation/manor-repair-files.txt').read_text(encoding='utf-8').splitlines():
 dst=qa/rel;dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(r/rel,dst)
print('Staged Manor repair and native regression suite')
