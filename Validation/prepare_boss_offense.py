from pathlib import Path
import shutil
r=Path('D:/NakiLegend/Repository');o=r/'Validation/BossOffenseV9';o.mkdir(exist_ok=True);(o/'runtime').mkdir(exist_ok=True)
s=(r/'Validation/BossBreathingV8/Smoke.cs').read_text(encoding='utf-8-sig').replace('NakiBossBreathingV8Smoke','NakiBossOffenseV9Smoke').replace('Naki.BossBreathingV8','Naki.BossOffenseV9').replace('Validation/BossBreathingV8/runtime/','Validation/BossOffenseV9/runtime/')
s=s.replace('Quiet();object boss=List("enemies").Cast<object>().First(e=>(bool)Get(e,"boss"));','''Quiet();object boss=List("enemies").Cast<object>().First(e=>(bool)Get(e,"boss"));
 object[] normalFan={10,10f};typeof(NakiPrototype).GetMethod("DensifyFan",B).Invoke(p,normalFan);
 Set("bossAttackEmission",true);object[] bossFan={10,10f};typeof(NakiPrototype).GetMethod("DensifyFan",B).Invoke(p,bossFan);Set("bossAttackEmission",false);
 Check((int)bossFan[0]>(int)normalFan[0],"Boss active-phase fans denser than ordinary attacks island "+island);
 Check(Mathf.Abs(((int)bossFan[0]-1)*(float)bossFan[1]-90)<.01f,"Added bullets preserve original fan span");
 Set("bossAttackEmission",true);int n=List("shots").Count;Call("PressureFan",Get("position"),Vector2.up,10,10f,4.5f,EnumVal("Element","Dark"),1.2f);Set("bossAttackEmission",false);
 Check(List("shots").Count-n==12,"Actual pressure fan emits twelve instead of ten during boss attack");
 foreach(object shot in List("shots"))if((bool)Get(shot,"hostile"))Check((float)Get(shot,"speed")==4.5f,"Density buff does not raise projectile speed");
 if(island==5){Set(boss,"attackNumber",4);Set(boss,"nextAttack",0f);Set(boss,"recoveryUntil",0f);Set(boss,"windup",0f);Call("UpdateTacticalEnemy",boss,.01f);float tell=(float)Get(boss,"windup");Check(Mathf.Abs(tell-.6f)<.01f,"Captain warning duration retained");Check((float)Get(boss,"nextAttack")-F("clock")-tell<.7f,"Captain active-phase inter-attack gap compressed");Set(boss,"windup",0f);}
''')
s=s.replace('PASS "+checks+" checks: Divine Light pulse/split limits, gradual Light/Stone mastery, weapon impact output, fire-counter mana budget, boss HP, cover navigation/shot blocking, legacy saves and prior forest/Manor regressions.','PASS "+checks+" checks: six-island active-phase density, preserved warning and slower speed, unchanged health cuts and full breathing windows.')
s=s.replace('Call("TickBossBreathing",boss);','Call("TickBossBreathing",boss,.01f);')
s=s.replace('Check((bool)Get("BossResting"),"Boss enters output opening island "+island);','Check((bool)Get("BossResting"),"Boss enters output opening island "+island);\n Vector2 prior=(Vector2)Get(boss,"position");Call("TickBossBreathing",boss,.4f);Check(Vector2.Distance(prior,(Vector2)Get(boss,"position"))>.05f,"Resting movable boss repositions without attacking");\n Check((int)Get("additionalCoverCount")==1,"Boss room gains one cover with reserved walking clearance");')
s=s.replace('float speed=(float)Get("HostileProjectileSpeedScale");','float speed=(float)Get("HostileProjectileSpeedScale");if(island==5)Check(speed<.7f,"Captain projectiles separately slowed below previous boss arena speed");')
s=s.replace('Quiet();object boss=List("enemies").Cast<object>().First(e=>(bool)Get(e,"boss"));', 'Quiet();object boss=List("enemies").Cast<object>().First(e=>(bool)Get(e,"boss"));\n Set(boss,"nextAttack",F("clock")+100);Set(boss,"recoveryUntil",F("clock")+100);Set(boss,"windup",0f);Vector2 activeBefore=(Vector2)Get(boss,"position");Call("UpdateTacticalEnemy",boss,.2f);Check(Vector2.Distance(activeBefore,(Vector2)Get(boss,"position"))>.05f,"Active boss repositions during short weapon recovery instead of standing still island "+island);\n if(island<5){Set(boss,"recoveryUntil",0f);Call("TickBossSignature",boss,.01f);IDictionary signatures=(IDictionary)Get("bossSignatures");object sig=signatures[boss];Check((float)Get(sig,"next")-F("clock")<2.2f,"Identity control skill starts well before breathing window");}\n')
(o/'Smoke.cs').write_text(s,encoding='utf-8');qa=r.parent/'Validation/DesertV2/UnitySmokeProject'
for p in (qa/'Assets/NakiPrototype/Editor').glob('*Smoke.cs'):
 dst=o/('archived-'+p.name);shutil.copy2(p,dst);p.rename(p.with_suffix('.cs.v9-control-disabled'))
for src in (r/'Assets/NakiPrototype').rglob('*.cs'):shutil.copy2(src,qa/'Assets/NakiPrototype'/src.relative_to(r/'Assets/NakiPrototype'))
shutil.copy2(o/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/BossOffenseV9Smoke.cs')
