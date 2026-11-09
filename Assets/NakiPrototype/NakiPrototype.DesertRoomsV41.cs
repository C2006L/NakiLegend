using UnityEngine;
namespace NakiLegend.Prototype {
 public sealed partial class NakiPrototype {
  private Sprite desertTeachingArt;
  private GameObject desertLandmark;
  private Vector2 desertLandmarkAt;
  private int desertLandmarkRoom=-1;
  private bool DesertLandmarksActive {get{return !ThunderActive&&!GatehouseActive&&!gatehouseSampleMode&&!bossPracticeMode;}}
  private Sprite DesertTeachingSprite {
   get {
    if(desertTeachingArt!=null)return desertTeachingArt;
    // A grounded sandstone relief: engraved wind paths, worn cap and shaded plinth.
    const int width=48,height=64;var tex=new Texture2D(width,height,TextureFormat.RGBA32,false);tex.filterMode=FilterMode.Point;
    for(int y=0;y<height;y++)for(int x=0;x<width;x++){
     Color c=Color.clear;int edge=y>53?9+(y-53)/2:8;
     if(y>=7&&y<=60&&x>=edge&&x<width-edge){float grain=((x*17+y*31)%11)/110f;c=Color.Lerp(C("95764f"),C("c6a470"),x<13?.1f:.6f+grain);if(x==edge||y==60)c=C("e0bd83");if(x>width-edge-4)c=C("725b43");}
     if(y>=12&&y<=49&&x>=13&&x<=34)c=Color.Lerp(C("9e8057"),C("b89765"),((x*7+y*11)%9)/12f);
     if(y>=3&&y<=10&&x>=4&&x<=43)c=y>=8?C("d5b47e"):y>=5?C("99794f"):C("68523e");
     if(y>=20&&y<=44&&x>=16&&x<=31){float curve=23+6*Mathf.Sin((y-20)*.24f);if(Mathf.Abs(x-curve)<1.3f)c=C("65563f");if(Mathf.Abs(x-curve-1.8f)<.7f)c=C("dfbe84");}
     if(y>=16&&y<=18&&x>=17&&x<=31)c=C("735e42");
     tex.SetPixel(x,y,c);
    }
    tex.Apply();desertTeachingArt=Sprite.Create(tex,new Rect(0,0,width,height),new Vector2(.5f,.08f),height/1.3f,0,SpriteMeshType.FullRect);desertTeachingArt.name="Desert engraved sandstone lesson";ownedAssets.Add(tex);ownedAssets.Add(desertTeachingArt);return desertTeachingArt;
   }
  }
  private void BuildDesertLandmark(RoomNode n){
   desertLandmark=null;desertLandmarkRoom=-1;if(!DesertLandmarksActive||n.kind!=RoomKind.Entrance&&n.kind!=RoomKind.Rest&&n.kind!=RoomKind.Event)return;
   desertLandmarkAt=FindFreeInteractionPoint(new Vector2(n.kind==RoomKind.Rest?2.2f:-2.2f,1));
   if(lessonAltar!=null&&Vector2.Distance(desertLandmarkAt,lessonPosition)<2)desertLandmarkAt=FindFreeInteractionPoint(new Vector2(3,1.5f));
   Sprite art=n.kind==RoomKind.Rest?PressureProp(0):DesertTeachingSprite;
   desertLandmark=Draw("Desert landmark / "+n.kind,art,desertLandmarkAt,Vector2.one*(1.25f/Mathf.Max(.01f,art.bounds.size.y)),Color.white,DepthAt(desertLandmarkAt.y));desertLandmarkRoom=n.id;
  }
  private string DesertLandmarkPrompt(){
   if(!DesertLandmarksActive||desertLandmark==null||desertLandmarkRoom!=currentRoom||Vector2.Distance(position,desertLandmarkAt)>1.5f)return "";
   RoomNode n=journeyRooms[currentRoom];return n.kind==RoomKind.Rest?"F 查看驿站补给记录":n.kind==RoomKind.Entrance?"F 查看鸣沙洲旧航路":"F 研读守渠人的记录";
  }
  private bool TryDesertLandmarkInteraction(){
   if(DesertLandmarkPrompt().Length==0)return false;RoomNode n=journeyRooms[currentRoom];
   if(n.kind==RoomKind.Rest)TellStory("木箱上留着守渠人的记号：饮水、绷带与元素结晶，供后来者取用。你已经在这里整理过一次行装。","箱底的字迹提醒你：每一处驿站只备一份补给。沿已打开的石门向深处走，击败守关者后再寻找通往下一层的石阶。");
   else if(n.kind==RoomKind.Entrance)TellStory("航图上的墨线止于山口。渡船把你留在鸣沙洲，眼前的旧渠却一路伸进埋沙的石城。","图旁有船夫补下的一句：先在外庭学会驾驭掌中的力量，再去追查断渠。守关者身后的石阶，才通往下一层。");
   else TellDesertRecord(n);return true;
  }
  private void TellDesertRecord(RoomNode n){
   if(n.stage==0)TellStory("石碑记着最后一次巡渠的日期。守渠人起初只发现井水变浅，随后整条水道的风都转向山口，雨云再也没有越过石城。","碑背是一幅简图：外庭、断渠、核心。有人在核心旁反复刻下同一句话——不要只把堵塞的沙挖开，要先找到使水停下的东西。");
   else if(n.stage==1)TellStory("巡渠人的记录被压在断壁下：闸轮未坏，铜齿仍能转动。可每次打开上游，水只在沟底打一个旋，就消失在深处。","他在末尾写道：若有人能穿过这层遗迹，请去核心找回源石。等水重新流动，再沿归渠水道检查闸门。开闸与降雨，从来是两件事。");
   else TellStory("最后一页记录来自核心入口。字迹被水泡过，留下的痕迹说明：这里并非从未有水，而是有人让源头不再回应外面的季风。","守渠人在图上圈出了返回旧闸的路。找回源石后还得清理闸室、修复水道；让雨落下，只是把水送回村中的第一步。");
  }
 }
}
