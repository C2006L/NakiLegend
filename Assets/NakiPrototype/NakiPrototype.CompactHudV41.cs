using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype {
 public sealed partial class NakiPrototype {
  private readonly Dictionary<int,Texture2D> compactIcons=new Dictionary<int,Texture2D>();
  private readonly Dictionary<int,Texture2D> compactMasks=new Dictionary<int,Texture2D>();
  private Texture2D compactAtlas;private bool compactAtlasChecked;
  private GUIStyle CompactText(int size,Color color){return new GUIStyle(journeySmallStyle){fontSize=size,normal={textColor=color}};}
  private void CompactPanel(Rect r){
   HudRect(r,new Color(.035f,.065f,.09f,.86f));Color silver=C("89999f"),gold=C("c5a96e");
   HudRect(new Rect(r.x+3,r.y+3,r.width-6,1),new Color(.72f,.77f,.78f,.2f));
   HudRect(new Rect(r.x,r.y,r.width,1),silver);HudRect(new Rect(r.x,r.yMax-1,r.width,1),silver);
   HudRect(new Rect(r.x,r.y,1,r.height),silver);HudRect(new Rect(r.xMax-1,r.y,1,r.height),silver);
   foreach(float x in new[]{r.x,r.xMax-6})foreach(float y in new[]{r.y,r.yMax-6}){
    HudRect(new Rect(x,y,6,2),gold);HudRect(new Rect(x,y,2,6),gold);
   }
  }
  private void CompactLabel(Rect r,string text,int size,Color c,TextAnchor anchor=TextAnchor.MiddleLeft){FittedLabel(r,text,CompactText(size,c),false,anchor);}
  private void CompactResource(float y,int icon,string value,float amount,float maximum,Color color){
   GUI.DrawTexture(new Rect(22,y,19,19),CompactIcon(icon));PowerBar(new Rect(49,y+6,103,7),amount,color,maximum);
   CompactLabel(new Rect(158,y-1,78,21),value,13,C("f3eddd"),TextAnchor.MiddleRight);
  }
  private void DrawCompactHud(float w,float h){
   Color ivory=C("f3eddd");RoomNode node=journeyRooms[currentRoom];
   CompactPanel(new Rect(12,12,238,116));
   CompactResource(22,20,health.ToString("0.#")+" / 6",health,6,C("e7837c"));
   CompactResource(47,21,shield.ToString("0.#")+" / "+EffectiveMaximumShield.ToString("0.#"),shield,EffectiveMaximumShield,C("72ccdf"));
   if(origin==Origin.Dark){GUI.DrawTexture(new Rect(22,72,19,19),CompactIcon(22));CompactLabel(new Rect(49,70,185,23),"贤者之石  "+darkPower.ToString("0.#"),14,C("c49dea"));}
   else CompactResource(72,23,origin==Origin.Unchosen?"未选择":energy.ToString("0")+" / "+LightMaximum.ToString("0"),origin==Origin.Light?energy:0,LightMaximum,C("edd18b"));
   CompactResource(97,24,elementPower.ToString("0")+" / "+ElementMaximum.ToString("0"),elementPower,ElementMaximum,C("75d7b9"));
   Enemy boss=null;foreach(Enemy e in enemies)if(e.boss&&e.hp>0){boss=e;break;}
   if(boss!=null)DrawBossHud(boss,w,ivory);
   else {
    CompactPanel(new Rect(w*.5f-279,10,558,clock<roomBannerUntil?76:53));
    CompactLabel(new Rect(w*.5f-280,13,560,24),RoomHeading(node),16,C("e3d2a9"),TextAnchor.MiddleCenter);
    CompactLabel(new Rect(w*.5f-240,38,480,21),JourneyRoomTitle(node),14,ivory,TextAnchor.MiddleCenter);
    if(clock<roomBannerUntil){string goal=node.cleared?(thunderChapterMode?ChapterObjective():stairs!=null?StairDestination():"沿已打开的石门继续探索"):"清理本处守敌";
     CompactLabel(new Rect(w*.5f-300,61,600,21),goal,13,C("b7c9c9"),TextAnchor.MiddleCenter);}
   }
   CompactPanel(new Rect(w-154,12,142,100));DrawMapCells(new Rect(w-145,20,123,65),true);
   CompactLabel(new Rect(w-147,85,128,19),"M 地图 · G 航图",12,C("c5d2d0"),TextAnchor.MiddleCenter);
   if(boss!=null)CompactLabel(new Rect(w-265,115,250,20),RoomHeading(node),12,C("cbd6d5"),TextAnchor.MiddleRight);
   Rect weapon=new Rect(12,h-89,256,64);CompactPanel(weapon);DrawSpriteUI(new Rect(22,h-81,47,43),CurrentWeapon.sprite);
   CompactLabel(new Rect(78,h-81,140,23),CurrentWeapon.name,15,ivory);
   CompactLabel(new Rect(78,h-54,150,20),"J 攻击 · H 切换",12,C("bdcfce"));
   int other=carriedWeapons[1-equippedSlot];if(other>=0)DrawSpriteUI(new Rect(228,h-75,27,31),weaponCatalog[other].sprite);
   float start=w*.5f-176;
   for(int i=0;i<3;i++)DrawCompactAbility(new Rect(start+i*72,h-93,56,56),i,true);
   for(int i=0;i<2;i++)DrawCompactAbility(new Rect(start+(i+3)*72,h-93,56,56),i,false);
   Rect dash=new Rect(w-78,h-93,56,56);CompactPanel(dash);GUI.DrawTexture(new Rect(dash.x+6,dash.y+6,44,44),CompactIcon(19));
   DrawCompactKey(dash,"K");if(clock<nextDash){HudRect(dash,new Color(0,0,0,.55f));CompactLabel(dash,(nextDash-clock).ToString("0.0"),17,ivory,TextAnchor.MiddleCenter);}
   CompactLabel(new Rect(w-112,h-33,120,19),ProfessionName+" · "+(clock<nextDash?"恢复中":"就绪"),12,C("cbd6d5"),TextAnchor.MiddleCenter);
   if(dash.Contains(Event.current.mousePosition))DrawCompactTip(w,h,ProfessionName+"战技 · K",profession==Profession.Warrior?"冲撞与近身反击":"闪避来袭攻击");
   CompactLabel(new Rect(14,h-20,390,16),"WASD 移动  F 交互  B 行囊  T 技能  ESC 暂停",11,C("b2c1c2"));
   string status=thunderChapterMode?thunderChapterPracticeBoss>=0?"首领试炼 · 独立测试":chapterSaveStatus:thunderPrototypeMode?thunderSaveStatus:checkpointStatus;
   if(clock<roomBannerUntil)CompactLabel(new Rect(w-430,h-18,310,16),status,11,C("b2c1c2"),TextAnchor.MiddleRight);
   DrawContextGuidance(w,h,1,ivory);
  }
  private void DrawCompactBoss(Enemy e,float w){
   float width=Mathf.Min(620,w-610),x=(w-width)*.5f;
   CompactPanel(new Rect(x-12,10,width+24,77));
   CompactLabel(new Rect(x,14,width,23),EnemyTitle(e)+(e.difficultyPhase>=2?" · III":e.secondPhase?" · II":" · I"),18,C("eddfbd"),TextAnchor.MiddleCenter);
   PowerBar(new Rect(x,42,width,9),e.hp,C("c55369"),Mathf.Max(1,e.maximum));
   CompactLabel(new Rect(x,56,width,20),BossCombatHint(e),13,C("d1dcdc"),TextAnchor.MiddleCenter);
  }
  private void DrawCompactKey(Rect r,string key){Rect box=new Rect(r.x+17,r.y-12,23,20);CompactPanel(box);CompactLabel(box,key,14,C("f3eddd"),TextAnchor.MiddleCenter);}
  private float CompactElementCooldown(int id){return new[]{2.4f,4.5f,3.5f,4f,2.4f,4.5f,2.4f,5f,2.4f,5f,4f,2.4f}[Mathf.Clamp(id,0,11)];}
  private void DrawCompactAbility(Rect r,int slot,bool primary){
   int id=primary?slot:chapterSkillSlots[slot];bool known=primary?origin!=Origin.Unchosen&&OriginAbilityKnown(id):ChapterSkillKnown(id);
   float ready=primary?OriginSkillReady(id):ChapterSkillReady(id),cost=primary?OriginSkillCost(id):SkillElementCost(ChapterSkillCost(id));
   string key=primary?OriginKeys[id].ToString().ToUpperInvariant():slot==0?"U":"I",name=primary?OriginSkillName(id):ActiveSkillNames[id];
   Color tint=primary?ActivePowerColor:C("75d7b9");CompactPanel(r);Color before=GUI.color;GUI.color=known?Color.white:new Color(.5f,.5f,.5f,.65f);
   GUI.DrawTexture(new Rect(r.x+5,r.y+5,r.width-10,r.height-10),CompactIcon(primary?(origin==Origin.Dark?3:0)+id:6+id));GUI.color=before;
   if(!known){HudRect(r,new Color(.015f,.025f,.035f,.65f));GUI.DrawTexture(new Rect(r.x+18,r.y+18,20,20),CompactIcon(25));}
   else if(clock<ready){float duration=primary?OriginSkillCooldown(id):CompactElementCooldown(id);int phase=Mathf.Clamp(Mathf.CeilToInt((ready-clock)/Mathf.Max(.1f,duration)*32),1,32);
    GUI.DrawTexture(r,CompactMask(phase));CompactLabel(r,(ready-clock).ToString("0.0"),17,C("ffffff"),TextAnchor.MiddleCenter);
   }else if((primary?(origin==Origin.Dark?darkPower:energy):elementPower)+.001f<cost){HudRect(r,new Color(.02f,.02f,.03f,.5f));HudRect(new Rect(r.x+7,r.yMax-6,r.width-14,2),C("d58d70"));}
   DrawCompactKey(r,key);HudRect(new Rect(r.x-7,r.yMax+2,r.width+14,20),new Color(.035f,.065f,.09f,.9f));CompactLabel(new Rect(r.x-7,r.yMax+2,r.width+14,20),name,13,known?tint:C("a4afb3"),TextAnchor.MiddleCenter);
   if(primary&&id==0&&lightChannel)HudRect(new Rect(r.x+4,r.yMax-4,r.width-8,2),C("fff2bb"));
   if(r.Contains(Event.current.mousePosition)){
    string detail=!known?(primary?OriginLearningPlace(id):"尚未领悟 · 探索各岛记录"):(primary?(origin==Origin.Dark?"贤者之石能量 ":"光之力 "):"元素之力 ")+cost.ToString("0.#");
    if(known&&primary&&origin==Origin.Light&&id==0)detail="启动 4 · 照射 16 / 秒 · 松开停止";
    DrawCompactTip(Screen.width/HudScaleFor(Screen.width,Screen.height),Screen.height/HudScaleFor(Screen.width,Screen.height),key+"  "+name,detail);
   }
  }
  private void DrawCompactTip(float w,float h,string title,string detail){Rect r=new Rect(w*.5f-235,h-168,470,54);CompactPanel(r);CompactLabel(new Rect(r.x+12,r.y+3,r.width-24,22),title,15,C("eed39b"));CompactLabel(new Rect(r.x+12,r.y+27,r.width-24,21),detail,13,C("d0dddd"));}
  private void DrawCompactContext(float w,float h,string text){
   if(string.IsNullOrEmpty(text))return;
   float width=Mathf.Min(540,Mathf.Max(220,journeySmallStyle.CalcSize(new GUIContent(text)).x+28));
   Vector3 at=view.WorldToScreenPoint(position+Vector2.up*1.6f);float scale=HudScaleFor(Screen.width,Screen.height);
   float x=Mathf.Clamp(at.x/scale-width*.5f,16,w-width-16),y=Mathf.Clamp((Screen.height-at.y)/scale,150,h-190);
   // Detailed pickup summaries can wrap without running across the skill dock.
   Rect r=new Rect(x,y,width,text.Length>42?53:32);CompactPanel(r);FittedLabel(new Rect(x+10,y+5,width-20,r.height-10),text,CompactText(14,C("e6e4d6")),true,TextAnchor.MiddleCenter);
  }
  private Texture2D CompactMask(int phase){Texture2D t;if(compactMasks.TryGetValue(phase,out t)&&t!=null)return t;
   t=new Texture2D(64,64,TextureFormat.RGBA32,false);t.filterMode=FilterMode.Point;for(int y=0;y<64;y++)for(int x=0;x<64;x++){
    float angle=Mathf.Repeat(Mathf.Atan2(x-31.5f,y-31.5f)/(Mathf.PI*2)+1,1);t.SetPixel(x,y,angle<=phase/32f?new Color(.015f,.025f,.035f,.78f):Color.clear);
   }t.Apply();compactMasks[phase]=t;ownedAssets.Add(t);return t;
  }
  private static float IconLine(Vector2 p,float ax,float ay,float bx,float by){Vector2 a=new Vector2(ax,ay),d=new Vector2(bx-ax,by-ay);return Vector2.Distance(p,a+d*Mathf.Clamp01(Vector2.Dot(p-a,d)/Mathf.Max(.001f,d.sqrMagnitude)));}
  private Texture2D CompactIcon(int id){
   Texture2D t;if(compactIcons.TryGetValue(id,out t)&&t!=null)return t;
   if(!compactAtlasChecked){compactAtlas=Resources.Load<Texture2D>("NakiArt/HudV41/skill-icons");compactAtlasChecked=true;}
   if(id>=0&&id<20&&compactAtlas!=null&&compactAtlas.isReadable){
    t=new Texture2D(64,64,TextureFormat.RGBA32,false);t.filterMode=FilterMode.Point;t.wrapMode=TextureWrapMode.Clamp;
    int column=id%5,row=3-id/5;for(int y=0;y<64;y++)for(int x=0;x<64;x++)t.SetPixel(x,y,compactAtlas.GetPixelBilinear((column+(x+.5f)/64)/5f,(row+(y+.5f)/64)/4f));
    t.name="Painted ability icon / "+id;t.Apply();compactIcons[id]=t;ownedAssets.Add(t);return t;
   }
   t=new Texture2D(64,64,TextureFormat.RGBA32,false);t.filterMode=FilterMode.Point;Color tint=C(id<3||id==23?"f2ce7f":id<6||id==22?"c49bea":id==20?"eb8287":id==21?"7ed8e6":"7de0c4");
   if(id==8||id==9)tint=C("d6c17a");if(id==10||id==11)tint=C("a3dfee");if(id==12||id==13)tint=C("efae73");if(id==16||id==17)tint=C("a8cc7e");if(id==19||id==25)tint=C("d7dfdf");
   for(int y=0;y<64;y++)for(int x=0;x<64;x++){
    Vector2 p=new Vector2(x,y),d=p-new Vector2(32,32);float radius=d.magnitude,angle=Mathf.Atan2(d.y,d.x),ink=100;
    switch(id){
     case 0:ink=Mathf.Min(IconLine(p,12,12,51,51),IconLine(p,17,10,54,47)+1);if(x>42&&y>43&&x+y<106)ink=Mathf.Min(ink,1);break;
     case 1:case 21:ink=Mathf.Min(IconLine(p,14,49,50,49),Mathf.Min(IconLine(p,14,49,17,26),Mathf.Min(IconLine(p,50,49,47,26),Mathf.Min(IconLine(p,17,26,32,12),IconLine(p,47,26,32,12)))));if(id==1)ink=Mathf.Min(ink,Mathf.Abs(radius-25)+1);break;
     case 2:case 23:ink=Mathf.Abs(radius-11);if(radius<8)ink=0;for(int k=0;k<8;k++){float a=k*Mathf.PI/4;ink=Mathf.Min(ink,IconLine(p,32+Mathf.Cos(a)*17,32+Mathf.Sin(a)*17,32+Mathf.Cos(a)*25,32+Mathf.Sin(a)*25));}break;
     case 3:ink=Mathf.Abs(Vector2.Distance(p,new Vector2(22,24))-10);ink=Mathf.Min(ink,Mathf.Min(IconLine(p,20,36,47,52),IconLine(p,31,29,55,42)));break;
     case 4:ink=Mathf.Min(Mathf.Abs(radius-19),Mathf.Abs(radius-11));ink=Mathf.Min(ink,Mathf.Min(IconLine(p,32,8,32,55),IconLine(p,9,32,55,32)));break;
     case 5:for(int k=0;k<5;k++){Vector2 center=new Vector2(15+k*8,18+(k%2)*13);ink=Mathf.Min(ink,Mathf.Abs(Vector2.Distance(p,center)-5));ink=Mathf.Min(ink,IconLine(p,center.x,center.y+7,center.x+6,center.y+17)+1);}break;
     case 6:case 14:ink=Mathf.Abs(radius-(18+5*Mathf.Sin(angle*2)));if(d.x+d.y>12)ink+=5;if(id==14)ink=Mathf.Min(ink,IconLine(p,43,42,49,27));break;
     case 7:case 15:case 24:ink=Mathf.Abs(radius-21);if(radius<18)ink=Mathf.Min(ink,Mathf.Abs(radius-(7+4*Mathf.Sin(angle*2))));if(id==15)ink=Mathf.Min(ink,IconLine(p,32,9,32,55));break;
     case 8:case 9:ink=Mathf.Min(IconLine(p,39,55,23,31),Mathf.Min(IconLine(p,23,31,42,33),IconLine(p,42,33,22,9)));if(id==9)ink=Mathf.Min(ink,Mathf.Abs(Vector2.Distance(p,new Vector2(47,17))-5));break;
     case 10:ink=Mathf.Min(IconLine(p,17,12,31,53),Mathf.Min(IconLine(p,31,53,48,12),IconLine(p,17,12,48,12)));ink=Mathf.Min(ink,IconLine(p,31,53,34,15));break;
     case 11:ink=Mathf.Abs(radius-21);for(int k=0;k<6;k++){float a=k*Mathf.PI/3;ink=Mathf.Min(ink,IconLine(p,32,32,32+Mathf.Cos(a)*25,32+Mathf.Sin(a)*25));}break;
     case 12:case 13:ink=Mathf.Abs(radius-(14+5*Mathf.Sin(angle*3)));if(id==12){ink=Mathf.Min(ink,IconLine(p,20,42,41,54));}else ink=Mathf.Min(ink,Mathf.Abs(radius-24));break;
     case 16:ink=Mathf.Min(IconLine(p,15,12,23,39),Mathf.Min(IconLine(p,23,39,32,15),Mathf.Min(IconLine(p,32,15,41,53),IconLine(p,41,53,50,12))));break;
     case 17:ink=Mathf.Min(Mathf.Abs(Vector2.Distance(p,new Vector2(22,30))-10),Mathf.Min(Mathf.Abs(Vector2.Distance(p,new Vector2(43,37))-9),Mathf.Abs(Vector2.Distance(p,new Vector2(39,16))-5)));break;
     case 18:ink=Mathf.Min(IconLine(p,12,15,12,48),Mathf.Min(IconLine(p,12,48,32,44),Mathf.Min(IconLine(p,32,44,52,48),Mathf.Min(IconLine(p,52,48,52,15),IconLine(p,32,13,32,44)))));if(y>=18&&y<=38&&y%7<2&&x>17&&x<47)ink=Mathf.Min(ink,1);break;
     case 19:ink=Mathf.Abs(Vector2.Distance(p,new Vector2(39,51))-4);ink=Mathf.Min(ink,Mathf.Min(IconLine(p,35,43,28,29),Mathf.Min(IconLine(p,28,29,15,15),Mathf.Min(IconLine(p,28,29,43,18),Mathf.Min(IconLine(p,35,40,47,33),IconLine(p,35,40,20,37))))));for(int k=0;k<3;k++)ink=Mathf.Min(ink,IconLine(p,8,24+k*9,18,24+k*9));break;
     case 20:{float hx=(x-32)/21f,hy=(y-31)/21f;float q=hx*hx+hy*hy-1;ink=q*q*q-hx*hx*hy*hy*hy<0?0:100;break;}
     case 22:ink=Mathf.Abs(Mathf.Abs(d.x)*.7f+Mathf.Abs(d.y)-23);ink=Mathf.Min(ink,IconLine(p,32,55,28,10));break;
     case 25:ink=Mathf.Min(IconLine(p,18,15,46,15),Mathf.Min(IconLine(p,18,15,18,34),Mathf.Min(IconLine(p,46,15,46,34),IconLine(p,18,34,46,34))));if(y>=34)ink=Mathf.Min(ink,Mathf.Abs(Vector2.Distance(p,new Vector2(32,35))-10));break;
    }
    Color c=Color.clear;if(ink<6.5f){c=tint;c.a=Mathf.Max(0,(6.5f-ink)/18f);}if(ink<3.5f){c=ink<1.2f?Color.Lerp(tint,Color.white,.58f):tint;c.a=ink<2.5f?1:.65f;}t.SetPixel(x,y,c);
   }t.Apply();compactIcons[id]=t;ownedAssets.Add(t);return t;
  }
 }
}
