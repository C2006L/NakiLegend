from pathlib import Path
import shutil
root=Path(__file__).resolve().parents[2]
out=root/"Validation/PressureV1"
s=(root/"Validation/LegacyV1/Smoke.cs").read_text(encoding="utf-8").replace("NakiLegacyV1Smoke","NakiPressureV1Smoke").replace("Naki.LegacyV1","Naki.PressureV1").replace("Validation/LegacyV1/runtime/","Validation/PressureV1/runtime/")
s=s.replace("(20+(scheduled-20)*.92f)","(20+(float)Get(enemy,\"windup\")+(scheduled-20-(float)Get(enemy,\"windup\"))*.85f)").replace("Enemy scheduling becomes eight percent faster without changing its telegraph","Enemy downtime shortens while the authored attack telegraph stays intact")
s=s.replace('before+1,"Ordinary encounter receives one additional melee reinforcement"','before+2,"Ordinary encounter receives a two-role reinforcement formation"').replace('before+1,"Wave companion is added once, not once per frame"','before+2,"Tactical reinforcements are added once, not once per frame"')
s=s.replace("LegacyV1Tests();p.fireChapterMode=","LegacyV1Tests();PressureTests();p.fireChapterMode=")
s=s.replace('Set("originAbilityMenu",false);','Set("originAbilityMenu",false);Set("challengeMenu",false);')
s=s.replace(' static void Next(){',(out/"tests.txt").read_text(encoding="utf-8")+'\n static void Next(){')
s=s.replace('   Check(errors.Count==0,"Actual HUD', '   if(stage==137){p.enabled=false;PressurePose();p.enabled=true;Next();return;}\n   if(stage==138){ScreenCapture.CaptureScreenshot(Out+"tactical-room.png");Next();return;}\n   if(stage==139){p.enabled=false;ChallengePose();p.enabled=true;Next();return;}\n   if(stage==140){ScreenCapture.CaptureScreenshot(Out+"optional-challenge.png");Next();return;}\n   if(stage==141){p.enabled=false;PressureBossPose();p.enabled=true;Next();return;}\n   if(stage==142){ScreenCapture.CaptureScreenshot(Out+"boss-pressure.png");Next();return;}\n   Check(errors.Count==0,"Actual HUD')
s=s.replace("native UI rendering.","native UI rendering; tactical cooperation, bounded staggered reinforcements, boss pressure phases and follow-ups, destructible cover, optional challenges and save continuity.")
(out/"Smoke.cs").write_text(s,encoding="utf-8")
qa=root.parent/"Validation/DesertV2/UnitySmokeProject"
for src in (root/"Assets/NakiPrototype").rglob("*"):
 if src.is_file():
  dst=qa/"Assets/NakiPrototype"/src.relative_to(root/"Assets/NakiPrototype")
  if not dst.exists() or dst.read_bytes()!=src.read_bytes():dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dst)
shutil.copy2(out/"Smoke.cs",qa/"Assets/NakiPrototype/Editor/PressureV1Smoke.cs")
print("Prepared isolated pressure, cooperation, cover and challenge regression")
