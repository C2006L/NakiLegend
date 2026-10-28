using System.Collections.Generic;
using UnityEngine;

namespace NakiLegend.Prototype
{
    public sealed partial class NakiPrototype
    {
        [Header("Animated pixel hero")]
        public bool useAnimatedPixelHero = true;
        public bool usePolishedHeroArt = true;
        [Range(.75f, 1.25f)] public float pixelHeroScale = 1f;

        private enum HeroAction { Idle, Walk, Attack, Cast, Dodge, Hurt, Defeated }
        private sealed class HeroFrame
        {
            public Sprite sprite;
            public bool flipX;
            public Vector2 grip;
            public Sprite gripCover;
            public float weaponAngle;
        }
        private sealed class DashEcho
        {
            public GameObject root;
            public SpriteRenderer renderer;
            public float expires;
        }

        private const int HeroFrameSize = 40;
        private const float HeroPixelsPerUnit = 26;
        private readonly Dictionary<HeroAction, HeroFrame[][]> heroFrames = new Dictionary<HeroAction, HeroFrame[][]>();
        private readonly List<DashEcho> dashEchoes = new List<DashEcho>();
        private Texture2D heroAtlas;
        private Sprite pixelSword, pixelSlash;
        private bool pixelHeroReady, swordHitPending;
        private float swordHitAt, pixelHurtUntil, nextEchoAt;
        private int nextEchoIndex;
        private int pixelDirection;
        private HeroAction currentHeroAction;
        private int currentHeroFrame;

        private static readonly Dictionary<char, Color32> HeroPalette = new Dictionary<char, Color32> {
            { 'k', C("151c2b") }, { 'h', C("242c40") }, { 'H', C("3c4962") }, { 'a', C("64738a") },
            { 's', C("efb780") }, { 'S', C("ffe0ac") }, { 'n', C("bd7d55") },
            { 'b', C("294c7b") }, { 'B', C("426f9e") }, { 'c', C("78a4bf") },
            { 'i', C("e6d9b6") }, { 'I', C("fff1cb") }, { 't', C("ac9474") },
            { 'l', C("60402f") }, { 'L', C("9e6e43") }, { 'g', C("ebbd57") },
            { 'G', C("976b35") }, { 'w', C("e9f4f5") }, { 'v', C("9baebd") },
            { 'd', C("35415a") }, { 'e', C("68e2e0") }
        };

        // Authored pixel parts are composited into real pose frames once at startup.
        // Head/body/legs/hands change independently; runtime playback never rotates a static portrait.
        private static readonly string[] HeadFront = {
            ".....kk...k.....", "...kkHHkkhkk....", "..kHHHHHHHhhk...", ".kHHHaHHHHHhhk..",
            ".kHHHHHHHHHhhhk.", "kHHHHHHHHHHhhhk.", ".khHHhHHhhHHhhk.",
            ".khhsshhssshhhk.", "..knSSSSSSSSnk..", "..ksSkkSSkkSsk..",
            "...sSwkSSwkSs...", "...knSSSSSSnk...", "....kssssssk....", ".....kkkkkk....."
        };
        private static readonly string[] HeadSide = {
            ".....kk...k.....", "...kkHHkkhkk....", "..kHHHHHHHhhk...", ".kHHHaHHHHHhhk..",
            ".kHHHHHHHHHhhhk.", "kHHHHHHHHHHhhhk.", ".khHHHHHHhhSSk..",
            ".khHHHHhhSSSSSk.", "..khhssSSSSSSSk.", "..khnsnSSSkkSSk.",
            "...knsnSSSwkSSk.", "....khSSSSSSSk..", ".....kssssssk...", "......kkkkkk...."
        };
        private static readonly string[] HeadBack = {
            ".....kk...k.....", "...kkHHkkhkk....", "..kHHHHHHHhhk...", ".kHHHaHHHHHhhk..",
            ".kHHHHHHHHHhhhk.", "kHHHHHHHHHHhhhk.", ".khHHHHHHHHhhhk.",
            ".khHHHHHHHhhhhk.", "..khHHHHHhhhhk..", "..khhHHHhhhhhk..",
            "...khhhhhhhhk...", "...kkhhhhhhkk...", "....kkhhhhkk....", ".....kkkkkk....."
        };
        private static readonly string[] Tunic = {
            "..kkkkkkkk..", ".kbbccccbbk.", "kbiIIIiIigbk", "kiIIIlIIIiik",
            "kiIIIlIIIitk", ".kiIilIIitk.", ".klllGglllk.", "..kiittitk.."
        };

