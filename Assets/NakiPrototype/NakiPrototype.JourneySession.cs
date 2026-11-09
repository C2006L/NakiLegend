using System;
using System.IO;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  [Serializable] private sealed class JourneySession {public int version=1,island;}
  private string JourneySessionPath {get{return Path.Combine(AtlasDirectory,"last-journey-v1.json");}}
  private bool CanRecordJourneySession {get{return enableDesertJourney&&!bossPracticeMode&&!gatehouseSampleMode&&!thunderPrototypeMode&&thunderChapterPracticeBoss<0&&(string.IsNullOrEmpty(checkpointPathOverride)||Path.GetFullPath(checkpointPathOverride)==Path.GetFullPath(AtlasSavePath(0)))&&string.IsNullOrEmpty(chapterCheckpointPathOverride)&&(!Application.isBatchMode||allowBatchCheckpoints&&!string.IsNullOrEmpty(chapterBatchSaveDirectory));}}
  private void RecordJourneySession()
  {
   if(!CanRecordJourneySession)return;
   try{
    string path=JourneySessionPath;Directory.CreateDirectory(Path.GetDirectoryName(path));
    File.WriteAllText(path+".tmp",JsonUtility.ToJson(new JourneySession{island=CurrentIsland.ordinal-1}));
    if(File.Exists(path))File.Replace(path+".tmp",path,path+".bak");else File.Move(path+".tmp",path);
   }catch(Exception e){Debug.LogWarning("Naki voyage resume marker: "+e.Message);}
  }
  private bool TryResumeJourneySession()
  {
   // Explicit chapter/practice scenes continue their own island; the main adventure resumes the last actual safe save.
   if(!CanRecordJourneySession||thunderChapterMode||thunderPrototypeMode||!enableDesertJourney)return false;
   foreach(string path in new[]{JourneySessionPath,JourneySessionPath+".bak"}){
    if(!File.Exists(path))continue;
    try{
     JourneySession saved=JsonUtility.FromJson<JourneySession>(File.ReadAllText(path));if(saved==null||saved.version!=1||saved.island<0||saved.island>6)continue;
     if(saved.island==0){checkpointPathOverride=AtlasSavePath(0);if(LoadCheckpoint(false))return true;continue;}
     StopOpeningCinematic();SetAtlasIsland(saved.island);
     if(LoadChapterCheckpoint(false)){chapterSetup=false;return true;}
     SetAtlasIsland(0);BeginJourney(false);
    }catch(Exception e){Debug.LogWarning("Naki voyage resume: "+e.Message);SetAtlasIsland(0);BeginJourney(false);}
   }
   return false;
  }
 }
}
