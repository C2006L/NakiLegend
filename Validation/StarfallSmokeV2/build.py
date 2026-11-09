from pathlib import Path
import shutil

root=Path(__file__).resolve().parents[2]
out=root/'Validation/StarfallSmokeV2'
qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
s=(root/'Validation/SkillStarfallV1/Smoke.cs').read_text(encoding='utf-8').replace('NakiSkillStarfallV1Smoke','NakiStarfallSmokeV2Smoke').replace('Naki.SkillStarfallV1','Naki.StarfallSmokeV2').replace('Validation/SkillStarfallV1/runtime/','Validation/StarfallSmokeV2/runtime/')
a=s.index(' static void Tick()');b=s.index(' static void SkillStarfallTests()',a)
s=s[:a]+''' static void Tick()
 {
  if(!EditorApplication.isPlaying||EditorApplication.isCompiling||++ticks<35)return;p=UnityEngine.Object.FindObjectOfType<NakiPrototype>();if(p==null||Get("world")==null)return;
  try {
   if(stage==0){p.enabled=false;StarfallTests();SmokeVisualTests();Quiet();Time.timeScale=0;SmokePose(1.18f);p.enabled=true;stage=1;ticks=0;return;}
   if(stage==1){ScreenCapture.CaptureScreenshot(Out+"violet-orbs-descent.png");stage=2;ticks=0;return;}
   if(stage==2){SmokePose(1.55f);stage=3;ticks=0;return;}
   if(stage==3){ScreenCapture.CaptureScreenshot(Out+"violet-orbs-impact.png");stage=4;ticks=0;return;}
   Check(errors.Count==0,"Native violet orb and smoke rendering has no runtime errors");File.WriteAllText(Out+"smoke-result.txt","PASS "+checks+" checks: fixed three targets, unchanged cost/timing/damage/seal, bounded expanding smoke, violet sphere texture, impact aftermath and cancellation.\\n");End(0);
  }catch(Exception e){Debug.LogException(e);File.WriteAllText(Out+"smoke-result.txt","FAIL after "+checks+"\\n"+e+"\\n"+string.Join("\\n",errors));End(1);}
 }
 static void SmokePose(float time)
 {
  StarfallPose(time);Call("ConfigureJourneyView");((Camera)Get("view")).transform.position=(Vector3)Get("JourneyViewCenter");Quiet();
 }
 static void SmokeVisualTests()
 {
  SmokePose(1.18f);object first=List("fallingStars")[0];var orb=(GameObject)Get(first,"star");var sprite=orb.GetComponent<SpriteRenderer>().sprite;
  Check(sprite.name=="Starfall violet sphere","Descending object uses a shaded sphere, not a star silhouette");
  Color center=sprite.texture.GetPixel(16,16),corner=sprite.texture.GetPixel(0,0);
  Check(center.a>.95f&&center.b>center.g&&corner.a==0,"Purple sphere has an opaque core and transparent corners");
  var smoke=(SpriteRenderer[])Get(first,"smoke");Check(smoke.Length==24&&smoke.Count(x=>x.gameObject.activeSelf)>=8,"Dense trail has a bounded bank of actual smoke puffs");
  var puff=smoke[5];Vector3 at=puff.transform.position;float scale=puff.transform.localScale.x;Set("clock",21.2f);Call("TickStarfall");Check(puff.transform.localScale.x>scale&&puff.transform.position!=at,"Smoke expands and drifts independently of the falling orb");
  Check(((GameObject)Get(first,"tail")).GetComponent<SpriteRenderer>()==null,"Tail no longer draws a solid rectangular light streak");
  Set("clock",21.28f);Call("TickStarfall");Check((bool)Get(first,"hit")&&!orb.activeSelf&&smoke.Any(x=>x.gameObject.activeSelf),"Orb disappears on impact while its smoke aftermath remains");
  Call("ClearStarfall");Check(List("fallingStars").Count==0,"Room cleanup removes all effect roots and their smoke children");
 }
'''+s[b:]
(out/'Smoke.cs').write_text(s,encoding='utf-8')
for src in (root/'Assets/NakiPrototype').rglob('*'):
 if src.is_file():
  dst=qa/'Assets/NakiPrototype'/src.relative_to(root/'Assets/NakiPrototype')
  if not dst.exists() or src.read_bytes()!=dst.read_bytes():dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dst)
shutil.copy2(out/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/StarfallSmokeV2Smoke.cs')
lines=(root/'Validation/EncounterIntentV1/compile.rsp').read_text(encoding='utf-8-sig').splitlines()
lines=[x for x in lines if not x.strip('"').endswith('.cs') and not x.strip('"').startswith('-out:')]
lines+=['"-out:'+str(out/'runtime-compile.dll')+'"']+['"'+str(p)+'"' for p in (root/'Assets/NakiPrototype').rglob('*.cs')]
(out/'compile.rsp').write_text('\n'.join(lines),encoding='utf-8')
lines=[x for x in lines if not x.strip('"').endswith('.cs') and not x.strip('"').startswith('-out:')]
lines+=['"-out:'+str(out/'test-compile.dll')+'"','"-r:'+str(out/'runtime-compile.dll')+'"','"'+str(out/'Smoke.cs')+'"']
(out/'test-compile.rsp').write_text('\n'.join(lines),encoding='utf-8')
print('Staged native orb/smoke visual and starfall mechanics checks')
