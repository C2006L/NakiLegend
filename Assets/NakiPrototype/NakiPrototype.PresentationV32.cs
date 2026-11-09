using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype {
 public sealed partial class NakiPrototype {
  private Material capitalRoadMaterial;
  private Sprite royalRoundShot,royalFastShot;
  private void DrawAlignedCapitalRoad(string label,Rect area,int stage){
   Sprite tile=chapterFloors[stage];Vector2 size=tile.bounds.size,origin=new Vector2(-journeyRooms[currentRoom].halfWidth,-journeyRooms[currentRoom].halfHeight);
   var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();Rect tex=tile.rect;float tw=tile.texture.width,th=tile.texture.height;
   for(int y=Mathf.FloorToInt((area.yMin-origin.y)/size.y);origin.y+y*size.y<area.yMax;y++)for(int x=Mathf.FloorToInt((area.xMin-origin.x)/size.x);origin.x+x*size.x<area.xMax;x++){
    Vector2 at=origin+new Vector2(x*size.x,y*size.y);float left=Mathf.Max(area.xMin,at.x),right=Mathf.Min(area.xMax,at.x+size.x),bottom=Mathf.Max(area.yMin,at.y),top=Mathf.Min(area.yMax,at.y+size.y);if(right<=left||top<=bottom)continue;
    int first=vertices.Count;vertices.Add(new Vector3(left,bottom));vertices.Add(new Vector3(right,bottom));vertices.Add(new Vector3(right,top));vertices.Add(new Vector3(left,top));
    float u0=(tex.x+(left-at.x)/size.x*tex.width)/tw,u1=(tex.x+(right-at.x)/size.x*tex.width)/tw,v0=(tex.y+(bottom-at.y)/size.y*tex.height)/th,v1=(tex.y+(top-at.y)/size.y*tex.height)/th;
    uv.Add(new Vector2(u0,v0));uv.Add(new Vector2(u1,v0));uv.Add(new Vector2(u1,v1));uv.Add(new Vector2(u0,v1));triangles.AddRange(new[]{first,first+1,first+2,first,first+2,first+3});
   }
   var mesh=new Mesh{name=label+" / shared floor UV grid"};mesh.SetVertices(vertices);var colors=new List<Color>();for(int i=0;i<vertices.Count;i++)colors.Add(Color.white);mesh.SetColors(colors);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();ownedAssets.Add(mesh);
   if(capitalRoadMaterial==null){capitalRoadMaterial=new Material(Shader.Find("Sprites/Default"));capitalRoadMaterial.color=Color.white;ownedAssets.Add(capitalRoadMaterial);}
   // The stage cells share an atlas. UVs select its exact cell; there is no color overlay or restarted tile origin.
   capitalRoadMaterial.mainTexture=tile.texture;var go=new GameObject(label);go.transform.SetParent(world,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=capitalRoadMaterial;renderer.sortingOrder=-999;
  }
  private bool RoyalProjectile(Shot shot){Enemy source=shot.emitter;return source!=null&&(source.kind==EnemyKind.CapitalKing||source.kind==EnemyKind.CapitalConduit&&source.iceOwner!=null&&source.iceOwner.kind==EnemyKind.CapitalKing);}
  private Sprite RoyalProjectileArt(bool fast){
   Sprite cached=fast?royalFastShot:royalRoundShot;if(cached!=null)return cached;
   const int size=24;var t=new Texture2D(size,size,TextureFormat.RGBA32,false);t.filterMode=FilterMode.Point;t.wrapMode=TextureWrapMode.Clamp;
   for(int y=0;y<size;y++)for(int x=0;x<size;x++){
    Vector2 p=new Vector2((x+.5f)/size,(y+.5f)/size);float radius=(p-Vector2.one*.5f).magnitude*2;Color c=Color.clear;
    if(fast){float edge=(1-p.x)*.48f;if(p.x>.1f&&Mathf.Abs(p.y-.5f)<edge)c=C("2d1018");if(p.x>.19f&&Mathf.Abs(p.y-.5f)<edge-.065f)c=C("f03c50");if(p.x>.36f&&Mathf.Abs(p.y-.5f)<edge-.16f)c=C("fff0e6");}
    else{if(radius<.96f)c=C("2d1018");if(radius<.8f)c=C("ff6573");if(radius<.59f)c=C("9e203b");if(radius<.3f)c=C("d52a42");if(radius<.8f&&Vector2.Distance(p,new Vector2(.33f,.72f))<.105f)c=C("fff1e5");}
    t.SetPixel(x,y,c);
   }t.Apply();cached=Sprite.Create(t,new Rect(0,0,size,size),Vector2.one*.5f,size,0,SpriteMeshType.FullRect);cached.name=fast?"Royal crimson pointed shot":"Royal crimson ring shot";ownedAssets.Add(t);ownedAssets.Add(cached);if(fast)royalFastShot=cached;else royalRoundShot=cached;return cached;
  }
  private void PrepareRoyalBeam(SignatureZone zone){
   zone.royalAim=true;Vector2 aim=(position-(zone.owner.position+Vector2.up*.2f)).normalized;if(aim.sqrMagnitude>.01f)zone.dir=aim;
   float cross=aim.x*intentPlayerVelocity.y-aim.y*intentPlayerVelocity.x;zone.gap=(cross<0?-1:1)*(56+zone.owner.difficultyPhase*8);zone.end=zone.active+3.2f;
  }
  private float DuelBeamWidth(SignatureZone zone){return zone.royalAim?.5f:.32f;}
  private Vector2 DuelBeamDirection(SignatureZone zone){
   Vector2 anchor=zone.owner.position+zone.anchorOffset+(zone.royalAim?Vector2.up*.2f:Vector2.zero);
   if(zone.royalAim&&clock<zone.active-.16f){Vector2 aim=position-anchor;if(aim.sqrMagnitude>.01f)zone.dir=aim.normalized;}
   return Quaternion.Euler(0,0,Mathf.Max(0,clock-zone.active)*zone.gap)*zone.dir;
  }
  private Vector2 PositionDuelBeam(SignatureZone zone){
   Vector2 dir=DuelBeamDirection(zone);zone.at=zone.owner.position+zone.anchorOffset+(zone.royalAim?Vector2.up*.2f:Vector2.zero)+dir*(zone.royalAim?zone.owner.radius*.6f:zone.owner.radius+.3f);ClipSignatureBeam(zone,dir,DuelBeamWidth(zone));return dir;
  }
  private bool RoyalBeamActive(Enemy owner){foreach(var z in signatureZones)if(z.owner==owner&&z.mode==8&&z.royalAim&&clock<z.end)return true;return false;}
  private void TickSiegeBallista(Enemy node){
   if(node.tacticCastUntil>0){
    if(clock<node.tacticCastUntil)return;node.tacticCastUntil=0;int before=shots.Count;
    OwnedPressureFan(node,node.position+node.attackDirection*.5f,node.attackDirection,3,8,5.6f,Element.Physical,1.1f);
    for(int i=before;i<shots.Count;i++){Shot bolt=shots[i];bolt.root.name="Siege ballista steel bolt";var renderer=bolt.root.GetComponent<SpriteRenderer>();renderer.sprite=arrowSprite;renderer.color=Color.white;bolt.root.transform.localScale=new Vector3(1f,.7f,1);bolt.root.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(bolt.direction.y,bolt.direction.x)*Mathf.Rad2Deg);RemoveRoundBulletDecorations(bolt);}
    Sfx("bow",false,.6f);return;
   }
   if(clock<node.nextAttack||!EnemyOnScreen(node.position,.3f))return;
   node.attackDirection=(IntentLead(.2f)-node.position).normalized;if(node.attackDirection.sqrMagnitude<.01f)node.attackDirection=Vector2.down;
   node.tacticCastUntil=clock+.55f;node.nextAttack=clock+2.8f;
   var glint=Draw("Ballista loaded bolt glint",arrowSprite,node.position+node.attackDirection*.45f,new Vector2(.4f,.4f),C("e6ecff"),DepthAt(node.position.y)+6);glint.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(node.attackDirection.y,node.attackDirection.x)*Mathf.Rad2Deg);fadingBodies.Add(new FadingBody{root=glint,renderer=glint.GetComponent<SpriteRenderer>(),expires=node.tacticCastUntil});
  }
 }
}
