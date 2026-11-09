using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private sealed class ManorImpact {public Enemy owner;public Vector2 at;public float born,when,radius,damage;public bool meteor;public GameObject ring,orb;}
  private sealed class ManorCaptainState {public int wave;public bool contact;public float nextDust,nextBurst,shieldUntil;public int bursts;}
  private readonly List<ManorImpact> manorImpacts=new List<ManorImpact>();
  private readonly Dictionary<Enemy,ManorCaptainState> manorCaptains=new Dictionary<Enemy,ManorCaptainState>();
  private void ConfigureManorEnemy(Enemy e)
  {
   int k=(int)e.kind-(int)EnemyKind.ManorPatrol;
   e.boss=e.kind==EnemyKind.ManorCaptain;e.radius=e.boss?.68f:k==3?.30f:.36f;
   float[] hp={175,305,145,130,200,6120,340,425,245,165,210};
   e.hp=hp[k];e.speed=k==3?2.7f:k==7?1.6f:2.15f;e.affinity=k==4||k==8||k==10?Element.Dark:Element.Physical;e.attackElement=e.affinity;e.soulEligible=!e.boss;
   if(k==1||k==5){e.tacticRole=5;e.attackDirection=(position-e.position).normalized;}
  }
  private EnemyIntent ManorIntent(EnemyKind k)
  {
   switch(k){case EnemyKind.ManorShield:return EnemyIntent.Anchor;case EnemyKind.ManorHound:return EnemyIntent.Intercept;
    case EnemyKind.ManorCrossbow:case EnemyKind.ManorArcher:return EnemyIntent.Skirmish;case EnemyKind.ManorClerk:return EnemyIntent.Support;
    case EnemyKind.ManorPriest:return EnemyIntent.Artillery;case EnemyKind.ManorMage:return EnemyIntent.Roam;default:return EnemyIntent.Pursue;}
  }
  private IntentStrike ManorStrike(Enemy e)
  {
   switch(e.kind){case EnemyKind.ManorHound:return IntentStrike.Charge;case EnemyKind.ManorCrossbow:case EnemyKind.ManorArcher:return IntentStrike.Arrow;
    case EnemyKind.ManorPriest:case EnemyKind.ManorHammer:return IntentStrike.Lob;case EnemyKind.ManorClerk:return IntentStrike.Support;
    case EnemyKind.ManorMage:return IntentStrike.Bolt;default:return IntentStrike.Slash;}
  }
  private float ManorRange(Enemy e)
  {return e.kind==EnemyKind.ManorPriest?9:e.kind==EnemyKind.ManorHammer?3.4f:e.kind==EnemyKind.ManorAxe?3.2f:e.kind==EnemyKind.ManorHound?4.8f:e.kind==EnemyKind.ManorPatrol||e.kind==EnemyKind.ManorShield?2.4f:9;}
  private void ManorWindup(Enemy e,IntentState s)
  {
   if(!IsManorEnemy(e.kind))return;
   float tell=e.kind==EnemyKind.ManorPriest?1.05f:e.kind==EnemyKind.ManorHammer?.95f:e.kind==EnemyKind.ManorAxe?.72f:e.kind==EnemyKind.ManorCrossbow?.72f:e.kind==EnemyKind.ManorArcher?.8f:e.kind==EnemyKind.ManorClerk?1:e.kind==EnemyKind.ManorHound?.65f:.52f;
   s.fireAt=clock+tell;e.windup=tell;e.nextAttack=s.fireAt+(e.kind==EnemyKind.ManorPriest?11:e.kind==EnemyKind.ManorHammer?2.3f:e.kind==EnemyKind.ManorMage?2.0f:1.45f);
   if(e.kind==EnemyKind.ManorHammer){e.strikePoint=NearestEnemyFooting(e.position+e.attackDirection*1.2f,.35f);if(e.warning!=null)Destroy(e.warning);e.warning=WarningCircle(e.strikePoint,2.1f);}
   if(e.kind==EnemyKind.ManorPriest&&e.warning!=null){Destroy(e.warning);e.warning=null;}
   if(e.kind==EnemyKind.ManorHound){int committed=0;foreach(var other in enemies)if(other!=e&&other.kind==e.kind&&(other.windup>0||other.chargeUntil>clock))committed++;if(committed>=2){s.casting=false;e.windup=0;e.nextAttack=clock+.75f;if(e.warning!=null)Destroy(e.warning);}}
  }
  private bool ResolveManorIntent(Enemy e,IntentState s)
  {
   if(!IsManorEnemy(e.kind))return false;
   if(e.kind==EnemyKind.ManorPriest){QueueManorImpact(e,NearestEnemyFooting(e.strikePoint,.4f),1.65f,1.7f,1.25f,true);e.recoveryUntil=clock+.8f;return true;}
   if(e.kind==EnemyKind.ManorHammer){QueueManorImpact(e,e.strikePoint,2.1f,1.65f,.15f,false);e.recoveryUntil=clock+1;Sfx("hammer",false,.7f);return true;}
   if(e.kind==EnemyKind.ManorMage){
    Vector2 aim=e.attackDirection;int start=shots.Count;PressureFan(e.position+aim*.55f,aim,4,13,4.4f,Element.Dark,1.15f);
    for(int i=start;i<shots.Count;i++){var shot=shots[i];shot.root.name="Dark mage smoke orb";shot.root.GetComponent<SpriteRenderer>().color=C("6f369b");AddShotRibbon(shot,C("402b58"),.11f);shot.tracking=false;}
    e.recoveryUntil=clock+.65f;Sfx("staff",false,.35f);return true;
   }
   if(e.kind==EnemyKind.ManorClerk){
    int helped=0;foreach(var ally in enemies)if(ally!=e&&!ally.boss&&ally.hp>0&&Vector2.Distance(e.position,ally.position)<4&&!ProjectilePathBlocked(e.position,ally.position,.05f)){
     ally.tacticWardUntil=clock+2.2f;SpellSpark(ally.position,C("9c8fb4"),.45f);if(++helped==2)break;
    }
    if(helped==0)PressureFan(e.position,e.attackDirection,1,0,3.8f,Element.Dark,1.1f);e.recoveryUntil=clock+.75f;return true;
   }
   return false;
  }
  private void QueueManorImpact(Enemy owner,Vector2 at,float radius,float damage,float delay,bool meteor)
  {
   var m=new ManorImpact{owner=owner,at=at,born=clock,when=clock+delay,radius=radius,damage=damage,meteor=meteor};
   m.ring=WarningCircle(at,radius);m.ring.name=meteor?"Dark priest meteor landing tell":"Hammer impact tell";
   if(meteor){m.ring.GetComponent<SpriteRenderer>().color=new Color(.64f,.32f,.82f,.65f);m.orb=Draw("Falling dark violet smoky meteor",disc,at+Vector2.up*5,Vector2.one*.5f,C("542275"),DepthAt(at.y)+40);}
   manorImpacts.Add(m);Sfx(meteor?"warn":"heavy",false,.35f);
  }
  private void TickManorImpacts()
  {
   for(int i=manorImpacts.Count-1;i>=0;i--){var m=manorImpacts[i];bool cancelled=m.owner.hp<=0||!enemies.Contains(m.owner);
    if(!cancelled&&clock<m.when){if(m.orb!=null){float t=Mathf.Clamp01((clock-m.born)/(m.when-m.born));m.orb.transform.position=m.at+Vector2.up*Mathf.Lerp(5,.1f,t*t);if(Mathf.FloorToInt(clock*18)!=Mathf.FloorToInt((clock-.016f)*18)){var smoke=Draw("Meteor thick smoke",softDisc??disc,m.orb.transform.position+new Vector3(.1f,.2f,0),Vector2.one*.6f,new Color(.16f,.08f,.22f,.65f),DepthAt(m.at.y)+39);fadingBodies.Add(new FadingBody{root=smoke,renderer=smoke.GetComponent<SpriteRenderer>(),expires=clock+.3f});}}continue;}
    if(!cancelled){SpellSpark(m.at,m.meteor?C("813caf"):C("dad0bd"),m.radius);if(Vector2.Distance(position,m.at)<m.radius+.25f&&!ProjectilePathBlocked(m.at,position,.04f))TakeJourneyDamage(m.damage,m.meteor?Element.Dark:Element.Physical);Sfx(m.meteor?"stone":"hammer",false,.65f);cameraShakeUntil=clock+.15f;}
    if(m.ring!=null)Destroy(m.ring);if(m.orb!=null)Destroy(m.orb);manorImpacts.RemoveAt(i);
   }
  }
  private void ClearManorImpacts(){foreach(var m in manorImpacts){if(m.ring!=null)Destroy(m.ring);if(m.orb!=null)Destroy(m.orb);}manorImpacts.Clear();manorCaptains.Clear();}
  private void SpawnManorWave(RoomNode n)
  {
   if(n.id==14){SpawnJourneyEnemy(EnemyKind.ManorCaptain,new Vector2(0,1.8f),n.depth);return;}
   var rng=new System.Random(activeSeed^n.id*941^currentWave*73);
   EnemyKind[][] sets={
    new[]{EnemyKind.ManorPatrol,EnemyKind.ManorCrossbow,EnemyKind.ManorShield,EnemyKind.ManorHound,EnemyKind.ManorArcher},
    new[]{EnemyKind.ManorAxe,EnemyKind.ManorArcher,EnemyKind.ManorHound,EnemyKind.ManorCrossbow,EnemyKind.ManorPatrol},
    new[]{EnemyKind.ManorHammer,EnemyKind.ManorMage,EnemyKind.ManorShield,EnemyKind.ManorArcher,EnemyKind.ManorPatrol},
    new[]{EnemyKind.ManorPriest,EnemyKind.ManorShield,EnemyKind.ManorAxe,EnemyKind.ManorArcher,EnemyKind.ManorHound,EnemyKind.ManorClerk}};
   int set=n.id==13?3:n.id==4?1:(n.id+currentWave)%3;EnemyKind[] choices=sets[set];
   int count=n.kind==RoomKind.Elite?6:5;for(int i=0;i<count;i++){
    EnemyKind kind=choices[(i+currentWave-1)%choices.Length];QueueEnemy(kind,FindJourneySpawn(rng,.45f));var p=pendingSpawns[pendingSpawns.Count-1];p.when=clock+1.3f+i*.3f;
   }
  }
  private void TickManorCaptain(Enemy e,float dt)
  {
   ManorCaptainState s;if(!manorCaptains.TryGetValue(e,out s)){s=new ManorCaptainState();manorCaptains[e]=s;}
   int phase=e.hp<e.maximum*.35f?2:e.hp<e.maximum*.70f?1:0;e.difficultyPhase=phase;e.secondPhase=phase>0;
   if(phase>s.wave){s.wave=phase;SummonJourneyAdds(phase==1?EnemyKind.ManorPatrol:EnemyKind.ManorCrossbow,3);SpellSpark(e.position,C("b6b3d5"),1);}
   // Staggered salvos leave gaps; no trajectory warning lines for normal bullets.
   if(s.bursts>0&&clock>=s.nextBurst){s.bursts--;s.nextBurst=clock+.2f;Vector2 aim=(position-e.position).normalized;PressureFan(e.position+aim*.8f,aim,9+phase*2,12,5.2f+phase*.4f,Element.Dark,1.2f);}
   e.tacticRole=clock<s.shieldUntil?5:0;
   Vector2 old=e.position;e.position=NearestEnemyFooting(e.position,e.radius);
   if(clock<e.tacticControlUntil){e.windup=e.chargeUntil=0;e.recoveryUntil=Mathf.Max(e.recoveryUntil,clock+.25f);UpdateJourneyEnemyPose(e,false);return;}
   if(e.chargeUntil>clock){e.position=MoveEnemyCharge(e,e.attackDirection*8.3f*dt);if(!s.contact&&DistanceToSegment(position,old,e.position)<e.radius+.35f){s.contact=true;TakeJourneyDamage(1.7f,Element.Physical);}if((e.position-old).sqrMagnitude<.000001f&&dt>0)e.chargeUntil=clock;
    if(clock>=s.nextDust){s.nextDust=clock+.09f;SpellSpark(e.position,C("a5a4ab"),.18f);}e.recoveryUntil=clock+.8f;
   }
   else if(e.windup>0){e.windup=Mathf.Max(0,e.windup-dt);if(e.windup<=0){if(e.warning!=null)Destroy(e.warning);e.warning=null;ResolveManorCaptain(e,s);}}
   else if(clock>=e.recoveryUntil){
    Vector2 offset=position-e.position;if(clock<s.shieldUntil){MoveIcePursuit(e,e.speed*1.35f*dt);if(clock>=s.nextDust){s.nextDust=clock+.18f;SpellSpark(e.position+e.attackDirection*.7f,C("b6b3d5"),.35f);}}if(clock>=e.nextAttack&&EnemyOnScreen(e.position,.7f)){e.attackNumber++;int move=e.attackNumber%5;e.attackDirection=offset.sqrMagnitude>.01f?offset.normalized:Vector2.down;e.strikePoint=NearestEnemyFooting(IntentLead(.35f),.4f);e.actionStarted=clock;
     e.windup=move==1?.8f:move==2?.65f:move==3?1:.6f;e.nextAttack=clock+e.windup+(phase==2?.65f:.95f);
     e.warning=move==1?ChapterLineWarning(e.position,e.attackDirection,5.5f,1.35f):move==2?WarningSector(e.position,e.attackDirection,3.0f):move==3?WarningCircle(e.strikePoint,1.75f):null;
    }else{float target=offset.magnitude<3?1:offset.magnitude>5.5f?1:.45f;Vector2 goal=position;MoveIcePursuit(e,e.speed*target*dt);e.attackDirection=Vector2.Lerp(e.attackDirection,offset.normalized,dt*2).normalized;}
   }
   e.velocity=dt>0?(e.position-old)/dt:Vector2.zero;UpdateJourneyEnemyPose(e,e.velocity.sqrMagnitude>.001f);e.body.flipX=e.attackDirection.x<0;
  }
  private void ResolveManorCaptain(Enemy e,ManorCaptainState s)
  {
   e.actionStarted=clock;e.recoveryUntil=clock+.75f;int move=e.attackNumber%5;
   if(move==1){s.contact=false;e.chargeUntil=clock+.72f;s.bursts=e.difficultyPhase>0?3:2;s.nextBurst=clock+.18f;Sfx("spear",false,.65f);return;}
   if(move==2){
    Vector2 to=position-e.position;if(to.magnitude<3.2f&&Vector2.Dot(to.normalized,e.attackDirection)>.25f&&!ProjectilePathBlocked(e.position,position,.04f))TakeJourneyDamage(1.65f,Element.Physical);
    var slash=Draw("Captain broad spear sweep",EnemyCrescent,e.position,Vector2.one*6,C("c5c9df"),DepthAt(e.position.y)+8);slash.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(e.attackDirection.y,e.attackDirection.x)*Mathf.Rad2Deg);fadingBodies.Add(new FadingBody{root=slash,renderer=slash.GetComponent<SpriteRenderer>(),expires=clock+.26f});PressureFan(e.position,e.attackDirection,7+e.difficultyPhase*2,17,5.5f,Element.Physical,1.15f);Sfx("spear",false,.65f);return;
   }
   if(move==3){QueueManorImpact(e,e.strikePoint,1.75f,1.6f,.25f,false);if(e.difficultyPhase>0){Vector2 side=new Vector2(-e.attackDirection.y,e.attackDirection.x);QueueManorImpact(e,NearestEnemyFooting(e.strikePoint+side*2.5f,.4f),1.2f,1.3f,.7f,false);QueueManorImpact(e,NearestEnemyFooting(e.strikePoint-side*2.5f,.4f),1.2f,1.3f,.7f,false);}return;}
   if(move==4){s.shieldUntil=clock+1.5f;e.tacticRole=5;e.recoveryUntil=clock+.3f;AddFloatingHit("举盾推进 · 绕侧反击",e.position+Vector2.up,C("ded6ed"));s.bursts=2;s.nextBurst=clock+.35f;return;}
   int count=14+e.difficultyPhase*4;for(int i=0;i<count;i++){float angle=(i+.5f)*Mathf.PI*2/count;Vector2 ray=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));PressureFan(e.position+ray*.8f,ray,1,0,4.5f,Element.Dark,1.2f);}s.bursts=3+e.difficultyPhase;s.nextBurst=clock+.3f;Sfx("spear",false,.45f);
  }
 }
}
