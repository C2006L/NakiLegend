from pathlib import Path
import subprocess, shutil, sys

root = Path(__file__).resolve().parents[2]
if '--reuse-runtime' not in sys.argv:
    subprocess.run(['python', str(root / 'Validation/CombatV3/build.py')], check=True)
s = (root / 'Validation/CombatV3/Smoke.cs').read_text(encoding='utf-8')
s = s.replace('NakiCombatV3Smoke', 'NakiCombatPolishSmoke').replace('Naki.CombatV3', 'Naki.CombatPolish')
s = s.replace('Validation/CombatV3/runtime/', 'Validation/CombatPolish/runtime/')
s = s.replace('CombatV3Tests();', 'CombatV3Tests();CombatPolishTests();')
s = s.replace(' public static void RunPreview()', ' public static void RunImpactPreview(){SessionState.SetBool("Naki.CombatPolish.ImpactOnly",true);RunPreview();}\n public static void RunPreview()')
s = s.replace('   if(stage==0&&SessionState.GetBool("Naki.CombatPolish.OnlyFire"', '   if(stage==0&&SessionState.GetBool("Naki.CombatPolish.ImpactOnly",false))stage=79;\n   if(stage==0&&SessionState.GetBool("Naki.CombatPolish.OnlyFire"')
s = s.replace(' static void Next(){', (root / 'Validation/CombatPolish/tests.txt').read_text(encoding='utf-8') + '\n static void Next(){')
s = s.replace('   Check(errors.Count==0,', '''   if(stage==79){p.enabled=false;PolishImpactPose();Time.timeScale=0;p.enabled=true;Next();return;}
   if(stage==80){ScreenCapture.CaptureScreenshot(Out+"element-impacts.png");Next();return;}
   if(stage==81){Time.timeScale=1;Next();return;}
   if(stage==82&&SessionState.GetBool("Naki.CombatPolish.ImpactOnly",false)){Check(File.Exists(Out+"element-impacts.png"),"Native impact preview capture written");Check(errors.Count==0,"Native impact preview renders without errors");File.WriteAllText(Out+"preview-result.txt","PASS "+checks+" native impact rendering checks");End(0);return;}
   Check(errors.Count==0,''')
s = s.replace('static void End(int code){', 'static void End(int code){SessionState.SetBool("Naki.CombatPolish.ImpactOnly",false);')
s = s.replace('checks: combat V3 input/', 'checks: ordered swept collision, safe retargeting, impact feedback, partial crystal save and contextual workshop hints; combat V3 input/')
(root / 'Validation/CombatPolish/Smoke.cs').write_text(s, encoding='utf-8')
qa = root.parent / 'Validation/DesertV2/UnitySmokeProject'
shutil.copy2(root / 'Validation/CombatPolish/Smoke.cs', qa / 'Assets/NakiPrototype/Editor/CombatPolishSmoke.cs')
print('Prepared combat polish native regression')
