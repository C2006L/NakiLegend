from pathlib import Path
import shutil,re
root=Path(__file__).resolve().parents[2];out=root/'Validation/SkillStarfallV1';qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
s=(root/'Validation/RouteClarityV3/Smoke.cs').read_text(encoding='utf-8').replace('NakiRouteClarityV3Smoke','NakiSkillStarfallV1Smoke').replace('Naki.RouteClarityV3','Naki.SkillStarfallV1').replace('Validation/RouteClarityV3/runtime/','Validation/SkillStarfallV1/runtime/')
start=s.index(' static void Tick()');end=s.index(' static void End(',start)
s=s[:start]+''' static void Tick()
 {
  if(!EditorApplication.isPlaying||EditorApplication.isCompiling||++ticks<35)return;p=UnityEngine.Object.FindObjectOfType<NakiPrototype>();if(p==null||Get("world")==null)return;
  try {
   if(stage==0){p.enabled=false;SkillStarfallTests();FullIceTests();IceMechanicsTests();RouteClarityTests();Quiet();Time.timeScale=0;StarfallPose(.25f);p.enabled=true;stage=1;ticks=0;return;}
   if(stage==1){ScreenCapture.CaptureScreenshot(Out+"starfall-warning.png");stage=2;ticks=0;return;}
   if(stage==2){StarfallPose(1.1f);stage=3;ticks=0;return;}
   if(stage==3){ScreenCapture.CaptureScreenshot(Out+"starfall-descent.png");stage=4;ticks=0;return;}
   if(stage==4){StarfallPose(1.24f);stage=5;ticks=0;return;}
   if(stage==5){ScreenCapture.CaptureScreenshot(Out+"starfall-impact.png");stage=6;ticks=0;return;}
   if(stage==6){Set("learnedElementSkills",4095);Set("chapterSkillsMenu",true);stage=7;ticks=0;return;}
   if(stage==7){ScreenCapture.CaptureScreenshot(Out+"global-skill-menu.png");stage=8;ticks=0;return;}
   Check(errors.Count==0,"Native combat and HUD render without errors");File.WriteAllText(Out+"smoke-result.txt","PASS "+checks+" checks: zero-cost attacks and charged bows, independent resource pools, actual multi-island travel/loadout saves, global elemental identities, learning without auto-equip, original-snapshot migration, starfall telegraph/descent/AoE/seal/cancellation, audio import, full ice route and ending regressions.\\n");End(0);
  }catch(Exception e){Debug.LogException(e);File.WriteAllText(Out+"smoke-result.txt","FAIL after "+checks+"\\n"+e+"\\n"+string.Join("\\n",errors));End(1);}
 }
'''+(out/'tests.txt').read_text(encoding='utf-8')+'\n'+s[end:]
(out/'Smoke.cs').write_text(s,encoding='utf-8')
for src in (root/'Assets/NakiPrototype').rglob('*'):
 if src.is_file():
  dst=qa/'Assets/NakiPrototype'/src.relative_to(root/'Assets/NakiPrototype')
  if not dst.exists() or src.read_bytes()!=dst.read_bytes():dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dst)
shutil.copy2(out/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/SkillStarfallV1Smoke.cs')
lines=(root/'Validation/RouteClarityV3/compile.rsp').read_text(encoding='utf-8-sig').splitlines()
lines=[x for x in lines if not x.strip('"').endswith('.cs') and not x.strip('"').startswith('-out:')]
lines+=['"-out:'+str(out/'runtime-compile.dll')+'"']+['"'+str(p)+'"' for p in (root/'Assets/NakiPrototype').rglob('*.cs')]
(out/'compile.rsp').write_text('\n'.join(lines),encoding='utf-8')
lines=(out/'compile.rsp').read_text(encoding='utf-8').splitlines();lines=[x for x in lines if not x.strip('"').endswith('.cs') and not x.strip('"').startswith('-out:')]
lines+=['"-out:'+str(out/'test-compile.dll')+'"','"-r:'+str(out/'runtime-compile.dll')+'"','"'+str(out/'Smoke.cs')+'"'];(out/'test-compile.rsp').write_text('\n'.join(lines),encoding='utf-8')
(out/'runtime').mkdir(exist_ok=True)
print('Staged native skill / starfall regression and four visual captures')
