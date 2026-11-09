using System;
using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  [Header("Royal Capital · final chapter")] public bool capitalMode;
  private int capitalFlags,capitalLetters,capitalSeen,capitalEnding,capitalCg=-1,capitalLetter=-1,capitalPage,capitalConfirm;
  private bool capitalArmed,capitalLoading;
  private float capitalOpened;
  private const int CapitalRescue=1,CapitalArchive=2,CapitalControl=4,CapitalHeart=8;
  private static readonly string[] CapitalRooms={"归航栈桥","封港长街","商会库房","关税哨所","下城市场","运粮小巷","居民避难所","码头医所","封港统领","王印城门","巡律长廊","军械库","航务档案室","契约庭道","贡纳泵站","王印总闸","内城诊所","王印执律官","日冕前庭","王宫侧廊","近卫军械室","御前书房","日轮回廊","仪式守备厅","空置议事厅","御前整备室","日冕王座厅","港务检查站","封锁粮桥","候船人小屋","货栈后巷","守粮人仓房","工匠街口","贡纳汇流厅","水槽值班房","泵站侧渠","检修器材库","近卫换防廊","王印审验厅","旧日誓约室","失令近卫营","仪式备件室"};
  private void BuildCapitalGraph()
  {
   journeyRooms.Clear();chapterDoors.Clear();chapterNames=(string[])CapitalRooms.Clone();
   for(int s=0;s<3;s++){
    int b=s*9;stageStarts[s]=b;stageEnds[s]=b+8;
    for(int i=0;i<9;i++){
     RoomKind k=i==0?RoomKind.Entrance:i==2?RoomKind.Reward:i==3||i==6?RoomKind.Event:i==7?RoomKind.Rest:i==8?RoomKind.Boss:i==5?RoomKind.Elite:RoomKind.Combat;
     if(s==0&&i==3)k=RoomKind.Elite;if(s==2&&i==3)k=RoomKind.Event;
     journeyRooms.Add(new RoomNode{id=b+i,stage=s,floor=s,depth=24+s*3,cell=new Vector2Int(i%3,i/3),kind=k,halfWidth=10,halfHeight=6.3f,layout=i%6,waves=k==RoomKind.Combat?2:k==RoomKind.Elite?2:k==RoomKind.Boss?1:0,stairsTo=-1});
    }
    int[] route={0,1,2,5,4,3,6,7,8};for(int j=1;j<route.Length;j++)ConnectRooms(b+route[j-1],b+route[j]);ConnectRooms(b+1,b+4);ConnectRooms(b+4,b+7);
   }
   ExpandCapitalGraph();
   journeyRooms[8].stairsTo=9;journeyRooms[9].stairsTo=8;journeyRooms[17].stairsTo=18;journeyRooms[18].stairsTo=17;
  }
  private void BeginCapital(bool newSeed)
  {
   manorCourtyardMode=fireChapterMode=iceSampleMode=woodlandMode=thunderPrototypeMode=gatehouseSampleMode=bossPracticeMode=lightningTrialMode=false;capitalMode=thunderChapterMode=true;
   if(!tuningCaptured){initialSwordDamage=swordDamage;initialSpellDamage=spellDamage;tuningCaptured=true;}swordDamage=initialSwordDamage;spellDamage=initialSpellDamage;
   if(newSeed||activeSeed==0)activeSeed=journeySeed!=0&&!newSeed?journeySeed:Environment.TickCount&int.MaxValue;if(activeSeed==0)activeSeed=71001;
   ResetGatehouseProgress();ResetPlayerProgression();LoadThunderChapterEquipment();BuildCapitalGraph();LoadCapitalArt();
   chapterWest=chapterEast=chapterRanger=chapterStone=chapterEnding=chapterCg=chapterLampCarried=chapterLampGiven=false;
   chapterWaymarks=chapterShortcuts=0;chapterWork=0;chapterSetup=chapterSavePending=chapterRewardMenu=chapterSkillsMenu=false;
   capitalFlags=capitalLetters=capitalSeen=capitalEnding=capitalConfirm=0;capitalLetter=capitalCg=-1;capitalArmed=false;
   manorIntro=manorVictoryCg=false;manorLetter=-1;manorChoiceMode=0;
   clock=0;origin=thunderPreset==ThunderPreset.Dark?Origin.Dark:Origin.Light;health=6;shield=EffectiveMaximumShield;elementPower=ElementMaximum;energy=origin==Origin.Light?LightMaximum:0;darkPower=origin==Origin.Dark?12:0;
   orbLearned=bladeLearned=waveLearned=windEmpowered=true;originKnowledge|=10;chapterMarkKnown=chapterArcKnown=iceSpikeKnown=iceRingKnown=fireBoltKnown=emberRingKnown=true;
   chapterSkillSlots[0]=0;chapterSkillSlots[1]=1;carriedWeapons[1]=7;ResetChapterCooldowns();nextSword=nextSpell=nextDash=dashUntil=invulnerableUntil=flashUntil=0;
   journeyComplete=pendingBossClear=won=false;doorTransition=0;
   int stage=Mathf.Clamp(thunderChapterPracticeBoss>=0?thunderChapterPracticeBoss:thunderChapterStartFloor,0,2);
   if(stage>0||thunderChapterPracticeBoss>=0){capitalFlags=15;capitalLetters=7;capitalSeen=7;for(int s=0;s<stage;s++)journeyRooms[s*9+8].cleared=true;}
   EnterExplorationRoom(thunderChapterPracticeBoss>=0?stageEnds[stage]:stageStarts[stage],-1);
   if(!capitalLoading){if(thunderChapterPracticeBoss>=0)TellStory("王都首领试炼：机关可以被武器击破。首领收招时仍会移动，命中可压制其下一轮起手。练习不覆盖正式王都存档。");else if(stage==0)QueueCapitalCg(0);}
  }
  private bool CapitalGateReady(int room)
  {return room==8?(capitalFlags&CapitalRescue)!=0:room==17?journeyRooms[8].cleared&&(capitalFlags&6)==6:room==26?journeyRooms[17].cleared&&capitalLetters==7&&(capitalFlags&CapitalHeart)!=0:true;}
  private ChapterAction CapitalFlowAction()
  {return FlowAction(currentRoom==6?1001:currentRoom==12?1002:currentRoom==15?1003:currentRoom==21?((capitalLetters&1)==0?1010:(capitalLetters&2)==0?1011:1012):currentRoom==25?1005:currentRoom==26?1041:1030);}
  private bool CapitalTravelAllowed(int floor)
  {
   if(capitalMode)return floor<=0||journeyRooms[floor==1?8:17].cleared;
   if(origin!=Origin.Light)return false;
   for(int i=0;i<6;i++)if(!atlasFinished[i])return false;
   bool lightReturn=capitalReturnVerified||manorCourtyardMode&&chapterEnding&&manorEnding==1;
   if(!lightReturn)return false;if(floor<=0)return true;
   foreach(string path in new[]{AtlasSavePath(6),AtlasSavePath(6)+".bak"})try{if(!System.IO.File.Exists(path))continue;var s=JsonUtility.FromJson<ChapterCheckpoint>(System.IO.File.ReadAllText(path));if(s!=null&&(s.map=="capital-v1"||s.map=="capital-v2")&&s.rooms!=null&&(s.rooms.Length==27||s.rooms.Length==42)&&s.rooms[floor==1?8:17].cleared)return true;}catch(Exception){}
   return false;
  }
  private bool capitalReturnVerified;
  private void RefreshCapitalReturn()
  {
   capitalReturnVerified=manorCourtyardMode&&chapterEnding&&manorEnding==1;
   if(capitalReturnVerified)return;
   foreach(string path in new[]{AtlasSavePath(5),AtlasSavePath(5)+".bak"})try{if(!System.IO.File.Exists(path))continue;var s=JsonUtility.FromJson<ChapterCheckpoint>(System.IO.File.ReadAllText(path));if(s!=null&&s.ending&&s.manorEnding==1&&s.origin==(int)Origin.Light){capitalReturnVerified=true;break;}}catch(Exception){}
  }
  private bool CapitalGateLocked(int room,int direction)
  {foreach(int other in journeyRooms[room].links)if(NeighborDirection(journeyRooms[room],journeyRooms[other])==direction&&!CapitalGateReady(other))return true;return false;}
  private int CapitalGoal {get{int s=journeyRooms[currentRoom].stage;return s==0?(capitalFlags&1)==0?6:8:s==1?(capitalFlags&2)==0?12:(capitalFlags&4)==0?15:17:capitalLetters!=7?21:(capitalFlags&8)==0?25:26;}}
  private string CapitalObjective()
  {
   if(chapterEnding)return capitalEnding==1?"六片已分存，返乡船即将启航 · G 回忆航图":"NakiTong 已重生 · 诸岛居民接过未竟之事";
   if(chapterStone)return "国王已败 · 靠近王印残台，决定自己心中之光的去向";
   if(!journeyRooms[currentRoom].cleared)return currentRoom==8?"封港统领 · 破坏两座弩机，解除封港阵列":currentRoom==17?"王印执律官 · 击破敕令柱，腾出安全走廊":currentRoom==26?"国王 · 击破贡纳节点，借石柱挡下裁决光束":"第 "+currentWave+" / "+journeyRooms[currentRoom].waves+" 波 · 近卫贴身、射手侧移、术士封路";
   int g=CapitalGoal;return g==6?"前往居民避难所 · 解除关税哨所，开启通往运粮小巷的撤离通道":g==12?"前往航务档案室 · 取回原始航海名册":g==15?"清理贡纳泵站 · 关闭王印总闸":g==21?"前往御前书房 · 在同一册原件中读完名册、契约与仪式手稿":g==25?"原件已读毕 · 前往御前整备室，想清自己的选择":"前往"+CapitalRooms[g]+" · 沿金色路标继续";
  }
  private void CapitalEntered(RoomNode n)
  {
   CapitalExpansionStory(n);
   if(n.kind==RoomKind.Rest&&!n.claimed){n.claimed=true;health=Mathf.Min(6,health+2);shield=Mathf.Min(EffectiveMaximumShield,shield+2);elementPower=Mathf.Min(ElementMaximum,elementPower+35);JourneyHint("王都医所 · 本处补给一次；F5 保存，T 自行配置技能");}
   if(!capitalLoading&&n.id==1&&(capitalSeen&2)==0)QueueCapitalCg(1);
   BuildChapterActions();
   if(n.id==25&&capitalLetters==7&&(capitalFlags&CapitalHeart)==0){capitalFlags|=CapitalHeart;QueueCapitalCg(2);}
   if(n.id==26&&!n.cleared&&!capitalLoading&&thunderChapterPracticeBoss<0&&capitalLetters==7&&(capitalFlags&CapitalHeart)!=0)TellStory(CapitalKingLines);
  }
  private void BuildCapitalActions(RoomNode n)
  {
   if(!n.cleared)return;
   if(n.id==0)AddChapterAction(1000,new Vector2(-3,1),"与港务员核对归航凭证",ResidentSprite(0));
   if(n.id==6)AddChapterAction(1001,new Vector2(-2,1),(capitalFlags&1)!=0?"查看居民撤离记录":"打开避难所，放行居民与粮车",CapitalProp(10));
   if(n.id==12)AddChapterAction(1002,new Vector2(0,1),"从档案柜取回原始航海名册",CapitalProp(3));
   if(n.id==15)AddChapterAction(1003,new Vector2(0,1),(capitalFlags&4)!=0?"总闸已停 · 核对疏散路线":"关闭贡纳泵站总闸",CapitalProp(9));
   if(n.id==21){int next=(capitalLetters&1)==0?0:(capitalLetters&2)==0?1:(capitalLetters&4)==0?2:0;AddChapterAction(1010+next,new Vector2(0,1),capitalLetters==7?"重读合订原件 · 三份记录":"翻阅合订原件 · "+CapitalLetterTitles[next],CapitalProp(4));}
   if(n.id==24)AddChapterAction(1004,new Vector2(0,1),"听留守近卫讲述国王迟迟未签的撤令",ResidentSprite(6));
   // The final reflection plays on entering the preparation room, after the bound archive is read.
   // Skill management remains on T; rest rooms need no duplicate document desk.
   if(n.stairsTo>=0){Vector2 at=new Vector2(6,2);AddChapterAction(1030,at,"前往"+AtlasRegions[6][journeyRooms[n.stairsTo].stage],CapitalProp(11));}
   if(n.id==26&&chapterStone){AddChapterAction(1041,new Vector2(-2,1),"保留第七束光 · 分还六片，踏上归途",CapitalProp(7));AddChapterAction(1042,new Vector2(2,1),"以心补光 · 让 NakiTong 重生",CapitalProp(7));}
   if(n.id==0||chapterEnding)AddChapterAction(1007,new Vector2(-6,-2),"展开群岛航图");
  }
  private void CapitalAction(int kind)
  {
   if(!journeyRooms[currentRoom].cleared)return;
   if(kind==1000)TellStory("归航凭证上的日期，与封港告示相差三天。港口还在为归来的船挂灯，锁链却早已奉命备好。","凭证背面夹着一张港务副本：受命者交还记录以前，返航船与随船家属一律不得离港。正本转存航务档案室。","少年把凭证折好，收进衣襟。先去下城，看看被留在名单上的人。 ");
   if(kind==1001){if((capitalFlags&CapitalRescue)!=0){TellStory("撤离记录上，老人、医师与随船家属的名字已经逐一核过。粮车从运粮小巷通过，避难所仍为需要休息的人留着门。");return;}if(!journeyRooms[3].cleared){JourneyHint("关税哨所仍在扣押居民 · 先解除哨所守军");return;}capitalFlags|=1;TellStory("少年用哨所钥匙打开侧门，又搬开运粮小巷的路障。老人先走，抬药箱的人随后，最后一辆粮车缓缓越过门槛。","守粮人核对名单，把空车留给还走不动的人。他问少年要把粮送去哪里。","少年：照你们原来的名单送。哪户在等，就先问哪户。","避难所的撤离记录已经留在门内。通往封港统领的道路现在可以安全通行。");}
   if(kind==1002){capitalFlags|=2;TellStory("誊本把‘归期’改成了‘下一次出征’。少年展开压在旧账册底下的原始名册，船员、医师和家属的名字一行也没有少。","暮钟岛粮船的三次召回，旁边都有同一枚王印。连已经失踪的人，也仍被列作契约的担保人。","少年把名册与自己的归航凭证并在一起：先带走原件。到了御前书房，再核对那份不能退出的契约。");}
   if(kind==1003){if((capitalFlags&CapitalControl)!=0){TellStory("检修记录：贡纳支路已关闭，供水支路保持运行。留守工匠把手摇泵的轮班表钉在总闸旁，水槽边仍有来取水的人。");return;}if(!journeyRooms[14].cleared){JourneyHint("先清理贡纳泵站，才能安全关闭总闸");return;}capitalFlags|=4;TellStory("总闸分出两条支路：粗铜管通往街区的水槽，细管则把贡纳之光送进王宫。少年按照旧检修图，只关掉了后者。","街口的水轮仍在转。留守工匠重新接上手摇泵，守粮人推着车从另一侧穿过。","王印失去一处供给，城里却没有因此停下来。少年把这件事记在原图背面，准备亲口告诉国王。");}
   if(kind==1004)TellStory("留守近卫：那份撤令我送进去过三次。暂停征召，放归返航船——正文每次都改过，签名的位置却一直空着。","一页被揉皱的批注写着：‘上一次听任地方停运，北区三日无水。若再放手，谁保证下一次不会更坏？’","少年展开泵站检修图，纸背还留着工匠沾水的手印。近卫看了许久，向王座厅让开一步：去把你亲眼见过的事告诉他。");
   if(kind==1005){if(capitalLetters!=7){JourneyHint("先在御前书房读完三份原件，再作最后决定");return;}capitalFlags|=8;QueueCapitalCg(2);}
   if(kind==1006){chapterSkillsMenu=true;return;}if(kind==1007){OpenWorldAtlas();return;}
   if(kind>=1010&&kind<=1012){capitalLetter=kind-1010;capitalPage=0;capitalArmed=false;capitalOpened=Time.unscaledTime;BlockWorldInteraction();return;}
   if(kind==1030){int target=journeyRooms[currentRoom].stairsTo;if(target>=0&&((target==9&&!journeyRooms[8].cleared)||(target==18&&!journeyRooms[17].cleared)))return;if(target>=0)EnterExplorationRoom(target,-1);}
   if(kind==1041||kind==1042){if(!chapterStone||chapterEnding||capitalEnding!=0)return;capitalConfirm=kind-1040;capitalArmed=false;capitalOpened=Time.unscaledTime;BlockWorldInteraction();return;}
   UpdateExplorationDoors();BuildChapterActions();RequestCheckpoint();SaveChapterCheckpoint(false);
  }
  private void CompleteCapitalBoss()
  {
   foreach(var e in enemies){if(e.root!=null)Destroy(e.root);if(e.shadow!=null)Destroy(e.shadow);if(e.warning!=null)Destroy(e.warning);}enemies.Clear();
   foreach(var p in pendingSpawns)if(p.mark!=null)Destroy(p.mark);pendingSpawns.Clear();foreach(var s in shots)if(s.root!=null)Destroy(s.root);shots.Clear();ClearCapitalCombat();ClearThunderHazards();ClearBossSignatures();
   ClearJourneyRoom();health=Mathf.Max(1,health);
   if(currentRoom==26){chapterStone=true;QueueCapitalCg(3);}else TellStory(currentRoom==8?"统领跪倒在断裂的弩机前，封港令从盾后滑落。守粮人带着撤离记录走上台阶，守军终于让开了粮车的道路。":"三道王印柱熄灭。执律官扶住石台，第一次看清契约原件上的删改。","通往"+AtlasRegions[6][currentRoom==8?1:2]+"的门已打开。沿唯一的楼梯继续前进。");
   BuildChapterActions();RequestCheckpoint();SaveChapterCheckpoint(false);
  }
 }
}
