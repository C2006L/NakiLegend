using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NakiLegend.Prototype.Editor
{
    public static class NakiSampleScenes
    {
        public const string ChapterPath="Assets/Scenes/NakiThunderChapter.unity";
        public const string ChapterBossPath="Assets/Scenes/NakiThunderBossPractice.unity";
        [MenuItem("Tools/Naki Legend/Play/1 Desert Adventure")]
        public static void OpenDesert(){OpenScene("Assets/Scenes/NakiTest.unity");}
        [MenuItem("Tools/Naki Legend/Play/2 Thunder Plateau Adventure")]
        public static void OpenChapter(){OpenScene(ChapterPath);}
        [MenuItem("Tools/Naki Legend/Boss Practice/2 Thunder Plateau")]
        public static void OpenChapterBoss(){OpenScene(ChapterBossPath);}
        [MenuItem("Tools/Naki Legend/Boss Practice/1 Desert")]
        public static void OpenDesertBoss(){OpenScene("Assets/Scenes/NakiBossPractice.unity");}
        [MenuItem("Tools/Naki Legend/Play/5 Woodland Adventure")]
        public static void OpenWood(){OpenScene("Assets/Scenes/NakiWoodlandChapter.unity");}
        [MenuItem("Tools/Naki Legend/Boss Practice/5 Woodland")]
        public static void OpenWoodBoss(){OpenScene("Assets/Scenes/NakiWoodlandBossPractice.unity");}
        [MenuItem("Tools/Naki Legend/Play/3 Ice Harbor Sample")]
        public static void OpenIce(){OpenScene("Assets/Scenes/NakiIceSample.unity");}
        [MenuItem("Tools/Naki Legend/Boss Practice/3 Ice Beast")]
        public static void OpenIceBoss(){OpenScene("Assets/Scenes/NakiIceBossPractice.unity");}
        public static void BuildIceScenes()
        {
            foreach(bool practice in new[]{false,true}){
                var scene=EditorSceneManager.OpenScene(ChapterPath);var p=Object.FindObjectOfType<NakiPrototype>();
                p.name=practice?"Ice Beast Practice":"Ice Harbor Sample";p.iceSampleMode=true;p.woodlandMode=false;p.thunderChapterMode=true;
                p.showOpeningOnStart=false;p.thunderChapterPracticeBoss=practice?0:-1;p.thunderChapterStartFloor=0;
                EditorUtility.SetDirty(p);EditorSceneManager.MarkSceneDirty(scene);
                if(!EditorSceneManager.SaveScene(scene,practice?"Assets/Scenes/NakiIceBossPractice.unity":"Assets/Scenes/NakiIceSample.unity"))throw new System.InvalidOperationException("Could not save ice sample scene");
            }
        }
        [MenuItem("Tools/Naki Legend/World Route")]
        public static void ShowRoute(){EditorUtility.DisplayDialog("Naki 群岛航线","1 沙漠岛（可玩） → 2 雷雨高原（可玩） → 3 冰山岛（冰港三房样板） → 4 烈焰岛（待制作） → 5 青栖林海（已有章节试航） → 6 大骑士的府邸（待制作） → 本土。\n\nPlay 打开章节，Boss Practice 只练习。现有森林存档可以继续，不要求补打新岛。","知道了");}
        public static void BuildWoodScenes(){foreach(bool practice in new[]{false,true}){var scene=EditorSceneManager.OpenScene(ChapterPath);var p=Object.FindObjectOfType<NakiPrototype>();p.woodlandMode=true;p.thunderChapterMode=true;p.showOpeningOnStart=false;p.thunderChapterPracticeBoss=practice?2:-1;EditorUtility.SetDirty(p);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene,practice?"Assets/Scenes/NakiWoodlandBossPractice.unity":"Assets/Scenes/NakiWoodlandChapter.unity");}}
        private static void OpenScene(string path)
        {
            if(EditorApplication.isPlaying){Debug.LogWarning("请先停止 Play，再切换测试场景。");return;}
            if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())EditorSceneManager.OpenScene(path);
        }
        public static void BuildThunderChapterScenes()
        {
            foreach(bool practice in new[]{false,true})
            {
                var scene=EditorSceneManager.OpenScene("Assets/Scenes/NakiTest.unity");
                var p=Object.FindObjectOfType<NakiPrototype>();p.name=practice?"Thunder Boss Practice":"Thunder Plateau Adventure";
                p.thunderChapterMode=true;p.thunderPrototypeMode=p.gatehouseSampleMode=p.bossPracticeMode=p.lightningTrialMode=false;
                p.enableDesertJourney=true;p.showOpeningOnStart=false;p.journeySeed=16001;
                p.thunderChapterStartFloor=0;p.thunderChapterPracticeBoss=practice?2:-1;p.thunderPreset=NakiPrototype.ThunderPreset.Light;
                EditorUtility.SetDirty(p);EditorSceneManager.MarkSceneDirty(scene);
                if(!EditorSceneManager.SaveScene(scene,practice?ChapterBossPath:ChapterPath))throw new System.InvalidOperationException("Could not save thunder chapter scene");
            }
        }
        public const string ThunderPath="Assets/Scenes/NakiThunderPrototype.unity";
        [MenuItem("Tools/Naki Legend/Open Thunder Plateau Prototype")]
        public static void OpenThunder()
        {
            if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())EditorSceneManager.OpenScene(ThunderPath);
        }
        public static void BuildThunderScene()
        {
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/NakiTest.unity");
            var p=Object.FindObjectOfType<NakiPrototype>();p.name="Thunder Plateau Two Room Prototype";
            p.thunderPrototypeMode=true;p.enableDesertJourney=true;p.gatehouseSampleMode=p.bossPracticeMode=p.lightningTrialMode=false;
            p.showOpeningOnStart=false;p.journeySeed=15001;p.thunderStartRoom=0;p.thunderPreset=NakiPrototype.ThunderPreset.Light;
            EditorUtility.SetDirty(p);EditorSceneManager.MarkSceneDirty(scene);
            if(!EditorSceneManager.SaveScene(scene,ThunderPath))throw new System.InvalidOperationException("Could not save thunder scene");
        }
        public const string GatehousePath="Assets/Scenes/NakiGatehouseSample.unity";
        [MenuItem("Tools/Naki Legend/Open Old Canal Gatehouse")]
        public static void OpenGatehouse()
        {
            if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())EditorSceneManager.OpenScene(GatehousePath);
        }
        // Author through Unity's serializer so scene flags survive an editor round trip.
        public static void BuildGatehouseScene()
        {
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/NakiTest.unity");
            var p=Object.FindObjectOfType<NakiPrototype>();
            p.name="Old Canal Gatehouse";p.gatehouseSampleMode=true;p.enableDesertJourney=true;
            p.bossPracticeMode=false;p.lightningTrialMode=false;p.showOpeningOnStart=false;p.journeySeed=13001;
            EditorUtility.SetDirty(p);EditorSceneManager.MarkSceneDirty(scene);
            if(!EditorSceneManager.SaveScene(scene,GatehousePath))throw new System.InvalidOperationException("Could not save gatehouse scene");
        }
        public static void BuildPracticeScenes()
        {
            foreach(bool lightning in new[]{false,true})
            {
                var scene=EditorSceneManager.OpenScene("Assets/Scenes/NakiTest.unity");
                var p=Object.FindObjectOfType<NakiPrototype>();p.gatehouseSampleMode=false;p.enableDesertJourney=true;
                p.bossPracticeMode=true;p.lightningTrialMode=lightning;p.showOpeningOnStart=false;
                EditorUtility.SetDirty(p);EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene,lightning?"Assets/Scenes/NakiElementTrial.unity":"Assets/Scenes/NakiBossPractice.unity");
            }
        }
    }
}
