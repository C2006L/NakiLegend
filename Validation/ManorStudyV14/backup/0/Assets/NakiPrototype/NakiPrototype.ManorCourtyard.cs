using System;
using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  [Header("Manor · first floor: Oath Courtyard")]
  public bool manorCourtyardMode;
  private bool manorIntro;
  private float manorIntroBegan;
  private bool TickManorOpening()
  {
   if(!manorCourtyardMode||!manorIntro)return false;
   if(Time.unscaledTime>manorIntroBegan+.4f&&(Pressed('f')||Pressed('e'))){manorIntro=false;chapterLampGiven=true;BlockWorldInteraction();RequestCheckpoint();SaveChapterCheckpoint(false);}return true;
  }
  private void DrawManorOpening(float w,float h)
  {
   HudRect(new Rect(0,0,w,h),Color.black);var art=Resources.Load<Texture2D>("NakiArt/ManorV1/opening");if(art!=null)GUI.DrawTexture(new Rect(0,0,w,h),art,ScaleMode.ScaleToFit);
   HudRect(new Rect(0,h-145,w,145),new Color(.025f,.035f,.065f,.91f));FittedLabel(new Rect(36,h-130,w-72,38),"暮钟不迎归人",journeyTitleStyle);
   FittedLabel(new Rect(36,h-86,w-72,45),"五座岛的灯重新亮起，受封的外庭却闭着门。那封同样的委任书，曾把谁带到这里？",journeyTextStyle,true);
   FittedLabel(new Rect(36,h-35,w-72,27),"F 继续 · ESC 跳过",journeySmallStyle);
  }
  private readonly Dictionary<string,Sprite> manorSprites=new Dictionary<string,Sprite>();
  // Extract the actor's connected silhouette from its generated cell. Keep the
  // original PPU and foot anchor; trimming a neighboring row must never rescale a pose.
  private Rect ManorActorRect(Texture2D t)
  {
   Color32[] pixels=t.GetPixels32();bool[] seen=new bool[pixels.Length];int[] queue=new int[pixels.Length];int largest=0,bx0=0,by0=0,bx1=t.width,by1=t.height;
   for(int start=0;start<pixels.Length;start++){
    if(seen[start]||pixels[start].a<128)continue;int head=0,tail=0,x0=t.width,y0=t.height,x1=0,y1=0;seen[start]=true;queue[tail++]=start;
    while(head<tail){int at=queue[head++],x=at%t.width,y=at/t.width;x0=Math.Min(x0,x);y0=Math.Min(y0,y);x1=Math.Max(x1,x);y1=Math.Max(y1,y);
     for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){int xx=x+dx,yy=y+dy;if(xx<0||xx>=t.width||yy<0||yy>=t.height)continue;int next=yy*t.width+xx;if(!seen[next]&&pixels[next].a>=128){seen[next]=true;queue[tail++]=next;}}
    }
    if(tail>largest){largest=tail;bx0=Math.Max(0,x0-2);by0=Math.Max(0,y0-2);bx1=Math.Min(t.width,x1+3);by1=Math.Min(t.height,y1+3);}
   }
   return new Rect(bx0,by0,bx1-bx0,by1-by0);
  }
  private static readonly string[] ManorRoomNames={"暮钟渡口","石狮夹道","勇者纪念庭","钟廊巡路","废弩塔","旧马厩","退役营房","受封登记处","封粮门道","誓庭前室","翻车石径","巡卫补给间","荒芜花庭","断旗台","誓庭卫长"};
  private static bool IsManorEnemy(EnemyKind k){return k>=EnemyKind.ManorPatrol&&k<=EnemyKind.ManorKnight;}
  private Sprite ManorSprite(string name,float width=1,Vector2? pivot=null)
  {
   string key=name+"/"+width;Sprite s;if(manorSprites.TryGetValue(key,out s)&&s!=null)return s;
   var t=Resources.Load<Texture2D>("NakiArt/ManorCourtyardV2/"+name);
   if(t==null)throw new InvalidOperationException("Missing manor art: "+name);
   t.filterMode=FilterMode.Point;t.wrapMode=TextureWrapMode.Clamp;
   Rect rect=new Rect(0,0,t.width,t.height);Vector2 anchor=pivot??new Vector2(.5f,.08f);
   if(name.StartsWith("actor-")||name.StartsWith("extra-")){rect=ManorActorRect(t);anchor=new Vector2((anchor.x*t.width-rect.x)/rect.width,(anchor.y*t.height-rect.y)/rect.height);}
   s=Sprite.Create(t,rect,anchor,t.width/width,0,SpriteMeshType.FullRect);
   s.name="Manor / "+name;ownedAssets.Add(s);manorSprites[key]=s;return s;
  }
  private void LoadManorArt()
  {
   for(int actor=0;actor<11;actor++){
    var frames=new Sprite[8];bool extra=actor>=6;
    for(int frame=0;frame<8;frame++)frames[frame]=ManorSprite((extra?"extra-":"actor-")+(extra?actor-6:actor)+"-"+frame,actor==5?2.45f:actor==3?1.65f:1.48f);
    enemyActionArt[(EnemyKind)((int)EnemyKind.ManorPatrol+actor)]=frames;
   }
   thunderFloor=ManorSprite("prop-0",3,Vector2.one*.5f);thunderWall=ManorSprite("prop-2",2,Vector2.one*.5f);
   for(int i=0;i<3;i++)chapterFloors[i]=thunderFloor;LoadManorFinaleArt();
  }
  private void BuildManorGraph()
  {
   journeyRooms.Clear();chapterDoors.Clear();chapterNames=(string[])ManorRoomNames.Clone();
   Vector2Int[] cells={new Vector2Int(0,0),new Vector2Int(1,0),new Vector2Int(2,0),new Vector2Int(3,0),new Vector2Int(4,0),new Vector2Int(1,-1),new Vector2Int(2,-1),new Vector2Int(3,-1),new Vector2Int(4,-1),new Vector2Int(4,1),new Vector2Int(1,-2),new Vector2Int(2,-2),new Vector2Int(3,-2),new Vector2Int(4,-2),new Vector2Int(5,1)};
   for(int i=0;i<15;i++){
    RoomKind kind=i==0?RoomKind.Entrance:i==2||i==7?RoomKind.Event:i==6||i==9?RoomKind.Rest:i==11?RoomKind.Reward:i==4||i==13?RoomKind.Elite:i==14?RoomKind.Boss:RoomKind.Combat;
    journeyRooms.Add(new RoomNode{id=i,stage=0,floor=0,depth=18+i/4,cell=cells[i],kind=kind,halfWidth=i==0?9:10,halfHeight=i==0?6:6.2f,layout=i%6,waves=kind==RoomKind.Boss?1:kind==RoomKind.Elite?4:kind==RoomKind.Combat?3:0});
   }
   int[,] links={{0,1},{1,2},{2,3},{3,4},{4,9},{9,14},{1,5},{5,6},{6,7},{7,8},{8,4},{5,10},{10,11},{11,12},{12,13},{13,8}};
   for(int i=0;i<links.GetLength(0);i++)ConnectRooms(links[i,0],links[i,1]);
   ExtendManorGraph();
  }
  private void BeginManorCourtyard(bool newSeed)
  {
   fireChapterMode=iceSampleMode=woodlandMode=thunderPrototypeMode=gatehouseSampleMode=bossPracticeMode=lightningTrialMode=false;thunderChapterMode=true;
   if(!tuningCaptured){initialSwordDamage=swordDamage;initialSpellDamage=spellDamage;tuningCaptured=true;}
   swordDamage=initialSwordDamage;spellDamage=initialSpellDamage;
   if(newSeed||activeSeed==0)activeSeed=journeySeed!=0&&!newSeed?journeySeed:Environment.TickCount&int.MaxValue;if(activeSeed==0)activeSeed=61001;
   ResetGatehouseProgress();ResetPlayerProgression();LoadThunderChapterEquipment();BuildManorGraph();LoadManorArt();
   chapterWest=chapterEast=chapterRanger=chapterStone=chapterEnding=chapterCg=chapterLampCarried=chapterLampGiven=false;
   manorProgress=manorEnding=manorChoiceMode=0;manorEndingSeen=false;chapterWaymarks=chapterShortcuts=0;chapterWork=0;chapterSavePending=chapterSetup=chapterRewardMenu=chapterSkillsMenu=false;
   clock=0;origin=thunderPreset==ThunderPreset.Dark?Origin.Dark:Origin.Light;energy=origin==Origin.Light?100:0;darkPower=origin==Origin.Dark?12:0;
   health=6;shield=EffectiveMaximumShield;elementPower=ElementMaximum;orbLearned=bladeLearned=waveLearned=windEmpowered=true;
   chapterMarkKnown=chapterArcKnown=iceSpikeKnown=iceRingKnown=fireBoltKnown=emberRingKnown=true;
   chapterSkillSlots[0]=0;chapterSkillSlots[1]=1;carriedWeapons[1]=7;ResetChapterCooldowns();
   nextSword=nextSpell=nextDash=dashUntil=invulnerableUntil=flashUntil=0;journeyComplete=pendingBossClear=won=false;doorTransition=0;
   EnterExplorationRoom(thunderChapterPracticeBoss>=0?stageEnds[Mathf.Clamp(thunderChapterPracticeBoss,0,2)]:stageStarts[Mathf.Clamp(thunderChapterStartFloor,0,2)],-1);
   manorIntro=thunderChapterPracticeBoss<0;manorIntroBegan=Time.unscaledTime;
   if(thunderChapterPracticeBoss>=0){manorProgress=7;journeyRooms[14].cleared=thunderChapterPracticeBoss>0;journeyRooms[29].cleared=thunderChapterPracticeBoss>1;chapterStone=journeyRooms[14].cleared;chapterWaymarks=3;journeyRooms[4].cleared=journeyRooms[13].cleared=true;TellStory("誓庭卫长练习 · R 重试，练习不写入府邸旅程存档。");}
   else TellStory("渡船在暮钟第三响时靠岸。没人迎接带回碎片的少年，只有城墙上的旧旗还朝着海。","石阶上留着运粮车的辙印，通向闭锁的外庭。少年将五岛的行路记录收好，走上灰石夹道。");
  }
  private bool ManorGateLocked(int room,int direction)
  {
   if(ManorLaterGateLocked(room,direction))return true;
   foreach(int next in journeyRooms[room].links)if(NeighborDirection(journeyRooms[room],journeyRooms[next])==direction&&next==14)
    return (chapterWaymarks&3)!=3||!journeyRooms[4].cleared||!journeyRooms[13].cleared;
   return false;
  }
  private void ValidateManorCheckpoint(ChapterCheckpoint s)
  {
   if(!manorCourtyardMode)return;
   if(s.manorProgress<0||s.manorProgress>63||s.manorEnding<0||s.manorEnding>3||s.rooms[29].cleared&&(s.manorProgress&7)!=7||s.rooms[44].cleared&&!s.rooms[29].cleared||(s.ending||s.cgPending)&&(!s.rooms[44].cleared||s.manorEnding==0)||s.rooms[14].cleared&&((s.waymarks&3)!=3||!s.rooms[4].cleared||!s.rooms[13].cleared))throw new System.IO.InvalidDataException("Incomplete manor courtyard investigation");
  }
  private void BuildManorRoom(RoomNode n)
  {
   ReserveTransitSpace(n);doorSeals.Clear();doorSealDirections.Clear();doorShotBlocks.Clear();thunderRain.Clear();thunderFlow.Clear();
   if(n.id==0){
    var s=ManorSprite("harbor",n.halfWidth*2,Vector2.one*.5f);Stretch("Manor sea and stone quay",s,Vector2.zero,new Vector2(n.halfWidth*2,n.halfHeight*2),Color.white,-1000);
    // Authored shoreline: upper court above y=-.8, only the central pier extends south.
    projectileObstacles.Add(new Rect(-9,-6,7.65f,5.2f));projectileObstacles.Add(new Rect(1.15f,-6,7.85f,5.2f));
    ThunderTile("Grey path through quay opening",DoorPoint(2)+Vector2.right*.7f,new Vector2(1.4f,2.6f),-994);
    BuildManorGate(2);BuildChapterActions();return;
   }
   ThunderTile("Manor grey flagstone",Vector2.zero,new Vector2(n.halfWidth*2,n.halfHeight*2),-1000);
   for(int d=0;d<4;d++){
    bool door=HasDoor(d);float span=d<2?n.halfWidth:n.halfHeight;if(door){ThunderTile("Grey courtyard passage",DoorPoint(d)+DoorNormal(d)*.8f,d<2?new Vector2(2.8f,2.2f):new Vector2(2.2f,2.8f),-994);BuildManorGate(d);}
    for(float x=-span;x<span;x+=1.8f){float end=Mathf.Min(span,x+1.8f);if(door&&x<1.4f&&end>-1.4f){if(x< -1.4f)ThunderBoundary(d,x,-1.4f);if(end>1.4f)ThunderBoundary(d,1.4f,end);}else ThunderBoundary(d,x,end);}
   }
   BuildManorLaterScenery(n);
   if(IsCombatRoom(n)){
    Vector2[] cover=n.kind==RoomKind.Boss?new[]{new Vector2(-6,2),new Vector2(6,-2)}:new[]{new Vector2(-4.8f,-2),new Vector2(4.8f,1.8f),new Vector2(-3.6f,2.7f),new Vector2(3.6f,-2.7f)};
    for(int i=0;i<cover.Length;i++)ManorCover(cover[(i+n.layout)%cover.Length],i%2==0?3:4);
   }
   if(n.id==10)ManorCover(new Vector2(0,1.6f),5);
   BuildChapterActions();
  }
  private void BuildManorGate(int direction)
  {
   Vector2 center=DoorPoint(direction);float angle=direction<2?0:90;
   GameObject frame=Stretch("Manor open gate frame",ManorSprite("prop-9",3.8f,Vector2.one*.5f),center,new Vector2(3.8f,1.6f),Color.white,DepthAt(center.y)+5);frame.transform.rotation=Quaternion.Euler(0,0,angle);
   var gate=Stretch("Manor closed gate",ManorSprite("prop-8",3.8f,Vector2.one*.5f),center,new Vector2(3.8f,1.6f),Color.white,DepthAt(center.y)+6);gate.transform.rotation=frame.transform.rotation;doorSeals.Add(gate);doorSealDirections.Add(direction);
  }
  private void ManorCover(Vector2 at,int prop)
  {
   if(IntersectsTransit(at,new Vector2(2.3f,2))||Blocked(at,.95f))return;
   Sprite art=ManorSprite("prop-"+prop,prop==5?2.5f:2);Draw("Manor solid cover",art,at,Vector2.one,Color.white,DepthAt(at.y));
   Rect footprint=new Rect(at.x-.65f,at.y-.08f,1.3f,.48f);obstacles.Add(footprint);projectileObstacles.Add(footprint);
  }
  private void ManorHarborFooting()
  {
   harborRoom=currentRoom;
   foreach(Rect r in new[]{new Rect(-9,-6,7.65f,5.2f),new Rect(1.15f,-6,7.85f,5.2f),new Rect(-9,4.2f,18,1.8f)}){obstacles.Add(r);harborBanks.Add(r);}
  }
  private void BuildManorActions(RoomNode n)
  {
   if(!n.cleared)return;BuildManorLaterActions(n);
   if(n.id==0)AddChapterAction(601,new Vector2(.1f,-3.1f),"登船 · 展开 Naki 航图");
   if(n.id==2){AddChapterAction(602,new Vector2(-2,0),"查看勇者石像上的旧刻痕",ManorSprite("prop-6",2));}
   if(n.id==7)AddChapterAction(603,new Vector2(-2,0),"核对受封名册与旧委任书",ManorSprite("prop-7",1.65f));
   if(n.id==6||n.id==9)AddChapterAction(604,new Vector2(-2,0),n.claimed?"查看营房行路记录":"整顿行装 · 本处补给一次",ManorSprite("prop-7",1.4f));
   if(n.id==11&&!n.claimed)AddChapterAction(920,new Vector2(0,0),"查看巡卫封存物资",ManorSprite("prop-5",2.3f));
   if(n.id==14&&chapterStone){AddChapterAction(605,new Vector2(0,.2f),"与誓庭卫长交谈",enemyActionArt[EnemyKind.ManorCaptain][7]);AddManorPassage(606,15);}
  }
  private void ManorAction(int action)
  {
   RoomNode n=journeyRooms[currentRoom];
   if(ManorFinaleAction(action))return;
   if(action==601){OpenWorldAtlas(5);return;}
   if(action==602){chapterWaymarks|=1;TellStory("石像里的少年握着一柄磨损的短剑。底座刻着五座岛的旧名，和你行囊里的记录一一对应。","最后一行被人刮去，只剩半句话：‘带回最后一片，我就……’",origin==Origin.Dark?"右眼里的石头微微发烫。这里有与它相同的痕迹，却没有可供收取的余烬。":"掌心的光照亮刮痕。刻字的人曾经救过这些岛上的人，名字却被藏了起来。");}
   if(action==603){chapterWaymarks|=2;TellStory("名册夹页中有一封旧委任书。航线、取回碎片的命令，甚至国王的印章，都与你带来的那封相同。","签名已经褪色。旁边有人用新的墨水补了一句：‘勇者归来后，粮道仍未开。’","少年：先问清楚外庭的守卫。被关在这里的人，也该有回家的路。");}
   if(action==604){if(!n.claimed){n.claimed=true;health=Mathf.Min(6,health+1.5f);shield=Mathf.Min(EffectiveMaximumShield,shield+2);elementPower=Mathf.Min(ElementMaximum,elementPower+25);AnnounceDiscovery("行装整备 · 生命+1.5，护盾+2，元素+25");}TellStory("营房的墙上钉着一张旧村粮道图。巡卫的口粮按月发下去，送往村落的车却已经停了很久。","旧行路记录：弩塔与断旗台仍听从外庭军令。若想面见卫长，先取回两处的通行印，再带上受封登记处的名册。");}
   if(action==605)TellStory("誓庭卫长：你带来的，也是那样一封委任书。",origin==Origin.Dark?"卫长望向你的右眼：他从前也带着这块石头。起初，村里的人见了他，总会把门打开。":"誓庭卫长：他从前也带着这样的光。起初，村里的人见了他，总会把门打开。","少年：后来呢？","誓庭卫长：去粮道后面看看吧。门我替你留着，答案不该只由我来说。");
   if(action==606){if(!chapterStone)return;EnterExplorationRoom(15,-1);TellStory("通行印嵌进门廊，沉重的门闩缓缓退开。街巷尽头，有人正把空锅从屋里抬出来。","风里没有饭香，只有一股久未散去的药味。");return;}
   BuildChapterActions();UpdateExplorationDoors();RequestCheckpoint();SaveChapterCheckpoint(false);
  }
  private string ManorObjective()
  {
   string full=ManorFullObjective();if(full!=null)return full;
   if(chapterStone)return "外庭已平息 · 询问卫长，调查旧村封门；G 可回访群岛";
   if((chapterWaymarks&1)==0)return "沿石狮夹道向东 · 调查勇者纪念庭";
   if((chapterWaymarks&2)==0)return "寻找受封登记处 · 核对旧委任书";
   if(!journeyRooms[4].cleared||!journeyRooms[13].cleared)return "压下废弩塔与断旗台的守军 · 取得外庭通行印";
   return "誓庭封锁已解除 · 经北侧前室面见卫长";
  }
  private void CompleteManorCaptain()
  {
   foreach(Enemy e in enemies){if(e.root!=null)Destroy(e.root);if(e.shadow!=null)Destroy(e.shadow);if(e.warning!=null)Destroy(e.warning);}enemies.Clear();
   foreach(var p in pendingSpawns)if(p.mark!=null)Destroy(p.mark);pendingSpawns.Clear();foreach(var s in shots)if(s.root!=null)Destroy(s.root);shots.Clear();ClearManorImpacts();ClearThunderHazards();
   chapterStone=true;journeyComplete=false;ClearJourneyRoom();BuildChapterActions();UpdateExplorationDoors();
   TellStory("卫长将长枪横放在地上，单膝跪下。盔甲里的呼吸仍然沉重。","誓庭卫长：通行印，你拿去。可别再让村里的人，为一封委任书付账。","通行印落在石阶上。少年没有立刻离开，等着他把话说完。");
   RequestCheckpoint();SaveChapterCheckpoint(false);
  }
  private void AppendManorWeapon(List<WeaponDef> items)
  {
   WeaponDef spear=Weapon("誓庭卫枪",WeaponType.Spear,Element.Physical,38,.72f,3.1f,1,2.35f,null,"d4dfec");
   spear.family=132;spear.sourceIsland=6;spear.signature=true;spear.maxHits=3;items.Add(spear);
   var stamp=Weapon("掌印断令锤",WeaponType.Sword,Element.Physical,44,.95f,2.2f,2,1.5f,null,"e9b95c");stamp.family=133;stamp.sourceIsland=6;stamp.signature=true;items.Add(stamp);
   var oath=Weapon("暮誓断链剑",WeaponType.Sword,Element.Physical,35,.55f,2.4f,2,1.6f,null,"a783d6");oath.family=134;oath.sourceIsland=6;oath.signature=true;oath.maxHits=3;items.Add(oath);
  }
  private void ExtendManorWeaponIdentity()
  {
   if(weaponIdentities==null||weaponIdentities.Length!=132)return;
   var t=Resources.Load<Texture2D>("NakiArt/ManorCourtyardV2/captain-spear");if(t==null)throw new InvalidOperationException("Missing captain spear art");
   Array.Resize(ref weaponIdentities,135);weaponIdentities[132]=new WeaponIdentity{id=132,displayName="誓庭卫枪",sheet="NakiArt/ManorCourtyardV2/captain-spear",width=t.width,height=t.height,pivotX=.20f,pivotY=.553f,form="thrust",cast="weapon_132_cast",hit="weapon_132_hit",accent=4,pixelHash="manor-captain-spear-v2"};
   var sheet=Resources.Load<Texture2D>("NakiArt/ManorFinaleV3/weapons");for(int i=0;i<2;i++)weaponIdentities[133+i]=new WeaponIdentity{id=133+i,displayName=i==0?"掌印断令锤":"暮誓断链剑",sheet="NakiArt/ManorFinaleV3/weapons",x=i*sheet.width/2,y=0,width=sheet.width/2,height=sheet.height,pivotX=.18f,pivotY=.5f,form=i==0?"hammer":"crescent",cast=i==0?"weapon_011_cast":"weapon_000_cast",hit=i==0?"weapon_011_hit":"weapon_000_hit",accent=i==0?4:5,pixelHash="manor-finale-weapon-"+i};
  }
 }
}
