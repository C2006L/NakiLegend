using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private string FireHarborLabel {get{return fireHeartRecovered?(fireEvacuation==3?"与守岸人核对归港名单":"询问未归居民的去向"):forgeBridgeOpen?"询问地心撤离路线":"询问黑砂海岸的消息";}}
  private void FireHarborConversation()
  {
   if(chapterEnding){TellStory("守岸人：昨夜终于又闻见面包香了。孩子们说，屋里的灯比山上的火亮。","守岸人把一封旧回信放在面包旁：北方府邸催过药和粮，最近却只肯让运淬炼器具的船靠岸。","少年：先把这里的人照顾好。这封信我收着，下一趟船，也该去问问那些没能回家的人。");return;}
   if(fireHeartRecovered&&fireEvacuation==3){TellStory("守岸人合上名单：一人不少，最后的物资也进仓了。炉匠把面团留在公共炉灶旁，就等你掌心的火种。","少年：那就先烤第一炉。让大家知道，今晚可以安心留在家里。");return;}
   if(fireHeartRecovered){TellStory("守岸人：山腹里的火已经退了，可归岸名单还缺两笔。阿铎在熔龟庭院东侧的石廊接应他们。","少年：火石已经寻回，人还没有到齐。我这就沿炉道回去。");return;}
   if(forgeBridgeOpen){TellStory("守岸人：工坊的人已经过桥，地心避难站却还没有回信。石阶下去，阿铎会带你找到最后的撤离路。","少年：桥能接回人，也该有一条路让山里的热流回去。");return;}
   if(chapterStone){TellStory("守岸人：熔蜥已经退回岩洞，工坊那边还传来断断续续的锻链声。阿铎的信里提过一座撤离桥。","少年：我先去找他，别让大家在桥边等太久。");return;}
   if(chapterRanger){TellStory("守岸人：避难所的人都接到了，多谢。灰砂路尽头那头熔蜥还没有安静下来，去工坊的人过不去。","少年：我会给它降下温，也替大家留出路。");return;}
   TellStory("守岸人：火山以前也会发红，夜里却总能闻见面包香。这几天，只剩下灰了。","少年：先把困在避难所的人接出来，再去看看那头熔蜥。");
  }
  private void PrepareCoreReturn()
  {
   if(currentRoom!=44||!journeyRooms[44].cleared||!fireHeartRecovered||(fireEvacuation&1)==0)return;
   if((fireEvacuation&2)!=0){JourneyHint("物资已经撤出 · 在归岸路口按 F 返回黑砂码头");return;}
   fireEvacuation|=2;
   TellStory("最后一辆物资车驶过石廊。阿铎把车上的绳结重新系紧，让居民先沿旧路往岸边走。","阿铎：名单齐了。等你准备好，就从旁边的归岸路口回港，镇上的炉子还等着我们。");
  }
  private void TravelCoreToHarbor()
  {
   if((currentRoom!=44&&currentRoom!=30)||!journeyRooms[currentRoom].cleared||!fireHeartRecovered||fireEvacuation!=3)return;
   EnterExplorationRoom(0,-1);position=NearestSafeFooting(new Vector2(-1.8f,2.5f));BlockWorldInteraction();
   TellStory("你随最后一辆车走过冷却的炉道。天色渐暗，黑砂码头上的人一个接一个迎了上来。","守岸人：名单已经点齐。到左边的公共炉灶来吧，今晚该有第一炉面包了。");
  }
 }
}
