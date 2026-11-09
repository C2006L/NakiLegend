from pathlib import Path
import shutil
root=Path(__file__).resolve().parents[2];out=root/'Validation/WeaponSupplyV2'
s=(root/'Validation/FiveIslandPressureV2/Smoke.cs').read_text(encoding='utf-8').replace('NakiFiveIslandPressureV2Smoke','NakiWeaponSupplyV2Smoke').replace('Naki.FiveIslandPressureV2','Naki.WeaponSupplyV2').replace('Validation/FiveIslandPressureV2/runtime/','Validation/WeaponSupplyV2/runtime/')
s=s.replace('FiveIslandPressureTests();p.','FiveIslandPressureTests();WeaponSupplyV2Tests();p.')
s=s.replace(' static void Next(){',(out/'tests.txt').read_text(encoding='utf-8')+'\n static void Next(){')
(out/'Smoke.cs').write_text(s,encoding='utf-8')
qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
for src in (root/'Assets/NakiPrototype').rglob('*'):
 if src.is_file():
  dst=qa/'Assets/NakiPrototype'/src.relative_to(root/'Assets/NakiPrototype')
  if not dst.exists() or src.read_bytes()!=dst.read_bytes():dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dst)
shutil.copy2(out/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/WeaponSupplyV2Smoke.cs')
lines=(root/'Validation/FiveIslandPressureV2/compile.rsp').read_text(encoding='utf-8-sig').splitlines()
lines=[x for x in lines if not x.strip('"').endswith('.cs') and not x.strip('"').startswith('-out:')]
lines+=['"-out:'+str(out/'runtime-compile.dll')+'"']+['"'+str(p)+'"' for p in (root/'Assets/NakiPrototype').rglob('*.cs')]
(out/'compile.rsp').write_text('\n'.join(lines),encoding='utf-8')
s=(root/'Validation/FiveIslandPressureV2/sync_runtime.py').read_text(encoding='utf-8').replace('Validation/FiveIslandPressureV2/runtime/pre-pressure-sync','Validation/WeaponSupplyV2/runtime/pre-supply-sync')
(out/'sync_runtime.py').write_text(s,encoding='utf-8')
print('Staged profession-aware balanced caches, old-offer repair and deterministic drop sampling')
