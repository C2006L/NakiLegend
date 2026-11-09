using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  // Movement intent and the weapon's attack are independent: a guard can hold its post and still thrust.
  private enum IntentStrike { Slash,Thrust,Charge,Bolt,Arrow,Lob,Support }
  private sealed class IntentBomb {public Enemy owner;public Vector2 from,at;public float born,land;public GameObject ball,tell;}
  private readonly List<IntentBomb> intentBombs=new List<IntentBomb>();
  private Sprite enemyCrescent;
  private bool IntentSpearman(Enemy e){return !woodlandMode&&(e.kind==EnemyKind.SpearGuard||e.kind==EnemyKind.StormGuard||e.kind==EnemyKind.ForgeGuard);}
  private bool IntentKnife(Enemy e){return !woodlandMode&&e.kind==EnemyKind.AshRaider;}
  private bool IntentClaws(Enemy e){return !woodlandMode&&(e.kind==EnemyKind.IceGuard||e.kind==EnemyKind.SnowCrab||e.kind==EnemyKind.MoltenCrab);}
  private IntentStrike ChooseIntentStrike(Enemy e,IntentState s)
  {
   if(IsManorEnemy(e.kind))return ManorStrike(e);
   float d=Vector2.Distance(position,e.position);
   if(IntentSpearman(e))return d<3.4f?IntentStrike.Thrust:e.kind==EnemyKind.StormGuard&&d<5.2f&&s.attack%3!=0?IntentStrike.Charge:IntentStrike.Bolt;
   if(IntentKnife(e))return IntentStrike.Slash;
   if(IntentClaws(e))return d<2.3f?IntentStrike.Slash:IntentStrike.Charge;
   if(s.role==EnemyIntent.Support)return IntentStrike.Support;
   if(s.role==EnemyIntent.Intercept||s.role==EnemyIntent.Roam)return IntentStrike.Charge;
   if(!woodlandMode&&e.kind==EnemyKind.StoneSentinel&&d<2.4f)return IntentStrike.Slash;
   if(s.role==EnemyIntent.Artillery)return IntentStrike.Lob;
   if(!woodlandMode&&(e.kind==EnemyKind.IceArcher||e.kind==EnemyKind.SandKite))return IntentStrike.Arrow;
   return s.role==EnemyIntent.Pursue?IntentStrike.Slash:IntentStrike.Bolt;
  }
  private float IntentAttackRange(Enemy e,IntentState s)
  {return IsManorEnemy(e.kind)?ManorRange(e):IntentKnife(e)?2.15f:IntentClaws(e)?4.3f:IntentSpearman(e)?8:s.role==EnemyIntent.Pursue?2.1f:s.role==EnemyIntent.Intercept||s.role==EnemyIntent.Roam?4.1f:8;}
  private void BeginArmedIntent(Enemy e,IntentState s)
  {
   s.attack++;s.strike=ChooseIntentStrike(e,s);bool charge=s.strike==IntentStrike.Charge,melee=s.strike==IntentStrike.Slash||s.strike==IntentStrike.Thrust;
   Vector2 goal=IntentLead(charge?.4f:s.strike==IntentStrike.Lob?.45f:melee?0:s.attack%2==0?.22f:0);
   e.attackDirection=(goal-e.position).normalized;if(e.attackDirection.sqrMagnitude<.01f)e.attackDirection=Vector2.down;
   e.strikePoint=NearestEnemyFooting(goal,.35f);e.attackNumber++;e.actionStarted=clock;
   float tell=charge?.65f:s.strike==IntentStrike.Lob?.65f:s.strike==IntentStrike.Thrust?.52f:melee?.42f:.38f;
   s.casting=true;s.fireAt=clock+tell;e.windup=tell;e.nextAttack=s.fireAt+(s.strike==IntentStrike.Lob?2.4f:s.strike==IntentStrike.Support?2.8f:1.35f)*EncounterRecoveryScale;
   if(e.warning!=null)Destroy(e.warning);
   // Only committed movement / ground impacts retain area warnings. Ranged weapon shots have no trajectory line.
   e.warning=charge?ChapterLineWarning(e.position,e.attackDirection,4.3f,e.radius*2+.2f):null;
   if(s.strike==IntentStrike.Lob)e.warning=WarningCircle(e.strikePoint,1.05f);
   if(!charge&&s.strike!=IntentStrike.Lob){var glint=Draw(melee?"Weapon grip windup":"Enemy muzzle charge",disc,e.position+e.attackDirection*(e.radius+.15f)+Vector2.up*.35f,Vector2.one*(melee?.16f:.12f),melee?C("ffd8a4"):SpellTint(e.attackElement),DepthAt(e.position.y)+4);fadingBodies.Add(new FadingBody{root=glint,renderer=glint.GetComponent<SpriteRenderer>(),expires=s.fireAt});}
   ManorWindup(e,s);
  }
  private Sprite EnemyCrescent
  {
   get{if(enemyCrescent==null){var t=new Texture2D(48,48,TextureFormat.RGBA32,false);t.filterMode=FilterMode.Point;t.wrapMode=TextureWrapMode.Clamp;
    for(int y=0;y<48;y++)for(int x=0;x<48;x++){Vector2 v=new Vector2(x-23.5f,y-23.5f)/23;float angle=Mathf.Abs(Mathf.Atan2(v.y,v.x)),r=v.magnitude;float edge=.82f+.12f*Mathf.Cos(angle);t.SetPixel(x,y,angle<1.15f&&r<edge&&r>edge-.17f?Color.white:Color.clear);}t.Apply();enemyCrescent=Sprite.Create(t,new Rect(0,0,48,48),Vector2.one*.5f,48);ownedAssets.Add(t);ownedAssets.Add(enemyCrescent);}return enemyCrescent;}
  }
  private void ResolveArmedIntent(Enemy e,IntentState s)
  {
   e.actionStarted=clock;e.recoveryUntil=clock+.5f;
   if(ResolveManorIntent(e,s))return;
   if(s.strike==IntentStrike.Charge){s.charging=true;s.contact=false;s.chargeEnd=e.chargeUntil=clock+.58f;Sfx(IntentSpearman(e)?"spear":"dash",false,.3f);return;}
   if(s.strike==IntentStrike.Slash||s.strike==IntentStrike.Thrust){
    s.contact=false;s.meleeUntil=clock+.24f;s.meleeReach=e.kind==EnemyKind.ManorAxe?2.75f:s.strike==IntentStrike.Thrust?2.65f:IntentKnife(e)?1.55f:1.9f;
    var fx=Draw(s.strike==IntentStrike.Thrust?"Enemy spear physical thrust":"Enemy visible weapon slash",s.strike==IntentStrike.Thrust?arrowSprite:EnemyCrescent,e.position,Vector2.one,Color.Lerp(SpellTint(e.attackElement),Color.white,.55f),DepthAt(e.position.y)+7);
    s.weaponFx=fx;fadingBodies.Add(new FadingBody{root=fx,renderer=fx.GetComponent<SpriteRenderer>(),expires=s.meleeUntil});Sfx(s.strike==IntentStrike.Thrust?"spear":e.kind==EnemyKind.StoneSentinel?"hammer":"sword",false,.42f);TickIntentMelee(e,s,0);return;
   }
   if(s.strike==IntentStrike.Lob){var tell=WarningCircle(e.strikePoint,1.05f);var ball=Draw("Enemy thrown coal or stone",disc,e.position+Vector2.up*.45f,Vector2.one*.26f,Color.Lerp(SpellTint(e.attackElement),C("58402a"),.6f),DepthAt(e.position.y)+12);intentBombs.Add(new IntentBomb{owner=e,from=e.position+Vector2.up*.45f,at=e.strikePoint,born=clock,land=clock+.48f,ball=ball,tell=tell});Sfx("heavy",false,.28f);return;}
   if(s.strike==IntentStrike.Support){bool helped=false;foreach(Enemy ally in enemies)if(ally!=e&&!ally.boss&&ally.hp>0&&Vector2.Distance(e.position,ally.position)<4&&!ProjectilePathBlocked(e.position,ally.position,.05f)){ally.tacticWardUntil=clock+2;ally.hp=Mathf.Min(ally.maximum,ally.hp+ally.maximum*.025f);SpellSpark(ally.position,C("9ad8a6"),.35f);helped=true;}if(helped)return;}
   int first=shots.Count,count=e.kind==EnemyKind.ManorCrossbow||e.kind==EnemyKind.ManorArcher?3:IntentSpearman(e)?1:!woodlandMode&&e.kind==EnemyKind.EmberLizard?3:s.role==EnemyIntent.Anchor?3:2;
   PressureFan(e.position+e.attackDirection*(e.radius+.18f),e.attackDirection,count,count==1?0:count==3?(e.kind==EnemyKind.ManorCrossbow?5:18):8,IntentSpearman(e)?6:5.4f,e.attackElement,IntentSpearman(e)?1.2f:1.05f);
   Sfx(s.strike==IntentStrike.Arrow?"bow":IntentSpearman(e)?"spear":e.attackElement==Element.Lightning?"staff_electric":"staff",false,.28f);
   for(int i=first;i<shots.Count;i++){var shot=shots[i];var r=shot.root.GetComponent<SpriteRenderer>();r.color=SpellTint(e.attackElement);
    if(s.strike==IntentStrike.Arrow||IntentSpearman(e)){r.sprite=arrowSprite;shot.root.transform.localScale=new Vector3(IntentSpearman(e)?.95f:.6f,IntentSpearman(e)?.58f:.4f,1);shot.root.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(shot.direction.y,shot.direction.x)*Mathf.Rad2Deg);RemoveRoundBulletDecorations(shot);}
   }
  }
  private void TickIntentMelee(Enemy e,IntentState s,float dt)
  {
   e.position=MoveEnemyCharge(e,e.attackDirection*(s.strike==IntentStrike.Thrust?3:IntentKnife(e)?2.4f:.8f)*dt);
   if(s.weaponFx!=null){float t=Mathf.Clamp01(1-(s.meleeUntil-clock)/.24f),angle=Mathf.Atan2(e.attackDirection.y,e.attackDirection.x)*Mathf.Rad2Deg;
    s.weaponFx.transform.position=e.position+Vector2.up*.3f+e.attackDirection*(s.strike==IntentStrike.Thrust?s.meleeReach*.5f:0);
    s.weaponFx.transform.localScale=s.strike==IntentStrike.Thrust?new Vector3(s.meleeReach/arrowSprite.bounds.size.x,.2f/arrowSprite.bounds.size.y,1):Vector3.one*s.meleeReach*2;
    s.weaponFx.transform.rotation=Quaternion.Euler(0,0,angle+(s.strike==IntentStrike.Slash?Mathf.Lerp(-35,35,t):0));
   }
   Vector2 to=position-e.position;bool contact=s.strike==IntentStrike.Thrust?DistanceToSegment(position,e.position,e.position+e.attackDirection*s.meleeReach)<.42f:to.magnitude<s.meleeReach+.2f&&Vector2.Dot(to.normalized,e.attackDirection)>.35f;
   if(!s.contact&&contact&&!ProjectilePathBlocked(e.position,position,.04f)){s.contact=true;TakeJourneyDamage(s.strike==IntentStrike.Thrust?1.35f:1.15f,e.attackElement);}
  }
  private void TickIntentBombs()
  {
   for(int i=intentBombs.Count-1;i>=0;i--){var b=intentBombs[i];bool expired=b.owner.hp<=0||!enemies.Contains(b.owner);float t=Mathf.Clamp01((clock-b.born)/(b.land-b.born));
    if(!expired&&clock<b.land){b.ball.transform.position=Vector2.Lerp(b.from,b.at,t)+Vector2.up*Mathf.Sin(t*Mathf.PI)*1.3f;continue;}
    if(!expired){SpellSpark(b.at,SpellTint(b.owner.attackElement),1.15f);Sfx("stone",false,.4f);if(Vector2.Distance(position,b.at)<1.05f&&!ProjectilePathBlocked(b.at,position,.03f))TakeJourneyDamage(1.25f,b.owner.attackElement);}
    Destroy(b.ball);Destroy(b.tell);intentBombs.RemoveAt(i);
   }
  }
  private void ClearIntentBombs(){foreach(var b in intentBombs){if(b.ball!=null)Destroy(b.ball);if(b.tell!=null)Destroy(b.tell);}intentBombs.Clear();}
  private string ArmedIntentName(Enemy e)
  {IntentState s=IntentFor(e);switch(s.strike){case IntentStrike.Thrust:return "蓄势 · 枪尖穿刺";case IntentStrike.Slash:return "近身挥斩";case IntentStrike.Charge:return IntentSpearman(e)?"持枪突进":"锁向冲撞";case IntentStrike.Arrow:return "弓矢连射";case IntentStrike.Lob:return "抛投 · 留意落点";case IntentStrike.Support:return "护阵供养";default:return IntentSpearman(e)?"枪芒直射":"元素射击";}}
 }
}