        private sealed class PixelCanvas
        {
            public readonly Color32[] pixels = new Color32[HeroFrameSize * HeroFrameSize];
            public void Dot(int x, int y, char color)
            {
                if (x < 0 || y < 0 || x >= HeroFrameSize || y >= HeroFrameSize || color == '.') return;
                pixels[y * HeroFrameSize + x] = HeroPalette[color];
            }
            public void Box(int x, int y, int width, int height, char color)
            {
                for (int row = y; row < y + height; row++) for (int col = x; col < x + width; col++) Dot(col, row, color);
            }
            public void Stamp(string[] rows, int x, int y)
            {
                for (int row = 0; row < rows.Length; row++)
                    for (int col = 0; col < rows[row].Length; col++) Dot(x + col, y + rows.Length - row - 1, rows[row][col]);
            }
            public void Stroke(Vector2Int a, Vector2Int b, int thickness, char color)
            {
                int steps = Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
                for (int i = 0; i <= steps; i++)
                {
                    float t = steps == 0 ? 0 : i / (float)steps;
                    int x = Mathf.RoundToInt(Mathf.Lerp(a.x, b.x, t)), y = Mathf.RoundToInt(Mathf.Lerp(a.y, b.y, t));
                    Box(x - thickness / 2, y - thickness / 2, thickness, thickness, color);
                }
            }
            public void Mirror()
            {
                for (int y = 0; y < HeroFrameSize; y++) for (int x = 0; x < HeroFrameSize / 2; x++)
                {
                    int a = y * HeroFrameSize + x, b = y * HeroFrameSize + HeroFrameSize - 1 - x;
                    Color32 swap = pixels[a]; pixels[a] = pixels[b]; pixels[b] = swap;
                }
            }
        }

        private static int PoseCount(HeroAction action)
        {
            switch (action) {
                case HeroAction.Walk: case HeroAction.Attack: return 6;
                case HeroAction.Cast: case HeroAction.Dodge: return 4;
                case HeroAction.Defeated: return 1;
                default: return 2;
            }
        }

        private void LoadPixelHero()
        {
            if (!useAnimatedPixelHero) return;
            // Seven action rows per direction; six columns leave every frame aligned to a 40px grid.
            heroAtlas = new Texture2D(HeroFrameSize * 6, HeroFrameSize * 28, TextureFormat.RGBA32, false);
            heroAtlas.name = "NakiHero-4directions-100poses";
            heroAtlas.filterMode = FilterMode.Point;
            heroAtlas.wrapMode = TextureWrapMode.Clamp;
            heroAtlas.SetPixels32(new Color32[heroAtlas.width * heroAtlas.height]);
            ownedAssets.Add(heroAtlas);
            foreach (HeroAction action in System.Enum.GetValues(typeof(HeroAction)))
            {
                var directions = new HeroFrame[4][];
                for (int direction = 0; direction < 4; direction++)
                {
                    int count = PoseCount(action);
                    directions[direction] = new HeroFrame[count];
                    for (int frame = 0; frame < count; frame++)
                    {
                        PixelCanvas canvas;
                        HeroFrame pose = PaintHeroPose(action, direction, frame, out canvas);
                        int x = frame * HeroFrameSize, y = ((3 - direction) * 7 + (6 - (int)action)) * HeroFrameSize;
                        heroAtlas.SetPixels32(x, y, HeroFrameSize, HeroFrameSize, canvas.pixels);
                        pose.sprite = Sprite.Create(heroAtlas, new Rect(x, y, HeroFrameSize, HeroFrameSize), new Vector2(.5f, .15f), HeroPixelsPerUnit, 0, SpriteMeshType.FullRect);
                        pose.sprite.name = "Naki_" + direction + "_" + action + "_" + frame;
                        ownedAssets.Add(pose.sprite);
                        directions[direction][frame] = pose;
                    }
                }
                heroFrames.Add(action, directions);
            }
            heroAtlas.Apply(false, false);
            pixelSword = MakeSprite(new[] {
                "......k.....................", ".....kgk....................", ".....kgk....................",
                ".....kgkkkkkkkkkkkkkk.......", ".kkkkkgGwwwwwwwwwwwwwkk.....", "klLLlggeIIIIvvvvvvvvvvwwk...",
                "klLLlggGvvvvvvvvvvvvvvwwk...", ".kkkkkgGkkkkkkkkkkkkkkk.....", ".....kgk....................",
                ".....kgk....................", "......k.....................", "............................"
            }, ToColorPalette(), 28);
            // Move the pivot from the center to the grip, so the hand anchors the hilt throughout a swing.
            Sprite gripSword = Sprite.Create(pixelSword.texture, pixelSword.rect, new Vector2(3.5f / 28, 6f / 12), 28, 0, SpriteMeshType.FullRect);
            gripSword.name = "Naki_pixel_sword"; ownedAssets.Add(gripSword); pixelSword = gripSword;
            pixelSlash = BuildSlashSprite();
            LoadPolishedHeroSprites();
            heroSprite = heroFrames[HeroAction.Idle][0][0].sprite;
            if (softDisc == null) softDisc = CreateSoftDisc();
            pixelHeroReady = true;
        }

