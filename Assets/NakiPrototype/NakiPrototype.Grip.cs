using UnityEngine;

namespace NakiLegend.Prototype
{
    public sealed partial class NakiPrototype
    {
        private static readonly Vector2Int[,] MovementGripPixels = {
            { new Vector2Int(159,188), new Vector2Int(162,184), new Vector2Int(164,179), new Vector2Int(166,162) },
            { new Vector2Int(89,186), new Vector2Int(88,184), new Vector2Int(92,184), new Vector2Int(178,152) },
            { new Vector2Int(153,184), new Vector2Int(154,173), new Vector2Int(149,179), new Vector2Int(179,159) },
            { new Vector2Int(103,185), new Vector2Int(110,176), new Vector2Int(106,176), new Vector2Int(84,150) }
        };
        private static readonly Vector2Int[,] ActionsGripPixels = {
            { new Vector2Int(172,142), new Vector2Int(155,197), new Vector2Int(139,173), new Vector2Int(128,220) },
            { new Vector2Int(84,151), new Vector2Int(151,199), new Vector2Int(157,201), new Vector2Int(128,220) },
            { new Vector2Int(172,144), new Vector2Int(141,192), new Vector2Int(137,174), new Vector2Int(128,220) },
            { new Vector2Int(87,147), new Vector2Int(119,185), new Vector2Int(119,169), new Vector2Int(128,220) }
        };
        private static readonly Vector2Int[,] WalkGripPixels = {
            { new Vector2Int(159,184), new Vector2Int(162,182), new Vector2Int(165,180), new Vector2Int(160,185), new Vector2Int(166,174), new Vector2Int(163,183) },
            { new Vector2Int(85,184), new Vector2Int(88,183), new Vector2Int(88,182), new Vector2Int(88,183), new Vector2Int(90,185), new Vector2Int(91,184) },
            { new Vector2Int(156,184), new Vector2Int(160,183), new Vector2Int(155,181), new Vector2Int(154,183), new Vector2Int(151,179), new Vector2Int(159,181) },
            { new Vector2Int(102,190), new Vector2Int(102,190), new Vector2Int(107,183), new Vector2Int(107,183), new Vector2Int(107,183), new Vector2Int(107,183) }
        };
        private SpriteRenderer heldHandCover;

        // Measured on the actual 256px body frames, not on the old procedural hero.
        // The same source pixel coordinates define the socket and the finger cover.
        private void CalibratePolishedGrip(HeroFrame frame, HeroAction action, int direction, int sourcePose)
        {
            int row = direction == 0 ? 0 : direction == 1 ? 2 : direction == 2 ? 1 : 3;
            bool walkingSheet = frame.sprite.texture.name.Contains("walk");
            Vector2Int point = walkingSheet ? WalkGripPixels[row,sourcePose]
                : action == HeroAction.Idle || action == HeroAction.Attack || action == HeroAction.Walk
                    ? MovementGripPixels[row,sourcePose] : ActionsGripPixels[row,sourcePose];
            frame.grip = new Vector2((point.x-128)/130f,(232-point.y)/130f);
            if(action == HeroAction.Defeated) return;
            Rect source = frame.sprite.rect;
            frame.gripCover = Sprite.Create(frame.sprite.texture,
                new Rect(source.x+point.x-5,source.y+256-point.y-5,10,10),
                Vector2.one*.5f,130,0,SpriteMeshType.FullRect);
            frame.gripCover.name = "Grip cover " + frame.sprite.name;
            ownedAssets.Add(frame.gripCover);
        }

        private void DrawGripCover(HeroFrame frame, Vector2 at)
        {
            if(heldHandCover==null || heldHandCover.transform.parent!=world)
                heldHandCover=Draw("Weapon holding fingers",frame.gripCover,at,Vector2.one,
                    Color.white,heroBody.sortingOrder+2).GetComponent<SpriteRenderer>();
            heldHandCover.gameObject.SetActive(frame.gripCover!=null);
            heldHandCover.sprite=frame.gripCover;heldHandCover.flipX=frame.flipX;
            heldHandCover.color=heroBody.color;heldHandCover.sortingOrder=heroBody.sortingOrder+2;
            heldHandCover.transform.position=at;heldHandCover.transform.localScale=Vector3.one*pixelHeroScale;
        }
    }
}
