using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private bool voyagerJournal;
  private int voyagerSelection;
  private static readonly string[] VoyagerTitles={"鸣沙洲 · 前一位修渠者","雷雨高原 · 紫石旅人","霜潮群岛 · 没有归票的船单","赤炉群岛 · 容器与命令","青栖林海 · 北方旧信"};
  private int AvailableVoyagerNotes
  {
   get{
    int bits=originLoreBits&31;if(journeyRooms.Count==0)return bits;
    if(CurrentIsland.ordinal==1&&(orbLearned||desertStoneRecovered))bits|=1;
    if(CurrentIsland.ordinal==2&&thunderChapterMode&&journeyRooms.Count>6&&journeyRooms[6].cleared)bits|=2;
    if(iceSampleMode&&journeyRooms.Count>2&&journeyRooms[2].cleared)bits|=4;
    if(fireChapterMode&&(fireBoltKnown||fireHearthLit))bits|=8;
    if(woodlandMode&&chapterLampGiven)bits|=16;
    return bits;
   }
  }
  private string[] VoyagerLines(int island)
  {
   bool dark=origin==Origin.Dark;
   switch(island){
    case 0:return new[]{
     "井边旧碑留着一份修渠拓印，落款也是‘奉王命出海的勇敢少年’。原来，在这趟旅程之前，已经有人走过同一条路。",
     "碑背没有庆功诗，只刻着一句：‘闸开以后，先看看水有没有到村口。’少年把这句话抄进航海手记。",
     dark?"右眼掠过那位旅人留下的归途残响。石头能留住力量与片段记忆，却不能把一个完整的人带回来。":"少年把微光留在井沿。国王要的是风石，村庄等的却是水。他决定取回碎片后，还要把旧渠接到人们门前。"};
    case 1:return new[]{
     "旧巡路簿记着一场护送：一位右眼藏着紫石的旅人，曾替村民挡住山口落雷。使用贤者之石能量的人，也曾为别人留在雨中。",
     "那人的航线与少年手中的王室委任书一致。页角写着：‘不要只记取走了什么，也记谁还没有回家。’",
     dark?"另一页解释了淬炼：残响可以驱动术式，完整的意志却不能由许多能量点数拼成。少年依照铜纹边注，学着让残响在目标身上留下一枚印记。":"少年想起山口等待归人的灯，把光展开在身前。力量不必总向外倾泻，有时也该替别人挡住一次落下的碎石。"};
    case 2:return new[]{
     "破冰队记录夹着一页旧船单：另一位奉王命收集碎片的少年，最后一次靠岸后，被授予北方的府邸。单上没有他的返乡船票。",
     "船单背面是一份未送达的药单，收件地是暮钟岛。少年记下这个名字：收齐碎片以后，前一位勇者为什么没有回家？",
     dark?"冰晶映出披斗篷旅人的残影。他也曾站在归航灯旁，右眼里的紫光却让倒影慢慢变得陌生。":"守塔人的字条写着：灯不用照亮整片海，只要让归来的船认出岸。少年把药单与这张字条一起收好。"};
    case 3:return new[]{
     "炉匠在旧淬炼图上指出一圈细纹：容器能盛放力量，也能被刻入命令。王室封蜡上，留着同样的接驳纹。",
     "送往北方府邸的货单，将药物、粮食和额外的淬炼器具列在同一批。尚不能证明是谁下的命令，但足以让少年决定亲自查清。",
     dark?"炉匠：‘贤者之石没有要你服从谁。要分清石头自己的脉动，和后来刻上去的命令。’少年第一次认真检查右眼中的紫纹。":"炉匠：‘把火收回炉膛，是为了让人过冬，不是为了把它永远关住。’少年松开掌心，让光一轮轮向四周散开。光在炉壁上相遇，又分成细小的光流，照向被遮住的角落。"};
    default:return new[]{
     "新芽顶开旧叶，林册夹页滑出一封北方旧信：‘这里的种子也不再发芽。我原想守住他们，如今却越来越听不见他们说话。’",
     "信封写着‘暮钟岛·受封者府邸’，淬炼纹旁还画着来时的航线。它与古树身上的寄生纹相似，却不能只凭这一点认定写信人的身份。",
     dark?"右眼传来相似的灼痛。少年合上林册：‘能留下多少力量，不该决定我还能不能自己作主。我要去找写信的人。’":"少年没有催新芽长大，只移开压住它的枯枝。他收好旧信：‘真正的守护，该给别人留下生长和离开的路。我要去找写信的人。’"};
   }
  }
  private void OpenVoyagerJournal()
  {
   if(origin==Origin.Unchosen||health<=0||pendingWeaponPickup!=null||StoryActive||chapterCg||showRainCG)return;
   int available=AvailableVoyagerNotes;originLoreBits|=available;originKnowledge|=available;
   voyagerSelection=Mathf.Clamp(CurrentIsland.ordinal-1,0,4);if((available&(1<<voyagerSelection))==0)for(int i=0;i<5;i++)if((available&(1<<i))!=0){voyagerSelection=i;break;}
   inventoryMenu=false;voyagerJournal=true;CancelBowCharge();BlockWorldInteraction();RequestCheckpoint();
   if(SafeCheckpointRoom){if(thunderChapterMode)SaveChapterCheckpoint(false);else SaveCheckpoint(false);}
  }
  private bool HandleVoyagerJournal()
  {
   if(!voyagerJournal)return false;
   if(Pressed('e')||Pressed('b')||Pressed('v')){voyagerJournal=false;BlockWorldInteraction();return true;}
   if(Pressed('a'))voyagerSelection=(voyagerSelection+4)%5;if(Pressed('d'))voyagerSelection=(voyagerSelection+1)%5;
   return true;
  }
  private void DrawVoyagerJournal(float w,float h)
  {
   HudRect(new Rect(0,0,w,h),C("10212a"));Rect panel=new Rect(w/2-500,h/2-280,1000,560);HudRect(panel,C("172d33"));
   FittedLabel(new Rect(panel.x+25,panel.y+20,750,40),"航海手记 · 前一位勇者的归途",journeyTitleStyle);
   if(FlatButton(new Rect(panel.x+800,panel.y+20,170,38),"B / ESC 返回")){voyagerJournal=false;BlockWorldInteraction();return;}
   int available=AvailableVoyagerNotes;
   for(int i=0;i<5;i++)if(FlatButton(new Rect(panel.x+25,panel.y+90+i*76,275,62),(available&(1<<i))!=0?VoyagerTitles[i]:"尚未找到 · "+new[]{"井边旧碑","巡路簿","破冰队船单","炉匠淬炼图","林册夹页"}[i],voyagerSelection==i))voyagerSelection=i;
   float x=panel.x+330;FittedLabel(new Rect(x,panel.y+90,635,40),VoyagerTitles[voyagerSelection],journeyTitleStyle);
   if((available&(1<<voyagerSelection))!=0){string[] lines=VoyagerLines(voyagerSelection);for(int i=0;i<lines.Length;i++)FittedLabel(new Rect(x,panel.y+152+i*97,630,84),lines[i],journeyTextStyle,true);}
   else FittedLabel(new Rect(x,panel.y+155,630,180),"这页仍是空白。沿岛屿探索、救援和调查时，亲眼见到的物证才会写进手记。\n\n航图上的推荐顺序不会替你取得线索；缺失的记录可在回访时补读。",journeyTextStyle,true);
   FittedLabel(new Rect(panel.x+25,panel.y+500,950,43),"A / D 或点击选页 · 只回顾已经取得的线索，不消耗资源，也不重复领取奖励",journeySmallStyle,true);
  }
 }
}
