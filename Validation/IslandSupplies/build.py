from pathlib import Path
import shutil

root=Path(__file__).resolve().parents[2]
folder=root/'Validation/IslandSupplies'
s=(root/'Validation/WoodlandV2/Smoke.cs').read_text(encoding='utf-8').replace('NakiWoodlandV2Smoke','NakiIslandSuppliesSmoke').replace('Naki.WoodlandV2','Naki.IslandSupplies').replace('Validation/WoodlandV2/runtime/','Validation/IslandSupplies/runtime/')
s=s.replace('WoodlandV2Tests();p.fireChapterMode=', 'WoodlandV2Tests();IslandSupplyTests();p.fireChapterMode=')
s=s.replace('Length==33','Length==42')
s=s.replace('Call("CoreAction",505);Quiet();','Call("CoreAction",505);Quiet();Call("CoreAction",508);Quiet();')
s=s.replace(' static void Next(){',(folder/'tests.txt').read_text(encoding='utf-8')+'\n static void Next(){')
s=s.replace('   Check(errors.Count==0,"Actual HUD', '   if(stage==123){p.enabled=false;SupplyPose(false);p.enabled=true;Next();return;}\n   if(stage==124){ScreenCapture.CaptureScreenshot(Out+"fire-supply.png");Next();return;}\n   if(stage==125){p.enabled=false;SupplyPose(true);p.enabled=true;Next();return;}\n   if(stage==126){ScreenCapture.CaptureScreenshot(Out+"forest-supply.png");Next();return;}\n   if(stage==127){p.enabled=false;PackbeastPose();p.enabled=true;Next();return;}\n   if(stage==128){ScreenCapture.CaptureScreenshot(Out+"packbeast-telegraph.png");Next();return;}\n   Check(errors.Count==0,"Actual HUD')
(folder/'Smoke.cs').write_text(s,encoding='utf-8')
qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
for source in (root/'Assets/NakiPrototype').rglob('*'):
 if not source.is_file():continue
 target=qa/'Assets/NakiPrototype'/source.relative_to(root/'Assets/NakiPrototype')
 if not target.exists() or target.read_bytes()!=source.read_bytes():
  target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(source,target)
shutil.copy2(folder/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/IslandSuppliesSmoke.cs')
print('Prepared isolated themed supplies, Fire aftermath and packbeast regression')
