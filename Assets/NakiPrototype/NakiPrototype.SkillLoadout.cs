using System;
using System.IO;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  // IDs are global: travelling never changes the meaning of a selected slot.
  private static readonly string[] GlobalSkillNames={"风之刃","护身风波","雷标","跃电","冰锥","霜环","火种投射","烬环","回旋风刃","护甲脉冲","根刺","孢种"};
  [Serializable] private sealed class SkillLoadout {public int version,known,mastery;public int[] slots;public float[] remaining;}
  private int learnedElementSkills;
  private float forestRootReady,forestSporeReady;
  private void ResetSkillLoadout(){combatMasteryTier=0;learnedElementSkills=0;forestRootReady=forestSporeReady=0;chapterSkillSlots[0]=0;chapterSkillSlots[1]=1;chapterMarkKnown=chapterArcKnown=iceSpikeKnown=iceRingKnown=fireBoltKnown=emberRingKnown=false;}
  private bool ElementSkillKnown(int id)
  {
   if(id<0||id>=GlobalSkillNames.Length)return false;
   if((learnedElementSkills&(1<<id))!=0)return true;
   switch(id){case 0:return bladeLearned;case 1:return waveLearned;case 2:return !woodlandMode&&chapterMarkKnown;case 3:return !woodlandMode&&chapterArcKnown;case 4:return iceSpikeKnown;case 5:return iceRingKnown;case 6:return fireBoltKnown;case 7:return emberRingKnown;case 8:return ExtraSkillKnown(8);case 9:return ExtraSkillKnown(9);default:return false;}
  }
  private SkillLoadout CaptureSkillLoadout()
  {
   int known=learnedElementSkills;var remaining=new float[12];for(int i=0;i<12;i++){if(ElementSkillKnown(i))known|=1<<i;remaining[i]=Mathf.Max(0,ChapterSkillReady(i)-clock);}
   return new SkillLoadout{version=1,known=known,mastery=CombatMasteryTier,slots=(int[])chapterSkillSlots.Clone(),remaining=remaining};
  }
  private void ValidateSkillLoadout(SkillLoadout data)
  {
   if(EmptyLegacySkillLoadout(data))return;
   if(data.version!=1||data.known<0||data.known>=1<<12||data.mastery<0||data.mastery>5||data.slots==null||data.slots.Length!=2||data.slots[0]==data.slots[1]||data.remaining==null||data.remaining.Length!=12)throw new InvalidDataException("Invalid elemental skill loadout");
   foreach(int id in data.slots)if(id<0||id>=12)throw new InvalidDataException("Invalid elemental skill ID");
   foreach(float time in data.remaining)if(!FiniteRange(time,0,1000))throw new InvalidDataException("Invalid elemental skill cooldown");
  }
  private void RestoreSkillLoadout(SkillLoadout data,bool merge=false)
  {
   if(EmptyLegacySkillLoadout(data))return;ValidateSkillLoadout(data);learnedElementSkills=merge?CaptureSkillLoadout().known|data.known:data.known;
   combatMasteryTier=merge?Mathf.Max(CombatMasteryTier,data.mastery):Mathf.Max(CurrentIsland.ordinal-1,data.mastery);
   Array.Copy(data.slots,chapterSkillSlots,2);
   bladeLearned=(learnedElementSkills&1)!=0;waveLearned=(learnedElementSkills&2)!=0;
   chapterMarkKnown=(learnedElementSkills&(1<<(woodlandMode?10:2)))!=0;chapterArcKnown=(learnedElementSkills&(1<<(woodlandMode?11:3)))!=0;
   iceSpikeKnown=(learnedElementSkills&16)!=0;iceRingKnown=(learnedElementSkills&32)!=0;fireBoltKnown=(learnedElementSkills&64)!=0;emberRingKnown=(learnedElementSkills&128)!=0;
   for(int i=0;i<12;i++)SetElementSkillReady(i,clock+(merge?Mathf.Max(data.remaining[i],Mathf.Max(0,ChapterSkillReady(i)-clock)):data.remaining[i]));
  }
  private void SetElementSkillReady(int id,float ready)
  {switch(id){case 0:bladeReadyAt=ready;break;case 1:waveReadyAt=ready;break;case 2:chapterMarkReady=ready;break;case 3:chapterArcReady=ready;break;case 4:iceSpikeReady=ready;break;case 5:iceRingReady=ready;break;case 6:fireBoltReady=ready;break;case 7:emberRingReady=ready;break;case 8:returnBladeReady=ready;break;case 9:guardPulseReady=ready;break;case 10:forestRootReady=ready;break;case 11:forestSporeReady=ready;break;}}
  private void MigrateLegacySkillLoadout(ChapterCheckpoint saved)
  {
   if(saved==null||saved.player==null||!EmptyLegacySkillLoadout(saved.player.loadout)||saved.skills==null||saved.skills.Length!=2)return;
   saved.player.loadout=null;
   if(saved.island=="woodland")for(int i=0;i<2;i++)if(saved.skills[i]==2||saved.skills[i]==3)saved.skills[i]+=8;
  }
  // JsonUtility may materialize an absent nested record as an empty version-zero object.
  private static bool EmptyLegacySkillLoadout(SkillLoadout data){return data==null||data.version==0&&data.known==0&&data.mastery==0&&(data.slots==null||data.slots.Length==0)&&(data.remaining==null||data.remaining.Length==0);}
 }
}
