using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private Sprite ManorVillageProp(int cell,float width)
  {
   string key="village/"+cell+"/"+width;Sprite sprite;
   if(manorSprites.TryGetValue(key,out sprite)&&sprite!=null)return sprite;
   var texture=Resources.Load<Texture2D>("NakiArt/ManorVillageV4/props");
   if(texture==null)throw new System.InvalidOperationException("Missing Manor village artwork");
   texture.filterMode=FilterMode.Point;
   float size=texture.width/3f;
   sprite=Sprite.Create(texture,new Rect(cell%3*size,(1-cell/3)*size,size,size),new Vector2(.5f,.09f),size/width,0,SpriteMeshType.FullRect);
   sprite.name="Manor village / "+cell;ownedAssets.Add(sprite);manorSprites[key]=sprite;return sprite;
  }
  private void ManorVillageBuilding(int cell,Vector2 at,float width)
  {
   Vector2 footprint=new Vector2(width*.76f,1.05f);
   if(IntersectsTransit(at+Vector2.up*.45f,footprint))return;
   Draw("Manor village building / "+cell,ManorVillageProp(cell,width),at,Vector2.one,Color.white,DepthAt(at.y));
   // Only the ground footprint blocks walking; roof and awning do not.
   Rect block=new Rect(at.x-footprint.x*.5f,at.y-.08f,footprint.x,footprint.y);
   obstacles.Add(block);projectileObstacles.Add(block);
  }
  private void AddManorPassage(int action,int target)
  {
   RoomNode n=journeyRooms[currentRoom];Vector2 at=PreferredStairPoint(n);
   string label=(journeyRooms[target].stage>n.stage?"前往":"返回")+ActiveLayerName(journeyRooms[target].stage);
   AddChapterAction(action,at,label,ManorVillageProp(3,2.5f));
  }
  private int ManorFlowGoal
  {
   get{
    if(manorEnding!=0)return -1;
    if(!journeyRooms[14].cleared)return (chapterWaymarks&1)==0?2:(chapterWaymarks&2)==0?7:!journeyRooms[4].cleared?4:!journeyRooms[13].cleared?13:14;
    if((manorProgress&1)==0)return journeyRooms[18].cleared?17:18;
    if((manorProgress&2)==0)return journeyRooms[20].cleared?19:20;
    if((manorProgress&4)==0)return 24;
    return !journeyRooms[29].cleared?29:44;
   }
  }
  private ChapterAction ManorFlowAction()
  {
   if(currentRoom==14&&journeyRooms[14].cleared)return FlowAction(606);
   if(currentRoom==15)return FlowAction(610);
   if(currentRoom==29&&journeyRooms[29].cleared)return FlowAction(615);
   if(currentRoom==30)return FlowAction(616);
   if(currentRoom==44)return FlowAction(720,721,722,723);
   return FlowAction(602,603,611,612,613,614,617,618,619,620,601);
  }
 }
}
