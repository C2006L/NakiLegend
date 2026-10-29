using UnityEngine;
namespace NakiLegend.Prototype
{
    public sealed partial class NakiPrototype
    {
        // Derived from the active chapter; never accumulated on save/load.
        private int IslandTier { get { return CurrentIsland.ordinal-1; } }
        private float EffectiveMaximumShield { get { return maximumShield + CurrentIsland.shieldBonus; } }
        private float SkillStrength { get { return CurrentIsland.skillScale; } }
        private float SkillDamage(float amount) { return amount * SkillStrength; }
        private float WindGuardDuration { get { return thunderChapterMode ? 1.2f : 1; } }
        private float WindGuardMultiplier { get { return thunderChapterMode ? .65f : .7f; } }
        private float ChapterGroundDamage { get { return thunderChapterMode ? journeyRooms[currentRoom].stage == 2 ? 2 : 1.5f : 1; } }
        private void NormalizeOriginPower()
        {if(origin==Origin.Dark)energy=0;else if(origin==Origin.Light)darkPower=0;}
    }
}
