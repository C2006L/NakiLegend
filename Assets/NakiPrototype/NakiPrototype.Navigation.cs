using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private readonly Sprite[] terrainV12=new Sprite[3];
  private Sprite gateFrameV12,stairsV12,masonryV12;
  private readonly List<Rect> transitReservations=new List<Rect>();
  private int arrivalRecoveries;
  private float roomBannerUntil;
  private void LoadDesertV12Art()
  {
   string[] names={"floor-sand","floor-canal","floor-core"};
   for(int i=0;i<3;i++){Texture2D t=Resources.Load<Texture2D>("NakiArt/DesertV12/"+names[i]);if(t==null)continue;t.wrapMode=TextureWrapMode.Repeat;terrainV12[i]=Sprite.Create(t,new Rect(0,0,t.width,t.height),Vector2.one*.5f,t.width/4f,0,SpriteMeshType.FullRect);ownedAssets.Add(terrainV12[i]);}
   gateFrameV12=JourneySprite("DesertV12/door-frame",400,Vector2.one*.5f);stairsV12=JourneySprite("DesertV12/stairs",400,new Vector2(.5f,.15f));
   if(gateFrameV12!=null)masonryV12=ModuleSprite(gateFrameV12.texture,new Rect(.23f,.77f,.15f,.19f));
  }
  private void TileFloor(string name,Vector2 at,Vector2 size,int stage,int order)
  {
   Sprite s=terrainV12[stage]??floorModules[stage];GameObject obj=Draw(name,s,at,Vector2.one,Color.white,order);var sr=obj.GetComponent<SpriteRenderer>();sr.drawMode=SpriteDrawMode.Tiled;sr.size=size;
  }
  private void ReserveTransitSpace(RoomNode node)
  {
   transitReservations.Clear();
   for(int d=0;d<4;d++)if(HasDoor(d)){Vector2 c=DoorPoint(d,-.7f),size=d<2?new Vector2(3.8f,4.6f):new Vector2(4.6f,3.8f);transitReservations.Add(new Rect(c-size*.5f,size));}
   transitReservations.Add(new Rect(-1.9f,-node.halfHeight-.25f,3.8f,3.6f));
   if(node.stairsTo>=0){Vector2 c=PreferredStairPoint(node);transitReservations.Add(new Rect(c-new Vector2(1.65f,1.15f),new Vector2(3.3f,2.9f)));}
  }
  private Vector2 PreferredStairPoint(RoomNode node){return new Vector2(node.halfWidth-2.15f,node.halfHeight-2.0f);}
  private bool IntersectsTransit(Vector2 at,Vector2 footprint)
  {Rect candidate=new Rect(at-footprint*.5f,footprint);foreach(Rect r in transitReservations)if(r.Overlaps(candidate))return true;return false;}
  private bool SafeFooting(Vector2 at,float radius)
  {
   if(!CanStandInExploration(at,radius)||Blocked(at,radius)||ProjectilePathBlocked(at,at,.18f))return false;
   int exits=0;foreach(Vector2Int n in Cardinal){Vector2 next=at+(Vector2)n*.45f;if(CanStandInExploration(next,radius)&&!Blocked(next,radius))exits++;}return exits>=3;
  }
  private Vector2 SafeArrivalPoint(int entry)
  {return NearestSafeFooting(entry>=0?DoorPoint(entry,-1.9f):CoastalHarborRoom||RainQuayRoom?new Vector2(-1,journeyRooms[currentRoom].halfHeight*.28f):new Vector2(0,-journeyRooms[currentRoom].halfHeight+1.6f));}
  private Vector2 NearestSafeFooting(Vector2 desired)
  {
   if(SafeFooting(desired,.34f))return desired;
   Rect b=CurrentRoomBounds;Vector2 best=Vector2.zero;float score=float.MaxValue;
   for(float y=b.yMin+.8f;y<=b.yMax-.8f;y+=.3f)for(float x=b.xMin+.8f;x<=b.xMax-.8f;x+=.3f){Vector2 p=new Vector2(x,y);float d=(p-desired).sqrMagnitude;if(d<score&&SafeFooting(p,.34f)){best=p;score=d;}}
   if(score==float.MaxValue)throw new System.InvalidOperationException("No safe arrival point in room "+currentRoom);return best;
  }
  private void BuildReadableGate(int direction,int stage)
  {
   Vector2 n=DoorNormal(direction),center=DoorPoint(direction);bool horizontal=direction<2;
   TileFloor("Door approach",center+n*.9f,horizontal?new Vector2(2.8f,2.3f):new Vector2(2.3f,2.8f),stage,-994);
   BuildIslandGate(direction);
  }

  private Rect DoorSealCollision(int direction){Vector2 c=DoorPoint(direction);return direction<2?new Rect(c.x-1.18f,c.y-.35f,2.36f,.7f):new Rect(c.x-.35f,c.y-1.18f,.7f,2.36f);}
  private string StairDestination()
  {
   RoomNode n=journeyRooms[currentRoom];if(thunderChapterMode&&n.stairsTo>=0)return (journeyRooms[n.stairsTo].stage>n.stage?"前往":"返回")+ActiveLayerName(journeyRooms[n.stairsTo].stage);if(n.stairsTo<0)return "";RoomNode target=journeyRooms[n.stairsTo];if(IsGatehouseRoom(target))return "前往归渠水道";if(IsGatehouseRoom(n))return "返回古渠核心";return (target.stage>n.stage?"前往":"返回")+"第 "+(target.stage+1)+" 层 · "+StageNames[target.stage];
  }
 }
}
