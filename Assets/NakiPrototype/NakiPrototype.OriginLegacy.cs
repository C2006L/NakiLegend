using System;
using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private int originLoreBits,originKnowledge,primarySkill,queuedOriginChapter;
  private bool originAbilityMenu;
  private float originGuardUntil,originGuardReturnAt;
  private Vector2 originBeamDirection;
  private GameObject originBeam;
  private static bool ValidDarkPower(float value){return !float.IsNaN(value)&&!float.IsInfinity(value)&&value>=0;}
  private float DarkStrength {get{return 1+Mathf.Min(.15f,.07f*(float)(Math.Log(1+(double)darkPower/25)/Math.Log(2)));}}
  private string PrimarySkillName {get{return OriginSkillName(0);}}
  private float PrimarySkillCost {get{return OriginSkillCost(0);}}
  private float PrimarySkillCooldown {get{return OriginSkillCooldown(0);}}
  private void RestoreOriginLegacy(int lore,int knowledge,int skill,int queued)
  {
   originLoreBits=lore&63;originKnowledge=(knowledge|(lore&31))&31;queuedOriginChapter=Mathf.Clamp(queued,0,5);primarySkill=Mathf.Clamp(skill,0,2);
   originAbilityMenu=false;originGuardUntil=originGuardReturnAt=0;ResetOriginCombat();
  }
  private bool OriginAbilityKnown(int id)
  {
   if(id<0||id>2||!orbLearned)return false;
   int known=originKnowledge|(originLoreBits&31);if(bossPracticeMode||lightningTrialMode||thunderChapterPracticeBoss>=0)known|=10;
   return id==0||id==1&&(known&2)!=0||id==2&&(known&10)==10;
  }
  private bool ValidOriginLegacy(int lore,int knowledge,int skill,int queued){return lore>=0&&lore<=63&&knowledge>=0&&knowledge<=31&&skill>=0&&skill<=2&&queued>=0&&queued<=5;}
  private void MergeOriginJourney(ThunderCheckpoint source)
  {
   originLoreBits|=source.lore;originKnowledge|=source.knowledge;if(source.routeOrigin>0){origin=(Origin)source.routeOrigin;darkPower=origin==Origin.Dark?source.dark:0;energy=origin==Origin.Light?source.light:0;}
   primarySkill=Mathf.Clamp(source.primary,0,2);RestoreSkillLoadout(source.loadout,true);RequestCheckpoint();
  }
  private void QueueOriginMoment(int island)
  {if(origin==Origin.Unchosen||island<0||island>4||(originLoreBits&(1<<island))!=0)return;queuedOriginChapter=island+1;originKnowledge|=1<<island;}
  private void TickOriginLegacy()
  {
   if(queuedOriginChapter>0&&journeyRooms.Count>0&&journeyRooms[currentRoom].cleared&&!StoryActive&&!chapterCg&&!chapterSetup&&!rewardMenu&&!chapterRewardMenu&&!inventoryMenu&&!mapMenu&&!talentMenu&&!voyagerJournal&&!worldAtlasMenu){
    int i=queuedOriginChapter-1;queuedOriginChapter=0;
    if((originLoreBits&(1<<i))==0){originLoreBits|=1<<i;originKnowledge|=1<<i;
     TellStory(VoyagerLines(i));if(i==1||i==3)AnnounceDiscovery(OriginAbilityKnown(2)?"已领悟 O "+OriginSkillName(1)+" · P "+OriginSkillName(2):OriginAbilityKnown(1)?"已领悟 O "+OriginSkillName(1):"主属性记录已收好 · 先在鸣沙洲古井碑领悟基础战技");RequestCheckpoint();if(thunderChapterMode)SaveChapterCheckpoint(false);
    }
   }
   TickOriginVisuals();
  }
  private void CastEmpoweredMeteor()
  {
   CastStarfall();
  }
  private bool HandleOriginAbilityMenu()
  {if(!originAbilityMenu)return false;if(Pressed('e')||Pressed('b'))originAbilityMenu=false;return true;}
  private void DrawOriginAbilityMenu(float w,float h)
  {
   HudRect(new Rect(0,0,w,h),C("10212a"));CenterLabel(new Rect(w/2-430,h/2-265,860,45),origin==Origin.Dark?"贤者之石 · 残响与意志":"掌心之光 · 凝聚、守护、流转",journeyTitleStyle);
   for(int i=0;i<3;i++){
    Rect card=new Rect(w/2-400,h/2-195+i*112,800,96);bool known=OriginAbilityKnown(i);HudRect(card,known?C("1c3b40"):C("202e38"));
    FittedLabel(new Rect(card.x+18,card.y+12,760,30),char.ToUpperInvariant(OriginKeys[i])+" · "+OriginSkillName(i)+(known?"":" · 尚未领悟"),journeyTextStyle);
    string description=origin==Origin.Dark?new[]{"按住释放五颗追踪流星 · 每组 1 贤者之石能量","挂印8秒，自发范围脉冲；命中可提前引爆，最多6次，击杀后转移邻敌 · 6 贤者之石能量，冷却5秒","五处追踪圈在坠落前锁定；大范围坠星，重叠命中100%/50%/25%，引爆蚀魂印 · 16 贤者之石能量，冷却5秒"}[i]:new[]{"按住 L 持续照射，松开或 K 闪避停止 · 启动 4，每秒 16 光能",LightGuardDuration.ToString("0.#")+" 秒内受到的伤害降低70%，受击返还光能，上限8 · 消耗 20","三轮环身光弹，碰敌或障碍后分裂；每颗初始光弹最多分裂3次 · 消耗35"}[i];
    FittedLabel(new Rect(card.x+18,card.y+49,760,37),known?description:OriginLearningPlace(i),journeySmallStyle,true);
   }
   CenterLabel(new Rect(w/2-420,h/2+145,840,62),MasteryCaption+" · 旅途推进提升术式，返航保留修习进度。\n"+(origin==Origin.Dark?"储能无上限，蓄能增幅递减 · L / O / P 主术式，U / I 元素技能":"停手 1.5 秒后每秒回光 4 · L / O / P 主术式，U / I 元素技能"),journeySmallStyle);
   if(FlatButton(new Rect(w/2-110,h/2+225,220,42),"B / ESC 返回"))originAbilityMenu=false;
  }
 }
}
