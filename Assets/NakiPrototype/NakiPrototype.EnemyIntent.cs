using System;
using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private enum EnemyIntent { Pursue, Skirmish, Intercept, Roam, Anchor, Artillery, Support }
  private sealed class IntentState
  {
   public EnemyIntent role;public System.Random random;public Vector2 home,goal,waypoint,steering;
   public float decideAt,pathAt,fireAt,chargeEnd;public bool casting,charging,contact;public int decision,attack;public IntentStrike strike;public float meleeUntil,meleeReach;public GameObject weaponFx;
  }
  private readonly Dictionary<Enemy,IntentState> enemyIntents=new Dictionary<Enemy,IntentState>();
  private Vector2 intentPlayerLast,intentPlayerVelocity;private float intentPlayerSample=-1;
  private EnemyIntent NaturalIntent(Enemy e)
  {
   if(IsCapitalEnemy(e.kind))return CapitalIntent(e.kind);if(IsManorEnemy(e.kind))return ManorIntent(e.kind);
   if(woodlandMode){int k=ArrayIndexWood(e.kind);return k==0?EnemyIntent.Skirmish:k==1||k==6||k==12?EnemyIntent.Roam:k==2||k==4?EnemyIntent.Artillery:k==3?EnemyIntent.Intercept:k==5?EnemyIntent.Anchor:k==7?EnemyIntent.Pursue:EnemyIntent.Support;}
   switch(e.kind){
    case EnemyKind.SandKite:case EnemyKind.ChargeMoth:case EnemyKind.IceWisp:case EnemyKind.IceArcher:case EnemyKind.EmberLizard:return EnemyIntent.Skirmish;
    case EnemyKind.SpearGuard:case EnemyKind.ThunderStone:case EnemyKind.IceGuard:case EnemyKind.SnowCrab:case EnemyKind.ForgeGuard:case EnemyKind.MoltenCrab:return EnemyIntent.Anchor;
    case EnemyKind.StormGuard:case EnemyKind.FrostHound:return EnemyIntent.Intercept;
    case EnemyKind.Tumbleweed:case EnemyKind.CopperRoller:case EnemyKind.SnowMoth:case EnemyKind.AshBat:case EnemyKind.AshPackbeast:return EnemyIntent.Roam;
    case EnemyKind.StoneSentinel:case EnemyKind.IceRunecaster:case EnemyKind.CoalThrower:case EnemyKind.FurnaceCore:return EnemyIntent.Artillery;
    case EnemyKind.EmberAcolyte:return EnemyIntent.Support;default:return EnemyIntent.Pursue;
   }
  }
  private IntentState IntentFor(Enemy e)
  {
   IntentState state;if(enemyIntents.TryGetValue(e,out state))return state;
   state=new IntentState{home=e.position,goal=e.position,role=NaturalIntent(e),random=new System.Random(unchecked(activeSeed^currentRoom*913^enemies.IndexOf(e)*7919^(int)e.kind*37)),decideAt=clock};state.strike=ChooseIntentStrike(e,state);enemyIntents[e]=state;return state;
  }
  private void UpdateIntentPlayerMotion()
  {
   if(intentPlayerSample>=0&&clock>intentPlayerSample+.0001f){Vector2 v=(position-intentPlayerLast)/(clock-intentPlayerSample);intentPlayerVelocity=Vector2.Lerp(intentPlayerVelocity,Vector2.ClampMagnitude(v,moveSpeed*1.4f),.5f);}
   if(intentPlayerSample<0||clock>intentPlayerSample){intentPlayerLast=position;intentPlayerSample=clock;}
  }
  private Vector2 IntentLead(float seconds){return position+Vector2.ClampMagnitude(intentPlayerVelocity*seconds,1.35f);}
  private GameObject EnemyMuzzleTell(Vector2 at,Vector2 aim)
  {return Draw("Enemy muzzle charge",disc,at+aim*.35f+Vector2.up*.2f,Vector2.one*.14f,new Color(1,.84f,.52f,.8f),DepthAt(at.y)+5);}
  private void IntentRoleChanged(Enemy e)
  {
   IntentState state=IntentFor(e);if(e.tacticRole==3)state.role=EnemyIntent.Support;else if(e.tacticRole==4)state.role=EnemyIntent.Skirmish;else if(e.tacticRole==5)state.role=EnemyIntent.Anchor;
   state.decideAt=clock;
  }
  private Vector2 IntentGoal(Enemy e,IntentState s)
  {
   Vector2 to=position-e.position,aim=to.sqrMagnitude>.01f?to.normalized:Vector2.down,side=new Vector2(-aim.y,aim.x);
   int handed=(s.decision++%3==0?-1:1);float distance=to.magnitude;Vector2 desired;
   switch(s.role){
    case EnemyIntent.Pursue:desired=position;break;
    case EnemyIntent.Intercept:desired=IntentLead(.8f)+side*(s.decision%2==0?1.4f:-1.4f);break;
    case EnemyIntent.Roam:{float angle=(float)s.random.NextDouble()*Mathf.PI*2;desired=(s.decision%3==0?IntentLead(.45f):e.position)+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*(1.8f+(float)s.random.NextDouble()*1.8f);break;}
    case EnemyIntent.Skirmish:desired=distance<3.4f?e.position-aim*2.4f:distance>6.3f?position-aim*5:e.position+side*handed*2.1f;break;
    case EnemyIntent.Artillery:desired=Vector2.Distance(e.position,s.home)>2.2f?s.home:distance<3.2f?e.position-aim*2:e.position+side*handed*1.1f;break;
    case EnemyIntent.Anchor:desired=Vector2.Distance(position,s.home)<3?Vector2.MoveTowards(s.home,position,1.1f):s.home;break;
    default:{Enemy ally=null;foreach(Enemy other in enemies)if(other!=e&&!other.boss&&other.hp>0&&!CombatFixture(other)&&(ally==null||other.hp/other.maximum<ally.hp/ally.maximum))ally=other;desired=distance<3?e.position-aim*2:ally!=null?ally.position-aim*2:s.home;break;}
   }
   // Ranged roles relocate around cover instead of waiting indefinitely behind it.
   if((s.role==EnemyIntent.Skirmish||s.role==EnemyIntent.Artillery||s.role==EnemyIntent.Support)&&ProjectilePathBlocked(e.position,position,.1f))desired=position-aim*3.5f+side*handed*2.8f;
   return NearestEnemyFooting(desired,e.radius);
  }
  private void MoveIntentEnemy(Enemy e,IntentState s,float dt)
  {
   if(clock>=s.decideAt){s.goal=IntentGoal(e,s);s.decideAt=clock+.8f+(float)s.random.NextDouble()*.65f;s.pathAt=0;}
   if(clock>=s.pathAt){s.waypoint=IcePursuitWaypoint(e.position,s.goal,e.radius);s.pathAt=clock+.45f;}
   Vector2 direction=s.waypoint-e.position;if(direction.magnitude<.10f)return;direction.Normalize();
   Vector2 separation=Vector2.zero;foreach(Enemy other in enemies)if(other!=e&&other.hp>0){Vector2 away=e.position-other.position;float d=away.magnitude,limit=e.radius+other.radius+.3f;if(d>.01f&&d<limit)separation+=away/d*(1-d/limit);}
   direction=(direction+Vector2.ClampMagnitude(separation,1)*.7f).normalized;s.steering=Vector2.Lerp(s.steering,direction,Mathf.Clamp01(dt*9));
   float speed=Mathf.Max(e.speed,s.role==EnemyIntent.Intercept?2.65f:s.role==EnemyIntent.Pursue?2.2f:s.role==EnemyIntent.Roam?2.1f:1.6f);
   if(s.role==EnemyIntent.Anchor)speed*=.55f;if(clock<e.iceSlowUntil)speed*=.6f;
   e.position=MoveEnemyWithinRoom(e.position,Vector2.ClampMagnitude(s.steering,1)*speed*dt,e.radius);
  }
  private bool TickEnemyIntent(Enemy e,float dt)
  {
   if(e.boss||CombatFixture(e))return false;IntentState s=IntentFor(e);Vector2 old=e.position;RecoverIceFooting(e);
   if(clock<e.tacticControlUntil){s.casting=s.charging=false;s.meleeUntil=0;if(s.weaponFx!=null)Destroy(s.weaponFx);e.windup=e.chargeUntil=0;IntentPose(e,old,dt);return true;}
   if(clock<s.meleeUntil){TickIntentMelee(e,s,dt);IntentPose(e,old,dt);return true;}
   if(s.charging){
    if(clock<s.chargeEnd){e.position=MoveEnemyCharge(e,e.attackDirection*(s.role==EnemyIntent.Roam?6.4f:7)*dt);if(!s.contact&&DistanceToSegment(position,old,e.position)<e.radius+.29f&&!ProjectilePathBlocked(e.position,position,.03f)){s.contact=true;TakeJourneyDamage(1.1f,e.attackElement);}if((e.position-old).sqrMagnitude<.000001f&&dt>0)s.chargeEnd=clock;}
    else{s.charging=false;e.chargeUntil=0;e.recoveryUntil=clock+.7f;s.decideAt=clock+.25f;}
    IntentPose(e,old,dt);return true;
   }
   if(s.casting){
    if(e.windup<=0){s.casting=false;e.nextAttack=Mathf.Max(e.nextAttack,clock+.4f);}
    else {e.windup=Mathf.Max(0,s.fireAt-clock);if(clock>=s.fireAt){s.casting=false;if(e.warning!=null)Destroy(e.warning);e.warning=null;ResolveIntentAttack(e,s);}}
    IntentPose(e,old,dt);return true;
   }
   if(clock>=e.recoveryUntil){
    float d=Vector2.Distance(position,e.position);bool canShoot=EnemyOnScreen(e.position,e.radius+.35f)&&!ProjectilePathBlocked(e.position,position,.1f);
    float range=IntentAttackRange(e,s);
    if(clock>=e.nextAttack&&d<range&&canShoot){BeginIntentAttack(e,s);}
    else MoveIntentEnemy(e,s,dt);
   }
   IntentPose(e,old,dt);return true;
  }
  private void BeginIntentAttack(Enemy e,IntentState s){BeginArmedIntent(e,s);}
  private void ResolveIntentAttack(Enemy e,IntentState s){ResolveArmedIntent(e,s);}
  private void IntentPose(Enemy e,Vector2 old,float dt)
  {e.velocity=dt>0?(e.position-old)/dt:Vector2.zero;if(!woodlandMode&&IsThunderEnemy(e.kind))UpdateThunderPose(e);else UpdateJourneyEnemyPose(e,e.velocity.sqrMagnitude>.001f);
   IntentState state;if(enemyIntents.TryGetValue(e,out state)){bool striking=clock<state.meleeUntil;if(state.casting||state.charging||striking)e.body.flipX=e.attackDirection.x<0;
    e.root.transform.localRotation=Quaternion.Euler(0,0,striking&&state.strike==IntentStrike.Slash?Mathf.Sin((1-(state.meleeUntil-clock)/.24f)*Mathf.PI)*-10*(e.attackDirection.x<0?-1:1):0);}
   UpdateEliteSigil(e);}
  private void ResetEnemyIntents(){ResetEarlyEncounterGroups();ClearIntentBombs();enemyIntents.Clear();intentPlayerSample=-1;intentPlayerVelocity=Vector2.zero;}
  private void ShapeIntentFormation(RoomNode room)
  {
   if(capitalMode||manorCourtyardMode)return;
   if(pendingSpawns.Count==0||room.kind!=RoomKind.Combat&&room.kind!=RoomKind.Elite)return;
   int budget=LearningEncounter?4:room.kind==RoomKind.Elite?7:CurrentIsland.ordinal>=4?6:5;
   while(pendingSpawns.Count>budget){var p=pendingSpawns[pendingSpawns.Count-1];if(p.mark!=null)Destroy(p.mark);pendingSpawns.RemoveAt(pendingSpawns.Count-1);}
   if(ShapeEarlyFormation(room))return;
   EnemyKind[] set=woodlandMode?new[]{WoodKinds[1],WoodKinds[0],WoodKinds[2],WoodKinds[5],WoodKinds[3],WoodKinds[8]}:fireChapterMode?new[]{EnemyKind.AshRaider,EnemyKind.EmberLizard,EnemyKind.CoalThrower,EnemyKind.ForgeGuard,EnemyKind.AshBat,EnemyKind.EmberAcolyte}:iceSampleMode?new[]{EnemyKind.FrostHound,EnemyKind.IceArcher,EnemyKind.IceRunecaster,EnemyKind.IceGuard,EnemyKind.SnowMoth,EnemyKind.IceWisp}:ThunderActive?new[]{EnemyKind.StormGuard,EnemyKind.ChargeMoth,EnemyKind.ThunderStone,EnemyKind.CopperRoller,EnemyKind.StormGuard,EnemyKind.ChargeMoth}:new[]{EnemyKind.SandScorpion,EnemyKind.SandKite,EnemyKind.StoneSentinel,EnemyKind.SpearGuard,EnemyKind.Tumbleweed,EnemyKind.SandKite};
   if(fireChapterMode&&room.stage==0){set[3]=EnemyKind.AshRaider;set[5]=EnemyKind.EmberLizard;}
   for(int i=0;i<pendingSpawns.Count;i++){int at=i==0?0:i==1?1:(i+currentWave+room.stage)%set.Length;pendingSpawns[i].kind=set[at];pendingSpawns[i].role=0;pendingSpawns[i].when=clock+1+i*.28f;}
  }
 }
}
