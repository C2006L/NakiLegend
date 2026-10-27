using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private GUIStyle journeyNodeStyle;
  private string journeyToast;private float journeyToastUntil;
  private void EnsureJourneyStyles()
  {
   if(journeyTitleStyle!=null)return;journeyFont=Resources.Load<Font>("Fonts/FusionPixel");
   journeyTitleStyle=NewLabel(24,C("f4db9a"),FontStyle.Normal);journeyTextStyle=NewLabel(20,C("f3eddd"),FontStyle.Normal);journeySmallStyle=NewLabel(16,C("ccd8d7"),FontStyle.Normal);
   journeyButtonStyle=new GUIStyle(GUI.skin.button){fontSize=20,wordWrap=true,alignment=TextAnchor.MiddleCenter};journeyNodeStyle=new GUIStyle(journeySmallStyle){alignment=TextAnchor.MiddleCenter};
   foreach(var s in new[]{journeyTitleStyle,journeyTextStyle,journeySmallStyle,journeyButtonStyle,journeyNodeStyle})s.font=journeyFont;
  }
  private void CenterLabel(Rect box,string text,GUIStyle style){GUI.Label(box,text,new GUIStyle(style){alignment=TextAnchor.MiddleCenter,wordWrap=true});}
  private void PowerBar(Rect box,float value,Color color){HudRect(box,C("30404a"));HudRect(new Rect(box.x,box.y,box.width*Mathf.Clamp01(value/100),box.height),color);}
  private void DrawJourneyHud()
  {
   if(world==null||journeyRooms.Count==0)return;EnsureJourneyStyles();var saved=GUI.matrix;var old=GUI.color;
   float scale=Mathf.Max(.25f,Mathf.Min(Screen.width/1280f,Screen.height/720f));GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));GUI.color=Color.white;
   float w=Screen.width/scale,h=Screen.height/scale;Color panel=new Color(.035f,.065f,.09f,.95f);RoomNode node=journeyRooms[currentRoom];
   if(thunderChapterMode&&chapterCg){DrawChapterCg(w,h);GUI.matrix=saved;GUI.color=old;return;}
   if(showRainCG){HudRect(new Rect(0,0,w,h),Color.black);if(rainCG!=null)GUI.DrawTexture(new Rect(0,0,w,h),rainCG,ScaleMode.ScaleToFit);GUI.Label(new Rect(30,24,700,36),"甘泽归沙 · 愿长风携雨而来",journeyTitleStyle);if(StoryActive)DrawJourneyDialogue(w,h);else GUI.Label(new Rect(30,h-50,700,36),"F 继续探索",journeyTextStyle);GUI.matrix=saved;GUI.color=old;return;}
   HudRect(new Rect(16,16,302,158),panel);GUI.Label(new Rect(28,24,280,30),(thunderChapterMode?ChapterTitle:thunderPrototypeMode?ThunderTitle:IsGatehouseRoom(node)?GatehouseHeading(node):"第 "+(node.stage+1)+" 层 · "+StageNames[node.stage]),journeyTextStyle);
   GUI.Label(new Rect(28,61,86,23),"生命 "+health.ToString("0.#"),journeySmallStyle);for(int i=0;i<6;i++)HudRect(new Rect(126+i*28,66,23,10),health>=i+1?C("eb856c"):health>i?C("a76561"):C("493e47"));
   GUI.Label(new Rect(28,86,90,23),"护盾 "+shield.ToString("0.#"),journeySmallStyle);for(int i=0;i<4;i++){HudRect(new Rect(126+i*42,91,36,8),C("304958"));HudRect(new Rect(126+i*42,91,36*Mathf.Clamp01(shield-i),8),C("72d9eb"));}
   GUI.Label(new Rect(28,112,95,23),(origin==Origin.Unchosen?"主属性待选":origin==Origin.Dark?"暗之力 "+Mathf.FloorToInt(darkPower):"光之力 "+Mathf.FloorToInt(energy)),journeySmallStyle);PowerBar(new Rect(126,118,160,7),origin==Origin.Unchosen?0:origin==Origin.Dark?darkPower:energy,origin==Origin.Dark?C("bc7cf4"):C("efd18c"));
   GUI.Label(new Rect(28,138,95,23),"元素之力 "+Mathf.FloorToInt(elementPower),new GUIStyle(journeySmallStyle){fontSize=14});PowerBar(new Rect(126,144,160,7),elementPower,C("6edbc4"));
   HudRect(new Rect(w-204,16,188,184),panel);DrawMapCells(new Rect(w-194,24,168,142),true);GUI.Label(new Rect(w-191,171,175,25),"M 地图 · B 行囊",journeySmallStyle);
   if(ThunderActive){HudRect(new Rect(w-204,204,188,62),panel);GUI.Label(new Rect(w-195,208,177,56),"F5 保存 · F9 读取\n"+(thunderChapterMode?chapterSaveStatus:thunderSaveStatus),new GUIStyle(journeySmallStyle){fontSize=14});}
   if(CheckpointsEnabled)GUI.Label(new Rect(w-204,204,195,52),"F5 存档 · F9 读档\n"+(checkpointStatus.Length>12?checkpointStatus.Substring(0,11)+"…":checkpointStatus),journeySmallStyle);
   foreach(Enemy e in enemies)
   {
    if(e.boss)DrawBossHud(e,w,panel);
    else{Vector3 at=view.WorldToScreenPoint((Vector3)e.position+Vector3.up*(e.kind==EnemyKind.StoneSentinel?1.85f:1.3f));float x=at.x/scale,y=(Screen.height-at.y)/scale;Rect bar=new Rect(x-25,y,50,5);HudRect(bar,C("3d3737"));bar.width*=Mathf.Clamp01(e.hp/e.maximum);HudRect(bar,e.elite?C("d8b278"):C("dc8569"));if(e.elite)CenterLabel(new Rect(x-95,y-25,190,24),EnemyTitle(e)+(e.kind==EnemyKind.StoneSentinel?(clock<e.recoveryUntil?" · 破绽":" · 护甲"):""),journeySmallStyle);}
   }
   foreach(var hit in floatingHits){Vector3 at=view.WorldToScreenPoint(hit.at);GUI.color=hit.color;CenterLabel(new Rect(at.x/scale-110,(Screen.height-at.y)/scale,220,30),hit.text,journeyTextStyle);}GUI.color=Color.white;
   HudRect(new Rect(16,h-111,340,78),panel);
   for(int i=0;i<2;i++)GUI.Label(new Rect(28,h-105+i*25,320,26),(i==equippedSlot?"◆ ":"   ")+(i+1)+" "+(carriedWeapons[i]<0?"空武器栏":weaponCatalog[carriedWeapons[i]].name),journeyTextStyle);
   GUI.Label(new Rect(28,h-53,320,22),"H 换武器 · B 行囊 · 1 / 2 补给",journeySmallStyle);
   SkillBox(w-466,h-103,origin==Origin.Unchosen?"L 主属性":origin==Origin.Dark?"L 流星":"L 元气弹",orbLearned,nextSpell,8,true);if(thunderChapterMode){for(int i=0;i<2;i++){int k=chapterSkillSlots[i];SkillBox(w-313+i*153,h-103,(i==0?"U ":"I ")+ChapterSkillNames[k],ChapterSkillKnown(k),ChapterSkillReady(k),ChapterSkillCost(k),false);}}else{SkillBox(w-313,h-103,"U 风之刃",bladeLearned,bladeReadyAt,6,false);SkillBox(w-160,h-103,"I 护身风波",waveLearned,waveReadyAt,10,false);}
   string objective=!node.cleared?"第 "+currentWave+" / "+node.waves+" 波 · 剩余 "+(enemies.Count+pendingSpawns.Count):journeyComplete?"甘泽归沙 · 继续探索支路":stairs!=null?"阶梯已开启 · "+StairDestination():"战斗结束 · 沿打开的石门继续探索";
   if(GatehouseActive)objective=GatehouseObjective();if(thunderPrototypeMode)objective=ThunderObjective();if(thunderChapterMode)objective=ChapterObjective();
   if(nextWaveAt>=0)objective="下一波即将到来 · 留意地面预警";
   if(!thunderChapterMode||!bossSpawned||node.cleared){HudRect(new Rect(w/2-245,88,490,38),panel);CenterLabel(new Rect(w/2-236,91,472,30),objective,journeySmallStyle);}
   if(clock<roomBannerUntil&&!StoryActive){HudRect(new Rect(w/2-230,137,460,66),panel);CenterLabel(new Rect(w/2-215,143,430,31),(thunderChapterMode?ChapterTitle:thunderPrototypeMode?"雷雨高原 · 独立两房原型":GatehouseActive?(gatehouseSampleMode?"旧渠闸室样板":"归渠水道 · 沙漠收尾"):"第 "+(node.stage+1)+" 层 · "+StageNames[node.stage]),journeyTitleStyle);CenterLabel(new Rect(w/2-215,177,430,24),JourneyRoomTitle(node),journeySmallStyle);}
   DrawContextGuidance(w,h,scale,panel);
   if(journeyToastUntil>clock&&!string.IsNullOrEmpty(journeyToast)){HudRect(new Rect(w/2-320,h-207,640,42),panel);CenterLabel(new Rect(w/2-308,h-202,616,32),journeyToast,journeySmallStyle);}
   GUI.Label(new Rect(20,h-27,w-40,26),(thunderChapterMode?"WASD 移动   J 武器   K 闪避   F 交互   H 换武器   B 行囊   T 技能   M 地图   F5 / F9 存读   N 配置":thunderPrototypeMode?"WASD 移动   J 武器   K 闪避   F 交互   H 换武器   B 行囊   M 地图   ESC 暂停   R 重试   N 原型配置":"WASD 移动   J 武器   K 闪避   F 交互   H 换武器   B 行囊   M 地图   ESC 暂停   R 重试   N 新旅程"),journeySmallStyle);
   if((health<=0||paused)&&!StoryActive&&!mapMenu&&!rewardMenu&&!inventoryMenu){HudRect(new Rect(w/2-250,h/2-60,500,120),panel);CenterLabel(new Rect(w/2-230,h/2-43,460,45),health<=0?"旅程未竟 · R 重试":"暂歇片刻 · ESC 继续",journeyTitleStyle);if(paused&&!ThunderActive)CenterLabel(new Rect(w/2-230,h/2+12,460,30),"V 重播开场",journeySmallStyle);}
   if(thunderChapterMode&&chapterSetup)DrawChapterSetup(w,h);else if(thunderChapterMode&&chapterSkillsMenu)DrawChapterSkills(w,h);else if(thunderChapterMode&&chapterRewardMenu)DrawChapterReward(w,h);else if(thunderPrototypeMode&&thunderSetupMenu)DrawThunderSetup(w,h);else if(OriginChoiceReady)DrawOriginChoice(w,h);else if(StoryActive)DrawJourneyDialogue(w,h);else if(inventoryMenu)DrawBackpack(w,h);else if(mapMenu)DrawJourneyMap(w,h);else if(rewardMenu)DrawJourneyReward(w,h);
   if(doorTransition>0)HudRect(new Rect(0,0,w,h),new Color(0,0,0,Mathf.Sin(Mathf.Clamp01(doorTransition/.24f)*Mathf.PI)));
   GUI.matrix=saved;GUI.color=old;
  }
  private void DrawBossHud(Enemy e,float w,Color panel)
  {
   Rect box=new Rect(w/2-242,16,484,66);HudRect(box,panel);Sprite[] art;if(enemyActionArt.TryGetValue(e.kind,out art))DrawSpriteUI(new Rect(box.x+6,box.y+3,61,58),art[0]);
   GUI.Label(new Rect(box.x+76,box.y+4,390,29),(lightningTrialMode?"雷印试炼 · ":"")+EnemyTitle(e)+(e.secondPhase?"  II":"  I"),journeyTextStyle);
   string move=e.phaseUntil>clock?(IsChapterBoss(e.kind)?"雷流失衡 · 即将进入第二阶段":"风石共鸣 · 即将进入第二阶段"):e.windup>0?"蓄力："+EnemyMoveName(e):clock<e.recoveryUntil?"收招 · 攻击机会":"观察前摇，利用掩体与闪避";
   GUI.Label(new Rect(box.x+76,box.y+30,390,24),move,journeySmallStyle);HudRect(new Rect(box.x+76,box.y+56,390,5),C("453c39"));HudRect(new Rect(box.x+76,box.y+56,390*Mathf.Clamp01(e.hp/e.maximum),5),e.secondPhase?C("d69acb"):C("df9b78"));
  }
  private void DrawContextGuidance(float w,float h,float scale,Color panel)
  {
   string text=thunderChapterMode?ChapterPrompt():thunderPrototypeMode?ThunderPrompt():GatehouseActive?GatehousePrompt():"";
   if(GatehouseActive&&!gatehouseSampleMode&&canalEpilogueComplete&&journeyRooms[currentRoom].gatehouseIndex==4&&Vector2.Distance(position,ThunderDockAt)<1.4f)text="F 乘船前往第二岛 · 雷雨高原";
   if(stairs!=null)
   {
    Vector3 point=view.WorldToScreenPoint((Vector3)stairPosition+Vector3.up*1.65f);float x=Mathf.Clamp(point.x/scale,145,w-145),y=Mathf.Clamp((Screen.height-point.y)/scale,210,h-250);
    CenterLabel(new Rect(x-16,y,32,34),"↓",journeyTitleStyle);
    if(Vector2.Distance(position,stairPosition)<1.6f)text="F "+StairDestination();
   }
   if(text.Length==0&&lessonAltar!=null&&Vector2.Distance(position,lessonPosition)<1.35f)text="F 学习 · "+(pendingLesson==1?(origin==Origin.Dark?"流星":"元气弹"):pendingLesson==2?"风之刃":"护身风波");
   WeaponDrop drop=NearestWeaponDrop();
   if(text.Length==0&&drop!=null)text="F 拾取 "+weaponCatalog[drop.data.id].name+" · "+WeaponSummary(weaponCatalog[drop.data.id]);
   if(text.Length==0&&rewardChest!=null&&Vector2.Distance(position,chestPosition)<1.4f)text="F 打开旅人补给";
   if(text.Length>0){HudRect(new Rect(w/2-340,h-158,680,38),panel);CenterLabel(new Rect(w/2-330,h-154,660,30),text,journeySmallStyle);}
  }
  private void SkillBox(float x,float y,string label,bool learned,float ready,float cost,bool light)
  {HudRect(new Rect(x,y,144,70),new Color(.035f,.065f,.09f,.95f));GUI.Label(new Rect(x+8,y+7,132,29),label,journeyTextStyle);var style=new GUIStyle(journeySmallStyle);style.normal.textColor=light?(origin==Origin.Dark?C("bc7cf4"):C("efd18c")):C("6edbc4");GUI.Label(new Rect(x+8,y+40,132,24),!learned?"未学会":clock<ready?(ready-clock).ToString("0.0")+"秒":(light?(origin==Origin.Dark?"暗 ":"光 "):"元素 ")+cost,style);}
  private string JourneyRoomTitle(RoomNode n){if(thunderChapterMode)return chapterNames[n.id];if(thunderPrototypeMode)return n.id==0?"草坡战斗房 · 3 波":"引雷机关房 · 2 波";if(IsGatehouseRoom(n))return GatehouseRoomName(n);return JourneyRoomType(n.kind)+" · "+RoomNames[n.layout%RoomNames.Length];}
  private static string JourneyRoomType(RoomKind k){switch(k){case RoomKind.Entrance:return "入口";case RoomKind.Rest:return "休憩";case RoomKind.Event:return "旧渠";case RoomKind.Reward:return "补给";case RoomKind.Guardian:return "守渠";case RoomKind.Boss:return "首领";case RoomKind.Elite:return "精英";default:return "战斗";}}
  private void DrawMapCells(Rect area,bool small)
  {
   int stage=journeyRooms[currentRoom].stage;float step=Mathf.Min(area.width/6,area.height/6);Vector2 origin=new Vector2(area.center.x-2.5f*step,area.center.y+2.5f*step);
   foreach(RoomNode n in journeyRooms)if(n.stage==stage&&n.visited)foreach(int link in n.links){RoomNode b=journeyRooms[link];if(!b.discovered)continue;JourneyMapLine(origin+new Vector2(n.cell.x,-n.cell.y)*step,origin+new Vector2(b.cell.x,-b.cell.y)*step,(ExplorationDoorLocked(n.id,NeighborDirection(n,b)))?C("c78962"):C("617e80"));}
   foreach(RoomNode n in journeyRooms)if(n.stage==stage&&n.discovered)
   {
    Vector2 p=origin+new Vector2(n.cell.x,-n.cell.y)*step;Rect box=new Rect(p.x-step*.37f,p.y-step*.30f,step*.74f,step*.60f);
    Color c=n.id==currentRoom?C("bb9759"):n.cleared?C("356d63"):n.visited?C("9b6559"):C("3c505d");HudRect(new Rect(box.x-1,box.y-1,box.width+2,box.height+2),n.id==currentRoom?C("f2d49c"):C("6c8180"));HudRect(box,c);
    string label=n.visited?(thunderChapterMode?ChapterMapLabel(n):thunderPrototypeMode?(n.id==0?"草坡":"引雷"):IsGatehouseRoom(n)?new[]{"来路","闸室","总闸","检修","望台"}[n.gatehouseIndex]:JourneyRoomType(n.kind)):"?";if(n.visited&&n.cleared&&n.stairsTo>=0)label=IsGatehouseRoom(n)?"返回":IsGatehouseRoom(journeyRooms[n.stairsTo])?"水道":journeyRooms[n.stairsTo].stage>n.stage?"下层":"上层";
    if(!small)GUI.Label(box,label,journeyNodeStyle);else if(n.visited&&n.stairsTo>=0&&n.cleared)CenterLabel(box,"S",journeySmallStyle);
   }
  }
  private static void JourneyMapLine(Vector2 a,Vector2 b,Color c){HudRect(new Rect(Mathf.Min(a.x,b.x)-1,Mathf.Min(a.y,b.y)-1,Mathf.Max(2,Mathf.Abs(a.x-b.x)),Mathf.Max(2,Mathf.Abs(a.y-b.y))),c);}
  private void DrawJourneyMap(float w,float h)
  {
   HudRect(new Rect(w/2-365,h/2-290,730,580),new Color(.035f,.065f,.09f,.985f));GUI.Label(new Rect(w/2-334,h/2-270,670,38),(thunderChapterMode?ChapterTitle+" · 探索记录":thunderPrototypeMode?"雷雨高原原型 · 探索记录":GatehouseActive?(gatehouseSampleMode?"旧渠闸室 · 探索记录":"归渠水道 · 探索记录"):"第 "+(journeyRooms[currentRoom].stage+1)+" 层 · "+StageNames[journeyRooms[currentRoom].stage]),journeyTitleStyle);
   DrawMapCells(new Rect(w/2-280,h/2-210,560,420),false);GUI.Label(new Rect(w/2-334,h/2+224,670,54),(thunderChapterMode?"金：所在房间   绿：已清理   铜线：未开放检修道\n沿石门探索，清理后可往返；F 使用阶梯   M / ESC 返回":thunderPrototypeMode?"金：所在房间   绿：已清理   ?：未知\n清房后沿门往返；本原型没有换层石阶   M / ESC 返回":GatehouseActive?"金：所在房间   绿：已清理   ?：未知\n铜色连线：检修闸尚未开放；可沿北路绕行   M / ESC 返回":"金：所在房间   绿：已清理   ?：相邻未知房间\n下层 / 上层：阶梯；靠近按 F 使用   M / ESC 返回"),journeySmallStyle);
  }
  private void DrawJourneyDialogue(float w,float h)
  {
   HudRect(new Rect(30,h-197,w-60,165),new Color(.035f,.065f,.09f,.98f));var wrap=new GUIStyle(journeyTextStyle){wordWrap=true};GUI.Label(new Rect(55,h-178,w-110,102),storyLines[storyIndex],wrap);GUI.Label(new Rect(55,h-64,650,29),"F 继续   ESC 略过   "+(storyIndex+1)+" / "+storyLines.Length,journeySmallStyle);
  }
  private void DrawJourneyReward(float w,float h)
  {
   HudRect(new Rect(w/2-390,h/2-135,780,270),new Color(.035f,.065f,.09f,.985f));GUI.Label(new Rect(w/2-360,h/2-112,720,38),"旅人的补给 · 选择一项",journeyTitleStyle);
   string[] choices={"清泉补给\n生命 +2，清泉药剂 +1","磨砺剑刃\n近战伤害 +2","元素补给\n元素 +35，元素药剂 +1"};for(int i=0;i<3;i++)if(FlatButton(new Rect(w/2-360+i*245,h/2-45,230,110),choices[i],routeChoice==i))ClaimJourneyReward(i);GUI.Label(new Rect(w/2-360,h/2+90,720,30),"A / D 选择    F 确认    药剂可在 B 行囊中使用",journeySmallStyle);
  }
 }
}
