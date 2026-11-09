from pathlib import Path
import shutil
root=Path(__file__).resolve().parents[2];out=root/'Validation/WeaponIdentityV3';qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
s=(root/'Validation/BossMobilityV2/Smoke.cs').read_text(encoding='utf-8').replace('NakiBossMobilityV2Smoke','NakiWeaponIdentityV3Smoke').replace('Naki.BossMobilityV2','Naki.WeaponIdentityV3').replace('Validation/BossMobilityV2/runtime/','Validation/WeaponIdentityV3/runtime/')
s=s.replace('.Length==123','.Length==132').replace('s.texture.name=="ranged-atlas"','s.texture.name.StartsWith("sheet-")').replace(')).texture.name=="ranged-atlas"',')).texture.name.StartsWith("sheet-")').replace('Contains("staff_electric")','Contains("weapon_009_cast")')
s=s.replace('Check(Get(List("shots")[0],"skillAudio").ToString()=="spore","Old and new Wood staves both apply the short poison effect described on their cards");','object poisoned=Spawn("StormGuard",new Vector2(2,0));Set(poisoned,"hp",1000f);Call("ResolveProjectileEnemyHit",List("shots")[0],poisoned,new Vector2(2,0));Check((float)Get(poisoned,"woodPoisonUntil")>F("clock"),"Wood staff poison persists independently of its new dedicated weapon sound");')
s=s.replace('BossMobilityV2Tests();p.','BossMobilityV2Tests();WeaponIdentityTests();p.')
s=s.replace('123 weapon definitions,','132 weapon identities, 264 unique weapon voices, nine route weapons and their save-compatible resource pools,')
s=s.replace(' static void Next(){',(out/'tests.txt').read_text(encoding='utf-8')+'\n static void Next(){')
s=s.replace('if(stage==174){ScreenCapture.CaptureScreenshot(Out+"inspection-court.png");Next();return;}','if(stage==174){ScreenCapture.CaptureScreenshot(Out+"inspection-court.png");Next();return;}\n   if(stage==175){IdentityPose(48);Next();return;}\n   if(stage==176){ScreenCapture.CaptureScreenshot(Out+"needle-staff.png");Next();return;}\n   if(stage==177){IdentityPose(93);Next();return;}\n   if(stage==178){ScreenCapture.CaptureScreenshot(Out+"lantern-staff.png");Next();return;}\n   if(stage==179){IdentityPose(123);Next();return;}\n   if(stage==180){ScreenCapture.CaptureScreenshot(Out+"light-staff.png");Next();return;}\n   if(stage==181){IdentityPose(129);Next();return;}\n   if(stage==182){ScreenCapture.CaptureScreenshot(Out+"sage-lantern.png");Next();return;}')
(out/'Smoke.cs').write_text(s,encoding='utf-8')
for src in (root/'Assets/NakiPrototype').rglob('*'):
 if src.is_file():
  dst=qa/'Assets/NakiPrototype'/src.relative_to(root/'Assets/NakiPrototype')
  if not dst.exists() or src.read_bytes()!=dst.read_bytes():dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dst)
shutil.copy2(out/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/WeaponIdentityV3Smoke.cs')
lines=(root/'Validation/BossMobilityV2/compile.rsp').read_text(encoding='utf-8-sig').splitlines();lines=[x for x in lines if not x.strip('"').endswith('.cs') and not x.strip('"').startswith('-out:')]
lines+=['"-out:'+str(out/'runtime-compile.dll')+'"']+['"'+str(p)+'"' for p in (root/'Assets/NakiPrototype').rglob('*.cs')]
(out/'compile.rsp').write_text('\n'.join(lines),encoding='utf-8')
s=(root/'Validation/BossMobilityV2/sync_runtime.py').read_text(encoding='utf-8').replace('Validation/BossMobilityV2/runtime/pre-mobility-sync','Validation/WeaponIdentityV3/runtime/pre-identity-sync');(out/'sync_runtime.py').write_text(s,encoding='utf-8')
print('Staged full prior boss/route/save audit plus weapon identities, audio and origin-weapon tests')
