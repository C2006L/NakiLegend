using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private bool IsHammer {get{return (CurrentWeapon.name.Contains("锤")||CurrentWeapon.name.Contains("槌"));}}
  private void AddArsenal()
  {
   if(weaponCatalog.Length!=23)return;
   var all=new List<WeaponDef>(weaponCatalog);
   all.Add(Weapon("巡风双刃",WeaponType.Sword,Element.Wind,13,.22f,1.35f,1,.9f,weaponCatalog[13].sprite,"b0f5df"));
   all.Add(Weapon("霜脊穿甲弩",WeaponType.Bow,Element.Ice,20,.85f,11,2,1.1f,weaponCatalog[15].sprite,"a5e5ff"));
   all.Add(Weapon("回响雷杖",WeaponType.Staff,Element.Lightning,13,.75f,10,2,1.3f,weaponCatalog[9].sprite,"a3dfff"));
   all.Add(Weapon("震炉战锤",WeaponType.Sword,Element.Fire,34,1.05f,1.9f,3,1.2f,weaponCatalog[11].sprite,"ffc18c"));
   weaponCatalog=all.ToArray();weaponCatalog[23].maxHits=3;weaponCatalog[26].maxHits=4;
  }
  private void ArsenalMeleeImpact()
  {
   IdentityMeleeEffect();OriginMeleeWave();RelicMeleeImpact();if(CurrentWeapon.name!="震炉战锤")return;
   SkillSound("meteor","hit");SpellSpark(position+swingDirection*.9f,C("ffbc80"),.65f);
   foreach(Enemy e in enemies)if(Vector2.Distance(e.position,position)<2.3f&&!ProjectilePathBlocked(position,e.position,.04f))e.iceSlowUntil=Mathf.Max(e.iceSlowUntil,clock+.65f);
  }
  private void ArsenalShot(Shot shot)
  {
   ConfigureRelicShot(shot,CurrentWeapon);
   if(CurrentWeapon.name=="霜脊穿甲弩")shot.hitsLeft=bowShotCharge>=.75f?3:2;
   if(CurrentWeapon.name=="回响雷杖")shot.chapterArc=true;
   if(CurrentWeapon.name=="冰轨重弩")shot.hitsLeft=bowShotCharge>=.75f?4:2;
   if(CurrentWeapon.name=="炉膛连弩"||CurrentWeapon.name=="竹节排弩")shot.hitsLeft=bowShotCharge>=.75f?2:1;
   if(CurrentWeapon.type==WeaponType.Staff&&CurrentWeapon.element==Element.Wood)shot.skillAudio="spore";
  }
 }
}
