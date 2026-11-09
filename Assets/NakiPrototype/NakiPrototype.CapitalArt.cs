using System;
using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private readonly Dictionary<string,Sprite> capitalArt=new Dictionary<string,Sprite>();
  private Sprite CapitalCell(string sheet,int index,float height,int columns=4,int rows=3,bool opaque=false)
  {
   string key=sheet+index+"/"+height;Sprite result;if(capitalArt.TryGetValue(key,out result)&&result!=null)return result;
   Texture2D t=Resources.Load<Texture2D>("NakiArt/CapitalV1/"+sheet);if(t==null)throw new InvalidOperationException("Missing capital art: "+sheet);
   t.filterMode=FilterMode.Point;int cw=t.width/columns,ch=t.height/rows,x=index%columns*cw,y=(rows-1-index/columns)*ch;
   int left=0,bottom=0,right=cw,top=ch;
   if(!opaque){Color[] pixels=t.GetPixels(x,y,cw,ch);left=cw;bottom=ch;right=top=0;
    for(int yy=0;yy<ch;yy++)for(int xx=0;xx<cw;xx++)if(pixels[yy*cw+xx].a>.5f){left=Math.Min(left,xx);bottom=Math.Min(bottom,yy);right=Math.Max(right,xx+1);top=Math.Max(top,yy+1);}
    if(right<=left||top<=bottom)throw new InvalidOperationException("Empty capital atlas cell "+key);
   }
   result=Sprite.Create(t,new Rect(x+left,y+bottom,right-left,top-bottom),opaque?Vector2.one*.5f:new Vector2(.5f,0),(top-bottom)/height,0,SpriteMeshType.FullRect);result.name="Capital / "+key;ownedAssets.Add(result);capitalArt[key]=result;return result;
  }
  private Sprite CapitalActor(int i){return CapitalCell("actors",i,i>=8&&i<=10?2.65f:1.48f);}
  private Sprite CapitalProp(int i){return CapitalCell("props",i,i==3?2.3f:i==10?2.5f:i==0?2.1f:i==11?2.2f:1.65f);}
  private void LoadCapitalArt()
  {
   for(int k=0;k<11;k++){var frames=new Sprite[k>=8?12:8];for(int j=0;j<frames.Length;j++)frames[j]=k>=8?CapitalAnimation(k-8,j):CapitalActor(k);enemyActionArt[(EnemyKind)((int)EnemyKind.CapitalHalberd+k)]=frames;}
   var node=new Sprite[8];for(int j=0;j<8;j++)node[j]=CapitalProp(7);enemyActionArt[EnemyKind.CapitalConduit]=node;
   for(int s=0;s<3;s++)chapterFloors[s]=CapitalCell("floors",s,3,3,1,true);thunderFloor=chapterFloors[0];thunderWall=chapterFloors[1];
  }
  private void CapitalCover(Vector2 at,int prop)
  {
   if(IntersectsTransit(at,new Vector2(1.8f,1.7f)))return;
   Draw("Capital cover · "+prop,CapitalProp(prop),at,Vector2.one,Color.white,DepthAt(at.y));
   float width=prop==5?1.5f:.85f;AddRoundedCover(new Rect(at.x-width*.5f,at.y+.04f,width,.3f),new Rect(at.x-width*.55f,at.y,width*1.1f,prop==0?1.8f:.85f));
  }
  private Sprite CapitalGateFrame(){Sprite result;if(capitalArt.TryGetValue("centered-gate",out result))return result;Sprite art=CapitalProp(2);result=Sprite.Create(art.texture,art.rect,Vector2.one*.5f,art.pixelsPerUnit,0,SpriteMeshType.FullRect);ownedAssets.Add(result);capitalArt["centered-gate"]=result;return result;}
  private void BuildCapitalDoorRoad(int direction,int stage){
   Vector2 normal=DoorNormal(direction),gate=DoorPoint(direction);bool vertical=direction<2;
   Vector2 size=vertical?new Vector2(2.8f,4.5f):new Vector2(4.5f,2.8f);DrawAlignedCapitalRoad("Capital connecting road / "+direction,new Rect(gate+normal*.25f-size*.5f,size),stage);
  }
  private void BuildCapitalRoom(RoomNode n)
  {
   ReserveTransitSpace(n);doorSeals.Clear();doorSealDirections.Clear();doorShotBlocks.Clear();thunderRain.Clear();thunderFlow.Clear();
   var floor=Draw("Capital region paving · "+AtlasRegions[6][n.stage],chapterFloors[n.stage],Vector2.zero,Vector2.one,Color.white,-1000).GetComponent<SpriteRenderer>();floor.drawMode=SpriteDrawMode.Tiled;floor.size=new Vector2(n.halfWidth*2,n.halfHeight*2);
   for(int d=0;d<4;d++){
    bool door=HasDoor(d);if(door)BuildCapitalDoorRoad(d,n.stage);float span=d<2?n.halfWidth:n.halfHeight;
    if(door){var open=Stretch("Capital bronze gate arch",CapitalGateFrame(),DoorPoint(d),new Vector2(3.3f,1.5f),Color.white,DepthAt(DoorPoint(d).y)+5);open.transform.rotation=Quaternion.Euler(0,0,d<2?0:90);
     var closed=Stretch("Capital bronze gate seal",square,DoorPoint(d),d<2?new Vector2(2.65f,.25f):new Vector2(.25f,2.65f),C("a88e5c"),DepthAt(DoorPoint(d).y)+6);doorSeals.Add(closed);doorSealDirections.Add(d);
    }
    for(float a=-span;a<span;a+=1.5f){float b=Mathf.Min(span,a+1.5f);if(door&&a<1.4f&&b> -1.4f){if(a< -1.4f)ThunderBoundary(d,a,-1.4f);if(b>1.4f)ThunderBoundary(d,1.4f,b);}else ThunderBoundary(d,a,b);}
   }
   if(IsCombatRoom(n)){
    CapitalCover(new Vector2(-5,1.5f),n.stage==0?5:0);CapitalCover(new Vector2(5,-2),n.stage==0?5:0);
    if(n.kind==RoomKind.Boss){CapitalCover(new Vector2(-5,-2.7f),0);CapitalCover(new Vector2(5,2.7f),0);}
   }
   if(n.id==21||n.id==12){foreach(float x in new[]{-6f,-2,2,6})Draw("Royal study bookshelf",CapitalProp(3),new Vector2(x,3.7f),Vector2.one,Color.white,DepthAt(3.7f));}
   if(n.id==14||n.id==15)foreach(float x in new[]{-5f,5})Draw("Closed royal tribute pipe",CapitalProp(6),new Vector2(x,3),Vector2.one,Color.white,DepthAt(3));
   if(n.kind==RoomKind.Rest)Draw("Capital medical alcove",CapitalProp(10),new Vector2(0,3.8f),Vector2.one,Color.white,DepthAt(3.8f));
   if(n.id==26)Draw("Royal sun throne",CapitalProp(8),new Vector2(0,4),Vector2.one,Color.white,DepthAt(4));
   DressCapitalExpansion(n);BuildChapterActions();
  }
 }
}
