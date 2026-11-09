from pathlib import Path
import shutil

root=Path(__file__).resolve().parents[2]
folder=root/'Validation/LegacyV1'
s=(root/'Validation/IslandSupplies/Smoke.cs').read_text(encoding='utf-8').replace('NakiIslandSuppliesSmoke','NakiLegacyV1Smoke').replace('Naki.IslandSupplies','Naki.LegacyV1').replace('Validation/IslandSupplies/runtime/','Validation/LegacyV1/runtime/')
s=s.replace('Length==42','Length==123').replace('IslandSupplyTests();p.fireChapterMode=','IslandSupplyTests();LegacyV1Tests();p.fireChapterMode=')
s=s.replace('Set("storyLines",null);Set("paused",false);','Set("storyLines",null);Set("queuedOriginChapter",0);Set("originAbilityMenu",false);Set("paused",false);')
s=s.replace('if(mode<3)Check(mode==1&&id==27||Get(weapon,"element").ToString()==(mode==0?"Fire":mode==1?"Wood":"Ice"),','if(mode<3)Check(!(bool)Get(weapon,"signature"),')
s=s.replace('Get(((Array)Get("weaponCatalog")).GetValue(offered[0]),"type").ToString()=="Staff"&&Get(((Array)Get("weaponCatalog")).GetValue(offered[1]),"type").ToString()=="Bow"','Get(((Array)Get("weaponCatalog")).GetValue(offered[0]),"type").ToString()!=Get(((Array)Get("weaponCatalog")).GetValue(offered[1]),"type").ToString()')
s=s.replace('existing Arsenal/Ice/Fire regressions plus three Forest environments, navigable natural cover, denser staged enemies, independent boss AI and knots, telegraphed burnable spore zones, bounded poison, appended weapons and old-save migration, rescue/clue/planting/CG/talent order, explicit safe return voyage, native UI rendering.', 'five-island regressions, eight resident sprites and protected evacuation, seventeen once-only boss reward families, 123 weapon definitions, persisted random offers, unbounded dark saves and voyage continuity, primary abilities and five-island route memories, real signature combat effects and native UI rendering.')
s=s.replace(' static void Next(){',(folder/'tests.txt').read_text(encoding='utf-8')+'\n static void Next(){')
s=s.replace('   Check(errors.Count==0,"Actual HUD', '   if(stage==129){p.enabled=false;RefugePose(false);p.enabled=true;Next();return;}\n   if(stage==130){ScreenCapture.CaptureScreenshot(Out+"refuge-during-combat.png");Next();return;}\n   if(stage==131){p.enabled=false;RefugePose(true);p.enabled=true;Next();return;}\n   if(stage==132){ScreenCapture.CaptureScreenshot(Out+"refuge-safe.png");Next();return;}\n   if(stage==133){p.enabled=false;LegacyPowerPose();p.enabled=true;Next();return;}\n   if(stage==134){ScreenCapture.CaptureScreenshot(Out+"unbounded-dark-power.png");Next();return;}\n   if(stage==135){p.enabled=false;Set("originAbilityMenu",true);p.enabled=true;Next();return;}\n   if(stage==136){ScreenCapture.CaptureScreenshot(Out+"origin-ability-menu.png");Next();return;}\n   Check(errors.Count==0,"Actual HUD')
(folder/'Smoke.cs').write_text(s,encoding='utf-8')
qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
for source in (root/'Assets/NakiPrototype').rglob('*'):
 if not source.is_file():continue
 target=qa/'Assets/NakiPrototype'/source.relative_to(root/'Assets/NakiPrototype')
 if not target.exists() or target.read_bytes()!=source.read_bytes():
  target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(source,target)
shutil.copy2(folder/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/LegacyV1Smoke.cs')
print('Prepared isolated NPC, relics, random loot and origin-legacy regression')
