using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private sealed class CoreBurst {public Enemy owner;public Vector2 aim;public int count;public float when;public GameObject warning;}
  private readonly List<CoreBurst> coreBursts=new List<CoreBurst>();
  private static bool IsCoreEnemy(EnemyKind k){return k>=EnemyKind.MagmaTortoise&&k<=EnemyKind.AshPackbeast;}
  private void ConfigureCoreEnemy(Enemy e){e.boss=e.kind==EnemyKind.MagmaTortoise;e.radius=e.boss?1.15f:e.kind==EnemyKind.AshPackbeast?.58f:.35f;e.hp=e.maximum=e.boss?1800:e.kind==EnemyKind.AshPackbeast?172:144;e.speed=e.boss?.85f:e.kind==EnemyKind.AshPackbeast?1.4f:1.15f;e.affinity=e.attackElement=Element.Fire;e.iceArmor=e.boss?4:e.kind==EnemyKind.AshPackbeast?1:0;e.soulEligible=true;e.nextAttack=clock+1.5f;}
  private void UpdateCoreEnemy(Enemy e,float dt)
  {
   if(e.kind==EnemyKind.AshPackbeast){UpdatePackbeast(e,dt);return;}
   RecoverIceFooting(e);Vector2 old=e.position,to=position-e.position;
   if(e.boss&&!e.secondPhase&&e.hp<e.maximum*.7f){e.secondPhase=true;e.windup=0;if(e.warning!=null)Destroy(e.warning);e.recoveryUntil=clock+1.4f;e.nextAttack=clock+1.6f;AddFloatingHit("潮焰回涌 · 两段火雨",e.position+Vector2.up*2,C("ffce87"));}
   if(e.chargeUntil>0){if(clock<e.chargeUntil){e.position=MoveEnemyCharge(e,e.attackDirection*5.2f*(clock<e.iceSlowUntil?.6f:1)*dt);if(DistanceToSegment(position,old,e.position)<e.radius+.28f&&!ProjectilePathBlocked(e.position,position,.04f))TakeJourneyDamage(1.4f,Element.Fire);}else{e.chargeUntil=0;e.recoveryUntil=clock+1.1f;}}
   else if(e.windup>0){e.windup-=dt;if(e.windup<=0){if(e.warning!=null)Destroy(e.warning);ResolveCoreMove(e);}}
   else if(clock>=e.recoveryUntil){bool visible=!ProjectilePathBlocked(e.position,position,.1f);if(visible&&clock>=e.nextAttack&&to.magnitude<(e.boss?14:8)){
    e.attackNumber++;e.attackDirection=to.sqrMagnitude>.01f?to.normalized:Vector2.down;e.strikePoint=position;e.windup=e.boss?1.1f:.9f;e.nextAttack=clock+e.windup+(e.boss?1.6f:1.45f);
    e.warning=CoreWarning(e);
   }else if(!visible||to.magnitude>(e.boss?4:5))MoveIcePursuit(e,e.speed*(clock<e.iceSlowUntil?.65f:1)*dt);}
   PoseWorkshopEnemy(e,old,dt);
  }
  private GameObject CoreFanWarning(Enemy e,Vector2 aim,int count,float span){return EnemyMuzzleTell(e.position,aim);}
  private GameObject CoreWarning(Enemy e)
  {
   if(e.kind==EnemyKind.AshPackbeast)return ChapterLineWarning(e.position,e.attackDirection,4.8f,1.1f);
   if(!e.boss)return CoreFanWarning(e,e.attackDirection,5,64);
   int move=e.attackNumber%3;if(move==2)return WarningCircle(e.strikePoint,1.05f);
   if(move==0)return CoreFanWarning(e,e.attackDirection,e.hp<=e.maximum/3f?24:e.secondPhase?22:20,154);
   return EnemyMuzzleTell(e.position,e.attackDirection);
  }
  private void CoreFan(Enemy e,Vector2 aim,int count,float span,float speed)
  {
   for(int i=0;i<count;i++){Vector2 d=Quaternion.Euler(0,0,(i/(float)(count-1)-.5f)*span)*aim;CoreBullet(e,d,speed);}
  }
  private void CoreBullet(Enemy e,Vector2 direction,float speed)
  {
   int before=shots.Count;SpawnShot(e.position+direction*(e.radius+.25f),direction,true,null,e.position);if(shots.Count==before)return;
   Shot s=shots[shots.Count-1];s.emitter=e;s.element=Element.Fire;s.damage=e.boss?1.6f:1.1f;s.speed=speed;s.life=4.5f;s.root.GetComponent<SpriteRenderer>().color=C("ffb76b");
  }
  private void ResolveCoreMove(Enemy e)
  {
   e.recoveryUntil=clock+(e.boss?1.4f:.7f);
   if(e.kind==EnemyKind.AshPackbeast){e.chargeUntil=clock+.6f;return;}
   if(!e.boss){CoreFan(e,e.attackDirection,5,64,4.4f);QueueFireBomb(e.strikePoint,1.15f,.65f);return;}
   bool last=e.hp<=e.maximum/3f;int count=last?24:e.secondPhase?22:20;int move=e.attackNumber%3;
   if(move==0){CoreFan(e,e.attackDirection,count,154,4.3f);QueueCoreBurst(e,e.attackDirection,count,clock+.65f);}
   else if(move==1){int ring=last?36:e.secondPhase?32:28;for(int i=0;i<ring;i++){Vector2 d=Quaternion.Euler(0,0,i*360f/ring)*Vector2.right;if(Vector2.Dot(d,e.attackDirection)>.707f)continue;CoreBullet(e,d,3.8f);}QueueCoreBurst(e,-e.attackDirection,last?14:10,clock+.8f);}
   else{QueueFireBomb(e.strikePoint,1.05f,1.05f);Vector2 side=new Vector2(-e.attackDirection.y,e.attackDirection.x);QueueFireBomb(FindFreeInteractionPoint(e.strikePoint+side*2.3f),1.55f,.8f);CoreFan(e,e.attackDirection,last?15:11,120,4.1f);}
  }
  private void QueueCoreBurst(Enemy e,Vector2 aim,int count,float when){coreBursts.Add(new CoreBurst{owner=e,aim=aim,count=count,when=when,warning=CoreFanWarning(e,aim,count,154)});}
  private void TickCoreBursts(){for(int i=coreBursts.Count-1;i>=0;i--){CoreBurst b=coreBursts[i];if(b.owner.hp<=0||!enemies.Contains(b.owner)){if(b.warning!=null)Destroy(b.warning);coreBursts.RemoveAt(i);continue;}if(clock<b.when)continue;if(b.warning!=null)Destroy(b.warning);CoreFan(b.owner,b.aim,b.count,154,4.7f);coreBursts.RemoveAt(i);}}
  private void ClearCoreBursts(){foreach(CoreBurst b in coreBursts)if(b.warning!=null)Destroy(b.warning);coreBursts.Clear();}
  private void DropBossEnergy(Enemy e)
  {
   if(!enableDesertJourney||!e.boss||e.bossEnergyReleased||e.hp<=0||e.hp>e.maximum/3f)return;
   e.bossEnergyReleased=true;Vector2 at=FindFreeInteractionPoint(Vector2.MoveTowards(e.position,position,2.2f));
   var crystal=new StoredEssence{at=at,value=40,born=clock};journeyRooms[currentRoom].essence.Add(crystal);DrawEssence(crystal);
   AddFloatingHit("元素补给 +40",at+Vector2.up*.7f,C("7be6d0"));
  }
 }
}
