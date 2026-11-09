from pathlib import Path
import json,shutil,uuid
r=Path('D:/NakiLegend/Repository');root=r/'Assets/NakiPrototype';out=r/'Validation/CombatV18';out.mkdir(parents=True,exist_ok=True)
manifest=out/'files.json';files=set(json.loads(manifest.read_text()) if manifest.exists() else [])
def edit(name,old,new):
 p=root/name;s=p.read_text(encoding='utf-8-sig')
 if old not in s:
  assert new in s,(name,old[:70]);return
 rel=str(p.relative_to(r)).replace('\\','/');bak=out/'backup-source'/rel;bak.parent.mkdir(parents=True,exist_ok=True)
 if not bak.exists():shutil.copy2(p,bak)
 p.write_text(s.replace(old,new),encoding='utf-8');files.add(rel);manifest.write_text(json.dumps(sorted(files)),encoding='utf-8')
# Resource growth and compatible save validation.
edit('NakiPrototype.OriginCombat.cs','LightRegenRate=4,LightRegenDelay=1.5f,LightBeamDrain=18','LightRegenRate=5,LightRegenDelay=1.5f,LightBeamDrain=16')
for name in ['NakiPrototype.OriginCombat.cs','NakiPrototype.Elements.cs']:
 p=root/name;s=p.read_text(encoding='utf-8-sig')
 for old in ['Mathf.Min(100,energy+','Mathf.Min(100, energy +']:
  if old in s:edit(name,old,old.replace('100','LightMaximum'))
for name in ['NakiPrototype.FireCoast.cs','NakiPrototype.IceSample.cs','NakiPrototype.ManorCourtyard.cs','NakiPrototype.Origin.cs','NakiPrototype.Thunder.cs','NakiPrototype.ThunderChapter.cs']:
 edit(name,'energy=origin==Origin.Light?100:0','energy=origin==Origin.Light?LightMaximum:0')
edit('NakiPrototype.Narrative.cs','if(origin==Origin.Light)energy=100','if(origin==Origin.Light)energy=LightMaximum')
for name in ['NakiPrototype.Checkpoints.cs','NakiPrototype.ThunderSave.cs']:
 edit(name,'FiniteRange(s.light,0,100)','FiniteRange(s.light,0,200)')
