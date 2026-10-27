using UnityEngine;

namespace NakiLegend.Prototype
{
    public sealed partial class NakiPrototype
    {
        [Header("Generated 2D artwork")]
        public bool useGeneratedArtwork = true;

        private bool illustratedArtReady;
        private Sprite roomArtwork, pillarArtwork, doorArtwork, swordArtwork, softDisc;
        private GameObject heroShadow, weaponTrail;
        private Vector2 swingDirection = Vector2.right, castDirection = Vector2.right;
        private float swingStarted, castPoseUntil;
        private GUIStyle headingStyle, labelStyle, smallLabelStyle;

        private static int DepthAt(float footY) { return 200 - Mathf.RoundToInt(footY * 40); }

        private Sprite Artwork(string name, Rect region, Vector2 pivot, float pixelsPerUnit)
        {
            // Textures live in Resources so an already-attached prototype needs no manual assignments.
            Texture2D texture = Resources.Load<Texture2D>("NakiArt/" + name);
            if (texture == null) return null;
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            if (region.width <= 0) region = new Rect(0, 0, texture.width, texture.height);
            if (region.xMax > texture.width || region.yMax > texture.height)
            {
                Debug.LogError("Naki artwork import size is too small: " + name + ". Disable resizing in texture import settings.");
                return null;
            }
            Sprite sprite = Sprite.Create(texture, region, pivot, pixelsPerUnit, 0, SpriteMeshType.FullRect);
            sprite.name = name;
            ownedAssets.Add(sprite);
            return sprite;
        }

        private void LoadIllustratedAssets()
        {
            if (!useGeneratedArtwork) return;
            roomArtwork = Artwork("desert-room-v2", Rect.zero, Vector2.one * .5f, 76);
            // Rectangles are sprite slicing metadata from alpha bounds; source PNGs remain untouched.
            Sprite newHero = Artwork("hero-idle-v2", new Rect(193, 66, 654, 1414), new Vector2(.5f, .035f), 857);
            pillarArtwork = Artwork("sandstone-pillar-v2", new Rect(241, 64, 541, 1420), new Vector2(.5f, .035f), 631);
            doorArtwork = Artwork("wind-door-v2", new Rect(64, 86, 1017, 1187), new Vector2(.5f, .025f), 527);
            swordArtwork = Artwork("rune-sword-v2", new Rect(80, 158, 1823, 486), new Vector2(.13f, .5f), 1458);
            illustratedArtReady = roomArtwork != null && newHero != null && pillarArtwork != null && doorArtwork != null && swordArtwork != null;
            if (!illustratedArtReady)
            {
                Debug.LogWarning("Naki: generated artwork is incomplete. Using the original prototype visuals.");
                return;
            }
            heroSprite = newHero;
            softDisc = CreateSoftDisc();
            disc = softDisc;
        }

