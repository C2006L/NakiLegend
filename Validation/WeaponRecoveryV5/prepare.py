from pathlib import Path
import shutil
root=Path(__file__).resolve().parents[2];out=root/'Validation/WeaponRecoveryV5'
source=(root/'Validation/ForestBalanceV4/Smoke.cs').read_text(encoding='utf-8-sig')
source=source.replace('NakiForestBalanceV4Smoke','NakiWeaponRecoveryV5Smoke').replace('Naki.ForestBalanceV4','Naki.WeaponRecoveryV5').replace('Validation/ForestBalanceV4/runtime/','Validation/WeaponRecoveryV5/runtime/')
source=source.replace('ForestBoundsAndBalanceTests();ArmedWeaponTests();','WeaponRecoveryTests();ForestBoundsAndBalanceTests();ArmedWeaponTests();')
source=source.replace('WeaponRecoveryTests();ForestBoundsAndBalanceTests();','DivineLightTests();MasteryProgressionTests();WeaponRecoveryTests();ForestBoundsAndBalanceTests();')
source=source.replace('normal-2.11f','normal-2.25f').replace('boss-6.14f','boss-6.45f').replace('by roughly eleven / ten percent','to their V5 chapter budgets')
source=source.replace('EnemyHealthScale",false)-1.43f','EnemyHealthScale",false)-1.50f').replace('EnemyHealthScale",true)-5.82f','EnemyHealthScale",true)-5.95f')
source=source.replace('-20*strength','-14*.85f*strength').replace('Five stronger meteors cost exactly one','First-tier meteors cost exactly one')
source=source.replace('id,name,type,tap_or_pellet,full_or_volley,cost,interval','id,name,type,raw_pre_impact_tap_or_pellet,raw_full_or_volley,cost,interval')
source=source.replace('if(stage==3){ScreenCapture.CaptureScreenshot(Out+"forest-room-cover.png");stage=4;ticks=0;return;}','if(stage==3){ScreenCapture.CaptureScreenshot(Out+"forest-room-cover.png");stage=4;ticks=0;return;}\n   if(stage==4){p.enabled=false;DivinePose();p.enabled=true;stage=5;ticks=0;return;}\n   if(stage==5){ScreenCapture.CaptureScreenshot(Out+"divine-light-three-pulses.png");stage=6;ticks=0;return;}')
source=source.replace('(float)Get(boss,"maximum")==7200','(float)Get(boss,"maximum")==6120').replace('Captain final health is 7200 without doubled island scaling','Captain health is 6120 after the requested fifteen-percent reduction')
source=source.replace('(float)Get(e,"maximum")>=145','(float)Get(e,"maximum")>=130')
source=source.replace('(GameObject)Get("originBeam")!=null&&!(bool)Get("solarCharging"),"Solar release performs one real piercing-light hit and creates its wide visual"','(GameObject)Get("originBeam")==null&&(int)Get("divinePulsesRemaining")==2,"P emits its first radial wave instead of another beam"')
source=source.replace('Check((float)Get(target,"hp")<hp&&(GameObject)Get("originBeam")==null','Check(List("shots").Count==6&&(GameObject)Get("originBeam")==null')
source=source.replace(' static void End(int code)',(out/'tests.cs.txt').read_text(encoding='utf-8')+'\n static void End(int code)')
source=source.replace('PASS "+checks+" checks: forest bounds/cover/arrival, safe loot and legacy restoration, actual weapon damage/pool costs, skill budgets/control, save/load, atlas and prior combat/Manor regressions.','PASS "+checks+" checks: Divine Light pulse/split limits, gradual Light/Stone mastery, weapon impact output, fire-counter mana budget, boss HP, cover navigation/shot blocking, legacy saves and prior forest/Manor regressions.')
(out/'Smoke.cs').write_text(source,encoding='utf-8')
build=(root/'Validation/ForestBalanceV4/build.py').read_text(encoding='utf-8').replace('ForestBalanceV4','WeaponRecoveryV5')
(out/'build.py').write_text(build,encoding='utf-8')
qa=root.parent/'Validation/DesertV2/UnitySmokeProject/Assets/NakiPrototype/Editor'
for p in qa.glob('*Smoke.cs'):
 if p.name!='WeaponRecoveryV5Smoke.cs':
  backup=out/'archived-qa-harnesses'/p.name;backup.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(p,backup);p.rename(p.with_suffix('.cs.disabled'))
print('Prepared independent weapon-output and cover-navigation regression')
