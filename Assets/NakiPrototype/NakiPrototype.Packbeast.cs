using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private void StopPackbeastCharge(Enemy e,bool blocked)
  {
   e.chargeUntil=0;e.recoveryUntil=clock+(blocked?1.35f:1.1f);e.nextAttack=Mathf.Max(e.nextAttack,e.recoveryUntil+.65f);
   if(blocked)SpellSpark(e.position+e.attackDirection*e.radius,C("baa788"),.18f);
  }
  private void UpdatePackbeast(Enemy e,float dt)
  {
   Vector2 old=e.position;dt=Mathf.Clamp(dt,0,.05f);
   if(e.chargeUntil>0){
    if(clock>=e.chargeUntil)StopPackbeastCharge(e,false);
    else{
     float speed=Mathf.Lerp(2.5f,7.2f,Mathf.Clamp01((clock-e.chargeBegan)/.16f))*(clock<e.iceSlowUntil?.6f:1);
     Vector2 step=e.attackDirection*speed*dt;int count=Mathf.Max(1,Mathf.CeilToInt(step.magnitude/.08f));
     for(int i=0;i<count;i++){Vector2 next=e.position+step/count;if(!IceNavigationLine(e.position,next,e.radius)){StopPackbeastCharge(e,true);break;}e.position=next;}
     if(!e.chargeContact&&DistanceToSegment(position,old,e.position)<e.radius+.28f&&!ProjectilePathBlocked(e.position,position,.04f)){e.chargeContact=true;TakeJourneyDamage(1.4f,Element.Fire);}
     if(e.chargeUntil>0&&clock>=e.chargeDustAt){e.chargeDustAt=clock+.15f;SpellSpark(e.position-e.attackDirection*.5f,C("a5927a"),.08f);}
    }
   }else if(e.windup>0){
    e.windup=Mathf.Max(0,e.windup-dt);
    if(e.windup<=0){if(e.warning!=null){Destroy(e.warning);e.warning=null;}e.chargeBegan=clock;e.chargeUntil=clock+.85f;e.chargeContact=false;e.chargeDustAt=clock;}
   }else if(clock>=e.recoveryUntil){
    RecoverIceFooting(e);Vector2 to=position-e.position;
    if(clock>=e.nextAttack&&to.magnitude<8&&to.magnitude>.65f&&!ProjectilePathBlocked(e.position,position,.15f)){
     e.attackDirection=to.normalized;e.strikePoint=position;e.actionStarted=clock;e.windup=.85f;e.nextAttack=clock+3.1f;
     e.warning=ChapterLineWarning(e.position,e.attackDirection,5.8f,e.radius*2);
    }else if(to.magnitude>2.6f||ProjectilePathBlocked(e.position,position,.15f))MoveIcePursuit(e,e.speed*(clock<e.iceSlowUntil?.6f:1)*dt);
   }
   PoseWorkshopEnemy(e,old,dt);
   // Face the locked charge direction through wind-up, dash and recovery. Tracking the
   // player after passing them caused the old left/right flicker.
   if(e.windup>0||e.chargeUntil>0||clock<e.recoveryUntil)e.body.flipX=e.attackDirection.x<0;
  }
 }
}
