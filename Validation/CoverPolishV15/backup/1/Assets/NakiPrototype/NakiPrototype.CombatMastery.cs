using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private int combatMasteryTier;
  private static readonly float[] PhysicalTrainingOutput={1.05f,1.15f,1.25f,1.4f,1.5f,1.5f};
  private static readonly float[] StaffTrainingOutput={1.1f,1.2f,1.35f,1.55f,1.75f,1.75f};
  private static readonly float[] LightTrainingOutput={1,1.25f,1.5f,1.8f,2.1f,2.5f};
  private static readonly float[] StoneTrainingOutput={.85f,.95f,1.05f,1.15f,1.25f,1.35f};
  private int CombatMasteryTier
  {
   get{
    int tier=Mathf.Max(combatMasteryTier,CurrentIsland.ordinal-1);
    // Recover the completed journey from old saves that predate the mastery field.
    for(int i=0;i<5;i++)if((originLoreBits&(1<<i))!=0)tier=Mathf.Max(tier,i+1);
    return Mathf.Clamp(tier,0,5);
   }
  }
  private float OriginTrainingScale {get{return (origin==Origin.Dark?StoneTrainingOutput:LightTrainingOutput)[CombatMasteryTier];}}
  private float MeteorBaseDamage {get{return 14+CombatMasteryTier;}}
  private float OriginDamage(float amount){return SkillDamage(amount*OriginTrainingScale);}
  private float LightGuardDuration {get{return 1.5f+CombatMasteryTier*.1f;}}
  private string MasteryCaption {get{return "修习 "+(CombatMasteryTier+1)+"阶";}}
 }
}
