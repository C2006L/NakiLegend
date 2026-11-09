from pathlib import Path
import subprocess,shutil
root=Path(__file__).resolve().parents[2]
subprocess.run(['python',str(root/'Validation/FireCoast/build.py')],check=True)
s=(root/'Validation/FireCoast/Smoke.cs').read_text(encoding='utf-8').replace('NakiFireCoastSmoke','NakiFireWorkshopSmoke').replace('Naki.FireCoast','Naki.FireWorkshop').replace('Validation/FireCoast/runtime/','Validation/FireWorkshop/runtime/')
s=s.replace('FireCoastTests();p.fireChapterMode=false;', 'FireCoastTests();FireWorkshopTests();p.fireChapterMode=false;')
s=s.replace('FireCoastTests();p.enabled=true;stage=52;', 'FireCoastTests();FireWorkshopTests();p.enabled=true;stage=52;')
s=s.replace(' static void Next(){',(root/'Validation/FireWorkshop/tests.txt').read_text(encoding='utf-8')+'\n static void Next(){')
s=s.replace('   Check(errors.Count==0,','''   if(stage==62){p.enabled=false;WorkshopPose(21);Set("clock",F("clock")+3);Call("TickPendingSpawns");foreach(object e in List("enemies"))Set(e,"nextAttack",100000f);p.enabled=true;Next();return;}
   if(stage==63){ScreenCapture.CaptureScreenshot(Out+"workshop-battle.png");Next();return;}
   if(stage==64){p.enabled=false;WorkshopPose(27);object e=List("enemies")[0];Set("position",new Vector2(-3,-1));Set(e,"hp",650f);Set(e,"nextAttack",100000f);Call("UpdateWorkshopEnemy",e,.01f);Set(e,"attackNumber",2);Set(e,"attackDirection",(new Vector2(-3,-1)).normalized);Set(e,"windup",2f);e.GetType().GetField("warning",B).SetValue(e,Call("ForgeWarning",e));p.enabled=true;Next();return;}
   if(stage==65){ScreenCapture.CaptureScreenshot(Out+"forge-master.png");Next();return;}
   if(stage==66){p.enabled=false;WorkshopPose(23);Set("forgeRescued",true);Call("WorkshopAction",411);Quiet();foreach(object b in List("bridgeLowerings"))Set(b,"start",Time.unscaledTime-1);Call("TickWinchMotions");Set("position",new Vector2(-2.5f,-1.8f));Call("UpdatePixelHero",Vector2.zero);p.enabled=true;Next();return;}
   if(stage==67){ScreenCapture.CaptureScreenshot(Out+"evacuation-bridge.png");Next();return;}
   if(stage==68){p.enabled=false;WorkshopPose(20);Call("WorkshopAction",410);Quiet();Set("chapterSkillsMenu",true);p.enabled=true;Next();return;}
   if(stage==69){ScreenCapture.CaptureScreenshot(Out+"ember-skills.png");Next();return;}
   Check(errors.Count==0,''')
s=s.replace('Fire Coast 15 rooms and all waves;','Fire Coast and Workshop 30 rooms and all waves; old fifteen-room save migration; evacuation bridge; forge master molds and delayed volleys; ember ring; eight shield nonstacking progression;')
(root/'Validation/FireWorkshop/Smoke.cs').write_text(s,encoding='utf-8')
qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
shutil.copy2(root/'Validation/FireWorkshop/Smoke.cs',qa/'Assets/NakiPrototype/Editor/FireWorkshopSmoke.cs')
print('Prepared two-floor forge native regression')
