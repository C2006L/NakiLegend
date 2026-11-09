from pathlib import Path
import re,shutil
r=Path(__file__).resolve().parents[1];o=r/'Validation/ManorPressureV6';o.mkdir(exist_ok=True)
p=r/'Assets/NakiPrototype/NakiPrototype.ManorCombat.cs';s=p.read_text(encoding='utf-8');s=s.replace('SpellSpark(e.position+e.attackDirection*.7f,C("b6b3d5"),.18f);','if(clock>=s.nextDust){s.nextDust=clock+.18f;SpellSpark(e.position+e.attackDirection*.7f,C("b6b3d5"),.35f);}')
p.write_text(s,encoding='utf-8')
s=(r/'Validation/WeaponRecoveryV5/Smoke.cs').read_text(encoding='utf-8-sig').replace('NakiWeaponRecoveryV5Smoke','NakiManorPressureV6Smoke').replace('Naki.WeaponRecoveryV5','Naki.ManorPressureV6').replace('Validation/WeaponRecoveryV5/runtime/','Validation/ManorPressureV6/runtime/')
s=s.replace('DivineLightTests();MasteryProgressionTests();WeaponRecoveryTests();ForestBoundsAndBalanceTests();ArmedWeaponTests();ThemedRewardTests();SignatureReadabilityTests();IntentTests();SkillStarfallTests();FullIceTests();RouteClarityTests();ManorTests();','CaptainPressureTests();DivineLightTests();MasteryProgressionTests();ArmedWeaponTests();ManorTests();')
s=s.replace('List("pendingSpawns").Count==2,"Captain pressure phase','List("pendingSpawns").Count==3,"Captain pressure phase')
extra='''
 static void CaptainPressureTests(){
  ManorPose(14);Quiet();object boss=List("enemies")[0];
  var state=Activator.CreateInstance(typeof(NakiPrototype).GetNestedType("ManorCaptainState",BindingFlags.NonPublic));
  Check((float)Get(boss,"maximum")==6120,"Attack redesign preserves boss health");
  Set(boss,"attackNumber",4);Call("ResolveManorCaptain",boss,state);Check((float)Get(state,"shieldUntil")>F("clock")&&(int)Get(boss,"tacticRole")==5,"Shield advance has timed directional guard");
  Set(boss,"attackNumber",5);int before=List("shots").Count;Call("ResolveManorCaptain",boss,state);Check(List("shots").Count-before==14,"Opening circular volley has fourteen spaced projectiles");
  Set(boss,"difficultyPhase",2);before=List("shots").Count;Call("ResolveManorCaptain",boss,state);Check(List("shots").Count-before==22&&(int)Get(state,"bursts")==4,"Late phase adds twenty-two radial bullets and four timed fans");
  Set(boss,"attackNumber",1);Call("ResolveManorCaptain",boss,state);Check((float)Get(boss,"chargeUntil")>F("clock")&&(int)Get(state,"bursts")==2,"Charge combines actual movement and delayed covering fire");
  Check(((string[])Get("AtlasNames"))[5]=="暮钟岛"&&((string)Call("LayerName",5,0)).StartsWith("暮钟岛"),"Atlas and layer heading agree on island name");
  ForestPose(1);foreach(object obj in List("obstacles")){Rect solid=(Rect)obj;Check(solid.width>0&&solid.height>0,"Forest collision has valid footprint");}
  var renderers=UnityEngine.Object.FindObjectsOfType<SpriteRenderer>().Where(x=>x.name=="Woodland natural cover").ToArray();Check(renderers.Length>0,"Forest test room contains real cover");
  foreach(var renderer in renderers){Bounds b=renderer.bounds;Rect visual=new Rect(b.min.x,b.min.y,b.size.x,b.size.y);bool small=false;foreach(Rect solid in List("obstacles"))if(visual.Contains(solid.center)&&solid.width<visual.width*.8f&&solid.height<visual.height*.75f)small=true;Check(small,"Cover collision excludes crown and image margins");}
 }
'''
s=s.replace(' static void Tick()',extra+' static void Tick()')
(o/'Smoke.cs').write_text(s,encoding='utf-8');(o/'runtime').mkdir(exist_ok=True)
qa=r.parent/'Validation/DesertV2/UnitySmokeProject'
old=qa/'Assets/NakiPrototype/Editor/WeaponRecoveryV5Smoke.cs'
if old.exists():old.rename(old.with_suffix('.cs.disabled'))
for src in (r/'Assets/NakiPrototype').rglob('*.cs'):
 dst=qa/'Assets/NakiPrototype'/src.relative_to(r/'Assets/NakiPrototype');shutil.copy2(src,dst)
shutil.copy2(o/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/ManorPressureV6Smoke.cs')
print('Prepared independent pressure regression')

