using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private Element IslandLootElement {get{return CurrentIsland.ordinal==1?Element.Wind:CurrentIsland.ordinal==2?Element.Lightning:CurrentIsland.ordinal==3?Element.Ice:CurrentIsland.ordinal==4?Element.Fire:Element.Wood;}}
  private bool IsIslandThemedWeapon(int id){if(id<0||id>=weaponCatalog.Length)return false;WeaponDef w=weaponCatalog[id];if(capitalMode||manorCourtyardMode)return w.element==Element.Physical||OriginWeaponAllowed(w)&&(w.pool!=PowerPool.Elemental||w.element==(origin==Origin.Dark?Element.Dark:Element.Light));return w.element==IslandLootElement&&w.pool==PowerPool.Elemental;}
  private bool IsOriginSupplyWeapon(int id){return id>=0&&id<weaponCatalog.Length&&weaponCatalog[id].pool!=PowerPool.Elemental&&OriginWeaponAllowed(weaponCatalog[id])&&WeaponSourceIsland(id)<=CurrentIsland.ordinal;}
  private void RollOptionalOriginSupply(System.Random rng,RoomNode room,List<int> selected)
  {
   // Keep two local-element choices and a local preferred category; rare main-path weapons can occupy the third.
   if(room.kind!=RoomKind.Reward||rng.Next(100)>=18)return;var pool=SupplyCandidates().FindAll(id=>IsOriginSupplyWeapon(id)&&SupplyCategory(id)==SupplyCategory(selected[2]));if(pool.Count>0)selected[2]=pool[rng.Next(pool.Count)];
  }
  private int RollIslandEliteWeapon(RoomNode room)
  {return RollSupplyCategory(new System.Random(activeSeed^room.id*1907^room.stage*317^0x454c),ProfessionSupplyCategory,new List<int>(),new HashSet<int>());}
  private void DropIslandEliteReward(RoomNode room)
  {
   if(room.kind!=RoomKind.Elite||room.lootSeeded)return;room.lootSeeded=true;int id=RollIslandEliteWeapon(room);ChapterDrop(id,FindFreeInteractionPoint(new Vector2(0,-1)));
   var essence=new StoredEssence{at=FindFreeInteractionPoint(new Vector2(2,.5f)),value=35,born=clock};room.essence.Add(essence);DrawEssence(essence);AnnounceDiscovery("精英战利品 · "+weaponCatalog[id].name);
  }
  private bool OwnsIslandElement()
  {foreach(int id in carriedWeapons)if(IsIslandThemedWeapon(id))return true;foreach(int id in reserveWeapons)if(IsIslandThemedWeapon(id))return true;return false;}
  // A one-time explicit choice for existing cleared saves that left the fire island without fire equipment.
  private bool DepartureArsenalAvailable {get{return fireChapterMode&&currentRoom==0&&chapterEnding&&!journeyRooms[0].departureArmoryClaimed&&!OwnsIslandElement();}}
  private void BuildDepartureArmory()
  {if(DepartureArsenalAvailable)AddChapterAction(920,FindFreeInteractionPoint(new Vector2(3.8f,1.4f)),"领取炉匠的火器军械 · 三类自选",WorldPropSprite(6));}
 }
}
