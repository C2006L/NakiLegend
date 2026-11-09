using System;
using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private int[] learnedTalents={-1,-1,-1,-1,-1,-1};
  private bool talentMenu;
  private int talentSelection;
  private float repairTalentReady;
  private readonly int[] talentOffers=new int[3];
  private static readonly string[] TalentNames={"迎刃反击","碎甲余护","战地整备","熟练拉弦","元素容器","节能施法","元素余韵","结晶采集"};
  private static readonly string[] TalentDetails={"近战命中窗口内，可反射攻击范围内的普通敌弹。","护盾被一击打碎时，本次溢出伤害不扣生命；无盾时仍会受伤。","每8秒尝试消耗4元素恢复1护盾；受击后至少等2秒，不消耗光之力与贤者之石。","弓弩蓄力时间缩短25%，快射与攻击间隔不变。","元素上限从100提升到140，立即补充新增的40容量。","技能元素费用再降低20%；武器费用与流星1贤者之石能量不变。","元素技能伤害提高10%，不提高普通武器伤害。","敌人掉落的元素结晶增加25%，构造物无灵魂规则不变。"};
  private bool HasTalent(int id){return Array.IndexOf(learnedTalents,id)>=0;}
  private static float SavedElementMaximum(int[] talents){return talents!=null&&Array.IndexOf(talents,4)>=0?140:100;}
  private float ElementMaximum {get{return SavedElementMaximum(learnedTalents);}}
  private void ResetTalents(){learnedTalents=new[]{-1,-1,-1,-1,-1,-1};talentMenu=false;talentSelection=0;repairTalentReady=0;}
  private int[] CopyTalents(){return (int[])learnedTalents.Clone();}
  private static void ValidateTalents(int[] data)
  {
   if(data==null||data.Length==0)return;if(data.Length!=6)throw new System.IO.InvalidDataException("Invalid talent ledger");
   var seen=new HashSet<int>();foreach(int id in data)if(id < -1||id>=TalentNames.Length||id>=0&&!seen.Add(id))throw new System.IO.InvalidDataException("Invalid or repeated talent");
  }
  private void RestoreTalents(int[] data){ValidateTalents(data);learnedTalents=data==null||data.Length==0?new[]{-1,-1,-1,-1,-1,-1}:(int[])data.Clone();talentMenu=false;repairTalentReady=clock+8;}
  private void MergeVoyageTalents(int[] source)
  {
   // The travelled player's decision wins a conflict; one choice per island.
   if(source==null)return;var merged=(int[])source.Clone();
   for(int i=0;i<6;i++)if(merged[i]<0&&learnedTalents[i]>=0&&Array.IndexOf(merged,learnedTalents[i])<0)merged[i]=learnedTalents[i];
   RestoreTalents(merged);elementPower=Mathf.Min(elementPower,ElementMaximum);RequestCheckpoint();
  }
  private bool LoadVoyageChapter(int[] source,ThunderCheckpoint journey=null){if(journey==null)journey=CaptureChapterPlayer();if(!LoadChapterCheckpoint(false))return false;MergeOriginJourney(journey);MergeVoyageTalents(source);RestoreSkillLoadout(journey.loadout,true);SaveChapterCheckpoint(false);return true;}
  private bool IslandTalentReady {get{return !capitalMode&&!bossPracticeMode&&!gatehouseSampleMode&&!thunderPrototypeMode&&(!thunderChapterMode?canalEpilogueComplete:thunderChapterPracticeBoss<0&&chapterStone&&chapterEnding&&(!woodlandMode||woodEndingCgSeen)&&(!iceSampleMode||iceFullChapter&&iceEndingCgSeen));}}
  private void BuildTalentOffers()
  {
   var pool=new List<int>();for(int i=0;i<TalentNames.Length;i++)if(!HasTalent(i))pool.Add(i);
   var rng=new System.Random(activeSeed+CurrentIsland.ordinal*13007);
   for(int i=pool.Count-1;i>0;i--){int k=rng.Next(i+1),v=pool[i];pool[i]=pool[k];pool[k]=v;}
   for(int i=0;i<3;i++)talentOffers[i]=i<pool.Count?pool[i]:-1;
   talentSelection=0;
  }
  private bool HandleTalents()
  {
   if(!talentMenu&&IslandTalentReady&&learnedTalents[CurrentIsland.ordinal-1]<0&&SafeCheckpointRoom&&!StoryActive&&!showRainCG&&!chapterCg&&!chapterSetup&&!openingActive&&!AudioSuspended){BuildTalentOffers();talentMenu=true;CancelBowCharge();}
   if(!talentMenu)return false;
   if(Pressed('a'))talentSelection=(talentSelection+2)%3;if(Pressed('d'))talentSelection=(talentSelection+1)%3;
   if(Pressed('f'))LearnTalent(talentSelection);return true;
  }
  private void LearnTalent(int choice)
  {
   int island=CurrentIsland.ordinal-1;if(!talentMenu||!IslandTalentReady||learnedTalents[island]>=0||choice<0||choice>2)return;
   int id=talentOffers[choice];if(id<0||HasTalent(id))return;
   float oldMax=ElementMaximum;learnedTalents[island]=id;elementPower=Mathf.Min(ElementMaximum,elementPower+ElementMaximum-oldMax);
   repairTalentReady=clock+8;talentMenu=false;BlockWorldInteraction();JourneyHint("已学会天赋："+TalentNames[id]+(island==0?" · 已领悟回旋风刃，可在下一岛按 T 配置":island==1?" · 已领悟护甲脉冲，按 T 配置":""));RequestCheckpoint();
   if(thunderChapterMode)SaveChapterCheckpoint(false);else SaveCheckpoint(false);
  }
  private void DrawTalentChoice(float w,float h)
  {
   HudRect(new Rect(0,0,w,h),new Color(.025f,.05f,.07f,.96f));
   CenterLabel(new Rect(w/2-450,h/2-230,900,45),CurrentIsland.name+" · 旅途所悟",journeyTitleStyle);
   CenterLabel(new Rect(w/2-450,h/2-175,900,35),"选择一项天赋，带入接下来的旅程",journeyTextStyle);
   for(int i=0;i<3;i++){int id=talentOffers[i];if(id<0)continue;Rect card=new Rect(w/2-450+i*305,h/2-105,285,245);if(FlatButton(card,"",talentSelection==i))talentSelection=i;CenterLabel(new Rect(card.x+15,card.y+24,255,40),TalentNames[id],journeyTitleStyle);FittedLabel(new Rect(card.x+20,card.y+85,245,125),TalentDetails[id],journeyTextStyle,true);}
   if(FlatButton(new Rect(w/2-180,h/2+170,360,55),"F 学习所选天赋"))LearnTalent(talentSelection);
   CenterLabel(new Rect(w/2-430,h/2+240,860,28),"A / D 或点击选卡 · 同岛仅一次，选择随安全存档保存",journeySmallStyle);
  }
  private string TalentSummary(){var names=new List<string>();foreach(int id in learnedTalents)if(id>=0)names.Add(TalentNames[id]);return names.Count==0?"尚无天赋 · 岛屿收尾后领悟":"已学天赋："+string.Join("、",names);}
  private void TickRepairTalent()
  {
   if(!HasTalent(2)||clock<repairTalentReady||shield>=EffectiveMaximumShield||clock-lastDamageAt<2)return;
   repairTalentReady=clock+8;if(elementPower<ElementCost(4))return;
   SpendPower(PowerPool.Elemental,4);shield=Mathf.Min(EffectiveMaximumShield,shield+1);SpellSpark(position,C("81dce4"),.35f);
  }
  private void ReflectMeleeProjectiles()
  {
   if(!HasTalent(0))return;
   foreach(Shot shot in shots)if(shot.hostile&&shot.dispellable&&Vector2.Distance(position,shot.position)<CurrentWeapon.range+.2f&&Vector2.Dot(swingDirection,(shot.position-position).normalized)>.25f&&!ProjectilePathBlocked(position,shot.position,.04f))
   {if(CurrentWeapon.type==WeaponType.Spear&&DistanceToSegment(shot.position,position,position+swingDirection*CurrentWeapon.range)>.3f)continue;shot.hostile=false;shot.damage=12;shot.direction=-shot.direction;shot.tracking=false;shot.target=null;shot.life=1.4f;shot.speed=9;shot.element=Element.Wind;if(shot.root.GetComponent<SpriteRenderer>().sprite==arrowSprite)shot.root.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(shot.direction.y,shot.direction.x)*Mathf.Rad2Deg);shot.root.GetComponent<SpriteRenderer>().color=C("85eed1");}
  }
 }
}
