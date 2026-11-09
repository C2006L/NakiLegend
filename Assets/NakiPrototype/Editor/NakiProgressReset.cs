#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace NakiLegend.Prototype.Editor
{
 [InitializeOnLoad] public static class NakiProgressReset
 {
  private static readonly string[] Keys={"journey-v1.json","thunder-chapter-v1.json","thunder-prototype-v1.json","ice-sample-v1.json","fire-chapter-v1.json","woodland-chapter-v1.json","manor-chapter-v1.json","last-journey-v1.json"};
  private static string ProjectRoot {get{return Path.GetFullPath(Path.Combine(Application.dataPath,".."));}}
  private static string RequestPath {get{return Path.Combine(ProjectRoot,"Temp","ResetNakiJourney.request");}}
  static NakiProgressReset(){EditorApplication.update+=CheckRequest;}
  private static void CheckRequest()
  {
   if(Application.isBatchMode||!File.Exists(RequestPath)||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
   if(EditorApplication.isPlayingOrWillChangePlaymode){EditorApplication.isPlaying=false;return;}
   File.Delete(RequestPath);
   try{BackupAndReset();}catch(Exception e){File.WriteAllText(Path.Combine(ProjectRoot,"Temp","ResetNakiJourney.result"),"FAILED: "+e.Message);Debug.LogException(e);}
  }
  [MenuItem("Tools/Naki Legend/Saves/Backup and Reset Five Islands")]
  public static void BackupAndReset()
  {
   if(EditorApplication.isPlayingOrWillChangePlaymode){Directory.CreateDirectory(Path.GetDirectoryName(RequestPath));File.WriteAllText(RequestPath,"Reset progress after stopping Play");EditorApplication.isPlaying=false;return;}
   string source=Path.Combine(Application.persistentDataPath,"NakiLegend");
   string backup=Path.Combine(ProjectRoot,"ProgressBackups",DateTime.Now.ToString("yyyyMMdd-HHmmss-fff"));
   string[] files=Directory.Exists(source)?Directory.GetFiles(source).Where(p=>Keys.Any(k=>Path.GetFileName(p)==k||Path.GetFileName(p).StartsWith(k+".",StringComparison.Ordinal))).ToArray():new string[0];
   Directory.CreateDirectory(backup);
   // Copy every approved progress file first. A failed backup must never erase the source.
   foreach(string file in files){string target=Path.Combine(backup,Path.GetFileName(file));File.Copy(file,target,false);if(!File.ReadAllBytes(file).SequenceEqual(File.ReadAllBytes(target)))throw new IOException("Progress backup verification failed");}
   foreach(string file in files)File.Delete(file);
   string result="RESET OK: "+files.Length+" progress files archived to "+backup;
   File.WriteAllText(Path.Combine(backup,"reset.txt"),result);Directory.CreateDirectory(Path.Combine(ProjectRoot,"Temp"));File.WriteAllText(Path.Combine(ProjectRoot,"Temp","ResetNakiJourney.result"),result);
   Debug.Log(result+". Open Tools > Naki Legend > Play > 1 Desert Adventure to start at Mingsha. Audio preferences and project assets are preserved.");
  }
 }
}
#endif
