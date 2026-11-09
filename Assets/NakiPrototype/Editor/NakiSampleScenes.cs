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
        [MenuItem("Tools/Naki Legend/Boss Practice/5 Thorn Mother")]
        public static void OpenThornMother(){OpenWoodBossAt(0);}
        [MenuItem("Tools/Naki Legend/Boss Practice/5 Spore Crown")]
        public static void OpenSporeCrown(){OpenWoodBossAt(1);}
        [MenuItem("Tools/Naki Legend/Boss Practice/5 Ancient Guardian")]
        public static void OpenAncientGuardian(){OpenWoodBossAt(2);}
        private static void OpenWoodBossAt(int boss){if(EditorApplication.isPlaying)return;OpenScene("Assets/Scenes/NakiWoodlandBossPractice.unity");var p=Object.FindObjectOfType<NakiPrototype>();if(p!=null){p.woodlandMode=true;p.fireChapterMode=p.iceSampleMode=false;p.thunderChapterPracticeBoss=boss;EditorUtility.SetDirty(p);}}
        [MenuItem("Tools/Naki Legend/Play/5 Woodland Adventure")]
        public static void OpenWood(){OpenScene("Assets/Scenes/NakiWoodlandChapter.unity");}
        [MenuItem("Tools/Naki Legend/Boss Practice/5 Woodland")]
        public static void OpenWoodBoss(){OpenScene("Assets/Scenes/NakiWoodlandBossPractice.unity");}
        [MenuItem("Tools/Naki Legend/Play/3 Ice Harbor Sample")]
        [MenuItem("Tools/Naki Legend/Play/3 Frost Tide Adventure")]
        public static void OpenIce(){OpenScene("Assets/Scenes/NakiIceSample.unity");}
        [MenuItem("Tools/Naki Legend/Boss Practice/3 Ice Beast")]
        public static void OpenIceBoss(){OpenScene("Assets/Scenes/NakiIceBossPractice.unity");}
        [MenuItem("Tools/Naki Legend/Boss Practice/3 Frost Mirror")]
        public static void OpenFrostMirror(){OpenIcePracticeAt(1);}
        [MenuItem("Tools/Naki Legend/Boss Practice/3 Tide Guardian")]
        public static void OpenTideGuardian(){OpenIcePracticeAt(2);}
        private static void OpenIcePracticeAt(int boss){if(EditorApplication.isPlaying)return;OpenScene("Assets/Scenes/NakiIceBossPractice.unity");var p=Object.FindObjectOfType<NakiPrototype>();if(p!=null){p.iceFullChapter=true;p.thunderChapterPracticeBoss=boss;EditorUtility.SetDirty(p);}}
        [MenuItem("Tools/Naki Legend/Play/4 Fire Islands Adventure")]
        public static void OpenFireCoast(){OpenScene("Assets/Scenes/NakiFireCoast.unity");}
        [MenuItem("Tools/Naki Legend/Play/4 Forge Workshop Test")]
        public static void OpenFireWorkshop(){OpenScene("Assets/Scenes/NakiFireWorkshop.unity");}
        [MenuItem("Tools/Naki Legend/Play/4 Fire Core Test")]
        public static void OpenFireCore(){OpenFireCoreAt(-1);}
        [MenuItem("Tools/Naki Legend/Boss Practice/4 Magma Tortoise")]
        public static void OpenMagmaTortoise(){OpenFireCoreAt(2);}
        private static void OpenFireCoreAt(int boss){if(EditorApplication.isPlaying)return;OpenScene("Assets/Scenes/NakiFireCoast.unity");var p=Object.FindObjectOfType<NakiPrototype>();if(p!=null){p.fireChapterMode=true;p.iceSampleMode=p.woodlandMode=false;p.thunderChapterStartFloor=2;p.thunderChapterPracticeBoss=boss;EditorUtility.SetDirty(p);}}
        [MenuItem("Tools/Naki Legend/Boss Practice/4 Forge Master")]
        public static void OpenForgeBoss(){OpenScene("Assets/Scenes/NakiForgeBossPractice.unity");}
        [MenuItem("Tools/Naki Legend/Boss Practice/4 Red Spine Lizard")]
        public static void OpenFireBoss(){OpenScene("Assets/Scenes/NakiFireBossPractice.unity");}
        public static void BuildFireScenes(){string[] names={"NakiFireCoast","NakiFireBossPractice","NakiFireWorkshop","NakiForgeBossPractice"};for(int i=0;i<names.Length;i++){var scene=EditorSceneManager.OpenScene(ChapterPath);var p=Object.FindObjectOfType<NakiPrototype>();p.name=names[i];p.fireChapterMode=true;p.iceSampleMode=p.woodlandMode=false;p.thunderChapterMode=true;p.showOpeningOnStart=false;p.thunderChapterPracticeBoss=i==1?0:i==3?1:-1;p.thunderChapterStartFloor=i>=2?1:0;EditorUtility.SetDirty(p);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene,"Assets/Scenes/"+names[i]+".unity");}}
        public static void BuildIceScenes()
        {
            foreach(bool practice in new[]{false,true}){
                var scene=EditorSceneManager.OpenScene(ChapterPath);var p=Object.FindObjectOfType<NakiPrototype>();
                p.name=practice?"Ice Beast Practice":"Ice Harbor Sample";p.iceSampleMode=true;p.iceFullChapter=true;p.woodlandMode=false;p.thunderChapterMode=true;
                p.showOpeningOnStart=false;p.thunderChapterPracticeBoss=practice?0:-1;p.thunderChapterStartFloor=0;
                EditorUtility.SetDirty(p);EditorSceneManager.MarkSceneDirty(scene);
                if(!EditorSceneManager.SaveScene(scene,practice?"Assets/Scenes/NakiIceBossPractice.unity":"Assets/Scenes/NakiIceSample.unity"))throw new System.InvalidOperationException("Could not save ice sample scene");
            }
        }
        [MenuItem("Tools/Naki Legend/World Route")]
        public static void ShowRoute(){EditorUtility.DisplayDialog("Naki 群岛航线","1 鸣沙洲（可玩） → 2 雷雨高原（可玩） → 3 霜潮群岛（霜潮群岛三层章节） → 4 赤炉群岛（黑砂海岸、工坊、地心三层与结尾可玩） → 5 青栖林海（已有章节试航） → 6 暮钟岛（归功外庭可玩，后两层待开放） → 本土。\n\nPlay 打开章节，Boss Practice 只练习。现有森林存档可以继续，不要求补打新岛。","知道了");}
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
