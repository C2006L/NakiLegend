using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private void AddWoodWeapons()
  {
   if(weaponCatalog==null||weaponCatalog.Length<27||weaponCatalog.Length>=33)return;
   var items=new System.Collections.Generic.List<WeaponDef>(weaponCatalog);
   items.Add(Weapon("竹节轻弩",WeaponType.Bow,Element.Physical,18,.6f,10,1,1.1f,weaponCatalog[8].sprite,"c3dea1"));
   items.Add(Weapon("木髓枝杖",WeaponType.Staff,Element.Wood,24,.65f,10,2,1.3f,weaponCatalog[9].sprite,"a9d28b"));
   items.Add(Weapon("盘根长枪",WeaponType.Spear,Element.Wood,34,.6f,2.6f,1,1.85f,weaponCatalog[10].sprite,"c4d493"));
   items.Add(Weapon("朽芽重锤",WeaponType.Sword,Element.Wood,42,1.0f,1.9f,2,1.3f,weaponCatalog[11].sprite,"b5bc83"));
   items.Add(Weapon("青脉藤弓",WeaponType.Bow,Element.Wood,24,.8f,11,2,1.1f,weaponCatalog[12].sprite,"a6d7a3"));
   items.Add(Weapon("采枝短刀",WeaponType.Sword,Element.Wood,24,.28f,1.5f,1,1.05f,weaponCatalog[13].sprite,"d1e4ac"));
   weaponCatalog=items.ToArray();weaponCatalog[27].scatter=2;weaponCatalog[29].maxHits=3;weaponCatalog[30].maxHits=4;weaponCatalog[31].scatter=.7f;
  }
  private void MigrateOldWoodWeapons(ChapterCheckpoint s)
  {
   if(s.player==null||s.rooms==null)return;
   if(s.player.carried!=null)for(int i=0;i<s.player.carried.Length;i++)s.player.carried[i]=OldWoodWeapon(s.player.carried[i]);
   if(s.player.reserves!=null)for(int i=0;i<s.player.reserves.Length;i++)s.player.reserves[i]=OldWoodWeapon(s.player.reserves[i]);
   foreach(var room in s.rooms)if(room!=null&&room.weapons!=null)foreach(var w in room.weapons)if(w!=null)w.id=OldWoodWeapon(w.id);
  }
  private static int OldWoodWeapon(int id){return id>=8&&id<=13?id+19:id;}
  private void RequestWoodReturn()
  {
   if(!woodlandMode||!WorldInteractionReady||!SafeCheckpointRoom||StoryActive||chapterCg)return;
   pendingIceVoyage=509;voyageArmed=false;voyageOpened=Time.unscaledTime;CancelBowCharge();
  }
  private void ReturnWoodlandToFire()
  {
   if(!woodlandMode||!SaveChapterCheckpoint(false))return;
   var player=CaptureChapterPlayer();int[] ledger=CopyTalents();woodlandMode=false;fireChapterMode=true;iceSampleMode=false;chapterCheckpointPathOverride=null;
   if(LoadVoyageChapter(ledger,player))return;
   fireChapterMode=false;woodlandMode=true;JourneyHint("没有可继续的赤炉群岛安全点，已留在青栖林海；当前青栖林海进度已保存");
  }
  private string VoyageTitle {get{return pendingIceVoyage==724?"归航 · 返回本土王都":pendingIceVoyage==509?"返回上一座岛：赤炉群岛":pendingIceVoyage==507?"前往下一座岛：青栖林海":pendingIceVoyage==304?"返回上一座岛：雷雨高原":"前往下一座岛：赤炉群岛";}}
  private string VoyageDescription {get{return pendingIceVoyage==724?"潮水已经转向，岸边的船等着最后一名归来者。少年带上六片源石、旧村的家书与王室原令，向王都归航港启程。\n暮钟岛进度会保存，装备与技能配置随身；若已到过王都，将继续那里的安全点。":pendingIceVoyage==509?"青栖林海进度与天赋会保留。这是回访路线，将继续赤炉群岛自己的安全点；找不到存档时会留在青栖林海。":pendingIceVoyage==507?"赤炉群岛进度和天赋将保留。首次登青栖林海继承装备、火与冰技能；已有青栖林海进度则继续其安全点。":pendingIceVoyage==304?"这是返航路线，将读取雷雨高原的安全点。霜潮群岛进度和已学天赋会保留。":"这是继续主线的航程。已有赤炉群岛进度将继续读取，首次登岛会继承当前装备和天赋。";}}
  private string VoyageStay {get{return pendingIceVoyage==724?"暂留暮钟岛 · ESC":pendingIceVoyage==509?"留在雾岸码头 · ESC":pendingIceVoyage==507?"留在黑砂码头 · ESC":"留在冰港 · ESC";}}
 }
}
