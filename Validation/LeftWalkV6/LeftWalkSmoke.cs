using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NakiLegend.Prototype;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NakiLeftWalkSmoke
{
    private const string Flag = "NakiLeftWalkSmoke.Running";
    private static readonly string Output = Path.GetFullPath(Path.Combine(Application.dataPath, "../Validation/LeftWalkV6/")) + "/";
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    private static int ticks;
    private static int checks;

    [InitializeOnLoadMethod] private static void Reload()
    {
        if (SessionState.GetBool(Flag, false)) EditorApplication.update += Update;
    }

    public static void Run()
    {
        Directory.CreateDirectory(Output);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        NakiPrototype prototype = new GameObject("Naki Pixel Animation Test").AddComponent<NakiPrototype>();
        prototype.enableDesertJourney = false;
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), "Assets/PixelSmoke.unity");
        SessionState.SetBool(Flag, true);
        EditorApplication.update -= Update; EditorApplication.update += Update;
        EditorApplication.isPlaying = true;
    }

    private static object Get(object obj, string name)
    {
        return obj.GetType().GetField(name, Private | BindingFlags.Public).GetValue(obj);
    }
    private static void Set(object obj, string name, object value)
    {
        obj.GetType().GetField(name, Private | BindingFlags.Public).SetValue(obj, value);
    }
    private static object Call(NakiPrototype p, string name, params object[] args)
    {
        return typeof(NakiPrototype).GetMethod(name, Private).Invoke(p, args);
    }
    private static void Check(bool value, string description)
    {
        if (!value) throw new Exception(description);
        checks++; Debug.Log("NAKI_POLISHED_PASS: " + description);
    }
    private static void Pose(NakiPrototype p, float time, Vector2 direction, Vector2 movement)
    {
        Set(p, "clock", time); Set(p, "facing", direction);
        Set(p, "swordUntil", 0f); Set(p, "castPoseUntil", 0f); Set(p, "dashUntil", 0f); Set(p, "pixelHurtUntil", 0f);
        Call(p, "UpdatePixelHero", movement);
    }

    private static void Update()
    {
        if (!EditorApplication.isPlaying || EditorApplication.isCompiling || ++ticks < 20) return;
        NakiPrototype p = UnityEngine.Object.FindObjectOfType<NakiPrototype>();
        if (p == null || Get(p, "world") == null) return;
        EditorApplication.update -= Update;
        try
        {
            p.enabled = false;
            Set(p, "position", Vector2.zero);
            Check((bool)Get(p, "pixelHeroReady") && (bool)Get(p, "polishedHeroReady"), "Polished animation is enabled by default");
            Texture2D atlas = (Texture2D)Get(p, "heroAtlas");
            Check(atlas.filterMode == FilterMode.Point, "Atlas uses crisp point sampling");
            File.WriteAllBytes(Output + "hero-atlas.png", atlas.EncodeToPNG());
            Pose(p, 1f, Vector2.right, Vector2.right);
            var body = (SpriteRenderer)Get(p, "heroBody");
            Sprite walkA = body.sprite;
            Pose(p, 1f, Vector2.left, Vector2.left);
            Check(body.sprite == walkA && body.flipX, "Left gait mirrors the approved right gait");
            Pose(p, 1f, Vector2.right, Vector2.right);
            Check(!body.flipX, "Facing right clears the mirror flag");
            var walks = (System.Array)((System.Collections.IDictionary)Get(p, "heroFrames"))[Get(p, "currentHeroAction")];
            Check(((System.Array)walks.GetValue(1)).Length == 6, "Each direction has six dedicated walking poses");
            Pose(p, 1.26f, Vector2.right, Vector2.right);
            Sprite walkB = body.sprite;
            for (int step = 0; step < 6; step++)
            {
                float t = 1f + (step + .1f) / 9.6f;
                Pose(p, t, Vector2.right, Vector2.right); Sprite rightStep = body.sprite;
                Pose(p, t, Vector2.left, Vector2.left);
                Check(body.sprite == rightStep && body.flipX, "Left and right share leg pose phase " + step);
            }
            Pose(p, 1.26f, Vector2.right, Vector2.right);
            Check(walkA != walkB, "Walking advances pose frames");
            Check(walkA.texture.name.Contains("hero-walk") && walkA != walkB, "Movement uses generated action sprites");
            Check(walkA.rect.width == 256 && walkB.rect.width == 256, "High detail frames are loaded at padded 256px cells");
            Pose(p, 1, Vector2.up, Vector2.zero); Sprite back = body.sprite;
            Pose(p, 1, Vector2.down, Vector2.zero); Sprite front = body.sprite;
            Check(back != front, "Up/down directions have distinct face and back art");
            Check(front.pivot == new Vector2(128,24) && front.pixelsPerUnit == 130, "All sprites use a shared ground anchor and scale");
            Set(p, "position", new Vector2(.013f, .027f)); Call(p, "UpdatePixelHero", Vector2.zero);
            Check(Vector2.Distance(((GameObject)Get(p, "hero")).transform.position, new Vector2(.013f,.027f)) < .0001f, "Polished movement preserves subpixel position without coarse snapping");
            Set(p, "position", Vector2.zero);
            Check(!((GameObject)Get(p, "sword")).activeSelf, "Sword is included in the polished body sprite without a duplicate overlay");

            IList enemies = (IList)Get(p, "enemies");
            object target = enemies[0]; Set(target, "position", new Vector2(1, 0));
            float initialHp = (float)Get(target, "hp");
            Set(p, "clock", 10f); Set(p, "facing", Vector2.right);
            Call(p, "Swing");
            Check((float)Get(target, "hp") == initialHp, "Sword windup does not damage immediately");
            Set(p, "clock", 10.05f); Call(p, "UpdateSwordImpact"); Call(p, "UpdatePixelHero", Vector2.zero);
            Vector3 gripA = ((GameObject)Get(p, "sword")).transform.position;
            Check((float)Get(target, "hp") == initialHp, "No damage before the contact frame");
            Set(p, "clock", 10.12f); Call(p, "UpdateSwordImpact"); Call(p, "UpdatePixelHero", Vector2.zero);
            Vector3 gripB = ((GameObject)Get(p, "sword")).transform.position;
            Check(Mathf.Abs((float)Get(target, "hp") - (initialHp - p.swordDamage)) < .01f, "Sword damage lands at the contact frame");
            Check(body.sprite.texture.name.Contains("hero-movement") && body.sprite != walkA, "Sword strike changes the full-body sprite");
            Call(p, "UpdateSwordImpact");
            Check(Mathf.Abs((float)Get(target, "hp") - (initialHp - p.swordDamage)) < .01f, "One swing hits each target only once");

            Set(p, "clock", 11f); Call(p, "Swing"); Call(p, "BeginDodge", Vector2.right);
            Set(p, "clock", 11.12f); Call(p, "UpdateSwordImpact"); Call(p, "UpdatePixelHero", Vector2.zero);
            Check(!(bool)Get(p, "swordHitPending"), "Dodge cancels an unfinished sword swing");
            Check(Get(p, "currentHeroAction").ToString() == "Dodge", "Dodge uses a crouched pose");
            float hp = (float)Get(p, "health"); Call(p, "Hurt");
            Check((float)Get(p, "health") == hp, "Dodge invulnerability still works");
            Pose(p, 12, Vector2.right, Vector2.zero); Set(p, "invulnerableUntil", 0f); Call(p, "Hurt"); Call(p, "UpdatePixelHero", Vector2.zero);
            Check(Get(p, "currentHeroAction").ToString() == "Hurt", "Taking damage displays recoil frames");
            Pose(p, 13, Vector2.right, Vector2.zero);
            float energy = (float)Get(p, "energy"); int shots = ((IList)Get(p, "shots")).Count;
            Call(p, "Cast"); Call(p, "UpdatePixelHero", Vector2.zero);
            Check(Get(p, "currentHeroAction").ToString() == "Cast", "Spell input displays casting poses");
            Check(((IList)Get(p, "shots")).Count == shots + 3 && Mathf.Abs((float)Get(p, "energy") - (energy - 25)) < .01f && p.spellDamage == 15,
                "Spell projectile count, damage and energy cost preserved");
            Pose(p, 14.12f, Vector2.one.normalized, Vector2.zero);
            Set(p, "swingStarted", 14f); Set(p, "swordUntil", 14.32f); Set(p, "swingDirection", Vector2.one.normalized);
            Call(p, "UpdatePixelHero", Vector2.zero);
            float actualAngle = ((GameObject)Get(p, "sword")).transform.eulerAngles.z;
            Check(Mathf.Abs(Mathf.DeltaAngle(actualAngle, 10)) < .1f, "Weapon follows the exact diagonal aim while the body uses four directions");
            Call(p, "ResetRoom");
            Check(((IList)Get(p, "enemies")).Count == 2 && ((IList)Get(p, "dashEchoes")).Count == 4, "Restart restores the room without accumulating afterimages");
            Set(p, "health", 0f); Call(p, "UpdatePixelHero", Vector2.zero);
            Check(Get(p, "currentHeroAction").ToString() == "Defeated" && !((GameObject)Get(p, "sword")).activeSelf, "Defeat displays a collapsed pose and hides the weapon");
            Call(p, "ResetRoom");
            Camera camera = (Camera)Get(p, "view");
            camera.aspect = 16f / 9; camera.orthographicSize = 6.8f;
            Capture(camera, Output + "unity-left-walk-v6.png", 1600, 900);

            // Export a real Unity animation sequence at 24 fps, with the character walking in place for comparison.
            foreach (object enemy in (IList)Get(p, "enemies")) ((GameObject)Get(enemy, "root")).SetActive(false);
            Set(p, "position", Vector2.zero);
            camera.transform.position = new Vector3(0, .45f, -10); camera.orthographicSize = 1.35f;
            Directory.CreateDirectory(Output + "frames");
            int index = 0;
            foreach (Vector2 direction in new[] { Vector2.right, Vector2.down, Vector2.up, Vector2.left })
                for (int f = 0; f < 24; f++)
                {
                    Pose(p, 20 + f / 24f, direction, direction);
                    Capture(camera, Output + "frames/frame-" + (index++).ToString("D3") + ".png", 432, 432);
                }
            foreach (string action in new[] { "Attack", "Cast", "Dodge", "Hurt" })
                for (int f = 0; f < 24; f++)
                {
                    float phase = (f % 12) / 12f;
                    Pose(p, 30, Vector2.right, Vector2.zero);
                    if (action == "Attack") { Set(p, "swingStarted", 30 - phase * .32f); Set(p, "swordUntil", 30 + (1 - phase) * .32f); Set(p, "swingDirection", Vector2.right); }
                    if (action == "Cast") { Set(p, "castPoseUntil", 30 + (1 - phase) * .32f); Set(p, "castDirection", Vector2.right); }
                    if (action == "Dodge") { Set(p, "dashUntil", 30 + (1 - phase) * .17f); Set(p, "dashDirection", Vector2.right); }
                    if (action == "Hurt") Set(p, "pixelHurtUntil", 30 + (1 - phase) * .2f);
                    Call(p, "UpdatePixelHero", Vector2.zero);
                    Capture(camera, Output + "frames/frame-" + (index++).ToString("D3") + ".png", 432, 432);
                }
            // Restore state and verify the original encounter can still reach victory.
            Call(p, "ResetRoom"); enemies = (IList)Get(p, "enemies");
            object first = enemies[0], second = enemies[1];
            Call(p, "DamageEnemy", first, 999f); Call(p, "DamageEnemy", second, 999f);
            Call(p, "Update");
            Check((bool)Get(p, "bossSpawned") && enemies.Count == 1, "Boss progression is intact");
            Call(p, "DamageEnemy", enemies[0], 999f); Call(p, "Update");
            Check((bool)Get(p, "won") && Get(p, "currentHeroAction").ToString() == "Idle", "Victory settles the hero into idle");
            string pipeline = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline.GetType().Name;
            File.WriteAllText(Output + "smoke-result.txt", "PASS: " + checks + " checks; Unity 2022.3 / " + pipeline + ".\nLeft walking mirrors every right-walking leg pose, mirror resets on turning, continuous movement and combat progression preserved.\n192 actual Unity render frames exported; preview stretches short actions to show their poses.\n");
            Finish(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error); File.WriteAllText(Output + "smoke-result.txt", "FAIL: " + error); Finish(1);
        }
    }

    private static int RegionHash(Sprite sprite, int yStart, int rows)
    {
        Color[] pixels = sprite.texture.GetPixels((int)sprite.rect.x, (int)sprite.rect.y + yStart, 40, rows);
        int hash = 17; unchecked { foreach (Color pixel in pixels) hash = hash * 31 + ((Color32)pixel).GetHashCode(); } return hash;
    }
    private static int VisibleHeight(Sprite sprite)
    {
        Color[] pixels = sprite.texture.GetPixels((int)sprite.rect.x, (int)sprite.rect.y, 40, 40);
        int min = 40, max = 0;
        for (int y = 0; y < 40; y++) for (int x = 0; x < 40; x++) if (pixels[y * 40 + x].a > .5f) { min = Mathf.Min(min, y); max = Mathf.Max(max, y); }
        return max - min + 1;
    }
    private static void Capture(Camera camera, string path, int width, int height)
    {
        RenderTexture previous = RenderTexture.active, oldTarget = camera.targetTexture;
        var target = new RenderTexture(width, height, 24); var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
        try {
            camera.targetTexture = target; camera.aspect = width / (float)height; camera.Render(); RenderTexture.active = target;
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0); texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG());
        } finally {
            camera.targetTexture = oldTarget; RenderTexture.active = previous; target.Release();
            UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(texture);
        }
    }
    private static void Finish(int code)
    {
        SessionState.SetBool(Flag, false); EditorApplication.update -= Update; EditorApplication.Exit(code);
    }
}
