using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private sealed class WeaponZone {public Vector2 at;public float when,until,next,radius,damage;public Element element;public bool pull;public GameObject root;}
  private sealed class WeaponVolley {public Vector2 at,aim;public float when,damage;public Element element;public WeaponEffect effect;public int identityWeapon=-1;}
  private readonly List<WeaponZone> weaponZones=new List<WeaponZone>();
  private readonly List<WeaponVolley> weaponVolleys=new List<WeaponVolley>();
  private void ConfigureRelicShot(Shot s,WeaponDef w)
  {
   s.weaponEffect=w.effect;s.weaponVariant=w.variant;s.identityWeapon=ArrayWeaponId(w);
   if(w.effect==WeaponEffect.Venom||w.effect==WeaponEffect.Spore)s.skillAudio="spore";
   if(w.effect==WeaponEffect.Needle){s.speed=14;s.hitsLeft=3;}
   if(w.effect==WeaponEffect.PressureBolt){s.hitsLeft=2;s.damage*=1.15f;}
   if(w.effect==WeaponEffect.LightningEnd)s.hitsLeft=bowShotCharge>=.75f?4:2;
   if(w.effect==WeaponEffect.MovingBow&&clock-lastMovementAt<.8f)s.damage*=1.25f;
   if(w.effect==WeaponEffect.FlameSpray){s.life=.45f;s.speed=7;s.root.transform.localScale=Vector3.one*.2f;}
   if(w.type!=WeaponType.Bow)return;
   if(w.variant==2){s.hitsLeft++;s.speed*=1.15f;}
   if(w.variant==1||w.effect==WeaponEffect.SideArrows&&bowShotCharge>=.75f||w.effect==WeaponEffect.OwlBow&&bowShotCharge>=.9f||w.effect==WeaponEffect.ThornBurst){
    float angle=w.effect==WeaponEffect.OwlBow?5:w.effect==WeaponEffect.ThornBurst?18:12;
    s.damage*=w.variant==1?.7f:1;
    foreach(int sign in new[]{-1,1})RelicBullet(s.position,Quaternion.Euler(0,0,sign*angle)*s.direction,s.damage*.45f,s.element,s.speed,s.life,WeaponEffect.None,s.identityWeapon);
   }
   if(w.effect==WeaponEffect.WindBurst){for(int i=1;i<=2;i++)weaponVolleys.Add(new WeaponVolley{at=s.position,aim=s.direction,when=clock+i*.12f,damage=s.damage*.6f,element=s.element,effect=WeaponEffect.None,identityWeapon=s.identityWeapon});}
  }
  private float lastMovementAt;
  private void RelicBullet(Vector2 at,Vector2 direction,float damage,Element element,float speed,float life,WeaponEffect effect,int identityWeapon=-1)
  {
   if(ProjectilePathBlocked(at,at,.06f))return;int before=shots.Count;SpawnShot(at,direction,false,null,at);if(shots.Count==before)return;
   Shot s=shots[shots.Count-1];s.damage=damage;s.element=element;s.speed=speed;s.life=life;s.tracking=false;s.weaponEffect=effect;s.root.transform.localScale=Vector3.one*.13f;s.root.GetComponent<SpriteRenderer>().color=SpellTint(element);AddShotRibbon(s,SpellTint(element),.02f);StyleIdentityProjectile(s,identityWeapon);
  }
  private void RelicHit(Shot s,Enemy target,Vector2 contact)
  {
   if(s.hostile||s.weaponEffect==WeaponEffect.None)return;
   if(s.weaponEffect==WeaponEffect.Venom||s.weaponEffect==WeaponEffect.Spore){target.woodPoisonUntil=clock+3;target.woodPoisonNext=Mathf.Max(target.woodPoisonNext,clock+.7f);}
   if(s.weaponEffect==WeaponEffect.ForgePull&&target.boss)target.relicVulnerableUntil=clock+1;
   if(s.weaponEffect==WeaponEffect.WaterPush||s.weaponEffect==WeaponEffect.PressureBolt){if(!target.boss)target.position=MoveEnemyWithinRoom(target.position,s.direction*.5f,target.radius);}
   if(s.weaponEffect==WeaponEffect.TidalSlow||s.weaponEffect==WeaponEffect.BloomRoot||s.weaponEffect==WeaponEffect.VineTether)target.iceSlowUntil=Mathf.Max(target.iceSlowUntil,clock+(target.boss?.3f:1.4f));
   if(s.weaponEffect==WeaponEffect.BloomRoot||s.weaponEffect==WeaponEffect.VineTether){if(!target.boss){target.tacticControlUntil=clock+(target.elite?.2f:.5f);target.chargeUntil=clock;target.windup=0;if(target.warning!=null)Destroy(target.warning);target.recoveryUntil=Mathf.Max(target.recoveryUntil,clock+.5f);}}
   if(s.weaponEffect==WeaponEffect.ForgePull&&!target.boss){Vector2 toward=(position-target.position).normalized;Vector2 next=MoveEnemyWithinRoom(target.position,toward*.8f,target.radius);if(!ProjectilePathBlocked(target.position,next,target.radius))target.position=next;}
   if(s.weaponEffect==WeaponEffect.MarkChain){if(target.relicMarkUntil>clock){target.relicMarkUntil=0;Enemy other=null;foreach(Enemy e in enemies)if(e!=target&&e.hp>0&&Vector2.Distance(e.position,contact)<4&&!ProjectilePathBlocked(contact,e.position,.06f)){other=e;break;}if(other!=null){LightningSegment(contact,other.position,.035f,C("acf0ff"),.2f);ApplyElementDamage(other,s.damage*.65f,Element.Lightning);}}else target.relicMarkUntil=clock+3;}
   if(s.weaponEffect==WeaponEffect.MagmaCore||s.weaponEffect==WeaponEffect.Quicksand){QueueWeaponZone(contact,s.weaponEffect==WeaponEffect.Quicksand,s.damage*.6f,s.element);}
   if(s.weaponEffect==WeaponEffect.LightningEnd)SpellSpark(contact,SpellTint(Element.Lightning),.35f);
   ResolveRelicImpact(s,contact);
  }
  private void ResolveRelicImpact(Shot s,Vector2 contact)
  {
   if(s.effectResolved||s.hostile)return;s.effectResolved=true;
   if(s.weaponEffect==WeaponEffect.RockShatter||s.weaponEffect==WeaponEffect.CrystalShatter||s.weaponEffect==WeaponEffect.SeedBurst){for(int i=-1;i<=1;i++){Vector2 d=Quaternion.Euler(0,0,i*50)*(-s.direction);RelicBullet(contact+d*.18f,d,s.damage*.35f,s.element,6,.6f,WeaponEffect.None,s.identityWeapon);}}
   if(s.weaponEffect==WeaponEffect.LightningEnd){foreach(Enemy e in new List<Enemy>(enemies))if(e.hp>0&&Vector2.Distance(e.position,contact)<.75f&&!ProjectilePathBlocked(contact,e.position,.03f))ApplyElementDamage(e,s.damage*.4f,Element.Lightning);}
   if(s.weaponEffect==WeaponEffect.Spore){foreach(Enemy e in new List<Enemy>(enemies))if(e.hp>0&&Vector2.Distance(e.position,contact)<1.1f&&!ProjectilePathBlocked(contact,e.position,.03f)){e.woodPoisonUntil=Mathf.Max(e.woodPoisonUntil,clock+3);e.woodPoisonNext=clock+.7f;}}
  }
  private bool RelicRicochet(Shot s,Vector2 previous,float wall)
  {
   if(s.hostile||s.weaponEffect!=WeaponEffect.MirrorBounce||s.ricochets>0||float.IsPositiveInfinity(wall))return false;
   Vector2 d=s.direction;foreach(Rect b in projectileObstacles){Rect r=new Rect(b.xMin-.12f,b.yMin-.12f,b.width+.24f,b.height+.24f);if(!r.Contains(s.position))continue;if(previous.x<r.xMin||previous.x>r.xMax)d.x=-d.x;else d.y=-d.y;break;}
   if(d==s.direction)d=-d;Vector2 next=s.position+d*.06f;if(ProjectilePathBlocked(next,next,.1f))return false;
   s.direction=d;s.position=next;s.ricochets++;SpellSpark(next,C("c3eeff"),.14f);return true;
  }
  private void QueueWeaponZone(Vector2 at,bool pull,float damage,Element element)
  {
   if(weaponZones.Count>=5)return;float radius=pull?1.1f:.85f;
   GameObject root=Draw(pull?"Friendly quicksand":"Friendly molten core",windRingSprite??disc,at,Vector2.one*radius*2,new Color(pull?.78f:1,pull?.67f:.55f,.28f,.65f),-620);
   weaponZones.Add(new WeaponZone{at=at,when=clock+.5f,until=clock+(pull?1.5f:.6f),next=clock+.5f,radius=radius,damage=damage,element=element,pull=pull,root=root});
  }
  private void TickRelicCombat()
  {
   foreach(Enemy e in new List<Enemy>(enemies))if(e.hp>0&&clock<e.woodPoisonUntil&&clock>=e.woodPoisonNext){e.woodPoisonNext=clock+.7f;DamageEnemy(e,SkillDamage(3.5f));}
   for(int i=weaponVolleys.Count-1;i>=0;i--){var v=weaponVolleys[i];if(clock<v.when)continue;RelicBullet(v.at,v.aim,v.damage,v.element,11,1,v.effect,v.identityWeapon);weaponVolleys.RemoveAt(i);}
   for(int i=weaponZones.Count-1;i>=0;i--){var z=weaponZones[i];if(clock>=z.next){z.next=clock+.25f;foreach(Enemy e in new List<Enemy>(enemies))if(e.hp>0&&Vector2.Distance(e.position,z.at)<z.radius+e.radius&&!ProjectilePathBlocked(z.at,e.position,.04f)){if(z.pull){if(!e.boss)e.position=MoveEnemyWithinRoom(e.position,(z.at-e.position).normalized*.18f,e.radius);}else ApplyElementDamage(e,z.damage,z.element);}}
    if(clock>=z.until){if(z.root!=null)Destroy(z.root);weaponZones.RemoveAt(i);}}
  }
  private void ClearRelicCombat(){whisperHits.Clear();soulCrossMarks.Clear();foreach(var z in weaponZones)if(z.root!=null)Destroy(z.root);weaponZones.Clear();weaponVolleys.Clear();}
  private void RelicMeleeImpact()
  {
   WeaponDef w=CurrentWeapon;if(w.effect==WeaponEffect.RingReturn){int before=shots.Count;RelicBullet(position+swingDirection*.5f,swingDirection,w.damage*.5f,w.element,7,1.7f,WeaponEffect.None,HeldWeaponIdentity);if(shots.Count>before)shots[shots.Count-1].returnBlade=true;}
   if(w.effect==WeaponEffect.IceImpact||w.effect==WeaponEffect.FireThrust){for(int i=-1;i<=1;i++){Vector2 d=Quaternion.Euler(0,0,i*18)*swingDirection;RelicBullet(position+d*.6f,d,w.damage*.3f,w.element,8,.5f,WeaponEffect.None,HeldWeaponIdentity);}}
  }
 }
}
