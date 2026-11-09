from pathlib import Path
import shutil
root=Path(__file__).resolve().parents[2];out=root/'Validation/MingshaPolishV2'
s=(root/'Validation/OriginCombatV2/Smoke.cs').read_text(encoding='utf-8').replace('NakiOriginCombatV2Smoke','NakiMingshaPolishV2Smoke').replace('Naki.OriginCombatV2','Naki.MingshaPolishV2').replace('Validation/OriginCombatV2/runtime/','Validation/MingshaPolishV2/runtime/')
s=s.replace('OriginCombatV2Tests();p.', 'OriginCombatV2Tests();MingshaPolishTests();p.')
s=s.replace(' static void Next(){',(out/'tests.txt').read_text(encoding='utf-8')+'\n static void Next(){')
s=s.replace('   Check(errors.Count==0,"Actual HUD','''   if(stage==171){p.enabled=false;RainQuayPose();Set("position",new Vector2(-.18f,-2.7f));Call("UpdatePixelHero",Vector2.zero);p.enabled=true;Next();return;}
   if(stage==172){ScreenCapture.CaptureScreenshot(Out+"rain-quay-banks.png");Next();return;}
   if(stage==173){p.enabled=false;Enter(42);KillCurrent();Quiet();p.enabled=true;Next();return;}
   if(stage==174){ScreenCapture.CaptureScreenshot(Out+"inspection-court.png");Next();return;}
   Check(errors.Count==0,"Actual HUD''')
(out/'Smoke.cs').write_text(s,encoding='utf-8')
qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
for src in (root/'Assets/NakiPrototype').rglob('*'):
 if src.is_file():
  dst=qa/'Assets/NakiPrototype'/src.relative_to(root/'Assets/NakiPrototype')
  if not dst.exists() or src.read_bytes()!=dst.read_bytes():dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dst)
shutil.copy2(out/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/MingshaPolishV2Smoke.cs')
lines=(root/'Validation/OriginCombatV2/compile.rsp').read_text(encoding='utf-8-sig').splitlines()
lines=[x for x in lines if not x.strip('"').endswith('.cs') and not x.strip('"').startswith('-out:')]
lines+=['"-out:'+str(out/'runtime-compile.dll')+'"']+['"'+str(p)+'"' for p in (root/'Assets/NakiPrototype').rglob('*.cs')]
(out/'compile.rsp').write_text('\n'.join(lines),encoding='utf-8')
print('Staged bank-only canal routes, old-save migration, tuning and actual warrior audio tests')
