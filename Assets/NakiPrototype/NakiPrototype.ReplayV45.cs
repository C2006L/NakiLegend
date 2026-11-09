using System;
using System.IO;
using UnityEngine;
namespace NakiLegend.Prototype {
 public sealed partial class NakiPrototype {
  [Serializable] private sealed class ReplayProfile {public int version=1;public string profile;}
  [Serializable] private sealed class ReplayEndings {public int version=1,manor,capital;}
  private int replayMenu,replayChoice,replayOrigin;
  private bool replayWasPaused;
  private float replayOpened;
  private ReplayEndings replayEndings;
  private bool ReplayEnded {get{return CapitalTerminal||manorCourtyardMode&&manorEnding>0&&manorEndingSeen;}}
  private bool ReplayDiskReady {get{return !Application.isBatchMode||allowBatchCheckpoints;}}
  private string ActiveReplayPath {get{return Path.Combine(JourneyRootDirectory,"active-playthrough-v1.json");}}
  private static bool ValidReplayProfile(string id){if(id==null||id.Length>100)return false;foreach(char c in id)if(!char.IsLetterOrDigit(c)&&c!='-'&&c!='_')return false;return true;}
  private void WriteReplayFile(string path,string json){Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllText(path+".tmp",json);if(File.Exists(path))File.Replace(path+".tmp",path,path+".bak");else File.Move(path+".tmp",path);}
  private void RestoreActiveReplayProfile(){
   if(!enableDesertJourney||!string.IsNullOrEmpty(journeyTestProfile)||bossPracticeMode||thunderChapterPracticeBoss>=0)return;
   foreach(string path in new[]{ActiveReplayPath,ActiveReplayPath+".bak"})try{if(!File.Exists(path))continue;var record=JsonUtility.FromJson<ReplayProfile>(File.ReadAllText(path));if(record!=null&&record.version==1&&ValidReplayProfile(record.profile)&&(record.profile==""||Directory.Exists(Path.Combine(JourneyRootDirectory,"playthroughs",record.profile)))){journeyTestProfile=record.profile;return;}}catch(Exception e){Debug.LogWarning("Replay profile: "+e.Message);}
  }
  private void ActivateReplayProfile(string profile){if(!ValidReplayProfile(profile))throw new InvalidDataException("Invalid replay profile");WriteReplayFile(ActiveReplayPath,JsonUtility.ToJson(new ReplayProfile{profile=profile}));journeyTestProfile=profile;}
  private ReplayEndings ReadReplayEndings(){
   if(replayEndings!=null)return replayEndings;replayEndings=new ReplayEndings();
   try{string path=Path.Combine(JourneyRootDirectory,"ending-collection-v1.json");if(File.Exists(path)){var record=JsonUtility.FromJson<ReplayEndings>(File.ReadAllText(path));if(record!=null&&record.version==1&&record.manor>=0&&record.manor<=7&&record.capital>=0&&record.capital<=3)replayEndings=record;}}catch(Exception e){Debug.LogWarning("Ending collection: "+e.Message);}return replayEndings;
  }
  private void RememberReplayEnding(){
   if(!ReplayDiskReady)return;var record=ReadReplayEndings();
   if(manorCourtyardMode&&manorEndingSeen&&manorEnding>=1&&manorEnding<=3)record.manor|=1<<(manorEnding-1);
   if(CapitalTerminal)record.capital|=1<<(capitalEnding-1);
   try{WriteReplayFile(Path.Combine(JourneyRootDirectory,"ending-collection-v1.json"),JsonUtility.ToJson(record,true));}catch(Exception e){Debug.LogWarning("Ending collection: "+e.Message);}
  }
  private string ForkReplayProfile(bool fresh){
   string source=JourneySaveDirectory,previous=journeyTestProfile;
   if(!ReplayDiskReady)throw new IOException("Practice cannot replace journey saves");
   RememberReplayEnding();PrepareReplaySave();
   if(origin!=Origin.Unchosen&&!(manorCourtyardMode&&manorEnding>=2)&&!SaveAtlasIsland())throw new IOException("当前安全点保存失败，原进度未改动");
   string profile="journey-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")+"-"+Guid.NewGuid().ToString("N").Substring(0,8),destination=Path.Combine(JourneyRootDirectory,"playthroughs",profile);Directory.CreateDirectory(destination);
   if(!fresh&&Directory.Exists(source))foreach(string path in Directory.GetFiles(source)){
    string name=Path.GetFileName(path);bool save=name.StartsWith("last-journey-v1.json",StringComparison.Ordinal);
    foreach(var island in IslandProfiles)if(name.StartsWith(island.saveKey,StringComparison.Ordinal))save=true;
    if(save)File.Copy(path,Path.Combine(destination,name),false);
   }
   WriteReplayFile(Path.Combine(destination,"previous-journey-v1.json"),JsonUtility.ToJson(new ReplayProfile{profile=previous}));
   ActivateReplayProfile(profile);return previous;
  }
  private bool ReplayClearedRoom {get{
   if(world==null||currentRoom<0||currentRoom>=journeyRooms.Count||!journeyRooms[currentRoom].cleared||health<=0||challengeActive||pendingSpawns.Count>0)return false;
   foreach(var enemy in enemies)if(enemy.hp>0&&!CombatFixture(enemy))return false;
   return true;
  }}
  private void PrepareReplaySave(){
   if(!ReplayClearedRoom)throw new IOException("先清理当前房间，再从暂停菜单重玩");
   // A paused finishing effect never advances to its expiry. Retire it only after explicit confirmation.
   StopOriginAction();CancelBowCharge();ClearStarfall();
   foreach(var blast in coverBlasts)if(blast.warning!=null)Destroy(blast.warning);coverBlasts.Clear();
   for(int i=enemies.Count-1;i>=0;i--)if(enemies[i].hp<=0||CombatFixture(enemies[i])){var enemy=enemies[i];if(enemy.root!=null)Destroy(enemy.root);if(enemy.shadow!=null)Destroy(enemy.shadow);if(enemy.warning!=null)Destroy(enemy.warning);enemies.RemoveAt(i);}
  }
  private bool HandleReplayShortcut(bool next,bool interact){
   if(paused&&next){OpenReplayMenu();return true;} // Never fall through to the old N chapter/reset shortcut.
   if(ReplayEnded&&(next||interact)&&!worldAtlasMenu){OpenReplayMenu();return true;}
   return false;
  }
  private void OpenReplayMenu(){
   if(!enableDesertJourney||!ReplayEnded&&!paused)return;
   if(StoryActive||chapterCg||openingActive){JourneyHint("先结束当前对话或过场，再打开重玩菜单");return;}
   if(!ReplayEnded&&!ReplayClearedRoom){JourneyHint("先清理当前房间，再从暂停菜单重玩");return;}
   replayWasPaused=paused;paused=false;replayMenu=1;replayChoice=0;replayOrigin=origin==Origin.Dark?1:0;replayOpened=Time.unscaledTime;CancelBowCharge();BlockWorldInteraction();RememberReplayEnding();
  }
  private void CancelReplayMenu(){replayMenu=0;paused=replayWasPaused;BlockWorldInteraction();}
  private void ChooseReplay(int choice){
   if(choice==0&&!ReplayEnded)return;replayChoice=choice;replayMenu=2;replayOpened=Time.unscaledTime;BlockWorldInteraction();
  }
  private bool HandleReplayInput(){
   if(replayMenu==0)return HandleReplayShortcut(Pressed('n'),Pressed('f'));
   if(Pressed('e')){if(replayMenu==2){replayMenu=1;replayOpened=Time.unscaledTime;BlockWorldInteraction();}else CancelReplayMenu();return true;}
   if(replayMenu==1){for(int i=0;i<4;i++)if(Pressed((char)('1'+i)))ChooseReplay(i);return true;}
   if(replayChoice==1&&(Pressed('a')||Pressed('d')))replayOrigin=1-replayOrigin;
   if(WorldInteractionReady&&Time.unscaledTime>replayOpened+.3f&&Pressed('f'))ExecuteReplay();return true;
  }
  private void ExecuteReplay(){
   if(replayMenu!=2||!WorldInteractionReady||Time.unscaledTime<replayOpened+.3f)return;
   string previous=journeyTestProfile;int island=CurrentIsland.ordinal-1;
   try{
    if(replayChoice==3){
     string path=Path.Combine(JourneySaveDirectory,"previous-journey-v1.json");if(!File.Exists(path))throw new IOException("当前是最初的旅程，没有上一轮进度");var parent=JsonUtility.FromJson<ReplayProfile>(File.ReadAllText(path));if(parent==null||parent.version!=1||!ValidReplayProfile(parent.profile))throw new InvalidDataException("上一轮记录无法读取");
     PrepareReplaySave();
     if(origin!=Origin.Unchosen&&!(manorCourtyardMode&&manorEnding>=2)&&!SaveAtlasIsland())throw new IOException("当前进度未能保存");
     ActivateReplayProfile(parent.profile);SetAtlasIsland(0);if(!TryResumeJourneySession())throw new IOException("上一轮安全点暂时无法读取");
    }else{
     ForkReplayProfile(replayChoice==2);
     if(replayChoice==0){
      if(capitalMode){capitalEnding=capitalConfirm=0;capitalCg=capitalLetter=-1;capitalSeen&=~48;chapterEnding=journeyComplete=false;chapterStone=true;storyLines=null;EnterExplorationRoom(26,-1);}
      else{manorEnding=manorChoiceMode=0;manorEndingSeen=false;manorProgress&=7;manorVictorySeen=true;manorVictoryCg=false;chapterEnding=chapterCg=journeyComplete=false;storyLines=null;health=Mathf.Max(1,health);EnterExplorationRoom(44,-1);}
      BuildChapterActions();UpdateExplorationDoors();UpdateFlowGuide();if(!SaveAtlasIsland())throw new IOException("重玩分支未能保存");
     }else{
      SetAtlasIsland(replayChoice==2?0:island);storyLines=null;StopOpeningCinematic();thunderPreset=replayOrigin==1?ThunderPreset.Dark:ThunderPreset.Light;
      if(replayChoice==2||island==0)BeginJourney(true);else BeginThunderChapter(true);
      // This is an explicit fresh chapter, so its new state replaces only the fork's copy.
      RecordJourneySession();if(origin!=Origin.Unchosen&&!SaveAtlasIsland())throw new IOException("新旅程未能保存");
     }
    }
    replayMenu=0;paused=worldAtlasMenu=atlasConfirm=false;BlockWorldInteraction();JourneyHint("已进入独立重玩分支 · 原通关进度与结局记录保留");
   }catch(Exception e){
    if(journeyTestProfile!=previous){try{ActivateReplayProfile(previous);SetAtlasIsland(island);LoadAtlasIsland();}catch(Exception restore){Debug.LogWarning("Replay rollback: "+restore.Message);}}
    JourneyHint(e.Message);replayMenu=1;replayOpened=Time.unscaledTime;Debug.LogWarning("Replay: "+e.Message);
   }
  }
  private static int ReplayBits(int value){int count=0;for(;value>0;value>>=1)count+=value&1;return count;}
  private void DrawReplayEndButton(float w,float h){if(FlatButton(new Rect(w-350,h-169,320,38),"重玩与其他结局 · N"))OpenReplayMenu();}
  private void DrawReplayMenu(float w,float h){
   HudRect(new Rect(0,0,w,h),C("101c26"));float width=Mathf.Min(900,w-60),x=(w-width)/2;var record=ReadReplayEndings();
   FittedLabel(new Rect(x,h*.10f,width,45),replayMenu==1?"故事还可以有另一种走法":"开始之前",journeyTitleStyle);
   if(replayMenu==1){
    FittedLabel(new Rect(x,h*.19f,width,70),"结局收录：暮钟岛 "+ReplayBits(record.manor)+" / 3 · 王都 "+ReplayBits(record.capital)+" / 2\n重玩使用独立存档分支，原旅程与已收录结局保留。",journeyTextStyle,true);
    string[] options={"1 回到抉择前 · 保留调查和首领胜利，体验另一个结局","2 重玩本章 · 从入口重新探索，可切换光 / 贤者之石路线","3 全新旅程 · 从鸣沙洲开始，重新选择形态与职业","4 返回上一轮旅程 · 继续原来的通关进度"};
    for(int i=0;i<4;i++){bool enabled=GUI.enabled;GUI.enabled=enabled&&(i!=0||ReplayEnded)&&(i!=3||File.Exists(Path.Combine(JourneySaveDirectory,"previous-journey-v1.json")));if(FlatButton(new Rect(x,h*.34f+i*62,width,52),options[i]))ChooseReplay(i);GUI.enabled=enabled;}
    if(FlatButton(new Rect(x,h-85,width,44),"ESC 返回当前旅程"))CancelReplayMenu();return;
   }
   string words=replayChoice==0?"回到当前章节的最终抉择前。调查与首领胜利保留，不必重新战斗。原结局仍会保留在上一轮旅程和结局收录中。":replayChoice==1?"从当前章节入口重新开始，使用本章初始配置。此前章节仍保留完成进度，本章的剧情和首领重新体验。\n路线："+(replayOrigin==0?"光之力":"贤者之石")+" · A / D 切换":replayChoice==2?"开启独立的新旅程，从鸣沙洲重新选择形态和职业。装备、技能学习与关卡进度从头开始；原通关进度和结局收录保留。":"切回上一轮旅程的最近安全点。当前分支也会保留，原有结局不会被改写。";
   FittedLabel(new Rect(x,h*.28f,width,180),words,journeyTextStyle,true);
   if(FlatButton(new Rect(x,h*.65f,width*.47f,58),"返回选择 · ESC")){replayMenu=1;BlockWorldInteraction();}
   if(FlatButton(new Rect(x+width*.53f,h*.65f,width*.47f,58),"确认开始 · F")&&WorldInteractionReady)ExecuteReplay();
  }
 }
}