        private Sprite CreateSoftDisc()
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float radius = new Vector2((x + .5f) / size * 2 - 1, (y + .5f) / size * 2 - 1).magnitude;
                    float alpha = 1 - Mathf.SmoothStep(.35f, 1, radius);
                    texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
                }
            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * .5f, size);
            ownedAssets.Add(sprite); ownedAssets.Add(texture);
            return sprite;
        }

        private void BuildIllustratedRoom()
        {
            Draw("Generated sandstone room", roomArtwork, new Vector2(0, .55f), Vector2.one, Color.white, -1000);
            Draw("Wind-sealed doorway", doorArtwork, new Vector2(0, 4.25f), Vector2.one, Color.white, DepthAt(4.25f));
            foreach (Vector2 at in new[] { new Vector2(-6, 1.8f), new Vector2(6, 1.8f), new Vector2(-6, -2.3f), new Vector2(6, -2.3f) })
            {
                // Only the footing blocks movement. The column above it can occlude actors behind it.
                obstacles.Add(new Rect(at.x - .42f, at.y - .24f, .84f, .48f));
                Bounds body = pillarArtwork.bounds;
                projectileObstacles.Add(new Rect(at.x + body.min.x, at.y + body.min.y, body.size.x, body.size.y));
                Draw("Pillar ground shadow", softDisc, at + new Vector2(.12f, -.03f), new Vector2(1.5f, .5f), new Color(.16f, .1f, .06f, .3f), -800);
                Draw("Independent sandstone pillar", pillarArtwork, at, Vector2.one, Color.white, DepthAt(at.y));
            }
        }

        private void SetupIllustratedHero()
        {
            swingStarted = -1; castPoseUntil = 0;
            if (pixelHeroReady) { SetupPixelHero(); return; }
            if (!illustratedArtReady) return;
            heroShadow = Draw("Hero ground shadow", softDisc, position, new Vector2(.8f, .27f), new Color(.08f, .06f, .04f, .35f), -799);
            sword.GetComponent<SpriteRenderer>().sprite = swordArtwork;
            sword.GetComponent<SpriteRenderer>().color = Color.white;
            sword.transform.localScale = Vector3.one;
            weaponTrail = Draw("Sword swing glow", softDisc, position, new Vector2(1.35f, .18f), new Color(1, .83f, .4f, .55f), 500);
            weaponTrail.SetActive(false);
            UpdateIllustratedHero(Vector2.zero);
        }

        private void UpdateIllustratedHero(Vector2 movement)
        {
            if (pixelHeroReady) { UpdatePixelHero(movement); return; }
            if (!illustratedArtReady) return;
            float bob = movement.sqrMagnitude > 0 ? Mathf.Sin(clock * 17) * .035f : Mathf.Sin(clock * 2.5f) * .009f;
            hero.transform.position = position + Vector2.up * bob;
            heroBody.sortingOrder = DepthAt(position.y);
            heroShadow.transform.position = position;
            bool swinging = clock < swordUntil;
            Vector2 aim = swinging ? swingDirection : clock < castPoseUntil ? castDirection : facing;
            float angle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
            if (swinging) angle += Mathf.Lerp(-65, 65, Mathf.Clamp01((clock - swingStarted) / .13f));
            else if (clock >= castPoseUntil) angle -= 30;
            sword.SetActive(true);
            sword.transform.rotation = Quaternion.Euler(0, 0, angle);
            sword.transform.position = position + Vector2.up * .7f + aim * .22f;
            sword.GetComponent<SpriteRenderer>().sortingOrder = heroBody.sortingOrder + (aim.y > .6f ? -1 : 1);
            weaponTrail.SetActive(swinging);
            if (swinging)
            {
                weaponTrail.transform.position = (Vector2)sword.transform.position + (Vector2)sword.transform.right * .6f;
                weaponTrail.transform.rotation = sword.transform.rotation;
                weaponTrail.GetComponent<SpriteRenderer>().sortingOrder = heroBody.sortingOrder + 2;
            }
        }

        private static void HudRect(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private void DrawPrototypeHud()
        {
            if (world == null) return;
            if (headingStyle == null)
            {
                headingStyle = NewLabel(22, C("ffedbc"), FontStyle.Bold);
                labelStyle = NewLabel(16, C("f4e8d4"), FontStyle.Normal);
                smallLabelStyle = NewLabel(13, C("d4c6ae"), FontStyle.Normal);
            }
            Matrix4x4 previousMatrix = GUI.matrix;
            Color previousColor = GUI.color;
            int previousDepth = GUI.depth;
            float scale = Mathf.Max(.3f, Mathf.Min(Screen.width / 1100f, Screen.height / 680f));
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
            GUI.color = Color.white; GUI.depth = -100;
            float width = Screen.width / scale, height = Screen.height / scale;
            Color panel = new Color(.08f, .1f, .14f, .94f);
            HudRect(new Rect(16, 16, 310, 140), panel);
            HudRect(new Rect(16, 16, 3, 140), C("d6ad64"));
            GUI.Label(new Rect(30, 24, 285, 32), "NAKI LEGEND", headingStyle);
            GUI.Label(new Rect(30, 59, 90, 25), "HP " + health + "/6", labelStyle);
            for (int i = 0; i < 6; i++)
                HudRect(new Rect(120 + i * 28, 65, 21, 12), i < health ? C("e47b65") : C("514548"));
            GUI.Label(new Rect(30, 86, 280, 24), "LIGHT  " + Mathf.FloorToInt(energy) + " / 100", smallLabelStyle);
            HudRect(new Rect(30, 112, 278, 7), C("403b37"));
            HudRect(new Rect(30, 112, 278 * energy / 100, 7), C("edc578"));
            GUI.Label(new Rect(30, 125, 280, 26), "DODGE  " + (clock >= nextDash ? "READY" : (nextDash - clock).ToString("0.0") + "s"), smallLabelStyle);
            foreach (Enemy enemy in enemies)
            {
                if (enemy.boss)
                {
                    float barWidth = Mathf.Min(460, width - 370), x = width - barWidth - 16;
                    HudRect(new Rect(x, 16, barWidth, 70), panel);
                    GUI.Label(new Rect(x + 14, 24, barWidth - 28, 26), "WIND SCORPION", labelStyle);
                    HudRect(new Rect(x + 14, 59, barWidth - 28, 9), C("403b37"));
                    HudRect(new Rect(x + 14, 59, (barWidth - 28) * Mathf.Clamp01(enemy.hp / enemy.maximum), 9), C("d27e60"));
                }
                else
                {
                    Vector3 at = view.WorldToScreenPoint((Vector3)enemy.position + Vector3.up * .6f);
                    Rect bar = new Rect(at.x / scale - 23, (Screen.height - at.y) / scale, 46, 5);
                    HudRect(bar, C("403b37"));
                    bar.width *= Mathf.Clamp01(enemy.hp / enemy.maximum); HudRect(bar, C("d27e60"));
                }
            }
            HudRect(new Rect(16, height - 75, width - 32, 59), panel);
            GUI.Label(new Rect(30, height - 69, width - 60, 26), "WASD  Move     J  Sword     K  Dodge     L  Light Orbs     ESC  Pause     R  Restart", labelStyle);
            GUI.Label(new Rect(30, height - 43, width - 60, 22), pixelHeroReady ? "NAKI LEGEND  /  Pixel hero action test" : "NAKI LEGEND  /  Combat test", smallLabelStyle);
            string message = health <= 0 ? "DEFEATED  -  R to retry" : won ? "ROOM CLEARED!  -  R to replay" : paused ? "PAUSED  -  ESC to resume" : "";
            if (message.Length > 0)
            {
                HudRect(new Rect(width / 2 - 255, height / 2 - 44, 510, 88), panel);
                GUI.Label(new Rect(width / 2 - 233, height / 2 - 18, 480, 42), message, headingStyle);
            }
            GUI.matrix = previousMatrix; GUI.color = previousColor; GUI.depth = previousDepth;
        }

        private static GUIStyle NewLabel(int fontSize, Color color, FontStyle fontStyle)
        {
            var style = new GUIStyle(GUI.skin.label) {
                fontSize = fontSize, fontStyle = fontStyle, alignment = TextAnchor.UpperLeft,
                wordWrap = false, clipping = TextClipping.Clip, fixedHeight = 0,
                contentOffset = Vector2.zero, padding = new RectOffset(0, 0, 1, 0)
            };
            style.normal.textColor = color;
            return style;
        }
    }
}
