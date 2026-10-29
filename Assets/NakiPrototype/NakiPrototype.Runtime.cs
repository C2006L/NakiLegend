using UnityEngine;

namespace NakiLegend.Prototype
{
    public sealed partial class NakiPrototype
    {
        // Unity restores component fields across a Play-mode assembly reload, but
        // readonly dictionaries and runtime-only room/weapon objects are rebuilt empty.
        // Recreate them together; restoring just one animation would leave combat broken.
        private bool RuntimeCachesMissing()
        {
            return world != null &&
                ((useAnimatedPixelHero && heroFrames.Count != 7) ||
                 (enableDesertJourney && (weaponCatalog == null || journeyRooms.Count == 0)));
        }

        private void OnEnable()
        {
            if (Application.isPlaying && RuntimeCachesMissing()) RebuildRuntimeAfterReload();
        }

        private void RebuildRuntimeAfterReload()
        {
            StopOpeningCinematic();
            if (world != null) { world.gameObject.SetActive(false); Destroy(world.gameObject); }
            world = null;
            foreach (Object asset in ownedAssets) if (asset != null) Destroy(asset);
            ownedAssets.Clear(); heroFrames.Clear(); enemyActionArt.Clear();worldPropSprites.Clear();woodPropBlocks.Clear();woodPropRoom=-1;
            pixelHeroReady = polishedHeroReady = layeredHeroReady = false;
            windRingSprite = sectorWarning = essenceSprite = null;
            journeyTitleStyle = journeyTextStyle = journeySmallStyle = journeyButtonStyle = journeyNodeStyle = null;
            openingCaptionStyle = null;
            Start();if(thunderChapterMode){chapterSetup=false;bool resumed=LoadChapterCheckpoint(false);JourneyHint(resumed?"脚本已更新 · 已恢复第二岛安全点":"脚本已更新 · 已重开本次练习");return;}if(thunderPrototypeMode){thunderSetupMenu=false;LoadThunderCheckpoint(false);}
            if (enableDesertJourney) JourneyHint(checkpointStatus.StartsWith("已读")?"脚本已更新 · 已恢复最近安全房间":"脚本已更新 · 已重新开始本轮旅程");
        }
    }
}
