using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NakiLegend.Prototype.Editor
{
    public static class NakiSampleScenes
    {
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
