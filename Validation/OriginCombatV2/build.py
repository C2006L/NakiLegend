from pathlib import Path
import shutil
root=Path(__file__).resolve().parents[2];out=root/'Validation/OriginCombatV2';out.mkdir(exist_ok=True)
s=(root/'Validation/PassageV2/Smoke.cs').read_text(encoding='utf-8').replace('NakiPassageV2Smoke','NakiOriginCombatV2Smoke').replace('Naki.PassageV2','Naki.OriginCombatV2').replace('Validation/PassageV2/runtime/','Validation/OriginCombatV2/runtime/')
s=s.replace('PassageV2Tests();p.fireChapterMode=','PassageV2Tests();OriginCombatV2Tests();p.fireChapterMode=')
s=s.replace('p.enabled=false;PassageV2Tests();p.enabled=true;stage=155','p.enabled=false;OriginCombatV2Tests();p.enabled=true;stage=161')
lines=[]
for line in s.splitlines():
 if 'Unlocked dark laser consumes its own dark cost' in line:line='  UpdatedDarkSealTest();'
 elif 'Light guard uses light energy without spending' in line:line='  UpdatedGuardSolarTest();'
 elif 'Loading a checkpoint resets transient guard and beam timers' in line:line='  Set("originSecondReady",999f);Set("originThirdReady",999f);Call("RestoreOriginLegacy",31,31,1,0);Check(F("originSecondReady")==0&&F("originThirdReady")==0,"Restoring origin memory clears transient action timers before saved values are applied");'
 elif 'Light primary emits five orbs for one eight-light payment' in line:line='  OriginBeamContractTest();'
 if 'Five-island route memories unlock both advanced primary abilities' in line:line=line.replace('Select(dark);Enter(0);','Select(dark);Set("orbLearned",true);Enter(0);')
 lines.append(line)
s='\n'.join(lines)+'\n'
s=s.replace('Check(List("shots").Count==5&&F("energy")==94,"Light volley costs six light, independent of elemental mana");', 'Check((bool)Get("lightChannel")&&List("shots").Count==0&&F("energy")==96&&F("elementPower")==96,"Light channel startup spends four light independently of elemental mana");Call("StopOriginAction",true);')
s=s.replace('if(light)Set("energy",Mathf.Min(100,F("energy")+.3f));','if(light)Call("TickLightRecovery",.05f);')
s=s.replace('if(time>=F("nextSword"))Call("UseEquippedWeapon");if(time>=F("nextSpell"))Call(light?"CastLightVolley":"CastDarkMeteor");','if(light)Call("TickOriginInput",.05f,step%180<52,false,false);else if(time>=F("nextSpell"))Call("CastDarkMeteor");if(!(bool)Get("OriginActionActive")&&time>=F("nextSword"))Call("UseEquippedWeapon");')
s=s.replace('float time=F("clock")+.05f;Set("clock",time);Call("TickPlayerDefenses",.05f);','float time=F("clock")+.05f;Set("clock",time);if(light){Vector2 at=(Vector2)Get("position"),goal=(Vector2)Get(boss,"position")+new Vector2(Mathf.Cos(time*.3f),Mathf.Sin(time*.3f))*3;Set("position",(Vector2)Call("Move",at,Vector2.ClampMagnitude(goal-at,.14f),.3f));Set("facing",((Vector2)Get(boss,"position")-(Vector2)Get("position")).normalized);}Call("TickPlayerDefenses",.05f);')
s=s.replace('   Check(immortal?(float)Get(boss,"hp")<=0:', '   File.WriteAllLines(Out+"combat-probes.csv",rows);Check(immortal?(float)Get(boss,"hp")<=0:')
s=s.replace('Set("primarySkill",0);Set("orbLearned",true);Set("position",new Vector2(0,-4));','Set("primarySkill",0);Set("orbLearned",true);if(light)Set("originKnowledge",10);Set("position",new Vector2(0,-4));')
s=s.replace('Call("TickOriginInput",.05f,step%180<52,false,false);','Call("TickOriginInput",.05f,step%180<52,time>=F("originSecondReady"),time>=F("originThirdReady"));')
s=s.replace('"light_stationary_upper_damage_probe"','"light_mobile_full_kit_probe"')
s=s.replace('object boss=List("enemies")[0];Set("encounterRecord",null);','object boss=List("enemies")[0];float probeBossInitial=(float)Get(boss,"hp");Set("encounterRecord",null);')
s=s.replace('Check(immortal?(float)Get(boss,"hp")<=0:F("health")<=0||hitCount>=3,"Sustained real combat probe exercises the boss, projectile damage and resource loop: "+immortal);','Check(light?probeBossInitial-(float)Get(boss,"hp")>100&&casts>=10:immortal?(float)Get(boss,"hp")<=0:F("health")<=0||hitCount>=3,"Sustained real combat probe exercises real damage and resource loops without requiring the redesigned Light kit to match the old orb kill time: "+profile);')
s=s.replace(' static void Next(){',(out/'tests.txt').read_text(encoding='utf-8')+'\n static void Next(){')
s=s.replace('   Check(errors.Count==0,"Actual HUD','''   if(stage==161){p.enabled=false;OriginBeamPose(false);p.enabled=true;Next();return;}
   if(stage==162){ScreenCapture.CaptureScreenshot(Out+"light-channel.png");Next();return;}
   if(stage==163){p.enabled=false;OriginGuardPose();p.enabled=true;Next();return;}
   if(stage==164){ScreenCapture.CaptureScreenshot(Out+"light-guard.png");Next();return;}
   if(stage==165){p.enabled=false;OriginBeamPose(true);p.enabled=true;Next();return;}
   if(stage==166){ScreenCapture.CaptureScreenshot(Out+"solar-piercing.png");Next();return;}
   if(stage==167){p.enabled=false;Set("originAbilityMenu",true);p.enabled=true;Next();return;}
   if(stage==168){ScreenCapture.CaptureScreenshot(Out+"origin-key-guide.png");Next();return;}
   if(stage==169){p.enabled=false;OriginSealPose();p.enabled=true;Next();return;}
   if(stage==170){ScreenCapture.CaptureScreenshot(Out+"soul-seal.png");Next();return;}
   Check(errors.Count==0,"Actual HUD''')
(out/'Smoke.cs').write_text(s,encoding='utf-8')
qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
for src in (root/'Assets/NakiPrototype').rglob('*'):
 if src.is_file():
  dst=qa/'Assets/NakiPrototype'/src.relative_to(root/'Assets/NakiPrototype')
  if not dst.exists() or src.read_bytes()!=dst.read_bytes():dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dst)
shutil.copy2(out/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/OriginCombatV2Smoke.cs')
lines=(root/'Validation/PassageV2/compile.rsp').read_text(encoding='utf-8-sig').splitlines()
lines=[line for line in lines if not line.strip('"').endswith('.cs') and not line.strip('"').startswith('-out:')]
lines+=['"-out:'+str(out/'runtime-compile.dll')+'"']+['"'+str(p)+'"' for p in (root/'Assets/NakiPrototype').rglob('*.cs')]
(out/'compile.rsp').write_text('\n'.join(lines),encoding='utf-8')
print('Staged origin channels, independent bindings, learned progression and save compatibility')
