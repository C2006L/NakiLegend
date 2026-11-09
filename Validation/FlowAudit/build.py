from pathlib import Path
import shutil
root=Path(__file__).resolve().parents[2]
out=root/"Validation/FlowAudit"
s=(root/"Validation/PressureV1/Smoke.cs").read_text(encoding="utf-8").replace("NakiPressureV1Smoke","NakiFlowAuditSmoke").replace("Naki.PressureV1","Naki.FlowAudit").replace("Validation/PressureV1/runtime/","Validation/FlowAudit/runtime/")
s=s.replace("PressureTests();p.fireChapterMode=","PressureTests();FlowAuditTests();p.fireChapterMode=")
s=s.replace('((GameObject)Get("chapterFocus")).transform.Find("Workshop objective exit arrow")','((GameObject)Get("flowGuide")).transform')
s=s.replace(' static void Next(){',(out/"tests.txt").read_text(encoding="utf-8")+'\n static void Next(){')
s=s.replace('   Check(errors.Count==0,"Actual HUD','   if(stage==143){p.enabled=false;FlowAuditPose();p.enabled=true;Next();return;}\n   if(stage==144){ScreenCapture.CaptureScreenshot(Out+"fire-direct-return.png");Next();return;}\n   Check(errors.Count==0,"Actual HUD')
(out/"Smoke.cs").write_text(s,encoding="utf-8")
qa=root.parent/"Validation/DesertV2/UnitySmokeProject"
for src in (root/"Assets/NakiPrototype").rglob("*"):
 if src.is_file():
  dst=qa/"Assets/NakiPrototype"/src.relative_to(root/"Assets/NakiPrototype")
  if not dst.exists() or dst.read_bytes()!=src.read_bytes():dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dst)
shutil.copy2(out/"Smoke.cs",qa/"Assets/NakiPrototype/Editor/FlowAuditSmoke.cs")
print("Prepared five-island ending-flow audit")
