using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private Sprite[] passageGates,stormFixtures;
  private Sprite PassageSprite(bool storm,int id)
  {
   Sprite[] cache=storm?stormFixtures:passageGates;
   if(cache==null){
    Texture2D t=Resources.Load<Texture2D>("NakiArt/PassageV2/"+(storm?"storm-props":"island-gates"));
    if(t==null)throw new System.InvalidOperationException("Missing passage artwork");
    t.filterMode=FilterMode.Point;t.wrapMode=TextureWrapMode.Clamp;
    int rows=storm?2:3;cache=new Sprite[rows*2];
    for(int i=0;i<cache.Length;i++){
     float cw=t.width/2f,ch=t.height/(float)rows;
     cache[i]=Sprite.Create(t,new Rect((i%2)*cw,(rows-1-i/2)*ch,cw,ch),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);ownedAssets.Add(cache[i]);
    }
    if(storm)stormFixtures=cache;else passageGates=cache;
   }
   return cache[Mathf.Clamp(id,0,cache.Length-1)];
  }
  private void BuildIslandGate(int direction)
  {
   Vector2 center=DoorPoint(direction),normal=DoorNormal(direction),tangent=new Vector2(-normal.y,normal.x);
   Sprite art=PassageSprite(false,CurrentIsland.ordinal-1);float angle=direction<2?0:90;
   // Keep the stone sockets visible when the moving gate has opened.
   foreach(int side in new[]{0,1}){
    Rect r=art.rect;float strip=r.width*.23f;r.x+=side==0?0:r.width-strip;r.width=strip;
    Sprite socket=Sprite.Create(art.texture,r,Vector2.one*.5f,100,0,SpriteMeshType.FullRect);ownedAssets.Add(socket);
    Vector2 at=center+tangent*(side==0?-1.4f:1.4f);
    GameObject post=Stretch("Carved gate socket",socket,at,new Vector2(.84f,1.35f),Color.white,DepthAt(center.y)+5);post.transform.rotation=Quaternion.Euler(0,0,angle);
    Vector2 size=direction<2?new Vector2(.55f,.7f):new Vector2(.7f,.55f);Rect block=new Rect(at-size*.5f,size);obstacles.Add(block);projectileObstacles.Add(block);
   }
   GameObject gate=Stretch("Island sealed gate",art,center,new Vector2(3.65f,1.35f),Color.white,DepthAt(center.y)+6);
   gate.transform.rotation=Quaternion.Euler(0,0,angle);doorSeals.Add(gate);doorSealDirections.Add(direction);
  }
  private GameObject StormFixture(int id,Vector2 at,float size,Transform parent=null)
  {
   Sprite art=PassageSprite(true,id);float aspect=art.bounds.size.x/art.bounds.size.y;
   GameObject obj=Stretch("Authored storm fixture "+id,art,at+Vector2.up*(size*.22f),new Vector2(size*aspect,size),Color.white,DepthAt(at.y));
   if(parent!=null)obj.transform.SetParent(parent,true);return obj;
  }
  private void ResetChapterCooldowns()
  {
   chapterMarkReady=chapterArcReady=iceSpikeReady=iceRingReady=fireBoltReady=emberRingReady=0;
  }
 }
}
