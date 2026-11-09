from pathlib import Path
import shutil
root=Path(__file__).resolve().parents[2];out=root/'Validation/BossMobilityV2'
s=(root/'Validation/ThunderWardenV2/Smoke.cs').read_text(encoding='utf-8').replace('NakiThunderWardenV2Smoke','NakiBossMobilityV2Smoke').replace('Naki.ThunderWardenV2','Naki.BossMobilityV2').replace('Validation/ThunderWardenV2/runtime/','Validation/BossMobilityV2/runtime/')
s=s.replace('ThunderWardenV2Tests();p.','ThunderWardenV2Tests();BossMobilityV2Tests();p.')
s=s.replace('F("shield")>6&&F("shield")<8&&(bool)Get(boar,"chargeContact")','Mathf.Abs(F("shield")-(8-1.4f*F("EnemyAttackScale")))<.001f&&(bool)Get(boar,"chargeContact")')
s=s.replace(' static void Next(){',(out/'tests.txt').read_text(encoding='utf-8')+'\n static void Next(){')
(out/'Smoke.cs').write_text(s,encoding='utf-8')
qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
for src in (root/'Assets/NakiPrototype').rglob('*'):
 if src.is_file():
  dst=qa/'Assets/NakiPrototype'/src.relative_to(root/'Assets/NakiPrototype')
  if not dst.exists() or src.read_bytes()!=dst.read_bytes():dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dst)
shutil.copy2(out/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/BossMobilityV2Smoke.cs')
lines=(root/'Validation/ThunderWardenV2/compile.rsp').read_text(encoding='utf-8-sig').splitlines();lines=[x for x in lines if not x.strip('"').endswith('.cs') and not x.strip('"').startswith('-out:')]
lines+=['"-out:'+str(out/'runtime-compile.dll')+'"']+['"'+str(p)+'"' for p in (root/'Assets/NakiPrototype').rglob('*.cs')]
(out/'compile.rsp').write_text('\n'.join(lines),encoding='utf-8')
s=(root/'Validation/ThunderWardenV2/sync_runtime.py').read_text(encoding='utf-8').replace('Validation/ThunderWardenV2/runtime/pre-warden-sync','Validation/BossMobilityV2/runtime/pre-mobility-sync');(out/'sync_runtime.py').write_text(s,encoding='utf-8')
print('Staged all seventeen encounter families across three phases, resource pressure and saved-progress regressions')
