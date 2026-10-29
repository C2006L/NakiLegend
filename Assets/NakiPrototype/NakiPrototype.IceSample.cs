using System;
using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  [Header("Ice harbor three-room sample")] public bool iceSampleMode;
  private bool iceSpikeKnown,iceRingKnown;
  private float iceSpikeReady,iceRingReady,iceRingBorn;
  private GameObject iceRingVisual;
  private readonly Dictionary<int,Sprite> iceArt=new Dictionary<int,Sprite>();
  private static readonly string[] IceSkills={"风之刃","护身风波","雷标","跃电","冰锥","霜环"};
  private string[] ActiveSkillNames {get{return iceSampleMode?IceSkills:woodlandMode?new[]{"风之刃","护身风波","根刺","孢种"}:ChapterSkillNames;}}
  private int ChapterRoomCount {get{return iceSampleMode?3:woodlandMode?45:39;}}
  private int FinalChapterBossRoom {get{return iceSampleMode?2:31;}}
  private string ChapterIslandKey {get{return iceSampleMode?"ice_sample":woodlandMode?"woodland":"thunder_plateau";}}
  private string ChapterMapKey {get{return iceSampleMode?"ice-s1-v1":woodlandMode?"w3-v1":"d3-v1";}}
  private static bool IsIceEnemy(EnemyKind kind){return kind==EnemyKind.IceGuard||kind==EnemyKind.IceWisp||kind==EnemyKind.IceBeast;}
  private Sprite IceSprite(int index)
  {
   Sprite sprite;if(iceArt.TryGetValue(index,out sprite)&&sprite!=null)return sprite;
   Texture2D t=Resources.Load<Texture2D>("NakiArt/IceSample/atlas");if(t==null)throw new InvalidOperationException("Missing ice sample atlas");t.filterMode=FilterMode.Point;
   float height=index==2?2.9f:index==3?1.5f:index==6?2.3f:index==0?1.6f:index==4?1.1f:1.2f;
   // Authored regions follow the generated subjects, including the wider beast.
   // A uniform grid would clip its horns and include parts of the next cell.
   Rect[] regions={new Rect(10,12,430,438),new Rect(470,12,366,438),new Rect(840,0,600,466),new Rect(1440,70,330,390),new Rect(10,468,415,390),new Rect(446,477,380,385),new Rect(889,480,500,385),new Rect(1410,465,350,400)};
   Rect cell=regions[index];cell=new Rect(cell.x*t.width/1774f,t.height-(cell.y+cell.height)*t.height/887f,cell.width*t.width/1774f,cell.height*t.height/887f);
   sprite=Sprite.Create(t,cell,new Vector2(.5f,.15f),cell.height/height,0,SpriteMeshType.FullRect);ownedAssets.Add(sprite);iceArt[index]=sprite;return sprite;
  }
  private void LoadIceArt()
  {
   foreach(EnemyKind kind in new[]{EnemyKind.IceGuard,EnemyKind.IceWisp,EnemyKind.IceBeast}){int index=kind==EnemyKind.IceGuard?0:kind==EnemyKind.IceWisp?1:2;var frames=new Sprite[16];for(int i=0;i<16;i++)frames[i]=IceSprite(index);enemyActionArt[kind]=frames;}
   Texture2D floor=Resources.Load<Texture2D>("NakiArt/IceSample/floor");if(floor==null)throw new InvalidOperationException("Missing ice sample floor");floor.wrapMode=TextureWrapMode.Repeat;
   Sprite tile=Sprite.Create(floor,new Rect(0,0,floor.width,floor.height),Vector2.one*.5f,floor.width/4f,0,SpriteMeshType.FullRect);ownedAssets.Add(tile);thunderFloor=tile;for(int i=0;i<3;i++)chapterFloors[i]=tile;
  }
  private void BuildIceGraph()
  {
   journeyRooms.Clear();chapterDoors.Clear();chapterNames=new[]{"冰港码头","冰封回廊","裂冰甲兽 · 晶甲庭"};
   for(int i=0;i<3;i++){var n=new RoomNode{id=i,stage=0,floor=0,cell=new Vector2Int(i,0),halfWidth=8+i,halfHeight=4.5f+i*.5f,kind=i==0?RoomKind.Rest:i==1?RoomKind.Combat:RoomKind.Boss,waves=i==1?3:1,stairsTo=-1,depth=8,layout=i};if(i>0)n.links.Add(i-1);if(i<2)n.links.Add(i+1);journeyRooms.Add(n);}
   for(int i=0;i<3;i++){stageStarts[i]=0;stageEnds[i]=2;}
  }
  private void BeginIceSample(bool newSeed)
  {
   woodlandMode=thunderPrototypeMode=gatehouseSampleMode=bossPracticeMode=lightningTrialMode=false;thunderChapterMode=true;
   if(!tuningCaptured){initialSwordDamage=swordDamage;initialSpellDamage=spellDamage;tuningCaptured=true;}swordDamage=initialSwordDamage;spellDamage=initialSpellDamage;
   if(newSeed||activeSeed==0)activeSeed=journeySeed!=0&&!newSeed?journeySeed:Environment.TickCount&int.MaxValue;if(activeSeed==0)activeSeed=31001;
   ResetGatehouseProgress();ResetPlayerProgression();LoadThunderChapterEquipment();BuildIceGraph();LoadIceArt();
   chapterWest=chapterEast=chapterRanger=chapterStone=chapterEnding=chapterCg=chapterLampCarried=chapterLampGiven=false;
   chapterMarkKnown=chapterArcKnown=true;iceSpikeKnown=iceRingKnown=thunderChapterPracticeBoss>=0;iceSpikeReady=iceRingReady=0;
   chapterWaymarks=chapterShortcuts=0;chapterWork=0;chapterSavePending=chapterSetup=chapterSkillsMenu=chapterRewardMenu=false;
   chapterSkillSlots[0]=2;chapterSkillSlots[1]=thunderChapterPracticeBoss>=0?4:3;
   clock=0;health=6;shield=EffectiveMaximumShield;elementPower=100;origin=thunderPreset==ThunderPreset.Dark?Origin.Dark:Origin.Light;energy=origin==Origin.Light?100:0;darkPower=origin==Origin.Dark?6:0;
   orbLearned=bladeLearned=waveLearned=windEmpowered=true;carriedWeapons[1]=9;
   nextSword=nextSpell=nextDash=dashUntil=invulnerableUntil=flashUntil=bladeReadyAt=waveReadyAt=chapterMarkReady=chapterArcReady=0;
   journeyComplete=pendingBossClear=won=false;doorTransition=0;
   EnterExplorationRoom(thunderChapterPracticeBoss>=0?2:0,-1);
   TellStory(thunderChapterPracticeBoss>=0?"裂冰甲兽试炼：雷击能打碎晶甲，冰锥与霜环可在 T 中配置。练习不覆盖冰港存档。":"船夫：湾口的潮水冻住了。沿栈桥往东走，守塔人还在等破冰队的消息。","风把细雪推过石板，你将雷石收进行囊，走向那盏暖灯。");
  }
  private void BuildIceCover(RoomNode n)
  {
   Vector2[] points=n.id==0?new[]{new Vector2(-5,-1),new Vector2(4,1)}:n.id==1?new[]{new Vector2(-4,1.4f),new Vector2(-1,-1.5f),new Vector2(3,1.6f)}:new[]{new Vector2(-6.5f,1),new Vector2(6.5f,-1)};
   foreach(Vector2 at in points){if(IntersectsTransit(at,new Vector2(1.6f,1.5f)))continue;Draw("Solid ice crystal cover",IceSprite(4),at,Vector2.one,Color.white,DepthAt(at.y));obstacles.Add(new Rect(at.x-.65f,at.y-.12f,1.3f,.45f));projectileObstacles.Add(new Rect(at.x-.65f,at.y-.12f,1.3f,1.1f));}
  }
  private void SpawnIceWave(RoomNode n)
  {
   if(n.id==2){SpawnJourneyEnemy(EnemyKind.IceBeast,Vector2.zero,8);return;}
   var random=new System.Random(activeSeed+currentWave*113);int count=currentWave==1?2:3;
   for(int i=0;i<count;i++){QueueEnemy(currentWave==1||i%2==0?EnemyKind.IceGuard:EnemyKind.IceWisp,FindJourneySpawn(random,.45f));pendingSpawns[pendingSpawns.Count-1].when=clock+1.2f+i*.3f;}
  }
  private void BuildIceActions(RoomNode n)
  {
   if(!n.cleared)return;
   if(n.id==0){AddChapterAction(301,new Vector2(-3,1),"与守塔人交谈",IceSprite(3));AddChapterAction(304,new Vector2(3,-2),"登船返回雷雨高原",IceSprite(6));}
   if(n.id==2&&chapterStone){AddChapterAction(302,new Vector2(-3,2),iceSpikeKnown?"翻阅破冰队记录":"拾起破冰记录 · 学习冰法术",IceSprite(5));WoodTrailSign(new Vector2(3,-2),303,"沿已清理的栈道返回码头");}
  }
  private void IceAction(int action)
  {
   if(action==301){if(chapterStone&&iceSpikeKnown){chapterEnding=true;TellStory("守塔人：这是破冰队留下的笔迹。他们还在更深的冰层里。","少年：我会沿着记录继续找。先把这盏灯留着。");}else TellStory("守塔人：灯还亮着，可海湾已经很久没有换过气了。破冰队去了东面的回廊，再没有回来。","少年：我会沿栈道找过去。雷光能击碎那些晶甲，也许能替他们打开路。");}
   if(action==302){if(currentRoom!=2||!chapterStone||!journeyRooms[2].cleared)return;bool first=!iceSpikeKnown;iceSpikeKnown=iceRingKnown=true;if(first){chapterSkillSlots[0]=chapterMarkKnown?2:0;chapterSkillSlots[1]=4;TellStory("记录：霜不只是封住归途。让它沿着尖端生长，便能成为护身的冰锥。","你将手贴在冰晶上。寒光在指尖收拢，又化作一圈短暂的霜纹。");AnnounceDiscovery("已领悟冰锥与霜环 · I 冰锥；T 可调整");}else TellStory("记录在一张结霜的海图旁停下，箭头指向岛屿更深处。这里还不是冰石所在的极夜晶庭。");}
   if(action==303){EnterExplorationRoom(0,-1);TellStory("栈道上的碎冰已经清开。守塔人的灯，在风雪里重新露了出来。");}
   if(action==304){ReturnFromIce();return;}
   BuildChapterActions();RequestCheckpoint();SaveChapterCheckpoint(false);
  }
  private void CompleteIceBoss()
  {
   foreach(Enemy e in enemies){if(e.warning!=null)Destroy(e.warning);Destroy(e.root);if(e.shadow!=null)Destroy(e.shadow);}enemies.Clear();
   foreach(PendingSpawn p in pendingSpawns)if(p.mark!=null)Destroy(p.mark);pendingSpawns.Clear();foreach(Shot s in shots)if(s.root!=null)Destroy(s.root);shots.Clear();ClearThunderHazards();ClearChapterSkillEffects();
   ClearJourneyRoom();chapterStone=true;journeyComplete=false;
   TellStory("裂冰甲兽收拢碎裂的甲片，退回了冰雾深处。冰层下露出一册湿透的记录。","少年：这是破冰队的东西。先看看他们留下了什么。");
   BuildChapterActions();UpdateExplorationDoors();RequestCheckpoint();SaveChapterCheckpoint(false);
  }
  private void EmbarkIceSample()
  {
   if(iceSampleMode||woodlandMode||!chapterEnding||!SafeCheckpointRoom)return;SaveChapterCheckpoint(false);var player=CaptureChapterPlayer();Origin old=origin;bool mark=chapterMarkKnown,arc=chapterArcKnown;
   float oldSword=swordDamage-initialSwordDamage,oldSpell=spellDamage-initialSpellDamage;thunderPreset=old==Origin.Dark?ThunderPreset.Dark:ThunderPreset.Light;
   iceSampleMode=true;woodlandMode=false;thunderChapterPracticeBoss=-1;thunderChapterStartFloor=0;chapterCheckpointPathOverride=null;
   if(LoadChapterCheckpoint(false))return;
   BeginIceSample(false);ApplyChapterPlayer(player);swordDamage=initialSwordDamage+oldSword;spellDamage=initialSpellDamage+oldSpell;origin=old;NormalizeOriginPower();chapterMarkKnown=mark;chapterArcKnown=arc;chapterSkillSlots[0]=mark?2:0;chapterSkillSlots[1]=arc?3:1;EnterExplorationRoom(0,-1);
   TellStory("巡路员把一张冰港海图交给你：破冰队已经失联多日，湾口的灯却始终没有熄。","小船越过最后一道雨幕，靠上了覆雪的旧栈桥。");RequestCheckpoint();
  }
  private void ReturnFromIce()
  {
   SaveChapterCheckpoint(false);iceSampleMode=false;chapterSetup=false;LoadEquipmentArt();LoadThunderArt();BeginThunderChapter(false);
   if(!LoadChapterCheckpoint(false)){chapterSetup=true;TellStory("船回到高原港口。当前没有可继续的高原安全点，可选择试航配置。");}
  }
  private string IceObjective(){return !chapterStone?currentRoom==0?"与守塔人交谈 · 沿东侧栈桥寻找破冰队":"雷击破晶甲 · 清理回廊，继续向东":!iceSpikeKnown?"拾起破冰队记录 · 领悟冰锥与霜环":chapterEnding?"冰港样板完成 · 破冰队的去向仍在岛屿深处":"沿栈道返回码头 · 将记录交给守塔人";}
  private bool HandleIceSetup()
  {
   if(chapterConfirmRestart){if(Pressed('e'))chapterConfirmRestart=false;return true;}
   if(Pressed('1'))thunderPreset=ThunderPreset.Light;if(Pressed('2'))thunderPreset=ThunderPreset.Dark;
   if(Pressed('f')){if(HasChapterSave)LoadChapterCheckpoint(true);else StartChapterFromSetup();}
   if(Pressed('e'))chapterSetup=false;return true;
  }
  private void DrawIceSetup(float w,float h)
  {
   HudRect(new Rect(0,0,w,h),new Color(.02f,.05f,.09f,.96f));
   CenterLabel(new Rect(w/2-430,90,860,48),"第三岛 · 冰港三房样板",journeyTitleStyle);
   CenterLabel(new Rect(w/2-450,150,900,80),"覆雪码头 → 冰封回廊 → 裂冰甲兽\n雷击破晶甲；击退甲兽后，从破冰队记录学习冰锥与霜环。",journeyTextStyle);
   bool enabled=GUI.enabled;if(chapterConfirmRestart)GUI.enabled=false;
   for(int i=0;i<2;i++)if(FlatButton(new Rect(w/2-290+i*300,260,280,65),i==0?"1 光之力":"2 贤者之石",(int)thunderPreset==i))thunderPreset=(ThunderPreset)i;
   if(FlatButton(new Rect(w/2-435,360,280,65),HasChapterSave?"继续冰港存档 · F":"开始冰港样板 · F")){if(HasChapterSave)LoadChapterCheckpoint(true);else StartChapterFromSetup();}
   if(FlatButton(new Rect(w/2-140,360,280,65),"另开冰港旅程"))StartChapterFromSetup();
   if(FlatButton(new Rect(w/2+155,360,280,65),"直接练习裂冰甲兽")){chapterSetup=false;thunderChapterPracticeBoss=0;BeginIceSample(false);}
   CenterLabel(new Rect(w/2-460,465,920,85),"冰港使用独立安全存档；练习不覆盖旅程。\n清房后自动保存；F5 保存，F9 读取；T 配置元素技能。\n这是第三岛的战斗样板，尚未开放完整冰山岛与最终冰石剧情。",journeySmallStyle);
   GUI.enabled=enabled;
   if(chapterConfirmRestart){HudRect(new Rect(w/2-370,220,740,280),C("10232d"));CenterLabel(new Rect(w/2-345,240,690,90),"冰港已有进度。是否另开旅程？\n继续可回到原安全点；另开会先保留历史副本。",journeyTextStyle);if(FlatButton(new Rect(w/2-330,370,320,65),"继续原进度")){chapterConfirmRestart=false;LoadChapterCheckpoint(true);}if(FlatButton(new Rect(w/2+10,370,320,65),"保留副本，另开旅程"))StartChapterFromSetup(true);}
  }
 }
}
