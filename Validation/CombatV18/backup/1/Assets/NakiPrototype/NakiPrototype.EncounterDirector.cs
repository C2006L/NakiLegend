using System;
using System.Collections.Generic;
using UnityEngine;

namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private readonly Dictionary<Enemy,Dictionary<int,int>> bossVolleyHits=new Dictionary<Enemy,Dictionary<int,int>>();
  private float encounterWaveStartedAt,reinforcementNotice=-1;
  private bool EncounterTactics {get{return enableDesertJourney&&journeyRooms.Count>currentRoom&&currentRoom>=0&&!gatehouseSampleMode&&!lightningTrialMode&&(IsCombatRoom(journeyRooms[currentRoom])||challengeActive);}}
  private bool LearningEncounter {get{return CurrentIsland.ordinal==1&&clearedCombatRooms<3;}}
  private int EncounterEnemyLimit {get{return LearningEncounter?5:CurrentIsland.ordinal<=2?7:8;}}
  private bool CombatFixture(Enemy e){return e.kind==EnemyKind.FrostLens||e.kind==EnemyKind.ForgeMold||e.kind==EnemyKind.WoodParasite;}
  private int ActiveCombatants {get{int n=0;foreach(Enemy e in enemies)if(e.hp>0&&!CombatFixture(e))n++;return n;}}

  private void ConfigureTacticalEnemy(Enemy e)
  {
   if(!EncounterTactics||CombatFixture(e))return;
   if(!e.boss&&!LearningEncounter)e.speed*=1.1f;
   e.nextAttack=clock+1.2f+(enemies.Count%4)*.17f;if(!e.boss){IntentFor(e);if(NaturalIntent(e)==EnemyIntent.Anchor)e.tacticRole=5;}
  }
  private void AssignTacticRole(Enemy e,int role)
  {
   if(e.boss||CombatFixture(e))return;e.tacticRole=role;IntentRoleChanged(e);
   if(role==1)e.speed*=1.12f;
   if(role==5){e.speed*=.85f;e.attackDirection=(position-e.position).normalized;}
   if(role==3){e.hp*=.8f;e.maximum=e.hp;}
   if(role>=3){e.tacticSigil=Draw(role==3?"Protective caster emblem":role==5?"Frontal shield emblem":"Sniper preparation emblem",windRingSprite??disc,e.position,Vector2.one*.7f,role==3?C("81dec1"):C("edb580"),-680);}
  }
  private EnemyKind TacticKind(int role)
  {
   if(manorCourtyardMode)return role==1?EnemyKind.ManorHound:role==3?EnemyKind.ManorClerk:role==4?EnemyKind.ManorArcher:role==5?EnemyKind.ManorShield:EnemyKind.ManorPatrol;
   if(woodlandMode)return role==1?WoodKinds[1]:role==3?WoodKinds[8]:WoodKinds[2];
   if(fireChapterMode)return role==1?EnemyKind.AshRaider:role==3?EnemyKind.EmberAcolyte:EnemyKind.EmberLizard;
   if(iceSampleMode)return role==1?EnemyKind.FrostHound:role==3?EnemyKind.IceRunecaster:EnemyKind.IceArcher;
   if(ThunderActive)return role==1?EnemyKind.StormGuard:EnemyKind.ChargeMoth;
   return role==1?EnemyKind.SandScorpion:EnemyKind.SandKite;
  }
  private void QueueTacticalCompanions(RoomNode room)
  {
   if(manorCourtyardMode||challengeActive||!EncounterTactics)return;
   var rng=new System.Random(activeSeed^currentRoom*41^currentWave*109);
   int count=TacticalCompanionCount;
   int formation=(currentRoom+currentWave+room.stage)%3;
   for(int i=0;i<count;i++){
    int role=LearningEncounter||i>=2?1:i==0?(formation==0?1:formation==1?3:4):formation==1?1:2;
    QueueEnemy(TacticKind(role),FindJourneySpawn(rng,.5f));
    var spawn=pendingSpawns[pendingSpawns.Count-1];spawn.role=role;spawn.when=clock+1.4f+i*.5f;
   }
   if(!LearningEncounter&&pendingSpawns.Count>0){int role=formation==0?5:1;var first=pendingSpawns[0];if(first.role==0)first.role=role;}
  }
  private bool CanReleaseReinforcement(PendingSpawn spawn)
  {
   if(!EncounterTactics)return true;
   if(ActiveCombatants>=EncounterEnemyLimit)return false;
   if(Vector2.Distance(position,spawn.at)<1.7f){
    spawn.when=clock+.65f;
    Vector2 next=FindJourneySpawn(new System.Random(activeSeed+currentRoom*73+(int)(clock*11)),.55f);
    spawn.at=next;if(spawn.mark!=null)spawn.mark.transform.position=next;return false;
   }
   return true;
  }
  private void UpdateTacticalEnemyCore(Enemy e,float dt)
  {
   if(TickBossBreathing(e,dt))return;
   if(e.kind==EnemyKind.ManorCaptain||e.kind==EnemyKind.ManorSeal||e.kind==EnemyKind.ManorKnight){TickManorCaptain(e,dt);return;}
   if(!EncounterTactics||CombatFixture(e)){UpdateJourneyEnemy(e,dt);return;}
   if(TickEnemyIntent(e,dt))return;
   if(e.boss){UpdateBossPressurePhase(e);if(TickBossSignature(e,dt))return;
    if(!EnemyOnScreen(e.position,.25f)&&e.windup<=0&&e.chargeUntil<=clock){e.nextAttack=Mathf.Max(e.nextAttack,clock+.4f);if(!RootedBoss(e))MoveIcePursuit(e,Mathf.Max(e.speed,1.5f)*dt);UpdateJourneyEnemyPose(e,true);return;}
   }
   if(clock<e.tacticControlUntil){e.windup=0;e.chargeUntil=0;SyncTacticalPose(e);return;}
   if(e.tacticSigil!=null){e.tacticSigil.transform.position=e.position;e.tacticSigil.GetComponent<SpriteRenderer>().color=e.tacticRole==3?C("81dec1"):C("edb580");}
   if(e.tacticRole==3||e.tacticRole==4){UpdateTacticalSpecialist(e,dt);return;}
   PrepareBossFootwork(e);
   Vector2 previousBossPosition=e.position;
   float ready=e.nextAttack,windup=e.windup;
   if(e.windup<=0&&e.chargeUntil<=clock&&clock>=e.nextAttack){
    int winding=0;foreach(Enemy other in enemies)if(other!=e&&(other.windup>0||other.tacticCastUntil>clock))winding++;
    if(winding>=(LearningEncounter?2:3))e.nextAttack=clock+.18f;
    else if(e.boss)ChoosePressureMove(e);
   }
   UpdateJourneyEnemy(e,dt);
   FinishBossFootwork(e,previousBossPosition,dt);
   if(e.nextAttack>ready&&e.nextAttack>clock){float tell=Mathf.Max(0,e.windup);float pace=LearningEncounter?.96f:e.boss?(e.difficultyPhase>=2?.76f:.86f):.85f;if(e.boss&&CurrentIsland.ordinal==1)pace*=.9f;e.nextAttack=clock+tell+Mathf.Max(.35f,(e.nextAttack-clock-tell)*pace*EncounterRecoveryScale);}
   if(e.boss&&e.windup>0&&windup<=0){int move=(UsesPreviousBossCounter(e)?e.attackNumber-1:e.attackNumber)%3;if(e.lastTacticMove==move)e.repeatedTacticMove++;else{e.lastTacticMove=move;e.repeatedTacticMove=1;}}
   if(e.boss&&windup>0&&windup<=dt+.001f&&e.windup<=0&&e.hp>0&&e.difficultyPhase>0&&clock>=e.tacticCooldown){e.tacticCooldown=clock+BossCounterInterval;}
   if(!e.boss&&(e.tacticRole==1||e.tacticRole==2)&&e.windup<=0&&e.chargeUntil<=clock&&clock>=e.recoveryUntil){
    Vector2 to=position-e.position;if(to.magnitude>2&&to.magnitude<8){Vector2 side=new Vector2(-to.y,to.x).normalized*((currentRoom+(int)e.kind)%2==0?1:-1);e.position=MoveEnemyWithinRoom(e.position,side*e.speed*.35f*dt,e.radius);SyncTacticalPose(e);}
   }
  }
  private void SyncTacticalPose(Enemy e)
  {if(e.root!=null)e.root.transform.position=e.position;if(e.shadow!=null)e.shadow.transform.position=e.position;if(e.body!=null){e.body.sortingOrder=DepthAt(e.position.y);e.body.flipX=position.x<e.position.x;}}
  private bool UsesPreviousBossCounter(Enemy e){return (int)e.kind<8||!woodlandMode&&e.kind>=EnemyKind.CopperWarden&&e.kind<=EnemyKind.StormStag;}
  private void ChoosePressureMove(Enemy e)
  {
   float d=Vector2.Distance(position,e.position);int desired=!woodlandMode&&e.kind==EnemyKind.CopperWarden?(d>4?0:d<2.7f?2:1):d<2.7f?1:d>6?2:(e.attackNumber+1)%3;
   if(desired==e.lastTacticMove&&e.repeatedTacticMove>=2)desired=(desired+1)%3;
   // Desert patterns use (attackNumber-1)%3; later islands use attackNumber%3.
   e.attackNumber=desired+(UsesPreviousBossCounter(e)?0:2);
  }
  private void UpdateTacticalSpecialist(Enemy e,float dt)
  {
   Vector2 to=position-e.position;
   if(e.tacticCastUntil>0){
    if(clock<e.tacticCastUntil){SyncTacticalPose(e);return;}
    e.tacticCastUntil=0;if(e.warning!=null)Destroy(e.warning);
    if(e.tacticRole==3){bool helped=false;foreach(Enemy ally in enemies)if(ally!=e&&!ally.boss&&ally.hp>0&&!CombatFixture(ally)&&Vector2.Distance(ally.position,e.position)<4.5f&&!ProjectilePathBlocked(e.position,ally.position,.05f)){ally.tacticWardUntil=clock+2.4f;ally.hp=Mathf.Min(ally.maximum,ally.hp+ally.maximum*.035f);SpellSpark(ally.position,C("8fe4bd"),.45f);helped=true;}
     if(!helped)PressureFan(e.position,e.attackDirection,3,28,4.2f,e.attackElement,1.05f);
    }else PressureFan(e.position+e.attackDirection*.5f,e.attackDirection,3,8,6.2f,e.attackElement,1.4f);
    e.nextAttack=clock+(e.tacticRole==3?3.4f:1.7f)*EncounterRecoveryScale;e.recoveryUntil=clock+.45f;SyncTacticalPose(e);return;
   }
   bool line=!ProjectilePathBlocked(e.position,position,.09f);
   if(clock>=e.nextAttack&&line&&to.magnitude<11){
    int casting=0;foreach(Enemy other in enemies)if(other!=e&&(other.windup>0||other.tacticCastUntil>clock))casting++;
    if(casting>=3){e.nextAttack=clock+.2f;return;}
    e.attackDirection=to.sqrMagnitude>.01f?to.normalized:Vector2.down;e.tacticCastUntil=clock+(e.tacticRole==3?.9f:.85f);
    e.warning=e.tacticRole==3?WarningCircle(e.position,1):ChapterLineWarning(e.position,e.attackDirection,11,.2f);SyncTacticalPose(e);return;
   }
   if(!line||to.magnitude>6||to.magnitude<3){Vector2 direction=to.normalized*(line&&to.magnitude<3?-1:1);Vector2 next=MoveEnemyWithinRoom(e.position,direction*e.speed*dt,e.radius);if((next-e.position).sqrMagnitude<.00001f)next=MoveEnemyWithinRoom(e.position,new Vector2(-direction.y,direction.x)*e.speed*dt,e.radius);e.position=next;}
   SyncTacticalPose(e);
  }
  private float AdjustTacticalDamage(Enemy e,float amount)
  {
   if(!EncounterTactics)return amount;amount*=SignatureDamageMultiplier(e);
   if(clock<e.tacticWardUntil)amount*=.65f;
   if(e.tacticCastUntil>clock&&amount>0){e.tacticCastUntil=0;e.nextAttack=clock+.8f;if(e.warning!=null)Destroy(e.warning);}
   return amount;
  }
  private float TacticalProjectileDamage(Shot shot,Enemy e)
  {
   float damage=shot.damage;if(!EncounterTactics)return damage;
   if(e.tacticRole==5&&Vector2.Dot(-shot.direction,e.attackDirection)>.35f){float factor=shot.hitsLeft>1||shot.element==Element.Wind?.8f:.4f;damage*=factor;if(clock>=e.affinityFeedbackAt){e.affinityFeedbackAt=clock+1.2f;AddFloatingHit("盾阵 · 绕侧或近战",e.position+Vector2.up,C("e4d49b"));}}
   if(e.boss&&shot.meteorGroup>0){Dictionary<int,int> groups;if(!bossVolleyHits.TryGetValue(e,out groups)){groups=new Dictionary<int,int>();bossVolleyHits[e]=groups;}
    int hits;groups.TryGetValue(shot.meteorGroup,out hits);if(groups.Count>32&&!groups.ContainsKey(shot.meteorGroup))groups.Clear();groups[shot.meteorGroup]=hits+1;
    damage*=hits==0?1:hits==1?.8f:hits==2?.65f:.5f;
   }
   return damage;
  }
  private void UpdateBossPressurePhase(Enemy e)
  {
   int phase=e.hp<=e.maximum*.35f?2:e.hp<=e.maximum*.7f?1:0;if(phase<=e.difficultyPhase)return;e.difficultyPhase=phase;
   if(phase==2){AddFloatingHit("决战 · 交错攻势",e.position+Vector2.up*1.6f,C("ffd59c"));}
   if(CurrentIsland.ordinal>=2&&ActiveCombatants<EncounterEnemyLimit-1){
    QueueEnemy(TacticKind(phase==1?4:1),FindJourneySpawn(new System.Random(activeSeed+currentRoom*17+phase),.5f));var spawn=pendingSpawns[pendingSpawns.Count-1];spawn.role=phase==1?4:1;spawn.summoned=true;spawn.when=clock+1.5f;
    if(!woodlandMode&&e.kind==EnemyKind.CopperWarden&&ActiveCombatants+pendingSpawns.Count<EncounterEnemyLimit){QueueEnemy(TacticKind(1),FindJourneySpawn(new System.Random(activeSeed+phase*97),.5f));var escort=pendingSpawns[pendingSpawns.Count-1];escort.role=1;escort.summoned=true;escort.when=clock+2.1f;}
   }
  }
  private void QueueBossPressureVolley(Enemy e)
  {
   SignatureState state;if(!bossSignatures.TryGetValue(e,out state)){state=new SignatureState();bossSignatures[e]=state;}
   if(clock>=state.next)StartBossSignature(e,state);
  }
  private void PressureFan(Vector2 at,Vector2 aim,int count,float spacing,float speed,Element element,float damage)
  {
   if(bossAttackEmission&&count>1){int previous=count;count=Mathf.CeilToInt(count*1.2f);if(count>1)spacing*=Mathf.Max(0,previous-1)/(float)(count-1);}
   int hostile=0;foreach(Shot s in shots)if(s.hostile)hostile++;
   for(int i=0;i<count&&hostile<180;i++,hostile++){
    Vector2 dir=Quaternion.Euler(0,0,(i-(count-1)*.5f)*spacing)*aim;int before=shots.Count;SpawnShot(at,dir,true,null,at);if(shots.Count==before)continue;Shot s=shots[shots.Count-1];s.element=element;s.damage=damage;s.speed=speed;s.life=3;s.root.GetComponent<SpriteRenderer>().color=C("ffd39c");
   }
  }
  private void TickEncounterPressure(float dt)
  {
   TickBreakableHazards();TickIntentBombs();UpdateIntentPlayerMotion();TickBossSignatures(dt);
   if(!EncounterTactics||LearningEncounter||challengeActive)return;
   RoomNode n=journeyRooms[currentRoom];if(n.kind!=RoomKind.Combat&&n.kind!=RoomKind.Elite||n.cleared||currentWave>=n.waves||pendingSpawns.Count>0)return;
   int left=ActiveCombatants;
   if(left>0&&(left<=2&&clock-encounterWaveStartedAt>=7||clock-encounterWaveStartedAt>=18)&&left<EncounterEnemyLimit-3){
    if(reinforcementNotice<0){reinforcementNotice=clock+1.25f;AddFloatingHit("远处传来援军脚步",position+Vector2.up*1.3f,C("eec48c"));}
    else if(clock>=reinforcementNotice){reinforcementNotice=-1;SpawnJourneyWave();}
   }else reinforcementNotice=-1;
  }
  private void ResetEncounterDirector()
  {
   bossVolleyHits.Clear();bossFootwork.Clear();reinforcementNotice=-1;encounterWaveStartedAt=clock;
   bossBreaths.Clear();ClearBossSignatures();ResetEnemyIntents();ClearBreakableState();challengeActive=challengeMenu=false;challengeRound=0;challengeAltar=null;encounterRecord=null;
  }
 }
}
