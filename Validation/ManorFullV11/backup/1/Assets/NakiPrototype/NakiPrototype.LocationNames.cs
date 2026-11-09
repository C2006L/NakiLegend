using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private string LayerName(int island,int stage){if(island==5)return "暮钟岛 · 第 1 层 · 归功外庭";return island==0&&stage>=3?"鸣沙洲 · 尾声 · 归渠水道":AtlasNames[Mathf.Clamp(island,0,4)]+" · 第 "+(stage+1)+" 层 · "+AtlasRegions[Mathf.Clamp(island,0,4)][Mathf.Clamp(stage,0,2)];}
  private string ActiveLayerName(int stage){return LayerName(CurrentIsland.ordinal-1,stage);}
  private void BuildCoastShelter(RoomNode n)
  {
   Vector2 at=new Vector2(-3,n.halfHeight-2);var t=Resources.Load<Texture2D>("NakiArt/LegacyV1/refuge");
   if(t!=null){var s=Sprite.Create(t,new Rect(0,0,t.width,t.height),new Vector2(.5f,.14f),t.width/3.8f,0,SpriteMeshType.FullRect);ownedAssets.Add(s);Draw("Coast basalt shelter embedded in wall",s,at,Vector2.one,Color.white,DepthAt(at.y)).transform.SetParent(chapterFocus.transform,true);}
   TrackCampCollision(at+Vector2.up*.9f,new Vector2(3.3f,1.8f),true);
   if(!n.cleared)return;
   Vector2 action=at+Vector2.down*.65f;AddChapterAction(401,action,chapterRanger?"与获救居民交谈":"转动避难所绞盘 · 打开救援门",BossPropSprite(1));
   if(chapterRanger){Vector2 point=at+new Vector2(1.1f,-.75f);Draw("Coast resident safely emerged",ResidentSprite(7),point,Vector2.one,Color.white,DepthAt(point.y)).transform.SetParent(chapterFocus.transform,true);}
  }
 }
}
