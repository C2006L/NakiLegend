from pathlib import Path
import subprocess,shutil,sys
root=Path(__file__).resolve().parents[2]
if '--reuse-runtime' not in sys.argv:
    subprocess.run(['python',str(root/'Validation/CombatPolish/build.py')],check=True)
s=(root/'Validation/CombatPolish/Smoke.cs').read_text(encoding='utf-8').replace('NakiCombatPolishSmoke','NakiBridgeSceneSmoke').replace('Naki.CombatPolish','Naki.BridgeScene').replace('Validation/CombatPolish/runtime/','Validation/BridgeScene/runtime/')
s=s.replace('CombatPolishTests();','CombatPolishTests();BridgeSceneTests();')
# Previous assertions used the old decorative pit coordinate.
s=s.replace('Call("Blocked",new Vector2(1,-1.4f),.1f)','Call("Blocked",new Vector2(3,0),.1f)')
s=s.replace(' static void Next(){',(root/'Validation/BridgeScene/tests.txt').read_text(encoding='utf-8')+'\n static void Next(){')
s=s.replace('   Check(errors.Count==0,','''   if(stage==82){p.enabled=false;BridgeScenePose(false);p.enabled=true;Next();return;}
   if(stage==83){ScreenCapture.CaptureScreenshot(Out+"bridge-closed.png");Next();return;}
   if(stage==84){p.enabled=false;BridgeScenePose(true);p.enabled=true;Next();return;}
   if(stage==85){ScreenCapture.CaptureScreenshot(Out+"bridge-open.png");Next();return;}
   Check(errors.Count==0,''')
s=s.replace('checks: ordered swept','checks: full-height evacuation trench, real bank crossing and worker evacuation/save tests; ordered swept')
(root/'Validation/BridgeScene/Smoke.cs').write_text(s,encoding='utf-8')
qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
for source in (root/'Assets/NakiPrototype').glob('*.cs'):
    target=qa/'Assets/NakiPrototype'/source.name
    if not target.exists() or target.read_bytes()!=source.read_bytes():shutil.copy2(source,target)
shutil.copy2(root/'Validation/BridgeScene/Smoke.cs',qa/'Assets/NakiPrototype/Editor/BridgeSceneSmoke.cs')
print('Prepared bridge scene regression')
