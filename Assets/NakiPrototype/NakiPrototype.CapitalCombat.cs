using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private sealed class CapitalFight {public int salvo,beat,move,phase=-1;public float next,shield,nodesReady,royalAt,trapAfterBeamAt;public int royalCombo;public bool contact,royalWindup;}
  private readonly Dictionary<Enemy,CapitalFight> capitalFights=new Dictionary<Enemy,CapitalFight>();
  private static bool IsCapitalEnemy(EnemyKind k){return k>=EnemyKind.CapitalHalberd&&k<=EnemyKind.CapitalConduit;}
  private static bool IsCapitalBoss(EnemyKind k){return k>=EnemyKind.CapitalCommander&&k<=EnemyKind.CapitalKing;}
  private EnemyIntent CapitalIntent(EnemyKind k)
  {return k==EnemyKind.CapitalShield?EnemyIntent.Anchor:k==EnemyKind.CapitalCrossbow||k==EnemyKind.CapitalArcher?EnemyIntent.Skirmish:k==EnemyKind.CapitalAlchemist?EnemyIntent.Artillery:k==EnemyKind.CapitalMage?EnemyIntent.Roam:k==EnemyKind.CapitalMaintainer?EnemyIntent.Support:k==EnemyKind.CapitalDuelist?EnemyIntent.Intercept:EnemyIntent.Pursue;}
  private void ConfigureCapitalEnemy(Enemy e)
  {
   e.boss=IsCapitalBoss(e.kind);e.radius=e.boss?.7f:e.kind==EnemyKind.CapitalConduit?.4f:.32f;e.speed=e.kind==EnemyKind.CapitalDuelist?2.8f:e.boss?2.15f:e.kind==EnemyKind.CapitalShield?1.35f:1.9f;
   e.hp=e.maximum=e.kind==EnemyKind.CapitalConduit?80:e.boss?new[]{4100f,4800,5500}[(int)e.kind-(int)EnemyKind.CapitalCommander]:NormalLifeBudget*(e.kind==EnemyKind.CapitalShield?1.65f:e.kind==EnemyKind.CapitalMage||e.kind==EnemyKind.CapitalMaintainer?.8f:1);
   e.affinity=e.attackElement=e.kind==EnemyKind.CapitalMage||e.kind==EnemyKind.CapitalKing?Element.Light:Element.Physical;e.soulEligible=!e.boss&&e.kind!=EnemyKind.CapitalConduit;
   if(e.kind==EnemyKind.CapitalShield)e.tacticRole=5;
  }
  private void SpawnCapitalWave(RoomNode n)
  {
   if(n.kind==RoomKind.Boss){SpawnJourneyEnemy((EnemyKind)((int)EnemyKind.CapitalCommander+n.stage),Vector2.zero,n.depth);return;}
   var rng=new System.Random(activeSeed^n.id*433^currentWave*71);int count=n.kind==RoomKind.Elite?6:5;
   EnemyKind[] roster=n.stage==0?new[]{EnemyKind.CapitalHalberd,EnemyKind.CapitalCrossbow,EnemyKind.CapitalShield,EnemyKind.CapitalArcher,EnemyKind.CapitalDuelist,EnemyKind.CapitalAlchemist}:n.stage==1?new[]{EnemyKind.CapitalHalberd,EnemyKind.CapitalShield,EnemyKind.CapitalArcher,EnemyKind.CapitalMage,EnemyKind.CapitalDuelist,EnemyKind.CapitalAlchemist}:new[]{EnemyKind.CapitalShield,EnemyKind.CapitalArcher,EnemyKind.CapitalMage,EnemyKind.CapitalDuelist,EnemyKind.CapitalMaintainer,EnemyKind.CapitalHalberd};
   for(int i=0;i<count;i++){int slot=currentWave%2==0?(i+2)%count:i;QueueEnemy(roster[slot],FindJourneySpawn(rng,.4f));pendingSpawns[pendingSpawns.Count-1].when=clock+1.2f+i*.27f;}
  }
  private CapitalFight CapitalState(Enemy e){CapitalFight s;if(!capitalFights.TryGetValue(e,out s)){s=new CapitalFight();capitalFights[e]=s;}return s;}
  private void TickCapitalEnemy(Enemy e,float dt)
  {
   if(e.kind==EnemyKind.CapitalConduit){TickCapitalConduit(e);return;}
   CapitalFight s=CapitalState(e);Vector2 old=e.position;Vector2 to=position-e.position;
   if(e.boss){e.difficultyPhase=e.hp<e.maximum*.35f?2:e.hp<e.maximum*.7f?1:0;if(s.phase<e.difficultyPhase){s.phase=e.difficultyPhase;if(e.kind==EnemyKind.CapitalKing&&s.phase==2){AddFloatingHit("王印崩解 · 国王亲征",e.position+Vector2.up*2,C("f7d58c"));e.speed=3.2f;}}}
   if(e.kind==EnemyKind.CapitalKing)e.speed=2.7f+e.difficultyPhase*.25f;
   if(clock<e.tacticControlUntil){e.windup=0;s.royalCombo=0;s.royalWindup=false;UpdateJourneyEnemyPose(e,false);return;}
   if(s.trapAfterBeamAt>0&&clock>=s.trapAfterBeamAt&&!CapitalBeamActive(e)){s.trapAfterBeamAt=0;QueueCapitalTrapPair(e,true);}
   if(s.salvo>0&&clock>=s.next){s.salvo--;s.beat++;s.next=clock+.58f;CapitalVolley(e,s);}
   if(TickRoyalCombo(e,s,dt))return;
   if(e.kind==EnemyKind.CapitalKing&&s.royalCombo>0&&e.chargeUntil<=clock){UpdateJourneyEnemyPose(e,false);return;}
   if(e.chargeUntil>clock){e.position=MoveEnemyCharge(e,e.attackDirection*(e.boss?7:6)*dt);if(!s.contact&&DistanceToSegment(position,old,e.position)<e.radius+.28f){s.contact=true;TakeJourneyDamage(RoyalAttackDamage(e,e.boss?1.4f:1.1f,2.2f),Element.Physical);}if((e.position-old).sqrMagnitude<.000001f&&dt>0)e.chargeUntil=clock;e.recoveryUntil=clock+.5f;}
   else if(e.windup>0){e.windup=Mathf.Max(0,e.windup-dt);if(e.windup<=0){if(e.warning!=null)Destroy(e.warning);e.warning=null;ResolveCapitalAttack(e,s);}}
   else if(RoyalRangedActive(e,s)){if(!CapitalBeamActive(e))CapitalFootwork(e,s,dt,to);}
   else if(clock>=e.recoveryUntil){
    CapitalFootwork(e,s,dt,to);
    float range=e.boss?12:e.kind==EnemyKind.CapitalHalberd?3.6f:e.kind==EnemyKind.CapitalShield?2.7f:e.kind==EnemyKind.CapitalDuelist?4.6f:10;
    if(clock>=e.nextAttack&&to.magnitude<range&&EnemyOnScreen(e.position,.4f)&&!ProjectilePathBlocked(e.position,position,.08f)){
     e.attackNumber++;s.move=e.boss?CapitalMoveCycle(e):e.attackNumber%2;if(e.kind==EnemyKind.CapitalCommander&&to.magnitude<2.7f&&s.move==0)s.move=3;e.attackDirection=(IntentLead(.18f)-e.position).normalized;if(e.attackDirection.sqrMagnitude<.01f)e.attackDirection=Vector2.down;e.strikePoint=NearestEnemyFooting(IntentLead(.3f),.4f);e.actionStarted=clock;
     bool charge=e.boss?s.move==0:e.kind==EnemyKind.CapitalDuelist||e.kind==EnemyKind.CapitalHalberd&&s.move==0;
     e.windup=charge?.7f:e.kind==EnemyKind.CapitalAlchemist?1:.55f;
     if(charge)e.warning=ChapterLineWarning(e.position,e.attackDirection,e.boss?5:3.6f,e.boss?1.1f:.65f);
     else if(e.kind==EnemyKind.CapitalCommander&&s.move==3)e.warning=WarningSector(e.position,e.attackDirection,2.8f);
     else if(e.kind==EnemyKind.CapitalShield||e.kind==EnemyKind.CapitalHalberd)e.warning=WarningSector(e.position,e.attackDirection,2.5f);
     if(e.boss)AddFloatingHit(CapitalMoveName(e,s.move),e.position+Vector2.up*2,C("f4d58e"));
     e.nextAttack=clock+e.windup+(e.boss?1.6f:e.kind==EnemyKind.CapitalMaintainer?5:e.kind==EnemyKind.CapitalAlchemist?3.4f:1.8f);
    }
   }
   e.tacticRole=clock<s.shield?5:e.kind==EnemyKind.CapitalShield?5:0;e.velocity=dt>0?(e.position-old)/dt:Vector2.zero;UpdateJourneyEnemyPose(e,e.velocity.sqrMagnitude>.00001f);
   // A complete actor silhouette is retained; animate the committed weapon motion as a whole.
   e.root.transform.localRotation=Quaternion.identity;
  }
  private string CapitalMoveName(Enemy e,int move)
  {
   if(e.kind==EnemyKind.CapitalCommander)return new[]{"盾列冲锋","封港弩阵","交错警戒","盾击横扫 · 绕侧反击"}[move];
   if(e.kind==EnemyKind.CapitalMagistrate)return new[]{"执律突进","分隔敕令 · 穿行光束间隙","错时落印 · 注意两侧落点","驱逐敕令 · 慢环与定点弹"}[move];
   return new[]{e.difficultyPhase==2?"亲征 · 王剑追击":"敕令 · 近卫突进","贡纳 · 击破供能节点","裁决 · 借石柱挡光","敕令 · 交错弹幕"}[move];
  }
  private void CapitalFootwork(Enemy e,CapitalFight s,float dt,Vector2 to)
  {
   float distance=to.magnitude;Vector2 aim=distance>.01f?to.normalized:Vector2.down,side=new Vector2(-aim.y,aim.x);Vector2 dir=aim;
   EnemyIntent role=CapitalIntent(e.kind);
   if(e.boss)dir=clock<s.shield||e.kind==EnemyKind.CapitalKing&&e.difficultyPhase==2?aim:distance>4.5f?aim:distance<2.5f?-aim:side*((e.attackNumber&1)==0?1:-1);
   else if(role==EnemyIntent.Skirmish||role==EnemyIntent.Artillery||role==EnemyIntent.Support)dir=distance<4?-aim:distance>7?aim:side*((e.attackNumber&1)==0?1:-1);
   else if(role==EnemyIntent.Roam)dir=(aim*.25f+new Vector2(Mathf.Sin(clock*.8f+(int)e.kind),Mathf.Cos(clock*1.2f+(int)e.kind))).normalized;
   else if(role==EnemyIntent.Anchor)dir=distance>3?aim:side*.3f;
   else if(e.kind==EnemyKind.CapitalDuelist)dir=(aim+side*Mathf.Sin(clock*.9f)*.8f).normalized;
   if(e.kind==EnemyKind.CapitalCommander)dir=distance>2.4f?aim:side*.45f;
   if(e.kind==EnemyKind.CapitalKing)dir=distance>2.5f?(IntentLead(.4f)-e.position).normalized:side;
   Vector2 step=dir*e.speed*dt,previous=e.position;e.position=MoveEnemyWithinRoom(e.position,step,e.radius);if(dt>0&&(e.position-previous).sqrMagnitude<.00001f)e.position=MoveEnemyWithinRoom(e.position,side*e.speed*dt,e.radius);
  }
  private void ResolveCapitalAttack(Enemy e,CapitalFight s)
  {
   e.actionStarted=clock;e.recoveryUntil=clock+.65f;
   if(e.boss){
    CapitalControlGround(e,s.move);
    if(s.move==0){QueueManorImpact(e,NearestEnemyFooting(e.position+e.attackDirection*3.7f,.5f),1.6f,RoyalAttackDamage(e,1.15f,2.2f),.85f,false);s.contact=false;e.chargeUntil=clock+(e.kind==EnemyKind.CapitalKing&&e.difficultyPhase==2?.85f:.6f);if(e.kind==EnemyKind.CapitalKing){s.royalCombo=e.difficultyPhase==2?3:2;s.royalAt=e.chargeUntil;}return;}
    if(e.kind==EnemyKind.CapitalCommander){if(s.move==3){s.shield=clock+1.2f;CapitalShieldSweep(e);return;}if(s.move==1){CapitalNodes(e,2);QueueCapitalTrapPair(e,false);}s.salvo=s.move==2?5:4;s.beat=0;s.next=clock;return;}
    if(e.kind==EnemyKind.CapitalMagistrate){CapitalNodes(e,2);Vector2 side=new Vector2(-e.attackDirection.y,e.attackDirection.x);
     if(s.move==1){CreateMagistrateBeams(e,side);s.salvo=3;s.beat=0;s.next=clock+3.4f;}
     if(s.move==2){foreach(int sign in new[]{-1,1})QueueManorImpact(e,NearestEnemyFooting(e.strikePoint+side*sign*2.6f,.4f),1.15f,1.05f,sign<0?1.1f:1.55f,false);}
     if(s.move==3){s.salvo=6;s.beat=0;s.next=clock;}return;
    }
    if(s.move==1){CapitalNodes(e,2);QueueCapitalTrapPair(e,true);s.salvo=4;s.beat=0;s.next=clock;}
    if(s.move==2){int before=signatureZones.Count;AddSignatureZone(e,8,e.position+e.attackDirection*(e.radius+.3f),e.attackDirection,13,3.2f);if(signatureZones.Count>before){var beam=signatureZones[signatureZones.Count-1];PrepareRoyalBeam(beam);s.trapAfterBeamAt=beam.end+.2f;}s.salvo=0;s.beat=0;s.next=clock+4.1f;}
    if(s.move==3){s.salvo=e.difficultyPhase==2?7:6;s.beat=0;s.next=clock;}return;
   }
   if(e.kind==EnemyKind.CapitalHalberd||e.kind==EnemyKind.CapitalShield||e.kind==EnemyKind.CapitalDuelist){
    if((e.kind==EnemyKind.CapitalHalberd&&s.move==0)||e.kind==EnemyKind.CapitalDuelist){s.contact=false;e.chargeUntil=clock+.38f;}
    else{if(Vector2.Distance(e.position,position)<2.65f&&Vector2.Dot((position-e.position).normalized,e.attackDirection)>.25f&&!ProjectilePathBlocked(e.position,position,.04f))TakeJourneyDamage(1.1f,Element.Physical);var slash=Draw("Royal halberd sweep",EnemyCrescent,e.position,Vector2.one*4.7f,C("f1d7a1"),DepthAt(e.position.y)+8);slash.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(e.attackDirection.y,e.attackDirection.x)*Mathf.Rad2Deg);fadingBodies.Add(new FadingBody{root=slash,renderer=slash.GetComponent<SpriteRenderer>(),expires=clock+.24f});}
    if(e.kind==EnemyKind.CapitalHalberd&&s.move==1)PressureFan(e.position+e.attackDirection*.6f,e.attackDirection,1,0,3.5f,Element.Physical,.9f);Sfx("spear",false,.4f);return;
   }
   if(e.kind==EnemyKind.CapitalMaintainer){CapitalNodes(e,1);foreach(Enemy other in enemies)if(other.iceOwner==e)other.hp=Mathf.Min(other.maximum,other.hp+15);AddFloatingHit("维护王印 · 击破节点或打断维护官",e.position+Vector2.up,C("e2c68f"));return;}
   if(e.kind==EnemyKind.CapitalAlchemist){QueueManorImpact(e,e.strikePoint,1.4f,.9f,1.15f,false);PressureFan(e.position,e.attackDirection,5,16,3,Element.Physical,.75f);Sfx("staff",false,.3f);return;}
   if(e.kind==EnemyKind.CapitalMage){AddSignatureZone(e,2,e.position+e.attackDirection*.6f,e.attackDirection,5.5f,1.2f);Sfx("staff",false,.3f);return;}
   int firstArrow=shots.Count;
   PressureFan(e.position+e.attackDirection*.6f,e.attackDirection,e.kind==EnemyKind.CapitalArcher?1:2,10,5.5f,Element.Physical,1.1f);
   for(int i=firstArrow;i<shots.Count;i++){
    Shot arrow=shots[i];var renderer=arrow.root.GetComponent<SpriteRenderer>();renderer.sprite=arrowSprite;renderer.color=Color.white;
    arrow.root.name=e.kind==EnemyKind.CapitalArcher?"Royal archer arrow":"Royal crossbow bolt";
    arrow.root.transform.localScale=e.kind==EnemyKind.CapitalArcher?new Vector3(.7f,.5f,1):new Vector3(.55f,.5f,1);
    arrow.root.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(arrow.direction.y,arrow.direction.x)*Mathf.Rad2Deg);RemoveRoundBulletDecorations(arrow);
   }
   e.recoveryUntil=clock+.8f;Sfx("bow",false,.4f);
  }
  private void CapitalVolley(Enemy e,CapitalFight s)
  {
   if(!EnemyOnScreen(e.position,.3f))return;
   Vector2 aim=(IntentLead(.25f)-e.position).normalized;if(e.kind==EnemyKind.CapitalKing&&s.move==3&&s.beat==3)SpawnHeavyMother(e);if(e.kind==EnemyKind.CapitalCommander&&s.move==2&&s.beat==3)SpawnHeavyMother(e);
   if(e.kind==EnemyKind.CapitalKing&&s.beat%3==2){int first=shots.Count;PressureFan(e.position+aim*.85f,aim,2,18,5.6f,Element.Light,1.1f);for(int i=first;i<shots.Count;i++)ArmBriefHoming(shots[i],.6f);}
   bool circle=e.kind==EnemyKind.CapitalCommander&&s.move==2||e.kind==EnemyKind.CapitalMagistrate&&s.move==3||e.kind==EnemyKind.CapitalKing&&s.move==1;
   if(circle){if(s.beat%2==0)PressureFan(e.position+aim*.85f,aim,2,12,5.5f,Element.Physical,1.1f);int count=e.kind==EnemyKind.CapitalKing?16:14;for(int i=0;i<count;i++){if(i==s.beat%count||i==(s.beat+1)%count)continue;Vector2 ray=Quaternion.Euler(0,0,i*360f/count+s.beat*8)*Vector2.up;PressureFan(e.position+ray*.85f,ray,1,0,3.3f,Element.Light,.8f);}}
   else{Vector2 tilted=Quaternion.Euler(0,0,(s.beat%2==0?-1:1)*16)*aim;PressureFan(e.position+tilted*.85f,tilted,7+e.difficultyPhase*2,12,3.2f,Element.Light,.8f);if(s.beat%2==0)PressureFan(e.position+aim*.85f,aim,2,12,5.5f,Element.Physical,1.1f);}
  }
  private void CapitalNodes(Enemy owner,int count)
  {
   CapitalFight state=CapitalState(owner);if(clock<state.nodesReady)return;foreach(Enemy e in enemies)if(e.kind==EnemyKind.CapitalConduit&&e.iceOwner==owner&&e.hp>0)return;state.nodesReady=clock+(owner.kind==EnemyKind.CapitalKing?32:owner.boss?22:8);
   for(int i=0;i<count;i++){float x=count==1?3:count==2?(i==0?-3.5f:3.5f):(i-1)*3.4f;Vector2 at=NearestEnemyFooting(new Vector2(x,3),.65f);if(Vector2.Distance(position,at)<1.4f)at=NearestEnemyFooting(at+Vector2.down*2,.65f);Enemy node=SpawnJourneyEnemy(EnemyKind.CapitalConduit,at,journeyRooms[currentRoom].depth);node.iceOwner=owner;node.body.sprite=owner.kind==EnemyKind.CapitalCommander?CapitalProp(1):CapitalProp(7);node.nextAttack=clock+1.3f+i*.25f;}
  }
  private void TickCapitalConduit(Enemy e)
  {
   Enemy owner=e.iceOwner;if(owner==null||owner.hp<=0||!enemies.Contains(owner)){DamageEnemy(e,e.hp+1000);return;}
   // A downed maintenance officer stops its mechanism; a boss's moving recovery stops new volleys only.
   if(RecoveryWindow(owner)){e.nextAttack=Mathf.Max(e.nextAttack,clock+.3f);return;}
   if(CapitalBeamActive(owner)){e.nextAttack=Mathf.Max(e.nextAttack,clock+.4f);return;}
   if(owner.kind==EnemyKind.CapitalCommander){if(CommanderChargeActive(owner)&&e.tacticCastUntil<=0){e.nextAttack=Mathf.Max(e.nextAttack,clock+.35f);return;}TickSiegeBallista(e);return;}
   if(clock>=e.nextAttack&&EnemyOnScreen(e.position,.3f)){e.nextAttack=clock+2.8f;PressureFan(e.position,(IntentLead(.1f)-e.position).normalized,3,17,3.3f,Element.Light,.8f);SpellSpark(e.position+Vector2.up*.5f,C("efd69a"),.3f);}
  }
  private void CapitalConduitBroken(Enemy node)
  {
   Enemy owner=node.iceOwner;if(owner==null||owner.hp<=0)return;CapitalNodeDisrupted(owner,node);foreach(Enemy e in enemies)if(e!=node&&e.kind==EnemyKind.CapitalConduit&&e.iceOwner==owner&&e.hp>0)return;
   if(!owner.boss){owner.windup=0;owner.nextAttack=clock+2;return;}
   InterruptCapitalZones(owner);CapitalFight s=CapitalState(owner);s.salvo=0;s.trapAfterBeamAt=0;s.shield=0;s.royalCombo=0;s.royalWindup=false;owner.windup=0;owner.chargeUntil=clock;if(owner.warning!=null)Destroy(owner.warning);owner.warning=null;
   BossBreath breath=new BossBreath{until=clock+3.5f,suppressLimit=clock+4.7f,next=clock+3.5f+BossActiveSeconds(owner)};bossBreaths[owner]=breath;owner.nextAttack=breath.until+.2f;AddFloatingHit("王印断供 · 压制窗口",owner.position+Vector2.up*2,C("f4dfa1"));
  }
  private bool CapitalAttackFinishing(Enemy e)
  {CapitalFight s;if(capitalFights.TryGetValue(e,out s)&&(s.salvo>0||s.royalCombo>0||s.trapAfterBeamAt>0))return true;foreach(var z in signatureZones)if(z.owner==e&&clock<z.end)return true;foreach(var m in manorImpacts)if(m.owner==e&&clock<m.when)return true;foreach(Enemy node in enemies)if(node.kind==EnemyKind.CapitalConduit&&node.iceOwner==e&&node.tacticCastUntil>0)return true;return false;}
  private void ClearCapitalCombat(){ClearCapitalGaits();capitalFights.Clear();}
 }
}
