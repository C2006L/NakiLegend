from pathlib import Path
r=Path('D:/NakiLegend/Repository');a=r/'Assets/NakiPrototype';p=a/'NakiPrototype.BossBreathing.cs';s=p.read_text(encoding='utf-8-sig');start=s.index('  private void TickBossBreathing(') if '  private void TickBossBreathing(' in s else s.index('  private bool TickBossBreathing(')
s=s[:start]+'''  private bool BossAttackFinishing(Enemy e)
  {
   if(e.windup>0||e.chargeUntil>clock||e.phaseUntil>clock)return true;
   SignatureState signature;if(bossSignatures.TryGetValue(e,out signature)&&signature.hold>clock)return true;
   ManorCaptainState captain;if(manorCaptains.TryGetValue(e,out captain)&&(captain.bursts>0||captain.shieldUntil>clock))return true;
   foreach(var burst in coreBursts)if(burst.owner==e)return true;
   foreach(var volley in woodVolleys)if(volley.owner==e)return true;
   return false;
  }
  private bool TickBossBreathing(Enemy e,float dt)
  {
   if(!e.boss)return false;
   BossBreath state;if(!bossBreaths.TryGetValue(e,out state)){state=new BossBreath{next=clock+7.5f};bossBreaths[e]=state;}
   if(clock>=state.until){
    if(clock<state.next)return false;
    // Stop starting new moves, but let the current tell, charge and queued volley finish.
    e.nextAttack=Mathf.Max(e.nextAttack,clock+.4f);
    SignatureState signature;if(bossSignatures.TryGetValue(e,out signature))signature.next=Mathf.Max(signature.next,clock+.4f);
    if(BossAttackFinishing(e))return false;
    state.until=clock+3.5f;state.next=state.until+7.5f;
    AddFloatingHit("收招 · 调整步伐",e.position+Vector2.up,C("f4dfa1"));
   }
   Vector2 before=e.position;
   if(!RootedBoss(e)&&clock>=e.tacticControlUntil&&dt>0){Vector2 to=position-e.position;Vector2 aim=to.sqrMagnitude>.01f?to.normalized:Vector2.down;Vector2 step=to.magnitude<2.6f?-aim:new Vector2(-aim.y,aim.x)*((e.attackNumber&1)==0?1:-1);e.position=BossStep(e,step*.85f*dt);e.attackDirection=aim;}
   e.velocity=dt>0?(e.position-before)/dt:Vector2.zero;e.tacticRole=0;
   e.nextAttack=Mathf.Max(e.nextAttack,state.until+.2f);e.recoveryUntil=Mathf.Max(e.recoveryUntil,clock+.05f);UpdateJourneyEnemyPose(e,e.velocity.sqrMagnitude>.00001f);return true;
  }
 }
}
''';p.write_text(s,encoding='utf-8')
p=a/'NakiPrototype.cs';s=p.read_text(encoding='utf-8-sig').replace('            if(hostile&&BossResting)return;\n','');p.write_text(s,encoding='utf-8')
p=a/'NakiPrototype.Elements.cs';s=p.read_text(encoding='utf-8-sig').replace('if (BossResting || clock < invulnerableUntil','if (clock < invulnerableUntil');p.write_text(s,encoding='utf-8')
(r/'Validation/natural-recovery-files.txt').write_text('\n'.join('Assets/NakiPrototype/'+n for n in ['NakiPrototype.BossBreathing.cs','NakiPrototype.cs','NakiPrototype.Elements.cs']),encoding='utf-8')
