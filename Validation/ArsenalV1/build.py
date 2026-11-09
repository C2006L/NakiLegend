from pathlib import Path
import subprocess,shutil,re,sys
root=Path(__file__).resolve().parents[2]
if '--reuse-runtime' not in sys.argv:subprocess.run(['python',str(root/'Validation/IceBagPolish/build.py')],check=True)
s=(root/'Validation/IceBagPolish/Smoke.cs').read_text(encoding='utf-8').replace('NakiIceBagPolishSmoke','NakiArsenalSmoke').replace('Naki.IceBagPolish','Naki.Arsenal').replace('Validation/IceBagPolish/runtime/','Validation/ArsenalV1/runtime/')
# Old fixed numeric combat assertions are historical after the balance revision.
start=s.index('   if(stage==0){p.enabled=false;');end=s.index('p.fireChapterMode=false;p.iceSampleMode=false;p.woodlandMode=false;CombatPose();',start)
s=s[:start]+'   if(stage==0){p.enabled=false;NarrativeAndPower();EndingGate();ArsenalTests();'+s[end:]
s=s.replace(' static void Next(){',(root/'Validation/ArsenalV1/tests.txt').read_text(encoding='utf-8')+'\n static void Next(){')
# Render only relevant new UI, rather than invoke obsolete fixed-count fixtures.
s=s.replace('p.fireChapterMode=false;p.iceSampleMode=false;p.woodlandMode=false;CombatPose();p.enabled=true;Next();return;','p.fireChapterMode=false;p.iceSampleMode=false;p.woodlandMode=false;TalentPose();p.enabled=true;stage=92;ticks=0;return;')
s=s.replace('   Check(errors.Count==0,','''   if(stage==92){ScreenCapture.CaptureScreenshot(Out+"talent-choice.png");Next();return;}
   if(stage==93){p.enabled=false;SkillsPose();p.enabled=true;Next();return;}
   if(stage==94){ScreenCapture.CaptureScreenshot(Out+"journey-skills.png");Next();return;}
   Check(errors.Count==0,''')
s=s.replace('File.WriteAllText(Out+(SessionState.GetBool("Naki.Arsenal.Preview",false)?"preview-result.txt":"smoke-result.txt"),"PASS "+checks+', 'File.WriteAllText(Out+(SessionState.GetBool("Naki.Arsenal.Preview",false)?"preview-result.txt":"smoke-result.txt"),"PASS "+checks+')
a=s.index(' checks: full-height evacuation');b=s.index('\\n");End(0);',a);s=s[:a]+' checks: new balance, one-time island talents and legacy checkpoint compatibility, larger elemental capacity, new weapons/skills, separate hammer audio, encounter density, cover navigation and actual UI rendering.'+s[b:]
(root/'Validation/ArsenalV1/Smoke.cs').write_text(s,encoding='utf-8')
qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
for source in (root/'Assets/NakiPrototype').rglob('*'):
 if not source.is_file():continue
 target=qa/'Assets/NakiPrototype'/source.relative_to(root/'Assets/NakiPrototype')
 if not target.exists() or target.read_bytes()!=source.read_bytes():target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(source,target)
shutil.copy2(root/'Validation/ArsenalV1/Smoke.cs',qa/'Assets/NakiPrototype/Editor/ArsenalSmoke.cs')
print('Prepared Arsenal native regression with updated balance specification')
