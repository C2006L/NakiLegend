using System;
using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private bool woodEndingCgSeen;
  private readonly Dictionary<int,Sprite> woodlandNature=new Dictionary<int,Sprite>();
  private readonly List<Rect> woodlandCoverLanes=new List<Rect>();
  private bool WoodlandCoverLaneReserved(Vector2 at,Vector2 size){Rect candidate=new Rect(at-size*.5f,size);foreach(Rect lane in woodlandCoverLanes)if(lane.Overlaps(candidate))return true;return false;}
  private float WoodlandGrowth {get{return woodlandMode&&journeyRooms.Count==45&&journeyRooms[18].cleared?1:0;}}
  private Sprite WoodlandNature(int id)
  {
   Sprite s;if(woodlandNature.TryGetValue(id,out s)&&s!=null)return s;
   var t=Resources.Load<Texture2D>("NakiArt/WoodlandV2/nature");if(t==null)throw new InvalidOperationException("Missing woodland nature atlas");t.filterMode=FilterMode.Point;
   Rect[] regions={new Rect(0,20,735,410),new Rect(770,90,750,300),new Rect(0,500,685,480),new Rect(690,400,846,624)};
   Rect r=regions[id];r=new Rect(r.x*t.width/1536,(1024-r.yMax)*t.height/1024,r.width*t.width/1536,r.height*t.height/1024);
   s=Sprite.Create(t,r,new Vector2(.5f,.12f),r.height/(id==3?2.9f:id==0?1.7f:1.05f),0,SpriteMeshType.FullRect);ownedAssets.Add(s);woodlandNature[id]=s;return s;
  }
  private void LoadWoodlandV2Art()
  {
   string[] names={"forest-floor","fungal-floor","canopy-floor"};
   for(int i=0;i<3;i++){var t=Resources.Load<Texture2D>("NakiArt/WoodlandV2/"+names[i]);if(t==null)throw new InvalidOperationException("Missing woodland floor "+names[i]);t.filterMode=FilterMode.Point;t.wrapMode=TextureWrapMode.Repeat;var s=Sprite.Create(t,new Rect(0,0,t.width,t.height),Vector2.one*.5f,t.width/5f,0,SpriteMeshType.FullRect);ownedAssets.Add(s);chapterFloors[i]=s;}
   var frames=new Sprite[16];for(int i=0;i<16;i++)frames[i]=WoodlandNature(3);enemyActionArt[WoodKinds[9]]=frames;
   var knots=new Sprite[16];for(int i=0;i<16;i++)knots[i]=WoodlandNature(2);enemyActionArt[EnemyKind.WoodParasite]=knots;
   chapterEndingArt=Resources.Load<Texture2D>("NakiArt/WoodlandV2/seedling-ending");
  }
  private void RefineWoodGraph()
  {
   int[][] ids={new[]{0,1,2,3,4,5,7,8,9,10,11,39,40,41,6},new[]{12,13,14,15,16,17,19,20,21,22,23,24,42,43,18},new[]{25,26,27,28,29,30,32,33,34,35,36,37,38,44,31}};
   string[][] names={new[]{"雾岸码头","蜂鸣栈径","断藤岔口","木箱旧仓","旧叶种床","村口根道","风倒木庭","棘甲浅坡","蛙声洼地","荚果窄庭","避风火塘","林册残页","蜂巢外沿","母虫巢口","棘甲母虫"},new[]{"空心树门","腐木浅坑","封丝树孔","悬根遗箱","空果壳地","采种人牢笼","白丝断床","盘根双径","休眠种根","腐根长廊","苔下休憩","夜菌小庭","孢囊外庭","菌冠前室","菌冠孢囊"},new[]{"枝间升台","夜灯树桥","折枝鹿径","青脉遗箱","归鸟枝台","林海望台","垂根双环","螟灯旧庭","蜈行裂木","风穿叶廊","最后休憩","种床回梯","枯叶内庭","古树根结","古树守望者"}};
   for(int floor=0;floor<3;floor++)for(int i=0;i<15;i++){
    RoomNode n=journeyRooms[ids[floor][i]];chapterNames[n.id]=names[floor][i];
    if(i==3)n.kind=RoomKind.Reward;else if(i==10)n.kind=RoomKind.Rest;
    if(n.id==14||n.id==16||n.id==21)n.kind=RoomKind.Event;
    if(n.id==44)n.kind=RoomKind.Elite;
    if(n.kind==RoomKind.Combat||n.kind==RoomKind.Elite)n.waves=n.kind==RoomKind.Elite?5:floor==0&&(i==1||i==2)?3:4;
    // Preserve cell positions, links, IDs and old clear flags, so every historical save remains usable.
    n.layout=(n.id+activeSeed)%6;
   }
  }
  private void AddWoodCover(Vector2 at,int style,float scale=1)
  {
   Sprite art=WoodlandNature(style);Rect room=CurrentRoomBounds;
   // Leave a full walking lane behind every cover piece, including its canopy.
   // A floor-bound image alone can still trap the hero in the gap against a wall.
   Vector2 min=(Vector2)art.bounds.min*scale,max=(Vector2)art.bounds.max*scale;
   at=new Vector2(Mathf.Clamp(at.x,room.xMin+1.2f-min.x,room.xMax-1.2f-max.x),Mathf.Clamp(at.y,room.yMin+1.2f-min.y,room.yMax-1.2f-max.y));
   Vector2 center=at+(min+max)*.5f;
   if(IntersectsTransit(center,max-min+Vector2.one*.25f))return;
   Rect body=new Rect(at+min,max-min),lane=Rect.MinMaxRect(body.xMin-1.1f,body.yMin-1.1f,body.xMax+1.1f,body.yMax+1.1f);
   foreach(Rect existing in obstacles)if(lane.Overlaps(existing))return;
   Draw("Woodland natural cover",art,at,Vector2.one*scale,Color.white,DepthAt(at.y));
   // Shared body prevents walking into a canopy that blocks the weapon muzzle.
   Rect solid=Rect.MinMaxRect(body.xMin+body.width*.16f,body.yMin+body.height*.08f,body.xMax-body.width*.16f,body.yMin+body.height*(style==1?.67f:.48f));
   obstacles.Add(solid);projectileObstacles.Add(solid);woodlandCoverLanes.Add(lane);
  }
  private void BuildWoodlandCover(RoomNode n)
  {
   woodlandCoverLanes.Clear();if(n.kind==RoomKind.Rest||n.kind==RoomKind.Event||n.kind==RoomKind.Reward)return;
   Vector2[][] layouts={new[]{new Vector2(-3.5f,-1.3f),new Vector2(3.5f,1)},new[]{new Vector2(-3.5f,1),new Vector2(3.5f,-1.3f)},new[]{new Vector2(-3.5f,1),new Vector2(3.5f,1),new Vector2(0,-1.5f)},new[]{new Vector2(-3.5f,-1.3f),new Vector2(3.5f,-1.3f)},new[]{new Vector2(-3.5f,-1.3f),new Vector2(3.5f,1)},new[]{new Vector2(-3.5f,1),new Vector2(3.5f,-1.3f)}};
   var chosen=n.kind==RoomKind.Boss?new[]{new Vector2(-4.5f,2.15f),new Vector2(3.5f,-1.3f)}:layouts[n.layout%6];
   for(int i=0;i<chosen.Length;i++)AddWoodCover(chosen[i],n.kind==RoomKind.Boss?1:(n.stage+i)%2,.8f);
  }
  private void ReserveWoodlandProps(RoomNode n)
  {
   if(n.id==17){transitReservations.Add(new Rect(-5.3f,-1.5f,6.6f,5.5f));return;}
   if(n.id==0||n.id==4||n.id==6||n.id==14||n.id==16||n.id==18||n.id==21||n.id==31)
    transitReservations.Add(new Rect(-4.3f,-1.9f,4.6f,5));
  }
  private void WoodRoomReward(RoomNode n)
  {
   if(n.kind==RoomKind.Elite){DropIslandEliteReward(n);return;}
   if(n.lootSeeded||n.kind==RoomKind.Boss||n.kind==RoomKind.Reward)return;n.lootSeeded=true;
   if(n.kind==RoomKind.Combat||n.kind==RoomKind.Elite||n.kind==RoomKind.Reward){
    if(n.kind==RoomKind.Reward)ChapterDrop(profession==Profession.Mage?28:profession==Profession.Archer?31:32,new Vector2(-2,0));
    if(n.kind==RoomKind.Elite)ChapterDrop(n.stage==0?27:n.stage==1?29:30,new Vector2(-2,0));
    var e=new StoredEssence{at=FindFreeInteractionPoint(new Vector2(2,0)),value=n.kind==RoomKind.Elite?40:25,born=clock};n.essence.Add(e);DrawEssence(e);
   }
  }
  private int WoodlandGoal {get{return !woodlandMode?-1:chapterStone?(chapterLampGiven?0:4):journeyRooms[currentRoom].stage==0?6:journeyRooms[currentRoom].stage==1?(!chapterRanger?17:WoodClues<2?((chapterWaymarks&1)==0?14:16):18):31;}}
  private bool WoodFinalBossCleared {get{return woodlandMode&&journeyRooms.Count==45&&journeyRooms[31].cleared&&chapterStone;}}
  private bool WoodCanEnd {get{return WoodFinalBossCleared&&chapterLampGiven;}}
  private int WoodClues {get{int n=0;for(int i=0;i<3;i++)if((chapterWaymarks&(1<<i))!=0)n++;return n;}}
  private void MigrateWoodlandCheckpoint(ChapterCheckpoint s){if(woodlandMode&&s!=null&&s.island=="woodland"&&s.map=="w3-v1"&&s.rooms!=null&&s.rooms.Length==45){MigrateOldWoodWeapons(s);s.map="w3-v2";}}
  private void DrawWoodEnding(float w,float h)
  {
   HudRect(new Rect(0,0,w,h),Color.black);if(chapterEndingArt!=null)GUI.DrawTexture(new Rect(0,0,w,h),chapterEndingArt,ScaleMode.ScaleToFit);
   HudRect(new Rect(0,h-145,w,145),new Color(.025f,.055f,.035f,.92f));CenterLabel(new Rect(20,h-132,w-40,40),"旧叶落处，新芽有声",journeyTitleStyle);
   CenterLabel(new Rect(20,h-85,w-40,36),"留一粒吧。不是怕它不回来，是等下一次春天。",journeyTextStyle);
   CenterLabel(new Rect(20,h-43,w-40,30),"F / ESC 继续 · 木元素已寻回 · 林地重新有了生长与凋落的季节",journeySmallStyle);
  }
 }
}
