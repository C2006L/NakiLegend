using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private Enemy combatTarget;
  private GameObject combatReticle;
  private int attackSequence;
  private Enemy AutoTarget(float range)
  {
   Enemy best=null;float bestDistance=range*range;
   foreach(Enemy e in enemies)
   {if(e.hp<=0)continue;float d=(e.position-position).sqrMagnitude;if(d>bestDistance||ProjectilePathBlocked(position,e.position,.08f))continue;best=e;bestDistance=d;}
   if(combatTarget!=null&&enemies.Contains(combatTarget)&&combatTarget.hp>0&&(combatTarget.position-position).sqrMagnitude<range*range&&!ProjectilePathBlocked(position,combatTarget.position,.08f))
    if(best==null||(combatTarget.position-position).sqrMagnitude<=bestDistance*1.4f)best=combatTarget;
   combatTarget=best;return best;
  }
  private Vector2 AutoAim(float range,float speed,float scatter,out Enemy target)
  {
   target=AutoTarget(range);Vector2 direction=facing;
   if(target!=null)
   {
    Vector2 offset=target.position-position;float time=speed>0?Mathf.Clamp(offset.magnitude/speed,0,.8f):0;
    direction=(offset+Vector2.ClampMagnitude(target.velocity,5)*time*.75f).normalized;
   }
   if(scatter>0){float jitter=(Mathf.Repeat(Mathf.Sin(++attackSequence*17.713f+activeSeed*.013f)*1937.31f,1)*2-1)*scatter;direction=(Vector2)(Quaternion.Euler(0,0,jitter)*direction);}
   return direction.sqrMagnitude>.1f?direction:facing;
  }
  private void TickAutoTarget()
  {
   if(combatReticle==null)combatReticle=Draw("Automatic target",windRingSprite??disc,Vector2.zero,Vector2.one,C("f2d28b"),-590);
   Enemy e=AutoTarget(10);combatReticle.SetActive(e!=null);
   if(e!=null){combatReticle.transform.position=e.position+Vector2.up*.08f;combatReticle.transform.localScale=new Vector3(e.radius*2.3f+.3f,(e.radius*2.3f+.3f)*.46f,1);}
  }
 }
}
