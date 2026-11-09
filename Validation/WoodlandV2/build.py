from pathlib import Path
import shutil

root=Path(__file__).resolve().parents[2]
folder=root/'Validation/WoodlandV2'
s=(root/'Validation/FireCore/Smoke.cs').read_text(encoding='utf-8').replace('NakiFireCoreSmoke','NakiWoodlandV2Smoke').replace('Naki.FireCore','Naki.WoodlandV2').replace('Validation/FireCore/runtime/','Validation/WoodlandV2/runtime/')
s=s.replace('BossEnergyTests();p.fireChapterMode=', 'BossEnergyTests();WoodlandV2Tests();p.fireChapterMode=')
s=s.replace('((Array)Get("weaponCatalog")).Length==27','((Array)Get("weaponCatalog")).Length==33').replace('Four distinct new weapon definitions append without renumbering old weapons','Forest weapons append after the existing twenty-seven without renumbering')
s=s.replace('pass<9&&','pass<32&&')
s=s.replace(' static void Next(){',(folder/'tests.txt').read_text(encoding='utf-8')+'\n static void Next(){')
s=s.replace('   Check(errors.Count==0,"Actual HUD', '''   if(stage==109){p.enabled=false;ForestPose(1);p.enabled=true;Next();return;}
   if(stage==110){ScreenCapture.CaptureScreenshot(Out+"woodland-edge.png");Next();return;}
   if(stage==111){p.enabled=false;ForestPose(19);Call("QueueWoodZone",new Vector2(3,0),1f);p.enabled=true;Next();return;}
   if(stage==112){ScreenCapture.CaptureScreenshot(Out+"woodland-fungal.png");Next();return;}
   if(stage==113){p.enabled=false;ForestPose(31);object tree=List("enemies")[0];Set(tree,"attackDirection",Vector2.down);Set(tree,"attackNumber",3);Set(tree,"secondPhase",true);Set(tree,"nextAttack",99999f);Set(tree,"recoveryUntil",99999f);Call("ResolveWoodMove",tree);p.enabled=true;Next();return;}
   if(stage==114){ScreenCapture.CaptureScreenshot(Out+"ancient-guardian.png");Next();return;}
   if(stage==115){p.enabled=false;ForestEndingPose();p.enabled=true;Next();return;}
   if(stage==116){ScreenCapture.CaptureScreenshot(Out+"woodland-ending-cg.png");Next();return;}
   if(stage==117){p.enabled=false;Call("FinishChapterEnding");Quiet();Call("HandleTalents");p.enabled=true;Next();return;}
   if(stage==118){ScreenCapture.CaptureScreenshot(Out+"woodland-talent.png");Next();return;}
   if(stage==119){p.enabled=false;ForestPose(6);p.enabled=true;Next();return;}
   if(stage==120){ScreenCapture.CaptureScreenshot(Out+"thorn-mother.png");Next();return;}
   if(stage==121){p.enabled=false;ForestPose(0);p.enabled=true;Next();return;}
   if(stage==122){ScreenCapture.CaptureScreenshot(Out+"forest-harbor.png");Next();return;}
   Check(errors.Count==0,"Actual HUD''')
s=s.replace('checks: Arsenal/Ice ending regressions, 45-room Fire Core progression and migration, denser waves, navigable cover, third boss telegraphs and burst phases, one-time 40-energy boss drops, evacuation/hearth/CG/talent order, Fire-to-Forest inheritance, native rendering.', 'checks: existing Arsenal/Ice/Fire regressions plus three Forest environments, navigable natural cover, denser staged enemies, independent boss AI and knots, telegraphed burnable spore zones, bounded poison, appended weapons and old-save migration, rescue/clue/planting/CG/talent order, explicit safe return voyage, native UI rendering.')
(folder/'Smoke.cs').write_text(s,encoding='utf-8')
qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
for source in (root/'Assets/NakiPrototype').rglob('*'):
 if not source.is_file():continue
 target=qa/'Assets/NakiPrototype'/source.relative_to(root/'Assets/NakiPrototype')
 if not target.exists() or target.read_bytes()!=source.read_bytes():
  target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(source,target)
shutil.copy2(folder/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/WoodlandV2Smoke.cs')
print('Prepared isolated Forest V2 regression and presentation scenes')
