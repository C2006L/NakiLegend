using UnityEditor;
using UnityEngine;

namespace NakiLegend.Prototype.Editor
{
    public sealed class NakiArtImportSettings : AssetPostprocessor
    {
        private const string ArtFolder = "Assets/NakiPrototype/Resources/NakiArt";

        public override uint GetVersion() { return 4; }

        private static bool IsArtwork(string path)
        {
            return path.Replace('\\', '/').StartsWith(ArtFolder + "/", System.StringComparison.Ordinal);
        }

        private void OnPreprocessTexture()
        {
            if (IsArtwork(assetPath)) Configure((TextureImporter)assetImporter);
        }

        private static void Configure(TextureImporter importer)
        {
            importer.textureType = TextureImporterType.Default;
            importer.textureShape = TextureImporterShape.Texture2D;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 4096;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.filterMode = importer.assetPath.Contains("/ThunderS1/")?FilterMode.Point:FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
        }

        [InitializeOnLoadMethod]
        private static void AfterScriptsReload()
        {
            EditorApplication.delayCall += EnsureImportSettings;
        }

        [MenuItem("Tools/Naki Legend/Reimport generated artwork")]
        public static void EnsureImportSettings()
        {
            if (!AssetDatabase.IsValidFolder(ArtFolder)) return;
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { ArtFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;
                if (importer.npotScale == TextureImporterNPOTScale.None && importer.maxTextureSize == 4096 &&
                    importer.textureType == TextureImporterType.Default && importer.textureShape == TextureImporterShape.Texture2D && !importer.mipmapEnabled &&
                    importer.textureCompression == TextureImporterCompression.Uncompressed && importer.alphaIsTransparency) continue;
                Configure(importer);
                importer.SaveAndReimport();
            }
        }
    }
}
