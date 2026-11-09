from pathlib import Path
import shutil
r=Path('D:/NakiLegend/Repository');out=r/'Validation/BossDuelsV13';qa=r.parent/'Validation/DesertV2/UnitySmokeProject'
manifest=r/'Validation/boss-duels-files.txt';paths=manifest.read_text(encoding='utf-8').splitlines()
for rel in ['Assets/NakiPrototype/NakiPrototype.Origin.cs','Assets/NakiPrototype/NakiPrototype.Origin.cs.meta']:
 if rel not in paths:paths.append(rel)
manifest.write_text('\n'.join(paths)+'\n',encoding='utf-8')
s=(r/'Validation/ManorRepairV12/Smoke.cs').read_text(encoding='utf-8-sig').replace('NakiManorRepairV12Smoke','NakiBossDuelsV13Smoke').replace('Naki.ManorRepairV12','Naki.BossDuelsV13').replace('Validation/ManorRepairV12/runtime/','Validation/BossDuelsV13/runtime/')
tests='''
 static void DuelTests(){
 string[] kinds={"WindScorpion","Colossus","ApeKing","CopperWarden","ThunderSentinel","StormStag","IceBeast","FrostMirror","TideGuardian","RedSpineLizard","ForgeMaster","MagmaTortoise"};
 for(int id=0;id<17;id++){
  int island=id<12?id/3:id<15?4:5;MasteryFixture(island);Set("clock",100f);Set("position",new Vector2(0,-3));((Camera)Get("view")).transform.position=new Vector3(0,0,-10);
  string kind=id<12?kinds[id]:id<15?((Array)Get("WoodKinds")).GetValue(id-3).ToString():id==15?"ManorSeal":"ManorKnight";
  object boss=Spawn(kind,new Vector2(0,1));Set(boss,"hp",10000f);Set(boss,"maximum",10000f);Set(boss,"difficultyPhase",2);ClearShots();Call("ClearBossSignatures");
  Call("BeginDuelMechanic",boss,id);Check(List("duelSalvos").Count==1,"Boss has a committed combination salvo "+kind);Call("TickDuelSalvos");Check(List("shots").Count==0,"Combination waits for its readable preparation "+kind);
  bool laser=new[]{1,4,7,10,11,14,15,16}.Contains(id);Check(List("signatureZones").Cast<object>().Any(z=>(int)Get(z,"mode")==8)==laser,"Only appropriate bosses receive sustained beam "+kind);
  if(laser){Check((bool)Call("BossAttackFinishing",boss),"Recovery waits for committed beam "+kind);Check(((string)Call("BossCombatHint",boss)).Contains("蓄势"),"Beam cue explains current threat "+kind);}
  int max=0;for(int frame=0;frame<16;frame++){Set("clock",100.9f+frame*.25f);Call("TickBossSignatures",.25f);max=Math.Max(max,List("shots").Count);}
  Check(max>=12,"Multi-wave attack pressures space without faster bullets "+kind);Check(List("duelSalvos").Count==0,"Committed salvo finishes naturally "+kind);
  foreach(object shot in List("shots"))Check((float)Get(shot,"speed")<=4.5f,"New pattern keeps moderate raw projectile speed");
  Call("ClearBossSignatures");Check(List("duelSalvos").Count==0&&List("signatureZones").Count==0,"Room cleanup removes owned attacks "+kind);
 }
 ManorPose(14);ClearShots();Call("ClearBossSignatures");object captain=List("enemies")[0];float hp=(float)Get(captain,"maximum");Call("BeginDuelMechanic",captain,16);Check(List("duelSalvos").Count==0&&List("signatureZones").Count==0&&(float)Get(captain,"maximum")==hp,"Approved Oath Captain receives no new attack package or health change");
 // Beam must be harmless during its tell, damage when active, and stop at physical cover.
 MasteryFixture(0);Set("clock",100f);object stone=Spawn("Colossus",Vector2.zero);Set(stone,"hp",10000f);Set(stone,"maximum",10000f);Set("position",new Vector2(0,3));Set("invulnerableUntil",0f);Set("swordUntil",0f);Set("shield",10f);Set("health",6f);Call("AddSignatureZone",stone,8,Vector2.zero,Vector2.up,5f,2.4f);Call("TickBossSignatures",0f);Check(F("shield")==10,"Beam preparation never deals damage");
 Set("clock",100.9f);Call("TickBossSignatures",.01f);Check(F("shield")<10,"Active beam actually damages the player");Set("shield",10f);Set("invulnerableUntil",0f);Set("clock",101.8f);List("projectileObstacles").Add(new Rect(-.8f,1,1.6f,.3f));Call("TickBossSignatures",.01f);Check(F("shield")==10,"Cover blocks sustained beam damage");
 object z=List("signatureZones")[0];Check(((GameObject)Get(z,"left")).transform.localScale.x<=1,"Visible beam stops at the same cover as damage");
 List("projectileObstacles").Clear();Set(z,"radius",100f);Call("TickDuelBeam",z);Check(((GameObject)Get(z,"left")).transform.localScale.x<=((Rect)Get("CurrentRoomBounds")).yMax+.01f,"Beam also stops at room edge instead of drawing beyond the arena");
 // A due recovery cannot discard a queued attack; spent bullets survive the transition.
 MasteryFixture(0);Set("clock",100f);stone=Spawn("Colossus",Vector2.zero);Call("TickBossBreathing",stone,.01f);Set("clock",107.6f);Call("BeginDuelMechanic",stone,1);Check(!(bool)Call("TickBossBreathing",stone,.01f),"Recovery waits instead of interrupting a new committed attack");
 for(int frame=0;frame<20;frame++){Set("clock",108.5f+frame*.25f);Call("TickBossSignatures",.25f);}int live=List("shots").Count;Check((bool)Call("TickBossBreathing",stone,.01f)&&List("shots").Count==live&&live>0,"Natural rest preserves already emitted projectiles");
 // Basic melee clears a bounded frontal arc, not all bullets and not a reflected fan.
 MasteryFixture(0);Set("position",Vector2.zero);Set("clock",100f);Set("swingDirection",Vector2.up);Set("profession",EnumVal("Profession","Warrior"));((int[])Get("carriedWeapons"))[0]=0;Set("equippedSlot",0);ClearShots();
 for(int i=0;i<5;i++)Call("SpawnShot",new Vector2(.05f,.4f+i*.15f),Vector2.down,true,null,new Vector2(.05f,.4f+i*.15f));Call("SpawnShot",new Vector2(0,-.8f),Vector2.up,true,null,new Vector2(0,-.8f));int count=List("shots").Count;Call("CutMeleeProjectiles");Check(List("shots").Count==count-3,"One ordinary melee impact cuts at most three frontal bullets");Check(List("shots").Cast<object>().All(a=>(bool)Get(a,"hostile")),"Basic cutting does not grant the reflection talent");Check(List("shots").Cast<object>().Any(a=>((Vector2)Get(a,"position")).y<0),"Shots from behind remain dangerous");
 ClearShots();Set("shield",10f);Set("health",6f);Set("invulnerableUntil",0f);Set("swordUntil",100.3f);Set("swordHitPending",true);float attack=F("EnemyAttackScale");Call("TakeJourneyDamage",1f,EnumVal("Element","Physical"));Check(Mathf.Abs(F("shield")-(10-.7f*attack))<.001f&&(bool)Get("swordHitPending"),"Committed warrior strike takes reduced damage and still lands");
 Set("profession",EnumVal("Profession","Mage"));Set("shield",10f);Set("invulnerableUntil",0f);Set("swordHitPending",true);Set("swordUntil",100.3f);Call("TakeJourneyDamage",1f,EnumVal("Element","Physical"));Check(Mathf.Abs(F("shield")-(10-attack))<.001f&&!(bool)Get("swordHitPending"),"Other professions do not gain warrior commitment armor");
 Array catalog=(Array)Get("weaponCatalog");float melee=(float)Call("WeaponOutputScale",catalog.GetValue(0)),staff=(float)Call("WeaponOutputScale",catalog.GetValue(3));Check(melee>1.05f&&staff<1.1f,"Starter balance narrows staff versus melee damage gap");
 }
 static void DuelPreview(){MasteryFixture(3);object e=Spawn("ForgeMaster",new Vector2(1,2));Set(e,"hp",10000f);Set("position",new Vector2(-2,-1));Set("clock",100f);Call("BeginDuelMechanic",e,10);Set("clock",101.2f);Call("TickBossSignatures",.01f);Set("invulnerableUntil",10000f);Call("ConfigureJourneyView");((Camera)Get("view")).transform.position=(Vector3)Get("JourneyViewCenter");Call("UpdatePixelHero",Vector2.zero);Quiet();}
'''
s=s.replace(' static void Tick()\n',tests+' static void Tick()\n').replace('ManorFullV11Tests();ManorRepairTests();','ManorFullV11Tests();ManorRepairTests();DuelTests();')
s=s.replace('RepairPreview(15);Set("position",new Vector2(0,1));','DuelPreview();Set("position",new Vector2(-2,-1));')
s=s.replace('Manor village assets, single exits, safe passage, progressive navigation, distinct evidence, both ending interactions and old-save compatibility.','boss combinations, beam collision, natural recovery, preserved captain, melee counterplay, weapon balance and Manor regressions.')
(out/'Smoke.cs').write_text(s,encoding='utf-8')
for p in (qa/'Assets/NakiPrototype/Editor').glob('*Smoke.cs'):p.rename(p.with_suffix('.cs.duel-disabled'))
shutil.copy2(out/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/BossDuelsV13Smoke.cs')
for rel in paths:
 dst=qa/rel;dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(r/rel,dst)
print('Staged isolated native boss-duel regression suite')
