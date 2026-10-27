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
  {return NearestSafeFooting(entry>=0?DoorPoint(entry,-1.9f):new Vector2(0,-journeyRooms[currentRoom].halfHeight+1.6f));}
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
   TileFloor("Door approach",center+n*.9f,horizontal?new Vector2(2.4f,2.3f):new Vector2(2.3f,2.4f),stage,-994);
   float angle=direction==0?0:direction==1?180:direction==2?-90:90;
   if(gateFrameV12!=null){GameObject frame=Stretch("Carved doorway",gateFrameV12,center+n*.32f,new Vector2(4.2f,1.85f),Color.white,DepthAt(center.y)+4);frame.transform.rotation=Quaternion.Euler(0,0,angle);}
   Vector2 tangent=new Vector2(-n.y,n.x);
   foreach(float sign in new[]{-1f,1f})
   {
    Vector2 post=center+tangent*sign*1.65f;Vector2 footprint=horizontal?new Vector2(.65f,1.1f):new Vector2(1.1f,.65f);
    Rect collision=new Rect(post-footprint*.5f,footprint);obstacles.Add(collision);projectileObstacles.Add(collision);
   }
   var gate=new GameObject("Closed bronze portcullis");gate.transform.SetParent(world,false);gate.transform.position=center;
   for(int i=-4;i<=4;i++){GameObject bar=Stretch("Gate iron bar",square,center+Vector2.right*i*.235f,new Vector2(.08f,1.3f),C("775b3f"),620);bar.transform.SetParent(gate.transform,true);}
   foreach(float y in new[]{-.45f,.45f}){GameObject beam=Stretch("Gate crossbeam",square,center+Vector2.up*y,new Vector2(2.08f,.13f),C("be9357"),621);beam.transform.SetParent(gate.transform,true);}
   GameObject seal=Draw("Gate wind lock",windRingSprite??disc,center,Vector2.one*.5f,C("e88c67"),623);seal.transform.SetParent(gate.transform,true);gate.transform.rotation=Quaternion.Euler(0,0,angle);doorSeals.Add(gate);
   Rect block=horizontal?new Rect(-1.18f,center.y-.35f,2.36f,.7f):new Rect(center.x-.35f,-1.18f,.7f,2.36f);doorShotBlocks.Add(block);projectileObstacles.Add(block);
  }
  private string StairDestination()
  {
   RoomNode n=journeyRooms[currentRoom];if(n.stairsTo<0)return "";RoomNode target=journeyRooms[n.stairsTo];return (target.stage>n.stage?"前往":"返回")+"第 "+(target.stage+1)+" 层 · "+StageNames[target.stage];
  }
 }
}
