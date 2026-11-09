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
   Sprite art=ManorVillageProp(cell,width);Rect visible=ManorBuildingFootprint(art,at);Vector2 footprint=visible.size;
   if(IntersectsTransit(visible.center,footprint))return;
   Draw("Manor village building / "+cell,art,at,Vector2.one,Color.white,DepthAt(at.y));
   // Reserve the visible silhouette so actors cannot disappear beneath the roof.
   AddManorBuildingCollision(art,at);
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
    if(ManorReturnReady)return currentRoom==0?0:44;
    if(manorEnding!=0)return -1;
    if(!journeyRooms[14].cleared)return (chapterWaymarks&1)==0?2:(chapterWaymarks&2)==0?7:!journeyRooms[4].cleared?4:!journeyRooms[13].cleared?13:14;
    if((manorProgress&1)==0)return journeyRooms[18].cleared?17:18;
    if((manorProgress&2)==0)return journeyRooms[20].cleared?19:20;
    if((manorProgress&4)==0)return 24;
    if(!journeyRooms[29].cleared)return 29;if(journeyRooms[44].cleared)return 44;
    return manorLetters!=7?32:!journeyRooms[34].storyRead?34:!journeyRooms[41].storyRead?41:!journeyRooms[43].storyRead?43:44;
   }
  }
  private ChapterAction ManorFlowAction()
  {
   if(ManorReturnReady&&(currentRoom==0||currentRoom==44))return FlowAction(724);
   if(currentRoom==14&&journeyRooms[14].cleared)return FlowAction(606);
   if(currentRoom==15)return FlowAction(610);
   if(currentRoom==29&&journeyRooms[29].cleared)return FlowAction(615);
   if(currentRoom==30)return FlowAction(616);
   if(currentRoom==32&&manorLetters!=7)return FlowAction((manorLetters&1)==0?730:(manorLetters&2)==0?731:732);
   if(currentRoom==44)return FlowAction(720,721,722,723);
   if(currentRoom==32)return null;
   if(currentRoom==34||currentRoom==41)return journeyRooms[currentRoom].storyRead?null:FlowAction(617);
   if(currentRoom==43)return journeyRooms[43].storyRead?null:FlowAction(618);
   return FlowAction(602,603,611,612,613,614,617,618,619,620,601);
  }
 }
}
