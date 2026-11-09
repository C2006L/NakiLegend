using UnityEngine;
namespace NakiLegend.Prototype {
 public sealed partial class NakiPrototype {
  private int thunderPendingStoryRoom=-1;
  private bool ThunderStoryRoute {get{return thunderChapterMode&&!capitalMode&&!manorCourtyardMode&&!fireChapterMode&&!iceSampleMode&&!woodlandMode;}}
  private bool ThunderBossStoryReady(int stage){
   if(!ThunderStoryRoute||thunderChapterPracticeBoss>=0)return true;
   if(journeyRooms[stageEnds[stage]].cleared)return true;
   if(stage==0)return journeyRooms[8].cleared&&chapterWaymarks==7;
   if(stage==1)return chapterWest&&chapterEast&&chapterRanger&&journeyRooms[17].storyRead&&journeyRooms[20].cleared&&journeyRooms[21].claimed&&(chapterLampCarried||chapterLampGiven)&&chapterArcKnown;
   return journeyRooms[35].cleared&&journeyRooms[36].storyRead&&chapterWest&&chapterEast&&chapterRanger;
  }
  private bool ThunderStoryDoorLocked(int room,int direction){
   if(!ThunderStoryRoute)return false;
   foreach(int next in journeyRooms[room].links)if(NeighborDirection(journeyRooms[room],journeyRooms[next])==direction&&next==stageEnds[journeyRooms[next].stage])return !ThunderBossStoryReady(journeyRooms[next].stage);
   return false;
  }
  private string ThunderStoryGateReason(int room,int direction){
   return ThunderStoryDoorLocked(room,direction)?ThunderRouteObjective():null;
  }
  private int ThunderStoryGoal {
   get {
    if(!ThunderStoryRoute)return -1;int stage=journeyRooms[currentRoom].stage;
    if(stage==0){if(!journeyRooms[8].cleared)return 8;if(chapterWaymarks!=7)return 9;if(!journeyRooms[6].cleared||!chapterMarkKnown)return 6;return 12;}
    if(stage==1){if(!chapterWest)return 14;if(!chapterEast||!chapterArcKnown)return 16;if(!chapterRanger||!journeyRooms[17].storyRead)return 17;if(!journeyRooms[20].cleared)return 20;if(!journeyRooms[21].claimed)return 21;if(!chapterLampCarried&&!chapterLampGiven)return 22;return journeyRooms[18].cleared?25:18;}
    if(!journeyRooms[35].cleared)return 35;if(!journeyRooms[36].storyRead)return (chapterShortcuts&(1<<4))==0?29:36;return 31;
   }
  }
  private string ThunderRouteObjective(){
   int stage=journeyRooms[currentRoom].stage;
   if(stage==0)return !journeyRooms[8].cleared?"先清理"+chapterNames[8]+"的精英守卫":chapterWaymarks!=7?"前往三块旧路标 · 扶正全部路标，再挑战守路铜卫":!journeyRooms[6].cleared?"路标已扶正 · 前往守路铜卫":!chapterMarkKnown?"研读铜纹台 · 学习雷标":"沿铜卫身后的石阶前往引雷遗址";
   if(stage==1)return !chapterWest?"先复位西引雷节点":!chapterEast?"继续探索，复位东引雷节点":!chapterArcKnown?"研读东节点铜纹台 · 学习跃电":!chapterRanger||!journeyRooms[17].storyRead?"找到巡路员，听完她的巡路记录":!journeyRooms[20].cleared?"清理"+chapterNames[20]+"的精英守卫":!journeyRooms[21].claimed?"前往避雨棚 · 扶正受损棚梁":!chapterLampCarried&&!chapterLampGiven?"取回送往山口的灯罩，再挑战守关者":"归路已经接通 · 前往"+chapterNames[18];
   return !journeyRooms[35].cleared?"先清理"+chapterNames[35]+"的精英守卫":!journeyRooms[36].storyRead?(chapterShortcuts&(1<<4))==0?"打通检修门 · 前往巡路记录":"翻阅巡路记录，确认守护兽受困的真相":"调查已完成 · 前往圣所解除守护兽的束缚";
  }
  private void FinishThunderStoryRecord(){
   if(thunderPendingStoryRoom<0||StoryActive)return;int id=thunderPendingStoryRoom;thunderPendingStoryRoom=-1;
   if(!ThunderStoryRoute||id!=currentRoom)return;journeyRooms[id].storyRead=journeyRooms[id].claimed=true;BuildChapterActions();UpdateExplorationDoors();UpdateFlowGuide();RequestCheckpoint();SaveChapterCheckpoint(false);
  }
 }
}
