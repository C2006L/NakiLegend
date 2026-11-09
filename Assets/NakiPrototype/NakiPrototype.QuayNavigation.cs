using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private bool RainQuayRoom {get{return !thunderChapterMode&&GatehouseActive&&CurrentGatehouseIndex==4;}}
  private Sprite rainQuayArt;
  private Rect QuayRect(float left,float top,float width,float height)
  {RoomNode n=journeyRooms[currentRoom];return new Rect((left-.5f)*n.halfWidth*2,(.5f-top-height)*n.halfHeight*2,width*n.halfWidth*2,height*n.halfHeight*2);}
  private void BuildQuayFooting(bool rain=false)
  {
   var land=new List<Rect>();
   if(rain){land.Add(new Rect(.065f,.16f,.87f,.30f));land.Add(new Rect(.445f,.43f,.087f,.57f));}
   else{
    land.Add(new Rect(.055f,.20f,.80f,.34f));land.Add(new Rect(.855f,.20f,.05f,.23f));land.Add(new Rect(.27f,.12f,.48f,.10f));
    if(fireChapterMode){land.Add(new Rect(.245f,.48f,.047f,.20f));land.Add(new Rect(.462f,.48f,.035f,.22f));land.Add(new Rect(.737f,.48f,.039f,.20f));}
    else{land.Add(new Rect(.363f,.455f,.030f,.28f));land.Add(new Rect(.532f,.455f,.106f,.29f));land.Add(new Rect(.838f,.43f,.034f,.285f));}
   }
   // Exit paths are real pieces of the painted quay, at their raised shoreline height.
   RoomNode node=journeyRooms[currentRoom];
   if(HasDoor(1)){float pier=rain?.489f:fireChapterMode?.48f:.585f;float width=(rain||fireChapterMode?2.4f:2.05f)/(node.halfWidth*2);land.Add(new Rect(pier-width*.5f,.43f,width,rain?.57f:.29f));}
   for(int d=0;d<4;d++)if(HasDoor(d)&&d!=1){
    Vector2 c=DoorPoint(d);float u=c.x/(node.halfWidth*2)+.5f,v=.5f-c.y/(node.halfHeight*2);
    float half=1.48f/(d<2?node.halfWidth*2:node.halfHeight*2);
    if(d==0)land.Add(new Rect(u-half,0,half*2,.25f));
    else land.Add(new Rect(d==3?0:.84f,v-half,.16f,half*2));
   }
   var xs=new SortedSet<float>{0,1};var ys=new SortedSet<float>{0,1};foreach(Rect r in land){xs.Add(r.xMin);xs.Add(r.xMax);ys.Add(r.yMin);ys.Add(r.yMax);}
   float[] x=new float[xs.Count],y=new float[ys.Count];xs.CopyTo(x);ys.CopyTo(y);
   for(int i=0;i<x.Length-1;i++)for(int j=0;j<y.Length-1;j++){
    Vector2 mid=new Vector2((x[i]+x[i+1])*.5f,(y[j]+y[j+1])*.5f);bool dry=false;foreach(Rect r in land)if(r.Contains(mid)){dry=true;break;}if(dry)continue;
    Rect block=QuayRect(x[i],y[j],x[i+1]-x[i],y[j+1]-y[j]);obstacles.Add(block);if(!rain)harborBanks.Add(block);
   }
   if(!rain)harborRoom=currentRoom;
  }
  private void BuildPaintedQuayApproaches(Texture2D texture,bool rain)
  {
   // The paving joins the dry courtyard at the actual door height, including its outer corridor.
   Rect sample=rain?new Rect(.38f,.62f,.08f,.11f):new Rect(.44f,.65f,.10f,.10f);
   Sprite paving=Sprite.Create(texture,new Rect(texture.width*sample.x,texture.height*sample.y,texture.width*sample.width,texture.height*sample.height),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);ownedAssets.Add(paving);
   if(HasDoor(1)&&fireChapterMode){
    Rect source=rain?new Rect(.445f,.47f,.09f,.5f):fireChapterMode?new Rect(.461f,.48f,.038f,.22f):new Rect(.605f,.48f,.048f,.24f);
    Sprite deck=Sprite.Create(texture,new Rect(source.x*texture.width,(1-source.y-source.height)*texture.height,source.width*texture.width,source.height*texture.height),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);ownedAssets.Add(deck);
    float pier=rain?.489f:fireChapterMode?.48f:.585f;Rect place=QuayRect(pier-1.2f/(journeyRooms[currentRoom].halfWidth*2),.43f,2.4f/(journeyRooms[currentRoom].halfWidth*2),rain?.57f:.29f);
    Stretch("Wide dry pier deck",deck,place.center,place.size,Color.white,-994);
   }
   for(int d=0;d<4;d++)if(HasDoor(d)&&d!=1){
    Vector2 normal=DoorNormal(d),at=DoorPoint(d)+normal*.1f;
    var renderer=Draw("Continuous dry quay exit",paving,at,Vector2.one,Color.white,-995).GetComponent<SpriteRenderer>();
    renderer.drawMode=SpriteDrawMode.Tiled;renderer.size=d<2?new Vector2(2.85f,2.8f):new Vector2(2.8f,2.85f);
    BuildIslandGate(d);
   }
  }
  private void BuildRainQuay(RoomNode room)
  {
   gateWaterVisuals.Clear();gateRainVisuals.Clear();gateWheelVisual=gateWorkRing=gateRotor=null;gateSandVisuals[0]=gateSandVisuals[1]=null;
   if(rainQuayArt==null){var t=Resources.Load<Texture2D>("NakiArt/PassageV2/rain-harbor");if(t==null)throw new System.InvalidOperationException("Missing rain quay artwork");t.filterMode=FilterMode.Point;rainQuayArt=Sprite.Create(t,new Rect(0,0,t.width,t.height),Vector2.one*.5f,t.width/16f,0,SpriteMeshType.FullRect);ownedAssets.Add(rainQuayArt);}
   Stretch("Rain lookout quay and moored boat",rainQuayArt,Vector2.zero,new Vector2(room.halfWidth*2,room.halfHeight*2),Color.white,-1000);
   BuildQuayFooting(true);
   BuildPaintedQuayApproaches(rainQuayArt.texture,true);
   EnsureThunderDock();
  }
 }
}
