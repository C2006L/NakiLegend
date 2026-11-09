using System;
using UnityEngine;
namespace NakiLegend.Prototype {
 public sealed partial class NakiPrototype {
  private void ExpandCapitalGraph(){
   Vector2Int[] cells={new Vector2Int(0,0),new Vector2Int(2,0),new Vector2Int(3,0),new Vector2Int(2,1),new Vector2Int(3,1),new Vector2Int(4,1),new Vector2Int(2,2),new Vector2Int(3,2),new Vector2Int(4,2)};
   foreach(RoomNode n in journeyRooms)n.links.Clear();
   for(int stage=0;stage<3;stage++){
    int b=stage*9,extra=27+stage*5;for(int i=0;i<9;i++)journeyRooms[b+i].cell=cells[i];
    Vector2Int[] additions={new Vector2Int(1,0),new Vector2Int(4,0),new Vector2Int(0,1),new Vector2Int(1,1),new Vector2Int(1,2)};
    for(int i=0;i<5;i++){
     RoomKind kind=i==0||i==3?RoomKind.Combat:i==1?RoomKind.Elite:i==2?RoomKind.Event:RoomKind.Reward;
     journeyRooms.Add(new RoomNode{id=extra+i,stage=stage,floor=stage,depth=24+stage*3,cell=additions[i],kind=kind,halfWidth=i==2?8.5f:10,halfHeight=i==2?5.3f:6.3f,layout=(stage+i)%6,waves=kind==RoomKind.Combat?2:kind==RoomKind.Elite?3:0,stairsTo=-1});
    }
    int[] route={b,extra,b+1,b+2,extra+1,b+5,b+4,b+3,b+6,b+7,b+8};for(int i=1;i<route.Length;i++)ConnectRooms(route[i-1],route[i]);
    ConnectRooms(b,extra+2);ConnectRooms(extra+2,extra+3);ConnectRooms(extra+3,extra);ConnectRooms(b+6,extra+4);
   }
  }
  private void MigrateCapitalCheckpoint(ChapterCheckpoint s){
   if(!capitalMode||s==null||s.map!="capital-v1"||s.rooms==null||s.rooms.Length!=27)return;
   var expanded=new ChapterRoomState[42];Array.Copy(s.rooms,expanded,27);
   for(int i=27;i<42;i++){int stage=(i-27)/5;bool passed=s.rooms[stage*9+8]!=null&&s.rooms[stage*9+8].cleared;
    expanded[i]=new ChapterRoomState{cleared=passed,claimed=passed,weapons=Array.Empty<CheckpointWeapon>(),crystals=Array.Empty<CheckpointEssence>()};
   }
   s.rooms=expanded;s.map="capital-v2";
  }
  private void CapitalExpansionStory(RoomNode n){
   if(n.id<27||!n.cleared||n.claimed)return;int local=(n.id-27)%5;if(local>2)return;n.claimed=true;
   if(n.stage==0){
    if(local==0)TellStory("检查站的守军放下弩。港务员把两份告示摆在一起：居民手里的欢迎告示刚印好，封港令却早了三天。","港务员：他们原先说，只核对你的归航凭证。今早连替船员拿药的人，也被写进了不得离港的名单。前面长街上还在扣人。");
    if(local==1)TellStory("粮桥上的锁链断了，桥下停着几辆装满面粉的车。守粮人没有急着过桥，先让少年看了一眼空药箱。","守粮人：关税哨所扣了钥匙。先把那边的人放出来，我再带粮车走运粮小巷。避难所里的人等不了王宫回信。");
    if(local==2)TellStory("候船屋里，旧船票被压在一盏没有点燃的灯下。留守的妇人逐一念出船员的名字，每念一个，就把一只杯子放回桌上。","她说，欢迎仪式已筹备了很久。他们不知道，那张不能离港的名单也把自己算了进去。");
   }else if(n.stage==1){
    if(local==0)TellStory("工匠把旧水管图摊在街边。供水和贡纳共用一段总管，粗支路通向居民水槽，细支路一直伸进王宫。","工匠：整座泵都砸掉，明早这里就没有水了。先去航务档案室找原始名册，再到泵站辨清总闸，别照着那张新誊的图动手。");
    if(local==1)TellStory("汇流厅的铜表仍在转，运粮记录和征召名册用同一条紫线系在墙上。少年认出了暮钟岛的船名。","检修员：谁离开，谁的家属就补进担保栏。档案室留着第一次签署的原件；现在的誊本，把退出契约的那一行涂掉了。");
    if(local==2)TellStory("值班房里贴着一张手摇泵轮班表。工匠指着旧事故记录：北区曾因三日停水死过人，国王此后才把所有支路接进王印。","工匠：害怕再出事，我能理解。但水槽今天是谁修的，他也该来看看。");
   }else{
    if(local==0)TellStory("换防廊的近卫退到墙边。一名伤兵把未送出的交班记录交给少年，上面反复写着同一句话：返乡者不得再列作征召资源。","伤兵：原件在御前书房。别只拿我们的说法去见他。把那几封信看完，才知道他害怕什么，也才知道我们究竟失去了什么。");
    if(local==1)TellStory("审验台要求交出归航凭证，才能进入仪式守备厅。少年把名册和泵站检修图并排展开，停下的贡纳支路仍在纸背留下水印。","值守官看过之后让开了门：去书房吧。国王保留的旧信，和后来盖印的命令，都在那里。看完以后，议事厅还有一份没有签名的撤令草稿。");
    if(local==2)TellStory("旧誓约室的墙上没有胜利者画像，只有第一次水灾后的救援名单。签名里既有后来成为近卫的人，也有今天仍在值班的工匠。","少年看着那些不同的笔迹，把名册收好。王印曾经把他们联结起来，却不该替所有人决定余下的一生。");
   }
  }
  private void DressCapitalExpansion(RoomNode n){
   if(n.id<27)return;int local=(n.id-27)%5;
   if(local==2){Draw("Capital local witness",ResidentSprite(n.stage==0?1:n.stage==1?0:6),new Vector2(0,1),Vector2.one,Color.white,DepthAt(1));Draw("Capital witness shelter",CapitalProp(10),new Vector2(-4,2.5f),Vector2.one,Color.white,DepthAt(2.5f));}
   if(local==1){CapitalCover(new Vector2(-4,-1.5f),n.stage==0?5:0);CapitalCover(new Vector2(4,1.5f),n.stage==0?5:0);}
  }
 }
}
