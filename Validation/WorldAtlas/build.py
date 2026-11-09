from pathlib import Path
import shutil
root=Path(__file__).resolve().parents[2];out=root/'Validation/WorldAtlas';out.mkdir(exist_ok=True)
s=(root/'Validation/FlowAudit/Smoke.cs').read_text(encoding='utf-8').replace('NakiFlowAuditSmoke','NakiWorldAtlasSmoke').replace('Naki.FlowAudit','Naki.WorldAtlas').replace('Validation/FlowAudit/runtime/','Validation/WorldAtlas/runtime/')
s=s.replace('FlowAuditTests();p.fireChapterMode=','FlowAuditTests();WorldAtlasTests();p.fireChapterMode=')
s=s.replace('Set("originAbilityMenu",false);','Set("originAbilityMenu",false);Set("worldAtlasMenu",false);Set("atlasConfirm",false);')
s=s.replace(' static void Next(){',(out/'tests.txt').read_text(encoding='utf-8')+'\n static void Next(){')
s=s.replace(' public static void Run(){',' public static void RunAtlasOnly(){SessionState.SetBool("Naki.WorldAtlas.AtlasOnly",true);Run();}\n public static void Run(){')
s=s.replace('   if(stage==0){p.enabled=false;NarrativeAndPower();','   if(stage==0&&SessionState.GetBool("Naki.WorldAtlas.AtlasOnly",false)){p.enabled=false;WorldAtlasTests();p.enabled=true;stage=145;ticks=0;return;}\n   if(stage==0){p.enabled=false;NarrativeAndPower();')
s=s.replace('static void End(int code){','static void End(int code){SessionState.SetBool("Naki.WorldAtlas.AtlasOnly",false);')
s=s.replace('   Check(errors.Count==0,"Actual HUD','   if(stage==145){p.enabled=false;AtlasPose();p.enabled=true;Next();return;}\n   if(stage==146){ScreenCapture.CaptureScreenshot(Out+"naki-world-map.png");Next();return;}\n   if(stage==147){p.enabled=false;ForestFinishPose();p.enabled=true;Next();return;}\n   if(stage==148){ScreenCapture.CaptureScreenshot(Out+"forest-seedbed.png");Next();return;}\n   Check(errors.Count==0,"Actual HUD')
(out/'Smoke.cs').write_text(s,encoding='utf-8')
qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
for src in (root/'Assets/NakiPrototype').rglob('*'):
 if src.is_file():
  dst=qa/'Assets/NakiPrototype'/src.relative_to(root/'Assets/NakiPrototype')
  if not dst.exists() or src.read_bytes()!=dst.read_bytes():dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dst)
shutil.copy2(out/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/WorldAtlasSmoke.cs')
lines=(root/'Validation/PressureV1/compile.rsp').read_text(encoding='utf-8-sig').splitlines()
lines=[s for s in lines if not s.strip('"').endswith('.cs') and not s.strip('"').startswith('-out:')]
lines+=['"-out:'+str(out/'runtime-compile.dll')+'"']+['"'+str(p)+'"' for p in (root/'Assets/NakiPrototype').rglob('*.cs')]
(out/'compile.rsp').write_text('\n'.join(lines),encoding='utf-8')
print('Staged isolated world-map + forest completion regression')
