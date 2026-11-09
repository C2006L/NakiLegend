from pathlib import Path
import shutil,uuid
root=Path(__file__).resolve().parents[2];out=root/'Validation/WeaponRecoveryV5';qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
for p in list((root/'Assets/NakiPrototype').rglob('*.cs')):
 meta=Path(str(p)+'.meta')
 if not meta.exists():meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\n',encoding='utf-8')
for src in (root/'Assets/NakiPrototype').rglob('*'):
 if src.is_file():
  dst=qa/'Assets/NakiPrototype'/src.relative_to(root/'Assets/NakiPrototype')
  if not dst.exists() or src.read_bytes()!=dst.read_bytes():dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dst)
lines=(root/'Validation/ArmedEnemiesV2/compile.rsp').read_text(encoding='utf-8-sig').splitlines()
lines=[x for x in lines if not x.strip('"').endswith('.cs') and not x.strip('"').startswith('-out:')]
lines+=['"-out:'+str(out/'runtime-compile.dll')+'"']+['"'+str(p)+'"' for p in (root/'Assets/NakiPrototype').rglob('*.cs')]
(out/'compile.rsp').write_text('\n'.join(lines),encoding='utf-8')
if (out/'Smoke.cs').exists():shutil.copy2(out/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/WeaponRecoveryV5Smoke.cs')
(out/'runtime').mkdir(exist_ok=True)
print('Staged forest bounds/balance source and native test runner in isolated Unity project')
