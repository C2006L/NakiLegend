from pathlib import Path
import shutil
r=Path('D:/NakiLegend/Repository');o=r/'Validation/BossBreathingV8';o.mkdir(exist_ok=True);(o/'runtime').mkdir(exist_ok=True)
s=(r/'Validation/ForestEndingV7/Smoke.cs').read_text(encoding='utf-8').replace('NakiForestEndingV7Smoke','NakiBossBreathingV8Smoke').replace('Naki.ForestEndingV7','Naki.BossBreathingV8').replace('Validation/ForestEndingV7/runtime/','Validation/BossBreathingV8/runtime/').replace('ForestSeedFlowV7Tests();','BossBreathingV8Tests();')
extra=''' static void BossBreathingV8Tests(){
 for(int island=0;island<6;island++){
 if(island==0){Fresh();Select();Enter(Find("Boss"));}else if(island==1)Chapter(0,0);else if(island==2){FullIce();Enter(Find("Boss"));}else if(island==3)FirePose(12);else if(island==4)ForestPose(6);else ManorPose(14);
 Quiet();object boss=List("enemies").Cast<object>().First(e=>(bool)Get(e,"boss"));
 Check(Mathf.Abs((float)Call("EnemyHealthScale",false)-(island==5?.75f:new[]{1.5f,1.85f,2.25f,2.07f,2.28f}[island]*.75f))<.001f,"Ordinary health reduced 25 percent island "+island);
 float expected=island==5?.65f:new[]{5.95f,5.1f,6.45f,6.05f,7.04f}[island]*.65f*(island==4?.7f:1);
 Check(Mathf.Abs((float)Call("EnemyHealthScale",true)-expected)<.001f,"Boss health reduced 35 percent island "+island);
 Set("invulnerableUntil",0f);Call("TickBossBreathing",boss);Set("clock",F("clock")+7.6f);Call("UpdateTacticalEnemy",boss,.01f);Check((bool)Get("BossResting"),"Boss enters output opening island "+island);
 float hp=F("health"),shield=F("shield");Call("TakeJourneyDamage",2f,EnumVal("Element","Physical"));Check(F("health")==hp&&F("shield")==shield,"Lingering hazard cannot damage during output opening");
 int count=List("shots").Count;Call("SpawnShot",Get(boss,"position"),Vector2.right,true,null,(Vector2?)Get(boss,"position"));Check(List("shots").Count==count,"No delayed or reinforcement projectiles during opening");
 Set("clock",F("clock")+3.4f);Check((bool)Get("BossResting"),"Opening lasts at least 3.4 seconds");Set("clock",F("clock")+.2f);Check(!(bool)Get("BossResting"),"Combat resumes after 3.5 seconds");
 float speed=(float)Get("HostileProjectileSpeedScale");Check(speed<1,"Boss arena hostile projectile speed softened");
 if(island==5)Check(Mathf.Abs((float)Get(boss,"maximum")-3978)<.1f,"Captain real spawned health reduced from 6120 to 3978");
 }
 }
'''
s=s.replace(' static void Tick()',extra+' static void Tick()');(o/'Smoke.cs').write_text(s,encoding='utf-8')
qa=r.parent/'Validation/DesertV2/UnitySmokeProject'
for p in (qa/'Assets/NakiPrototype/Editor').glob('*Smoke.cs'):p.rename(p.with_suffix('.cs.v8-retry-disabled'))
for src in (r/'Assets/NakiPrototype').rglob('*.cs'):shutil.copy2(src,qa/'Assets/NakiPrototype'/src.relative_to(r/'Assets/NakiPrototype'))
shutil.copy2(r/'Assets/NakiPrototype/NakiPrototype.BossBreathing.cs.meta',qa/'Assets/NakiPrototype/NakiPrototype.BossBreathing.cs.meta')
shutil.copy2(o/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/BossBreathingV8Smoke.cs')

