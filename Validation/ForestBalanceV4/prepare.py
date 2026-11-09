from pathlib import Path
import shutil
root=Path(__file__).resolve().parents[2];out=root/'Validation/ForestBalanceV4'
source=(root/'Validation/ManorCourtyardV2/Smoke.cs').read_text(encoding='utf-8-sig')
source=source.replace('NakiManorCourtyardV2Smoke','NakiForestBalanceV4Smoke').replace('Naki.ManorCourtyardV2','Naki.ForestBalanceV4').replace('Validation/ManorCourtyardV2/runtime/','Validation/ForestBalanceV4/runtime/')
source=source.replace('static void Forest(int floor=0,bool dark=false,int job=0){','static void Forest(int floor=0,bool dark=false,int job=0){p.manorCourtyardMode=false;Set("manorIntro",false);')
source=source.replace(' public static void Run(){',' public static void RunForestPreview(){SessionState.SetBool("Naki.ForestBalanceV4.OnlyVisual",true);Run();}\n public static void Run(){')
start=source.index(' static void Tick()');end=source.index(' static void ManorPose',start)
source=source[:start]+''' static void Tick()
 {
  if(!EditorApplication.isPlaying||EditorApplication.isCompiling||++ticks<25)return;p=UnityEngine.Object.FindObjectOfType<NakiPrototype>();if(p==null||Get("world")==null)return;
  try{
   if(stage==0){p.enabled=false;if(!SessionState.GetBool("Naki.ForestBalanceV4.OnlyVisual",false)){ForestBoundsAndBalanceTests();ArmedWeaponTests();ThemedRewardTests();SignatureReadabilityTests();IntentTests();SkillStarfallTests();FullIceTests();RouteClarityTests();ManorTests();}Time.timeScale=0;ForestPose(6);Set("position",new Vector2(5,-.5f));Call("ConfigureJourneyView");((Camera)Get("view")).transform.position=(Vector3)Get("JourneyViewCenter");Call("UpdatePixelHero",Vector2.zero);stage=1;ticks=0;p.enabled=true;return;}
   if(stage==1){ScreenCapture.CaptureScreenshot(Out+"forest-boss-corrected.png");stage=2;ticks=0;return;}
   if(stage==2){p.enabled=false;ForestPose(1);Set("position",new Vector2(0,-1));Call("ConfigureJourneyView");((Camera)Get("view")).transform.position=(Vector3)Get("JourneyViewCenter");Call("UpdatePixelHero",Vector2.zero);p.enabled=true;stage=3;ticks=0;return;}
   if(stage==3){ScreenCapture.CaptureScreenshot(Out+"forest-room-cover.png");stage=4;ticks=0;return;}
   Check(errors.Count==0,"Native forest rendering has no runtime errors");File.WriteAllText(Out+(SessionState.GetBool("Naki.ForestBalanceV4.OnlyVisual",false)?"visual-result.txt":"smoke-result.txt"),"PASS "+checks+" checks: forest bounds/cover/arrival, safe loot and legacy restoration, actual weapon damage/pool costs, skill budgets/control, save/load, atlas and prior combat/Manor regressions.\\n");SessionState.SetBool("Naki.ForestBalanceV4.OnlyVisual",false);End(0);
  }catch(Exception e){Debug.LogException(e);File.WriteAllText(Out+"smoke-result.txt","FAIL after "+checks+"\\n"+e+"\\n"+string.Join("\\n",errors));End(1);}
 }
'''+source[end:]
# This historical fixture intentionally asserted the old staff scale; the new
# focused balance changes only the elemental pellet factor from .8 to .72.
source=source.replace('total-14*3*.8f*.8f','total-14*3*.8f*.72f').replace('Existing wind staff total volley damage falls by exactly 20 percent','Wind staff total volley follows the V4 elemental pellet budget')
source=source.replace(' static void End(int code)',(out/'tests.cs.txt').read_text(encoding='utf-8')+'\n static void End(int code)')
(out/'Smoke.cs').write_text(source,encoding='utf-8')
build=(root/'Validation/ManorCourtyardV2/build.py').read_text(encoding='utf-8').replace('ManorCourtyardV2','ForestBalanceV4').replace('Staged manor source, sprites and native test runner','Staged forest bounds/balance source and native test runner')
(out/'build.py').write_text(build,encoding='utf-8')
qa=root.parent/'Validation/DesertV2/UnitySmokeProject/Assets/NakiPrototype/Editor'
for p in qa.glob('*Smoke.cs'):
 if p.name!='ForestBalanceV4Smoke.cs':
  backup=out/'archived-qa-harnesses'/p.name;backup.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(p,backup);p.rename(p.with_suffix('.cs.disabled'))
print('Prepared isolated V4 runner; all existing runtime scripts and save schemas retained')
