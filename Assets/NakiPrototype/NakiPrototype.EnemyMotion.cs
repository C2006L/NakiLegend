using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private Vector2 MoveEnemyCharge(Enemy e,Vector2 delta)
  {
   Vector2 at=e.position;int steps=Mathf.Max(1,Mathf.CeilToInt(delta.magnitude/.06f));Vector2 step=delta/steps;
   for(int i=0;i<steps;i++){
    Vector2 next=at+step;
    if(!IceNavigationLine(at,next,e.radius)){
     e.chargeUntil=0;e.recoveryUntil=Mathf.Max(e.recoveryUntil,clock+1);e.nextAttack=Mathf.Max(e.nextAttack,e.recoveryUntil+.35f);break;
    }
    at=next;
   }
   return at;
  }
  private void StabilizeEnemyFacing(Enemy e,bool previousFlip)
  {
   if(e.body==null)return;
   bool committed=e.windup>0||e.chargeUntil>clock||e.tacticCastUntil>clock||clock<e.recoveryUntil;
   Vector2 direction=committed?e.attackDirection:position-e.position;
   // Keep the last facing when the target crosses the vertical centreline by a few pixels.
   float threshold=committed?.15f:.30f;
   e.body.flipX=Mathf.Abs(direction.x)>threshold?direction.x<0:previousFlip;
  }
 }
}
