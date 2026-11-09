using System;
using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private static string[] FireStages {get{return AtlasRegions[3];}}
  private bool fireHeartRecovered,fireHearthLit;
  private int fireEvacuation;
  private readonly List<GameObject> coreEvacuees=new List<GameObject>();
  private float coreEvacStarted=-1;
  private readonly Dictionary<int,Sprite> coreArt=new Dictionary<int,Sprite>();
  private Sprite CoreArt(int id)
  {
   Sprite s;if(coreArt.TryGetValue(id,out s)&&s!=null)return s;
   var t=Resources.Load<Texture2D>("NakiArt/FireCore/creatures");if(t==null)throw new InvalidOperationException("Missing Fire Core creatures");t.filterMode=FilterMode.Point;
   // The generated subjects have unequal widths. Explicit regions prevent neighboring-sprite leaks.
   Rect[] regions={new Rect(0,0,780,710),new Rect(795,150,455,560),new Rect(0,710,752,540),new Rect(770,770,484,484)};
   Rect r=regions[id];r=new Rect(r.x*t.width/1254,(1254-r.yMax)*t.height/1254,r.width*t.width/1254,r.height*t.height/1254);
   s=Sprite.Create(t,r,new Vector2(.5f,.10f),r.height/(id==0?3.65f:id==2?1.7f:id==3?1.65f:1.5f),0,SpriteMeshType.FullRect);ownedAssets.Add(s);coreArt[id]=s;return s;
  }
  private void LoadCoreArt()
  {
   var t=Resources.Load<Texture2D>("NakiArt/FireCore/floor");if(t==null)throw new InvalidOperationException("Missing Fire Core floor");t.filterMode=FilterMode.Point;t.wrapMode=TextureWrapMode.Repeat;
   var floor=Sprite.Create(t,new Rect(0,0,t.width,t.height),Vector2.one*.5f,t.width/5f,0,SpriteMeshType.FullRect);ownedAssets.Add(floor);chapterFloors[2]=floor;
   EnemyKind[] kinds={EnemyKind.MagmaTortoise,EnemyKind.EmberAcolyte,EnemyKind.AshPackbeast};
   for(int i=0;i<3;i++){var frames=new Sprite[16];for(int f=0;f<16;f++)frames[f]=CoreArt(i);enemyActionArt[kinds[i]]=frames;}
   chapterEndingArt=Resources.Load<Texture2D>("NakiArt/FireCore/hearth-ending");
  }
  private void AddFireCore()
  {
   string[] names={"地心避难站","蒸汽石径","灼风回廊","冷却石台","双炉夹道","旧冷却渠","灰潮石环","遗锻装备箱","熔脉长廊","余烬庭院","守炉哨厅","赤炉前庭","赤潮熔龟","灰烬撤离道","归岸石廊"};
   Array.Resize(ref chapterNames,45);
   for(int id=30;id<45;id++){
    int local=id-30;RoomKind kind=local==0||local==3||local==11?RoomKind.Rest:local==12?RoomKind.Boss:local==2||local==9?RoomKind.Elite:local==5?RoomKind.Event:local==7?RoomKind.Reward:RoomKind.Combat;
    // The final two rooms form a separate, gated evacuation lane after the guardian.
    Vector2Int cell=local==0?new Vector2Int(-1,0):local==13?new Vector2Int(2,3):local==14?new Vector2Int(3,3):new Vector2Int((local-1)%5,(local-1)/5);
    chapterNames[id]=names[local];journeyRooms.Add(new RoomNode{id=id,stage=2,floor=2,cell=cell,halfWidth=local==12?10:9.4f,halfHeight=local==12?5.6f:5.2f,kind=kind,waves=local>=13?2:4,layout=id,depth=20,stairsTo=-1});
   }
   for(int a=30;a<43;a++)for(int b=a+1;b<43;b++)if((journeyRooms[a].cell-journeyRooms[b].cell).sqrMagnitude==1)CoreLink(a,b);
   // 42 is (1,2); keep the rescue route explicitly attached on its east side.
   journeyRooms[43].cell=new Vector2Int(2,2);journeyRooms[44].cell=new Vector2Int(3,2);CoreLink(42,43);CoreLink(43,44);
   journeyRooms[29].stairsTo=30;journeyRooms[30].stairsTo=29;stageStarts[2]=30;stageEnds[2]=42;
  }
  private void CoreLink(int a,int b){if(!journeyRooms[a].links.Contains(b))journeyRooms[a].links.Add(b);if(!journeyRooms[b].links.Contains(a))journeyRooms[b].links.Add(a);}
  private void BuildCoreCover(RoomNode n)
  {
   if(n.kind==RoomKind.Rest||n.kind==RoomKind.Event||n.kind==RoomKind.Reward)return;
   Vector2[][] positions={new[]{new Vector2(-4.6f,2),new Vector2(4.6f,-2),new Vector2(.7f,2.7f)},new[]{new Vector2(-4.6f,-2),new Vector2(4.6f,2),new Vector2(-.7f,2.7f)},new[]{new Vector2(-4.5f,2),new Vector2(4.5f,2),new Vector2(-2.7f,-2.4f),new Vector2(2.7f,-2.4f)}};
   foreach(var at in positions[n.kind==RoomKind.Boss?2:n.layout%3]){
    if(IntersectsTransit(at,new Vector2(2.2f,2.2f)))continue;Sprite art=CoreArt(3);Draw("Cooled basalt cover",art,at,Vector2.one,Color.white,DepthAt(at.y));
    AddRoundedCover(new Rect(at.x-.55f,at.y-.06f,1.1f,.4f),new Rect(at+(Vector2)art.bounds.min,(Vector2)art.bounds.size));
   }
  }
  private void SpawnCoreWave(RoomNode n)
  {
   if(n.id==42){SpawnJourneyEnemy(EnemyKind.MagmaTortoise,new Vector2(0,1),n.depth);return;}
   var rng=new System.Random(activeSeed+n.id*421+currentWave*97);int count=n.id>=43?5:currentWave==1?5:6,ranged=0,cores=0;
   EnemyKind[] pool={EnemyKind.ForgeGuard,EnemyKind.AshPackbeast,EnemyKind.AshBat,EnemyKind.MoltenCrab,EnemyKind.EmberAcolyte,EnemyKind.FurnaceCore,EnemyKind.EmberLizard};
   for(int i=0;i<count;i++){
    EnemyKind k=i==0?EnemyKind.EmberAcolyte:pool[rng.Next(pool.Length)];
    if(k==EnemyKind.FurnaceCore&&cores++>=1)k=EnemyKind.ForgeGuard;
    if((k==EnemyKind.EmberAcolyte||k==EnemyKind.FurnaceCore||k==EnemyKind.EmberLizard)&&ranged++>=2)k=EnemyKind.AshPackbeast;
    QueueEnemy(k,FindJourneySpawn(rng,.65f),n.kind==RoomKind.Elite&&i==0);pendingSpawns[pendingSpawns.Count-1].when=clock+1.4f+i*.32f;
   }
  }
  private void BuildCoreActions(RoomNode n)
  {
   if(n.id>=43)BuildEvacuees(n);
   if(!n.cleared)return;
   if(n.id==30)AddChapterAction(fireHeartRecovered&&fireEvacuation==3?510:501,new Vector2(-3,1),fireHeartRecovered&&fireEvacuation==3?"随撤离队返回黑砂码头 · 点燃公共炉灶":"与阿铎商量最后的撤离",ResidentSprite(0));
   if(n.id==35)AddChapterAction(502,new Vector2(-2,0),n.claimed?"冷却渠已通水":"清理旧冷却渠",WorkshopProp(2));
   if(n.id==42&&fireHeartRecovered)AddChapterAction(503,new Vector2(2,-1),"沿东侧石廊接应最后的居民",FireProp(2));
   if(n.id==43)AddChapterAction(504,new Vector2(-6.6f,1f),(fireEvacuation&1)!=0?"第一批居民已到安全区":"避难门已开启 · 接应第一批居民");
   if(n.id==44){if((fireEvacuation&2)==0)AddChapterAction(505,new Vector2(-6.6f,1f),"接应物资车与最后一批居民");else AddChapterAction(508,new Vector2(2,-.5f),"沿归岸炉道返回黑砂码头 · 点燃公共炉灶");}
   BuildWorkshopGuide(n);
  }
  private void BuildEvacuees(RoomNode n,bool departing=false)
  {
   coreEvacuees.Clear();coreEvacStarted=-1;
   BuildCoreRefuge(n,departing);
  }
  private void TickCoreEvacuation(){if(fireChapterMode&&currentRoom==43&&coreEvacStarted>=0)UpdateCoreEvacuation(Time.unscaledTime-coreEvacStarted);}
  private void UpdateCoreEvacuation(float elapsed)
  {
   bool moving=false;
   for(int i=0;i<coreEvacuees.Count;i++){
    var person=coreEvacuees[i];if(person==null)continue;float distance=Mathf.Max(0,elapsed-i*.45f)*3.8f;
    Vector2[] route={new Vector2(-7.1f+i*1.05f,1.65f),new Vector2(-5.8f,0),new Vector2(7.3f,0),DoorPoint(2,1.1f)};Vector2 at=route[0];bool done=true;
    for(int leg=1;leg<route.Length;leg++){float length=Vector2.Distance(route[leg-1],route[leg]);if(distance<length){at=Vector2.Lerp(route[leg-1],route[leg],distance/length);done=false;break;}distance-=length;at=route[leg];}
    person.transform.position=at;var sr=person.GetComponent<SpriteRenderer>();sr.sortingOrder=DepthAt(at.y);sr.flipX=false;if(done)person.SetActive(false);else moving=true;
   }
   if(!moving)coreEvacStarted=-1;
  }
  private void CoreAction(int a)
  {
   if(a==501)TellStory("阿铎：火还困在山腹里。先把那块束住守护兽的石头取出来，热流才肯走旧渠。","少年：我去让炉火回到该去的地方。你守住这里，等最后一批人回来。");
   if(a==502&&currentRoom==35){RoomNode n=journeyRooms[currentRoom];if(!n.claimed){n.claimed=true;elementPower=Mathf.Min(ElementMaximum,elementPower+40);TellStory("少年拨开堵在渠口的灰渣。山泉贴着石槽流过，蒸汽向穹顶散去。","阿铎：留一点凉意给人走，也留一条路给山呼吸。");}}
   if(a==503)JourneyHint("从东侧石门前往灰烬撤离道 · 居民在入口安全处等候");
   if(a==504&&currentRoom==43&&journeyRooms[43].cleared&&fireHeartRecovered){if((fireEvacuation&1)!=0){JourneyHint("第一批居民已抵达安全区 · 继续向东接应最后一批人");return;}fireEvacuation|=1;TellStory("前方的火光终于不再逼近。医师从避难门后扶出居民，炉工推着小车沿清理过的石廊前往岸边。","阿铎：还有一辆物资车在后面。我去接，你替我们守住归岸的路。");}
   if(a==505)PrepareCoreReturn();
   if(a==508||a==510)TravelCoreToHarbor();
   if(a==506&&currentRoom==0){if(!fireHeartRecovered||fireEvacuation!=3){JourneyHint("先接回石廊里最后的居民与物资");return;}QueueOriginMoment(3);fireHearthLit=true;chapterCg=true;storyLines=null;}
   if(a==507){OpenWorldAtlas(4);return;}
   BuildChapterActions();if(a==504&&currentRoom==43){BuildEvacuees(journeyRooms[currentRoom],true);coreEvacStarted=Time.unscaledTime;}UpdateExplorationDoors();RequestCheckpoint();SaveChapterCheckpoint(false);
  }
  private string CoreObjective(){return !fireHeartRecovered?"沿冷却石径深入 · 寻找束缚赤潮熔龟的火石":(fireEvacuation&1)==0?"前往灰烬撤离道 · 接应第一批居民":fireEvacuation!=3?"前往归岸石廊 · 护送物资与最后的居民":"返回黑砂码头 · 点燃公共炉灶";}
  private void DrawFireEnding(float w,float h)
  {
   HudRect(new Rect(0,0,w,h),Color.black);if(chapterEndingArt!=null)GUI.DrawTexture(new Rect(0,0,w,h),chapterEndingArt,ScaleMode.ScaleToFit);
   HudRect(new Rect(0,h-145,w,145),new Color(.04f,.025f,.02f,.92f));CenterLabel(new Rect(20,h-132,w-40,40),"炉火重新升起",journeyTitleStyle);
   CenterLabel(new Rect(20,h-85,w-40,36),"火没有熄灭。它回到了炉膛，回到了人们的生活里。",journeyTextStyle);
   CenterLabel(new Rect(20,h-43,w-40,30),"F / ESC 继续 · 火元素已寻回 · 下一段航程：青栖林海",journeySmallStyle);
  }
 }
}
