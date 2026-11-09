using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private void AddIslandRangedWeapons()
  {
   if(weaponCatalog==null||weaponCatalog.Length!=33)return;
   var items=new List<WeaponDef>(weaponCatalog);
   string[] names={"熔晶引火杖","烬羽长弓","炉膛连弩","霜花枝杖","白桦霜弓","冰轨重弩","新芽灵杖","青叶猎弓","竹节排弩"};
   float[] damage={23,27,18,21,26,29,23,25,18},interval={.72f,.85f,.55f,.7f,.82f,.95f,.68f,.78f,.58f};
   for(int i=0;i<9;i++){
    int type=i%3;Element element=i<3?Element.Fire:i<6?Element.Ice:Element.Wood;
    Sprite art=IslandWeaponSprite(i)??weaponCatalog[type==0?3:type==1?12:8].sprite;
    items.Add(Weapon(names[i],type==0?WeaponType.Staff:WeaponType.Bow,element,damage[i],interval[i],type==0?10:11,type==2&&i!=5?1:2,type==0?1.5f:type==1?1.25f:1.35f,art,"ffffff"));
   }
   weaponCatalog=items.ToArray();for(int i=33;i<42;i++)weaponCatalog[i].scatter=i%3==2?2.2f:.7f;
   // Existing island equipment keeps its identity and stats, but also receives the
   // themed art, so an old checkpoint's first supply chest does not look generic.
   int[,] visuals={{21,33},{20,35},{16,36},{15,38},{24,38},{28,39},{31,40},{27,41}};
   for(int i=0;i<visuals.GetLength(0);i++){WeaponDef w=weaponCatalog[visuals[i,0]];w.sprite=weaponCatalog[visuals[i,1]].sprite;w.color=Color.white;w.bowFacesLeft=false;}
  }
  private Sprite IslandWeaponSprite(int id)
  {
   var t=Resources.Load<Texture2D>("NakiArt/IslandArmory/ranged-atlas");if(t==null)return null;t.filterMode=FilterMode.Point;
   int column=id%3,row=id/3;float[] edges={0,535f/1254,800f/1254,1},rows={0,408f/1254,824f/1254,1};
   Rect rect=new Rect(edges[column]*t.width,(1-rows[row+1])*t.height,(edges[column+1]-edges[column])*t.width,(rows[row+1]-rows[row])*t.height);
   var sprite=Sprite.Create(t,rect,new Vector2(column==0?.18f:column==1?.55f:.35f,.5f),column==1?rect.height:rect.width,0,SpriteMeshType.FullRect);ownedAssets.Add(sprite);return sprite;
  }
  private string IslandCacheName {get{return capitalMode?"王都封存军械 · 按职业自选":DepartureArsenalAvailable?"炉匠的离岛军械 · 火属性自选":fireChapterMode?"炉匠补给箱":woodlandMode?"护林补给箱":iceSampleMode?"破冰队补给箱":"巡路补给箱";}}
  private int[] IslandRewardChoices()
  {
   return RollSupplyOffers(journeyRooms[currentRoom]);
  }
  private string IslandWeaponFeature(WeaponDef w)
  {
   if(w.effect!=WeaponEffect.None)return RelicDescription(w);
   if(w.type==WeaponType.Sword)return "近战斩击 · 可切换方向反击";
   if(w.type==WeaponType.Spear)return "近战突刺 · 较长攻击距离";
   if(w.type==WeaponType.Staff)return "三发散射 · 自动瞄准"+(w.element==Element.Wood?" · 短时毒伤":"");
   if(w.name.Contains("弩"))return w.name=="炉膛连弩"||w.name=="竹节排弩"?"蓄力穿透 · 较快装填":w.name=="冰轨重弩"||w.name=="霜脊穿甲弩"?"蓄力重矢 · 穿透目标":"蓄力重矢 · 直线射击";
   return "蓄力增伤 · 自动瞄准";
  }
  private void ClaimIslandSupply(int choice)
  {
   RoomNode n=journeyRooms[currentRoom];bool departure=DepartureArsenalAvailable;if((!departure&&(n.kind!=RoomKind.Reward||n.claimed))||choice<0||choice>3)return;if(departure&&choice==3){chapterRewardMenu=false;return;}
   if(departure)n.departureArmoryClaimed=true;else n.claimed=n.lootSeeded=true;chapterRewardMenu=false;
   if(choice==3){health=Mathf.Min(6,health+2);shield=Mathf.Min(EffectiveMaximumShield,shield+2);elementPower=Mathf.Min(ElementMaximum,elementPower+35);Sfx("recover",false,.5f);JourneyHint("补给已使用 · 生命 +2，护盾 +2，元素 +35");}
   else{ChapterDrop(IslandRewardChoices()[choice],FindFreeInteractionPoint(position+Vector2.right*.8f));JourneyHint("装备已放在身旁 · F 拾取，可替换任意武器格");}
   if(rewardChest!=null){rewardChest.SetActive(false);Destroy(rewardChest);}rewardChest=null;if(departure)BuildChapterActions();BlockWorldInteraction();RequestCheckpoint();SaveChapterCheckpoint(false);
  }
  private void DrawIslandSupply(float w,float h)
  {
   Rect panel=new Rect(w/2-475,h/2-260,950,520);HudRect(panel,C("10232d"));
   CenterLabel(new Rect(panel.x+25,panel.y+18,900,40),IslandCacheName+" · 选择一份",journeyTitleStyle);
   CenterLabel(new Rect(panel.x+25,panel.y+61,900,30),DepartureArsenalAvailable?"炉匠：带一件火器再启程；近战、法杖与弓弩任选其一":"取走装备，或把补给留给自己；每箱只能领取一次",journeySmallStyle);
   int[] ids=IslandRewardChoices();
   for(int i=0;i<4;i++){
    Rect card=new Rect(panel.x+25+(i%2)*455,panel.y+105+(i/2)*166,440,153);
    if(FlatButton(card,"",chapterRewardChoice==i)){ClaimIslandSupply(i);return;}
    if(i<3){WeaponDef weapon=weaponCatalog[ids[i]];DrawSupplyWeaponIcon(weapon,card);
     FittedLabel(new Rect(card.x+118,card.y+15,310,32),(SupplyCategory(ids[i])==0?"近战 · ":SupplyCategory(ids[i])==1?"法杖 · ":"弓弩 · ")+weapon.name,journeyTextStyle);
     FittedLabel(new Rect(card.x+118,card.y+52,310,42),WeaponSummary(weapon),journeySmallStyle,true);
     FittedLabel(new Rect(card.x+118,card.y+99,310,40),IslandWeaponFeature(weapon),journeySmallStyle,true);
    }else if(DepartureArsenalAvailable){CenterLabel(new Rect(card.x+18,card.y+48,404,50),"稍后再领 · 保留本次机会",journeyTextStyle);}
    else{CenterLabel(new Rect(card.x+18,card.y+13,404,32),"草药与护具补给",journeyTextStyle);
     CenterLabel(new Rect(card.x+18,card.y+48,404,55),"生命 +2 · 护盾 +2\n元素之力 +35",journeyTextStyle);
     CenterLabel(new Rect(card.x+18,card.y+112,404,26),"立即恢复 · 不占武器格",journeySmallStyle);}
   }
   CenterLabel(new Rect(panel.x+25,panel.y+452,900,44),"A / D 选择 · F 确认 · ESC 返回（不消耗本箱）",journeySmallStyle);
  }
  private void DrawSupplyWeaponIcon(WeaponDef weapon,Rect card)
  {
   Rect target=new Rect(card.x+12,card.y+35,92,72);HudRect(new Rect(card.x+10,card.y+20,96,112),C("12202a"));
   Rect r=weapon.sprite.rect;float aspect=r.width/r.height;Rect fit=target;if(aspect>target.width/target.height){fit.height=target.width/aspect;fit.y+=(target.height-fit.height)/2;}else{fit.width=target.height*aspect;fit.x+=(target.width-fit.width)/2;}
   Color before=GUI.color;GUI.color=weapon.color;GUI.DrawTextureWithTexCoords(fit,weapon.sprite.texture,new Rect(r.x/weapon.sprite.texture.width,r.y/weapon.sprite.texture.height,r.width/weapon.sprite.texture.width,r.height/weapon.sprite.texture.height));GUI.color=before;
  }
 }
}
