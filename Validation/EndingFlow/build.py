from pathlib import Path
import shutil

root=Path(__file__).resolve().parents[2]
folder=root/'Validation/EndingFlow'
s=(root/'Validation/ArsenalV1/Smoke.cs').read_text(encoding='utf-8')
s=s.replace('NakiArsenalSmoke','NakiEndingFlowSmoke').replace('Naki.Arsenal','Naki.EndingFlow').replace('Validation/ArsenalV1/runtime/','Validation/EndingFlow/runtime/')
s=s.replace('ArsenalTests();p.fireChapterMode=', 'ArsenalTests();EndingFlowTests();p.fireChapterMode=')
s=s.replace(' static void Next(){',(folder/'tests.txt').read_text(encoding='utf-8')+'\n static void Next(){')
s=s.replace('   Check(errors.Count==0,"Actual HUD', '''   if(stage==95){p.enabled=false;IceEndingPose();p.enabled=true;Next();return;}
   if(stage==96){ScreenCapture.CaptureScreenshot(Out+"ice-ending-cg.png");Next();return;}
   if(stage==97){p.enabled=false;Call("FinishChapterEnding");Quiet();Call("HandleTalents");Call("LearnTalent",0);ReleaseInteraction();Call("RequestIceVoyage",304);p.enabled=true;Next();return;}
   if(stage==98){ScreenCapture.CaptureScreenshot(Out+"ice-voyage-confirm.png");Next();return;}
   if(stage==99){p.enabled=false;Call("CancelIceVoyage");Call("BuildChapterActions");p.enabled=true;Next();return;}
   if(stage==100){ScreenCapture.CaptureScreenshot(Out+"ice-after-talent.png");Next();return;}
   Check(errors.Count==0,"Actual HUD''')
s=s.replace('checks: new balance, one-time island talents and legacy checkpoint compatibility, larger elemental capacity, new weapons/skills, separate hammer audio, encounter density, cover navigation and actual UI rendering.', 'checks: arsenal regression plus Ice boss, sluice, CG and talent order; legacy completed saves; explicit cancellable voyages; persisted CG/talents; real UI rendering.')
(folder/'Smoke.cs').write_text(s,encoding='utf-8')
qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
for source in (root/'Assets/NakiPrototype').rglob('*'):
    if not source.is_file():continue
    target=qa/'Assets/NakiPrototype'/source.relative_to(root/'Assets/NakiPrototype')
    if not target.exists() or target.read_bytes()!=source.read_bytes():
        target.parent.mkdir(parents=True,exist_ok=True)
        shutil.copy2(source,target)
shutil.copy2(folder/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/EndingFlowSmoke.cs')
print('Prepared isolated Ice ending-flow regression; player save paths are never used')
