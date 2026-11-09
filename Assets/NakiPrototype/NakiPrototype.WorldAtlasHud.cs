using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private Rect AtlasArtRect(float w,float h)
  {float availableW=w-354,availableH=h-118,width=Mathf.Min(availableW,availableH*1.5f),height=width/1.5f;return new Rect(18+(availableW-width)*.5f,76+(availableH-height)*.5f,width,height);}
  private void AtlasLine(Vector2 a,Vector2 b,Color color)
  {float distance=Vector2.Distance(a,b);for(float d=0;d<distance;d+=15){Vector2 at=Vector2.Lerp(a,b,d/distance);HudRect(new Rect(at.x-2,at.y-2,4,4),color);}}
  private void DrawWorldAtlas(float w,float h)
  {
   if(atlasConfirm){DrawAtlasConfirmation(w,h);return;}
   HudRect(new Rect(0,0,w,h),C("0c1c25"));FittedLabel(new Rect(25,17,w-300,35),"NAKI · 群岛航图",journeyTitleStyle);
   if(FlatButton(new Rect(w-175,17,150,36),"G / ESC 返回")){CloseWorldAtlas();return;}
   int here=CurrentIsland.ordinal-1,stage=journeyRooms[currentRoom].stage;
   FittedLabel(new Rect(25,52,w-55,23),"当前位置："+RoomHeading(journeyRooms[currentRoom])+" · "+(thunderChapterMode?chapterNames[currentRoom]:JourneyRoomTitle(journeyRooms[currentRoom])),journeySmallStyle);
   Rect art=AtlasArtRect(w,h);if(atlasArt!=null)GUI.DrawTexture(art,atlasArt,ScaleMode.StretchToFill);
   Vector2[] points=new Vector2[7];for(int i=0;i<7;i++)points[i]=new Vector2(art.x+art.width*AtlasPoints[i].x,art.y+art.height*AtlasPoints[i].y);
   for(int i=0;i<5;i++)AtlasLine(points[i],points[i+1],new Color(.96f,.83f,.54f,.55f));
   AtlasLine(points[5],points[6],new Color(.96f,.83f,.54f,.3f));
   for(int i=0;i<7;i++){
    Vector2 point=points[i];Color color=i==here?C("f8db8d"):i<7&&atlasFinished[i]?C("7fd3b4"):i<7?C("d7e5df"):C("929fba");
    HudRect(new Rect(point.x-5,point.y-5,10,10),color);
    Rect label=new Rect(Mathf.Clamp(point.x-72,art.x+4,art.xMax-148),point.y+12,144,39);
    if(FlatButton(label,(i==here?"◆ ":"")+AtlasNames[i]+(i==6&&!CapitalTravelAllowed(-1)?" · 待归航":""),atlasIsland==i)){atlasIsland=i;atlasFloor=-1;atlasConfirm=false;atlasMessage="";}
   }
   float x=w-310,y=90,width=286;
   HudRect(new Rect(x-12,y-8,width+24,h-125),C("142b32"));FittedLabel(new Rect(x,y,width,36),AtlasNames[atlasIsland],journeyTitleStyle);
   string progress=atlasIsland==6&&!CapitalTravelAllowed(-1)?"五岛与暮钟光之归航后开放":atlasFinished[atlasIsland]?"已完成岛屿尾声":atlasSaved[atlasIsland]?"已有进度 · 已清理 "+atlasClears[atlasIsland]+" 间":"尚未踏足 · 可以自由启程";
   FittedLabel(new Rect(x,y+45,width,44),progress,journeySmallStyle,true);
   if(atlasIsland<7){
    if(FlatButton(new Rect(x,y+99,width,42),"继续当地安全点",atlasFloor<0))atlasFloor=-1;
    for(int i=0;i<3;i++){
     string label=(atlasIsland==here&&stage==i?"◆ 当前 · ":"")+(i+1)+"  "+AtlasRegions[atlasIsland][i];
     if(FlatButton(new Rect(x,y+153+i*51,width,44),label,atlasFloor==i))atlasFloor=i;
    }
    FittedLabel(new Rect(x,y+315,width,92),"选择层区会抵达该层入口。\n前置任务与首领进度保留，\n不会因旅行自动完成。",journeySmallStyle,true);
    string status=here<4&&atlasFinished[here]?"主线推荐："+AtlasNames[here+1]:here==4&&atlasFinished[4]?"主线推荐：暮钟岛 · 大骑士府邸":"主线推荐：继续当前岛屿故事";
    FittedLabel(new Rect(x,y+412,width,50),status,journeySmallStyle,true);
    if(FlatButton(new Rect(x,h-146,width,48),atlasIsland==here&&atlasFloor<0?"返回当前房间":"选择此行程 · F")){RequestAtlasTravel();return;}
   }
   else FittedLabel(new Rect(x,y+110,width,140),"前五岛与暮钟岛三层可自由前往。\n本土王国将在后续章节开放。",journeyTextStyle,true);
   FittedLabel(new Rect(x,h-91,width,65),atlasMessage!=""?atlasMessage:AtlasTravelReady?"清房后可自由出发；装备随身，\n各岛独立保存。":"当前可查看航图；\n清理房间后才能出发。",journeySmallStyle,true);
   FittedLabel(new Rect(25,h-30,w-350,23),"金色：当前位置   青绿：尾声完成   虚线：建议顺序   A / D 选岛 · 1 / 2 / 3 选层",journeySmallStyle);
   if(atlasConfirm)DrawAtlasConfirmation(w,h);
  }
  private void DrawAtlasConfirmation(float w,float h)
  {
   HudRect(new Rect(0,0,w,h),new Color(.02f,.04f,.05f,.86f));Rect box=new Rect(w/2-355,h/2-170,710,340);HudRect(box,C("16323a"));
   FittedLabel(new Rect(box.x+28,box.y+25,654,45),"前往"+AtlasNames[atlasIsland],journeyTitleStyle);
   string destination=atlasFloor<0?(atlasSaved[atlasIsland]?"继续该岛自己的安全存档":"首次登岛，从码头开始"):(atlasIsland==6?"王都 · ":"第 "+(atlasFloor+1)+" 层 · ")+AtlasRegions[atlasIsland][atlasFloor]+"入口";
   FittedLabel(new Rect(box.x+28,box.y+85,654,110),destination+"\n当前安全点会先保存，装备与光之力与贤者之石资源随身携带。\n旅行不会代替任务、播放结尾或领取天赋。",journeyTextStyle,true);
   if(FlatButton(new Rect(box.x+28,box.y+230,315,60),"留下 · ESC")){atlasConfirm=false;BlockWorldInteraction();return;}
   if(FlatButton(new Rect(box.x+367,box.y+230,315,60),"确认出发 · F")&&Time.unscaledTime>=atlasOpened+.3f)TravelAtlas(atlasIsland,atlasFloor);
  }
 }
}
