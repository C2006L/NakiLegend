using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private void RequestFireForestVoyage()
  {
   if(!fireChapterMode||!chapterEnding||!SafeCheckpointRoom||!WorldInteractionReady||StoryActive)return;
   pendingIceVoyage=507;voyageArmed=false;voyageOpened=Time.unscaledTime;CancelBowCharge();
  }
  private void EmbarkForestFromFire()
  {
   if(!fireChapterMode||!chapterEnding||!SaveChapterCheckpoint(false))return;
   var player=CaptureChapterPlayer();Origin old=origin;bool spike=iceSpikeKnown,ring=iceRingKnown,bolt=fireBoltKnown,ember=emberRingKnown;float cap=EffectiveMaximumShield,sw=swordDamage-initialSwordDamage,sp=spellDamage-initialSpellDamage;
   fireChapterMode=iceSampleMode=false;woodlandMode=true;thunderChapterPracticeBoss=-1;thunderChapterStartFloor=0;chapterCheckpointPathOverride=null;woodlandShieldFloor=cap;
   if(LoadVoyageChapter(player.talents,player)){woodlandShieldFloor=Mathf.Max(woodlandShieldFloor,cap);iceSpikeKnown|=spike;iceRingKnown|=ring;fireBoltKnown|=bolt;emberRingKnown|=ember;SaveChapterCheckpoint(false);return;}
   BeginThunderChapter(false);ApplyChapterPlayer(player);origin=old;NormalizeOriginPower();iceSpikeKnown=spike;iceRingKnown=ring;fireBoltKnown=bolt;emberRingKnown=ember;RestoreSkillLoadout(player.loadout);
   iceSpikeReady=iceRingReady=fireBoltReady=emberRingReady=clock;swordDamage=initialSwordDamage+sw;spellDamage=initialSpellDamage+sp;
   EnterExplorationRoom(0,-1);TellStory("炉匠把一包热面包放进行囊。船离岸时，镇上的第一缕炊烟正越过屋脊。","阿苓在青栖林海的栈桥等候：这些种子，已经很久没有发芽了。少年把受控的火种收在掌心，走向林缘。");RequestCheckpoint();SaveChapterCheckpoint(false);
  }
 }
}
