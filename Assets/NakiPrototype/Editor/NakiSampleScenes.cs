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
        [MenuItem("Tools/Naki Legend/Play/3 Thunder Boss Practice")]
        public static void OpenChapterBoss(){OpenScene(ChapterBossPath);}
        [MenuItem("Tools/Naki Legend/Play/4 Desert Boss Practice")]
        public static void OpenDesertBoss(){OpenScene("Assets/Scenes/NakiBossPractice.unity");}
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
