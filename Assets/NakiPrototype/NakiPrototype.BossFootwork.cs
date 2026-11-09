using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype {
 public sealed partial class NakiPrototype {
  private sealed class BossFootwork {public float until,next;public Vector2 direction;}
  private readonly Dictionary<Enemy,BossFootwork> bossFootwork=new Dictionary<Enemy,BossFootwork>();
  private bool RootedBoss(Enemy e){return woodlandMode&&e.kind==WoodKinds[11];}
  private Vector2 BossStep(Enemy e,Vector2 delta){Vector2 next=MoveEnemyWithinRoom(e.position,delta,e.radius);return iceSampleMode&&!IceNavigationLine(e.position,next,e.radius)?e.position:next;}
  private void PrepareBossFootwork(Enemy e){
   if(!e.boss||e.hp<=0||RootedBoss(e)||e.windup>0||e.chargeUntil>clock||e.phaseUntil>clock||e.recoveryUntil>clock)return;
   BossFootwork step;if(!bossFootwork.TryGetValue(e,out step)){step=new BossFootwork();bossFootwork[e]=step;}
   if(clock>=step.until&&clock>=step.next){
    Vector2 to=IntentLead(.35f)-e.position;float d=to.magnitude;Vector2 aim=d>.01f?to/d:Vector2.down;
    step.direction=d>4.2f?aim:d<2.2f?-aim:new Vector2(-aim.y,aim.x)*((e.attackNumber&1)==0?1:-1);
    step.until=clock+.85f;step.next=clock+4.2f;
   }
   if(clock<step.until)e.nextAttack=Mathf.Max(e.nextAttack,step.until+.08f);
  }
  private void FinishBossFootwork(Enemy e,Vector2 before,float dt){
   BossFootwork step;if(!e.boss||RootedBoss(e)||!bossFootwork.TryGetValue(e,out step)||clock>=step.until||e.windup>0||e.chargeUntil>clock||e.phaseUntil>clock||e.recoveryUntil>clock||dt<=0)return;
   if((e.position-before).sqrMagnitude<.000001f){
    float speed=Mathf.Max(1.25f,e.speed)*(clock<e.iceSlowUntil?.65f:1);Vector2 next=BossStep(e,step.direction*speed*dt);
    if((next-e.position).sqrMagnitude<.000001f){Vector2 tangent=new Vector2(-step.direction.y,step.direction.x);next=BossStep(e,tangent*speed*dt);if((next-e.position).sqrMagnitude<.000001f)next=BossStep(e,-tangent*speed*dt);}
    e.position=next;
   }
   e.velocity=(e.position-before)/dt;UpdateJourneyEnemyPose(e,e.velocity.sqrMagnitude>.00001f);
  }
 }
}
