from pathlib import Path
root=Path(__file__).resolve().parents[2];out=root/'Validation/ManorCourtyardV2'
s=(root/'Validation/ArmedEnemiesV2/Smoke.cs').read_text(encoding='utf-8').replace('NakiArmedEnemiesV2Smoke','NakiManorCourtyardV2Smoke').replace('Naki.ArmedEnemiesV2','Naki.ManorCourtyardV2').replace('Validation/ArmedEnemiesV2/runtime/','Validation/ManorCourtyardV2/runtime/')
s=s.replace('==132','==133')
s=s.replace('Check(!(bool)Call("TravelAtlas",5,0)&&p.fireChapterMode,"Unbuilt Manor is visible but cannot be entered");','Check(!(bool)Call("TravelAtlas",6,0)&&p.fireChapterMode,"Unbuilt kingdom remains visible but cannot be entered");')
s=s.replace('p.fireChapterMode=false;p.iceSampleMode=false;p.thunderChapterMode=', 'p.manorCourtyardMode=false;Set("manorIntro",false);p.fireChapterMode=false;p.iceSampleMode=false;p.thunderChapterMode=')
a=s.index(' static void Tick()');b=s.index(' static void SkillStarfallTests()',a)
s=s.replace('Directory.CreateDirectory(Out);NakiLegend','Directory.CreateDirectory(Out);NakiLegend.Prototype.Editor.NakiManorScenes.BuildScenes();NakiLegend',1)
a=s.index(' static void Tick()');b=s.index(' static void SkillStarfallTests()',a)
s=s[:a]+''' static void Tick()
 {
  if(!EditorApplication.isPlaying||EditorApplication.isCompiling||++ticks<25)return;p=UnityEngine.Object.FindObjectOfType<NakiPrototype>();if(p==null||Get("world")==null)return;
  try{
   if(stage==0){p.enabled=false;ArmedWeaponTests();ThemedRewardTests();SignatureReadabilityTests();IntentTests();SkillStarfallTests();FullIceTests();RouteClarityTests();ManorTests();Time.timeScale=0;ManorPose(0);stage=1;ticks=0;p.enabled=true;return;}
   if(stage==1){ScreenCapture.CaptureScreenshot(Out+"manor-quay.png");stage=2;ticks=0;return;}
   if(stage==2){ManorPose(13);p.enabled=false;ClearEnemies();Set("position",new Vector2(-4,-2));string[] names={"ManorAxe","ManorHammer","ManorPriest","ManorArcher","ManorMage"};for(int i=0;i<names.Length;i++)Spawn(names[i],new Vector2(-3+i*2,1));p.enabled=true;stage=3;ticks=0;return;}
   if(stage==3){ScreenCapture.CaptureScreenshot(Out+"manor-five-new-foes.png");stage=4;ticks=0;return;}
   if(stage==4){p.enabled=false;ClearEnemies();object priest=Spawn("ManorPriest",new Vector2(3,1));Set("clock",50f);Call("QueueManorImpact",priest,new Vector2(0,-1),1.65f,1.7f,1.25f,true);Set("clock",50.85f);Call("TickManorImpacts");p.enabled=true;stage=5;ticks=0;return;}
   if(stage==5){ScreenCapture.CaptureScreenshot(Out+"dark-priest-single-meteor.png");stage=6;ticks=0;return;}
   if(stage==6){ManorPose(14);p.enabled=false;Set("position",new Vector2(0,-.3f));Call("ConfigureJourneyView");((Camera)Get("view")).transform.position=(Vector3)Get("JourneyViewCenter");Call("UpdatePixelHero",Vector2.zero);object cap=List("enemies")[0];Set(cap,"attackNumber",0);Set(cap,"nextAttack",F("clock"));Call("TickManorCaptain",cap,.016f);p.enabled=true;stage=7;ticks=0;return;}
   if(stage==7){ScreenCapture.CaptureScreenshot(Out+"oath-captain-charge.png");stage=8;ticks=0;return;}
   if(stage==8){Set("manorIntro",true);stage=9;ticks=0;return;}
   if(stage==9){ScreenCapture.CaptureScreenshot(Out+"manor-opening.png");stage=10;ticks=0;return;}
   if(stage==10){ManorPose(0);((int[])Get("carriedWeapons"))[0]=132;Call("UpdatePixelHero",Vector2.zero);stage=11;ticks=0;return;}
   if(stage==11){ScreenCapture.CaptureScreenshot(Out+"captain-spear-held.png");stage=12;ticks=0;return;}
   if(stage==12){Call("OpenWorldAtlas",5);stage=13;ticks=0;return;}
   if(stage==13){ScreenCapture.CaptureScreenshot(Out+"manor-atlas.png");stage=14;ticks=0;return;}
   Check(errors.Count==0,"Native manor rendering has no runtime errors");File.WriteAllText(Out+"smoke-result.txt","PASS "+checks+" checks: manor first-floor graph, themed actors, armed AI, slow single meteor, captain movement/phase/energy/surrender, save/inventory and atlas return; previous combat and ice progression checks.\\n");End(0);
  }catch(Exception e){Debug.LogException(e);File.WriteAllText(Out+"smoke-result.txt","FAIL after "+checks+"\\n"+e+"\\n"+string.Join("\\n",errors));End(1);}
 }
 static void ManorPose(int room){p.manorCourtyardMode=p.thunderChapterMode=true;p.fireChapterMode=p.iceSampleMode=p.woodlandMode=false;p.thunderChapterPracticeBoss=-1;Call("BeginManorCourtyard",false);Set("manorIntro",false);Call("ChooseProfession",0);if(room==14){Set("chapterWaymarks",3);Set(Room(4),"cleared",true);Set(Room(13),"cleared",true);}Enter(room);Quiet();Set("position",new Vector2(-3,-2));Call("ConfigureJourneyView");((Camera)Get("view")).transform.position=(Vector3)Get("JourneyViewCenter");Call("UpdatePixelHero",Vector2.zero);}
 static void ClearEnemies(){foreach(object e in List("enemies")){UnityEngine.Object.Destroy((GameObject)Get(e,"root"));UnityEngine.Object.Destroy((GameObject)Get(e,"shadow"));}List("enemies").Clear();foreach(object e in List("pendingSpawns"))UnityEngine.Object.Destroy((GameObject)Get(e,"mark"));List("pendingSpawns").Clear();ClearShots();}
'''+s[b:]
s=s.replace(' static void End(', (out/'tests.txt').read_text(encoding='utf-8')+'\n static void End(')
(out/'Smoke.cs').write_text(s,encoding='utf-8')
print('Prepared manor native checks and previous 1059-check regression harness')
