using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private sealed class FallingStar {public Vector2 at;public float impactAt,damage;public GameObject root,ring,star,tail;public SpriteRenderer[] smoke;public bool hit;}
  private readonly List<FallingStar> fallingStars=new List<FallingStar>();
  private const float StarfallRadius=1.2f,StarfallWarning=.9f,StarfallDescent=.32f;
  private Sprite starfallSprite,starfallSmokeSprite;
  private Sprite StarfallSprite
  {
   get {if(starfallSprite==null)starfallSprite=CreateStarfallTexture(false);return starfallSprite;}
  }
  private Sprite StarfallSmokeSprite {get{if(starfallSmokeSprite==null)starfallSmokeSprite=CreateStarfallTexture(true);return starfallSmokeSprite;}}
  private Sprite CreateStarfallTexture(bool smoke)
  {
   const int size=32;var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);texture.name=smoke?"Starfall dense smoke":"Starfall violet sphere";texture.filterMode=FilterMode.Point;texture.wrapMode=TextureWrapMode.Clamp;
   Color dark=C("1b092e"),purple=C("622596"),lit=C("be79ed");Vector3 light=new Vector3(-.45f,.65f,.7f).normalized;
   for(int y=0;y<size;y++)for(int x=0;x<size;x++){
    float nx=(x-15.5f)/14.5f,ny=(y-15.5f)/14.5f,r=Mathf.Sqrt(nx*nx+ny*ny);Color c=Color.clear;
    if(smoke){float angle=Mathf.Atan2(ny,nx),edge=.8f+.1f*Mathf.Sin(angle*5+.4f)+.06f*Mathf.Cos(angle*9);float alpha=Mathf.Clamp01((edge-r)*7);c=new Color(1,1,1,alpha);}
    else if(r<1){float z=Mathf.Sqrt(1-r*r),shade=Mathf.Clamp01(Vector3.Dot(new Vector3(nx,ny,z),light));c=shade<.5f?Color.Lerp(dark,purple,shade*2):Color.Lerp(purple,lit,(shade-.5f)*1.45f);float energy=Mathf.Exp(-((nx+.18f)*(nx+.18f)+(ny-.08f)*(ny-.08f))*9)*.22f;c=Color.Lerp(c,lit,energy);c.a=Mathf.Clamp01((1-r)*12);}
    texture.SetPixel(x,y,c);
   }
   texture.Apply();var sprite=Sprite.Create(texture,new Rect(0,0,size,size),Vector2.one*.5f,32,0,SpriteMeshType.FullRect);sprite.name=texture.name;ownedAssets.Add(texture);ownedAssets.Add(sprite);return sprite;
  }
  private void UpdateStarfallSmoke(FallingStar s,float left)
  {
   for(int j=0;j<s.smoke.Length;j++){
    bool burst=j>=16;int k=burst?j-16:j;float born=s.impactAt+(burst?k*.012f:-StarfallDescent+k*.018f),age=clock-born,life=burst?.38f:.42f;var r=s.smoke[j];
    r.gameObject.SetActive(age>=0&&age<life);if(age<0||age>=life)continue;
    float t=age/life,angle=k*2.39996f;Vector2 drift=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));Vector2 at;
    if(burst)at=s.at+drift*(.12f+age*1.55f)+Vector2.up*age*.45f;
    else {float fall=Mathf.Clamp01(k*.018f/StarfallDescent);at=s.at+Vector2.up*(4*(1-fall*fall)+age*.55f)+Vector2.right*(Mathf.Sin(k*2.7f)*(.06f+age*.4f));}
    r.transform.position=at;r.transform.localScale=new Vector3(1+.12f*Mathf.Sin(angle),1,1)*Mathf.Lerp(burst?.35f:.25f,burst?.95f:.9f,t);r.transform.rotation=Quaternion.Euler(0,0,k*47+t*22);
    Color tint=Color.Lerp(C("251331"),C("504353"),t);tint.a=(burst?.78f:.9f)*(1-t);r.color=tint;
   }
  }
  private void CastStarfall()
  {
   if(!OriginAbilityKnown(2)||clock<originThirdReady||clock<dashUntil)return;
   Enemy target=OriginTarget(null,10);if(target==null){JourneyHint("视线内没有星陨目标");return;}
   if(darkPower<21){JourneyHint("星陨需要 21 贤者之石能量");return;}
   var candidates=new List<Enemy>();foreach(Enemy e in enemies)if(e.hp>0&&Vector2.Distance(position,e.position)<=10&&!ProjectilePathBlocked(position,e.position,.05f))candidates.Add(e);
   candidates.Sort((a,b)=>(a.position-position).sqrMagnitude.CompareTo((b.position-position).sqrMagnitude));
   float damage=OriginDamage(70)*DarkStrength;if(!SpendPower(PowerPool.Dark,21))return;
   StopOriginAction();CancelBowCharge();RecordEncounterCast();originThirdReady=clock+4.5f;castDirection=OriginAim(target);castPoseUntil=clock+.3f;
   for(int i=0;i<3;i++){
    Vector2 at=i<candidates.Count?candidates[i].position:target.position+new Vector2(i==1?-.85f:.85f,.35f);
    at=NearestSafeFooting(at);
    GameObject root=new GameObject("Starfall fixed landing zone "+(i+1));root.transform.SetParent(world,false);
    GameObject fill=Draw("Starfall warning fill",softDisc??disc,at,Vector2.one*(StarfallRadius*2),new Color(.58f,.28f,.85f,.15f),-625);fill.transform.SetParent(root.transform,true);
    GameObject ring=Draw("Starfall landing rune",windRingSprite??disc,at,Vector2.one*(StarfallRadius*2),new Color(.63f,.35f,.84f,.75f),-623);ring.transform.SetParent(root.transform,true);
    GameObject star=Draw("Descending violet energy orb",StarfallSprite,at+Vector2.up*4,Vector2.one*.65f,Color.white,810);star.transform.SetParent(root.transform,true);
    GameObject aura=Draw("Violet orb subdued aura",softDisc??disc,star.transform.position,Vector2.one*.85f,new Color(.4f,.09f,.65f,.3f),809);aura.transform.SetParent(star.transform,true);star.SetActive(false);
    GameObject tail=new GameObject("Starfall dense smoke trail");tail.transform.SetParent(root.transform,false);var smoke=new SpriteRenderer[24];
    for(int j=0;j<smoke.Length;j++){GameObject puff=Draw(j<16?"Trailing dark smoke":"Ground impact smoke",StarfallSmokeSprite,at,Vector2.one,Color.clear,j<16?808:DepthAt(at.y)+8);puff.transform.SetParent(tail.transform,true);smoke[j]=puff.GetComponent<SpriteRenderer>();puff.SetActive(false);}
    fallingStars.Add(new FallingStar{at=at,impactAt=clock+StarfallWarning+StarfallDescent+i*.13f,damage=damage,root=root,ring=ring,star=star,tail=tail,smoke=smoke});
   }
   SkillSound("starfall","cast");
  }
  private void TickStarfall()
  {
   for(int i=fallingStars.Count-1;i>=0;i--){FallingStar s=fallingStars[i];if(s.root==null){fallingStars.RemoveAt(i);continue;}
    float left=s.impactAt-clock;
    if(!s.hit){
     float pulse=1+.035f*Mathf.Sin(clock*14);s.ring.transform.localScale=Vector3.one*StarfallRadius*2*pulse;
     if(left<=StarfallDescent){s.star.SetActive(true);float fall=1-Mathf.Clamp01(left/StarfallDescent);s.star.transform.position=s.at+Vector2.up*(4*(1-fall*fall));}
     if(left<=0){s.hit=true;s.star.SetActive(false);s.ring.GetComponent<SpriteRenderer>().color=C("9c4cce");
      foreach(Enemy e in new List<Enemy>(enemies))if(e.hp>0&&Vector2.Distance(s.at,e.position)<=StarfallRadius+e.radius&&!ProjectilePathBlocked(s.at,e.position,.04f)){ApplySkillHit(e,s.damage,Element.Dark,"starfall");TriggerSoulSeal(e);}
      SkillSound("starfall","hit");
     }
    }else {s.ring.transform.localScale=Vector3.one*StarfallRadius*2*(1-left*.7f);var r=s.ring.GetComponent<SpriteRenderer>();Color c=r.color;c.a=Mathf.Clamp01(1+left/.35f);r.color=c;}
    UpdateStarfallSmoke(s,left);
    if(left<-.5f){Destroy(s.root);fallingStars.RemoveAt(i);}
   }
  }
  private void ClearStarfall(){foreach(var s in fallingStars)if(s.root!=null)Destroy(s.root);fallingStars.Clear();}
 }
}
