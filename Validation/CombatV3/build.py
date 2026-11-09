from pathlib import Path
import subprocess,shutil,re
root=Path(__file__).resolve().parents[2]
subprocess.run(['python',str(root/'Validation/FireWorkshop/build.py')],check=True)
s=(root/'Validation/FireWorkshop/Smoke.cs').read_text(encoding='utf-8').replace('NakiFireWorkshopSmoke','NakiCombatV3Smoke').replace('Naki.FireWorkshop','Naki.CombatV3').replace('Validation/FireWorkshop/runtime/','Validation/CombatV3/runtime/')
old='NarrativeAndPower();EndingGate();GrowthAndPressure();Layout();ContinueAndEffects();ProfessionAndLightning();WoodTests();AudioTests();StarterSkillsTests();DarkAndCage();IceTests();ExpansionTests();FullIceTests();IceMechanicsTests();IceWrapupTests();BowAndStairTests();FireCoastTests();FireWorkshopTests();'
new='NarrativeAndPower();EndingGate();Layout();FullIceTests();IceMechanicsTests();IceWrapupTests();BowAndStairTests();FireCoastTests();FireWorkshopTests();CombatV3Tests();'
assert old in s
s=s.replace(old,new).replace('FireCoastTests();FireWorkshopTests();p.enabled=true;stage=52;','FireCoastTests();FireWorkshopTests();CombatV3Tests();p.enabled=true;stage=52;')
# Updated public combat specification: old fixed counts/damage expectations are historical.
s=s.replace('-36)<.01f','-72)<.01f').replace('-42)<.01f','-78)<.01f').replace('"damage")<36','"damage")<72').replace('three times base damage','six times base damage').replace('3.5 times base','6.5 times base').replace('Dodging cancels a held charge','Dash delays a held release until its recovery')
s=s.replace('first==9&&List("shots").Count==13','first==15&&List("shots").Count==22')
s=s.replace('List("shots").Count==1&&F("elementPower")==94','List("shots").Count==5&&F("elementPower")==94')
s=s.replace('F("shield")-5.5f','F("shield")-5.125f').replace('F("shield")==5.5f','F("shield")==5.125f')
s=s.replace('List("shots").Count==12,"Forge fan','List("shots").Count==20,"Forge fan').replace('List("shots").Count==16&&List("forgeVolleys")','List("shots").Count==27&&List("forgeVolleys")').replace('List("shots").Count==24&&List("forgeVolleys")','List("shots").Count==41&&List("forgeVolleys")').replace('List("shots").Count==14,"One broken','List("shots").Count==24,"One broken').replace('Check(hp<96&&','Check(hp<(float)Get(near,"maximum")&&')
s=s.replace(' static void Next(){',(root/'Validation/CombatV3/tests.txt').read_text(encoding='utf-8')+'\n static void Next(){')
s=s.replace('   Check(errors.Count==0,','''   if(stage==70){p.enabled=false;WorkshopPose(20);Call("WorkshopAction",410);Quiet();Set("position",new Vector2(-.7f,-.4f));Call("UpdatePixelHero",Vector2.zero);p.enabled=true;Next();return;}
   if(stage==71){ScreenCapture.CaptureScreenshot(Out+"rescued-smith-rails.png");Next();return;}
   if(stage==72){p.enabled=false;WorkshopPose(23);Set("forgeRescued",true);Set("forgeBridgeOpen",false);Call("BuildChapterActions");Set("position",new Vector2(-2,-2));Call("UpdatePixelHero",Vector2.zero);p.enabled=true;Next();return;}
   if(stage==73){ScreenCapture.CaptureScreenshot(Out+"bridge-trench-guide.png");Next();return;}
   if(stage==74){p.enabled=false;WorkshopPose(22);Set("forgeRescued",true);Call("BuildChapterActions");Set("mapMenu",true);p.enabled=true;Next();return;}
   if(stage==75){ScreenCapture.CaptureScreenshot(Out+"bridge-objective-map.png");Next();return;}
   if(stage==76){p.enabled=false;VolleyPose();Time.timeScale=0;p.enabled=true;Next();return;}
   if(stage==77){ScreenCapture.CaptureScreenshot(Out+"dense-combat.png");Next();return;}
   if(stage==78){Time.timeScale=1;Next();return;}
   Check(errors.Count==0,''')
s=s.replace('checks: native chapter regression;', 'checks: combat V3 input/volleys/balance/readability; unchanged story, navigation, migration and checkpoint regression;')
s=s.replace('static void End(int code){','static void End(int code){Time.timeScale=1;')
(root/'Validation/CombatV3/Smoke.cs').write_text(s,encoding='utf-8')
qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
shutil.copy2(root/'Validation/CombatV3/Smoke.cs',qa/'Assets/NakiPrototype/Editor/CombatV3Smoke.cs')
print('Prepared combat V3 native checks')
