using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private const float HudTop=96,HudBottom=104;
  private GUIStyle journeyNodeStyle;
  private string journeyToast;private float journeyToastUntil;
  private static float HudScaleFor(float width,float height){return Mathf.Max(.1f,Mathf.Min(width/1280f,height/720f));}
  private static Rect JourneyViewport(float width,float height)
  {
   float s=HudScaleFor(width,height);return new Rect(0,HudBottom*s/height,1,1-(HudTop+HudBottom)*s/height);
  }
  private Vector3 JourneyViewCenter
  {get{RoomNode n=journeyRooms[currentRoom];return new Vector3(0,n.kind==RoomKind.Boss||n.kind==RoomKind.Guardian?1:.45f,-10);}}
  private void ConfigureJourneyView()
  {
   if(view==null||!enableDesertJourney||journeyRooms.Count==0)return;
   Rect area=JourneyViewport(Screen.width,Screen.height);if(view.rect!=area){view.rect=area;view.ResetAspect();}
   RoomNode n=journeyRooms[currentRoom];bool boss=n.kind==RoomKind.Boss||n.kind==RoomKind.Guardian;
   view.orthographicSize=Mathf.Max(n.halfHeight+(boss?2.4f:1.8f),(n.halfWidth+1.9f)/Mathf.Max(view.aspect,.1f));
  }
  private void EnsureJourneyStyles()
  {
   if(journeyTitleStyle!=null)return;journeyFont=Resources.Load<Font>("Fonts/FusionPixel");
   journeyTitleStyle=NewLabel(22,C("f4db9a"),FontStyle.Normal);journeyTextStyle=NewLabel(18,C("f3eddd"),FontStyle.Normal);journeySmallStyle=NewLabel(14,C("bdcfce"),FontStyle.Normal);
   journeyButtonStyle=new GUIStyle(GUI.skin.button){fontSize=18,wordWrap=true,alignment=TextAnchor.MiddleCenter};journeyNodeStyle=new GUIStyle(journeySmallStyle){alignment=TextAnchor.MiddleCenter};
   foreach(var s in new[]{journeyTitleStyle,journeyTextStyle,journeySmallStyle,journeyButtonStyle,journeyNodeStyle})s.font=journeyFont;
  }
  private GUIStyle FitLabel(Rect box,string text,GUIStyle style,bool wrap=false,TextAnchor anchor=TextAnchor.UpperLeft)
  {
   var s=new GUIStyle(style){wordWrap=wrap,alignment=anchor,padding=new RectOffset(0,0,0,0),contentOffset=Vector2.zero};
   var content=new GUIContent(text);while(s.fontSize>10&&(s.CalcHeight(content,box.width)>box.height||!wrap&&s.CalcSize(content).x>box.width))s.fontSize--;
   return s;
  }
  private void FittedLabel(Rect box,string text,GUIStyle style,bool wrap=false,TextAnchor anchor=TextAnchor.UpperLeft){GUI.Label(box,text,FitLabel(box,text,style,wrap,anchor));}
  private void CenterLabel(Rect box,string text,GUIStyle style){FittedLabel(box,text,style,true,TextAnchor.MiddleCenter);}
  private void PowerBar(Rect box,float value,Color color){HudRect(box,C("243d48"));HudRect(new Rect(box.x,box.y,box.width*Mathf.Clamp01(value/100),box.height),color);}
  private void HudDivider(float x,float y,float height){HudRect(new Rect(x,y,1,height),C("30434a"));}
  private void KeyCap(Rect box,string text){HudRect(box,C("293d47"));CenterLabel(box,text,journeySmallStyle);}
  private string ActivePowerLabel {get{return origin==Origin.Dark?"贤者之石 "+Mathf.FloorToInt(darkPower):origin==Origin.Light?"光之力 "+Mathf.FloorToInt(energy):"主属性待选择";}}
  private Color ActivePowerColor {get{return origin==Origin.Dark?C("b88ae8"):C("ecd18e");}}
  private string RoomHeading(RoomNode node){return thunderChapterMode?"雷雨高原 · "+ThunderStages[node.stage]:thunderPrototypeMode?ThunderTitle:IsGatehouseRoom(node)?GatehouseHeading(node):"沙漠岛 · "+StageNames[node.stage];}
  private void DrawJourneyHud()
  {
   if(world==null||journeyRooms.Count==0)return;EnsureJourneyStyles();Matrix4x4 saved=GUI.matrix;Color old=GUI.color;
   float scale=HudScaleFor(Screen.width,Screen.height),w=Screen.width/scale,h=Screen.height/scale;GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));GUI.color=Color.white;
   try
   {
    if(thunderChapterMode&&chapterCg){DrawChapterCg(w,h);return;}
    if(showRainCG&&!DesertEndingReady)showRainCG=false;
    if(showRainCG){HudRect(new Rect(0,0,w,h),Color.black);if(rainCG!=null)GUI.DrawTexture(new Rect(0,0,w,h),rainCG,ScaleMode.ScaleToFit);FittedLabel(new Rect(30,24,w-60,36),"甘泽归沙 · 愿长风携雨而来",journeyTitleStyle);if(StoryActive)DrawJourneyDialogue(w,h);else GUI.Label(new Rect(30,h-50,700,36),"F 继续探索",journeyTextStyle);return;}
    DrawJourneyRails(w,h);DrawCombatAnnotations(w,h,scale);
    bool overlay=StoryActive||mapMenu||rewardMenu||inventoryMenu||OriginChoiceReady||paused||health<=0||thunderPrototypeMode&&thunderSetupMenu||thunderChapterMode&&(chapterSetup||chapterSkillsMenu||chapterRewardMenu);
    if(overlay)HudRect(new Rect(0,0,w,h),new Color(.015f,.025f,.035f,.55f));
    if((health<=0||paused)&&!StoryActive&&!mapMenu&&!rewardMenu&&!inventoryMenu)
    {HudRect(new Rect(w/2-250,h/2-60,500,120),C("10232d"));CenterLabel(new Rect(w/2-230,h/2-43,460,45),health<=0?"旅程未竟 · R 回到安全点":"暂歇片刻 · ESC 继续",journeyTitleStyle);if(paused&&!ThunderActive)CenterLabel(new Rect(w/2-230,h/2+12,460,30),"V 重播开场",journeySmallStyle);}
    if(thunderChapterMode&&chapterSetup)DrawChapterSetup(w,h);else if(thunderChapterMode&&chapterSkillsMenu)DrawChapterSkills(w,h);else if(thunderChapterMode&&chapterRewardMenu)DrawChapterReward(w,h);else if(thunderPrototypeMode&&thunderSetupMenu)DrawThunderSetup(w,h);else if(OriginChoiceReady)DrawOriginChoice(w,h);else if(StoryActive)DrawJourneyDialogue(w,h);else if(inventoryMenu)DrawBackpack(w,h);else if(mapMenu)DrawJourneyMap(w,h);else if(rewardMenu)DrawJourneyReward(w,h);
    if(doorTransition>0)HudRect(new Rect(0,HudTop,w,h-HudTop-HudBottom),new Color(0,0,0,Mathf.Sin(Mathf.Clamp01(doorTransition/.24f)*Mathf.PI)));
   }
   finally{GUI.matrix=saved;GUI.color=old;}
  }
  private void DrawJourneyRails(float w,float h)
  {
   Color rail=C("101d26"),muted=C("8ea8aa");RoomNode node=journeyRooms[currentRoom];
   HudRect(new Rect(0,0,w,HudTop),rail);HudRect(new Rect(0,h-HudBottom,w,HudBottom),rail);
   HudRect(new Rect(0,HudTop-2,w,2),C("51636a"));HudRect(new Rect(0,h-HudBottom,w,2),C("51636a"));
   FittedLabel(new Rect(20,10,300,27),RoomHeading(node),journeyTextStyle);
   FittedLabel(new Rect(20,42,70,22),"生命 "+health.ToString("0.#"),journeySmallStyle);
   for(int i=0;i<6;i++){HudRect(new Rect(100+i*32,49,27,8),C("3e3c43"));HudRect(new Rect(100+i*32,49,27*Mathf.Clamp01(health-i),8),C("e78c75"));}
   FittedLabel(new Rect(20,67,70,22),"护盾 "+shield.ToString("0.#"),journeySmallStyle);
   int count=Mathf.Max(1,Mathf.CeilToInt(EffectiveMaximumShield));float step=192f/count;
   for(int i=0;i<count;i++){HudRect(new Rect(100+i*step,74,step-5,6),C("263e4a"));HudRect(new Rect(100+i*step,74,(step-5)*Mathf.Clamp01(shield-i),6),C("74d4e2"));}
   HudDivider(318,16,65);
   FittedLabel(new Rect(340,18,274,23),ActivePowerLabel,new GUIStyle(journeySmallStyle){normal={textColor=ActivePowerColor}});PowerBar(new Rect(340,45,272,5),origin==Origin.Dark?darkPower:origin==Origin.Light?energy:0,ActivePowerColor);
   FittedLabel(new Rect(340,58,274,22),"元素之力 "+Mathf.FloorToInt(elementPower),journeySmallStyle);PowerBar(new Rect(340,83,272,4),elementPower,C("72d8bc"));
   HudDivider(638,16,65);Enemy boss=null;foreach(Enemy e in enemies)if(e.boss){boss=e;break;}
   if(boss!=null)DrawBossHud(boss,w,rail);
   else
   {
    FittedLabel(new Rect(662,15,w-830,27),JourneyRoomTitle(node),journeyTextStyle);
    string message=node.cleared?(thunderChapterMode?ChapterObjective():GatehouseActive?GatehouseObjective():stairs!=null?"阶梯已开启 · "+StairDestination():"石门已开启 · 沿旧路继续探索"):"留意蓄力与地面预警";
    FittedLabel(new Rect(662,47,w-830,40),message,journeySmallStyle,true);
   }
   DrawMapCells(new Rect(w-148,9,128,66),true);FittedLabel(new Rect(w-149,76,132,20),"M 地图",journeySmallStyle,false,TextAnchor.MiddleCenter);
   DrawContextGuidance(w,h,1,rail);
   float y=h-73;DrawSpriteUI(new Rect(20,y,52,45),CurrentWeapon.sprite);
   FittedLabel(new Rect(84,y+1,290,25),CurrentWeapon.name,journeyTextStyle);
   int other=carriedWeapons[1-equippedSlot];FittedLabel(new Rect(84,y+29,290,22),"H 切换 · "+(other<0?"另一武器栏为空":weaponCatalog[other].name),journeySmallStyle);
   KeyCap(new Rect(402,y+5,28,24),"K");FittedLabel(new Rect(442,y+5,185,26),clock<nextDash?"闪避 · "+(nextDash-clock).ToString("0.0")+"秒":"闪避 · 就绪",journeySmallStyle);
   FittedLabel(new Rect(402,y+34,220,22),"B 行囊   "+(thunderChapterMode?"T 元素技能":"1 / 2 使用补给"),journeySmallStyle);
   SkillBox(w-576,y,origin==Origin.Unchosen?"L 主属性":origin==Origin.Dark?"L 流星":"L 元气弹",orbLearned,nextSpell,8,true);
   if(thunderChapterMode){for(int i=0;i<2;i++){int k=chapterSkillSlots[i];SkillBox(w-386+i*190,y,(i==0?"U ":"I ")+ChapterSkillNames[k],ChapterSkillKnown(k),ChapterSkillReady(k),ChapterSkillCost(k),false);}}
   else{SkillBox(w-386,y,"U 风之刃",bladeLearned,bladeReadyAt,6,false);SkillBox(w-196,y,"I 护身风波",waveLearned,waveReadyAt,10,false);}
   FittedLabel(new Rect(20,h-20,w-340,18),"WASD 移动   J 武器   F 交互   ESC 暂停   M 地图   F5 / F9 存读   R 重试   N "+(ThunderActive?"测试配置":"新旅程"),new GUIStyle(journeySmallStyle){fontSize=11,normal={textColor=muted}});
   string status=thunderChapterMode?thunderChapterPracticeBoss>=0?"Boss 练习 · 不写旅程存档":chapterSaveStatus:thunderPrototypeMode?thunderSaveStatus:checkpointStatus;
   FittedLabel(new Rect(w-315,h-20,295,18),status,new GUIStyle(journeySmallStyle){fontSize=11,normal={textColor=muted}},false,TextAnchor.MiddleRight);
  }
  private void DrawBossHud(Enemy e,float w,Color panel)
  {
   float width=w-830;FittedLabel(new Rect(662,10,width,27),EnemyTitle(e)+(e.secondPhase?" · II":" · I"),journeyTextStyle);
   string move=e.phaseUntil>clock?"力量涌动 · 即将变招":e.windup>0?"蓄力 · "+EnemyMoveName(e):clock<e.recoveryUntil?"收招 · 反击机会":"观察前摇，留意下一次攻击";
   FittedLabel(new Rect(662,41,width,27),move,journeySmallStyle);HudRect(new Rect(662,76,width,6),C("41383b"));HudRect(new Rect(662,76,width*Mathf.Clamp01(e.hp/e.maximum),6),e.secondPhase?C("c69bce"):C("dfa77d"));
  }
  private void DrawCombatAnnotations(float w,float h,float scale)
  {
   Rect area=new Rect(0,HudTop,w,h-HudTop-HudBottom);GUI.BeginGroup(area);
   foreach(Enemy e in enemies)if(!e.boss)
   {
    Vector3 at=view.WorldToScreenPoint((Vector3)e.position+Vector3.up*(e.kind==EnemyKind.ThunderStone?1.45f:e.kind==EnemyKind.StoneSentinel?1.85f:1.35f));float x=at.x/scale,y=(Screen.height-at.y)/scale-area.y;
    Rect bar=new Rect(x-22,y,44,4);HudRect(bar,C("343c40"));bar.width*=Mathf.Clamp01(e.hp/e.maximum);HudRect(bar,e.elite?C("d8b278"):C("dc8569"));if(e.elite)CenterLabel(new Rect(x-95,y-25,190,24),EnemyTitle(e),journeySmallStyle);
   }
   foreach(var hit in floatingHits){Vector3 at=view.WorldToScreenPoint(hit.at);GUI.color=hit.color;CenterLabel(new Rect(at.x/scale-110,(Screen.height-at.y)/scale-area.y,220,30),hit.text,journeySmallStyle);}GUI.color=Color.white;GUI.EndGroup();
  }
  private void DrawContextGuidance(float w,float h,float scale,Color panel)
  {
   string text="";RoomNode node=journeyRooms[currentRoom];
   if(node.cleared)
   {
    text=thunderChapterMode?ChapterPrompt():thunderPrototypeMode?ThunderPrompt():GatehouseActive?GatehousePrompt():"";
    if(GatehouseActive&&!gatehouseSampleMode&&canalEpilogueComplete&&node.gatehouseIndex==4&&Vector2.Distance(position,ThunderDockAt)<1.4f)text="F 乘船前往第二岛 · 雷雨高原";
    if(stairs!=null&&Vector2.Distance(position,stairPosition)<1.6f)text="F "+StairDestination();
    if(text.Length==0&&lessonAltar!=null&&Vector2.Distance(position,lessonPosition)<1.35f)text="F 研读古刻 · "+(pendingLesson==1?(origin==Origin.Dark?"流星":"元气弹"):pendingLesson==2?"风之刃":"护身风波");
    WeaponDrop drop=NearestWeaponDrop();if(text.Length==0&&drop!=null)text="F 拾取 "+weaponCatalog[drop.data.id].name+" · "+WeaponSummary(weaponCatalog[drop.data.id]);
    if(text.Length==0&&rewardChest!=null&&Vector2.Distance(position,chestPosition)<1.4f)text="F 打开旅人补给";
   }
   if(text.Length==0&&journeyToastUntil>clock&&!string.IsNullOrEmpty(journeyToast))text=journeyToast;
   if(text.Length==0&&node.cleared)text="沿已打开的石门继续探索";
   FittedLabel(new Rect(20,h-HudBottom+6,w-40,24),text,journeySmallStyle,false,TextAnchor.MiddleCenter);
  }
  private void SkillBox(float x,float y,string label,bool learned,float ready,float cost,bool light)
  {
   HudDivider(x,y+3,47);FittedLabel(new Rect(x+14,y,164,26),label,journeyTextStyle);
   Color c=light?ActivePowerColor:C("72d8bc");string detail=!learned?"尚未领悟":clock<ready?(ready-clock).ToString("0.0")+" 秒":(light?(origin==Origin.Dark?"暗之力 ":"光之力 "):"元素 ")+cost;
   FittedLabel(new Rect(x+14,y+29,164,23),detail,new GUIStyle(journeySmallStyle){normal={textColor=learned?c:C("758b94")}});
   if(learned&&clock<ready){float duration=light?(origin==Origin.Dark?1:1.2f):label.Contains("风波")?4.5f:label.Contains("雷标")?2:label.Contains("跃电")?3:1.2f;HudRect(new Rect(x+14,y+54,164*Mathf.Clamp01(1-(ready-clock)/duration),2),c);}
  }
  private string JourneyRoomTitle(RoomNode n){if(thunderChapterMode)return chapterNames[n.id];if(thunderPrototypeMode)return n.id==0?"雨中草坡":"西引雷坪";if(IsGatehouseRoom(n))return GatehouseRoomName(n);return JourneyRoomType(n.kind)+" · "+RoomNames[n.layout%RoomNames.Length];}
  private static string JourneyRoomType(RoomKind k){switch(k){case RoomKind.Entrance:return "入口";case RoomKind.Rest:return "休憩";case RoomKind.Event:return "路迹";case RoomKind.Reward:return "补给";case RoomKind.Guardian:return "守卫";case RoomKind.Boss:return "首领";case RoomKind.Elite:return "精英";default:return "战斗";}}
  private void DrawMapCells(Rect area,bool small)
  {
   int stage=journeyRooms[currentRoom].stage;float step=Mathf.Min(area.width/6,area.height/6);Vector2 origin=new Vector2(area.center.x-2.5f*step,area.center.y+2.5f*step);
   foreach(RoomNode n in journeyRooms)if(n.stage==stage&&n.visited)foreach(int link in n.links){RoomNode b=journeyRooms[link];if(!b.discovered)continue;JourneyMapLine(origin+new Vector2(n.cell.x,-n.cell.y)*step,origin+new Vector2(b.cell.x,-b.cell.y)*step,ExplorationDoorLocked(n.id,NeighborDirection(n,b))?C("c78962"):C("617e80"));}
   foreach(RoomNode n in journeyRooms)if(n.stage==stage&&n.discovered)
   {
    Vector2 p=origin+new Vector2(n.cell.x,-n.cell.y)*step;Rect box=new Rect(p.x-step*.37f,p.y-step*.30f,step*.74f,step*.60f);
    Color c=n.id==currentRoom?C("bb9759"):n.cleared?C("356d63"):n.visited?C("9b6559"):C("3c505d");HudRect(new Rect(box.x-1,box.y-1,box.width+2,box.height+2),n.id==currentRoom?C("f2d49c"):C("6c8180"));HudRect(box,c);
    string label=n.visited?(thunderChapterMode?ChapterMapLabel(n):thunderPrototypeMode?(n.id==0?"草坡":"引雷"):IsGatehouseRoom(n)?new[]{"来路","闸室","总闸","检修","望台"}[n.gatehouseIndex]:JourneyRoomType(n.kind)):"?";
    if(n.visited&&n.cleared&&n.stairsTo>=0)label=IsGatehouseRoom(n)?"返回":IsGatehouseRoom(journeyRooms[n.stairsTo])?"水道":journeyRooms[n.stairsTo].stage>n.stage?"下层":"上层";
    if(!small)CenterLabel(box,label,journeyNodeStyle);else if(n.visited&&n.stairsTo>=0&&n.cleared)CenterLabel(box,"S",new GUIStyle(journeySmallStyle){fontSize=10});
   }
  }
  private static void JourneyMapLine(Vector2 a,Vector2 b,Color c){HudRect(new Rect(Mathf.Min(a.x,b.x)-1,Mathf.Min(a.y,b.y)-1,Mathf.Max(2,Mathf.Abs(a.x-b.x)),Mathf.Max(2,Mathf.Abs(a.y-b.y))),c);}
  private void DrawJourneyMap(float w,float h)
  {
   HudRect(new Rect(w/2-365,h/2-290,730,580),C("10212a"));FittedLabel(new Rect(w/2-334,h/2-270,670,38),RoomHeading(journeyRooms[currentRoom])+" · 探索记录",journeyTitleStyle);
   DrawMapCells(new Rect(w/2-280,h/2-210,560,420),false);FittedLabel(new Rect(w/2-334,h/2+224,670,54),"金：所在房间   绿：已清理   ?：相邻未知房间\n沿石门探索；铜线为未开放检修道，S 为阶梯。M / ESC 返回",journeySmallStyle,true);
  }
  private void DrawJourneyDialogue(float w,float h)
  {
   Rect box=new Rect(40,h-218,w-80,174);HudRect(box,C("10232c"));HudRect(new Rect(box.x,box.y,3,box.height),C("b99b62"));FittedLabel(new Rect(box.x+26,box.y+22,box.width-52,102),storyLines[storyIndex],journeyTextStyle,true);FittedLabel(new Rect(box.x+26,box.y+137,box.width-52,25),"F 继续   ESC 略过   "+(storyIndex+1)+" / "+storyLines.Length,journeySmallStyle);
  }
  private void DrawJourneyReward(float w,float h)
  {
   HudRect(new Rect(w/2-390,h/2-135,780,270),C("10212a"));FittedLabel(new Rect(w/2-360,h/2-112,720,38),"旅人的补给 · 选择一项",journeyTitleStyle);
   string[] choices={"清泉补给\n生命 +2，清泉药剂 +1","磨砺剑刃\n近战伤害 +2","元素补给\n元素 +35，元素药剂 +1"};for(int i=0;i<3;i++)if(FlatButton(new Rect(w/2-360+i*245,h/2-45,230,110),choices[i],routeChoice==i))ClaimJourneyReward(i);FittedLabel(new Rect(w/2-360,h/2+90,720,30),"A / D 选择    F 确认    药剂可在 B 行囊中使用",journeySmallStyle);
  }
 }
}
