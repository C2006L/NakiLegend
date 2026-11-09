using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  // IDs remain stable so existing bags, rewards, boss progress and save locations survive.
  private static readonly int[][] IceMainRoutes={new[]{0,1,5,6,10,7,11,12,2},new[]{14,15,16,17,19,20,22,23,26},new[]{28,29,30,31,33,34,36,37,39,40}};
  private void RebuildIceRoute()
  {
   foreach(RoomNode n in journeyRooms){n.links.Clear();n.stairsTo=-1;n.kind=RoomKind.Combat;n.waves=n.stage<2?3:4;n.depth=9+n.stage*4;}
   Vector2Int[] path={new Vector2Int(0,0),new Vector2Int(1,0),new Vector2Int(2,0),new Vector2Int(3,0),new Vector2Int(3,1),new Vector2Int(3,2),new Vector2Int(2,2),new Vector2Int(1,2),new Vector2Int(0,2)};
   for(int stage=0;stage<3;stage++){
    int[] route=IceMainRoutes[stage];for(int i=0;i<route.Length;i++){journeyRooms[route[i]].cell=stage<2?path[i]:new[]{new Vector2Int(0,0),new Vector2Int(1,0),new Vector2Int(2,0),new Vector2Int(3,0),new Vector2Int(4,0),new Vector2Int(4,1),new Vector2Int(4,2),new Vector2Int(3,2),new Vector2Int(2,2),new Vector2Int(1,2)}[i];journeyRooms[route[i]].depth+=i/2;if(i>0)IceRouteLink(route[i-1],route[i]);}
    journeyRooms[route[0]].kind=RoomKind.Event;RoomNode boss=journeyRooms[route[route.Length-1]];boss.kind=RoomKind.Boss;boss.waves=1;
   }
   IceRouteBranch(3,1,new Vector2Int(1,-1),RoomKind.Combat);IceRouteBranch(4,3,new Vector2Int(1,-2),RoomKind.Elite);
   IceRouteBranch(9,5,new Vector2Int(2,-1),RoomKind.Reward);IceRouteBranch(8,11,new Vector2Int(2,3),RoomKind.Rest);IceRouteBranch(13,12,new Vector2Int(1,3),RoomKind.Combat);
   IceRouteBranch(18,20,new Vector2Int(4,2),RoomKind.Combat);IceRouteBranch(21,16,new Vector2Int(2,-1),RoomKind.Reward);
   IceRouteBranch(24,17,new Vector2Int(4,0),RoomKind.Event);IceRouteBranch(25,23,new Vector2Int(1,3),RoomKind.Rest);IceRouteBranch(27,26,new Vector2Int(-1,2),RoomKind.Elite);
   IceRouteBranch(32,31,new Vector2Int(3,-1),RoomKind.Rest);IceRouteBranch(35,30,new Vector2Int(2,-1),RoomKind.Reward);IceRouteBranch(38,36,new Vector2Int(4,3),RoomKind.Combat);IceRouteBranch(41,40,new Vector2Int(0,2),RoomKind.Event);
   foreach(int id in new[]{6,19,23,33,37})journeyRooms[id].kind=RoomKind.Elite;
   foreach(RoomNode n in journeyRooms)if(n.kind==RoomKind.Elite)n.waves=n.stage==0?3:n.stage==1?4:5;
   foreach(int id in new[]{0,8,14,25,28,32,24,41,9,21,35})journeyRooms[id].waves=0;
   chapterNames[2]="晶甲庭";chapterNames[6]="霜甲哨所";chapterNames[8]="避风营地";chapterNames[9]="旧船匠遗箱";chapterNames[11]="断桨巡道";chapterNames[12]="破霜门廊";chapterNames[13]="雪崖支路";
   chapterNames[23]="护灯队哨所";chapterNames[24]="绞盘观测站";chapterNames[25]="镜前营地";chapterNames[26]="霜镜猎场";chapterNames[27]="极夜巡道";
   chapterNames[32]="晶庭营地";chapterNames[35]="守灯遗藏";chapterNames[40]="寒潮祭庭";
   journeyRooms[2].stairsTo=14;journeyRooms[14].stairsTo=2;journeyRooms[26].stairsTo=28;journeyRooms[28].stairsTo=26;
  }
  private void IceRouteLink(int a,int b){journeyRooms[a].links.Add(b);journeyRooms[b].links.Add(a);}
  private void IceRouteBranch(int id,int parent,Vector2Int at,RoomKind kind){journeyRooms[id].cell=at;journeyRooms[id].kind=kind;IceRouteLink(parent,id);}
  private bool IceRestRoom(RoomNode n){return n.id==8||n.id==25||n.id==32;}
  private void BuildIceQuietRoom(RoomNode n)
  {
   if(!n.cleared)return;
   if(IceRestRoom(n)){
    Vector2 at=new Vector2(-2,1);Draw("Sheltered camp lantern",ResidentSprite(4),at+Vector2.left,Vector2.one,Color.white,DepthAt(at.y)).transform.SetParent(chapterFocus.transform,true);
    Draw("Covered expedition cargo",IceExpansionSprite(3),at+Vector2.right*1.2f,Vector2.one,Color.white,DepthAt(at.y)).transform.SetParent(chapterFocus.transform,true);
    AddChapterAction(315,at,n.claimed?"营地已整备 · 查看物资记录":"营地整备 · 生命+2、护盾+2、元素+25");
   }
   if(n.id==14||n.id==28||n.id==24){Vector2 at=new Vector2(-2,1);Draw("Expedition route journal",ChapterRecordSprite,at,Vector2.one,Color.white,DepthAt(at.y)).transform.SetParent(chapterFocus.transform,true);AddChapterAction(316,at,n.id==24?"查阅绞盘观测记录":"查阅本层行路记录");if(n.id==24)Draw("Frozen observation winch",BossPropSprite(1),at+Vector2.right*2.3f,Vector2.one,C("bde4eb"),DepthAt(at.y)).transform.SetParent(chapterFocus.transform,true);}
   if(n.id==41&&!chapterStone){Draw("Dormant return-flow winch",BossPropSprite(1),new Vector2(-3,1),Vector2.one,Color.white,DepthAt(1)).transform.SetParent(chapterFocus.transform,true);AddChapterAction(316,new Vector2(-3,.2f),"查看回流闸室 · 等待冰石归位");}
  }
  private void ReadIceRouteRecord()
  {
   if(currentRoom==24)TellStory("观测簿：裂谷里的回声会放大冰镜的震动。遇到持弓的守卫，先绕货箱靠近，再留意后排执礼者的霜纹。","纸边夹着破冰队的求援条：营地在回廊外侧，绞盘冻住了，请循灯来找我们。");
   else if(currentRoom==41)TellStory("闸轴冻在石槽里。两侧铁链通向祭庭深处，只有让冰石归位，回流闸才肯重新转动。");
   else TellStory(currentRoom==14?"破冰队记录：离开峡口，穿过长桥与雪壳滩。霜纹执礼者占着必经的转角，队员困在回廊外侧的营地里。":"晶庭残页：碎霜庭院之后，守望者仍守着寒潮祭庭。取回冰石，再去西侧回流闸室，给海湾留一条归路。");
  }
 }
}
