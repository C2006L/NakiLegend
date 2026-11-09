using System;
using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private enum WeaponEffect { None,WaterPush,WindBurst,Venom,RockShatter,SideArrows,RingReturn,LightningEnd,MarkChain,IceImpact,MirrorBounce,TidalSlow,FireThrust,ForgePull,MagmaCore,ThornBurst,Spore,BloomRoot,Quicksand,MovingBow,Needle,QuickCrossbow,OwlBow,CrystalShatter,FlameSpray,PressureBolt,SeedBurst,VineTether }
  private const int RelicStart=72,ExpandedWeaponCount=135;
  private Sprite RelicSprite(int index,WeaponType type)
  {
   if(index==16)return IslandWeaponSprite(7);
   var t=Resources.Load<Texture2D>("NakiArt/LegacyV1/boss-weapons-spaced");if(t==null)return weaponCatalog[type==WeaponType.Bow?12:type==WeaponType.Staff?3:1].sprite;
   float cell=t.width/4f;Rect rect=new Rect(index%4*cell,(3-index/4)*cell,cell,cell);
   Sprite s=Sprite.Create(t,rect,new Vector2(type==WeaponType.Bow&&(index==4||index==10)?.5f:.2f,.5f),cell,0,SpriteMeshType.FullRect);ownedAssets.Add(s);return s;
  }
  private void AddRelicCatalog()
  {
   if(weaponCatalog==null||weaponCatalog.Length!=42)return;var items=new List<WeaponDef>(weaponCatalog);
   string[] normal={"流沙杖","驼铃短弓","引雷针杖","铜簧速弩","雪鸮弓","碎晶杖","灰烬喷杖","炉压弩","荚果杖","藤索弩"};
   WeaponEffect[] effects={WeaponEffect.Quicksand,WeaponEffect.MovingBow,WeaponEffect.Needle,WeaponEffect.QuickCrossbow,WeaponEffect.OwlBow,WeaponEffect.CrystalShatter,WeaponEffect.FlameSpray,WeaponEffect.PressureBolt,WeaponEffect.SeedBurst,WeaponEffect.VineTether};
   for(int i=0;i<10;i++){
    int island=i/2+1;WeaponType type=i==1||i==3||i==4||i==7||i==9?WeaponType.Bow:WeaponType.Staff;
    Element element=island==1?Element.Wind:island==2?Element.Lightning:island==3?Element.Ice:island==4?Element.Fire:Element.Wood;
    int spriteIndex=island==4?i==6?0:2:island==3?i==4?4:3:island==5?i==8?6:8:0;
    Sprite sprite=(i<4?RelicSprite(new[]{0,4,7,6}[i],type):IslandWeaponSprite(spriteIndex))??weaponCatalog[type==WeaponType.Bow?8:3].sprite;
    AddWeaponFamily(items,normal[i],type,element,type==WeaponType.Staff?21:24,i==3?.35f:i==6?.22f:type==WeaponType.Staff?.72f:.8f,i==3?1:2,sprite,effects[i],island,false);
   }
   string[] bosses={"回泉铜杖","风铃连弩","钩尾毒弩","砂心重杖","逐风战弓","铜环双刃","镇雷重弩","角灯灵杖","裂霜重槌","镜花法杖","潮息长弓","赤脊长枪","锻链弩","熔心法杖","棘冠连弩","菌冠法杖","新生长弓"};
   WeaponType[] types={WeaponType.Staff,WeaponType.Bow,WeaponType.Bow,WeaponType.Staff,WeaponType.Bow,WeaponType.Sword,WeaponType.Bow,WeaponType.Staff,WeaponType.Sword,WeaponType.Staff,WeaponType.Bow,WeaponType.Spear,WeaponType.Bow,WeaponType.Staff,WeaponType.Bow,WeaponType.Staff,WeaponType.Bow};
   for(int i=0;i<17;i++){
    int island=i<5?1:i<8?2:i<11?3:i<14?4:5;Element element=i==2?Element.Wood:island==1?Element.Wind:island==2?Element.Lightning:island==3?Element.Ice:island==4?Element.Fire:Element.Wood;
    float damage=types[i]==WeaponType.Sword||types[i]==WeaponType.Spear?35:types[i]==WeaponType.Staff?25:28;
    AddWeaponFamily(items,bosses[i],types[i],element,damage,types[i]==WeaponType.Staff?.8f:.9f,2,RelicSprite(i,types[i]),(WeaponEffect)(i+1),island,true);
   }
   AppendOriginWeapons(items);AppendManorWeapon(items);weaponCatalog=items.ToArray();if(weaponCatalog.Length!=ExpandedWeaponCount)throw new InvalidOperationException("Invalid relic catalog size");ApplyWeaponIdentities();
  }
  private void AddWeaponFamily(List<WeaponDef> list,string name,WeaponType type,Element element,float damage,float interval,float cost,Sprite sprite,WeaponEffect effect,int island,bool signature)
  {
   int root=list.Count;
   for(int v=0;v<3;v++){
    WeaponDef w=Weapon(name+(v==1?"·散华":v==2?"·凝聚":""),type,element,damage*(v==1?.8f:v==2?1.2f:1),interval*(v==2?1.2f:1),type==WeaponType.Spear?2.6f:type==WeaponType.Sword?1.8f:11,(effect==WeaponEffect.FlameSpray?1:cost)+(v==2?1:0),type==WeaponType.Staff?1.45f:1.3f,sprite,"ffffff");
    w.effect=effect;w.variant=v;w.family=root;w.sourceIsland=island;w.signature=signature;w.maxHits=type==WeaponType.Sword||type==WeaponType.Spear?(v==1?4:3):1;if(v==1&&(type==WeaponType.Sword||type==WeaponType.Spear))w.range+=.3f;w.scatter=type==WeaponType.Bow?1.2f:.8f;list.Add(w);
   }
  }
  private int SignatureFamily(RoomNode n)
  {
   if(CurrentIsland.id==IslandId.Desert)return n.kind==RoomKind.Guardian?n.stage:2+selectedBoss;
   int[] group={5,8,11,14};return group[Mathf.Clamp(CurrentIsland.ordinal-2,0,3)]+Mathf.Clamp(n.stage,0,2);
  }
  private void RewardBossWeapon(RoomNode n)
  {
   if(capitalMode){if(n.kind==RoomKind.Boss&&!n.bossWeaponGranted&&thunderChapterPracticeBoss<0){n.bossWeaponGranted=true;ChapterDrop(RollIslandEliteWeapon(n),FindSafeLootPoint(new Vector2(2,-1)));healingSupplies++;AnnounceDiscovery("王都军械与清泉药剂 +1");}return;}
   if(manorCourtyardMode){if(n.kind==RoomKind.Boss&&!n.bossWeaponGranted&&thunderChapterPracticeBoss<0){n.bossWeaponGranted=true;ChapterDrop(n.id==14?132:n.id==29?133:134,new Vector2(2,-1));}return;}
   if(n.bossWeaponGranted||n.kind!=RoomKind.Boss&&n.kind!=RoomKind.Guardian||bossPracticeMode||thunderChapterMode&&thunderChapterPracticeBoss>=0||gatehouseSampleMode||lightningTrialMode)return;
   n.bossWeaponGranted=true;var rng=new System.Random(activeSeed^n.id*433^0x5381);int id=RelicStart+SignatureFamily(n)*3+rng.Next(3);
   Vector2 at=FindFreeInteractionPoint(new Vector2(1.8f,-1.4f));var drop=new StoredWeapon{id=id,at=at};n.drops.Add(drop);CreateWeaponDropVisual(drop);
   healingSupplies++;AnnounceDiscovery("首领馈赠 · "+weaponCatalog[id].name+" · 清泉药剂 +1");
  }
  private int WeaponFamily(int id){return id>=42?weaponCatalog[id].family:id;}
  private int SupplyCategory(int id){WeaponType t=weaponCatalog[id].type;return t==WeaponType.Sword||t==WeaponType.Spear?0:t==WeaponType.Staff?1:2;}
  private int ProfessionSupplyCategory {get{return profession==Profession.Mage?1:profession==Profession.Archer?2:0;}}
  private int RollSupplyCategory(System.Random rng,int category,List<int> selected,HashSet<int> previous)
  {
   var families=new HashSet<int>();foreach(int id in selected)families.Add(WeaponFamily(id));
   var pool=new List<int>();foreach(int id in SupplyCandidates())if(SupplyCategory(id)==category&&IsIslandThemedWeapon(id)&&!families.Contains(WeaponFamily(id)))pool.Add(id);
   var owned=new HashSet<int>();foreach(int id in carriedWeapons)if(id>=0)owned.Add(WeaponFamily(id));foreach(int id in reserveWeapons)owned.Add(WeaponFamily(id));
   var upgrades=pool.FindAll(id=>!owned.Contains(WeaponFamily(id))&&id!=0);if(upgrades.Count>0)pool=upgrades;
   var fresh=pool.FindAll(id=>!previous.Contains(WeaponFamily(id)));if(fresh.Count>=2)pool=fresh;
   if(pool.Count==0)throw new InvalidOperationException("Missing supply weapon category "+category);
   return pool[rng.Next(pool.Count)];
  }
  private int[] RollSupplyOffers(RoomNode n)
  {
   var rng=new System.Random(activeSeed^n.id*1103515245^0x6d29);var selected=new List<int>();
   var previous=new HashSet<int>();foreach(RoomNode old in journeyRooms)if(old.id!=n.id&&old.claimed&&old.supplyOffers!=null)foreach(int id in old.supplyOffers)previous.Add(WeaponFamily(id));
   if(n.supplyOffers!=null&&n.supplyOffers.Length==3){
    if(n.claimed)return n.supplyOffers; // Completed choices and ground rewards never change.
    int[] counts=new int[3];foreach(int id in n.supplyOffers)counts[SupplyCategory(id)]++;
    for(int missing=0;missing<3;missing++)if(counts[missing]==0){
     for(int slot=2;slot>=0;slot--)if(counts[SupplyCategory(n.supplyOffers[slot])]>1){
      selected.Clear();for(int j=0;j<3;j++)if(j!=slot)selected.Add(n.supplyOffers[j]);
      counts[SupplyCategory(n.supplyOffers[slot])]--;n.supplyOffers[slot]=RollSupplyCategory(rng,missing,selected,previous);counts[missing]++;RequestCheckpoint();break;
     }
    }
    for(int slot=0;slot<3;slot++)if(!IsIslandThemedWeapon(n.supplyOffers[slot])&&(slot!=2||!IsOriginSupplyWeapon(n.supplyOffers[slot]))){selected.Clear();for(int j=0;j<3;j++)if(j!=slot)selected.Add(n.supplyOffers[j]);n.supplyOffers[slot]=RollSupplyCategory(rng,SupplyCategory(n.supplyOffers[slot]),selected,previous);RequestCheckpoint();}
    return n.supplyOffers;
   }
   int preferred=ProfessionSupplyCategory;int[] order={preferred,(preferred+1)%3,(preferred+2)%3};if(rng.Next(2)==0){int swap=order[1];order[1]=order[2];order[2]=swap;}
   for(int slot=0;slot<3;slot++){
    selected.Add(RollSupplyCategory(rng,order[slot],selected,previous));
   }
   RollOptionalOriginSupply(rng,n,selected);n.supplyOffers=selected.ToArray();return n.supplyOffers;
  }
  private int WeaponSourceIsland(int id){if(id>=42)return weaponCatalog[id].sourceIsland;if(id<8)return 0;if(id<=13||id==25)return 2;if(id<=17||id==24||id>=36&&id<=38)return 3;if(id<=22||id==26||id>=33&&id<=35)return 4;if(id>=27)return 5;return 0;}
  private List<int> SupplyCandidates()
  {
   var list=new List<int>();int island=CurrentIsland.ordinal;
   for(int id=0;id<weaponCatalog.Length;id++){
    WeaponDef w=weaponCatalog[id];if(w.signature||WeaponSourceIsland(id)>island||!OriginWeaponAllowed(w))continue;
    if(id>=42){int weight=w.sourceIsland==island?12:3;for(int k=0;k<weight;k++)list.Add(id);}
    else if(id<8){for(int k=0;k<3;k++)list.Add(id);}
    else{bool local=island==2&&id>=8&&id<=13||island==3&&(id>=14&&id<=17||id>=36&&id<=38)||island==4&&(id>=18&&id<=22||id>=33&&id<=35)||island==5&&(id>=27&&id<=32||id>=39&&id<=41);for(int k=0;k<(local?10:1);k++)list.Add(id);}
   }return list;
  }
  private string RelicDescription(WeaponDef w)
  {
   string[] text={"","水纹弹 · 命中推开轻敌","三段风矢 · 连续射击","毒箭 · 短时持续伤害","石弹碰撞碎裂","蓄力主箭 · 两侧风矢","近身连斩 · 回旋环刃","蓄力贯穿 · 末端雷闪","连续命中引爆雷印","重击释放冰刺","冰弹最多反弹一次","潮汐箭 · 减速敌群","突刺带出短火浪","链箭拉近轻型敌人","命中熔核 · 延迟爆开","扇形棘刺 · 近射集中","孢弹扩散短时毒伤","蓄力束根 · 牵制轻敌","落点流沙牵引","移动后首箭增伤","高速雷针贯穿","较快装填 · 低单发伤害","满蓄多箭集中","冰晶命中后碎裂","近距离火焰散射","蓄压重矢 · 击退贯穿","荚果弹碰撞裂开","藤箭减速与短暂束缚"};
   bool melee=w.type==WeaponType.Sword||w.type==WeaponType.Spear;
   return text[(int)w.effect]+(w.variant==1?(melee?" · 扩大覆盖":" · 散射型"):w.variant==2?(melee?" · 重击型":w.type==WeaponType.Staff?" · 单发穿透":" · 重铸贯穿"):"");
  }
  private bool HandleSupplyMenu()
  {
   if(!chapterRewardMenu)return false;
   if(Pressed('a'))chapterRewardChoice=(chapterRewardChoice+3)%4;if(Pressed('d'))chapterRewardChoice=(chapterRewardChoice+1)%4;
   if(Pressed('f'))ClaimIslandSupply(chapterRewardChoice);if(Pressed('e'))chapterRewardMenu=false;return true;
  }
  private bool ValidSupplyOffers(int[] ids)
  {if(ids==null||ids.Length==0)return true;if(ids.Length!=3)return false;var seen=new HashSet<int>();foreach(int id in ids)if(id<0||id>=weaponCatalog.Length||!seen.Add(WeaponFamily(id)))return false;return true;}
 }
}
