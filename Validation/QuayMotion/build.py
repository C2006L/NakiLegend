from pathlib import Path
import shutil
root=Path(__file__).resolve().parents[2];out=root/'Validation/QuayMotion';out.mkdir(exist_ok=True)
s=(root/'Validation/WorldAtlas/Smoke.cs').read_text(encoding='utf-8').replace('NakiWorldAtlasSmoke','NakiQuayMotionSmoke').replace('Naki.WorldAtlas','Naki.QuayMotion').replace('Validation/WorldAtlas/runtime/','Validation/QuayMotion/runtime/')
s=s.replace('WorldAtlasTests();p.fireChapterMode=','WorldAtlasTests();QuayMotionTests();p.fireChapterMode=')
s=s.replace('p.enabled=false;WorldAtlasTests();p.enabled=true;stage=145','p.enabled=false;QuayMotionTests();p.enabled=true;stage=149')
s=s.replace(' static void Next(){',(out/'tests.txt').read_text(encoding='utf-8')+'\n static void Next(){')
s=s.replace('   Check(errors.Count==0,"Actual HUD','   if(stage==149){p.enabled=false;QuayHarborPose();p.enabled=true;Next();return;}\n   if(stage==150){ScreenCapture.CaptureScreenshot(Out+"ice-quay-shore.png");Next();return;}\n   if(stage==151){p.enabled=false;RainQuayPose();p.enabled=true;Next();return;}\n   if(stage==152){ScreenCapture.CaptureScreenshot(Out+"desert-rain-quay.png");Next();return;}\n   Check(errors.Count==0,"Actual HUD')
(out/'Smoke.cs').write_text(s,encoding='utf-8')
qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
for src in (root/'Assets/NakiPrototype').rglob('*'):
 if src.is_file():
  dst=qa/'Assets/NakiPrototype'/src.relative_to(root/'Assets/NakiPrototype')
  if not dst.exists() or src.read_bytes()!=dst.read_bytes():dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dst)
shutil.copy2(out/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/QuayMotionSmoke.cs')
lines=(root/'Validation/WorldAtlas/compile.rsp').read_text(encoding='utf-8-sig').splitlines()
lines=[s for s in lines if not s.strip('"').endswith('.cs') and not s.strip('"').startswith('-out:')]
lines+=['"-out:'+str(out/'runtime-compile.dll')+'"']+['"'+str(p)+'"' for p in (root/'Assets/NakiPrototype').rglob('*.cs')]
(out/'compile.rsp').write_text('\n'.join(lines),encoding='utf-8')
print('Staged shoreline, rain quay and enemy motion regressions')
