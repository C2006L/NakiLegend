from pathlib import Path
import shutil
root=Path(__file__).resolve().parents[2];out=root/'Validation/EncounterIntentV1';qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
s=(root/'Validation/SkillStarfallV1/Smoke.cs').read_text(encoding='utf-8').replace('NakiSkillStarfallV1Smoke','NakiEncounterIntentV1Smoke').replace('Naki.SkillStarfallV1','Naki.EncounterIntentV1').replace('Validation/SkillStarfallV1/runtime/','Validation/EncounterIntentV1/runtime/')
a=s.index(' static void Tick()');b=s.index(' static void SkillStarfallTests()',a)
s=s[:a]+''' static void Tick()
 {
  if(!EditorApplication.isPlaying||EditorApplication.isCompiling||++ticks<35)return;p=UnityEngine.Object.FindObjectOfType<NakiPrototype>();if(p==null||Get("world")==null)return;
  try{
   if(stage==0){p.enabled=false;IntentTests();SkillStarfallTests();FullIceTests();RouteClarityTests();Quiet();Time.timeScale=0;IntentPoseFixture();p.enabled=true;stage=1;ticks=0;return;}
   if(stage==1){ScreenCapture.CaptureScreenshot(Out+"mixed-local-combat.png");stage=2;ticks=0;return;}
   if(stage==2){TideSignaturePose();stage=3;ticks=0;return;}
   if(stage==3){ScreenCapture.CaptureScreenshot(Out+"tide-gap-wall.png");stage=4;ticks=0;return;}
   if(stage==4){FirePose(42);Quiet();object boss=List("enemies")[0];Set("clock",40f);Set("position",new Vector2(-2,-1));Signature(boss);Set("clock",41f);Call("TickBossSignatures",.01f);Call("ConfigureJourneyView");((Camera)Get("view")).transform.position=(Vector3)Get("JourneyViewCenter");Call("UpdatePixelHero",Vector2.zero);stage=5;ticks=0;return;}
   if(stage==5){ScreenCapture.CaptureScreenshot(Out+"tortoise-shell.png");stage=6;ticks=0;return;}
   Check(errors.Count==0,"Native intent / boss / camera rendering has no runtime errors");File.WriteAllText(Out+"smoke-result.txt","PASS "+checks+" checks: distinct enemy intents, fixed charge commitment, no ordinary projectile lanes, fewer mixed formations, off-screen fire gate, clamped local camera, fifteen boss signatures, real safe tide gap and shell flanking, circle-bot hit simulation, plus skill/resource/save and full ice ending regressions.\\n");End(0);
  }catch(Exception e){Debug.LogException(e);File.WriteAllText(Out+"smoke-result.txt","FAIL after "+checks+"\\n"+e+"\\n"+string.Join("\\n",errors));End(1);}
 }
'''+s[b:]
s=s.replace(' static void End(', (out/'tests.txt').read_text(encoding='utf-8')+'\n static void End(')
(out/'Smoke.cs').write_text(s,encoding='utf-8')
for src in (root/'Assets/NakiPrototype').rglob('*'):
 if src.is_file():
  dst=qa/'Assets/NakiPrototype'/src.relative_to(root/'Assets/NakiPrototype')
  if not dst.exists() or src.read_bytes()!=dst.read_bytes():dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dst)
shutil.copy2(out/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/EncounterIntentV1Smoke.cs')
lines=(root/'Validation/SkillStarfallV1/compile.rsp').read_text(encoding='utf-8-sig').splitlines();lines=[x for x in lines if not x.strip('"').endswith('.cs') and not x.strip('"').startswith('-out:')]
lines+=['"-out:'+str(out/'runtime-compile.dll')+'"']+['"'+str(p)+'"' for p in (root/'Assets/NakiPrototype').rglob('*.cs')]
(out/'compile.rsp').write_text('\n'.join(lines),encoding='utf-8')
lines=[x for x in lines if not x.strip('"').endswith('.cs') and not x.strip('"').startswith('-out:')]
lines+=['"-out:'+str(out/'test-compile.dll')+'"','"-r:'+str(out/'runtime-compile.dll')+'"','"'+str(out/'Smoke.cs')+'"'];(out/'test-compile.rsp').write_text('\n'.join(lines),encoding='utf-8')
(out/'runtime').mkdir(exist_ok=True)
print('Staged intent, smaller formation, boss-identity, circle-bot and local camera native regressions')
