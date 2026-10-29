using System;
using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  [Header("Woodland chapter")] public bool woodlandMode;
  private string[] WoodlandStages={"雾岸林缘","朽木腹地","树冠旧庭"};
  private EnemyKind[] WoodKinds={EnemyKind.ChargeMoth,EnemyKind.CopperRoller,EnemyKind.SpearGuard,EnemyKind.Tumbleweed,EnemyKind.SandScorpion,EnemyKind.ThunderStone,EnemyKind.StoneSentinel,EnemyKind.SandKite,EnemyKind.StormGuard,EnemyKind.CopperWarden,EnemyKind.ThunderSentinel,EnemyKind.StormStag,EnemyKind.WoodlandDeer};
  private string[] WoodNames={"针尾蜂","棘甲虫","裂荚株","匍藤猎手","灰孢团","苔背盾卫","腐叶蜈","吞苔蛙","引荧螟","棘甲母虫","菌冠孢囊","古树守望者","寄枝鹿"};
  private int ArrayIndexWood(EnemyKind kind){return Array.IndexOf(WoodKinds,kind);}
  private void EmbarkWoodland()
  {
   SaveChapterCheckpoint(false);var player=CaptureChapterPlayer();Origin previous=origin;
   woodlandMode=true;thunderChapterStartFloor=0;thunderChapterPracticeBoss=-1;chapterCheckpointPathOverride=null;
   if(LoadChapterCheckpoint(false))return;
   BeginThunderChapter(false);ApplyChapterPlayer(player);origin=previous;shield=Mathf.Min(EffectiveMaximumShield,shield+1);EnterExplorationRoom(0,-1);
   TellStory("巡路员递来一块求援木牌：邻岛的种子已经多年不发芽。","船穿过晨雾，停在青栖林海的旧栈桥旁。阿苓在码头等着。");RequestCheckpoint();
  }
  private void ConfigureWoodEnemy(Enemy e)
  {
   int k=ArrayIndexWood(e.kind);if(k<0)return;e.affinity=Element.Wood;e.attackElement=Element.Wood;
   float[] hp={38,80,52,62,36,100,64,70,44,560,900,1700,96};e.hp=e.maximum=hp[k];e.root.transform.localScale=Vector3.one;
   e.speed=k==2?.15f:k==5?.7f:k>=9&&k<=11?1:1.3f;e.root.name=WoodNames[k];
  }
  private void CastWoodSkill(int slot)
  {
   int skill=chapterSkillSlots[slot];if(skill==0){CastWindBlade();return;}if(skill==1){CastWindWave();return;}
   if(!ChapterSkillKnown(skill)||clock<ChapterSkillReady(skill))return;
   Enemy target;Vector2 aim=AutoAim(9,9,0,out target);if(target==null)return;
   if(!SpendPower(PowerPool.Elemental,skill==2?7:9))return;
   SkillSound(skill==2?"root":"spore","cast");
   if(skill==2){chapterMarkReady=clock+1.6f;ApplySkillHit(target,SkillDamage(24),Element.Wood,"root");target.recoveryUntil=Mathf.Max(target.recoveryUntil,clock+.6f);SpellSpark(target.position,C("a4e589"),.7f);}
   else{chapterArcReady=clock+2.4f;int before=shots.Count;SpawnShot(position+aim*.4f,aim,false,target,position);if(shots.Count==before)return;Shot shot=shots[shots.Count-1];shot.element=Element.Wood;shot.skillAudio="spore";shot.damage=SkillDamage(28);shot.tracking=true;shot.speed=7;shot.root.GetComponent<SpriteRenderer>().color=C("b7d37b");AddShotRibbon(shot,C("8bbf69"),.07f);}
  }
  private void DecorateWoodCover(Vector2 at)
  {
   Sprite[] art=enemyActionArt[EnemyKind.StormStag];Draw("Old hollow tree",art[0],at,new Vector2(.55f,.8f),new Color(.65f,.78f,.65f),DepthAt(at.y));
   obstacles.Add(new Rect(at.x-.55f,at.y-.2f,1.1f,.45f));projectileObstacles.Add(new Rect(at.x-.55f,at.y-.2f,1.1f,1.1f));
  }
  private void BuildWoodGraph()
  {
   journeyRooms.Clear();chapterDoors.Clear();chapterNames=new string[45];
   int[][] ids={new[]{0,1,2,3,4,5,7,8,9,10,11,39,40,41,6},new[]{12,13,14,15,16,17,19,20,21,22,23,24,42,43,18},new[]{25,26,27,28,29,30,32,33,34,35,36,37,38,44,31}};
   for(int i=0;i<45;i++)journeyRooms.Add(null);
   for(int floor=0;floor<3;floor++)for(int i=0;i<15;i++)
   {
    int id=ids[floor][i];var n=new RoomNode{id=id,stage=floor,floor=floor,depth=floor*4,cell=new Vector2Int(i%5,i/5),halfWidth=8+floor,halfHeight=4.5f+floor*.5f,layout=(i+activeSeed)%6,waves=2+floor,stairsTo=-1,kind=i==0?RoomKind.Rest:i==14?RoomKind.Boss:i==7?RoomKind.Elite:i==4||i==10?RoomKind.Event:RoomKind.Combat};
    if(i%5>0)n.links.Add(ids[floor][i-1]);if(i%5<4)n.links.Add(ids[floor][i+1]);if(i>=5&&i%2==0)n.links.Add(ids[floor][i-5]);if(i<10&&(i+5)%2==0)n.links.Add(ids[floor][i+5]);
    journeyRooms[id]=n;chapterNames[id]=WoodlandStages[floor]+" · "+(i==0?"营地":i==14?WoodNames[9+floor]:i==4?"旧种床":i==10?"采种小径":"林径 "+(i+1));
   }
   journeyRooms[6].stairsTo=12;journeyRooms[12].stairsTo=6;journeyRooms[18].stairsTo=25;journeyRooms[25].stairsTo=18;
   stageStarts[0]=0;stageStarts[1]=12;stageStarts[2]=25;stageEnds[0]=6;stageEnds[1]=18;stageEnds[2]=31;
  }
  private void LoadWoodArt()
  {
   Texture2D t=Resources.Load<Texture2D>("NakiArt/Woodland/creatures");if(t==null)throw new InvalidOperationException("Missing woodland creature atlas");t.filterMode=FilterMode.Point;
   for(int i=0;i<13;i++)
   {
    int tileIndex=i==9?1:i==12?9:i;Rect cell=new Rect((tileIndex%4)*t.width/4f,t.height-(tileIndex/4+1)*t.height/3f,t.width/4f,t.height/3f);
    Sprite s=Sprite.Create(t,cell,new Vector2(.5f,.45f),cell.height/(i>=9&&i<=11?2.7f:1.05f),0,SpriteMeshType.FullRect);ownedAssets.Add(s);var poses=new Sprite[16];for(int f=0;f<16;f++)poses[f]=s;enemyActionArt[WoodKinds[i]]=poses;
   }
   Texture2D ground=Resources.Load<Texture2D>("NakiArt/Woodland/ground");ground.wrapMode=TextureWrapMode.Repeat;
   Sprite tile=Sprite.Create(ground,new Rect(0,0,ground.width,ground.height),Vector2.one*.5f,ground.width/4f,0,SpriteMeshType.FullRect);ownedAssets.Add(tile);
   if(weaponCatalog.Length>=14){string[] names={"竹节轻弩","木髓枝杖","盘根长枪","朽芽重槌","青脉藤弓","采枝短刀"};for(int i=0;i<6;i++){weaponCatalog[8+i].name=names[i];weaponCatalog[8+i].element=i==0?Element.Physical:Element.Wood;weaponCatalog[8+i].cost=i==0?1:2;}}
   thunderFloor=tile;for(int i=0;i<3;i++)chapterFloors[i]=tile;
  }
  private void SpawnWoodWave(RoomNode n)
  {
   var random=new System.Random(activeSeed+n.id*97+currentWave*31);
   if(n.kind==RoomKind.Boss){SpawnJourneyEnemy(WoodKinds[9+n.stage],Vector2.zero,n.depth);return;}
   int count=3+n.stage;
   for(int i=0;i<count;i++){int k=(n.id+currentWave+i)%(n.stage==0?5:n.stage==1?9:10);QueueEnemy(WoodKinds[k==9?12:k],FindJourneySpawn(random,.5f));pendingSpawns[pendingSpawns.Count-1].when=clock+1.2f+i*.22f;}
  }
  private void BuildWoodActions(RoomNode n)
  {
   if(!n.cleared)return;
   if(n.id==0&&chapterEnding)AddChapterAction(209,new Vector2(2,-1),"返回雷雨高原",lessonArtwork);
   if(n.id==0)AddChapterAction(201,new Vector2(-2,1),chapterStone?"交还林册":"阿苓与种子木盒",lessonArtwork);
   if(n.id==6&&!chapterMarkKnown)AddChapterAction(202,new Vector2(-2,0),"研读林册 · 根刺",lessonArtwork);
   if(n.id==17)AddChapterAction(203,new Vector2(-2,0),"救出采种人",lessonArtwork);
   if(n.id==14||n.id==16||n.id==21)AddChapterAction(204,new Vector2(-2,0),"调查菌丝与幼芽",lessonArtwork);
   if(n.id==18&&!chapterArcKnown)AddChapterAction(205,new Vector2(-2,0),"领悟孢种",lessonArtwork);
   if(n.id==31&&chapterStone)AddChapterAction(206,new Vector2(-2,0),"返回旧种床",lessonArtwork);
   if(n.id==4&&chapterStone)AddChapterAction(207,new Vector2(-2,0),chapterLampGiven?"返回码头":"栽下种子",lessonArtwork);
  }
  private void WoodAction(int kind)
  {
   switch(kind)
   {
    case 209:SaveChapterCheckpoint(false);woodlandMode=false;LoadEquipmentArt();LoadThunderArt();BeginThunderChapter(false);LoadChapterCheckpoint(false);return;
    case 201:if(chapterStone&&chapterLampGiven){chapterEnding=true;TellStory("阿苓：今年的种子，不用再锁在盒子里了。","旧叶落处，新芽有声。森林终于重新有了春天。");}else TellStory("阿苓：叶子一年比一年厚，可我埋下的种子再没有探过头。","少年：还有根活着，就还有办法。先带我看看那块种床。");break;
    case 202:chapterMarkKnown=true;TellStory("林册：根会往有空隙的地方去。先听见它，再替它留一条路。");JourneyHint("已领悟根刺 · T 装备到 U / I");break;
    case 203:chapterRanger=true;TellStory("采种人：别把白丝当成树根。树根知道什么时候停，它不知道。");break;
    case 204:chapterWaymarks|=1<<(currentRoom==14?0:currentRoom==16?1:2);TellStory("枯木孔被白色菌丝封住，旁边一枚幼芽仍朝光伸展。木石的脉络通往树冠。");break;
    case 205:chapterArcKnown=true;TellStory("少年将孢种收进掌心：腐解也有自己的季节，不能让它吞掉所有新生。");JourneyHint("已领悟孢种 · T 配置");break;
    case 206:EnterExplorationRoom(4,-1);break;
    case 207:if(chapterLampGiven)EnterExplorationRoom(0,-1);else{chapterLampGiven=true;TellStory("少年移开厚叶，将种子放回柔软的泥土。","一线青光从指尖流下，新芽顶开了旧叶。");elementPower=Mathf.Max(0,elementPower);}break;
   }
   RequestCheckpoint();BuildChapterActions();SaveChapterCheckpoint(false);
  }
  private void WoodBossCleared(RoomNode n)
  {
   if(n.stage==0)TellStory("母虫退进了枯木深处，旧林册从落叶里露出来。","少年：先读懂根，才能替它留路。");
   else if(n.stage==1)TellStory("菌冠裂开，菌丝终于松开树洞。","采种人：古树还醒着。它已经很久没有睡过了。");
   else{chapterStone=true;journeyComplete=false;TellStory("守望者：我听见每一片叶子的饥饿……可我已经不能让它们落下。","少年剥离胸前的寄生结，取回木石。守望者缓缓收拢枝条。","少年：你可以歇一歇了。剩下的路，让新枝自己去长。返回种床，将种子栽下。");}
  }
  private string WoodObjective(){return chapterEnding?"新芽有声 · 木岛尾声完成":chapterStone?chapterLampGiven?"回码头 · 交还林册":"返回旧种床 · 栽下种子":journeyRooms[currentRoom].stage==1?"救出采种人，调查两处痕迹，再向树冠进发":"循林径探索 · 解除过度生长";}
 }
}
