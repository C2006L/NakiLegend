using System.Collections.Generic;
using UnityEngine;

namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private sealed class FragileCover {public int id,type;public Vector2 at;public Rect foot,shot;public float hp;public GameObject root;}
  private sealed class CoverBlast {public Vector2 at;public float when;public int type;public GameObject warning;}
  private readonly List<FragileCover> fragileCover=new List<FragileCover>();
  private readonly List<CoverBlast> coverBlasts=new List<CoverBlast>();
  private readonly Dictionary<int,Sprite> pressurePropArt=new Dictionary<int,Sprite>();
  private Sprite PressureProp(int id)
  {
   Sprite s;if(pressurePropArt.TryGetValue(id,out s)&&s!=null)return s;
   var t=Resources.Load<Texture2D>("NakiArt/PressureV1/props");if(t==null)return id==2?IceSprite(4):WorldPropSprite(6);t.filterMode=FilterMode.Point;
   Rect r=new Rect(id%3*t.width/3f,(1-id/3)*t.height/2f,t.width/3f,t.height/2f);
   s=Sprite.Create(t,r,new Vector2(.5f,.15f),r.width/1.8f,0,SpriteMeshType.FullRect);ownedAssets.Add(s);pressurePropArt[id]=s;return s;
  }
  private void BuildTacticalProps(RoomNode n)
  {
   if(capitalMode||manorCourtyardMode||gatehouseSampleMode||lightningTrialMode)return;
   if(n.kind==RoomKind.Reward){BuildOptionalChallengeAltar(n);}
   if(!IsCombatRoom(n)&&n.kind!=RoomKind.Reward)return;
   int count=CurrentIsland.ordinal==1?1:2;
   Vector2[] anchors={new Vector2(-2.6f,1.1f),new Vector2(2.8f,-1.8f),new Vector2(-5.5f,-1.5f),new Vector2(5.5f,1.5f)};
   int built=0;
   for(int i=0;i<anchors.Length&&built<count;i++){
    Vector2 at=anchors[(i+n.layout)%anchors.Length];if(IntersectsTransit(at,new Vector2(2,2))||Blocked(at,.9f)||ProjectilePathBlocked(at,at,.8f)||woodlandMode&&WoodlandCoverLaneReserved(at,new Vector2(1.1f,1.1f)))continue;
    int id=built++;if((n.coverBroken&(1<<id))!=0)continue;
    int type=iceSampleMode?2:woodlandMode?3:fireChapterMode?n.stage==2?4:id%2:CurrentIsland.ordinal==2?1:0;
    AddFragileCover(id,type,at);
   }
  }
  private void AddFragileCover(int id,int type,Vector2 at)
  {
   var c=new FragileCover{id=id,type=type,at=at,hp=type==3?60:44,foot=new Rect(at.x-.5f,at.y-.2f,1,.45f),shot=new Rect(at.x-.55f,at.y-.22f,1.1f,1.1f)};
   c.root=Draw(type==0?"Breakable timber crate":type==1?"Explosive oil barrel":type==2?"Brittle frost crystal":type==3?"Burnable thorn cover":"Fractured volcanic ore",PressureProp(type),at,Vector2.one,Color.white,DepthAt(at.y));
   fragileCover.Add(c);AddRoundedCover(c.foot,c.shot);
  }
  private void DamageFragileCover(FragileCover c,float damage,Element element)
  {
   if(!fragileCover.Contains(c))return;c.hp-=damage*(element==Element.Fire&&c.type==3?3:1);SpellSpark(c.at+Vector2.up*.3f,element==Element.Fire?C("ffc582"):C("e2d5b5"),.15f);
   if(c.hp>0)return;fragileCover.Remove(c);obstacles.Remove(c.foot);roundedCover.Remove(c.foot);projectileObstacles.Remove(c.shot);if(c.root!=null){c.root.SetActive(false);Destroy(c.root);}journeyRooms[currentRoom].coverBroken|=1<<c.id;
   if(c.type==1||c.type==2||c.type==4){float radius=c.type==2?1.55f:1.8f;coverBlasts.Add(new CoverBlast{at=c.at,type=c.type,when=clock+.7f,warning=WarningCircle(c.at,radius)});}
   else SpellSpark(c.at,C("c8b68e"),.5f);
   RequestCheckpoint();
  }
  private void HitBreakableProjectile(Shot s,Vector2 from,Vector2 to,float wall)
  {
   Vector2 hit=Vector2.Lerp(from,to,wall);foreach(FragileCover c in new List<FragileCover>(fragileCover)){
    Rect expanded=new Rect(c.shot.x-.16f,c.shot.y-.16f,c.shot.width+.32f,c.shot.height+.32f);if(!expanded.Contains(hit))continue;
    DamageFragileCover(c,s.hostile?12:Mathf.Max(8,s.damage),s.element);return;
   }
  }
  private void HitBreakableMelee()
  {
   foreach(FragileCover c in new List<FragileCover>(fragileCover)){
    Vector2 to=c.at-position;bool within=CurrentWeapon.type==WeaponType.Spear?DistanceToSegment(c.at,position,position+swingDirection*CurrentWeapon.range)<.7f:to.magnitude<CurrentWeapon.range+.45f&&Vector2.Dot(to.normalized,swingDirection)>.25f;
    if(!within)continue;
    // The cover itself is the endpoint blocker. Earlier walls must still stop a swing.
    Vector2 near=c.at-to.normalized*.8f;if(!ProjectilePathBlocked(position,near,.02f))DamageFragileCover(c,CurrentMeleeDamage,CurrentWeapon.element);
   }
  }
  private void TickBreakableHazards()
  {
   for(int i=coverBlasts.Count-1;i>=0;i--){CoverBlast blast=coverBlasts[i];if(clock<blast.when)continue;coverBlasts.RemoveAt(i);if(blast.warning!=null)Destroy(blast.warning);
    float radius=blast.type==2?1.55f:1.8f;Element element=blast.type==2?Element.Ice:Element.Fire;SpellSpark(blast.at,SpellTint(element),radius);SkillSound(blast.type==2?"ice_ring":"meteor","hit");
    foreach(Enemy e in new List<Enemy>(enemies))if(e.hp>0&&Vector2.Distance(e.position,blast.at)<radius+e.radius&&!ProjectilePathBlocked(blast.at,e.position,.05f)){ApplyElementDamage(e,blast.type==2?24:70+CurrentIsland.ordinal*8,element);if(element==Element.Ice)e.iceSlowUntil=Mathf.Max(e.iceSlowUntil,clock+(e.boss?.25f:1.2f));}
    if(Vector2.Distance(position,blast.at)<radius+.25f&&!ProjectilePathBlocked(blast.at,position,.04f))TakeJourneyDamage(blast.type==2?.7f:1.2f,element);
    foreach(FragileCover c in new List<FragileCover>(fragileCover))if(Vector2.Distance(c.at,blast.at)<radius&&!ProjectilePathBlocked(blast.at,c.at-(c.at-blast.at).normalized*.8f,.02f))DamageFragileCover(c,55,element);
   }
  }
  private void ClearBreakableState()
  {
   fragileCover.Clear();foreach(CoverBlast b in coverBlasts)if(b.warning!=null)Destroy(b.warning);coverBlasts.Clear();
  }
 }
}
