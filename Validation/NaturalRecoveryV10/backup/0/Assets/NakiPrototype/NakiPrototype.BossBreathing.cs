using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private bool bossAttackEmission;
  private void UpdateTacticalEnemy(Enemy e,float dt)
  {
   Vector2 before=e.position;float oldRecovery=e.recoveryUntil,oldNext=e.nextAttack;
   bool previous=bossAttackEmission;bossAttackEmission=e.boss;
   try{UpdateTacticalEnemyCore(e,dt);}finally{bossAttackEmission=previous;}
   if(!e.boss||BossResting)return;
   if(!RootedBoss(e)&&clock>=e.tacticControlUntil&&e.windup<=0&&e.chargeUntil<=clock&&clock>=e.phaseUntil&&dt>0&&(e.position-before).sqrMagnitude<dt*dt*.25f){
    Vector2 to=position-e.position;Vector2 aim=to.sqrMagnitude>.01f?to.normalized:Vector2.down;
    Vector2 step=to.magnitude>5?aim:to.magnitude<2.3f?-aim:new Vector2(-aim.y,aim.x)*((e.attackNumber&1)==0?1:-1);
    float speed=(1.6f+e.difficultyPhase*.2f)*(clock<e.iceSlowUntil?.65f:1);Vector2 next=BossStep(e,step*speed*dt);
    if((next-e.position).sqrMagnitude<.00001f)next=BossStep(e,new Vector2(-step.y,step.x)*speed*dt);
    e.position=next;e.velocity=(e.position-before)/dt;UpdateJourneyEnemyPose(e,e.velocity.sqrMagnitude>.00001f);
   }
   // Preserve windup tells and the full breathing window, compress only active-phase gaps.
   if(e.recoveryUntil>oldRecovery+.001f&&e.recoveryUntil>clock)e.recoveryUntil=clock+(e.recoveryUntil-clock)*.65f;
   if(e.nextAttack>oldNext+.001f&&e.nextAttack>clock){float tell=Mathf.Max(0,e.windup);e.nextAttack=clock+tell+Mathf.Max(.2f,(e.nextAttack-clock-tell)*.65f);}
  }
  private sealed class BossBreath {public float next,until;}
  private readonly Dictionary<Enemy,BossBreath> bossBreaths=new Dictionary<Enemy,BossBreath>();
  private bool BossResting {get{foreach(var pair in bossBreaths)if(pair.Key.hp>0&&enemies.Contains(pair.Key)&&clock<pair.Value.until)return true;return false;}}
  private bool TickBossBreathing(Enemy e,float dt)
  {
   if(e.boss){
    BossBreath state;if(!bossBreaths.TryGetValue(e,out state)){state=new BossBreath{next=clock+7.5f};bossBreaths[e]=state;}
    if(clock>=state.next&&clock>=state.until){
     state.until=clock+3.5f;state.next=state.until+7.5f;
     // End the attack sequence completely, including delayed volleys and hazards.
     foreach(var enemy in enemies){enemy.windup=0;enemy.chargeUntil=0;if(enemy.warning!=null){Destroy(enemy.warning);enemy.warning=null;}}
     foreach(var captain in manorCaptains.Values){captain.bursts=0;captain.shieldUntil=0;}
     ClearThunderHazards();ClearFireHazards();ClearBossSignatures();foreach(var impact in manorImpacts){if(impact.ring!=null)Destroy(impact.ring);if(impact.orb!=null)Destroy(impact.orb);}manorImpacts.Clear();
     foreach(var shot in shots)if(shot.hostile)shot.life=0;
     AddFloatingHit("收招 · 反击时机",e.position+Vector2.up,C("f4dfa1"));
    }
   }
   if(!BossResting)return false;
   Vector2 before=e.position;
   if(e.boss&&!RootedBoss(e)&&clock>=e.tacticControlUntil&&dt>0){Vector2 to=position-e.position;Vector2 aim=to.sqrMagnitude>.01f?to.normalized:Vector2.down;Vector2 step=to.magnitude<2.6f?-aim:new Vector2(-aim.y,aim.x)*((e.attackNumber&1)==0?1:-1);e.position=BossStep(e,step*.85f*dt);e.attackDirection=aim;}
   e.velocity=dt>0?(e.position-before)/dt:Vector2.zero;e.windup=0;e.chargeUntil=0;e.tacticRole=e.boss?0:e.tacticRole;
   e.nextAttack=Mathf.Max(e.nextAttack,clock+.3f);e.recoveryUntil=Mathf.Max(e.recoveryUntil,clock+.05f);UpdateJourneyEnemyPose(e,e.velocity.sqrMagnitude>.00001f);return true;
  }
 }
}
