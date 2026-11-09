from pathlib import Path
import shutil
root=Path(__file__).resolve().parents[2];out=root/'Validation/FiveIslandPressureV2'
s=(root/'Validation/MingshaPolishV2/Smoke.cs').read_text(encoding='utf-8').replace('NakiMingshaPolishV2Smoke','NakiFiveIslandPressureV2Smoke').replace('Naki.MingshaPolishV2','Naki.FiveIslandPressureV2').replace('Validation/MingshaPolishV2/runtime/','Validation/FiveIslandPressureV2/runtime/')
s=s.replace('MingshaPolishTests();p.','MingshaPolishTests();FiveIslandPressureTests();p.')
s=s.replace('Call("EnemyHealthScale",false)-1.30f','Call("EnemyHealthScale",false)-1.43f').replace('Call("EnemyHealthScale",true)-5.2f','Call("EnemyHealthScale",true)-5.82f').replace('Call("EnemyHealthScale",false)-1.48f','Call("EnemyHealthScale",false)-1.70f').replace('Other islands retain their existing enemy health tuning','Thunder uses its updated health tuning')
s=s.replace('augmented>=old+1&&augmented<=old+2','augmented==old+(int)Get("TacticalCompanionCount")')
s=s.replace('List("pendingSpawns").Count==before+2,"Ordinary encounter receives a two-role reinforcement formation"','List("pendingSpawns").Count==before+(int)Get("TacticalCompanionCount"),"Ordinary encounter receives the new mixed reinforcement formation"')
s=s.replace('List("pendingSpawns").Count==before+2,"Tactical reinforcements are added once, not once per frame"','List("pendingSpawns").Count==before+(int)Get("TacticalCompanionCount"),"Tactical reinforcements are added once, not once per frame"')
s=s.replace('(scheduled-20-(float)Get(enemy,"windup"))*.85f','(scheduled-20-(float)Get(enemy,"windup"))*.85f*F("EncounterRecoveryScale")')
s=s.replace('List("shots").Count==5,"Final-phase boss resolves the warned fan"','List("shots").Count==7,"Final-phase boss resolves all seven warned projectiles"')
s=s.replace(' static void Next(){',(out/'tests.txt').read_text(encoding='utf-8')+'\n static void Next(){')
(out/'Smoke.cs').write_text(s,encoding='utf-8')
qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
for src in (root/'Assets/NakiPrototype').rglob('*'):
 if src.is_file():
  dst=qa/'Assets/NakiPrototype'/src.relative_to(root/'Assets/NakiPrototype')
  if not dst.exists() or src.read_bytes()!=dst.read_bytes():dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dst)
shutil.copy2(out/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/FiveIslandPressureV2Smoke.cs')
lines=(root/'Validation/MingshaPolishV2/compile.rsp').read_text(encoding='utf-8-sig').splitlines()
lines=[x for x in lines if not x.strip('"').endswith('.cs') and not x.strip('"').startswith('-out:')]
lines+=['"-out:'+str(out/'runtime-compile.dll')+'"']+['"'+str(p)+'"' for p in (root/'Assets/NakiPrototype').rglob('*.cs')]
(out/'compile.rsp').write_text('\n'.join(lines),encoding='utf-8')
script=(root/'Validation/MingshaPolishV2/sync_runtime.py').read_text(encoding='utf-8').replace('Validation/MingshaPolishV2/runtime/pre-mingsha-sync','Validation/FiveIslandPressureV2/runtime/pre-pressure-sync')
(out/'sync_runtime.py').write_text(script,encoding='utf-8')
print('Staged five-island health, warning, companion, cap and teaching regression tests')
