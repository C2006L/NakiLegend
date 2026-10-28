using UnityEngine;
namespace NakiLegend.Prototype
{
    public sealed partial class NakiPrototype
    {
        private void TellDesertLanding()
        {
            TellStory("渡船穿过浅滩，缓缓靠上沙漠岛的木栈桥。船夫收起缆绳：沙城就在前面，沿石路走，别追着沙里的风声。",
                "老人：海风从前会带来雨，如今连井底都只剩沙了。山口那座遗迹，已经很久没人回来。",
                "少年把航图收进怀里，踏上通往沙城的旧路。");
        }
        private void VisitDesertRest(RoomNode room)
        {
            room.claimed=true;health=Mathf.Min(6,health+2);elementPower=Mathf.Min(100,elementPower+40);
            if(origin==Origin.Light)energy=100;NormalizeOriginPower();
            TellStory(origin==Origin.Dark
                ?"你在背风的棚下喝了些水，重新裹好伤口。右眼的石头依旧沉默，休息无法填补其中空下来的灵魂。"
                :"你在背风的棚下稍歇。海上带来的那点暖意还在，掌心的光也渐渐安定下来。");
            AnnounceDiscovery(origin==Origin.Dark?"休憩 · 生命 +2，元素 +40；贤者之石未充能":"休憩 · 生命 +2，元素 +40，光之力恢复");
        }
        private void VisitDesertTrace(RoomNode room)
        {
            room.claimed=true;health=Mathf.Min(6,health+1);elementPower=Mathf.Min(100,elementPower+20);NormalizeOriginPower();
            switch(room.layout%3)
            {
                case 0:TellStory("半埋的路碑背面刻着一枚风纹，尖端朝向山口。沙粒在刻痕里一圈圈打转。","少年：这阵风一直往遗迹深处去。跟着它，也许就能找到源头。");break;
                case 1:TellStory("废弃的驿棚里留着一只封好的水囊和几枚元素结晶。旧航图上，通往遗迹的路被人反复描过。","你喝了一小口水，把航图上的路记在心里。");break;
                default:TellStory("枯井旁的风铃没有声音，铜片却不断朝同一个方向倾斜。","少年：风还在。只是回村的那条路，被什么挡住了。");break;
            }
            AnnounceDiscovery("寻路所得 · 生命 +1，元素之力 +20");
        }
        private void AnnounceDiscovery(string message){hintReadyAt=0;JourneyHint(message);}
        private bool DesertEndingReady
        {
            get{return !ThunderActive&&!bossPracticeMode&&!gatehouseSampleMode&&desertStoneRecovered&&canalEpilogueComplete
                &&gateRepairCommitted&&gateTotalUnlocked&&canalEpilogueStart>=0&&journeyRooms.Count>=canalEpilogueStart+5
                &&GatehouseRoom(1).cleared&&GatehouseRoom(3).cleared;}
        }
    }
}
