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
    if(next==44&&(manorProgress&7)!=7)return true;
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
   if(n.stage!=1||n.kind==RoomKind.Combat||n.kind==RoomKind.Elite||n.kind==RoomKind.Boss)return;
   var t=Resources.Load<Texture2D>("NakiArt/LegacyV1/refuge");if(t==null)return;
   Vector2 at=new Vector2(-5,3);var sprite=Sprite.Create(t,new Rect(0,0,t.width,t.height),new Vector2(.5f,.14f),t.width/3.4f,0,SpriteMeshType.FullRect);ownedAssets.Add(sprite);Draw("Sheltered village house",sprite,at,Vector2.one,C("b1b3b7"),DepthAt(at.y));
   Rect footprint=new Rect(-6.45f,3,2.9f,1.5f);obstacles.Add(footprint);projectileObstacles.Add(footprint);
  }
  private void BuildManorLaterActions(RoomNode n)
  {
   if(n.id>=15&&n.cleared){
    if(n.kind==RoomKind.Reward&&!n.claimed)AddChapterAction(920,new Vector2(0,-1),"查看府邸封存物资",ManorSprite("prop-5",1.8f));
    if(n.id==15)AddChapterAction(610,new Vector2(-2,0),"沿旧村门楼返回归功外庭",ManorSprite("prop-9",2));
    if(n.id==17)AddChapterAction(611,new Vector2(-2,0),"将粮仓道路开放给守粮人",ResidentSprite(6));
    if(n.id==19)AddChapterAction(612,new Vector2(-2,0),"将转运间的药箱交给医师",ResidentSprite(2));
    if(n.id==24)AddChapterAction(613,new Vector2(-2,0),"查阅淬炼档案与旧委任书",ManorSprite("prop-7",1.8f));
    if(n.id==22)AddChapterAction(614,new Vector2(-2,0),"查看栽种者留下的空种盒",ResidentSprite(5));
    if(n.id==29)AddChapterAction(615,new Vector2(3,-1),"沿解开的封印进入无昼内庭",ManorSprite("prop-9",2));
    if(n.id==30)AddChapterAction(616,new Vector2(-2,0),"沿长阶返回断粮旧村",ManorSprite("prop-9",2));
    if(n.id==32||n.id==34||n.id==41)AddChapterAction(617,new Vector2(-2,0),"核对前代勇者留下的记录",ManorSprite("prop-7",1.6f));
    if(n.id==23||n.id==38||n.id==39)AddChapterAction(620,new Vector2(-2,0),"查看这里留下的生活痕迹",ManorSprite("prop-7",1.4f));
    if(n.id==28)AddChapterAction(619,new Vector2(-2,0),n.claimed?"查看工坊行路记录":"工坊整备 · 本处补给一次",ManorSprite("prop-5",1.8f));
    if(n.id==43)AddChapterAction(618,new Vector2(-2,0),n.claimed?"重读内庭前的行路记录":"内庭整备 · 本处补给一次",ManorSprite("prop-5",1.8f));
    if(n.id==44&&manorEnding==0){
     if(origin==Origin.Light)for(int i=0;i<3;i++)if((manorProgress&(8<<i))==0)AddChapterAction(720+i,new Vector2(-3+i*3,1.2f),"以光拆解王印契约 · "+(i+1)+" / 3",ManorSprite("prop-6",1.25f));
     else AddChapterAction(723,new Vector2(0,1.2f),"查看显露的契约 · 决定完整意志的代价",ManorSprite("prop-7",1.8f));
    }
   }
  }
  private bool ManorFinaleAction(int action)
  {
   if(action<610||action>723)return false;
   if(action==610){EnterExplorationRoom(14,-1);return true;}
   if(action==611){if(!journeyRooms[18].cleared){TellStory("守粮人：粮仓在街巷那头，巡卫还扣着车。先让运粮的人能够经过，我就开锅。 ");return true;}manorProgress|=1;TellStory("粮车驶进施粥屋的院门。守粮人揭开最后一袋麦，先给排队的人分了热汤。","守粮人：暮钟的主人也曾把自己的口粮放在这里。后来，所有粮车都被改送工坊了。");}
   else if(action==612){if(!journeyRooms[20].cleared){TellStory("医师：药箱扣在转运间，病人不该再等那份受封公文。 ");return true;}manorProgress|=2;TellStory("医师把霜潮群岛寄来的药单与箱封对上，开门让病人进屋。","医师：他以前把药留给别人。后来，他开始要求我们证明，自己值得被留下。");}
   else if(action==613){manorProgress|=4;TellStory("委任书、佩剑编号与纪念像上的旧刻痕终于吻合。府邸主人，就是上一位出海的少年。","工坊记录：战斗余烬只是残响；完整淬炼会夺走一个人的意志。右眼的石头是人工容器，骑士体内才是真正的元素源石。","国王的契约刻进了容器：每一代受封者都要留在这里，直至找到下一位承受者。","少年：粮道已经开，药也送到了。先让这个命令停下来。");}
   else if(action==614)TellStory("栽种者：青栖林海寄来的空种盒还在。他说等最后一块碎片找齐，就陪我们把庭院种满。","木盒底层压着一封没有寄出的家书：‘我还记得来时的海风。’");
   else if(action==615){if(!journeyRooms[29].cleared)return true;EnterExplorationRoom(30,-1);TellStory("长阶尽头没有白昼。少年带上粮道、药单和淬炼档案，推开受封者留下的门。");return true;}
   else if(action==616){EnterExplorationRoom(29,-1);return true;}
   else if(action==617)TellStory("航海桌上摆着旧修渠拓印、霜潮船单、赤炉货单和北方种盒。那是同一条航路。","大骑士的笔迹：‘我以为只要再强一点，就不必再失去任何人。’","少年：我们来过同样的地方。但还没有走完同一条路。");
   else if(action==618){if(!journeyRooms[43].claimed){journeyRooms[43].claimed=true;health=Mathf.Min(6,health+2);shield=EffectiveMaximumShield;elementPower=Mathf.Min(ElementMaximum,elementPower+40);}TellStory("门内传来铠甲擦过石面的声音。","大骑士：他这一次，还是让一个孩子出海。","少年：我会带着碎片回去，也会带着这些记录回去。","大骑士：那就让我看看，你能不能替自己的决定站稳。");}
   else if(action==619){var n=journeyRooms[currentRoom];if(!n.claimed){n.claimed=true;health=Mathf.Min(6,health+1.5f);shield=Mathf.Min(EffectiveMaximumShield,shield+2);elementPower=Mathf.Min(ElementMaximum,elementPower+30);}TellStory("工坊的炉子终于停下来。把工具留给仍在修补房屋的人，少年沿着掌印前室继续向里。 ");}
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
   if(journeyRooms[44].cleared)return origin==Origin.Light?"拆解三处王印契约 · 取回真正的元素源石":"右眼的容器与契约相连 · 靠近档案作出抉择";
   return "经旧航海桌与王印档案深入 · 在内庭整备间休整后面见大骑士";
  }
  private void ResolveManorFinalBoss(Enemy e,ManorCaptainState state)
  {
   int move=e.attackNumber%5,phase=e.difficultyPhase;bool knight=e.kind==EnemyKind.ManorKnight;e.actionStarted=clock;e.recoveryUntil=clock+.65f;
   if(move==1){state.contact=false;e.chargeUntil=clock+(knight?.8f:.6f);state.bursts=knight?2:1;state.nextBurst=clock+.25f;Sfx(knight?"spear":"hammer",false,.7f);return;}
   if(move==2||move==3){Vector2 side=new Vector2(-e.attackDirection.y,e.attackDirection.x);int count=phase>0?3:2;for(int i=0;i<count;i++)QueueManorImpact(e,NearestEnemyFooting(e.strikePoint+side*(i-(count-1)*.5f)*2.3f,.4f),knight?1.25f:1.5f,1.5f,.85f+i*.25f,knight&&move==3);if(knight&&move==2)AddSignatureZone(e,2,e.position,e.attackDirection,6,1.2f);Sfx(knight?"stone":"hammer",false,.65f);return;}
   if(move==4){state.shieldUntil=clock+1.2f;e.tacticRole=5;state.bursts=2;state.nextBurst=clock+.35f;AddFloatingHit(knight?"誓甲反击 · 绕侧":"王印蓄势 · 绕侧",e.position+Vector2.up,C("c8a7e8"));return;}
   int rays=knight?16+phase*3:12+phase*3;for(int i=0;i<rays;i++){float angle=(i+.5f)*Mathf.PI*2/rays;Vector2 dir=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));PressureFan(e.position+dir*.8f,dir,1,0,knight?4.8f:4.2f,knight?Element.Dark:Element.Physical,1.3f);}state.bursts=2+phase;state.nextBurst=clock+.4f;
  }
  private void CompleteManorLaterBoss()
  {
   int room=currentRoom;foreach(var e in enemies){if(e.root!=null)Destroy(e.root);if(e.shadow!=null)Destroy(e.shadow);if(e.warning!=null)Destroy(e.warning);}enemies.Clear();foreach(var pending in pendingSpawns)if(pending.mark!=null)Destroy(pending.mark);pendingSpawns.Clear();ClearManorImpacts();ClearThunderHazards();ClearBossSignatures();ClearJourneyRoom();health=Mathf.Max(1,health);BuildChapterActions();UpdateExplorationDoors();
   TellStory(room==29?"掌印停在半空，旧命令从铜壳里一片片脱落。粮道与内庭之间的封锁解除了。":"重剑落地，铠甲裂开。与五岛记录相同的王印，终于完整显露。",room==29?"少年收起掌印器的核心，把通往内庭的阶梯留给仍在等待的人。":origin==Origin.Light?"大骑士：我也曾想带所有人回去。少年伸出手：先让这道命令停下。以光拆解三处契约。":"大骑士：这里需要的不是余烬，是一个完整的意志。你眼中的人工容器，也被刻着同一条命令。",room==44&&origin==Origin.Dark?"少年看向旧村的灯。村民，或是自己……选择前，先看清每一种代价。":"已打开安全点，可整理装备、保存或继续探索。");
   RequestCheckpoint();SaveChapterCheckpoint(false);
  }
  private void BeginManorEnding(int ending)
  {
   if(manorEnding!=0)return;manorEnding=ending;manorEndingSeen=false;manorChoiceMode=0;storyLines=null;chapterCg=true;BlockWorldInteraction();
   if(ending==1){TellStory("最后一枚王印熄灭。少年取回真正的元素源石，把完整的委任记录与原令一起收进行囊。","大骑士：不知道轮回过的灵魂，是否还懂得死亡。少年：至少，活着的人应该有回家的路。");SaveChapterCheckpoint(false);}
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
   FittedLabel(new Rect(w*.12f,h*.27f,w*.76f,130),"铠甲已破，契约却仍能重新驱动它。储存的战斗余烬无法替代完整意志。你的贤者之石能量不会被清空，所有积攒与战斗仍然有效。\n解除这一道契约，却需要有人承担它。旧村的居民，或者你自己。",journeyTextStyle,true);
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
   string line=panel==0?"六片碎片、未寄出的家书与王室原令，一同踏上归途。本土王国的对质尚待下一章。":panel==1?"少年：这里还有一人份。但他们不该为我而死。空甲仍然站立，少年终于倒在来时的石阶上。": "新骑士披上黑甲。旧村的门没有再打开；新的委任书，又在海的另一端盖上王印。";
   FittedLabel(new Rect(30,h-145,w-60,42),title,journeyTitleStyle);FittedLabel(new Rect(30,h-95,w-60,55),line,journeyTextStyle,true);FittedLabel(new Rect(30,h-33,w-60,30),manorEndingSeen&&panel>0?"F / R 返回抉择前安全点 · 本结局已独立记录":"F / ESC 继续",journeySmallStyle);
  }
  private void FinishManorEnding()
  {
   chapterCg=false;manorEndingSeen=true;BlockWorldInteraction();if(manorEnding==1){chapterEnding=true;BuildChapterActions();RequestCheckpoint();SaveChapterCheckpoint(false);}
  }
 }
}