edit('NakiPrototype.ThunderChapterSave.cs','FiniteRange(p.light,0,100)','FiniteRange(p.light,0,200)')
edit('NakiPrototype.JourneyHud.cs','origin==Origin.Light?energy:0,ActivePowerColor)','origin==Origin.Light?energy:0,ActivePowerColor,LightMaximum)')
edit('NakiPrototype.JourneyHud.cs','ActivePowerLabel+(origin==Origin.Unchosen?"":" · "+MasteryCaption)','ActivePowerLabel+(origin==Origin.Light?" / "+LightMaximum.ToString("0"):"")+(origin==Origin.Unchosen?"":" · "+MasteryCaption)')
# Tested Stone candidate, now authorised for production.
edit('NakiPrototype.OriginLegacy.cs','return 1+.07f*(float)(Math.Log(1+(double)darkPower/25)/Math.Log(2));','return 1+Mathf.Min(.15f,.07f*(float)(Math.Log(1+(double)darkPower/25)/Math.Log(2)));')
edit('NakiPrototype.CombatV3.cs','MeteorBaseDamage*OriginTrainingScale*strength','MeteorBaseDamage*OriginTrainingScale*strength*.8f')
edit('NakiPrototype.OriginCombat.cs','new[]{.8f,4.5f,4.5f}','new[]{.8f,8f,18f}')
edit('NakiPrototype.OriginCombat.cs','originMarkProcReady=clock+.45f','originMarkProcReady=clock+.8f')
edit('NakiPrototype.OriginCombat.cs','OriginDamage(34)*DarkStrength','OriginDamage(18)*DarkStrength')
edit('NakiPrototype.Starfall.cs','private sealed class FallingStar {','private sealed class FallingStar {public Dictionary<Enemy,int> castHits;')
edit('NakiPrototype.Starfall.cs','OriginDamage(70)*DarkStrength','OriginDamage(55)*DarkStrength')
edit('NakiPrototype.Starfall.cs','originThirdReady=clock+4.5f','originThirdReady=clock+18f')
edit('NakiPrototype.Starfall.cs','   for(int i=0;i<3;i++){','   var castHits=new Dictionary<Enemy,int>();for(int i=0;i<3;i++){')
edit('NakiPrototype.Starfall.cs','new FallingStar{at=at,','new FallingStar{castHits=castHits,at=at,')
edit('NakiPrototype.Starfall.cs','{ApplySkillHit(e,s.damage,Element.Dark,"starfall");TriggerSoulSeal(e);}','{int repeat;s.castHits.TryGetValue(e,out repeat);s.castHits[e]=repeat+1;ApplySkillHit(e,s.damage*(repeat==0?1:repeat==1?.5f:.25f),Element.Dark,"starfall");TriggerSoulSeal(e);}')
edit('NakiPrototype.OriginLegacy.cs','三处大圈落点后坠星，范围与敌方祭师相同，可引爆蚀魂印','三处大圈坠星；同一目标重复命中为100%/50%/25%，可引爆蚀魂印')
edit('NakiPrototype.OriginLegacy.cs','每秒 18 光能','每秒 16 光能')
# Final HP is set once, after all theme-specific configuration.
edit('NakiPrototype.Enemies.cs','enemy.maximum=enemy.hp;enemies.Add(enemy);','enemy.maximum=enemy.hp;ApplyEncounterLife(enemy);enemies.Add(enemy);')
edit('NakiPrototype.Journey.cs','(node.stage==0?650:1050)*.65f','(node.stage==0?350:800)')
edit('NakiPrototype.Exploration.cs','e.summoned=spawn.summoned;','e.summoned=spawn.summoned;ApplySummonedLife(e);')
edit('NakiPrototype.Bestiary.cs','e.hp=e.maximum*=1.35f','e.hp=e.maximum=Mathf.Max(e.maximum,NormalLifeBudget*2.3f)')
edit('NakiPrototype.IceCombat.cs','if(!IsIceEnemy(e.kind)||e.iceArmor<=0)return amount;','if(!IsIceEnemy(e.kind)||e.iceArmor<=0||RecoveryWindow(e))return amount;')
edit('NakiPrototype.FireCombat.cs','if(!IsFireEnemy(e.kind)||e.iceArmor<=0)return amount;','if(!IsFireEnemy(e.kind)||e.iceArmor<=0||RecoveryWindow(e))return amount;')
edit('NakiPrototype.BossSignatures.cs','{SignatureState state;return bossSignatures.TryGetValue','{if(RecoveryWindow(e))return 1;SignatureState state;return bossSignatures.TryGetValue')
edit('NakiPrototype.EncounterDirector.cs','if(clock<e.tacticWardUntil)amount*=.65f;','if(clock<e.tacticWardUntil&&!RecoveryWindow(e))amount*=.65f;')
# Keep the approved captain's authored bullet behaviour; classify other shots once,
# after each emitter has assigned its actual speed and damage.
edit('NakiPrototype.cs','public bool hostile;','public bool hostile,hostileNormalized,hostileFast;public Enemy emitter;')
edit('NakiPrototype.cs','hostile = hostile, life = hostile ? 5 : 2.8f','hostile = hostile, emitter=attackEmitter, life = hostile ? 5 : 2.8f')
edit('NakiPrototype.cs','Shot shot = shots[index]; shot.life -= dt;','Shot shot = shots[index];NormalizeHostileShot(shot); shot.life -= dt;')
edit('NakiPrototype.cs','shot.hostile&&enableDesertJourney?HostileProjectileSpeedScale:1','shot.hostile&&enableDesertJourney&&!shot.hostileNormalized?HostileProjectileSpeedScale:1')
# Preserve the captain speed multiplier even though its other tuning is retained.
edit('NakiPrototype.cs','!shot.hostileNormalized?HostileProjectileSpeedScale:1','(!shot.hostileNormalized||shot.emitter!=null&&shot.emitter.kind==EnemyKind.ManorCaptain)?HostileProjectileSpeedScale:1')
edit('NakiPrototype.BossBreathing.cs','bool previous=bossAttackEmission;bossAttackEmission=e.boss;','bool previous=bossAttackEmission;Enemy previousEmitter=attackEmitter;attackEmitter=e;bossAttackEmission=e.boss;')
edit('NakiPrototype.BossBreathing.cs','finally{bossAttackEmission=previous;}','finally{bossAttackEmission=previous;attackEmitter=previousEmitter;}')
edit('NakiPrototype.BossBreathing.cs','next=clock+7.5f','next=clock+BossActiveSeconds(e)')
edit('NakiPrototype.BossBreathing.cs','state.next=state.until+7.5f','state.next=state.until+BossActiveSeconds(e)')
edit('NakiPrototype.EncounterDirector.cs','if(bossAttackEmission&&count>1){','LimitHostileFan(ref count,ref spacing,speed);if(bossAttackEmission&&attackEmitter!=null&&attackEmitter.kind==EnemyKind.ManorCaptain&&count>1){')
edit('NakiPrototype.CombatV3.cs','count=Mathf.CeilToInt(count*((CurrentIsland.ordinal==1?1.3f:1.65f)*(bossAttackEmission?1.2f:1)))','count=Mathf.Min(count,bossAttackEmission?20:5)')
# New helper's .meta and tracked deployment list.
new=root/'NakiPrototype.EncounterBalanceV18.cs';files.add(str(new.relative_to(r)).replace('\\','/'))
meta=Path(str(new)+'.meta')
if not meta.exists():meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\n',encoding='utf-8')
files.add(str(meta.relative_to(r)).replace('\\','/'));manifest.write_text(json.dumps(sorted(files)),encoding='utf-8')
print('Updated',len(files),'files; not deployed')
