from pathlib import Path
import shutil,subprocess,uuid
root=Path(__file__).resolve().parents[2]
subprocess.run(['python',str(root/'Validation/IceChapter/build.py')],check=True)
s=(root/'Validation/IceChapter/Smoke.cs').read_text(encoding='utf-8').replace('NakiIceChapterSmoke','NakiFireCoastSmoke').replace('Validation/IceChapter/runtime/','Validation/FireCoast/runtime/').replace('Naki.IceChapter','Naki.FireCoast')
s=s.replace('static void Fresh(bool dark=true){','static void Fresh(bool dark=true){p.fireChapterMode=false;')
s=s.replace('static void Chapter(int floor=0,int boss=-1){','static void Chapter(int floor=0,int boss=-1){p.fireChapterMode=false;')
s=s.replace('static void Ice(bool practice=false){','static void Ice(bool practice=false){p.fireChapterMode=false;')
s=s.replace('EditorSceneManager.OpenScene("Assets/Scenes/NakiTest.unity");UnityEngine', 'NakiLegend.Prototype.Editor.NakiSampleScenes.BuildFireScenes();PlayerSettings.companyName="NakiValidation";PlayerSettings.productName="FireCoast"+DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");EditorSceneManager.OpenScene("Assets/Scenes/NakiTest.unity");UnityEngine')
s=s.replace('BowAndStairTests();p.iceSampleMode=false;', 'BowAndStairTests();FireCoastTests();p.fireChapterMode=false;p.iceSampleMode=false;')
s=s.replace(' static void Next(){',(root/'Validation/FireCoast/tests.txt').read_text(encoding='utf-8')+'\n static void Next(){')
s=s.replace(' public static void RunPreview()', ' public static void RunFireRegression(){SessionState.SetBool("Naki.FireCoast.OnlyFire",true);RunPreview();}\n public static void RunPreview()')
s=s.replace('   if(stage==0&&SessionState.GetBool("Naki.FireCoast.StairsPreview",false))stage=46;', '   if(stage==0&&SessionState.GetBool("Naki.FireCoast.OnlyFire",false)){p.enabled=false;FireCoastTests();p.enabled=true;stage=52;}\n   if(stage==0&&SessionState.GetBool("Naki.FireCoast.StairsPreview",false))stage=46;',1)
s=s.replace('static void End(int code){', 'static void End(int code){SessionState.SetBool("Naki.FireCoast.OnlyFire",false);')
s=s.replace('"FAIL after "+checks', '"FAIL after "+checks')
s=s.replace('Out+"smoke-result.txt","FAIL after', 'Out+(SessionState.GetBool("Naki.FireCoast.Preview",false)?"preview-result.txt":"smoke-result.txt"),"FAIL after')
start=s.index(' checks: five-room ice map;')
end=s.index('\\n");End(0);',start)
s=s[:start]+' checks: native chapter regression; Fire Coast 15 rooms and all waves; five new creature sprites; safe doors and shelter winch; ice and physical shell breaking; phased fire boss; delayed and destructible coal; finite burning; fire seed energy and cooldown; independent checkpoints and backups; real Ice/Fire boat roundtrip and inherited equipment; gameplay screenshots.'+s[end:]
s=s.replace('   Check(errors.Count==0,','''   if(stage==52){p.enabled=false;FirePose(0);p.enabled=true;Next();return;}
   if(stage==53){ScreenCapture.CaptureScreenshot(Out+"fire-harbor.png");Next();return;}
   if(stage==54){p.enabled=false;FirePose(6);ClearActors();foreach(var pair in new[]{Tuple.Create("AshRaider",new Vector2(-2,1)),Tuple.Create("EmberLizard",new Vector2(2,1)),Tuple.Create("AshBat",new Vector2(-2,-1)),Tuple.Create("CoalThrower",new Vector2(2,-1))}){object e=Spawn(pair.Item1,pair.Item2);Set(e,"nextAttack",100000f);Call("UpdateFireEnemy",e,.01f);}p.enabled=true;Next();return;}
   if(stage==55){ScreenCapture.CaptureScreenshot(Out+"fire-encounter.png");Next();return;}
   if(stage==56){p.enabled=false;FirePose(12);Set("chapterRanger",true);object e=List("enemies")[0];Set("position",new Vector2(-3,0));Set(e,"nextAttack",0f);Set(e,"attackNumber",1);Call("UpdateFireEnemy",e,.01f);p.enabled=true;Next();return;}
   if(stage==57){ScreenCapture.CaptureScreenshot(Out+"red-spine-lizard.png");Next();return;}
   if(stage==58){p.enabled=false;FirePose(13);Set("chapterStone",true);Set(Room(12),"cleared",true);Call("BuildChapterActions");p.enabled=true;Next();return;}
   if(stage==59){ScreenCapture.CaptureScreenshot(Out+"training-forge.png");Next();return;}
   if(stage==60){p.enabled=false;Set("fireBoltKnown",true);Set("chapterSkillsMenu",true);p.enabled=true;Next();return;}
   if(stage==61){ScreenCapture.CaptureScreenshot(Out+"fire-skills.png");Next();return;}
   Check(errors.Count==0,''')
(root/'Validation/FireCoast/Smoke.cs').write_text(s,encoding='utf-8')
qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
src=root/'Assets/NakiPrototype'
for path in src.rglob('*'):
    if not path.is_file():continue
    target=qa/'Assets/NakiPrototype'/path.relative_to(src)
    target.parent.mkdir(parents=True,exist_ok=True)
    shutil.copy2(path,target)
shutil.copy2(root/'Validation/FireCoast/Smoke.cs',qa/'Assets/NakiPrototype/Editor/FireCoastSmoke.cs')
# The old harness may coexist but is not executed.
print('Prepared native Unity regression project')
