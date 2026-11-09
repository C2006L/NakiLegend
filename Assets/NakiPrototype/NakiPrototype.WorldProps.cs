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
   Vector2 pivot=new Vector2(.5f,.12f);
   // The boat crosses the nominal chest cell. Trim that gutter while retaining the chest's world anchor and scale.
   if(index==6){float gutter=cell.width*.14f;pivot.x=(cell.width*.5f-gutter)/(cell.width-gutter);cell.xMin+=gutter;}
   sprite=Sprite.Create(atlas,cell,pivot,cell.height/height,0,SpriteMeshType.FullRect);ownedAssets.Add(sprite);worldPropSprites[index]=sprite;return sprite;
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
   GameObject person=Draw(chapterRanger?"Freed seed collector":"Captive seed collector",ResidentSprite(6),chapterRanger?new Vector2(.3f,.2f):at+Vector2.up*.4f,Vector2.one,Color.white,DepthAt(chapterRanger?.2f:.4f));person.transform.SetParent(chapterFocus.transform,true);
   AddWoodPropBlock(new Rect(-3.05f,.05f,.18f,1.15f));AddWoodPropBlock(new Rect(-1.13f,.05f,.18f,1.15f));AddWoodPropBlock(new Rect(-3.05f,1.02f,2.1f,.18f));
   if(!chapterRanger)AddWoodPropBlock(new Rect(-3.05f,-.12f,2.1f,.22f));
   AddChapterAction(chapterRanger?210:203,chapterRanger?new Vector2(.3f,-.4f):new Vector2(-2,-.7f),chapterRanger?"与获救采种人交谈":"用钥匙打开牢笼 · 解救采种人");
  }
  private void DrawWorldSign(Vector2 at,Transform parent,Vector2? direction=null)
  {
   GameObject root=new GameObject("Trail direction sign");root.transform.SetParent(parent,false);root.transform.position=at;
   int depth=DepthAt(at.y);
   System.Action<GameObject> attach=o=>o.transform.SetParent(root.transform,true);
   attach(Stretch("Post outline",square,at+Vector2.up*.35f,new Vector2(.2f,.9f),C("353b43"),depth));
   attach(Stretch("Weathered sign post",square,at+Vector2.up*.35f,new Vector2(.12f,.85f),C("78624d"),depth+1));
   attach(Stretch("Board outline",square,at+Vector2.up*.75f,new Vector2(1.02f,.45f),C("343942"),depth+2));
   attach(Stretch("Wooden direction board",square,at+Vector2.up*.75f,new Vector2(.94f,.36f),C("806b52"),depth+3));
   foreach(float y in new[]{.63f,.85f})attach(Stretch("Wood grain",square,at+Vector2.up*y,new Vector2(.82f,.02f),C("65533f"),depth+4));
   foreach(float x in new[]{-.4f,.4f})attach(Draw("Sign iron nail",disc,at+new Vector2(x,.75f),Vector2.one*.055f,C("d4d2c3"),depth+4));
   if(iceSampleMode){attach(Stretch("Snow cap",square,at+Vector2.up*.98f,new Vector2(1.04f,.08f),C("e1f2fa"),depth+5));attach(Stretch("Snow overhang",square,at+new Vector2(-.34f,.92f),new Vector2(.24f,.08f),C("c3dfe9"),depth+5));}
   Vector2 d=direction??Vector2.right,tangent=new Vector2(-d.y,d.x),center=at+Vector2.up*.75f;
   foreach(float sign in new[]{-1f,1f}){Vector2 tip=center+d*.19f,tail=center-d*.08f+tangent*sign*.11f,delta=tip-tail;var line=Stretch("Direction arrow",square,(tip+tail)*.5f,new Vector2(delta.magnitude,.035f),C("f6e9bf"),depth+6);line.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg);attach(line);}
  }
  private void WoodTrailSign(Vector2 at,int action,string label){// Use the island's existing rooted arch, rather than a floating arrow board.
   var arch=Draw("Forest return hollow",ForestQuestSprite(2),at,Vector2.one,Color.white,DepthAt(at.y));arch.transform.SetParent(chapterFocus.transform,true);
   AddChapterAction(action,at+Vector2.down*.4f,label);}
 }
}
