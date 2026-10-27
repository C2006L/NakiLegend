using UnityEngine;

namespace NakiLegend.Prototype
{
    public sealed partial class NakiPrototype
    {
        private int canalEpilogueStart=-1;
        private bool desertStoneRecovered,canalEpilogueComplete;
        private bool IsGatehouseRoom(RoomNode room){return room.gatehouseIndex>=0;}
        private bool GatehouseActive{get{return currentRoom>=0&&currentRoom<journeyRooms.Count&&IsGatehouseRoom(journeyRooms[currentRoom]);}}
        private int CurrentGatehouseIndex{get{return GatehouseActive?journeyRooms[currentRoom].gatehouseIndex:-1;}}
        private RoomNode GatehouseRoom(int local){return journeyRooms[(gatehouseSampleMode?0:canalEpilogueStart)+local];}

        private void ResetGatehouseProgress()
        {
            for(int i=0;i<gateSandCleared.Length;i++)gateSandCleared[i]=false;
            gateRepairCommitted=gateRewardCreated=gateTotalUnlocked=gateRainReturned=false;
            gateWorkRemaining=gateWorkDuration=0;gateWorkKind=-1;
            desertStoneRecovered=canalEpilogueComplete=false;canalEpilogueStart=-1;
        }

        private void AddGatehouseRooms(int start,int stage)
        {
            Vector2Int[] cells={new Vector2Int(2,1),new Vector2Int(2,2),new Vector2Int(2,3),new Vector2Int(3,2),new Vector2Int(3,3)};
            for(int local=0;local<cells.Length;local++)journeyRooms.Add(new RoomNode {
                id=start+local,gatehouseIndex=local,stage=stage,depth=3,cell=cells[local],layout=local,
                halfWidth=8,halfHeight=4.5f,kind=local==1||local==3?RoomKind.Combat:RoomKind.Entrance,
                waves=local==1?2:1
            });
            ConnectRooms(start,start+1);ConnectRooms(start+1,start+2);ConnectRooms(start+2,start+4);
            ConnectRooms(start+4,start+3);ConnectRooms(start+1,start+3);
        }

        private void UnlockCanalEpilogue(RoomNode bossRoom)
        {
            if(desertStoneRecovered)return;
            desertStoneRecovered=true;windEmpowered=true;elementPower=Mathf.Min(100,elementPower+30);
            canalEpilogueStart=journeyRooms.Count;AddGatehouseRooms(canalEpilogueStart,3);
            bossRoom.stairsTo=canalEpilogueStart;GatehouseRoom(0).stairsTo=bossRoom.id;
            pendingRainCG=false;UpdateJourneyPortal();
            TellStory("紫色裂纹熄灭，风元素碎片从守关者体内脱离。风刃与风波得到强化。",
                "少年：风回来了。可旧渠还堵着，我们得让水走完回家的路。\n前往亮起的石阶，靠近按 F 进入归渠水道。");
        }

        private bool TryCompleteCanalEpilogue()
        {
            if(canalEpilogueComplete){JourneyHint("甘泽归沙 · 可以继续回访修好的水渠");return true;}
            if(!desertStoneRecovered){JourneyHint("先取回风元素碎片，再迎接雨季");return true;}
            if(!gateRepairCommitted){JourneyHint("水渠仍被堵住 · 返回闸室清开两处沙堆，再转动手轮");return true;}
            if(!gateTotalUnlocked){JourneyHint("总闸还未打开 · 前往分水总闸按 F 操作");return true;}
            if(!GatehouseRoom(1).cleared||!GatehouseRoom(3).cleared){JourneyHint("检修庭院尚不安全 · 清理后再来迎接初雨");return true;}
            canalEpilogueComplete=journeyComplete=gateRainReturned=true;restoredCanals++;
            RefreshGatehouseWater();
            TellStory("少年：闸开了。现在，风和水都有回家的路。",
                "山口的云缓缓越过山脊。孩子伸出手：这是……雨吗？",
                "你和村民修好的旧渠重新响起水声。第一瓢水，留给树根。",
                "甘泽归沙 · 愿长风所到之处，皆有甘泽。");
            pendingRainCG=true;RequestCheckpoint();return true;
        }

        private string CanalCompletionHint()
        {
            if(canalEpilogueComplete)return "甘泽归沙 · 水渠已恢复，可以继续回访";
            if(!gateRepairCommitted)return "水渠尚未修好 · 返回闸室清沙转轮";
            if(!gateTotalUnlocked)return "总闸未开启 · 返回分水总闸操作阀门";
            if(!GatehouseRoom(3).cleared)return "沿南门清理检修庭院，再来迎接初雨";
            return "旧渠已畅通 · 靠近风铃按 F 迎接初雨";
        }
        private string CanalCompletionPrompt()
        {
            if(canalEpilogueComplete)return "水渠已恢复 · 本章已完成";
            return gateRepairCommitted&&gateTotalUnlocked&&GatehouseRoom(3).cleared
                ?"F 迎接初雨 · 完成沙漠岛":"F 查看旧渠恢复进度";
        }
        private string GatehouseRoomName(RoomNode node)
        {return !gatehouseSampleMode&&node.gatehouseIndex==0?"旧渠入口":GateRoomNames[node.gatehouseIndex];}
        private string GatehouseHeading(RoomNode node)
        {return (gatehouseSampleMode?"样板 · ":"归渠水道 · ")+GatehouseRoomName(node);}
    }
}
