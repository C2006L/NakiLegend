using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype {
 public sealed partial class NakiPrototype {
  private sealed class CapitalGait {public Mesh mesh;public MeshRenderer renderer;public Sprite sprite;public Vector2 last;public float travel;public Vector3[] rest,posed;public Color[] colors;}
  private readonly Dictionary<Enemy,CapitalGait> capitalGaits=new Dictionary<Enemy,CapitalGait>();
  private Material capitalGaitMaterial;
  private void TickCapitalGait(Enemy e,bool moving){
   if(e.kind<EnemyKind.CapitalHalberd||e.kind>EnemyKind.CapitalMaintainer)return;
   CapitalGait rig;if(!capitalGaits.TryGetValue(e,out rig)){
    rig=CreateCapitalGait(e);capitalGaits[e]=rig;
   }
   float distance=Vector2.Distance(e.position,rig.last);rig.last=e.position;
   bool walking=moving&&distance>.00001f&&e.windup<=0&&e.chargeUntil<=clock&&e.hp>0;
   e.body.enabled=!walking;rig.renderer.enabled=walking;if(!walking)return;
   rig.travel+=distance;float stride=Mathf.Sin(rig.travel*9),height=rig.sprite.bounds.size.y;
   for(int i=0;i<rig.rest.Length;i++){
    Vector3 v=rig.rest[i];float above=(v.y-rig.sprite.bounds.min.y)/height,leg=1-Mathf.SmoothStep(.12f,.43f,above),side=v.x<0?-1:1;
    v.x+=side*stride*.045f*leg;v.y+=Mathf.Max(0,stride*side)*.12f*leg+Mathf.Abs(stride)*.018f*(1-leg);
    if(e.body.flipX)v.x=-v.x;if(e.body.flipY)v.y=-v.y;rig.posed[i]=v;rig.colors[i]=e.body.color;
   }
   rig.mesh.vertices=rig.posed;rig.mesh.colors=rig.colors;rig.mesh.RecalculateBounds();rig.renderer.sortingLayerID=e.body.sortingLayerID;rig.renderer.sortingOrder=e.body.sortingOrder;
  }
  private CapitalGait CreateCapitalGait(Enemy e){
   const int columns=12,rows=20;Sprite sprite=e.body.sprite;var rig=new CapitalGait{sprite=sprite,last=e.position,mesh=new Mesh{name="Capital footstep rig"}};
   rig.rest=new Vector3[(columns+1)*(rows+1)];rig.posed=new Vector3[rig.rest.Length];rig.colors=new Color[rig.rest.Length];var uv=new Vector2[rig.rest.Length];var triangles=new List<int>();Rect cell=sprite.rect;Bounds b=sprite.bounds;
   for(int y=0;y<=rows;y++)for(int x=0;x<=columns;x++){int i=y*(columns+1)+x;float u=x/(float)columns,v=y/(float)rows;rig.rest[i]=new Vector3(Mathf.Lerp(b.min.x,b.max.x,u),Mathf.Lerp(b.min.y,b.max.y,v));uv[i]=new Vector2((cell.x+cell.width*u)/sprite.texture.width,(cell.y+cell.height*v)/sprite.texture.height);rig.colors[i]=Color.white;if(x<columns&&y<rows){int next=i+columns+1;triangles.AddRange(new[]{i,next,i+1,i+1,next,next+1});}}
   rig.mesh.vertices=rig.rest;rig.mesh.uv=uv;rig.mesh.colors=rig.colors;rig.mesh.triangles=triangles.ToArray();rig.mesh.RecalculateBounds();ownedAssets.Add(rig.mesh);
   if(capitalGaitMaterial==null){capitalGaitMaterial=new Material(Shader.Find("Sprites/Default"));capitalGaitMaterial.mainTexture=sprite.texture;ownedAssets.Add(capitalGaitMaterial);}
   var root=new GameObject("Capital grounded alternating footstep mesh");root.transform.SetParent(e.root.transform,false);root.AddComponent<MeshFilter>().sharedMesh=rig.mesh;rig.renderer=root.AddComponent<MeshRenderer>();rig.renderer.sharedMaterial=capitalGaitMaterial;rig.renderer.enabled=false;return rig;
  }
  private void ClearCapitalGaits(){foreach(var pair in capitalGaits){if(pair.Value.renderer!=null)Destroy(pair.Value.renderer.gameObject);if(pair.Value.mesh!=null)Destroy(pair.Value.mesh);if(pair.Key.body!=null)pair.Key.body.enabled=true;}capitalGaits.Clear();}
 }
}
