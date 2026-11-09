from pathlib import Path
import shutil,json
root=Path(__file__).resolve().parents[2];out=root/'Validation/PassageV2';out.mkdir(exist_ok=True)
s=(root/'Validation/VoyagerStory/Smoke.cs').read_text(encoding='utf-8').replace('NakiVoyagerStorySmoke','NakiPassageV2Smoke').replace('Naki.VoyagerStory','Naki.PassageV2').replace('Validation/VoyagerStory/runtime/','Validation/PassageV2/runtime/')
s=s.replace('VoyagerStoryTests();p.fireChapterMode=','VoyagerStoryTests();PassageV2Tests();p.fireChapterMode=')
s=s.replace('p.enabled=false;VoyagerStoryTests();p.enabled=true;stage=153','p.enabled=false;PassageV2Tests();p.enabled=true;stage=155')
s=s.replace('QuayPoint(.95f,.47f)','QuayPoint(.95f,.58f)')
s=s.replace('using System;','using System;\nusing System.Linq;')
s=s.replace(' static void Next(){',(out/'tests.txt').read_text(encoding='utf-8')+'\n static void Next(){')
s=s.replace('   Check(errors.Count==0,"Actual HUD','''   if(stage==155){p.enabled=false;PassageRainPose();p.enabled=true;Next();return;}
   if(stage==156){ScreenCapture.CaptureScreenshot(Out+"rain-exit.png");Next();return;}
   if(stage==157){p.enabled=false;PassageStormPose();p.enabled=true;Next();return;}
   if(stage==158){ScreenCapture.CaptureScreenshot(Out+"storm-gates.png");Next();return;}
   if(stage==159){p.enabled=false;Chapter();Enter(9);KillCurrent();Quiet();Call("UpdatePixelHero",Vector2.zero);p.enabled=true;Next();return;}
   if(stage==160){ScreenCapture.CaptureScreenshot(Out+"storm-waymarkers.png");Next();return;}
   Check(errors.Count==0,"Actual HUD''')
(out/'Smoke.cs').write_text(s,encoding='utf-8')
qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
for src in (root/'Assets/NakiPrototype').rglob('*'):
 if src.is_file():
  dst=qa/'Assets/NakiPrototype'/src.relative_to(root/'Assets/NakiPrototype')
  if not dst.exists() or src.read_bytes()!=dst.read_bytes():dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dst)
shutil.copy2(out/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/PassageV2Smoke.cs')
lines=(root/'Validation/VoyagerStory/compile.rsp').read_text(encoding='utf-8-sig').splitlines()
lines=[line for line in lines if not line.strip('"').endswith('.cs') and not line.strip('"').startswith('-out:')]
lines+=['"-out:'+str(out/'runtime-compile.dll')+'"']+['"'+str(p)+'"' for p in (root/'Assets/NakiPrototype').rglob('*.cs')]
(out/'compile.rsp').write_text('\n'.join(lines),encoding='utf-8')
print('Staged passage, session and cooldown regressions')
