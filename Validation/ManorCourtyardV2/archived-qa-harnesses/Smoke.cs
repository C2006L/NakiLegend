using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NakiLegend.Prototype;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NakiVisualSmoke
{
    private const string Flag = "NakiVisualSmoke.Running";
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    private static int ticks;

    [InitializeOnLoadMethod]
    private static void Reload()
    {
        if (SessionState.GetBool(Flag, false)) EditorApplication.update += Update;
    }

    public static void Run()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        new GameObject("Naki Art Smoke").AddComponent<NakiPrototype>();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), "Assets/Smoke.unity");
        SessionState.SetBool(Flag, true);
        SessionState.SetInt("NakiVisualSmoke.Phase", 0);
        EditorApplication.update -= Update;
        EditorApplication.update += Update;
        EditorApplication.isPlaying = true;
    }

    private static object Get(NakiPrototype p, string name) { return typeof(NakiPrototype).GetField(name, Private).GetValue(p); }
    private static object Call(NakiPrototype p, string name, params object[] args) { return typeof(NakiPrototype).GetMethod(name, Private).Invoke(p, args); }
    private static void Check(bool condition, string message) { if (!condition) throw new Exception(message); Debug.Log("NAKI_CHECK_PASS: " + message); }

    private static void Update()
    {
        if (!EditorApplication.isPlaying || EditorApplication.isCompiling || ++ticks < 20) return;
        try
        {
            NakiPrototype p = UnityEngine.Object.FindObjectOfType<NakiPrototype>();
            if (p == null || Get(p, "world") == null) return;
            int phase = SessionState.GetInt("NakiVisualSmoke.Phase", 0);
            if (phase == 0)
            {
                Check((bool)Get(p, "illustratedArtReady"), "All five generated PNG assets loaded");
                Check(((Sprite)Get(p, "heroSprite")).name == "hero-idle-v2", "Hero uses generated image");
                Check(((Sprite)Get(p, "roomArtwork")).texture.width == 1672, "NPOT background kept original resolution");
                Check(((Sprite)Get(p, "swordArtwork")).texture.width == 1983, "Sword kept original resolution");
                Vector2 blocked = (Vector2)Call(p, "Move", new Vector2(-6, .8f), new Vector2(0, .6f), .3f);
                Check(Mathf.Abs(blocked.y - .8f) < .01f, "Pillar footing blocks movement");
                Vector2 clamped = (Vector2)Call(p, "Move", Vector2.zero, new Vector2(100, 100), .3f);
                Check(clamped.x <= 9.11f && clamped.y <= 3.51f, "Movement remains inside painted floor");
                int before = ((IList)Get(p, "shots")).Count;
                float energy = (float)Get(p, "energy");
                Call(p, "Cast");
                Check(((IList)Get(p, "shots")).Count == before + 3, "Spell still creates three homing orbs");
                Check(Mathf.Abs((float)Get(p, "energy") - (energy - 25)) < .01f && p.spellDamage == 15, "Original spell cost and damage preserved");
                Call(p, "Swing");
                Call(p, "UpdateIllustratedHero", Vector2.right);
                GameObject weapon = (GameObject)Get(p, "sword");
                Check(weapon.activeSelf && weapon.GetComponent<SpriteRenderer>().sprite.name == "rune-sword-v2", "Sword image is animated separately");
                // Actual runtime render; no art is pasted into this verification image.
                Capture((Camera)Get(p, "view"), "D:/NakiLegend/Validation/DesertV2/unity-render-v2.png");
                ScreenCapture.CaptureScreenshot("D:/NakiLegend/Validation/DesertV2/unity-game-v2.png");
                SessionState.SetInt("NakiVisualSmoke.Phase", 1);
                ticks = 0;
            }
            else if (phase == 1)
            {
                Call(p, "ResetRoom");
                Check(((IList)Get(p, "enemies")).Count == 2, "Restart restores two enemies");
                Check(((IList)Get(p, "shots")).Count == 0, "Restart removes old projectiles");
                IList enemies = (IList)Get(p, "enemies");
                object a = enemies[0], b = enemies[1];
                Call(p, "DamageEnemy", a, 999f); Call(p, "DamageEnemy", b, 999f);
                SessionState.SetInt("NakiVisualSmoke.Phase", 2);
                ticks = 0;
            }
            else
            {
                Check((bool)Get(p, "bossSpawned"), "Boss spawns after clearing small enemies");
                IList enemies = (IList)Get(p, "enemies");
                Check(enemies.Count == 1, "One boss remains");
                Call(p, "DamageEnemy", enemies[0], 999f);
                Check((bool)Get(p, "won"), "Victory still triggers after boss defeat");
                string pipeline = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null ? UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline.GetType().Name : "Built-in";
                File.WriteAllText("D:/NakiLegend/Validation/DesertV2/smoke-result.txt", "PASS: PNG loading, original dimensions, floor bounds, pillar collision, spell resources, weapon image, restart, boss progression and victory.\nRenderer: Unity 2022.3 / " + pipeline + ". Isolated validation project. Live input feel and user window layout still need user review.\n");
                Finish(0);
            }
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            File.WriteAllText("D:/NakiLegend/Validation/DesertV2/smoke-result.txt", "FAIL: " + error);
            Finish(1);
        }
    }

    private static void Capture(Camera camera, string path)
    {
        RenderTexture previousActive = RenderTexture.active;
        RenderTexture previousTarget = camera.targetTexture;
        var target = new RenderTexture(1600, 900, 24);
        var pixels = new Texture2D(1600, 900, TextureFormat.RGB24, false);
        try
        {
            camera.targetTexture = target; camera.aspect = 1600f / 900;
            camera.Render(); RenderTexture.active = target;
            pixels.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0); pixels.Apply();
            File.WriteAllBytes(path, pixels.EncodeToPNG());
        }
        finally
        {
            camera.targetTexture = previousTarget; RenderTexture.active = previousActive;
            target.Release(); UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(pixels);
        }
    }

    private static void Finish(int code)
    {
        SessionState.SetBool(Flag, false);
        EditorApplication.update -= Update;
        EditorApplication.Exit(code);
    }
}
