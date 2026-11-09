using System;
using System.IO;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private bool worldAtlasMenu,atlasConfirm,atlasArmed;
  private int atlasIsland,atlasFloor=-1;
  private float atlasOpened;
  private Texture2D atlasArt;
  private readonly bool[] atlasSaved=new bool[7],atlasFinished=new bool[7];
  private readonly int[] atlasClears=new int[7];
  private string atlasMessage="";
  private static readonly string[] AtlasNames={"鸣沙洲","雷雨高原","霜潮群岛","赤炉群岛","青栖林海","暮钟岛","王都"};
  private static readonly Vector2[] AtlasPoints={new Vector2(.29f,.71f),new Vector2(.51f,.45f),new Vector2(.77f,.22f),new Vector2(.84f,.63f),new Vector2(.59f,.79f),new Vector2(.40f,.17f),new Vector2(.16f,.44f)};
  private static readonly string[][] AtlasRegions={new[]{"沙丘边缘","断渠遗迹","古渠核心"},new[]{"雨中草坡","引雷遗址","巡雷圣所"},new[]{"冰港沿岸","裂潮冰峡","寒潮晶庭"},new[]{"黑砂海岸","锻链工坊","地心赤炉"},new[]{"雾岸林缘","朽木腹地","树冠旧庭"},new[]{"归功外庭","断粮旧村","无昼内庭"},new[]{"归航港与下城","王印内城","日冕王宫"}};
  private bool AtlasAllowed {get{return (!CapitalOverlay||CapitalTerminal)&&enableDesertJourney&&(!manorCourtyardMode||!manorIntro)&&origin!=Origin.Unchosen&&health>0&&!bossPracticeMode&&!gatehouseSampleMode&&!thunderPrototypeMode&&!lightningTrialMode&&thunderChapterPracticeBoss<0&&!chapterSetup&&!openingActive&&!StoryActive&&!chapterCg&&!showRainCG&&!talentMenu&&!rewardMenu&&!chapterRewardMenu&&!inventoryMenu&&!chapterSkillsMenu&&!voyagerJournal&&!challengeMenu&&!originAbilityMenu&&pendingWeaponPickup==null;}}
  private bool AtlasTravelReady {get{return AtlasAllowed&&SafeCheckpointRoom&&(!thunderChapterMode||SafeChapterCheckpoint);}}
  private string AtlasDirectory {get{return JourneySaveDirectory;}}
  private string AtlasSavePath(int island){return Path.Combine(AtlasDirectory,IslandProfiles[island].saveKey);}
  private void OpenWorldAtlas(int selected=-1)
  {
   if(!AtlasAllowed){JourneyHint("先结束对话或奖励选择，再展开全域地图");return;}
   worldAtlasMenu=true;mapMenu=false;paused=false;atlasConfirm=atlasArmed=false;atlasIsland=selected>=0?Mathf.Clamp(selected,0,6):CurrentIsland.ordinal-1;atlasFloor=-1;atlasMessage="";CancelBowCharge();
   if(atlasArt==null)atlasArt=Resources.Load<Texture2D>("NakiArt/WorldAtlas/naki-region");RefreshAtlasProgress();if(!string.IsNullOrEmpty(journeyTestProfile)&&atlasFinished[CurrentIsland.ordinal-1]&&(selected<0||selected==CurrentIsland.ordinal-1))atlasIsland=Mathf.Min(6,CurrentIsland.ordinal);BlockWorldInteraction();
  }
  private void CloseWorldAtlas(){worldAtlasMenu=false;atlasConfirm=false;BlockWorldInteraction();}
  private bool HandleWorldAtlas()
  {
   if(!worldAtlasMenu){if(enableDesertJourney&&Pressed('g')&&AtlasAllowed){OpenWorldAtlas();return true;}return false;}
   if(!Held('f'))atlasArmed=true;
   if(Pressed('e')||Pressed('g')){if(atlasConfirm){atlasConfirm=false;BlockWorldInteraction();}else CloseWorldAtlas();return true;}
   if(atlasConfirm){if(atlasArmed&&Time.unscaledTime>=atlasOpened+.3f&&Pressed('f'))TravelAtlas(atlasIsland,atlasFloor);return true;}
   if(Pressed('a')){atlasIsland=(atlasIsland+6)%7;atlasFloor=-1;}if(Pressed('d')){atlasIsland=(atlasIsland+1)%7;atlasFloor=-1;}
   for(int i=0;i<3;i++)if(Pressed((char)('1'+i))&&atlasIsland<7)atlasFloor=i;
   if(Pressed('f'))RequestAtlasTravel();return true;
  }
  private void RequestAtlasTravel()
  {
   if(!FullJourneyTravelAllowed(atlasIsland,atlasFloor)){atlasMessage="完整旅程按章节推进：先完成前一章尾声，再从码头出发；楼层沿关卡通道前进。";return;}
   if(atlasIsland==6&&!CapitalTravelAllowed(atlasFloor)){atlasMessage="王都需先完成五岛与暮钟岛的光之归航；城内区域需依故事逐段开放。";return;}
   if(!AtlasTravelReady){atlasMessage="战斗中可以查看航图；清理房间后即可出发。";return;}
   atlasConfirm=true;atlasArmed=false;atlasOpened=Time.unscaledTime;
  }
  private void RefreshAtlasProgress()
  {
   for(int i=0;i<7;i++){atlasSaved[i]=atlasFinished[i]=false;atlasClears[i]=0;
    foreach(string path in new[]{AtlasSavePath(i),AtlasSavePath(i)+".bak"}){if(!File.Exists(path))continue;try{
     if(i==0){var s=JsonUtility.FromJson<JourneyCheckpoint>(File.ReadAllText(path));if(s==null||s.version!=1||s.rooms==null)continue;foreach(var r in s.rooms)if(r!=null&&r.cleared)atlasClears[i]++;atlasFinished[i]=s.canalComplete;}
     else{var s=JsonUtility.FromJson<ChapterCheckpoint>(File.ReadAllText(path));if(s==null||s.version!=1||s.rooms==null)continue;foreach(var r in s.rooms)if(r!=null&&r.cleared)atlasClears[i]++;atlasFinished[i]=s.ending;}
     atlasSaved[i]=true;break;
    }catch(Exception){atlasClears[i]=0;}}
   }
   int here=CurrentIsland.ordinal-1;atlasClears[here]=0;foreach(RoomNode n in journeyRooms)if(n.cleared)atlasClears[here]++;atlasSaved[here]=true;atlasFinished[here]=thunderChapterMode?chapterEnding:canalEpilogueComplete;RefreshCapitalReturn();
  }
  private void SetAtlasIsland(int island)
  {
   capitalMode=island==6;capitalCg=capitalLetter=-1;capitalConfirm=0;manorCourtyardMode=island==5;thunderChapterMode=island>0;fireChapterMode=island==3;iceSampleMode=island==2;iceFullChapter=true;woodlandMode=island==4;
   thunderPrototypeMode=gatehouseSampleMode=bossPracticeMode=lightningTrialMode=false;thunderChapterPracticeBoss=-1;thunderChapterStartFloor=0;chapterSetup=chapterCg=chapterSkillsMenu=false;pendingIceVoyage=-1;
  }
  private bool SaveAtlasIsland(){return thunderChapterMode?SaveChapterCheckpoint(false):SaveCheckpoint(false);}
  private bool LoadAtlasIsland(){return thunderChapterMode?LoadChapterCheckpoint(false):LoadCheckpoint(false);}
  private bool TravelAtlas(int island,int floor)
  {
   if(island<0||island>=7||floor < -1||floor>2||!AtlasTravelReady)return false;
   if(!FullJourneyTravelAllowed(island,floor)){atlasMessage="先完成前一章尾声，再继续旅程。";atlasConfirm=false;return false;}
   if(island==6&&!CapitalTravelAllowed(floor)){atlasMessage="请先完成归航与当前区域的调查。";atlasConfirm=false;return false;}
   int source=CurrentIsland.ordinal-1;
   if(source==island&&floor<0){CloseWorldAtlas();return true;}
   if(Application.isBatchMode&&!allowBatchCheckpoints)return false;
   var player=CaptureChapterPlayer();Origin path=origin;float swordBonus=swordDamage-initialSwordDamage,spellBonus=spellDamage-initialSpellDamage;
   bool inheritedMark=chapterMarkKnown,inheritedArc=chapterArcKnown;
   bool[] inherited={orbLearned,bladeLearned,waveLearned,windEmpowered,iceSpikeKnown,iceRingKnown,fireBoltKnown,emberRingKnown};
   float[] timers={Mathf.Max(0,iceSpikeReady-clock),Mathf.Max(0,iceRingReady-clock),Mathf.Max(0,fireBoltReady-clock),Mathf.Max(0,emberRingReady-clock)};
   string chapterOverride=chapterCheckpointPathOverride,desertOverride=checkpointPathOverride;
   // Write the source before touching island flags. Existing destination files are never replaced by a failed load.
   if(!SaveAtlasIsland()){atlasMessage="当前安全点未能保存，仍留在原地。";atlasConfirm=false;return false;}
   Vector2 arrival=position;
   if(source!=island){
    SetAtlasIsland(island);chapterCheckpointPathOverride=null;if(island==0)checkpointPathOverride=AtlasSavePath(0);
    bool exists=File.Exists(AtlasSavePath(island))||File.Exists(AtlasSavePath(island)+".bak");
    bool loaded=exists&&LoadAtlasIsland();
    if(exists&&!loaded){SetAtlasIsland(source);chapterCheckpointPathOverride=chapterOverride;checkpointPathOverride=desertOverride;LoadAtlasIsland();atlasConfirm=false;atlasMessage="目的地存档无法读取，已保留两岛文件并返回原安全点。";return false;}
    if(!loaded){LoadThunderArt();thunderPreset=path==Origin.Dark?ThunderPreset.Dark:ThunderPreset.Light;if(island==0)BeginJourney(false);else BeginThunderChapter(false);}
    // Local quests stay with their island. Equipment, consumables, talents and actual light/dark balances travel with the player.
    arrival=position;int[] destinationTalents=CopyTalents();int destinationLore=originLoreBits,destinationKnowledge=originKnowledge;
    bool[] destination={orbLearned,bladeLearned,waveLearned,windEmpowered,iceSpikeKnown,iceRingKnown,fireBoltKnown,emberRingKnown};
    float markRemaining=loaded?Mathf.Max(0,chapterMarkReady-clock):0,arcRemaining=loaded?Mathf.Max(0,chapterArcReady-clock):0;
    float[] ready={iceSpikeReady,iceRingReady,fireBoltReady,emberRingReady};for(int i=0;i<4;i++)ready[i]=loaded?Mathf.Max(0,ready[i]-clock):0;
    float destinationClock=clock;ApplyChapterPlayer(player);float shift=Mathf.Max(0,destinationClock-clock);clock+=shift;
    originSecondReady+=shift;originThirdReady+=shift;lightRegenReady+=shift;nextSword+=shift;nextSpell+=shift;nextDash+=shift;bladeReadyAt+=shift;waveReadyAt+=shift;lastDamageAt+=shift;returnBladeReady+=shift;guardPulseReady+=shift;repairTalentReady=clock+8;
    origin=path;NormalizeOriginPower();RestoreTalents(destinationTalents);MergeVoyageTalents(player.talents);originLoreBits|=destinationLore;originKnowledge|=destinationKnowledge;
    orbLearned=inherited[0]||(loaded&&destination[0]);bladeLearned=inherited[1]||(loaded&&destination[1]);waveLearned=inherited[2]||(loaded&&destination[2]);windEmpowered=inherited[3]||(loaded&&destination[3]);
    iceSpikeKnown=inherited[4]||(loaded&&destination[4]);iceRingKnown=inherited[5]||(loaded&&destination[5]);fireBoltKnown=inherited[6]||(loaded&&destination[6]);emberRingKnown=inherited[7]||(loaded&&destination[7]);
    iceSpikeReady=clock+Mathf.Max(timers[0],ready[0]);iceRingReady=clock+Mathf.Max(timers[1],ready[1]);fireBoltReady=clock+Mathf.Max(timers[2],ready[2]);emberRingReady=clock+Mathf.Max(timers[3],ready[3]);chapterMarkReady=clock+markRemaining;chapterArcReady=clock+arcRemaining;
    swordDamage=initialSwordDamage+swordBonus;spellDamage=initialSpellDamage+spellBonus;shield=Mathf.Min(shield,EffectiveMaximumShield);elementPower=Mathf.Min(elementPower,ElementMaximum);
    // A fresh test-style chapter must not grant its preset's unearned elemental skills.
    chapterMarkKnown|=inheritedMark;chapterArcKnown|=inheritedArc;
    RestoreSkillLoadout(player.loadout,true);
   }
   int room=floor<0?currentRoom:stageStarts[floor];bool pendingCg=chapterCg,pendingRain=pendingRainCG;
   storyLines=null;EnterExplorationRoom(room,-1);if(floor<0){position=NearestSafeFooting(arrival);UpdatePixelHero(Vector2.zero);}chapterCg=pendingCg;pendingRainCG=pendingRain;
   worldAtlasMenu=atlasConfirm=false;inventoryMenu=mapMenu=paused=false;BlockWorldInteraction();
   // This is travel, never a chapter-completion operation: no boss flags, quest flags or talents are awarded here.
   RequestCheckpoint();if(!SaveAtlasIsland())JourneyHint("已到达，但新安全点暂未写入；离岛前的存档仍保留");else JourneyHint("已到达"+ActiveLayerName(journeyRooms[currentRoom].stage)+"；G 全域地图");
   return true;
  }
 }
}
