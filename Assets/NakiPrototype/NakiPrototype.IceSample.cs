using System;
using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  [Header("Ice harbor exploration sample")] public bool iceSampleMode;
  private bool iceSpikeKnown,iceRingKnown;
  private float iceSpikeReady,iceRingReady,iceRingBorn;
  private GameObject iceRingVisual;
  private readonly Dictionary<int,Sprite> iceArt=new Dictionary<int,Sprite>();
  private static readonly string[] IceSkills={"风之刃","护身风波","雷标","跃电","冰锥","霜环"};
  private string[] ActiveSkillNames {get{return GlobalSkillNames;}}
  private int ChapterRoomCount {get{return capitalMode?42:manorCourtyardMode?45:fireChapterMode?45:iceSampleMode?(iceFullChapter?42:5):woodlandMode?45:39;}}
  private int FinalChapterBossRoom {get{return capitalMode?26:manorCourtyardMode?14:fireChapterMode?12:iceSampleMode?(iceFullChapter?40:2):31;}}
  private string ChapterIslandKey {get{return capitalMode?"capital":manorCourtyardMode?"manor_courtyard":fireChapterMode?"fire_coast":iceSampleMode?"ice_sample":woodlandMode?"woodland":"thunder_plateau";}}
  private string ChapterMapKey {get{return capitalMode?"capital-v2":manorCourtyardMode?"manor-three-v3":fireChapterMode?"fire-core-v3":iceSampleMode?(iceFullChapter?"ice-ch3-v2":"ice-s2-v1"):woodlandMode?"w3-v2":"d3-v1";}}
  private static bool IsIceEnemy(EnemyKind kind){return kind>=EnemyKind.IceGuard&&kind<=EnemyKind.FrostLens;}
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
   LoadIceExpansionArt();Texture2D floor=Resources.Load<Texture2D>("NakiArt/IceSample/floor");if(floor==null)throw new InvalidOperationException("Missing ice sample floor");floor.wrapMode=TextureWrapMode.Repeat;floor.filterMode=FilterMode.Point;
   Sprite tile=Sprite.Create(floor,new Rect(0,0,floor.width,floor.height),Vector2.one*.5f,floor.width/4f,0,SpriteMeshType.FullRect);ownedAssets.Add(tile);thunderFloor=tile;for(int i=0;i<3;i++)chapterFloors[i]=tile;if(iceFullChapter)LoadIceChapterArt();
  }
  private void BuildIceGraph()
  {
   journeyRooms.Clear();chapterDoors.Clear();chapterNames=new[]{"冰港码头","冰封回廊","裂冰甲兽 · 晶甲庭"};
   for(int i=0;i<3;i++){var n=new RoomNode{id=i,stage=0,floor=0,cell=new Vector2Int(i,0),halfWidth=8+i,halfHeight=4.5f+i*.5f,kind=i==0?RoomKind.Rest:i==1?RoomKind.Combat:RoomKind.Boss,waves=i==1?3:1,stairsTo=-1,depth=8,layout=i};if(i>0)n.links.Add(i-1);if(i<2)n.links.Add(i+1);journeyRooms.Add(n);}
   AddIceBranches();for(int i=0;i<3;i++){stageStarts[i]=0;stageEnds[i]=2;}if(iceFullChapter)ExpandIceChapterGraph();
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
   clock=0;health=6;shield=EffectiveMaximumShield;elementPower=100;origin=thunderPreset==ThunderPreset.Dark?Origin.Dark:Origin.Light;energy=origin==Origin.Light?LightMaximum:0;darkPower=origin==Origin.Dark?6:0;
   orbLearned=bladeLearned=waveLearned=windEmpowered=true;carriedWeapons[1]=9;
   nextSword=nextSpell=nextDash=dashUntil=invulnerableUntil=flashUntil=bladeReadyAt=waveReadyAt=chapterMarkReady=chapterArcReady=0;
   journeyComplete=pendingBossClear=won=false;doorTransition=0;
   EnterExplorationRoom(thunderChapterPracticeBoss>=0?(iceFullChapter?stageEnds[Mathf.Clamp(thunderChapterPracticeBoss,0,2)]:2):0,-1);
   TellStory(thunderChapterPracticeBoss>=0?"裂冰甲兽试炼：雷击能打碎晶甲，冰锥与霜环可在 T 中配置。练习不覆盖冰港存档。":"船夫：湾口的潮水冻住了。沿栈桥往东走，守塔人还在等破冰队的消息。","风把细雪推过石板，你将雷石收进行囊，走向那盏暖灯。");
  }
  private void BuildIceCover(RoomNode n)
  {
   if(iceFullChapter&&n.id>=5){BuildIceChapterCover(n);return;}Vector2[] points=n.id==0?new[]{new Vector2(-5,-1),new Vector2(4,1)}:n.id==1?new[]{new Vector2(-4,1.4f),new Vector2(-1,-1.5f),new Vector2(3,1.6f)}:new[]{new Vector2(-6.5f,1),new Vector2(6.5f,-1)};
   foreach(Vector2 at in points){if(IntersectsTransit(at,new Vector2(1.6f,1.5f)))continue;Draw("Solid ice crystal cover",IceSprite(4),at,Vector2.one,Color.white,DepthAt(at.y));AddRoundedCover(new Rect(at.x-.53f,at.y-.09f,1.06f,.36f),new Rect(at.x-.65f,at.y-.12f,1.3f,1.1f));}BuildIceExpansionCover(n);
  }
  private void SpawnIceWave(RoomNode n)
  {
   if(iceFullChapter){SpawnIceChapterWave(n);return;}if(n.id==2){SpawnJourneyEnemy(EnemyKind.IceBeast,Vector2.zero,8);return;}
   SpawnExpandedIceWave(n);
  }
  private void BuildIceActions(RoomNode n)
  {
   if(iceFullChapter){BuildIceChapterActions(n);return;}if(!n.cleared)return;
   if(n.id==0){AddChapterAction(301,new Vector2(-3,1),"与守塔人交谈",IceSprite(3));HarborBoat(304,Vector2.zero,new Vector2(-1,.45f),"展开 Naki 航图 · 自选目的地",IceSprite(6));}
   if(n.id==2&&chapterStone){AddChapterAction(302,new Vector2(-3,2),iceSpikeKnown?"翻阅破冰队记录":"拾起破冰记录 · 学习冰法术",IceSprite(5));WoodTrailSign(new Vector2(3,-2),303,"沿已清理的栈道返回码头");}
  }
  private void IceAction(int action)
  {
   if(iceFullChapter){IceChapterAction(action);return;}if(action==301){if(chapterStone&&iceSpikeKnown){chapterEnding=true;TellStory("守塔人：这是破冰队留下的笔迹。他们还在更深的冰层里。","少年：我会沿着记录继续找。先把这盏灯留着。");}else TellStory("守塔人：灯还亮着，可海湾已经很久没有换过气了。破冰队去了东面的回廊，再没有回来。","少年：我会沿栈道找过去。雷光能击碎那些晶甲，也许能替他们打开路。");}
   if(action==302){if(currentRoom!=2||!chapterStone||!journeyRooms[2].cleared)return;bool first=!iceSpikeKnown;iceSpikeKnown=iceRingKnown=true;if(first){TellStory("记录：霜不只是封住归途。让它沿着尖端生长，便能成为护身的冰锥。","你将手贴在冰晶上。寒光在指尖收拢，又化作一圈短暂的霜纹。");AnnounceDiscovery("已领悟冰锥与霜环 · T 自选装备，保留当前技能");}else TellStory("记录在一张结霜的海图旁停下，箭头指向岛屿更深处。这里还不是冰石所在的寒潮晶庭。");}
   if(action==303){EnterExplorationRoom(0,-1);TellStory("栈道上的碎冰已经清开。守塔人的灯，在风雪里重新露了出来。");}
   if(action==304){OpenWorldAtlas(2);return;}
   BuildChapterActions();RequestCheckpoint();SaveChapterCheckpoint(false);
  }
  private void CompleteIceBoss()
  {
   foreach(Enemy e in enemies){if(e.warning!=null)Destroy(e.warning);Destroy(e.root);if(e.shadow!=null)Destroy(e.shadow);}enemies.Clear();
   foreach(PendingSpawn p in pendingSpawns)if(p.mark!=null)Destroy(p.mark);pendingSpawns.Clear();foreach(Shot s in shots)if(s.root!=null)Destroy(s.root);shots.Clear();ClearThunderHazards();ClearChapterSkillEffects();
   if(iceFullChapter){FinishIceChapterBoss();return;}ClearJourneyRoom();chapterStone=true;journeyComplete=false;
   TellStory("裂冰甲兽收拢碎裂的甲片，退回了冰雾深处。冰层下露出一册湿透的记录。","少年：这是破冰队的东西。先看看他们留下了什么。");
   BuildChapterActions();UpdateExplorationDoors();RequestCheckpoint();SaveChapterCheckpoint(false);
  }
  private void EmbarkIceSample()
  {
   if(iceSampleMode||woodlandMode||!chapterEnding||!SafeCheckpointRoom)return;SaveChapterCheckpoint(false);var player=CaptureChapterPlayer();Origin old=origin;bool mark=chapterMarkKnown,arc=chapterArcKnown;
   float oldSword=swordDamage-initialSwordDamage,oldSpell=spellDamage-initialSpellDamage;thunderPreset=old==Origin.Dark?ThunderPreset.Dark:ThunderPreset.Light;
   iceSampleMode=true;iceFullChapter=true;woodlandMode=false;thunderChapterPracticeBoss=-1;thunderChapterStartFloor=0;chapterCheckpointPathOverride=null;
   if(LoadVoyageChapter(player.talents,player))return;
   BeginIceSample(false);ApplyChapterPlayer(player);swordDamage=initialSwordDamage+oldSword;spellDamage=initialSpellDamage+oldSpell;origin=old;NormalizeOriginPower();chapterMarkKnown=mark;chapterArcKnown=arc;EnterExplorationRoom(0,-1);
   TellStory("巡路员把一张冰港海图交给你：破冰队已经失联多日，湾口的灯却始终没有熄。","小船越过最后一道雨幕，靠上了覆雪的旧栈桥。");RequestCheckpoint();
  }
  private void ReturnFromIce()
  {
   var journeyPlayer=CaptureChapterPlayer();int[] carriedTalents=CopyTalents();SaveChapterCheckpoint(false);iceSampleMode=false;chapterSetup=false;LoadEquipmentArt();LoadThunderArt();BeginThunderChapter(false);
   if(!LoadVoyageChapter(carriedTalents,journeyPlayer)){chapterSetup=true;TellStory("船回到高原港口。当前没有可继续的高原安全点，可选择试航配置。");}
  }
  private string IceObjective(){if(iceFullChapter)return IceChapterObjective();if(currentRoom>=3)return journeyRooms[currentRoom].cleared?"支路已清理 · 拾取补给和装备，再返回回廊":currentRoom==3?"借货箱挡箭 · 避开猎犬扑击":"避开霜纹落点 · 优先处理祭司";return !chapterStone?currentRoom==0?"与守塔人交谈 · 沿东侧栈桥寻找破冰队":"主路向东 · 南北支路可寻找物资":!iceSpikeKnown?"拾起破冰队记录 · 领悟冰锥与霜环":chapterEnding?"记录已交回 · 货仓与瞭台支路仍可探索":"沿栈道返回码头 · 将记录交给守塔人";}
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
   CenterLabel(new Rect(w/2-430,90,860,48),"第三岛 · 霜潮群岛",journeyTitleStyle);
   CenterLabel(new Rect(w/2-450,150,900,80),"冰港货仓 → 裂潮冰峡 → 寒潮晶庭 · 三层 42 房\n救出破冰队、寻回冰石；清理回流闸后返回灯塔，迎接归航。",journeyTextStyle);
   bool enabled=GUI.enabled;if(chapterConfirmRestart)GUI.enabled=false;
   for(int i=0;i<2;i++)if(FlatButton(new Rect(w/2-290+i*300,260,280,65),i==0?"1 光之力":"2 贤者之石",(int)thunderPreset==i))thunderPreset=(ThunderPreset)i;
   if(FlatButton(new Rect(w/2-435,360,280,65),HasChapterSave?"继续冰港存档 · F":"开始霜潮旅程 · F")){if(HasChapterSave)LoadChapterCheckpoint(true);else StartChapterFromSetup();}
   if(FlatButton(new Rect(w/2-140,360,280,65),"另开冰港旅程"))StartChapterFromSetup();
   if(FlatButton(new Rect(w/2+155,360,280,65),"直接练习裂冰甲兽")){chapterSetup=false;thunderChapterPracticeBoss=0;BeginIceSample(false);}
   CenterLabel(new Rect(w/2-460,465,920,85),"冰港使用独立安全存档；练习不覆盖旅程。\n清房后自动保存；F5 保存，F9 读取；T 配置元素技能。\n三层霜潮群岛：救援破冰队、寻回冰石、疏通回流闸、点亮归航灯。",journeySmallStyle);
   GUI.enabled=enabled;
   if(chapterConfirmRestart){HudRect(new Rect(w/2-370,220,740,280),C("10232d"));CenterLabel(new Rect(w/2-345,240,690,90),"冰港已有进度。是否另开旅程？\n继续可回到原安全点；另开会先保留历史副本。",journeyTextStyle);if(FlatButton(new Rect(w/2-330,370,320,65),"继续原进度")){chapterConfirmRestart=false;LoadChapterCheckpoint(true);}if(FlatButton(new Rect(w/2+10,370,320,65),"保留副本，另开旅程"))StartChapterFromSetup(true);}
  }
 }
}
