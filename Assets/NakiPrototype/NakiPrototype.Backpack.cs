using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private bool inventoryMenu;private int bagSelection,healingSupplies,elementSupplies;
  private readonly List<int> reserveWeapons=new List<int>();
  private WeaponDrop pendingWeaponPickup;
  private void ResetBackpack(){inventoryMenu=false;pendingWeaponPickup=null;bagSelection=0;reserveWeapons.Clear();healingSupplies=1;elementSupplies=1;}
  private bool HandleBackpackInput()
  {
   if(StoryActive||showRainCG||rewardMenu||mapMenu)return false;
   if(inventoryMenu)
   {
    if(pendingWeaponPickup==null&&Pressed('v')){OpenVoyagerJournal();return true;}
    if(Pressed('b')||Pressed('e')){inventoryMenu=false;pendingWeaponPickup=null;return true;}
    if(Pressed('a'))bagSelection=(bagSelection+5)%6;if(Pressed('d'))bagSelection=(bagSelection+1)%6;
    if(Pressed('f')){if(pendingWeaponPickup!=null)ReplaceWithPendingPickup();else EquipBagWeapon(equippedSlot);}
    if(pendingWeaponPickup==null){if(Pressed('g'))DropBagWeapon();if(Pressed('1'))UseSupply(0);if(Pressed('2'))UseSupply(1);}return true;
   }
   if(health>0&&Pressed('b')){inventoryMenu=true;bagSelection=equippedSlot;return true;}return false;
  }
  private int BagWeaponId(int slot){return slot<2?carriedWeapons[slot]:slot-2<reserveWeapons.Count?reserveWeapons[slot-2]:-1;}
  private void EquipBagWeapon(int slot)
  {
   RequestCheckpoint();int id=BagWeaponId(bagSelection);if(id<0)return;
   if(bagSelection<2){int old=carriedWeapons[slot];carriedWeapons[slot]=id;carriedWeapons[bagSelection]=old;}
   else{int old=carriedWeapons[slot];carriedWeapons[slot]=id;if(old<0)reserveWeapons.RemoveAt(bagSelection-2);else reserveWeapons[bagSelection-2]=old;}
   equippedSlot=slot;swordHitPending=false;swordUntil=castPoseUntil=clock;nextSword=Mathf.Max(nextSword,clock+.15f);JourneyHint("已装备 "+CurrentWeapon.name);
  }
  private void DropBagWeapon()
  {
   RequestCheckpoint();int id=BagWeaponId(bagSelection);if(id<0)return;
   if(bagSelection<2){if(carriedWeapons[1-bagSelection]<0){JourneyHint("至少保留一把武器");return;}carriedWeapons[bagSelection]=-1;equippedSlot=1-bagSelection;}
   else reserveWeapons.RemoveAt(bagSelection-2);
   StoreGroundWeapon(id);swordHitPending=false;swordUntil=castPoseUntil=clock;
  }
  private void StoreGroundWeapon(int id)
  {var data=new StoredWeapon{id=id,at=FindFreeInteractionPoint(position+Vector2.right*.65f)};journeyRooms[currentRoom].drops.Add(data);CreateWeaponDropVisual(data);}
  private void ReplaceWithPendingPickup()
  {
   if(pendingWeaponPickup==null||bagSelection<0||bagSelection>=6)return;
   WeaponDrop drop=pendingWeaponPickup;RoomNode node=journeyRooms[currentRoom];
   if(!worldWeapons.Contains(drop)||!node.drops.Contains(drop.data)){pendingWeaponPickup=null;return;}
   int old=BagWeaponId(bagSelection);
   if(bagSelection<2)carriedWeapons[bagSelection]=drop.data.id;
   else if(bagSelection-2<reserveWeapons.Count)reserveWeapons[bagSelection-2]=drop.data.id;
   else if(reserveWeapons.Count<4)reserveWeapons.Add(drop.data.id);else return;
   node.drops.Remove(drop.data);worldWeapons.Remove(drop);Destroy(drop.root);if(old>=0)StoreGroundWeapon(old);
   pendingWeaponPickup=null;inventoryMenu=false;CancelBowCharge();swordHitPending=false;swordUntil=castPoseUntil=clock;nextSword=clock+.2f;RequestCheckpoint();

  }
  private void UseSupply(int kind)
  {
   if(health<=0)return;RequestCheckpoint();
   if(kind==0){if(healingSupplies<=0||health>=6){JourneyHint(health>=6?"生命已满":"没有清泉药剂");return;}healingSupplies--;health=Mathf.Min(6,health+2);JourneyHint("清泉药剂 · 恢复2生命");}
   else{if(elementSupplies<=0||elementPower>=ElementMaximum){JourneyHint(elementPower>=ElementMaximum?"元素之力已满":"没有元素药剂");return;}elementSupplies--;elementPower=Mathf.Min(ElementMaximum,elementPower+40);JourneyHint("元素药剂 · 恢复40元素之力");}
  }
  private bool FlatButton(Rect rect,string label,bool selected=false)
  {
   HudRect(new Rect(rect.x-1,rect.y-1,rect.width+2,rect.height+2),selected?C("d5bd87"):C("3d5961"));HudRect(rect,selected?C("294e4d"):C("182e39"));
   FittedLabel(new Rect(rect.x+10,rect.y+6,rect.width-20,rect.height-12),label,journeyTextStyle,true,TextAnchor.MiddleCenter);return GUI.Button(rect,GUIContent.none,GUIStyle.none);
  }
  private string ElementName(Element e){switch(e){case Element.Wind:return "风";case Element.Light:return "光";case Element.Lightning:return "雷";case Element.Fire:return "火";case Element.Ice:return "冰";case Element.Wood:return "木";case Element.Dark:return "贤者之石";default:return "物理";}}
  private string WeaponSummary(WeaponDef weapon)
  {return ElementName(weapon.element)+" · "+WeaponDamageLabel(weapon)+" · "+WeaponCostLabel(weapon)+(weapon.element==Element.Fire?" · 火克木×1.8":"");}
  private void DrawSpriteUI(Rect area,Sprite sprite)
  {
   if(sprite==null)return;Rect r=sprite.rect;float scale=Mathf.Min(area.width/r.width,area.height/r.height);Rect box=new Rect(area.center.x-r.width*scale/2,area.center.y-r.height*scale/2,r.width*scale,r.height*scale);
   GUI.DrawTextureWithTexCoords(box,sprite.texture,new Rect(r.x/sprite.texture.width,r.y/sprite.texture.height,r.width/sprite.texture.width,r.height/sprite.texture.height));
  }
  private void DrawBackpack(float w,float h)
  {
   Rect panel=new Rect(w/2-450,h/2-285,900,570);HudRect(panel,new Color(.035f,.065f,.09f,.985f));
   GUI.Label(new Rect(panel.x+28,panel.y+18,620,35),"行囊 · 双武器与备用装备",journeyTitleStyle);
   if(pendingWeaponPickup==null&&FlatButton(new Rect(panel.x+674,panel.y+18,168,35),"V 航海手记")){OpenVoyagerJournal();return;}
   FittedLabel(new Rect(panel.x+28,panel.y+61,840,26),ActivePowerLabel+(origin==Origin.Dark?" · 无上限":" /100")+"     元素之力 "+elementPower.ToString("0")+" /"+ElementMaximum+"     护盾上限 "+EffectiveMaximumShield,journeySmallStyle);
   bool wasEnabled=GUI.enabled;
   for(int i=0;i<6;i++)
   {
    Rect card=i<2?new Rect(panel.x+28,panel.y+102+i*120,270,108):new Rect(panel.x+322+(i-2)%2*274,panel.y+102+(i-2)/2*120,253,108);
    int id=BagWeaponId(i);bool selected=i==bagSelection;
    if(FlatButton(card,"",selected))bagSelection=i;
    FittedLabel(new Rect(card.x+12,card.y+9,card.width-24,26),(i<2?(i==equippedSlot?"◆ 当前 ":"携带 ")+(i+1):"备用 "+(i-1))+"  "+(id<0?"空位":weaponCatalog[id].name),journeyTextStyle);
    if(id>=0){DrawSpriteUI(new Rect(card.x+14,card.y+44,62,46),weaponCatalog[id].sprite);FittedLabel(new Rect(card.x+90,card.y+43,card.width-102,24),ElementName(weaponCatalog[id].element)+" · "+WeaponDamageLabel(weaponCatalog[id]),journeySmallStyle);FittedLabel(new Rect(card.x+90,card.y+72,card.width-102,24),"消耗 "+WeaponCostLabel(weaponCatalog[id]),journeySmallStyle);}
   }
   if(pendingWeaponPickup!=null)
   {
    WeaponDef incoming=weaponCatalog[pendingWeaponPickup.data.id];int old=BagWeaponId(bagSelection);
    FittedLabel(new Rect(panel.x+28,panel.y+343,840,32),"拾取："+incoming.name+" · "+WeaponSummary(incoming),journeyTextStyle);
    FittedLabel(new Rect(panel.x+28,panel.y+386,840,42),"选择上方任意格子替换；原武器会放回地上。当前使用的武器栏保持不变。",journeySmallStyle,true);
    if(FlatButton(new Rect(panel.x+28,panel.y+445,500,54),"F "+(old<0?"放入所选空位":"替换所选："+weaponCatalog[old].name)))ReplaceWithPendingPickup();
    if(FlatButton(new Rect(panel.x+550,panel.y+445,290,54),"取消 · 留在地上")){pendingWeaponPickup=null;inventoryMenu=false;}
    GUI.Label(new Rect(panel.x+28,panel.y+523,844,26),"A / D 或点击选择格子 · F 确认 · B / ESC 取消；战斗已暂停",journeySmallStyle);
    GUI.enabled=wasEnabled;return;
   }
   int selectedId=BagWeaponId(bagSelection);
   if(selectedId>=0)
   {
    WeaponDef weapon=weaponCatalog[selectedId];FittedLabel(new Rect(panel.x+28,panel.y+337,500,38),weapon.name+" · "+WeaponSummary(weapon),journeyTextStyle,true);
    GUI.Label(new Rect(panel.x+28,panel.y+376,500,57),"间隔 "+weapon.interval.ToString("0.00")+"秒 · 射程 "+weapon.range.ToString("0.#")+" · 自动瞄准\n"+(weapon.type==WeaponType.Bow?"满蓄 "+(BowReleaseDamage(weapon,1)/BowReleaseDamage(weapon,0)).ToString("0.#")+"倍 · K 保留蓄力 · "+(weapon.cost>0?"额外1"+WeaponPoolName(weapon):"不消耗能量"):weapon.type==WeaponType.Staff?StaffVolleyCount(weapon)+" 弹 · 每弹"+(weapon.damage*StaffPelletScale(weapon,StaffVolleyCount(weapon))*WeaponOutputScale(weapon)).ToString("0.#")+"伤害 · 每轮付费一次":"近身锁敌，伤害遵循武器范围"),journeySmallStyle);
    if(selectedId>=123||weapon.effect!=WeaponEffect.None){HudRect(new Rect(panel.x+26,panel.y+373,510,60),new Color(.035f,.065f,.09f,1));FittedLabel(new Rect(panel.x+28,panel.y+376,500,57),selectedId>=123?OriginWeaponDescription(selectedId):RelicDescription(weapon),journeySmallStyle,true);}
    if(FlatButton(new Rect(panel.x+550,panel.y+345,130,45),"装备到 1"))EquipBagWeapon(0);
    if(FlatButton(new Rect(panel.x+704,panel.y+345,130,45),"装备到 2"))EquipBagWeapon(1);
    if(FlatButton(new Rect(panel.x+704,panel.y+400,130,34),"G 丢弃"))DropBagWeapon();
   }
   if(FlatButton(new Rect(panel.x+28,panel.y+445,300,54),"1 清泉药剂 ×"+healingSupplies+" · 恢复2生命"))UseSupply(0);
   if(FlatButton(new Rect(panel.x+350,panel.y+445,300,54),"2 元素药剂 ×"+elementSupplies+" · 元素+40"))UseSupply(1);
   if(origin!=Origin.Unchosen&&SafeCheckpointRoom&&FlatButton(new Rect(panel.x+550,panel.y+400,130,34),"主属性战技")){inventoryMenu=false;originAbilityMenu=true;}
   if(FlatButton(new Rect(panel.x+674,panel.y+445,168,54),"T 元素技能")){inventoryMenu=false;chapterSkillsMenu=true;}
   GUI.Label(new Rect(panel.x+28,panel.y+523,844,26),TalentSummary(),journeySmallStyle);
   GUI.enabled=wasEnabled;

  }
 }
}