        private static Dictionary<char, Color> ToColorPalette()
        {
            var palette = new Dictionary<char, Color>();
            foreach (var pair in HeroPalette) palette[pair.Key] = pair.Value;
            return palette;
        }

        private HeroFrame PaintHeroPose(HeroAction action, int direction, int frame, out PixelCanvas canvas)
        {
            canvas = new PixelCanvas();
            bool side = direction == 1 || direction == 3, back = direction == 2;
            int[] stride = { -2, -1, 1, 2, 1, -1 };
            int walk = action == HeroAction.Walk ? stride[frame] : 0;
            int bob = action == HeroAction.Walk && (frame == 1 || frame == 4) ? 1 : 0;
            if (action == HeroAction.Idle) bob = frame;
            int lean = 0, crouch = 0;
            float baseAngle = side ? 0 : back ? 90 : -90;
            float angle = side ? -20 : back ? 65 : -25;
            if (action == HeroAction.Attack)
            {
                int[] leanFrames = { -1, -2, 2, 2, 1, 0 };
                float[] angles = { -65, -105, -35, 45, 70, 15 };
                lean = side ? leanFrames[frame] : 0;
                crouch = frame < 2 ? 1 : 0;
                angle = baseAngle + angles[frame];
            }
            if (action == HeroAction.Cast) { bob = frame == 1 || frame == 2 ? 1 : 0; angle = baseAngle + (frame == 0 ? -35 : 0); }
            if (action == HeroAction.Dodge) { lean = side ? 3 : 0; crouch = frame == 0 || frame == 3 ? 3 : 5; angle = baseAngle; }
            if (action == HeroAction.Hurt) { lean = side ? -2 : frame == 0 ? -1 : 1; crouch = frame == 0 ? 2 : 1; angle -= 25; }
            if (action == HeroAction.Defeated) { crouch = 7; lean = -3; angle = -70; }
            int cx = 20 + lean, bodyY = 12 + bob - crouch;
            int flap = action == HeroAction.Walk ? walk / 2 : action == HeroAction.Dodge ? -4 : action == HeroAction.Attack ? (frame < 2 ? 1 : -2) : 0;

            // Cape silhouette, including a trailing tip that changes with the body's motion.
            for (int y = 0; y < 12; y++)
            {
                int half = y > 8 ? 5 : 7;
                int shift = (12 - y) * flap / 12;
                canvas.Box(cx - half + shift, bodyY - 1 + y, half * 2, 1, 'k');
                canvas.Box(cx - half + 1 + shift, bodyY - 1 + y, half * 2 - 2, 1, y % 4 == 0 ? 'B' : 'b');
            }

            int leftX = side ? cx - 3 : cx - 5, rightX = side ? cx + 1 : cx + 2;
            int leftY = 6 + (side ? Mathf.Max(0, walk) : walk), rightY = 6 + (side ? Mathf.Max(0, -walk) : -walk);
            if (action == HeroAction.Attack) { leftX -= frame >= 2 && frame <= 3 ? 1 : 0; rightX += frame >= 2 && frame <= 3 ? 1 : 0; }
            if (action == HeroAction.Dodge || action == HeroAction.Defeated) { leftX -= 2; rightX += 1; leftY = rightY = 6; }
            DrawBoot(canvas, leftX, leftY, bodyY + 3);
            DrawBoot(canvas, rightX, rightY, bodyY + 3);

            Vector2Int leftShoulder = new Vector2Int(cx - 5, bodyY + 6), rightShoulder = new Vector2Int(cx + 5, bodyY + 6);
            Vector2Int offhand = new Vector2Int(cx - 7, bodyY + 1 - walk / 2);
            Vector2Int hand = new Vector2Int(cx + 7, bodyY + 1 + walk / 2);
            if (action == HeroAction.Attack)
            {
                float radius = frame < 2 ? 7 : 10;
                float rad = angle * Mathf.Deg2Rad;
                hand = new Vector2Int(cx + Mathf.RoundToInt(Mathf.Cos(rad) * radius), bodyY + 4 + Mathf.RoundToInt(Mathf.Sin(rad) * radius * .65f));
                offhand = new Vector2Int(cx - 6, bodyY + 3 + (frame < 2 ? 1 : -2));
            }
            else if (action == HeroAction.Cast)
            {
                int lift = frame == 0 ? 2 : frame == 3 ? 3 : 6;
                hand = new Vector2Int(cx + (side ? 9 : 7), bodyY + lift);
                offhand = new Vector2Int(cx - 5, bodyY + lift - 1);
            }
            else if (action == HeroAction.Dodge)
            {
                hand = new Vector2Int(cx + 8, bodyY + 3); offhand = new Vector2Int(cx - 8, bodyY + 4);
            }
            else if (action == HeroAction.Hurt) { offhand.y += 3; hand.y += 2; }

            DrawArm(canvas, leftShoulder, offhand);
            if (back) DrawArm(canvas, rightShoulder, hand);
            canvas.Stamp(Tunic, cx - 6, bodyY);
            if (back)
            {
                canvas.Box(cx - 6, bodyY, 12, 9, 'k');
                canvas.Box(cx - 5, bodyY + 1, 10, 8, 'b');
                canvas.Box(cx - 4, bodyY + 5, 8, 3, 'B');
                canvas.Box(cx - 2, bodyY + 2, 2, 5, 'B');
                canvas.Dot(cx + 4, bodyY + 7, 'g');
            }
            canvas.Stamp(back ? HeadBack : side ? HeadSide : HeadFront, cx - 8, bodyY + 7);
            // Cape collar and tiny feather clasp remain recognizable at gameplay scale.
            canvas.Box(cx - 5, bodyY + 7, 10, 2, 'b');
            canvas.Box(cx - 3, bodyY + 8, 7, 1, 'B');
            if (!back) { canvas.Dot(cx + 5, bodyY + 7, 'g'); canvas.Dot(cx + 6, bodyY + 6, 'w'); canvas.Dot(cx + 5, bodyY + 5, 'i'); }
            if (!back) DrawArm(canvas, rightShoulder, hand);
            if (action == HeroAction.Cast && (frame == 1 || frame == 2))
            {
                canvas.Dot(offhand.x - 2, offhand.y + 3, 'g');
                canvas.Dot(offhand.x - 1, offhand.y + 4, 'w');
                canvas.Dot(offhand.x, offhand.y + 3, 'g');
            }
            if (direction == 3) { canvas.Mirror(); hand.x = HeroFrameSize - 1 - hand.x; angle = 180 - angle; }
            return new HeroFrame { grip = new Vector2((hand.x + .5f - 20) / HeroPixelsPerUnit, (hand.y + .5f - 6) / HeroPixelsPerUnit), weaponAngle = angle };
        }

