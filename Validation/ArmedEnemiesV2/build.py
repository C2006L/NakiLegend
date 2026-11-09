from pathlib import Path
import shutil
root=Path(__file__).resolve().parents[2];out=root/'Validation/ArmedEnemiesV2';qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
s=(root/'Validation/SkillStarfallV1/Smoke.cs').read_text(encoding='utf-8').replace('NakiSkillStarfallV1Smoke','NakiArmedEnemiesV2Smoke').replace('Naki.SkillStarfallV1','Naki.ArmedEnemiesV2').replace('Validation/SkillStarfallV1/runtime/','Validation/ArmedEnemiesV2/runtime/')
a=s.index(' static void Tick()');b=s.index(' static void SkillStarfallTests()',a)
s=s[:a]+''' static void Tick()
 {
  if(!EditorApplication.isPlaying||EditorApplication.isCompiling||++ticks<35)return;p=UnityEngine.Object.FindObjectOfType<NakiPrototype>();if(p==null||Get("world")==null)return;
  try{
   if(stage==0){p.enabled=false;ArmedWeaponTests();ThemedRewardTests();SignatureReadabilityTests();IntentTests();SkillStarfallTests();FullIceTests();RouteClarityTests();Quiet();Time.timeScale=0;FireWeaponPose();p.enabled=true;stage=1;ticks=0;return;}
   if(stage==1){ScreenCapture.CaptureScreenshot(Out+"fire-weapons.png");stage=2;ticks=0;return;}
   if(stage==2){TideSignaturePose();stage=3;ticks=0;return;}
   if(stage==3){ScreenCapture.CaptureScreenshot(Out+"tide-gap-wall.png");stage=4;ticks=0;return;}
   if(stage==4){FirePose(42);Quiet();object boss=List("enemies")[0];Set("clock",40f);Set("position",new Vector2(-2,-1));Signature(boss);Set("clock",41f);Call("TickBossSignatures",.01f);Call("ConfigureJourneyView");((Camera)Get("view")).transform.position=(Vector3)Get("JourneyViewCenter");Call("UpdatePixelHero",Vector2.zero);stage=5;ticks=0;return;}
   if(stage==5){ScreenCapture.CaptureScreenshot(Out+"tortoise-shell.png");stage=6;ticks=0;return;}
   if(stage==6){BossEffectPose(27);stage=7;ticks=0;return;}
   if(stage==7){ScreenCapture.CaptureScreenshot(Out+"forge-chains.png");stage=8;ticks=0;return;}
   if(stage==8){BossEffectPose(12);stage=9;ticks=0;return;}
   if(stage==9){ScreenCapture.CaptureScreenshot(Out+"fire-hazards.png");stage=10;ticks=0;return;}
   if(stage==10){DepartureArmoryPose();stage=11;ticks=0;return;}
   if(stage==11){ScreenCapture.CaptureScreenshot(Out+"departure-armory.png");stage=12;ticks=0;return;}
   Check(errors.Count==0,"Native intent / boss / camera rendering has no runtime errors");File.WriteAllText(Out+"smoke-result.txt","PASS "+checks+" checks: weapon-matched spear/slash/lob attacks, themed elite and supply drops, one-time departure armory save, clear boss accents / HUD, plus previous enemy intent, local camera, skill/starfall/save and full ice route regressions.\\n");End(0);
  }catch(Exception e){Debug.LogException(e);File.WriteAllText(Out+"smoke-result.txt","FAIL after "+checks+"\\n"+e+"\\n"+string.Join("\\n",errors));End(1);}
 }
'''+s[b:]
(out/'tests.txt').write_text((root/'Validation/EncounterIntentV1/tests.txt').read_text(encoding='utf-8').replace('Set(Room(0),\"kind\",EnumVal(\"RoomKind\",\"Combat\"));Set(Room(0),\"cleared\",false);','Set(Room(0),\"kind\",EnumVal(\"RoomKind\",\"Combat\"));Set(Room(0),\"halfHeight\",6f);Set(Room(0),\"halfWidth\",10f);Set(Room(0),\"cleared\",false);')+'\n'+(out/'tests-extra.txt').read_text(encoding='utf-8'),encoding='utf-8')
s=s.replace(' static void End(', (out/'tests.txt').read_text(encoding='utf-8')+'\n static void End(')
(out/'Smoke.cs').write_text(s,encoding='utf-8')
for src in (root/'Assets/NakiPrototype').rglob('*'):
 if src.is_file():
  dst=qa/'Assets/NakiPrototype'/src.relative_to(root/'Assets/NakiPrototype')
  if not dst.exists() or src.read_bytes()!=dst.read_bytes():dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dst)
shutil.copy2(out/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/ArmedEnemiesV2Smoke.cs')
lines=(root/'Validation/SkillStarfallV1/compile.rsp').read_text(encoding='utf-8-sig').splitlines();lines=[x for x in lines if not x.strip('"').endswith('.cs') and not x.strip('"').startswith('-out:')]
lines+=['"-out:'+str(out/'runtime-compile.dll')+'"']+['"'+str(p)+'"' for p in (root/'Assets/NakiPrototype').rglob('*.cs')]
(out/'compile.rsp').write_text('\n'.join(lines),encoding='utf-8')
lines=[x for x in lines if not x.strip('"').endswith('.cs') and not x.strip('"').startswith('-out:')]
lines+=['"-out:'+str(out/'test-compile.dll')+'"','"-r:'+str(out/'runtime-compile.dll')+'"','"'+str(out/'Smoke.cs')+'"'];(out/'test-compile.rsp').write_text('\n'.join(lines),encoding='utf-8')
(out/'runtime').mkdir(exist_ok=True)
print('Staged armed enemies, themed rewards, departure save, boss accents and existing combat regressions')
