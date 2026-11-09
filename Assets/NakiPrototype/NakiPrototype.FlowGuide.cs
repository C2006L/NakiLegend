using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private GameObject flowGuide;
  private int flowGuideStamp;
  private int FlowGoal
  {
   get{
    if(journeyRooms.Count==0||bossPracticeMode||gatehouseSampleMode||lightningTrialMode||thunderPrototypeMode||thunderChapterMode&&thunderChapterPracticeBoss>=0)return -1;
    RoomNode n=journeyRooms[currentRoom];
    if(capitalMode)return chapterEnding?-1:CapitalGoal;if(manorCourtyardMode)return ManorFlowGoal;
    if(fireChapterMode){if(fireHeartRecovered&&fireEvacuation==3)return currentRoom==0||currentRoom==30||currentRoom==44?currentRoom:n.stage==1?28:n.stage==0?0:30;return chapterEnding?0:WorkshopGoal;}
    if(woodlandMode)return chapterEnding?-1:WoodlandGoal;
    if(iceSampleMode){if(!iceFullChapter)return -1;if(chapterEnding)return 0;if(chapterStone){if(!chapterRanger)return 18;return chapterLampCarried?0:41;}if(n.stage==1&&journeyRooms[26].cleared&&!chapterRanger)return 18;if(journeyRooms[2].cleared&&!iceSpikeKnown)return 2;if(n.stage==1&&!chapterRanger)return 18;return stageEnds[n.stage];}
    if(thunderChapterMode)return chapterStone?(currentRoom==0||n.stage<2?0:31):ThunderStoryGoal;
    if(!desertStoneRecovered||canalEpilogueStart<0)return -1;
    if(!GatehouseActive)return stageEnds[2];
    return canalEpilogueStart+(canalEpilogueComplete?4:!gateRepairCommitted?1:!gateTotalUnlocked?2:!GatehouseRoom(3).cleared?3:4);
   }
  }
  private int StoryGuideGoal {get{return FilterStoryGuideGoal(FlowGoal);}}
  private int FilterStoryGuideGoal(int goal){
    if(goal<0||goal>=journeyRooms.Count)return -1;RoomNode target=journeyRooms[goal];
    // Completed arenas can contain post-battle teaching or a story interaction.
    if(!IsCombatRoom(target)||target.cleared)return goal;
    if(target.kind==RoomKind.Boss||target.kind==RoomKind.Guardian)return -1;
    // Keep story objectives that require clearing a rescue/archive/mechanism room.
    bool story=capitalMode&&(goal==6||goal==12||goal==15||goal==21||goal==25)
     ||manorCourtyardMode&&(goal==18||goal==20||goal==24||goal==32||goal==34||goal==41||goal==43)
     ||woodlandMode&&goal==17||fireChapterMode&&(goal==4||goal==20||goal==23)
     ||iceSampleMode&&goal==18||ThunderStoryRoute&&(goal==14||goal==16||goal==17||goal==29)
     ||GatehouseActive;
    return story?goal:-1;
  }
  private int FlowNextRoom(int goal)
  {
   if(goal<0||goal>=journeyRooms.Count||goal==currentRoom)return -1;
   var queue=new Queue<int>();var first=new Dictionary<int,int>();queue.Enqueue(currentRoom);first[currentRoom]=-1;
   while(queue.Count>0){int id=queue.Dequeue();RoomNode n=journeyRooms[id];var options=new List<int>(n.links);if(n.stairsTo>=0)options.Add(n.stairsTo);
    foreach(int next in options){if(first.ContainsKey(next))continue;if(n.links.Contains(next)&&ExplorationDoorLocked(id,NeighborDirection(n,journeyRooms[next])))continue;first[next]=id==currentRoom?next:first[id];if(next==goal)return first[next];queue.Enqueue(next);}
   }
   return -1;
  }
  private ChapterAction FlowAction(params int[] kinds)
  {foreach(int kind in kinds)foreach(ChapterAction a in chapterActions)if(a.kind==kind)return a;return null;}
  private bool FlowInteractionPoint(out Vector2 at)
  {
   at=Vector2.zero;ChapterAction a=null;
   if(capitalMode)a=CapitalFlowAction();else if(manorCourtyardMode)a=ManorFlowAction();
   else if(fireChapterMode){if(currentRoom==30&&fireHeartRecovered&&fireEvacuation==3)a=FlowAction(510);else if(currentRoom==0)a=FlowAction(chapterEnding?507:506);else if(currentRoom==43)a=FlowAction(504);else if(currentRoom==44)a=FlowAction(fireEvacuation==3?508:505);else if(currentRoom==28)a=FlowAction(413);else if(currentRoom==20)a=FlowAction(410);else if(currentRoom==23)a=FlowAction(411);}
   else if(woodlandMode){if(currentRoom==31&&chapterStone)a=FlowAction(206);else if(currentRoom==4&&chapterStone)a=FlowAction(207);else if(currentRoom==0&&chapterStone)a=FlowAction(201);else if(currentRoom==17)a=FlowAction(203);else if(currentRoom==14||currentRoom==16||currentRoom==21)a=FlowAction(204);else if(currentRoom==6)a=FlowAction(202);else if(currentRoom==18)a=FlowAction(205);}
   else if(iceSampleMode){if(currentRoom==0)a=FlowAction(chapterEnding?340:320);else if(currentRoom==18)a=FlowAction(310);else if(currentRoom==2)a=FlowAction(302);else if(currentRoom==41)a=FlowAction(!chapterWest?312:!chapterEast?313:314);}
   else if(thunderChapterMode){if(currentRoom==0)a=FlowAction(chapterEnding?305:1);else if(currentRoom==31)a=FlowAction(7);else if(currentRoom==9)a=FlowAction((chapterWaymarks&1)==0?10:(chapterWaymarks&2)==0?11:12);else if(currentRoom==6)a=FlowAction(5);else if(currentRoom==14||currentRoom==16)a=FlowAction(currentRoom==14?3:4);else if(currentRoom==21)a=FlowAction(13);else if(currentRoom==22)a=FlowAction(14);else if(currentRoom==36)a=FlowAction(15);else if(currentRoom==17)a=FlowAction(2);else if(currentRoom==29)a=FlowAction(104);}
   else if(GatehouseActive){int k=canalEpilogueComplete?5:CurrentGatehouseIndex==1?(!gateSandCleared[0]?0:!gateSandCleared[1]?1:2):CurrentGatehouseIndex==2?4:5;at=canalEpilogueComplete?ThunderDockAt:GateActionPoints[k];return true;}
   if(a==null)return false;at=a.at;return true;
  }
  private void UpdateFlowGuide()
  {
   bool valid=world!=null&&journeyRooms.Count>currentRoom&&currentRoom>=0&&journeyRooms[currentRoom].cleared;
   int goal=valid?StoryGuideGoal:-1;
   int flags=(chapterEnding?1:0)|(chapterLampCarried?2:0)|(chapterLampGiven?4:0)|(chapterWest?8:0)|(chapterEast?16:0)|(gateSandCleared[0]?32:0)|(gateSandCleared[1]?64:0)|(gateRepairCommitted?128:0)|(gateTotalUnlocked?256:0)|(fireEvacuation<<10);
   int stamp=(currentRoom*397+goal)*397+flags;if(capitalMode)stamp=(stamp*397+capitalFlags)*397+capitalLetters;else if(manorCourtyardMode)stamp=stamp*397+manorProgress;
   if(valid&&goal>=0&&flowGuide!=null&&flowGuide.transform.parent==world&&flowGuideStamp==stamp)return;
   if(flowGuide!=null){flowGuide.SetActive(false);Destroy(flowGuide);}flowGuide=null;
   if(!valid||goal<0)return;flowGuideStamp=stamp;journeyRooms[goal].discovered=true;RoomNode n=journeyRooms[currentRoom];Vector2 at,normal;
   // The existing final-room trail transports directly to the old seedbed.
   bool localWoodRoute=woodlandMode&&chapterStone&&(currentRoom==31||currentRoom==4);
   if(goal==currentRoom||localWoodRoute){
    if(!FlowInteractionPoint(out at)){if(stairs==null)return;at=stairPosition;}
    flowGuide=Draw("Conclusion interaction marker",windRingSprite??disc,at,Vector2.one*.75f,new Color(1,.83f,.44f,.8f),-690);return;
   }
   int next=FlowNextRoom(goal);if(next<0)return;journeyRooms[next].discovered=true;
   if(next==n.stairsTo){Vector2 transitAt=stairPosition;if(capitalMode){var action=FlowAction(1030);if(action==null)return;transitAt=action.at;}else if(manorCourtyardMode){var action=ManorFlowAction();if(action==null)return;transitAt=action.at;}else if(stairs==null)return;flowGuide=Draw("Conclusion stair marker",windRingSprite??disc,transitAt,Vector2.one*.85f,C("f4d68a"),-690);return;}
   int direction=NeighborDirection(n,journeyRooms[next]);normal=DoorNormal(direction);at=DoorPoint(direction,-.95f);
   flowGuide=new GameObject("Conclusion route exit arrow");flowGuide.transform.SetParent(world,false);flowGuide.transform.position=at;
   foreach(float sign in new[]{-1f,1f}){Vector2 tip=at+normal*.2f,tail=at-normal*.2f+new Vector2(-normal.y,normal.x)*sign*.24f,delta=tip-tail;var line=Stretch("Conclusion exit chevron",square,(tip+tail)*.5f,new Vector2(delta.magnitude,.045f),C("f4d68a"),590);line.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg);line.transform.SetParent(flowGuide.transform,true);}
  }
 }
}
