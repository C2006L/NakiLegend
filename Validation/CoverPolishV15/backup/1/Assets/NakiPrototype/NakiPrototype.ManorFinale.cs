using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  // Existing outer-court IDs, save key and weapon IDs remain stable.
  private int manorProgress,manorEnding,manorChoice,manorChoiceMode;
  private bool manorEndingSeen;
  private float manorChoiceOpened;
  private static readonly string[][] ManorLaterNames={
   new[]{"旧村门楼","断粮街巷","施粥屋","扣押粮仓","医师诊所","药物转运间","淬炼管道","空种盒庭","守粮人旧屋","封印档案室","工坊回廊","钟楼驻弓台","残响试验间","掌印前室","掌印机关"},
   new[]{"受封长阶","黑旗回廊","旧航海桌","沉誓武备庭","王印档案","契约柱廊","双弩夹道","祭师静室","废誓花园","断链方庭","无昼回廊","旧村求援信","碎冠长廊","内庭整备间","暗黑大骑士"}};
  private void ExtendManorGraph()
  {
   Array.Resize(ref chapterNames,45);
   for(int floor=1;floor<3;floor++)for(int local=0;local<15;local++){
    int id=floor*15+local;RoomKind kind=local==14?RoomKind.Boss:local==0||local==13?RoomKind.Rest:local==3||local==11?RoomKind.Elite:local==7?RoomKind.Reward:local==2||local==4||local==8||local==9?RoomKind.Event:RoomKind.Combat;
    if(floor==2&&local==2)kind=RoomKind.Event;
    var n=new RoomNode{id=id,stage=floor,floor=floor,depth=22+floor*3+local/5,cell=new Vector2Int(local%5,-local/5),kind=kind,halfWidth=9.6f+floor*.4f,halfHeight=6,layout=(activeSeed+local+floor)%6,waves=kind==RoomKind.Boss?1:kind==RoomKind.Elite?4:kind==RoomKind.Combat?3:0,stairsTo=-1};
    journeyRooms.Add(n);chapterNames[id]=ManorLaterNames[floor-1][local];
    if(local>0&&local%5!=0)ConnectRooms(id-1,id);
    if(local>=5)ConnectRooms(id-5,id);
   }
   journeyRooms[14].stairsTo=15;journeyRooms[15].stairsTo=14;journeyRooms[29].stairsTo=30;journeyRooms[30].stairsTo=29;
   // A single final approach: the optional east room must not connect straight into the knight.
   journeyRooms[39].links.Remove(44);journeyRooms[44].links.Remove(39);
   chapterNames[32]="旧主人书房";journeyRooms[34].kind=RoomKind.Elite;journeyRooms[34].waves=4;journeyRooms[41].kind=RoomKind.Reward;journeyRooms[41].waves=0;
   stageStarts[0]=0;stageStarts[1]=15;stageStarts[2]=30;stageEnds[0]=14;stageEnds[1]=29;stageEnds[2]=44;
  }
  private void MigrateManorCheckpoint(ChapterCheckpoint s)
  {
   if(!manorCourtyardMode||s==null||s.island!="manor_courtyard"||s.map!="manor-courtyard-v2"||s.rooms==null||s.rooms.Length!=15)return;
   Array.Resize(ref s.rooms,45);for(int i=15;i<45;i++)s.rooms[i]=new ChapterRoomState{weapons=new CheckpointWeapon[0],crystals=new CheckpointEssence[0]};s.map="manor-three-v3";
  }
  private bool ManorLaterGateLocked(int room,int direction)
  {
   foreach(int next in journeyRooms[room].links)if(NeighborDirection(journeyRooms[room],journeyRooms[next])==direction){
    if(journeyRooms[room].stage==1&&!journeyRooms[14].cleared)return true;
    if(next==29&&(manorProgress&7)!=7)return true;
    if(journeyRooms[room].stage==2&&!journeyRooms[29].cleared)return true;
    if(next==44&&((manorProgress&7)!=7||manorLetters!=7))return true;
   }return false;
  }
  private void LoadManorFinaleArt()
  {
   var t=Resources.Load<Texture2D>("NakiArt/ManorFinaleV3/bosses");if(t==null)throw new InvalidOperationException("Missing Manor finale boss art");t.filterMode=FilterMode.Point;
   for(int row=0;row<2;row++){var frames=new Sprite[8];for(int f=0;f<8;f++){
    int pose=f==4||f==5?2:f==6||f==7?3:f%2;Rect cell=new Rect(pose*t.width/4f,(1-row)*t.height/2f,t.width/4f,t.height/2f);
    frames[f]=Sprite.Create(t,cell,new Vector2(.5f,.09f),cell.height/3.1f,0,SpriteMeshType.FullRect);frames[f].name=row==0?"Royal seal automaton":"Dark great knight";ownedAssets.Add(frames[f]);}
    enemyActionArt[row==0?EnemyKind.ManorSeal:EnemyKind.ManorKnight]=frames;
   }
  }
  private void BuildManorLaterScenery(RoomNode n)
  {
   if(n.stage!=1)return;
   if(n.id==17||n.id==19||n.id==23)ManorVillageBuilding(n.id==19?1:0,new Vector2(-4.7f,2.2f),4.6f);
   if(n.id==18||n.id==20)ManorVillageBuilding(2,new Vector2(-4.7f,2.2f),4.4f);
  }
  private void BuildManorLaterActions(RoomNode n)
  {
   if(n.id>=15&&n.cleared){
    if(n.id==32)BuildManorStudy();
    if(n.kind==RoomKind.Reward&&!n.claimed)AddChapterAction(920,new Vector2(2,-1),"查看府邸封存物资",ManorSprite("prop-5",1.8f));
    if(n.id==15)AddManorPassage(610,14);
    if(n.id==17)AddChapterAction(611,new Vector2(-4.7f,.9f),(manorProgress&1)!=0?"与守粮人交谈":"告知守粮人粮仓的情况",ResidentSprite(6));
    if(n.id==19)AddChapterAction(612,new Vector2(-4.7f,.9f),(manorProgress&2)!=0?"与医师交谈":"核对医师的药单",ResidentSprite(2));
    if(n.id==24)AddChapterAction(613,new Vector2(-2,0),"查阅封印档案",ManorVillageProp(4,2.1f));
    if(n.id==22)AddChapterAction(614,new Vector2(-2,0),"查看旧种盒与未寄出的信",ManorVillageProp(5,1.7f));
    if(n.id==29)AddManorPassage(615,30);
    if(n.id==30)AddManorPassage(616,29);

    if(n.id==23||n.id==38||n.id==39)AddChapterAction(620,new Vector2(-2,0),n.id==23?"阅读守粮人的留言":n.id==38?"查看荒园里的旧花盆":"辨认断链旁的刻痕",n.id==38?ManorVillageProp(5,1.7f):ManorVillageProp(4,1.8f));
    if(n.id==28)AddChapterAction(619,new Vector2(-2,0),n.claimed?"查看工坊行路记录":"工坊整备 · 本处补给一次",ManorSprite("prop-5",1.8f));
    if(n.id==43)AddChapterAction(618,new Vector2(-2,0),n.claimed?"重读内庭前的行路记录":"内庭整备 · 本处补给一次",ManorSprite("prop-5",1.8f));
    if(n.id==44&&manorEnding==0&&!manorVictoryCg){
     if(origin==Origin.Light){for(int i=0;i<3;i++)if((manorProgress&(8<<i))==0)AddChapterAction(720+i,new Vector2(-3+i*3,1.2f),"以光剥离王印 · "+(i+1)+" / 3",ManorSprite("prop-6",1.25f));}
     else AddChapterAction(723,new Vector2(0,1.2f),"查看铠甲上显露的契约",ManorVillageProp(4,1.8f));
    }
   }
  }
  private bool ManorFinaleAction(int action)
  {
   if(action>=730&&action<=732){manorLetter=action-730;manorLetterPage=0;BlockWorldInteraction();return true;}
   if(action<610||action>723)return false;
   if(action==610){EnterExplorationRoom(14,-1);return true;}
   if(action==611){if((manorProgress&1)!=0)TellStory("守粮人：锅里还有热的。今晚，大家总算不用空着肚子听钟了。");else if(!journeyRooms[18].cleared){TellStory("守粮人：粮仓里不是没有粮。巡卫把车扣在街那头，已经三天了。","他扶了扶空锅：只要车能过来，这里就还能开火。");return true;}else{manorProgress|=1;TellStory("卸下封条的粮车停在屋前。守粮人捧起一把麦，许久才松开手。","守粮人：他刚来的时候，也总把自己的那份放在这里。后来，工坊的钟越来越响，粮车却不再往村里走了。");}}
   else if(action==612){if((manorProgress&2)!=0)TellStory("医师：药已经分下去了。等咳声轻些，我想把窗打开，让他们听听外面的风。");else if(!journeyRooms[20].cleared){TellStory("医师：药从霜潮群岛送来，就锁进转运间了。批准的文书还在路上，屋里的人却不能再等。");return true;}else{manorProgress|=2;TellStory("医师核对药箱上的船印，划掉药单上一个个等待的名字。","医师：以前，那位受封的少年也是这样送药来的。他后来再没跨进这扇门。可每逢钟响，我还是会抬头看一眼。");}}
   else if(action==613){manorProgress|=4;TellStory("档案里夹着一份受封名册。名字旁的佩剑编号，与外庭石像上残留的刻痕相同。","附页写着：‘战斗残响可作燃料，不能替代完整意志。受封者右眼所藏为容器；元素源石另封于誓甲。’","纸背的王印仍很清楚：‘受封者不得擅离。契约可由继任者承接。’","少年把它与自己的委任书并在一起。两封信，盖的是同一枚印。");}
   else if(action==614)TellStory("木盒里只剩干土。底下压着青栖林海寄来的种单，还有一封没有封口的家书。","‘等最后一片找齐，我就回来。院子别急着种，给我留一小块。’","信末的墨被水洇开了。少年合上盒盖，没有把它带走。");
   else if(action==615){if(!journeyRooms[29].cleared)return true;EnterExplorationRoom(30,-1);TellStory("门后的长阶没有风。少年回头望见旧村亮起的窗，再将那份王印原令收进衣襟。");return true;}
   else if(action==616){EnterExplorationRoom(29,-1);return true;}
   else if(action==617){if(currentRoom==32)TellStory("航海手记的页边夹满了船票。鸣沙洲的修渠拓印、雷雨高原的巡路图，排成少年熟悉的顺序。","最后一页写着：‘回航那天，他们叫我勇者。我只想知道，家里那盏灯还亮不亮。’");else if(currentRoom==34)TellStory("原令附着一张淬炼图。容器的纹路从右眼延伸到誓甲，最终汇进王冠形的印记。","朱笔批注：‘契约不随肉身终止。前任若亡，由承接者续任。’","少年：所以那份封赏，从来没有写归期。");else TellStory("求援信写在药单背面：‘粮车停了。孩子们问，府邸里的勇者为什么不出来。’","信被退回三次。最后一次，封口上只有四个字：‘不得离庭。’","少年重新折好信。这一次，他知道该把它带给谁。");}
   else if(action==618){if(!journeyRooms[43].claimed){journeyRooms[43].claimed=true;health=Mathf.Min(6,health+2);shield=EffectiveMaximumShield;elementPower=Mathf.Min(ElementMaximum,elementPower+40);}TellStory("门内传来铠甲擦过石面的声音。","大骑士：他这一次，还是让一个孩子出海。","少年：我会带着碎片回去，也会带着这些记录回去。","大骑士：那就让我看看，你能不能替自己的决定站稳。");}
   else if(action==619){var n=journeyRooms[currentRoom];if(!n.claimed){n.claimed=true;health=Mathf.Min(6,health+1.5f);shield=Mathf.Min(EffectiveMaximumShield,shield+2);elementPower=Mathf.Min(ElementMaximum,elementPower+30);}TellStory("工具箱里还留着干净绷带和几枚未耗尽的晶石。少年补好护具，把剩下的东西收回原处。","隔墙传来机械翻转的声响。旧命令还在运转。");}
   else if(action==620)TellStory(currentRoom==23?"门后的饭碗洗得很干净。墙上还留着一行旧字：‘受封的少年说过，会把粮道打开。’":currentRoom==38?"花园里留下五种来自不同岛屿的旧花盆。没有一只花盆被当作淬炼容器。": "断链旁有一道被反复划过的刻痕：‘如果回来的人已经不是我，别让他替你作决定。’");
   else if(action>=720&&action<=722){if(origin!=Origin.Light||!journeyRooms[44].cleared)return true;manorProgress|=8<<(action-720);SpellSpark(position,C("ffe4a1"),1.1f);if((manorProgress&56)==56)BeginManorEnding(1);else TellStory("光穿过铠甲裂隙，剥离人工契约上的一道命令。那块元素源石，终于不再被王印遮住。");}
   else if(action==723){if(origin!=Origin.Dark||!journeyRooms[44].cleared)return true;SaveChapterCheckpoint(false);manorChoiceMode=1;manorChoice=0;manorChoiceOpened=Time.unscaledTime;CancelBowCharge();}
   else return false;
   BuildChapterActions();UpdateExplorationDoors();RequestCheckpoint();SaveChapterCheckpoint(false);return true;
  }
  private string ManorFullObjective()
  {
   if(manorEnding!=0)return manorEnding==1?"契约已解除 · 带六片碎片和证据返航":"结局已记录 · F9 返回抉择前安全点";
   int floor=journeyRooms[currentRoom].stage;
   if(floor==0)return chapterStone?"卫长已卸武 · 调查旧村封门，前往断粮旧村":null;
   if(!journeyRooms[14].cleared)return "返回归功外庭 · 先取得卫长的通行印";
   if(floor==1){if((manorProgress&1)==0)return "清出扣押粮仓 · 去施粥屋交还粮道";if((manorProgress&2)==0)return "清出药物转运间 · 将药箱送到医师诊所";if((manorProgress&4)==0)return "前往封印档案室 · 查明完整淬炼与旧委任书";return journeyRooms[29].cleared?"掌印封锁已解除 · 沿阶梯进入无昼内庭":"穿过掌印前室 · 停下掌印机关";}
   if(!journeyRooms[29].cleared)return "先解除断粮旧村的掌印封锁";
   if(!journeyRooms[44].cleared&&manorLetters!=7)return "前往旧主人书房 · 阅读三封书信（"+((manorLetters&1)+((manorLetters>>1)&1)+((manorLetters>>2)&1))+" / 3）";
   if(journeyRooms[44].cleared)return origin==Origin.Light?"拆解三处王印契约 · 取回真正的元素源石":"右眼的容器与契约相连 · 靠近档案作出抉择";
   return "信中往事已明 · 经内庭整备间进入大骑士房间";
  }
  private void ResolveManorFinalBoss(Enemy e,ManorCaptainState state)
  {
   int move=e.attackNumber%5,phase=e.difficultyPhase;bool knight=e.kind==EnemyKind.ManorKnight;e.actionStarted=clock;e.recoveryUntil=clock+.65f;
   if(knight&&(move==2||move==4))BeginDuelMechanic(e,16);
   if(move==1){state.contact=false;e.chargeUntil=clock+(knight?.8f:.6f);state.bursts=knight?2:1;state.nextBurst=clock+.25f;Sfx(knight?"spear":"hammer",false,.7f);return;}
   if(move==2||move==3){Vector2 side=new Vector2(-e.attackDirection.y,e.attackDirection.x);int count=phase>0?3:2;for(int i=0;i<count;i++)QueueManorImpact(e,NearestEnemyFooting(e.strikePoint+side*(i-(count-1)*.5f)*2.3f,.4f),knight?1.25f:1.5f,1.5f,.85f+i*.25f,knight&&move==3);if(knight&&move==2)AddSignatureZone(e,2,e.position,e.attackDirection,6,1.2f);Sfx(knight?"stone":"hammer",false,.65f);return;}
   if(move==4){state.shieldUntil=clock+1.2f;e.tacticRole=5;state.bursts=2;state.nextBurst=clock+.35f;AddFloatingHit(knight?"誓甲反击 · 绕侧":"王印蓄势 · 绕侧",e.position+Vector2.up,C("c8a7e8"));return;}
   BeginDuelMechanic(e,knight?16:15);
   int rays=knight?16+phase*3:12+phase*3;for(int i=0;i<rays;i++){float angle=(i+.5f)*Mathf.PI*2/rays;Vector2 dir=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));PressureFan(e.position+dir*.8f,dir,1,0,knight?4.8f:4.2f,knight?Element.Dark:Element.Physical,1.3f);}state.bursts=2+phase;state.nextBurst=clock+.4f;
  }
  private void CompleteManorLaterBoss()
  {
   int room=currentRoom;foreach(var e in enemies){if(e.root!=null)Destroy(e.root);if(e.shadow!=null)Destroy(e.shadow);if(e.warning!=null)Destroy(e.warning);}enemies.Clear();foreach(var pending in pendingSpawns)if(pending.mark!=null)Destroy(pending.mark);pendingSpawns.Clear();ClearManorImpacts();ClearThunderHazards();ClearBossSignatures();ClearJourneyRoom();health=Mathf.Max(1,health);BuildChapterActions();UpdateExplorationDoors();
   if(room==29)TellStory("掌印机关的铜臂缓缓垂下，再也盖不出下一道命令。门闩退进墙里，长阶从阴影中显露。","少年：村里的路已经通了。现在，该问问写下这些命令的人了。");
   else if(origin==Origin.Light)TellStory("重剑落地，铠甲的裂隙里露出三道仍在发亮的王印。空甲深处，传来断续的声音。","铠甲里的残响：我也曾想……带所有人回去。","少年抬起手。掌心的光没有钻进铠甲，而是落在那些束缚它的刻纹上。");
   else TellStory("重剑落地，断开的铠片却又被紫纹一点点牵回。少年右眼里的石头随之发烫。","铠甲里的残响：余烬能使它倒下，却不能让命令停下。它在等……下一个完整的意志。","少年望向旧村的方向。那里的窗刚刚重新亮起。");
   if(room==44&&!manorVictorySeen){storyLines=null;manorVictoryCg=true;BuildChapterActions();}
   RequestCheckpoint();SaveChapterCheckpoint(false);
  }
  private void BeginManorEnding(int ending)
  {
   if(manorEnding!=0)return;manorEnding=ending;manorEndingSeen=false;manorChoiceMode=0;storyLines=null;chapterCg=true;BlockWorldInteraction();
   if(ending==1){TellStory("最后一枚王印熄灭。空甲失去支撑，轻轻落在石阶上。那块被囚禁的元素源石，终于回到少年掌心。","残响散去前，留下最后一句：‘那盏灯……还亮着吗？’","少年：我会把信带回去。");SaveChapterCheckpoint(false);}
   else if(!Application.isBatchMode||allowBatchCheckpoints){try{File.WriteAllText(ChapterCheckpointPath+".ending-"+ending+".json",JsonUtility.ToJson(new ManorEndingRecord{ending=ending,seed=activeSeed,stone=darkPower},true));}catch(Exception){JourneyHint("结局记录未写入；抉择前安全点仍保留");}}
  }
  [Serializable]private sealed class ManorEndingRecord{public int ending,seed;public float stone;}
  private bool TickManorFinaleChoice()
  {
   if(!manorCourtyardMode)return false;
   if(manorEnding>=2&&manorEndingSeen){if(Pressed('f')||Pressed('r')){LoadChapterCheckpoint(true);BlockWorldInteraction();}return true;}
   if(manorChoiceMode==0)return false;
   if(Pressed('e')){manorChoiceMode=0;BlockWorldInteraction();return true;}
   if(manorChoiceMode==1&&(Pressed('a')||Pressed('d')))manorChoice=1-manorChoice;
   if(Pressed('f')&&Time.unscaledTime>manorChoiceOpened+.3f){if(manorChoiceMode==1){manorChoiceMode=2;manorChoiceOpened=Time.unscaledTime;}else BeginManorEnding(manorChoice==0?2:3);}return true;
  }
  private void DrawManorFinaleChoice(float w,float h)
  {
   HudRect(new Rect(0,0,w,h),C("111321"));FittedLabel(new Rect(w*.12f,h*.15f,w*.76f,50),manorChoiceMode==2?"最后确认 · 完整意志的代价":"一人份的贤者之石",journeyTitleStyle);
   FittedLabel(new Rect(w*.12f,h*.27f,w*.76f,130),"紫纹重新缝合破甲，右眼里的容器与它一同跳动。那些积攒的余烬无法替你回答：要继续站在这里，谁来付出一个完整的生命？\n旧村的灯还亮着。你也仍然活着。",journeyTextStyle,true);
   string[] options={"拒绝淬炼村民 · 不以无辜者换取力量\n少年最终败北，触发《一人份的贤者之石》","完整淬炼旧村居民 · 接受骑士之位\n村民失去生命，少年成为下一任暗黑大骑士"};
   for(int i=0;i<2;i++)if(FlatButton(new Rect(w*.12f+i*w*.39f,h*.49f,w*.37f,125),options[i],manorChoice==i)&&manorChoiceMode==1)manorChoice=i;
   FittedLabel(new Rect(w*.12f,h*.79f,w*.76f,60),manorChoiceMode==1?"A / D 选择 · F 查看后果并确认 · ESC 暂不选择":"已明确后果。再次按 F 执行当前选择；ESC 取消。抉择前安全点保留。",journeySmallStyle,true);
  }
  private void DrawManorEnding(float w,float h)
  {
   HudRect(new Rect(0,0,w,h),Color.black);int panel=manorEnding==1?0:manorEnding==2?1:2;
   string artName=panel==0?"01-return":panel==1?"02-one-soul":"03-last-knight";
   var art=Resources.Load<Texture2D>("NakiArt/ManorFinaleV3/"+artName);
   if(art!=null)GUI.DrawTexture(new Rect(0,0,w,Mathf.Max(1,h-155)),art,ScaleMode.ScaleToFit);
   else{art=Resources.Load<Texture2D>("NakiArt/ManorFinaleV3/endings");if(art!=null){float height=Mathf.Min(h-155,w/(art.width/(art.height/3f)));GUI.DrawTextureWithTexCoords(new Rect(0,(h-155-height)*.5f,w,height),art,new Rect(0,(2-panel)/3f,1,1/3f));}}
   string title=panel==0?"归途尚未结束":panel==1?"一人份的贤者之石":"屠魔的少年，成为最后的魔";
   string line=panel==0?"六片碎片、未寄出的家书与王室原令，一同踏上归途。海的另一端，还有人等着他，也有人欠一个回答。":panel==1?"少年：这里还有一人份。但他们不该为我而死。空甲仍然站立，少年倒在来时的石阶上。": "新骑士披上黑甲。旧村的门没有再打开；新的委任书，又在海的另一端盖上王印。";
   FittedLabel(new Rect(30,h-145,w-60,42),title,journeyTitleStyle);FittedLabel(new Rect(30,h-95,w-60,55),line,journeyTextStyle,true);FittedLabel(new Rect(30,h-33,w-60,30),manorEndingSeen&&panel>0?"F / R 返回抉择前安全点 · 本结局已独立记录":"F / ESC 继续",journeySmallStyle);
  }
  private void FinishManorEnding()
  {
   chapterCg=false;manorEndingSeen=true;BlockWorldInteraction();if(manorEnding==1){chapterEnding=true;BuildChapterActions();RequestCheckpoint();SaveChapterCheckpoint(false);}
  }
 }
}
