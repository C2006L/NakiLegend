from pathlib import Path
r=Path('D:/NakiLegend/Repository');a=r/'Assets/NakiPrototype'
p=a/'NakiPrototype.BossBreathing.cs';s=p.read_text(encoding='utf-8-sig').replace('float oldRecovery=e.recoveryUntil,oldNext=e.nextAttack;','Vector2 before=e.position;float oldRecovery=e.recoveryUntil,oldNext=e.nextAttack;');s=s.replace('   // Preserve windup tells','''   if(!RootedBoss(e)&&e.windup<=0&&e.chargeUntil<=clock&&clock>=e.phaseUntil&&dt>0&&(e.position-before).sqrMagnitude<dt*dt*.25f){
    Vector2 to=position-e.position;Vector2 aim=to.sqrMagnitude>.01f?to.normalized:Vector2.down;
    Vector2 step=to.magnitude>5?aim:to.magnitude<2.3f?-aim:new Vector2(-aim.y,aim.x)*((e.attackNumber&1)==0?1:-1);
    float speed=1.6f+e.difficultyPhase*.2f;Vector2 next=BossStep(e,step*speed*dt);
    if((next-e.position).sqrMagnitude<.00001f)next=BossStep(e,new Vector2(-step.y,step.x)*speed*dt);
    e.position=next;e.velocity=(e.position-before)/dt;UpdateJourneyEnemyPose(e,e.velocity.sqrMagnitude>.00001f);
   }
   // Preserve windup tells''');p.write_text(s,encoding='utf-8')
p=a/'NakiPrototype.BossSignatures.cs';s=p.read_text(encoding='utf-8-sig').replace('next=clock+5+BossIdentity(e)*.12f','next=clock+1.6f+BossIdentity(e)*.03f').replace('e.difficultyPhase>=2?7.5f:9.5f','e.difficultyPhase>=2?3.7f:4.2f');p.write_text(s,encoding='utf-8')
p=a/'NakiPrototype.EncounterRefresh.cs';s=p.read_text(encoding='utf-8-sig').replace('if(room.kind==RoomKind.Boss){AddBossFightCover','if(room.kind==RoomKind.Boss||room.kind==RoomKind.Guardian){AddBossFightCover');p.write_text(s,encoding='utf-8')
f=r/'Validation/boss-offense-files.txt';f.write_text(f.read_text(encoding='utf-8')+'\nAssets/NakiPrototype/NakiPrototype.BossSignatures.cs',encoding='utf-8')
p=r/'Validation/prepare_boss_offense.py';s=p.read_text(encoding='utf-8-sig').replace("p.with_suffix('.cs.v9-cover-disabled')","p.with_suffix('.cs.v9-movement-disabled')");s=s.replace('Check((int)Get("additionalCoverCount")==1,','Check((int)Get("additionalCoverCount")==1,');p.write_text(s,encoding='utf-8')
