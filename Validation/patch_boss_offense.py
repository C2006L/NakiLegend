from pathlib import Path
r=Path('D:/NakiLegend/Repository');a=r/'Assets/NakiPrototype'
p=a/'NakiPrototype.EncounterDirector.cs';s=p.read_text(encoding='utf-8-sig').replace('private void UpdateTacticalEnemy(Enemy e,float dt)','private void UpdateTacticalEnemyCore(Enemy e,float dt)');s=s.replace('int hostile=0;foreach(Shot s in shots)if(s.hostile)hostile++;','if(bossAttackEmission){int previous=count;count=Mathf.CeilToInt(count*1.2f);if(count>1)spacing*=Mathf.Max(0,previous-1)/(float)(count-1);}\n   int hostile=0;foreach(Shot s in shots)if(s.hostile)hostile++;');p.write_text(s,encoding='utf-8')
p=a/'NakiPrototype.BossBreathing.cs';s=p.read_text(encoding='utf-8-sig');s=s.replace('  private sealed class BossBreath', '''  private bool bossAttackEmission;
  private void UpdateTacticalEnemy(Enemy e,float dt)
  {
   float oldRecovery=e.recoveryUntil,oldNext=e.nextAttack;
   bool previous=bossAttackEmission;bossAttackEmission=e.boss;
   try{UpdateTacticalEnemyCore(e,dt);}finally{bossAttackEmission=previous;}
   if(!e.boss||BossResting)return;
   // Preserve windup tells and the full breathing window, compress only active-phase gaps.
   if(e.recoveryUntil>oldRecovery+.001f&&e.recoveryUntil>clock)e.recoveryUntil=clock+(e.recoveryUntil-clock)*.65f;
   if(e.nextAttack>oldNext+.001f&&e.nextAttack>clock){float tell=Mathf.Max(0,e.windup);e.nextAttack=clock+tell+Mathf.Max(.2f,(e.nextAttack-clock-tell)*.65f);}
  }
  private sealed class BossBreath''');p.write_text(s,encoding='utf-8')
p=a/'NakiPrototype.CombatV3.cs';s=p.read_text(encoding='utf-8-sig').replace('CurrentIsland.ordinal==1?1.3f:1.65f','(CurrentIsland.ordinal==1?1.3f:1.65f)*(bossAttackEmission?1.2f:1)');p.write_text(s,encoding='utf-8')
p=a/'NakiPrototype.ManorCombat.cs';s=p.read_text(encoding='utf-8-sig').replace('s.nextBurst=clock+.24f','s.nextBurst=clock+.2f').replace('s.bursts=2+e.difficultyPhase','s.bursts=3+e.difficultyPhase').replace('e.difficultyPhase>0?2:1','e.difficultyPhase>0?3:2');p.write_text(s,encoding='utf-8')
(r/'Validation/boss-offense-files.txt').write_text('\n'.join('Assets/NakiPrototype/NakiPrototype.'+n+'.cs' for n in ['EncounterDirector','BossBreathing','CombatV3','ManorCombat']),encoding='utf-8')
