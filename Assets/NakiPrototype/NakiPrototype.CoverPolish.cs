using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private readonly Dictionary<Rect,float> roundedCover=new Dictionary<Rect,float>();
  private void AddRoundedCover(Rect foot,Rect body)
  {obstacles.Add(foot);roundedCover[foot]=Mathf.Min(foot.width,foot.height)*.45f;projectileObstacles.Add(body);}
  private bool CoverBlocks(Rect rect,Vector2 at,float radius)
  {
   float corner;if(!roundedCover.TryGetValue(rect,out corner))corner=0;
   Vector2 closest=new Vector2(Mathf.Clamp(at.x,rect.xMin+corner,rect.xMax-corner),Mathf.Clamp(at.y,rect.yMin+corner,rect.yMax-corner));
   return (at-closest).sqrMagnitude<(radius+corner)*(radius+corner);
  }
  // A single segment serves beam drawing and damage. Never translate the picture after clipping.
  private Vector2 CoverBeamEnd(Vector2 start,Vector2 dir,float length,float radius)
  {
   dir.Normalize();Vector2 end=start+dir*length;
   float wall=ProjectileWallContact(start,end,radius);
   if(!float.IsPositiveInfinity(wall))length=Mathf.Min(length,Mathf.Max(0,wall*length-.015f));
   Rect room=CurrentRoomBounds;
   if(dir.x>.0001f)length=Mathf.Min(length,(room.xMax-start.x)/dir.x);
   else if(dir.x<-.0001f)length=Mathf.Min(length,(room.xMin-start.x)/dir.x);
   if(dir.y>.0001f)length=Mathf.Min(length,(room.yMax-start.y)/dir.y);
   else if(dir.y<-.0001f)length=Mathf.Min(length,(room.yMin-start.y)/dir.y);
   return start+dir*Mathf.Max(0,length);
  }
  private void ClipSignatureBeam(SignatureZone z,Vector2 dir,float width)
  {
   Vector2 end=CoverBeamEnd(z.at,dir,z.radius,width*.5f);
   // The muzzle may be beyond a nearby pillar. It must not teleport through that pillar.
   if(ProjectilePathBlocked(z.owner.position,z.at,width*.5f))end=z.at;
   z.left.transform.position=(z.at+end)*.5f;
   z.left.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(dir.y,dir.x)*Mathf.Rad2Deg);
   z.left.transform.localScale=new Vector3(Vector2.Distance(z.at,end),width,1);
  }
  // Read the whole row, then retain only the connected actor. A sword can extend
  // outside its nominal cell, while a neighbour's sword can never enter this pose.
  private Sprite ManorFinalePose(Texture2D source,int row,int pose)
  {
   int w=source.width,h=source.height/2,yBase=(1-row)*h;
   Color32[] pixels=source.GetPixels32();int[] labels=new int[w*h],queue=new int[w*h];int label=0,best=0,bestScore=0;
   for(int start=0;start<labels.Length;start++){
    if(labels[start]!=0||pixels[(start/w+yBase)*w+start%w].a<16)continue;
    int head=0,tail=0,score=0;label++;labels[start]=label;queue[tail++]=start;
    while(head<tail){int at=queue[head++],x=at%w,y=at/w;
     if(x>(pose+.25f)*w/4&&x<(pose+.75f)*w/4&&y>h*.2f&&y<h*.85f)score++;
     for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){
      int xx=x+dx,yy=y+dy;if(xx<0||xx>=w||yy<0||yy>=h)continue;int next=yy*w+xx;
      if(labels[next]==0&&pixels[(yy+yBase)*w+xx].a>=16){labels[next]=label;queue[tail++]=next;}
     }
    }
    if(score>bestScore){bestScore=score;best=label;}
   }
   if(best==0)throw new System.InvalidOperationException("Missing finale pose silhouette");
   int x0=w,y0=h,x1=0,y1=0;
   for(int i=0;i<labels.Length;i++)if(labels[i]==best){x0=Mathf.Min(x0,i%w);y0=Mathf.Min(y0,i/w);x1=Mathf.Max(x1,i%w);y1=Mathf.Max(y1,i/w);}
   int width=x1-x0+5,height=y1-y0+5;var output=new Color32[width*height];
   for(int i=0;i<labels.Length;i++)if(labels[i]==best)output[(i/w-y0+2)*width+i%w-x0+2]=pixels[(i/w+yBase)*w+i%w];
   var texture=new Texture2D(width,height,TextureFormat.RGBA32,false);texture.name="Isolated finale pose "+row+" / "+pose;texture.filterMode=FilterMode.Point;texture.wrapMode=TextureWrapMode.Clamp;texture.SetPixels32(output);texture.Apply();ownedAssets.Add(texture);
   Vector2 pivot=new Vector2(((pose+.5f)*w/4-x0+2)/width,(h*.09f-y0+2)/height);
   var sprite=Sprite.Create(texture,new Rect(0,0,width,height),pivot,h/3.1f,0,SpriteMeshType.FullRect);sprite.name=row==0?"Royal seal automaton":"Dark great knight";ownedAssets.Add(sprite);return sprite;
  }
 }
}
