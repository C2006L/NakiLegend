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
    if(Pressed('b')||Pressed('e')){inventoryMenu=false;pendingWeaponPickup=null;return true;}
    if(Pressed('a'))bagSelection=(bagSelection+5)%6;if(Pressed('d'))bagSelection=(bagSelection+1)%6;
    if(Pressed('f')){if(pendingWeaponPickup!=null)ReplaceWithPendingPickup();else EquipBagWeapon(equippedSlot);}
    if(Pressed('g'))DropBagWeapon();if(Pressed('1'))UseSupply(0);if(Pressed('2'))UseSupply(1);return true;
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
   if(pendingWeaponPickup==null)return;WeaponDrop drop=pendingWeaponPickup;
   int old=carriedWeapons[equippedSlot];carriedWeapons[equippedSlot]=drop.data.id;journeyRooms[currentRoom].drops.Remove(drop.data);worldWeapons.Remove(drop);Destroy(drop.root);if(old>=0)StoreGroundWeapon(old);
   pendingWeaponPickup=null;inventoryMenu=false;swordHitPending=false;swordUntil=castPoseUntil=clock;nextSword=clock+.2f;
  }
  private void UseSupply(int kind)
  {
   if(health<=0)return;RequestCheckpoint();
   if(kind==0){if(healingSupplies<=0||health>=6){JourneyHint(health>=6?"生命已满":"没有清泉药剂");return;}healingSupplies--;health=Mathf.Min(6,health+2);JourneyHint("清泉药剂 · 恢复2生命");}
   else{if(elementSupplies<=0||elementPower>=100){JourneyHint(elementPower>=100?"元素之力已满":"没有元素药剂");return;}elementSupplies--;elementPower=Mathf.Min(100,elementPower+40);JourneyHint("元素药剂 · 恢复40元素之力");}
  }
  private bool FlatButton(Rect rect,string label,bool selected=false)
  {
   HudRect(new Rect(rect.x-1,rect.y-1,rect.width+2,rect.height+2),selected?C("dec28b"):C("526c70"));HudRect(rect,selected?C("355e59"):C("192c36"));
   GUI.Label(rect,label,new GUIStyle(journeyTextStyle){alignment=TextAnchor.MiddleCenter,wordWrap=true});return GUI.Button(rect,GUIContent.none,GUIStyle.none);
  }
  private string ElementName(Element e){switch(e){case Element.Wind:return "风";case Element.Light:return "光";case Element.Lightning:return "雷";case Element.Fire:return "火";case Element.Ice:return "冰";case Element.Wood:return "木";case Element.Dark:return "暗";default:return "物理";}}
  private string WeaponSummary(WeaponDef weapon)
  {return ElementName(weapon.element)+" · 伤害 "+weapon.damage.ToString("0")+" · "+weapon.cost.ToString("0")+" 元素之力";}
  private void DrawSpriteUI(Rect area,Sprite sprite)
  {
   if(sprite==null)return;Rect r=sprite.rect;float scale=Mathf.Min(area.width/r.width,area.height/r.height);Rect box=new Rect(area.center.x-r.width*scale/2,area.center.y-r.height*scale/2,r.width*scale,r.height*scale);
   GUI.DrawTextureWithTexCoords(box,sprite.texture,new Rect(r.x/sprite.texture.width,r.y/sprite.texture.height,r.width/sprite.texture.width,r.height/sprite.texture.height));
  }
  private void DrawBackpack(float w,float h)
  {
   Rect panel=new Rect(w/2-450,h/2-285,900,570);HudRect(panel,new Color(.035f,.065f,.09f,.985f));
   GUI.Label(new Rect(panel.x+28,panel.y+18,840,35),"行囊 · 双武器与备用装备",journeyTitleStyle);
   GUI.Label(new Rect(panel.x+28,panel.y+61,840,26),"光之力 "+energy.ToString("0")+" /100     元素之力 "+elementPower.ToString("0")+" /100     暗之力 "+darkPower.ToString("0")+" /100",journeySmallStyle);
   bool wasEnabled=GUI.enabled;if(pendingWeaponPickup!=null)GUI.enabled=false;
   for(int i=0;i<6;i++)
   {
    Rect card=i<2?new Rect(panel.x+28,panel.y+102+i*115,265,99):new Rect(panel.x+322+(i-2)%2*274,panel.y+102+(i-2)/2*115,253,99);
    int id=BagWeaponId(i);bool selected=i==bagSelection;
    if(FlatButton(card,"",selected))bagSelection=i;
    GUI.Label(new Rect(card.x+10,card.y+7,card.width-20,26),(i<2?(i==equippedSlot?"◆ 当前 ":"携带 ")+(i+1):"备用 "+(i-1))+"  "+(id<0?"空位":weaponCatalog[id].name),journeyTextStyle);
    if(id>=0){DrawSpriteUI(new Rect(card.x+12,card.y+39,72,46),weaponCatalog[id].sprite);GUI.Label(new Rect(card.x+94,card.y+43,card.width-100,40),ElementName(weaponCatalog[id].element)+" · 伤害 "+weaponCatalog[id].damage+"\n消耗 "+weaponCatalog[id].cost+" 元素之力",new GUIStyle(journeySmallStyle){wordWrap=true});}
   }
   int selectedId=BagWeaponId(bagSelection);
   if(selectedId>=0)
   {
    WeaponDef weapon=weaponCatalog[selectedId];GUI.Label(new Rect(panel.x+28,panel.y+340,500,30),weapon.name+" · "+WeaponSummary(weapon),journeyTextStyle);
    GUI.Label(new Rect(panel.x+28,panel.y+376,500,57),"间隔 "+weapon.interval.ToString("0.00")+"秒 · 射程 "+weapon.range.ToString("0.#")+" · 自动瞄准\n"+(weapon.type==WeaponType.Bow||weapon.type==WeaponType.Staff?"散布 ±"+weapon.scatter+"°":"近身锁敌，伤害遵循武器范围"),journeySmallStyle);
    if(FlatButton(new Rect(panel.x+550,panel.y+345,130,45),"装备到 1"))EquipBagWeapon(0);
    if(FlatButton(new Rect(panel.x+704,panel.y+345,130,45),"装备到 2"))EquipBagWeapon(1);
    if(FlatButton(new Rect(panel.x+704,panel.y+400,130,34),"G 丢弃"))DropBagWeapon();
   }
   if(FlatButton(new Rect(panel.x+28,panel.y+445,300,54),"1 清泉药剂 ×"+healingSupplies+" · 恢复2生命"))UseSupply(0);
   if(FlatButton(new Rect(panel.x+350,panel.y+445,300,54),"2 元素药剂 ×"+elementSupplies+" · 元素+40"))UseSupply(1);
   GUI.Label(new Rect(panel.x+28,panel.y+523,844,26),"A / D 选装备 · F 装备至当前栏 · G 丢弃 · B / ESC 返回；菜单中暂停战斗",journeySmallStyle);
   GUI.enabled=wasEnabled;
   if(pendingWeaponPickup!=null)
   {
    Rect box=new Rect(w/2-355,h/2-120,710,240);HudRect(box,new Color(.045f,.09f,.12f,.99f));WeaponDef item=weaponCatalog[pendingWeaponPickup.data.id];
    GUI.Label(new Rect(box.x+25,box.y+20,660,38),"背包已满 · "+item.name,journeyTitleStyle);GUI.Label(new Rect(box.x+25,box.y+72,660,28),WeaponSummary(item),journeyTextStyle);
    if(FlatButton(new Rect(box.x+25,box.y+142,310,55),"F 替换当前："+CurrentWeapon.name))ReplaceWithPendingPickup();
    if(FlatButton(new Rect(box.x+365,box.y+142,310,55),"留在地上")){pendingWeaponPickup=null;inventoryMenu=false;}
   }
  }
 }
}
