using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private readonly Dictionary<int,Sprite> worldPropSprites=new Dictionary<int,Sprite>();
  private readonly List<Rect> woodPropBlocks=new List<Rect>();
  private int woodPropRoom=-1;
  private Sprite WorldPropSprite(int index)
  {
   Sprite sprite;if(worldPropSprites.TryGetValue(index,out sprite)&&sprite!=null)return sprite;
   Texture2D atlas=Resources.Load<Texture2D>("NakiArt/WorldPropsV1/props");if(atlas==null)return null;atlas.filterMode=FilterMode.Point;
   float height=index<2?2.6f:index==5?2.4f:index==6?1:1.35f;
   Rect cell=new Rect(index%4*atlas.width/4f,atlas.height-(index/4+1)*atlas.height/2f,atlas.width/4f,atlas.height/2f);
   sprite=Sprite.Create(atlas,cell,new Vector2(.5f,.12f),cell.height/height,0,SpriteMeshType.FullRect);ownedAssets.Add(sprite);worldPropSprites[index]=sprite;return sprite;
  }
  private void WoodProp(int index,Vector2 at,string label)
  {
   GameObject visual=Draw(label,WorldPropSprite(index),at,Vector2.one,index==3&&!chapterLampGiven?new Color(.65f,.52f,.3f):Color.white,DepthAt(at.y));visual.transform.SetParent(chapterFocus.transform,true);
  }
  private void WoodPropAction(int kind,int index,Vector2 at,string label)
  {AddChapterAction(kind,at+Vector2.down*.55f,label);WoodProp(index,at,label);}
  private void ClearWoodPropBlocks()
  {
   if(woodPropRoom==currentRoom)foreach(Rect rect in woodPropBlocks){obstacles.Remove(rect);projectileObstacles.Remove(rect);}
   woodPropBlocks.Clear();woodPropRoom=currentRoom;
  }
  private void AddWoodPropBlock(Rect rect){woodPropBlocks.Add(rect);obstacles.Add(rect);projectileObstacles.Add(rect);}
  private void BuildSeedCollectorCage()
  {
   Vector2 at=new Vector2(-2,0);WoodProp(chapterRanger?1:0,at,chapterRanger?"Opened seed collector cage":"Locked seed collector cage");
   GameObject person=Draw(chapterRanger?"Freed seed collector":"Captive seed collector",chapterRangerArt,chapterRanger?new Vector2(.3f,.2f):at+Vector2.up*.4f,Vector2.one,Color.white,DepthAt(chapterRanger?.2f:.4f));person.transform.SetParent(chapterFocus.transform,true);
   AddWoodPropBlock(new Rect(-3.05f,.05f,.18f,1.15f));AddWoodPropBlock(new Rect(-1.13f,.05f,.18f,1.15f));AddWoodPropBlock(new Rect(-3.05f,1.02f,2.1f,.18f));
   if(!chapterRanger)AddWoodPropBlock(new Rect(-3.05f,-.12f,2.1f,.22f));
   AddChapterAction(chapterRanger?210:203,chapterRanger?new Vector2(.3f,-.4f):new Vector2(-2,-.7f),chapterRanger?"与获救采种人交谈":"用钥匙打开牢笼 · 解救采种人");
  }
  private void DrawWorldSign(Vector2 at,Transform parent)
  {
   GameObject root=new GameObject("Trail direction sign");root.transform.SetParent(parent,false);
   GameObject post=Stretch("Sign post",square,at+Vector2.up*.35f,new Vector2(.12f,.9f),C("775334"),DepthAt(at.y));post.transform.SetParent(root.transform,true);
   GameObject board=Stretch("Trail arrow board",square,at+Vector2.up*.7f,new Vector2(.9f,.3f),C("b49459"),DepthAt(at.y)+1);board.transform.SetParent(root.transform,true);
   foreach(float slope in new[]{-30f,30f}){GameObject line=Stretch("Direction arrow",square,at+new Vector2(.25f,.7f),new Vector2(.24f,.045f),C("e5dcc0"),DepthAt(at.y)+2);line.transform.rotation=Quaternion.Euler(0,0,slope);line.transform.SetParent(root.transform,true);}
  }
  private void WoodTrailSign(Vector2 at,int action,string label){DrawWorldSign(at,chapterFocus.transform);AddChapterAction(action,at+Vector2.down*.4f,label);}
 }
}
