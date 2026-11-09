using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private float returnBladeReady,guardPulseReady;
  private float SkillElementBase(float cost){return cost>=10?6:cost>=8?5:cost>=6?4:cost;}
  private float SkillElementCost(float cost){return ElementCost(SkillElementBase(cost)*(HasTalent(5)?.8f:1));}
  private bool SpendSkillPower(float cost){bool spent=SpendPower(PowerPool.Elemental,SkillElementBase(cost)*(HasTalent(5)?.8f:1));if(spent)RecordEncounterCast();return spent;}
  private bool ValidSavedSkill(int id,int[] talents){return id>=0&&id<12&&(id!=8&&id!=9||talents!=null&&talents.Length==6&&talents[id==8?0:1]>=0);}
  private bool ExtraSkillKnown(int id){return learnedTalents[id==8?0:1]>=0;}
  private string[] ExtendedSkillNames(string[] original){var names=new string[10];for(int i=0;i<8;i++)names[i]=i<original.Length?original[i]:"未抵达的元素";names[8]="回旋风刃";names[9]="护甲脉冲";return names;}
  private void CastJourneySkill(int id)
  {
   if(!ExtraSkillKnown(id)){JourneyHint(id==8?"完成沙漠旅程并领悟天赋后学会回旋风刃":"完成雷雨高原旅程并领悟天赋后学会护甲脉冲");return;}
   if(clock<dashUntil)return;
   if(id==9)
   {
    if(clock<guardPulseReady||shield>=EffectiveMaximumShield||!SpendSkillPower(10))return;
    guardPulseReady=clock+8;shield=Mathf.Min(EffectiveMaximumShield,shield+2);SkillSound("wave","cast");SpellSpark(position,C("8fe8ee"),1.1f);return;
   }
   if(clock<returnBladeReady||elementPower<SkillElementCost(6))return;
   int start=shots.Count;if(SpawnSpellBurst(Element.Wind,"blade",12,7,1.8f,false,true)==0)return;
   for(int i=start;i<shots.Count;i++)shots[i].returnBlade=true;
   SpendSkillPower(6);returnBladeReady=clock+2.5f;
  }
  private void DrawUnifiedSkills(float w,float h)
  {
   string[] original=GlobalSkillNames;
   var ids=new List<int>();for(int i=0;i<original.Length;i++)if(i<2||ChapterSkillKnown(i)||chapterSkillSlots[0]==i||chapterSkillSlots[1]==i)ids.Add(i);
   Rect panel=new Rect(w/2-470,h/2-310,940,620);HudRect(panel,C("132630"));
   CenterLabel(new Rect(panel.x+20,panel.y+15,900,38),"元素与旅途战技 · 配置 U / I",journeyTitleStyle);
   CenterLabel(new Rect(panel.x+20,panel.y+57,900,28),"L 保留光之力／贤者之石主技能 · 清房后可更换",journeySmallStyle);
   for(int n=0;n<ids.Count;n++)
   {
    int id=ids[n];float x=panel.x+20+n%2*455,y=panel.y+105+n/2*75;bool known=ChapterSkillKnown(id);
    FittedLabel(new Rect(x,y,285,27),ActiveSkillNames[id],journeyTextStyle);
    string detail=known?SkillElementCost(ChapterSkillCost(id)).ToString("0.##")+" 元素":id==8?"沙漠收尾后领悟":id==9?"高原收尾后领悟":"继续探索学习";
    FittedLabel(new Rect(x,y+30,285,25),detail,journeySmallStyle);
    if(FlatButton(new Rect(x+285,y,68,52),"U",chapterSkillSlots[0]==id))EquipChapterSkill(id,0);
    if(FlatButton(new Rect(x+363,y,68,52),"I",chapterSkillSlots[1]==id))EquipChapterSkill(id,1);
   }
   CenterLabel(new Rect(panel.x+20,panel.y+562,900,35),"回旋风刃：往返穿透，同一目标不重复计伤 · 护甲脉冲：恢复2护盾，8秒冷却",journeySmallStyle);
  }
 }
}
