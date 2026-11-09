using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private readonly Dictionary<int,Sprite> residentArt=new Dictionary<int,Sprite>();
  private Sprite ResidentSprite(int id)
  {
   Sprite s;if(residentArt.TryGetValue(id,out s)&&s!=null)return s;
   var t=Resources.Load<Texture2D>("NakiArt/LegacyV1/residents");if(t==null)return id==0?chapterLampkeeper:chapterRangerArt;t.filterMode=FilterMode.Point;
   Rect r=new Rect(id%4*t.width/4f,(1-id/4)*t.height/2f,t.width/4f,t.height/2f);
   s=Sprite.Create(t,r,new Vector2(.5f,.03f),r.height/(id==0?1.45f:1.25f),0,SpriteMeshType.FullRect);ownedAssets.Add(s);residentArt[id]=s;return s;
  }
  private void ReserveRefuge(RoomNode n)
  {if(fireChapterMode&&(n.id==43||n.id==44))transitReservations.Add(new Rect(-8.6f,1.2f,4.4f,3.6f));}
  private void BuildCoreRefuge(RoomNode n,bool departing)
  {
   Vector2 at=new Vector2(-6.6f,2.2f);var t=Resources.Load<Texture2D>("NakiArt/LegacyV1/refuge");
   if(t!=null){var s=Sprite.Create(t,new Rect(0,0,t.width,t.height),new Vector2(.5f,.14f),t.width/3.8f,0,SpriteMeshType.FullRect);ownedAssets.Add(s);Draw("Basalt emergency refuge",s,at,Vector2.one,Color.white,DepthAt(at.y)).transform.SetParent(chapterFocus.transform,true);}
   if(!n.cleared){TrackCampCollision(new Vector2(-6.6f,3.1f),new Vector2(3.3f,1.8f),true);return;}
   if(n.id==43&&(fireEvacuation&1)!=0&&!departing||n.id==44&&(fireEvacuation&2)!=0)return;
   for(int i=0;i<2;i++){Vector2 point=new Vector2(-7.1f+i*1.05f,1.65f);GameObject person=Draw("Shelter worker safely emerged",ResidentSprite(n.id==43?(i==0?2:7):(i==0?1:0)),point,Vector2.one,Color.white,DepthAt(point.y));person.transform.SetParent(chapterFocus.transform,true);coreEvacuees.Add(person);}
  }
 }
}
