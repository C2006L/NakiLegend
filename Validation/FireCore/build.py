from pathlib import Path
import shutil

root=Path(__file__).resolve().parents[2]
folder=root/'Validation/FireCore'
s=(root/'Validation/EndingFlow/Smoke.cs').read_text(encoding='utf-8').replace('NakiEndingFlowSmoke','NakiFireCoreSmoke').replace('Naki.EndingFlow','Naki.FireCore').replace('Validation/EndingFlow/runtime/','Validation/FireCore/runtime/')
s=s.replace('EndingFlowTests();p.fireChapterMode=', 'EndingFlowTests();FireCoreTests();BossEnergyTests();p.fireChapterMode=')
s=s.replace(' static void Next(){',(folder/'tests.txt').read_text(encoding='utf-8')+'\n static void Next(){')
s=s.replace('   Check(errors.Count==0,"Actual HUD', '''   if(stage==101){p.enabled=false;CorePose(31);p.enabled=true;Next();return;}
   if(stage==102){ScreenCapture.CaptureScreenshot(Out+"core-encounter.png");Next();return;}
   if(stage==103){p.enabled=false;CorePose(42);object boss=List("enemies")[0];Set(boss,"hp",(float)Get(boss,"maximum")*.3f);Set(boss,"secondPhase",true);Set(boss,"attackNumber",3);Set(boss,"attackDirection",Vector2.down);Set(boss,"nextAttack",99999f);Set(boss,"recoveryUntil",99999f);Call("ResolveCoreMove",boss);Set("position",new Vector2(0,-3));p.enabled=true;Next();return;}
   if(stage==104){ScreenCapture.CaptureScreenshot(Out+"magma-tortoise.png");Next();return;}
   if(stage==105){p.enabled=false;FinishCoreStory();Call("CoreAction",506);p.enabled=true;Next();return;}
   if(stage==106){ScreenCapture.CaptureScreenshot(Out+"fire-hearth-cg.png");Next();return;}
   if(stage==107){p.enabled=false;Call("FinishChapterEnding");Quiet();Call("HandleTalents");p.enabled=true;Next();return;}
   if(stage==108){ScreenCapture.CaptureScreenshot(Out+"fire-talent.png");Next();return;}
   Check(errors.Count==0,"Actual HUD''')
s=s.replace('checks: arsenal regression plus Ice boss, sluice, CG and talent order; legacy completed saves; explicit cancellable voyages; persisted CG/talents; real UI rendering.', 'checks: Arsenal/Ice ending regressions, 45-room Fire Core progression and migration, denser waves, navigable cover, third boss telegraphs and burst phases, one-time 40-energy boss drops, evacuation/hearth/CG/talent order, Fire-to-Forest inheritance, native rendering.')
(folder/'Smoke.cs').write_text(s,encoding='utf-8')
qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
for source in (root/'Assets/NakiPrototype').rglob('*'):
    if not source.is_file():continue
    target=qa/'Assets/NakiPrototype'/source.relative_to(root/'Assets/NakiPrototype')
    if not target.exists() or target.read_bytes()!=source.read_bytes():
        target.parent.mkdir(parents=True,exist_ok=True)
        shutil.copy2(source,target)
shutil.copy2(folder/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/FireCoreSmoke.cs')
print('Prepared isolated third-floor, migration, ending and boss-energy native tests')
