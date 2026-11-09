using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype {
 public sealed partial class NakiPrototype {
  private sealed class AncientRootAttack {public Enemy owner;public Vector2 at,aim;public float born,length;public bool hit;public GameObject root;public SpriteRenderer[] spikes;}
  private readonly List<AncientRootAttack> ancientRootAttacks=new List<AncientRootAttack>();
  private Sprite ancientRootSpike;
  private Sprite AncientRootSpike {get{
   if(ancientRootSpike!=null)return ancientRootSpike;
   const int width=64,height=96;var tex=new Texture2D(width,height,TextureFormat.RGBA32,false);tex.filterMode=FilterMode.Point;tex.wrapMode=TextureWrapMode.Clamp;
   for(int y=0;y<height;y++)for(int x=0;x<width;x++){
    float t=y/(float)(height-1),u=x/(float)(width-1),center=.39f+.11f*Mathf.Sin(t*3.3f)+t*.21f,radius=.21f*Mathf.Pow(1-t,.85f)+.008f;
    float main=Mathf.Abs(u-center)/radius;
    float branchT=Mathf.Clamp01((t-.22f)/.44f),branchCenter=center-branchT*.32f,branchRadius=.085f*(1-branchT)+.004f;
    float branch=t>.22f&&t<.66f?Mathf.Abs(u-branchCenter)/branchRadius:100;
    float edge=Mathf.Min(main,branch);Color color=Color.clear;
    if(edge<1){
     float grain=Mathf.Sin(x*.71f+y*.16f)*.06f+Mathf.Sin(x*1.77f-y*.11f)*.035f;
     float light=Mathf.Clamp01(.72f-(u-center)/radius*.32f+grain);
     color=Color.Lerp(C("443129"),C("ad9366"),light);
     if(Mathf.Sin(x*.74f+Mathf.Sin(t*8)*2)> .82f)color=Color.Lerp(color,C("4e3b2b"),.45f);
     if(t>.68f)color=Color.Lerp(color,C("aac47b"),(t-.68f)*1.8f);
     if(edge>.87f)color=Color.Lerp(color,C("292a22"),(edge-.87f)/.13f);
    }
    tex.SetPixel(x,y,color);
   }
   tex.Apply();ancientRootSpike=Sprite.Create(tex,new Rect(0,0,width,height),new Vector2(.5f,.03f),64,0,SpriteMeshType.FullRect);ownedAssets.Add(tex);ownedAssets.Add(ancientRootSpike);return ancientRootSpike;
  }}
  private void BeginAncientRootAttack(ForestStrike strike){
   Vector2 end=CoverBeamEnd(strike.at,strike.aim,strike.length,.06f);float length=Mathf.Max(0,Vector2.Dot(end-strike.at,strike.aim));if(length<.35f)return;
   var attack=new AncientRootAttack{owner=strike.owner,at=strike.at,aim=strike.aim,born=clock,length=length,root=new GameObject("Ancient roots erupt along committed lane")};attack.root.transform.SetParent(world,false);
   int count=Mathf.Max(1,Mathf.CeilToInt(length/.9f));attack.spikes=new SpriteRenderer[count];
   for(int i=0;i<count;i++){
    float distance=Mathf.Min(length-.12f,.25f+i*.9f);Vector2 point=strike.at+strike.aim*distance;
    var crack=Stretch("Root earth fissure",square,point,new Vector2(.85f,.18f),C("352d27"),DepthAt(point.y)-2);crack.transform.SetParent(attack.root.transform,true);crack.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(strike.aim.y,strike.aim.x)*Mathf.Rad2Deg);
    var spike=Draw("Rising ancient thorn root",AncientRootSpike,point,Vector2.zero,Color.white,DepthAt(point.y)+12);spike.transform.SetParent(attack.root.transform,true);spike.GetComponent<SpriteRenderer>().flipX=(i&1)==0;attack.spikes[i]=spike.GetComponent<SpriteRenderer>();
   }
   ancientRootAttacks.Add(attack);SkillSound("root","hit");TickAncientRootAttacks();
  }
  private void TickAncientRootAttacks(){
   for(int n=ancientRootAttacks.Count-1;n>=0;n--){var attack=ancientRootAttacks[n];float age=clock-attack.born;
    if(age>=1||attack.owner.hp<=0||!enemies.Contains(attack.owner)){if(attack.root!=null)Destroy(attack.root);ancientRootAttacks.RemoveAt(n);continue;}
    for(int i=0;i<attack.spikes.Length;i++){
     float delay=i*.045f,rise=Mathf.Clamp01((age-delay)/.14f),fade=Mathf.Clamp01((1-age)/.22f);var spike=attack.spikes[i];spike.transform.localScale=new Vector3(.85f,Mathf.Lerp(.03f,.6f,rise),1);Color color=Color.white;color.a=rise*fade;spike.color=color;
    }
    float reach=Mathf.Clamp01((age-.10f)/.32f)*attack.length;
    if(!attack.hit&&reach>0&&DistanceToSegment(position,attack.at,attack.at+attack.aim*reach)<.55f&&!ProjectilePathBlocked(attack.at,position,.04f)){attack.hit=true;TakeJourneyDamage(1.8f,Element.Wood);}
   }
  }
  private void ClearAncientRootAttacks(Enemy owner=null){for(int i=ancientRootAttacks.Count-1;i>=0;i--)if(owner==null||ancientRootAttacks[i].owner==owner){if(ancientRootAttacks[i].root!=null)Destroy(ancientRootAttacks[i].root);ancientRootAttacks.RemoveAt(i);}}
 }
}
