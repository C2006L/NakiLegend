using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private float WeaponOutputScale(WeaponDef weapon){return (weapon.type==WeaponType.Staff?StaffTrainingOutput:PhysicalTrainingOutput)[CombatMasteryTier]*(weapon.type==WeaponType.Staff?.88f:weapon.type==WeaponType.Sword||weapon.type==WeaponType.Spear?1.15f:1);}
  private float WeaponImpactScale(int id,Element attack,Element target)
  {
   float scale=id>=0&&id<weaponCatalog.Length?WeaponOutputScale(weaponCatalog[id]):1.5f;
   // Weapon counterplay has its own budget. Elemental skills retain their existing 1.3x rule.
   if(attack==Element.Fire&&target==Element.Wood)scale*=1.8f/ElementMultiplier(attack,target);
   return scale;
  }
  private float MeleeDamageScale(WeaponDef weapon)
  {
   if(weapon.type==WeaponType.Spear)return 1.12f;
   if(weapon.type!=WeaponType.Sword)return 1;
   return weapon.name.Contains("锤")||weapon.name.Contains("槌")?1.22f:1.15f;
  }
  private float BowReleaseDamage(WeaponDef weapon,float charge)
  {
   if(weapon.type!=WeaponType.Bow)return weapon.damage;
   charge=Mathf.Clamp01(charge);
   return weapon.damage*(2f/3)*(1+(weapon.name.Contains("弩")?4.5f:4)*charge*charge);
  }
  private float BowCardDamage(WeaponDef weapon,float charge)
  {
   float damage=BowReleaseDamage(weapon,charge);
   if(weapon.effect==WeaponEffect.PressureBolt)damage*=1.15f;
   if(weapon.variant==1)damage*=.7f;
   if(weaponCatalog!=null&&System.Array.IndexOf(weaponCatalog,weapon)==128)damage*=.6f;
   return damage;
  }
  private bool LootFooting(Vector2 at)
  {
   if(float.IsNaN(at.x)||float.IsNaN(at.y)||float.IsInfinity(at.x)||float.IsInfinity(at.y))return false;
   Rect room=CurrentRoomBounds;
   return at.x>=room.xMin+.65f&&at.x<=room.xMax-.65f&&at.y>=room.yMin+.65f&&at.y<=room.yMax-.65f
    &&!IntersectsTransit(at,Vector2.one*.7f)&&SafeFooting(at,.28f);
  }
  private Vector2 FindSafeLootPoint(Vector2 preferred)
  {
   if(LootFooting(preferred))return preferred;
   Rect room=CurrentRoomBounds;Vector2 desired=new Vector2(
    float.IsNaN(preferred.x)||float.IsInfinity(preferred.x)?0:Mathf.Clamp(preferred.x,room.xMin+.7f,room.xMax-.7f),
    float.IsNaN(preferred.y)||float.IsInfinity(preferred.y)?0:Mathf.Clamp(preferred.y,room.yMin+.7f,room.yMax-.7f));
   Vector2 best=Vector2.zero;float score=float.MaxValue;
   for(float y=room.yMin+.7f;y<=room.yMax-.7f;y+=.35f)for(float x=room.xMin+.7f;x<=room.xMax-.7f;x+=.35f){
    Vector2 at=new Vector2(x,y);float d=(at-desired).sqrMagnitude;if(d<score&&LootFooting(at)){score=d;best=at;}
   }
   // Very small authored rooms may reserve their whole floor for interactions.
   // Relax only that reservation, never the floor boundary or physical collision.
   if(score==float.MaxValue)for(float y=room.yMin+.7f;y<=room.yMax-.7f;y+=.35f)for(float x=room.xMin+.7f;x<=room.xMax-.7f;x+=.35f){
    Vector2 at=new Vector2(x,y);float d=(at-desired).sqrMagnitude;if(d<score&&SafeFooting(at,.28f)){score=d;best=at;}
   }
   if(score==float.MaxValue)throw new System.InvalidOperationException("No safe loot footing in room "+currentRoom);
   return best;
  }
 }
}
