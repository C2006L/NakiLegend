using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype {
 public sealed partial class NakiPrototype {
  // Damage is expressed before player defenses. It is divided by the legacy island multiplier once at emission.
  private static readonly float[] BossSlowDamage={.95f,1.15f,1.35f,1.5f,1.65f,1.8f,2f};
  private static readonly float[] BossFastDamage={1.45f,1.7f,1.95f,2.15f,2.35f,2.55f,2.8f};
  private static readonly float[] BossHeavyDamage={2f,2.25f,2.5f,2.75f,2.95f,3.15f,3.4f};
  private static readonly float[] MinionSlowDamage={.75f,.9f,1f,1.1f,1.2f,1.35f,1.5f};
  private static readonly float[] MinionFastDamage={1.1f,1.25f,1.4f,1.55f,1.7f,1.85f,2f};
  private readonly Dictionary<Element,Sprite> hostileOrbs=new Dictionary<Element,Sprite>();
  private int EncounterChapterTier {get{return Mathf.Clamp(CurrentIsland.ordinal-1,0,6);}}
  private bool ApprovedCaptainEncounter {get{return manorCourtyardMode&&enemies.Exists(e=>e.kind==EnemyKind.ManorCaptain&&e.hp>0);}}
  private float ProfileShotDamage(Shot s,bool fast){
   int tier=EncounterChapterTier;Enemy source=s.emitter;
   if(source!=null&&CombatFixture(source)&&source.iceOwner!=null)source=source.iceOwner;
   bool boss=source!=null&&source.boss;
   if(ApprovedCaptainEncounter&&!boss)return s.heavyParent?1.5f:fast?1.25f:.85f;
   // The mother encounter already combines body, offspring and arena pressure. Keep its projectile budget independent.
   if(woodlandMode&&source!=null&&source.kind==WoodKinds[9])return s.heavyParent?2f:fast?1.65f:1.05f;
   return boss?(s.heavyParent?BossHeavyDamage[tier]:fast?BossFastDamage[tier]:BossSlowDamage[tier]):s.heavyParent?MinionFastDamage[tier]+.4f:fast?MinionFastDamage[tier]:MinionSlowDamage[tier];
  }
  private float ProfileBeamDamage(Enemy source){
   if(source!=null&&source.kind==EnemyKind.ManorCaptain)return 1.8f*EnemyAttackScale;
   return Mathf.Lerp(1.6f,3f,EncounterChapterTier/6f);
  }
  private Sprite ElementHostileOrb(Element element){
   Sprite sprite;if(hostileOrbs.TryGetValue(element,out sprite))return sprite;
   const int size=24;var t=new Texture2D(size,size,TextureFormat.RGBA32,false);t.filterMode=FilterMode.Point;t.wrapMode=TextureWrapMode.Clamp;
   Color rim=element==Element.Physical?C("f26d78"):SpellTint(element),edge=Color.Lerp(rim,C("1e172b"),.78f);
   for(int y=0;y<size;y++)for(int x=0;x<size;x++){
    Vector2 p=new Vector2((x+.5f)/size,(y+.5f)/size);float radius=(p-Vector2.one*.5f).magnitude*2;
    Color c=Color.clear;if(radius<.94f)c=edge;if(radius<.79f)c=rim;if(radius<.58f)c=Color.Lerp(rim,C("292034"),.28f);
    if(radius<.79f&&Vector2.Distance(p,new Vector2(.34f,.7f))<.11f)c=C("fff7eb");t.SetPixel(x,y,c);
   }
   t.Apply();sprite=Sprite.Create(t,new Rect(0,0,size,size),Vector2.one*.5f,size,0,SpriteMeshType.FullRect);ownedAssets.Add(t);ownedAssets.Add(sprite);hostileOrbs[element]=sprite;return sprite;
  }
  private void ArmBriefHoming(Shot s,float seconds){if(!s.hostile||s.heavyParent)return;s.hostileHomingUntil=clock+Mathf.Min(.7f,seconds);s.hostileTurnRate=1.15f;}
  private void OwnedPressureFan(Enemy owner,Vector2 at,Vector2 aim,int count,float spacing,float speed,Element element,float damage){
   Enemy previous=attackEmitter;bool previousBoss=bossAttackEmission;attackEmitter=owner;bossAttackEmission=owner!=null&&owner.boss;
   try{PressureFan(at,aim,count,spacing,speed,element,damage);}finally{attackEmitter=previous;bossAttackEmission=previousBoss;}
  }
  private void TickBriefHoming(Shot s,float dt){
   if(!s.hostile||clock>=s.hostileHomingUntil)return;
   s.direction=((Vector2)Vector3.RotateTowards(s.direction,(position-s.position).normalized,dt*s.hostileTurnRate,0)).normalized;
   if(s.hostileFast)s.root.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(s.direction.y,s.direction.x)*Mathf.Rad2Deg);
  }
 }
}
