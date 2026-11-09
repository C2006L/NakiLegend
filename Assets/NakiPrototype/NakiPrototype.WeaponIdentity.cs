using System;
using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  // These fields are assigned by JsonUtility.
#pragma warning disable 0649
  [Serializable] private sealed class WeaponIdentityManifest { public WeaponIdentity[] entries; }
  [Serializable] private sealed class WeaponIdentity
  {
   public int id,x,y,width,height,accent;public float pivotX,pivotY;
   public string displayName,sheet,form,cast,hit,pixelHash;
  }
#pragma warning restore 0649
  private WeaponIdentity[] weaponIdentities;
  private readonly Dictionary<int,Sprite> identityArtwork=new Dictionary<int,Sprite>();
  private readonly Dictionary<int,Sprite> identityBolts=new Dictionary<int,Sprite>();
  private void ApplyWeaponIdentities()
  {
   if(weaponIdentities==null){var asset=Resources.Load<TextAsset>("NakiArt/WeaponIdentityV3/manifest");if(asset==null)throw new InvalidOperationException("Missing weapon identity manifest");weaponIdentities=JsonUtility.FromJson<WeaponIdentityManifest>(asset.text).entries;}
   ExtendManorWeaponIdentity();
   if(weaponIdentities.Length!=weaponCatalog.Length)throw new InvalidOperationException("Weapon identity catalog mismatch");
   for(int i=0;i<weaponCatalog.Length;i++){
    var entry=weaponIdentities[i];if(entry.id!=i)throw new InvalidOperationException("Weapon identity ID mismatch");Sprite sprite;
    if(!identityArtwork.TryGetValue(i,out sprite)||sprite==null){var texture=Resources.Load<Texture2D>(entry.sheet);if(texture==null)throw new InvalidOperationException("Missing weapon artwork: "+entry.sheet);texture.filterMode=FilterMode.Point;
     sprite=Sprite.Create(texture,new Rect(entry.x,entry.y,entry.width,entry.height),new Vector2(entry.pivotX,entry.pivotY),Mathf.Max(entry.width,entry.height),0,SpriteMeshType.FullRect);sprite.name="Weapon identity "+i+" "+entry.displayName;ownedAssets.Add(sprite);identityArtwork[i]=sprite;}
    WeaponDef w=weaponCatalog[i];w.name=entry.displayName;w.sprite=sprite;w.color=Color.white;w.bowFacesLeft=false;
   }
  }
  private int ArrayWeaponId(WeaponDef weapon){return Array.IndexOf(weaponCatalog,weapon);}
  private int HeldWeaponIdentity {get{return carriedWeapons[equippedSlot];}}
  private Color IdentityTint(int id)
  {
   Color main=SpellTint(weaponCatalog[id].element);
   return Color.Lerp(main,Color.white,.035f*(id%6));
  }
  private bool PlayWeaponIdentitySound(int id,bool contact)
  {
   if(weaponIdentities==null||id<0||id>=weaponIdentities.Length)return false;
   if(!enableDesertJourney||AudioSuspended||AudioNarrative||openingActive)return true;SetupSoundscape();
   string key=contact?weaponIdentities[id].hit:weaponIdentities[id].cast;
   return PlaySoundCue(key,contact?1:0,contact?.65f:.78f,false);
  }
  // A distinct cached pixel glyph per weapon, including its family crest and variant feathering.
  // This is a code-native projectile effect, independent of the generated held-weapon artwork.
  private Sprite IdentityBolt(int id)
  {
   Sprite sprite;if(identityBolts.TryGetValue(id,out sprite)&&sprite!=null)return sprite;
   var e=weaponIdentities[id];int n=33;var rows=new string[n];bool arrow=e.form=="arrow"||e.form=="bolt",needle=e.form=="needle";
   for(int y=0;y<n;y++){char[] row=new char[n];for(int x=0;x<n;x++){
    float dx=x-16,dy=y-16,r=Mathf.Sqrt(dx*dx+dy*dy);bool rim=false,core=false;
    if(arrow||needle){rim=Mathf.Abs(dy)<(needle?1.4f:1)&&x>2&&x<29||x>21&&x<30&&Mathf.Abs(dy)<(30-x)*.53f||arrow&&x<10&&x>3&&Mathf.Abs(Mathf.Abs(dy)-(9-x)*.65f)<1.2f;core=Mathf.Abs(dy)<.6f&&x>10&&x<28;}
    else if(e.form=="lantern"||e.form=="mirror"||e.form=="spring"){rim=r>10&&r<12||Mathf.Abs(dx)<1.3f&&Mathf.Abs(dy)<6||Mathf.Abs(dy)<1.3f&&Mathf.Abs(dx)<6;core=r<2.4f;}
    else if(e.form=="crescent"){rim=r<13&&r>9&&dx>-(4+dy*.2f);core=r>10&&r<11.5f&&dx>3;}
    else if(e.form=="sunray"){rim=Mathf.Abs(dy)<1.7f&&x>1&&x<31||Mathf.Abs(dx-5)+Mathf.Abs(dy)<6;core=Mathf.Abs(dy)<.7f&&x>5&&x<29;}
    else if(e.form=="soul"){rim=r>7&&r<10||Mathf.Abs(dx)+Mathf.Abs(dy)<7;core=r<3;}
    else if(e.form=="crystal"){rim=Mathf.Abs(dx)/13+Mathf.Abs(dy)/7<1;core=Mathf.Abs(dx)/10+Mathf.Abs(dy)/3<1;}
    else if(e.form=="flame"){rim=x>3&&x<29&&Mathf.Abs(dy)<(30-x)*.4f+Mathf.Sin(x*.8f+id)*1.7f;core=x>16&&x<27&&Mathf.Abs(dy)<(29-x)*.19f;}
    else if(e.form=="seed"||e.form=="spore"){rim=r<8||(Mathf.Abs(dx+dy)<1.5f&&r<13);core=r<3;}
    else if(e.form=="stone"){rim=Mathf.Abs(dx)<9&&Mathf.Abs(dy)<9&&Mathf.Abs(dx+dy)<14;core=r<4;}
    else{rim=Mathf.Abs(dx)+Mathf.Abs(dy)<12;core=r<3;}
    // The engraved crest changes per ID and is visible on larger bolt previews.
    if(r>12&&r<15&&((x*7+y*11+id*13)%23)<2+e.accent%3)rim=true;
    row[x]=core?'w':rim?'c':'.';
   }rows[y]=new string(row);}
   sprite=MakeSprite(rows,new Dictionary<char,Color>{{'c',IdentityTint(id)},{'w',C("fff9e8")}},33);sprite.name="Identity bolt "+id+" "+e.form;identityBolts[id]=sprite;return sprite;
  }
  private void StyleIdentityProjectile(Shot shot,int id)
  {
   if(shot.hostile||weaponIdentities==null||id<0||id>=weaponCatalog.Length)return;shot.identityWeapon=id;
   var entry=weaponIdentities[id];var sr=shot.root.GetComponent<SpriteRenderer>();sr.sprite=IdentityBolt(id);sr.color=Color.white;
   bool bow=weaponCatalog[id].type==WeaponType.Bow,needle=entry.form=="needle";float size=bow?.48f:needle?.40f:.23f;
   if(weaponCatalog[id].variant==2)size*=1.18f;if(entry.form=="lantern"||entry.form=="mirror")size*=1.2f;
   shot.root.transform.localScale=new Vector3(size,bow||needle?size*.64f:size,1);shot.root.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(shot.direction.y,shot.direction.x)*Mathf.Rad2Deg);
   shot.root.name="Weapon "+id+" / "+entry.form;shot.skillAudio=null; // Weapon contact has its own voice, not the skill's audio.
   if(id==123){shot.hitsLeft=2;shot.root.transform.localScale=new Vector3(.55f,.15f,1);}
   if(id==127&&bowShotCharge>=.9f)shot.hitsLeft=3;
   if(id==128&&bowShotCharge>=.9f)shot.hitsLeft=2;
   if(id==129){Enemy target;AutoAim(10,6,0,out target);shot.target=target;shot.tracking=target!=null;shot.speed=6;}
   var trail=shot.root.GetComponent<TrailRenderer>();if(trail==null){AddShotRibbon(shot,IdentityTint(id),.022f);trail=shot.root.GetComponent<TrailRenderer>();}
   trail.startColor=IdentityTint(id);trail.endColor=new Color(trail.startColor.r,trail.startColor.g,trail.startColor.b,0);trail.time=.07f+.008f*entry.accent;trail.startWidth=(bow?.016f:.027f)+entry.accent*.0015f;trail.endWidth=0;
  }
  private void IdentityImpact(int id,Vector2 at,Vector2 aim)
  {
   if(weaponIdentities==null||id<0||id>=weaponIdentities.Length)return;PlayWeaponIdentitySound(id,true);
   if(fadingBodies.Count>140)return;var e=weaponIdentities[id];Color tint=IdentityTint(id);float rotation=Mathf.Atan2(aim.y,aim.x)*Mathf.Rad2Deg;
   int count=3+e.accent;float length=.1f+.018f*(id%5);
   for(int i=0;i<count;i++){Vector2 ray=Quaternion.Euler(0,0,rotation+i*360f/count+e.accent*7)*Vector2.right;ImpactStroke(at+ray*length*.6f,ray,length,.018f+(i%2)*.008f,tint);}
   if(e.form=="lantern"||e.form=="mirror"||e.form=="spring"||e.form=="hammer"){
    var ring=Draw("Weapon impact seal "+id,windRingSprite??disc,at,Vector2.one*(.28f+.025f*e.accent),tint,709);fadingBodies.Add(new FadingBody{root=ring,renderer=ring.GetComponent<SpriteRenderer>(),expires=clock+.16f});}
  }
  private void IdentityMeleeEffect()
  {
   int id=HeldWeaponIdentity;if(weaponIdentities==null||fadingBodies.Count>140)return;var e=weaponIdentities[id];Color tint=IdentityTint(id);Vector2 aim=swingDirection.normalized;
   bool thrust=CurrentWeapon.type==WeaponType.Spear;float angle=Mathf.Atan2(aim.y,aim.x)*Mathf.Rad2Deg;
   if(thrust){Vector2 at=position+aim*CurrentWeapon.range*.6f;ImpactStroke(at,aim,CurrentWeapon.range*.8f,.04f+.004f*e.accent,tint);for(int j=0;j<2;j++){Vector2 side=new Vector2(-aim.y,aim.x)*(j==0?1:-1)*.075f;ImpactStroke(at+side,aim,.45f+.03f*e.accent,.017f,tint);}return;}
   bool hammer=e.form=="hammer";int steps=hammer?6:7+e.accent;float radius=CurrentWeapon.range*(hammer?.65f:.8f);
   for(int i=0;i<steps;i++){float a=hammer?i*360f/steps:Mathf.Lerp(-64,64,i/(float)(steps-1))+angle;Vector2 ray=Quaternion.Euler(0,0,a)*Vector2.right;
    Vector2 at=hammer?position+aim*.9f+ray*.24f:position+ray*radius;ImpactStroke(at,hammer?ray:new Vector2(-ray.y,ray.x),hammer?.16f:.18f,.025f+.004f*e.accent,tint);}
  }
 }
}
