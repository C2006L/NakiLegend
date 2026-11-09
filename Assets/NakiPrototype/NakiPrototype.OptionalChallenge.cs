using System.Collections.Generic;
using UnityEngine;

namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private bool challengeActive,challengeMenu;
  private int challengeRound;
  private GameObject challengeAltar;
  private Vector2 challengeAltarAt;
  private string OptionalChallengeName {get{return iceSampleMode?"霜印试炼":woodlandMode?"林誓试炼":fireChapterMode?"余烬试炼":"风灯试炼";}}
  private bool ChallengeEligible(RoomNode n){return n.kind==RoomKind.Reward&&!n.challengeWon&&CurrentIsland.ordinal>=2&&!bossPracticeMode&&thunderChapterPracticeBoss<0&&!gatehouseSampleMode;}
  private void BuildOptionalChallengeAltar(RoomNode n)
  {
   if(!ChallengeEligible(n))return;challengeAltarAt=FindFreeInteractionPoint(new Vector2(-3,1.3f));
   transitReservations.Add(new Rect(challengeAltarAt-new Vector2(1,1),new Vector2(2,2)));
   challengeAltar=Draw("Optional ordeal altar",PressureProp(5),challengeAltarAt,Vector2.one,Color.white,DepthAt(challengeAltarAt.y));
  }
  private bool TryOptionalChallengeInteraction()
  {
   if(challengeAltar==null||challengeActive||!SafeCheckpointRoom||Vector2.Distance(position,challengeAltarAt)>1.45f)return false;
   challengeMenu=true;CancelBowCharge();return true;
  }
  private bool HandleOptionalChallengeMenu()
  {
   if(!challengeMenu)return false;
   if(Pressed('e')){challengeMenu=false;BlockWorldInteraction();return true;}
   if(Pressed('f'))StartOptionalChallenge();return true;
  }
  private void DrawOptionalChallengeMenu(float w,float h)
  {
   HudRect(new Rect(0,0,w,h),new Color(.02f,.025f,.035f,.94f));
   CenterLabel(new Rect(w/2-420,h/2-210,840,50),OptionalChallengeName+" · 自愿迎战",journeyTitleStyle);
   CenterLabel(new Rect(w/2-380,h/2-130,760,125),"两轮增援：侧翼追击、护阵者与狙击手\n开启后石门暂时关闭，清场才会重新开放\n奖励：本阶段武器变体一把，元素药剂一份",journeyTextStyle);
   CenterLabel(new Rect(w/2-380,h/2+12,760,60),"不影响原补给箱；可直接离开，继续主线。\n失败可回到上一个安全存档点。",journeySmallStyle);
   if(FlatButton(new Rect(w/2-300,h/2+105,280,64),"F 开启挑战"))StartOptionalChallenge();
   if(FlatButton(new Rect(w/2+20,h/2+105,280,64),"ESC 留待以后")){challengeMenu=false;BlockWorldInteraction();}
  }
  private void StartOptionalChallenge()
  {
   RoomNode n=journeyRooms[currentRoom];if(!challengeMenu||!ChallengeEligible(n)||!SafeCheckpointRoom)return;
   // Capture a safe state before the optional encounter. Never save an active ordeal.
   if(thunderChapterMode)SaveChapterCheckpoint(false);else SaveCheckpoint(false);
   challengeMenu=false;challengeActive=true;challengeRound=0;n.cleared=false;CancelBowCharge();BlockWorldInteraction();
   BeginEncounterRecord();if(challengeAltar!=null)challengeAltar.GetComponent<SpriteRenderer>().color=C("ffbb83");UpdateExplorationDoors();SpawnChallengeRound();
  }
  private void SpawnChallengeRound()
  {
   challengeRound++;var rng=new System.Random(activeSeed^currentRoom*211^challengeRound*59);int count=CurrentIsland.ordinal>=4?7:6;
   for(int i=0;i<count;i++){int role=i==0?3:i==1?4:i%2==0?1:2;QueueEnemy(TacticKind(role),FindJourneySpawn(rng,.5f),challengeRound==2&&i==2);PendingSpawn p=pendingSpawns[pendingSpawns.Count-1];p.role=role;p.when=clock+1.5f+i*.3f;}
   encounterWaveStartedAt=clock;invulnerableUntil=Mathf.Max(invulnerableUntil,clock+1.2f);
  }
  private bool TickOptionalChallenge()
  {
   if(!challengeActive)return false;
   if(enemies.Count==0&&pendingSpawns.Count==0){if(challengeRound<2)SpawnChallengeRound();else CompleteOptionalChallenge();}
   return true;
  }
  private void CompleteOptionalChallenge()
  {
   RoomNode n=journeyRooms[currentRoom];if(!challengeActive||n.challengeWon)return;challengeActive=false;n.cleared=true;n.challengeWon=true;FinishEncounterRecord(true);
   foreach(Shot s in shots)if(s.root!=null)Destroy(s.root);shots.Clear();ClearBossSignatures();
   var choices=SupplyCandidates().FindAll(id=>id>=42&&weaponCatalog[id].variant>0&&!weaponCatalog[id].signature);
   int reward=choices[new System.Random(activeSeed^currentRoom*541^0x2a7).Next(choices.Count)];
   var drop=new StoredWeapon{id=reward,at=FindFreeInteractionPoint(challengeAltarAt+Vector2.down)};n.drops.Add(drop);CreateWeaponDropVisual(drop);elementSupplies++;
   if(challengeAltar!=null){challengeAltar.SetActive(false);Destroy(challengeAltar);}challengeAltar=null;
   AnnounceDiscovery("试炼完成 · "+weaponCatalog[reward].name+" · 元素药剂 +1");UpdateExplorationDoors();RequestCheckpoint();if(thunderChapterMode)SaveChapterCheckpoint(false);
  }
 }
}
