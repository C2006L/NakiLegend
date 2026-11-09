using UnityEngine;
namespace NakiLegend.Prototype {
 public sealed partial class NakiPrototype {
  private readonly Sprite[] chapterRecordArt=new Sprite[7];
  private Sprite ChapterRecordSprite {
   get {
    int chapter=Mathf.Clamp(CurrentIsland.ordinal-1,0,6);if(chapterRecordArt[chapter]!=null)return chapterRecordArt[chapter];
    // Use a grounded, illustrated lectern at a fixed world height for all field records.
    // The old 48px placeholder looked like a giant HUD icon in the local camera.
    Sprite source=ManorSprite("prop-7",1);Texture2D texture=source.texture;
    Sprite sprite=Sprite.Create(texture,source.rect,new Vector2(.5f,.08f),source.rect.height/1.15f,0,SpriteMeshType.FullRect);
    sprite.name="Chapter record / "+chapter;ownedAssets.Add(sprite);chapterRecordArt[chapter]=sprite;return sprite;
   }
  }
  private Rect ManorBuildingFootprint(Sprite art,Vector2 at){
   // Reserve the whole visible building, with a small inset for transparent edge padding.
   Bounds b=art.bounds;return Rect.MinMaxRect(at.x+b.min.x+ b.size.x*.07f,at.y+b.min.y+b.size.y*.06f,at.x+b.max.x-b.size.x*.07f,at.y+b.max.y-b.size.y*.12f);
  }
  private void AddManorBuildingCollision(Sprite art,Vector2 at){
   Rect cell=art.rect;int width=Mathf.RoundToInt(cell.width),height=Mathf.RoundToInt(cell.height);
   Color[] pixels=art.texture.GetPixels(Mathf.RoundToInt(cell.x),Mathf.RoundToInt(cell.y),width,height);
   const int bands=20;
   for(int band=0;band<bands;band++){
    int low=band*height/bands,high=(band+1)*height/bands,left=width,right=-1;
    for(int y=low;y<high;y+=2)for(int x=0;x<width;x+=2)if(pixels[y*width+x].a>.25f){left=Mathf.Min(left,x);right=Mathf.Max(right,x);}
    if(right-left<width*.05f)continue;
    float unit=1/art.pixelsPerUnit;
    Rect solid=Rect.MinMaxRect(at.x+(left-art.pivot.x)*unit,at.y+(low-art.pivot.y)*unit,at.x+(right+2-art.pivot.x)*unit,at.y+(high-art.pivot.y)*unit+.015f);
    AddRoundedCover(solid,solid);
   }
  }
  private bool TideWallInProgress(Enemy owner){
   foreach(var zone in signatureZones)if(zone.mode==5&&zone.owner==owner&&clock<zone.end)return true;return false;
  }
  private bool InterceptTideGapShot(Shot shot,Vector2 from,Vector2 to){
   if(!shot.hostile||!iceSampleMode)return false;
   foreach(var zone in signatureZones){
    if(zone.mode!=5||clock<zone.active||clock>=zone.end||zone.owner.hp<=0)continue;
    float y=zone.at.y+(clock-zone.active)*1.8f;
    // Protect the act of crossing, not a distant camper behind the entire moving wall.
    if(Mathf.Abs(position.y-y)>1.15f||Mathf.Abs(position.x-zone.gap)>1.6f)continue;
    float t=Mathf.Abs(to.y-from.y)>.00001f?Mathf.Clamp01((y-from.y)/(to.y-from.y)):0;
    Vector2 near=Vector2.Lerp(from,to,t);
    if(Mathf.Abs(near.y-y)<.95f&&Mathf.Abs(near.x-zone.gap)<1.6f){SpellSpark(near,C("a9e9fa"),.25f);return true;}
   }
   return false;
  }
 }
}
