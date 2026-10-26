using UnityEngine;

namespace NakiLegend.Prototype
{
    public sealed partial class NakiPrototype
    {
        private bool polishedHeroReady;
        private float polishedWalkStarted;

        private void LoadPolishedHeroSprites()
        {
            polishedHeroReady = false;
            if (!usePolishedHeroArt) return;
            Texture2D movement = Resources.Load<Texture2D>("NakiArt/hero-movement-v5");
            Texture2D actions = Resources.Load<Texture2D>("NakiArt/hero-actions-v5");
            Texture2D walking = Resources.Load<Texture2D>("NakiArt/hero-walk-v5");
            if (movement == null || actions == null)
            {
                Debug.LogWarning("Naki Legend: polished hero atlas missing; using pixel hero fallback.");
                return;
            }
            movement.filterMode = FilterMode.Point;
            actions.filterMode = FilterMode.Point;
            var moveFrames = SliceHeroSheet(movement);
            var actionFrames = SliceHeroSheet(actions);
            Sprite[,] walkFrames = null;
            if (walking != null)
            {
                walking.filterMode = FilterMode.Point;
                walkFrames = SliceHeroSheet(walking, 6);
            }
            for (int direction = 0; direction < 4; direction++)
            {
                SetHeroAction(HeroAction.Idle, direction, moveFrames, 0, 0);
                if (walkFrames != null) SetHeroAction(HeroAction.Walk, direction, walkFrames, 0, 1, 2, 3, 4, 5);
                else SetHeroAction(HeroAction.Walk, direction, moveFrames, 0, 1, 0, 2);
                SetHeroAction(HeroAction.Attack, direction, moveFrames, 0, 1, 3, 3, 3, 0);
                SetHeroAction(HeroAction.Cast, direction, actionFrames, 0, 0, 0, 0);
                SetHeroAction(HeroAction.Dodge, direction, actionFrames, 1, 1, 1, 1);
                SetHeroAction(HeroAction.Hurt, direction, actionFrames, 2, 2);
                SetHeroAction(HeroAction.Defeated, direction, actionFrames, 3);
            }
            // The generated left-facing row barely moves its legs. Reuse the
            // approved right-facing gait in mirrored form, with the same phase.
            HeroFrame[] rightWalk = heroFrames[HeroAction.Walk][1];
            HeroFrame[] leftWalk = heroFrames[HeroAction.Walk][3];
            for (int i = 0; i < leftWalk.Length; i++)
            {
                leftWalk[i].sprite = rightWalk[i].sprite;
                leftWalk[i].flipX = true;
            }
            polishedHeroReady = true;
        }

        private Sprite[,] SliceHeroSheet(Texture2D texture, int columns = 4)
        {
            const int cell = 256;
            const float pixelsPerUnit = 130f;
            var sprites = new Sprite[4, columns];
            for (int direction = 0; direction < 4; direction++)
            {
                int sourceRow = direction == 0 ? 0 : direction == 1 ? 2 : direction == 2 ? 1 : 3;
                for (int pose = 0; pose < columns; pose++)
                {
                    var rect = new Rect(pose * cell, (3 - sourceRow) * cell, cell, cell);
                    Sprite sprite = Sprite.Create(texture, rect, new Vector2(.5f, 24f / cell), pixelsPerUnit, 0, SpriteMeshType.FullRect);
                    sprite.name = "NakiPolished_" + direction + "_" + pose;
                    sprites[direction, pose] = sprite;
                    ownedAssets.Add(sprite);
                }
            }
            return sprites;
        }

        private void SetHeroAction(HeroAction action, int direction, Sprite[,] sprites, params int[] poses)
        {
            HeroFrame[] previous = heroFrames[action][direction];
            var frames = new HeroFrame[poses.Length];
            for (int i = 0; i < frames.Length; i++)
            {
                HeroFrame old = previous[Mathf.Min(i, previous.Length - 1)];
                frames[i] = new HeroFrame { sprite = sprites[direction, poses[i]], grip = old.grip, weaponAngle = old.weaponAngle };
            }
            heroFrames[action][direction] = frames;
        }
    }
}
