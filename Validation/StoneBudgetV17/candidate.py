from pathlib import Path
import shutil,sys
r=Path('D:/NakiLegend/Repository');out=r/'Validation/StoneBudgetV17';qa=r.parent/'Validation/DesertV2/UnitySmokeProject/Assets/NakiPrototype'
names=['CombatV3','OriginCombat','OriginLegacy','Starfall']
if len(sys.argv)>1 and sys.argv[1]=='restore':
 for name in names:shutil.copy2(out/'qa-backup'/('NakiPrototype.'+name+'.cs'),qa/('NakiPrototype.'+name+'.cs'))
 print('Restored QA runtime scripts to production baseline');raise SystemExit
for name in names:
 file='NakiPrototype.'+name+'.cs';backup=out/'qa-backup'/file;backup.parent.mkdir(parents=True,exist_ok=True)
 if not backup.exists():shutil.copy2(qa/file,backup)
 else:assert (qa/file).read_bytes()==backup.read_bytes(),'QA already modified'
def edit(name,old,new):
 p=qa/('NakiPrototype.'+name+'.cs');s=p.read_text(encoding='utf-8-sig');assert old in s,(name,old);p.write_text(s.replace(old,new),encoding='utf-8')
edit('CombatV3','MeteorBaseDamage*OriginTrainingScale*strength','MeteorBaseDamage*OriginTrainingScale*strength*.8f')
edit('OriginLegacy','return 1+.07f*(float)(Math.Log(1+(double)darkPower/25)/Math.Log(2));','return 1+Mathf.Min(.15f,.07f*(float)(Math.Log(1+(double)darkPower/25)/Math.Log(2)));')
edit('OriginCombat','new[]{.8f,4.5f,4.5f}','new[]{.8f,8f,18f}')
edit('OriginCombat','originMarkProcReady=clock+.45f','originMarkProcReady=clock+.8f')
edit('OriginCombat','OriginDamage(34)*DarkStrength','OriginDamage(18)*DarkStrength')
edit('Starfall','private sealed class FallingStar {','private sealed class FallingStar {public Dictionary<Enemy,int> castHits;')
edit('Starfall','OriginDamage(70)*DarkStrength','OriginDamage(55)*DarkStrength')
edit('Starfall','originThirdReady=clock+4.5f','originThirdReady=clock+18f')
edit('Starfall','   for(int i=0;i<3;i++){','   var castHits=new Dictionary<Enemy,int>();for(int i=0;i<3;i++){')
edit('Starfall','new FallingStar{at=at,','new FallingStar{castHits=castHits,at=at,')
edit('Starfall','{ApplySkillHit(e,s.damage,Element.Dark,"starfall");TriggerSoulSeal(e);}','{int repeat;s.castHits.TryGetValue(e,out repeat);s.castHits[e]=repeat+1;ApplySkillHit(e,s.damage*(repeat==0?1:repeat==1?.5f:.25f),Element.Dark,"starfall");TriggerSoulSeal(e);}')
runner=qa/'Editor/NakiStoneBudgetV17Smoke.cs';s=runner.read_text(encoding='utf-8');s=s.replace('StoneBudgetV17/runtime/','StoneBudgetV17/runtime-candidate/');runner.write_text(s,encoding='utf-8')
print('Candidate installed in isolated QA only; production and player saves untouched')
