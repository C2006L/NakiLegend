using UnityEditor;
using UnityEngine;
namespace NakiLegend.Prototype.Editor
{
 public sealed class NakiAudioImport:AssetPostprocessor
 {
  private void OnPreprocessAudio()
  {
   if(!assetPath.Contains("/Resources/NakiAudio/")&&!assetPath.Contains("/Resources/NakiAudioV2/")&&!assetPath.Contains("/Resources/NakiSkills/")&&!assetPath.Contains("/Resources/NakiCombatAudioV3/"))return;
   var importer=(AudioImporter)assetImporter;var settings=importer.defaultSampleSettings;
   bool music=assetPath.Contains("_explore")||assetPath.Contains("_combat")||assetPath.Contains("_boss")||assetPath.Contains("ambient_")||assetPath.EndsWith("ending.wav")||assetPath.EndsWith("ending.ogg");
   settings.loadType=music?AudioClipLoadType.Streaming:AudioClipLoadType.DecompressOnLoad;
   settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=music?.75f:.85f;
   settings.preloadAudioData=!music;importer.defaultSampleSettings=settings;importer.forceToMono=!music;
  }
 }
}
