#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace NakiLegend.Prototype.Editor
{
 [InitializeOnLoad] public static class NakiManorReplayReset
 {
  private static string Root {get{return Path.GetFullPath(Path.Combine(Application.dataPath,".."));}}
  private static string Request {get{return Path.Combine(Root,"Temp","ResetNakiManor.request");}}
  static NakiManorReplayReset(){EditorApplication.update+=Tick;}
  private static void Tick()
  {
   if(Application.isBatchMode||!File.Exists(Request)||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
   if(EditorApplication.isPlayingOrWillChangePlaymode){EditorApplication.isPlaying=false;return;}
   File.Delete(Request);
   try{Reset();}catch(Exception e){File.WriteAllText(Path.Combine(Root,"Temp","ResetNakiManor.result"),"FAILED: "+e);Debug.LogException(e);}
  }
  [MenuItem("Tools/Naki Legend/Saves/Backup and Reset Manor Only")]
  public static void Reset()
  {
   if(EditorApplication.isPlayingOrWillChangePlaymode){Directory.CreateDirectory(Path.GetDirectoryName(Request));File.WriteAllText(Request,"Manor replay");EditorApplication.isPlaying=false;return;}
   string source=Path.Combine(Application.persistentDataPath,"NakiLegend");
   string backup=Path.Combine(Root,"ProgressBackups","ManorReplay-"+DateTime.Now.ToString("yyyyMMdd-HHmmss-fff"));
   string[] files=Directory.Exists(source)?Directory.GetFiles(source).Where(p=>Path.GetFileName(p)=="manor-chapter-v1.json"||Path.GetFileName(p).StartsWith("manor-chapter-v1.json.",StringComparison.Ordinal)).ToArray():new string[0];
   Directory.CreateDirectory(backup);
   foreach(string file in files){string target=Path.Combine(backup,Path.GetFileName(file));File.Copy(file,target,false);if(!File.ReadAllBytes(file).SequenceEqual(File.ReadAllBytes(target)))throw new IOException("Manor backup verification failed");}
   foreach(string file in files)File.Delete(file);
   string result="RESET OK: "+files.Length+" Manor files backed up to "+backup+". Other islands and preferences preserved.";
   File.WriteAllText(Path.Combine(backup,"reset.txt"),result);Directory.CreateDirectory(Path.Combine(Root,"Temp"));File.WriteAllText(Path.Combine(Root,"Temp","ResetNakiManor.result"),result);Debug.Log(result);
  }
 }
}
#endif
