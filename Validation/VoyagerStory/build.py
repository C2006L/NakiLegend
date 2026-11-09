from pathlib import Path
import shutil
root=Path(__file__).resolve().parents[2];out=root/'Validation/VoyagerStory';out.mkdir(exist_ok=True)
s=(root/'Validation/QuayMotion/Smoke.cs').read_text(encoding='utf-8').replace('NakiQuayMotionSmoke','NakiVoyagerStorySmoke').replace('Naki.QuayMotion','Naki.VoyagerStory').replace('Validation/QuayMotion/runtime/','Validation/VoyagerStory/runtime/')
s=s.replace('QuayMotionTests();p.fireChapterMode=','QuayMotionTests();VoyagerStoryTests();p.fireChapterMode=')
s=s.replace('p.enabled=false;QuayMotionTests();p.enabled=true;stage=149','p.enabled=false;VoyagerStoryTests();p.enabled=true;stage=153')
s=s.replace('Set("originAbilityMenu",false);','Set("originAbilityMenu",false);Set("voyagerJournal",false);')
s=s.replace(' static void Next(){',(out/'tests.txt').read_text(encoding='utf-8')+'\n static void Next(){')
s=s.replace('   Check(errors.Count==0,"Actual HUD','   if(stage==153){p.enabled=false;VoyagerJournalPose();p.enabled=true;Next();return;}\n   if(stage==154){ScreenCapture.CaptureScreenshot(Out+"voyager-journal.png");Next();return;}\n   Check(errors.Count==0,"Actual HUD')
(out/'Smoke.cs').write_text(s,encoding='utf-8')
qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
for src in (root/'Assets/NakiPrototype').rglob('*'):
 if src.is_file():
  dst=qa/'Assets/NakiPrototype'/src.relative_to(root/'Assets/NakiPrototype')
  if not dst.exists() or src.read_bytes()!=dst.read_bytes():dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dst)
shutil.copy2(out/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/VoyagerStorySmoke.cs')
lines=(root/'Validation/QuayMotion/compile.rsp').read_text(encoding='utf-8-sig').splitlines()
lines=[s for s in lines if not s.strip('"').endswith('.cs') and not s.strip('"').startswith('-out:')]
lines+=['"-out:'+str(out/'runtime-compile.dll')+'"']+['"'+str(p)+'"' for p in (root/'Assets/NakiPrototype').rglob('*.cs')]
(out/'compile.rsp').write_text('\n'.join(lines),encoding='utf-8')
print('Staged five-island evidence and journal verification')