        private static void DrawBoot(PixelCanvas canvas, int x, int y, int hipY)
        {
            canvas.Box(x, y + 2, 4, Mathf.Max(3, hipY - y - 1), 'k');
            canvas.Box(x + 1, y + 3, 2, Mathf.Max(2, hipY - y - 3), 'd');
            canvas.Box(x - 1, y, 6, 4, 'k');
            canvas.Box(x, y + 1, 4, 2, 'l');
            canvas.Box(x, y + 2, 3, 1, 'L');
        }

        private static void DrawArm(PixelCanvas canvas, Vector2Int shoulder, Vector2Int hand)
        {
            Vector2Int elbow = new Vector2Int((shoulder.x + hand.x) / 2, (shoulder.y + hand.y) / 2 - 1);
            canvas.Stroke(shoulder, elbow, 5, 'k'); canvas.Stroke(shoulder, elbow, 3, 'i');
            canvas.Stroke(elbow, hand, 4, 'k'); canvas.Stroke(elbow, hand, 2, 'L');
            canvas.Box(hand.x - 2, hand.y - 2, 4, 4, 'k');
            canvas.Box(hand.x - 1, hand.y - 1, 2, 2, 's'); canvas.Dot(hand.x, hand.y, 'S');
        }

        private Sprite BuildSlashSprite()
        {
            const int size = 48;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point; texture.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                Vector2 p = new Vector2(x - 23.5f, y - 23.5f);
                float angle = Mathf.Abs(Mathf.Atan2(p.y, p.x) * Mathf.Rad2Deg);
                float radius = p.magnitude;
                Color color = Color.clear;
                if (angle < 65 && radius > 18 && radius < 23) color = radius > 21 ? C("fff4cb") : new Color(1, .73f, .3f, .65f);
                texture.SetPixel(x, y, color);
            }
            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * .5f, 24, 0, SpriteMeshType.FullRect);
            ownedAssets.Add(sprite); ownedAssets.Add(texture); return sprite;
        }

        private void SetupPixelHero()
        {
            heldHandCover = null;
            currentHeroAction = HeroAction.Idle;
            pixelDirection = 1; pixelHurtUntil = 0; nextEchoAt = 0; nextEchoIndex = 0; swordHitPending = false;
            dashEchoes.Clear();
            heroShadow = Draw("Pixel hero shadow", softDisc, position, new Vector2(.65f, .2f), new Color(.08f, .06f, .04f, .35f), -799);
            sword.GetComponent<SpriteRenderer>().sprite = pixelSword;
            sword.GetComponent<SpriteRenderer>().color = Color.white;
            sword.transform.localScale = Vector3.one * pixelHeroScale;
            weaponTrail = Draw("Pixel sword arc", pixelSlash, position, Vector2.one * pixelHeroScale, Color.white, 500);
            weaponTrail.SetActive(false);
            for (int i = 0; i < 4; i++)
            {
                GameObject obj = Draw("Dodge echo " + i, heroSprite, position, Vector2.one * pixelHeroScale, new Color(.4f, .8f, 1, 0), 0);
                obj.SetActive(false);
                dashEchoes.Add(new DashEcho { root = obj, renderer = obj.GetComponent<SpriteRenderer>() });
            }
            UpdatePixelHero(Vector2.zero);
        }

        private static int FacingIndex(Vector2 aim)
        {
            if (Mathf.Abs(aim.x) > Mathf.Abs(aim.y)) return aim.x > 0 ? 1 : 3;
            return aim.y > 0 ? 2 : 0;
        }

        private void UpdatePixelHero(Vector2 movement)
        {
            if (!pixelHeroReady) return;
            HeroAction action;
            float phase;
            Vector2 aim = facing;
            if (health <= 0) { action = HeroAction.Defeated; phase = 0; }
            else if (won) { action = HeroAction.Idle; phase = 0; }
            else if (clock < pixelHurtUntil) { action = HeroAction.Hurt; phase = 1 - (pixelHurtUntil - clock) / .2f; }
            else if (clock < dashUntil) { action = HeroAction.Dodge; phase = 1 - (dashUntil - clock) / .17f; aim = dashDirection; }
            else if (clock < swordUntil) { action = HeroAction.Attack; phase = (clock - swingStarted) / (enableDesertJourney ? meleeActionDuration : .32f); aim = swingDirection; }
            else if (clock < castPoseUntil) { action = HeroAction.Cast; phase = 1 - (castPoseUntil - clock) / .32f; aim = castDirection; }
            else if (movement.sqrMagnitude > .01f)
            {
                action = HeroAction.Walk;
                if (currentHeroAction != HeroAction.Walk) polishedWalkStarted = clock;
                phase = polishedHeroReady ? Mathf.Repeat((clock - polishedWalkStarted) * 1.6f, 1) : Mathf.Repeat(clock * 2, 1);
            }
            else { action = HeroAction.Idle; phase = Mathf.Repeat(clock * .85f, 1); }
            if (aim.sqrMagnitude > .01f) pixelDirection = FacingIndex(aim);
            HeroFrame[] frames = heroFrames[action][pixelDirection];
            int frameIndex = Mathf.Clamp(Mathf.FloorToInt(phase * frames.Length), 0, frames.Length - 1);
            HeroFrame frame = frames[frameIndex];
            currentHeroAction = action; currentHeroFrame = frameIndex;
            Vector2 root = polishedHeroReady ? position : new Vector2(Mathf.Round(position.x * HeroPixelsPerUnit) / HeroPixelsPerUnit, Mathf.Round(position.y * HeroPixelsPerUnit) / HeroPixelsPerUnit);
            hero.transform.position = root;
            hero.transform.localScale = Vector3.one * pixelHeroScale;
            heroBody.sprite = frame.sprite; heroBody.flipX = frame.flipX;
            heroBody.sortingOrder = DepthAt(position.y);
            heroShadow.transform.position = root + Vector2.up * (polishedHeroReady ? .02f : .1f) * pixelHeroScale;
            heroShadow.transform.localScale = new Vector3(.65f, .2f, 1) * pixelHeroScale;
            sword.SetActive(!polishedHeroReady && action != HeroAction.Defeated);
            sword.transform.localScale = Vector3.one * pixelHeroScale;
            sword.transform.position = root + frame.grip * pixelHeroScale;
            float weaponAngle = frame.weaponAngle;
            if (action == HeroAction.Attack || action == HeroAction.Cast || action == HeroAction.Dodge)
            {
                float cardinalAngle = pixelDirection == 0 ? -90 : pixelDirection == 1 ? 0 : pixelDirection == 2 ? 90 : 180;
                weaponAngle += Mathf.DeltaAngle(cardinalAngle, Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg);
            }
            sword.transform.rotation = Quaternion.Euler(0, 0, weaponAngle);
            sword.GetComponent<SpriteRenderer>().sortingOrder = heroBody.sortingOrder + (pixelDirection == 2 ? -1 : 1);
            if (enableDesertJourney && layeredHeroReady) DrawHeldEquipment(action,pixelDirection,frameIndex,phase,root,aim);
            weaponTrail.SetActive(action == HeroAction.Attack && frameIndex >= 2 && frameIndex <= 3);
            if (weaponTrail.activeSelf)
            {
                bool spear=enableDesertJourney&&CurrentWeapon.type==WeaponType.Spear;
                weaponTrail.GetComponent<SpriteRenderer>().sprite=spear?square:pixelSlash;
                weaponTrail.transform.localScale=spear?new Vector3(CurrentWeapon.range*.75f,.045f,1):Vector3.one*pixelHeroScale;
                weaponTrail.transform.position = root + Vector2.up * .35f * pixelHeroScale;
                if(spear)weaponTrail.transform.position+=(Vector3)(swingDirection*CurrentWeapon.range*.5f);
                weaponTrail.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(swingDirection.y, swingDirection.x) * Mathf.Rad2Deg);
                weaponTrail.GetComponent<SpriteRenderer>().sortingOrder = heroBody.sortingOrder + 2;
            }
            foreach (DashEcho echo in dashEchoes)
            {
                float remaining = echo.expires - clock;
                echo.root.SetActive(remaining > 0);
                echo.renderer.color = new Color(.5f, .8f, 1, Mathf.Clamp01(remaining / .18f) * .35f);
            }
            if (action == HeroAction.Dodge && clock >= nextEchoAt)
            {
                DashEcho echo = dashEchoes[nextEchoIndex++ % dashEchoes.Count];
                echo.root.transform.position = hero.transform.position;
                echo.root.transform.localScale = hero.transform.localScale;
                echo.renderer.sprite = frame.sprite; echo.renderer.flipX = frame.flipX; echo.renderer.sortingOrder = heroBody.sortingOrder - 3;
                echo.expires = clock + .18f; echo.root.SetActive(true); nextEchoAt = clock + .045f;
            }
        }

        private void UpdateSwordImpact()
        {
            if (!swordHitPending || clock < swordHitAt) return;
            swordHitPending = false;int hitCount=0;
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                Enemy enemy = enemies[i]; Vector2 offset = enemy.position - position;
                float reach = enableDesertJourney ? CurrentWeapon.range : 1.4f;
                bool inStrike=enableDesertJourney&&CurrentWeapon.type==WeaponType.Spear
                    ? DistanceToSegment(enemy.position,position,position+swingDirection*reach)<=enemy.radius+.14f
                    : Vector2.Dot(swingDirection,offset.normalized)>.25f;
                if (offset.magnitude <= reach + enemy.radius && inStrike &&
                    (!enableDesertJourney || !ProjectilePathBlocked(position, enemy.position, .02f)))
                { if(enableDesertJourney) ApplyElementDamage(enemy,CurrentMeleeDamage,CurrentWeapon.element); else DamageEnemy(enemy,swordDamage);if(enableDesertJourney&&CurrentWeapon.maxHits>0&&++hitCount>=CurrentWeapon.maxHits)break; }
            }
        }

        private void BeginDodge(Vector2 input)
        {
            ramHits.Clear();
            dashDirection = input.sqrMagnitude > 0 ? input.normalized : facing;
            dashUntil = clock + .17f;
            invulnerableUntil = Mathf.Max(invulnerableUntil, dashUntil + .07f);
            nextDash = clock + .8f;
            swordHitPending = false; swordUntil = 0; castPoseUntil = 0;
        }
    }
}
