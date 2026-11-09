using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private sealed class BossBreath {public float next,until;}
  private readonly Dictionary<Enemy,BossBreath> bossBreaths=new Dictionary<Enemy,BossBreath>();
  private bool BossResting {get{foreach(var pair in bossBreaths)if(pair.Key.hp>0&&enemies.Contains(pair.Key)&&clock<pair.Value.until)return true;return false;}}
  private bool TickBossBreathing(Enemy e)
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
   e.velocity=Vector2.zero;e.windup=0;e.chargeUntil=0;e.tacticRole=e.boss?0:e.tacticRole;
   e.nextAttack=Mathf.Max(e.nextAttack,clock+.3f);e.recoveryUntil=Mathf.Max(e.recoveryUntil,clock+.05f);UpdateJourneyEnemyPose(e,false);return true;
  }
 }
}
