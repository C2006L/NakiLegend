using System;
using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  [Header("Woodland chapter")] public bool woodlandMode;
  private static string[] WoodlandStages {get{return AtlasRegions[4];}}
  private EnemyKind[] WoodKinds={EnemyKind.ChargeMoth,EnemyKind.CopperRoller,EnemyKind.SpearGuard,EnemyKind.Tumbleweed,EnemyKind.SandScorpion,EnemyKind.ThunderStone,EnemyKind.StoneSentinel,EnemyKind.SandKite,EnemyKind.StormGuard,EnemyKind.CopperWarden,EnemyKind.ThunderSentinel,EnemyKind.StormStag,EnemyKind.WoodlandDeer};
  private string[] WoodNames={"针尾蜂","棘甲虫","裂荚株","匍藤猎手","灰孢团","苔背盾卫","腐叶蜈","吞苔蛙","引荧螟","棘甲母虫","菌冠孢囊","古树守望者","寄枝鹿"};
  private int ArrayIndexWood(EnemyKind kind){return Array.IndexOf(WoodKinds,kind);}
  private void EmbarkWoodland()
  {
   SaveChapterCheckpoint(false);var player=CaptureChapterPlayer();Origin previous=origin;
   woodlandMode=true;thunderChapterStartFloor=0;thunderChapterPracticeBoss=-1;chapterCheckpointPathOverride=null;
   if(LoadVoyageChapter(player.talents,player))return;
   BeginThunderChapter(false);ApplyChapterPlayer(player);origin=previous;shield=Mathf.Min(EffectiveMaximumShield,shield+1);EnterExplorationRoom(0,-1);
   TellStory("巡路员递来一块求援木牌：邻岛的种子已经多年不发芽。","船穿过晨雾，停在青栖林海的旧栈桥旁。阿苓在码头等着。");RequestCheckpoint();
  }
  private void ConfigureWoodEnemy(Enemy e)
  {
   int k=ArrayIndexWood(e.kind);if(k<0)return;e.affinity=k==2||k==3||k==4||k==5||k==10||k==11?Element.Wood:Element.Physical;e.attackElement=Element.Wood;
   float[] hp={38,80,52,62,36,100,64,70,44,560,900,1700,96};e.hp=e.maximum=hp[k];e.root.transform.localScale=Vector3.one;
   e.speed=k==2?.15f:k==5?.7f:k>=9&&k<=11?1:1.3f;e.root.name=WoodNames[k];
  }
  private void CastWoodSkill(int slot)
  {
   int skill=chapterSkillSlots[slot];if(skill==0){CastWindBlade();return;}if(skill==1){CastWindWave();return;}
   if(!ChapterSkillKnown(skill)||clock<ChapterSkillReady(skill))return;
   Enemy target;Vector2 aim=AutoAim(9,9,0,out target);if(target==null)return;
   if(skill==11){if(clock<dashUntil||elementPower<SkillElementCost(9))return;if(SpawnSpellBurst(Element.Wood,"spore",28*.65f,7,1.6f,true)==0)return;SpendSkillPower(9);forestSporeReady=clock+2.4f;return;}if(!SpendSkillPower(7))return;
   SkillSound(skill==10?"root":"spore","cast");
   if(skill==10){forestRootReady=clock+1.6f;CastForestRoots(target,aim);}
   else{forestSporeReady=clock+2.4f;int before=shots.Count;SpawnShot(position+aim*.4f,aim,false,target,position);if(shots.Count==before)return;Shot shot=shots[shots.Count-1];shot.element=Element.Wood;shot.skillAudio="spore";shot.damage=SkillDamage(28);shot.tracking=true;shot.speed=7;shot.root.GetComponent<SpriteRenderer>().color=C("b7d37b");AddShotRibbon(shot,C("8bbf69"),.07f);}
  }
  private void DecorateWoodCover(Vector2 at)
  {
   AddWoodCover(at,0,.75f);
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
   chapterNames[17]="朽木腹地 · 看守牢笼";
   stageStarts[0]=0;stageStarts[1]=12;stageStarts[2]=25;stageEnds[0]=6;stageEnds[1]=18;stageEnds[2]=31;RefineWoodGraph();
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
   AddWoodWeapons();thunderFloor=tile;LoadWoodlandV2Art();
  }

  private void SpawnWoodWave(RoomNode n)
  {
   if(n.kind==RoomKind.Boss){Enemy boss=SpawnJourneyEnemy(WoodKinds[9+n.stage],Vector2.zero,n.depth);if(n.stage==2)SpawnWoodKnots(boss);return;}
   var rng=new System.Random(activeSeed+n.id*199+currentWave*71);int count=n.stage==0?5:n.stage==1?currentWave==1?5:6:currentWave==1?6:7,ranged=0,deer=0;
   int[] pool=n.stage==0?new[]{0,1,2,3,4}:n.stage==1?new[]{0,1,2,3,4,5,6,7}:new[]{0,1,2,3,4,5,6,7,8,12};
   for(int i=0;i<count;i++){int k=pool[rng.Next(pool.Length)];if(k==12&&deer++>=1)k=1;if((k==0||k==2||k==4||k==8)&&ranged++>=2)k=1;QueueEnemy(WoodKinds[k],FindJourneySpawn(rng,.6f),n.kind==RoomKind.Elite&&i==0);pendingSpawns[pendingSpawns.Count-1].when=clock+1.4f+i*.3f;}
  }
  private void BuildWoodActions(RoomNode n)
  {
   ClearWoodPropBlocks();
   if(n.id==17)BuildSeedCollectorCage();
   if(!n.cleared)return;
   if(n.id==0)HarborBoat(209,Vector2.zero,new Vector2(-1,.45f),"展开 Naki 航图 · 自选目的地",null);
   if(n.id==0){WoodPropAction(201,6,new Vector2(-2.7f,1.4f),chapterStone?chapterEnding?"重温旧叶落处，新芽有声":"将林册交还阿苓":"查看种子木盒 · 与阿苓交谈");GameObject npc=Draw("Herbalist A Ling",ResidentSprite(5),new Vector2(-4.2f,1.5f),Vector2.one,Color.white,DepthAt(1.1f));npc.transform.SetParent(chapterFocus.transform,true);}
   if(n.id==6&&!chapterMarkKnown)ForestQuestAction(202,4,new Vector2(-2,0),"翻阅林册 · 领悟根刺");
   if(n.id==14||n.id==16||n.id==21)ForestQuestAction(204,n.id==14?2:n.id==16?3:0,new Vector2(-2,0),n.id==14?"调查封丝树孔":n.id==16?"查看空果壳":"检查旧叶下的活种根");
   if(n.id==18&&!chapterArcKnown)ForestQuestAction(205,5,new Vector2(-2,0),"拾取孢种 · 领悟木元素法术");
   if(n.id==31&&chapterStone)WoodTrailSign(new Vector2(-2,0),206,"沿林间小径返回旧种床");
   if(n.id==4){ForestQuestProp(chapterLampGiven?1:0,new Vector2(-2,0),chapterLampGiven?"New seedling in restored bed":"Old seedbed");if(chapterStone)AddChapterAction(207,new Vector2(-2,-.6f),chapterLampGiven?"沿小径返回码头":"在种床栽下种子");}
   BuildForestAftermath(n);BuildWorkshopGuide(n);
  }
  private void WoodAction(int kind)
  {
   switch(kind)
   {
    case 209:OpenWorldAtlas(4);return;
    case 201:if(WoodCanEnd){chapterCg=true;storyLines=null;}else if(chapterStone)TellStory("阿苓：林册带回来了，种子呢？那块旧种床，还在等你。","少年低头看了看木盒：先让它回到泥土里，再把好消息带回来。");else TellStory("阿苓：叶子一年比一年厚，可我埋下的种子再没有探过头。","阿苓：以前送往北方府邸的种子，也收过这样的回信。采种人还没回来，林册也失在里面了。","少年：先救人，再看看根出了什么问题。那封信，也替我留着。");break;
    case 202:chapterMarkKnown=true;learnedElementSkills|=1<<10;TellStory("林册：根会往有空隙的地方去。先听见它，再替它留一条路。");JourneyHint("已领悟根刺 · 自动锁敌，直线最多缠住三只 · T 装备");break;
    case 203:if(currentRoom!=17||!journeyRooms[currentRoom].cleared||chapterRanger)return;chapterRanger=true;Sfx("clear",false,.5f);TellStory("你解开笼门的锁扣，推开锈蚀的铁门。采种人弯腰走出牢笼。","采种人：别把白丝当成树根。树根知道什么时候停，它不知道。","采种人指着林册：去看看封丝树孔、空果壳和活种根，找出白丝夺走了什么。我先沿清过的路去码头报平安。");break;
    case 210:TellStory("采种人：笼门终于开了……谢谢。那些白丝的尽头，就是古树的庭院。");break;
    case 204:if(currentRoom!=14&&currentRoom!=16&&currentRoom!=21)return;int bit=1<<(currentRoom==14?0:currentRoom==16?1:2);if((chapterWaymarks&bit)!=0){JourneyHint("这处生长痕迹已记录 · 继续寻找其他线索");return;}chapterWaymarks|=bit;elementPower=Mathf.Min(ElementMaximum,elementPower+20);TellStory(currentRoom==14?"树孔被白丝封住，朽木里却找不到本该栖息的虫。少年把菌丝的走向记进林册。":currentRoom==16?"空果壳仍挂在旧枝上，里面已经没有种仁。年复一年催长的枝叶，耗尽了下一季的养分。":"厚叶下的种根仍有生机，却迟迟不能萌发。少年将它记进林册：菌丝夺走了养分，尚未到新芽破土的时候。");break;
    case 205:chapterArcKnown=true;learnedElementSkills|=1<<11;TellStory("少年将孢种收进掌心：腐解也有自己的季节，不能让它吞掉所有新生。");JourneyHint("已领悟孢种 · 追踪孢种附加腐解伤害 · T 配置");break;
    case 206:if(!WoodFinalBossCleared)return;EnterExplorationRoom(4,-1);break;
    case 207:if(!WoodFinalBossCleared||currentRoom!=4){JourneyHint("先解除树冠旧庭中古树的寄生，再回旧叶种床栽种");return;}QueueOriginMoment(4);if(chapterLampGiven)EnterExplorationRoom(0,-1);else{chapterLampGiven=true;TellStory("少年移开厚叶，将种子放回柔软的泥土。","一线青光从指尖流下，新芽顶开了旧叶。","林册夹页里落出一封北方旧信，信封写着‘暮钟岛·受封者府邸’。少年把信收进航海手记。","林册还在行囊里。沿种床边的小径再按 F 返回码头，把新芽的消息告诉阿苓。");elementPower=Mathf.Max(0,elementPower);}break;
   }
   RequestCheckpoint();BuildChapterActions();SaveChapterCheckpoint(false);
  }
  private void WoodBossCleared(RoomNode n)
  {
   if(n.stage==0)TellStory("母虫退进了枯木深处，旧林册从落叶里露出来。","少年：先读懂根，才能替它留路。");
   else if(n.stage==1){shield=Mathf.Min(EffectiveMaximumShield,shield+1);AnnounceDiscovery("林纹护具 · 护盾上限 +1");TellStory("菌冠裂开，菌丝终于松开树洞。","采种人：古树还醒着。它已经很久没有睡过了。");}
   else{chapterStone=true;journeyComplete=false;TellStory("守望者：我听见每一片叶子的饥饿……可我已经不能让它们落下。","少年剥离胸前的寄生结，取回木石。守望者缓缓收拢枝条。","少年：你可以歇一歇了。剩下的路，让新枝自己去长。返回种床，将种子栽下。");}
  }
  private string WoodObjective(){if(!chapterStone&&currentRoom==6&&journeyRooms[6].cleared&&!chapterMarkKnown)return "翻阅落叶中的林册 · 领悟根刺，再沿树洞进入腹地";if(!chapterStone&&currentRoom==18&&journeyRooms[18].cleared&&!chapterArcKnown)return "拾取菌冠留下的孢种 · 领悟法术，再登树冠旧庭";return chapterEnding&&WoodCanEnd?"新芽有声 · 青栖林海尾声完成":chapterStone?chapterLampGiven?"回码头 · 交还林册":"返回旧种床 · 栽下种子":journeyRooms[currentRoom].stage==2?"前往古树旧庭 · 火可驱散孢雾，击破寄生结可削弱种阵":journeyRooms[currentRoom].stage==1?(!chapterRanger?"找到看守牢笼 · 清理看守并用钥匙开锁":System.Convert.ToString(chapterWaymarks,2).Replace("0","").Length<2?"采种人已获救 · 继续调查两处菌丝痕迹":"调查已完成 · 前往菌冠深处"):"循林径探索 · 解除过度生长";}
 }
}
