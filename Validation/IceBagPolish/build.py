from pathlib import Path
import subprocess,shutil,sys
root=Path(__file__).resolve().parents[2]
if '--reuse-runtime' not in sys.argv:subprocess.run(['python',str(root/'Validation/BridgeScene/build.py')],check=True)
s=(root/'Validation/BridgeScene/Smoke.cs').read_text(encoding='utf-8').replace('NakiBridgeSceneSmoke','NakiIceBagPolishSmoke').replace('Naki.BridgeScene','Naki.IceBagPolish').replace('Validation/BridgeScene/runtime/','Validation/IceBagPolish/runtime/')
s=s.replace('BridgeSceneTests();','BridgeSceneTests();IceBagPolishTests();')
s=s.replace(' static void Next(){',(root/'Validation/IceBagPolish/tests.txt').read_text(encoding='utf-8')+'\n static void Next(){')
s=s.replace('   Check(errors.Count==0,','''   if(stage==86){p.enabled=false;IceBagPose(0);p.enabled=true;Next();return;}
   if(stage==87){ScreenCapture.CaptureScreenshot(Out+"ice-rescue-open.png");Next();return;}
   if(stage==88){p.enabled=false;IceBagPose(1);p.enabled=true;Next();return;}
   if(stage==89){ScreenCapture.CaptureScreenshot(Out+"ice-camp-sign.png");Next();return;}
   if(stage==90){p.enabled=false;IceBagPose(2);p.enabled=true;Next();return;}
   if(stage==91){ScreenCapture.CaptureScreenshot(Out+"bag-replace-any-slot.png");Next();return;}
   Check(errors.Count==0,''')
(root/'Validation/IceBagPolish/Smoke.cs').write_text(s,encoding='utf-8')
qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
shutil.copy2(root/'Validation/IceBagPolish/Smoke.cs',qa/'Assets/NakiPrototype/Editor/IceBagPolishSmoke.cs')
