from pathlib import Path
import shutil
r=Path('D:/NakiLegend/Repository');o=r/'Validation/NaturalRecoveryV10';o.mkdir(exist_ok=True);(o/'runtime').mkdir(exist_ok=True)
s=(r/'Validation/BossOffenseV9/Smoke.cs').read_text(encoding='utf-8-sig').replace('NakiBossOffenseV9Smoke','NakiNaturalRecoveryV10Smoke').replace('Naki.BossOffenseV9','Naki.NaturalRecoveryV10').replace('Validation/BossOffenseV9/runtime/','Validation/NaturalRecoveryV10/runtime/').replace('BossBreathingV8Tests();','NaturalRecoveryV10Tests();')
extra=''' static void NaturalRecoveryV10Tests(){
 for(int island=0;island<6;island++){
 if(island==0){Fresh();Select();Enter(Find("Boss"));}else if(island==1)Chapter(0,0);else if(island==2){FullIce();Enter(Find("Boss"));}else if(island==3)FirePose(12);else if(island==4)ForestPose(6);else ManorPose(14);
 Quiet();object boss=List("enemies").Cast<object>().First(e=>(bool)Get(e,"boss"));Call("ClearBossSignatures");
 Call("TickBossBreathing",boss,.01f);Set("clock",F("clock")+7.6f);Set(boss,"windup",.6f);Set(boss,"chargeUntil",0f);Set(boss,"phaseUntil",0f);
 Check(!(bool)Call("TickBossBreathing",boss,.01f)&&!(bool)Get("BossResting"),"Boundary waits for current windup island "+island);Check((float)Get(boss,"windup")==.6f,"Pending tell not cancelled");Set(boss,"windup",0f);
 int n=List("shots").Count;Call("SpawnShot",Get(boss,"position"),Vector2.up,true,null,(Vector2?)Get(boss,"position"));Check(List("shots").Count==n+1,"Real hostile projectile created before rest");object shot=List("shots")[n];float life=(float)Get(shot,"life");object root=Get(shot,"root");
 Call("QueueManorImpact",boss,new Vector2(2,2),1f,1.2f,1f,false);int impacts=List("manorImpacts").Count;
 Check((bool)Call("TickBossBreathing",boss,.1f),"Finished attack transitions into rest");Check((float)Get(shot,"life")==life&&Get(shot,"root")==root&&List("shots").Contains(shot),"Transition preserves live projectile without resetting or recalling it");Check(List("manorImpacts").Count==impacts,"Already queued impact remains in world");
 object minion=Spawn("SandScorpion",new Vector2(3,0));Set(minion,"windup",.4f);Set(minion,"nextAttack",F("clock")+.2f);Check(!(bool)Call("TickBossBreathing",minion,.1f)&&(float)Get(minion,"windup")==.4f,"Minion windup is independent of boss rest");
 n=List("shots").Count;Call("SpawnShot",Get(boss,"position"),Vector2.right,true,null,(Vector2?)Get(boss,"position"));Check(List("shots").Count==n+1,"Minions and queued attacks may emit projectiles during boss rest");
 Set("invulnerableUntil",0f);Set("shield",4f);Call("TakeJourneyDamage",1f,EnumVal("Element","Physical"));Check(F("shield")<4,"Existing attacks remain dangerous; no global rest immunity");
 Vector2 before=(Vector2)Get(boss,"position");Call("TickBossBreathing",boss,.2f);Check(Vector2.Distance(before,(Vector2)Get(boss,"position"))>.01f,"Boss still adjusts position while resting");
 Set("clock",F("clock")+3.4f);Check((bool)Get("BossResting"),"Natural rest retains full 3.5 second duration");Set("clock",F("clock")+.2f);Check(!(bool)Get("BossResting"),"Rest expires normally");
 }
 ManorPose(14);Quiet();object captain=List("enemies")[0];Call("TickBossBreathing",captain,.01f);Set("clock",F("clock")+7.6f);
 object state=Activator.CreateInstance(typeof(NakiPrototype).GetNestedType("ManorCaptainState",BindingFlags.NonPublic));((IDictionary)Get("manorCaptains"))[captain]=state;Set(state,"bursts",2);Set(state,"nextBurst",F("clock"));Set(captain,"windup",0f);Set(captain,"chargeUntil",0f);Set(captain,"phaseUntil",0f);
 Check(!(bool)Call("TickBossBreathing",captain,.01f),"Captain rest waits for scheduled multi-wave volley");int count=List("shots").Count;Call("UpdateTacticalEnemy",captain,.01f);Check((int)Get(state,"bursts")==1&&List("shots").Count>count,"First pending fan completes naturally");Set("clock",F("clock")+.21f);Call("UpdateTacticalEnemy",captain,.01f);Check((int)Get(state,"bursts")==0,"Last pending fan completes naturally");Check((bool)Call("TickBossBreathing",captain,.01f),"Captain enters rest after final volley");
 }
'''
s=s.replace(' static void Tick()',extra+' static void Tick()').replace('six-island active-phase density, preserved warning and slower speed, unchanged health cuts and full breathing windows.','natural volley completion, retained projectiles and hazards, independent minions and boss-only rest.')
(o/'Smoke.cs').write_text(s,encoding='utf-8');qa=r.parent/'Validation/DesertV2/UnitySmokeProject'
for p in (qa/'Assets/NakiPrototype/Editor').glob('*Smoke.cs'):p.rename(p.with_suffix('.cs.v10-disabled'))
for src in (r/'Assets/NakiPrototype').rglob('*.cs'):shutil.copy2(src,qa/'Assets/NakiPrototype'/src.relative_to(r/'Assets/NakiPrototype'))
shutil.copy2(o/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/NaturalRecoveryV10Smoke.cs')
