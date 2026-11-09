from pathlib import Path
import shutil
root=Path(__file__).resolve().parents[2];out=root/'Validation/RouteClarityV3';qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
s=(root/'Validation/WeaponIdentityV3/Smoke.cs').read_text(encoding='utf-8').replace('NakiWeaponIdentityV3Smoke','NakiRouteClarityV3Smoke').replace('Naki.WeaponIdentityV3','Naki.RouteClarityV3').replace('Validation/WeaponIdentityV3/runtime/','Validation/RouteClarityV3/runtime/')
s=s.replace('WeaponIdentityTests();p.','WeaponIdentityTests();FullIceTests();RouteClarityTests();p.')
s=s.replace(' static void Next(){',(out/'tests.txt').read_text(encoding='utf-8')+'\n static void Next(){')
s=s.replace('   Check(errors.Count==0,','   if(stage>=183&&stage<=192){int mode=(stage-183)/2;if(stage%2==1)RouteClarityPose(mode);else ScreenCapture.CaptureScreenshot(Out+new[]{"ice-route-map.png","ice-observation.png","fire-refuge-combat.png","fire-refuge-rescued.png","fire-stair-label.png"}[mode]);Next();return;}\n   Check(errors.Count==0,')
(out/'Smoke.cs').write_text(s,encoding='utf-8')
for src in (root/'Assets/NakiPrototype').rglob('*'):
 if src.is_file():
  dst=qa/'Assets/NakiPrototype'/src.relative_to(root/'Assets/NakiPrototype')
  if not dst.exists() or src.read_bytes()!=dst.read_bytes():dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dst)
shutil.copy2(out/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/RouteClarityV3Smoke.cs')
lines=(root/'Validation/WeaponIdentityV3/compile.rsp').read_text(encoding='utf-8-sig').splitlines();lines=[x for x in lines if not x.strip('"').endswith('.cs') and not x.strip('"').startswith('-out:')]
lines+=['"-out:'+str(out/'runtime-compile.dll')+'"']+['"'+str(p)+'"' for p in (root/'Assets/NakiPrototype').rglob('*.cs')]
(out/'compile.rsp').write_text('\n'.join(lines),encoding='utf-8')
s=(root/'Validation/WeaponIdentityV3/sync_runtime.py').read_text(encoding='utf-8').replace('Validation/WeaponIdentityV3/runtime/pre-identity-sync','Validation/RouteClarityV3/runtime/pre-route-sync')
s=s.replace("if f.startswith('Assets/NakiPrototype/')", "if f.startswith('Assets/NakiPrototype/') and (f.endswith('.cs') or f.endswith('.cs.meta'))")
(out/'sync_runtime.py').write_text(s,encoding='utf-8')
print('Staged ice route, all 42-room playthrough/ending, old-save migration, soul seal and Fire rescue checks')
