using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private sealed class WoodZone {public Vector2 at;public float born,active,until,nextHit;public GameObject root;}
  private readonly List<WoodZone> woodZones=new List<WoodZone>();
  private sealed class WoodVolley {public Enemy owner;public Vector2 aim;public int count;public float when;public GameObject warning;}
  private readonly List<WoodVolley> woodVolleys=new List<WoodVolley>();
  private void ConfigureWoodCombat(Enemy e)
  {
   if(e.kind==EnemyKind.WoodParasite){e.hp=e.maximum=150;e.boss=false;e.radius=.35f;e.speed=0;e.soulEligible=false;e.summoned=true;e.affinity=e.attackElement=Element.Wood;e.nextAttack=clock+2.5f;e.root.transform.localScale=Vector3.one*.65f;return;}
   int k=ArrayIndexWood(e.kind);if(k<0)return;
   float[] hp={100,168,132,144,110,190,136,156,108,1250,1750,2100,190};int stage=journeyRooms[currentRoom].stage;
   e.elite=false;e.eliteKind=0;e.boss=k>=9&&k<=11;e.hp=e.maximum=hp[k]+(e.boss?0:stage*12);e.radius=e.boss?1.05f:k==1||k==5||k==12?.45f:.33f;
   e.affinity=k==0||k==6||k==7?Element.Physical:Element.Wood;e.attackElement=Element.Wood;e.speed=e.boss?(k==9?1.15f:k==11?1.1f:.95f):k==2?.5f:k==5?.95f:1.4f;e.nextAttack=clock+1.6f;
  }
  private void SpawnWoodKnots(Enemy boss)
  {
   foreach(Vector2 offset in new[]{new Vector2(-2.4f,.8f),new Vector2(2.4f,.8f),new Vector2(0,2.2f)}){
    var knot=SpawnJourneyEnemy(EnemyKind.WoodParasite,NearestEnemyFooting(boss.position+offset,.4f),0);knot.woodOwner=boss;knot.soulEligible=false;knot.summoned=true;
   }
  }
  private int WoodKnotsAlive(Enemy boss){int n=0;foreach(Enemy e in enemies)if(e.kind==EnemyKind.WoodParasite&&e.woodOwner==boss&&e.hp>0)n++;return n;}
  private void WoodKnotBroken(Enemy knot)
  {
   Enemy boss=knot.woodOwner;if(boss==null||boss.hp<=0||!enemies.Contains(boss))return;
   CancelForestStrikes(boss);boss.windup=0;boss.chargeUntil=0;if(boss.warning!=null)Destroy(boss.warning);boss.recoveryUntil=clock+1.6f;boss.nextAttack=clock+1.8f;
   for(int i=woodVolleys.Count-1;i>=0;i--)if(woodVolleys[i].owner==boss){if(woodVolleys[i].warning!=null)Destroy(woodVolleys[i].warning);woodVolleys.RemoveAt(i);}
   AddFloatingHit("寄生结剥离 · 古树喘息",boss.position+Vector2.up*2,C("b6edb0"));DamageEnemy(boss,boss.maximum*.08f);
  }
  private GameObject WoodFanWarning(Enemy e,Vector2 aim,int count,float span){return EnemyMuzzleTell(e.position,aim);}
  private void WoodFan(Enemy e,Vector2 aim,int count,float span,float speed=4.3f)
  {
   for(int i=0;i<count;i++){Vector2 d=Quaternion.Euler(0,0,(i/(float)(count-1)-.5f)*span)*aim;int before=shots.Count;SpawnShot(e.position+d*(e.radius+.2f),d,true,null,e.position);if(shots.Count==before)continue;Shot s=shots[shots.Count-1];s.emitter=e;s.element=Element.Wood;s.damage=e.boss?1.7f:1.1f;s.speed=speed;s.life=4.2f;s.root.GetComponent<SpriteRenderer>().color=C("d7c47d");}
  }
  private bool WoodCharger(int k){return k==1||k==3||k==5||k==6||k==7||k==9||k==12;}
  private float WoodDamageMultiplier(Enemy e,Element element)
  {
   if(!woodlandMode)return 1;float amount=1;int k=ArrayIndexWood(e.kind);
   if(element!=Element.Fire){if(clock<e.phaseUntil&&!e.boss)amount*=.75f;
    if((k==1||k==5)&&!earlyVanguards.Contains(e)&&Vector2.Dot((position-e.position).normalized,e.attackDirection.sqrMagnitude>.01f?e.attackDirection:(position-e.position).normalized)>.3f)amount*=.65f;}
   return amount;
  }
  private void UpdateWoodEnemy(Enemy e,float dt)
  {
   int k=ArrayIndexWood(e.kind);if(e.kind==EnemyKind.WoodParasite){if(e.woodOwner!=null&&e.woodOwner.hp>0&&clock>=e.nextAttack){Vector2 d=(position-e.position).normalized;if(!ProjectilePathBlocked(e.position,position,.08f))WoodFan(e,d,3,35,3.8f);e.nextAttack=clock+3.2f;}return;}
   if(k<0)return;RecoverIceFooting(e);Vector2 old=e.position,to=position-e.position;
   if(e.boss&&!e.secondPhase&&e.hp<e.maximum*.7f){e.secondPhase=true;e.windup=0;if(e.warning!=null)Destroy(e.warning);e.recoveryUntil=clock+1.3f;e.nextAttack=clock+1.5f;AddFloatingHit(k==11?"过度生长 · 剥离寄生结":k==10?"孢囊张开 · 留意错时落点":"母虫抬甲 · 留出冲撞退路",e.position+Vector2.up*2,C("dbe5a0"));}
   if(e.chargeUntil>0){if(clock<e.chargeUntil){e.position=MoveEnemyCharge(e,e.attackDirection*(e.boss?5.8f:5)*dt);if(DistanceToSegment(position,old,e.position)<e.radius+.3f&&!ProjectilePathBlocked(e.position,position,.04f))TakeJourneyDamage(e.boss?1.8f:1.3f,Element.Wood);}else{e.chargeUntil=0;e.recoveryUntil=clock+1.25f;}if(e.chargeUntil>clock&&(e.position-old).sqrMagnitude<.000001f){e.chargeUntil=0;e.recoveryUntil=clock+1.25f;}}
   else if(e.windup>0){e.windup-=dt;if(e.windup<=0){if(e.warning!=null)Destroy(e.warning);ResolveWoodMove(e);}}
   else if(clock>=e.recoveryUntil){bool clear=!ProjectilePathBlocked(e.position,position,.1f);float range=k==9&&(e.attackNumber+1)%3==1?6:WoodCharger(k)?e.boss?11:4.8f:11;
    if(clear&&clock>=e.nextAttack&&to.magnitude<range){e.attackNumber++;e.attackDirection=k==9&&(e.attackNumber%3==1)?(IntentLead(.4f)-e.position).normalized:to.sqrMagnitude>.001f?to.normalized:Vector2.down;e.strikePoint=position;e.actionStarted=clock;e.windup=k==9&&e.attackNumber%3==1?.85f:e.boss?1.1f:k==4?1:.85f;e.nextAttack=clock+e.windup+(e.boss?1.5f:1.5f-journeyRooms[currentRoom].stage*.1f);
     if(!ForestBossTell(e,k))e.warning=k==8?Draw("Moth leaf ward preparation",windRingSprite??disc,e.position,Vector2.one*.8f,C("a9d7ad"),DepthAt(e.position.y)):WoodCharger(k)&&e.attackNumber%3==1?ChapterLineWarning(e.position,e.attackDirection,k==9?5.8f*.9f+e.radius+.28f:4.8f,k==9?e.radius*2+.56f:e.boss?1.6f:.8f):k==4||k==7||e.boss&&e.attackNumber%3==2?WarningCircle(e.strikePoint,.9f):WoodFanWarning(e,e.attackDirection,e.boss?21:k==2?7:5,e.boss?150:64);
    }else if(!clear||to.magnitude>(WoodCharger(k)?2.6f:5))MoveIcePursuit(e,e.speed*(clock<e.iceSlowUntil?.6f:1)*dt);
   }
   PoseWorkshopEnemy(e,old,dt);if(e.chargeUntil>clock||e.windup>0)e.body.flipX=e.attackDirection.x<0;if(clock<e.phaseUntil&&!e.boss)e.body.color=C("b7edc7");if(k==0||k==8)e.root.transform.position+=Vector3.up*(.2f+.04f*Mathf.Sin(clock*6));
  }
  private void ResolveWoodMove(Enemy e)
  {
   int k=ArrayIndexWood(e.kind),move=e.attackNumber%3;e.recoveryUntil=clock+(e.boss?1.45f:.75f);if(ResolveForestBossMove(e,k))return;
   if(!e.boss){
    if(k==8){Enemy ally=null;foreach(Enemy target in enemies)if(target!=e&&!target.boss&&target.kind!=EnemyKind.WoodParasite&&ArrayIndexWood(target.kind)!=8&&Vector2.Distance(target.position,e.position)<4&&!ProjectilePathBlocked(e.position,target.position,.04f)){ally=target;break;}
     if(ally!=null){ally.phaseUntil=clock+3;AddFloatingHit("护叶 · 火可穿透",ally.position+Vector2.up,C("bbebc0"));Vector2 delta=ally.position-e.position;var line=Stretch("Moth protective leaf link",square,(e.position+ally.position)*.5f,new Vector2(delta.magnitude,.03f),C("a9d7ad"),DepthAt(ally.position.y)+3);line.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg);fadingBodies.Add(new FadingBody{root=line,renderer=line.GetComponent<SpriteRenderer>(),expires=clock+.35f});return;}}
    if(k==5&&move==1){if(Vector2.Distance(position,e.position)<3&&!ProjectilePathBlocked(e.position,position,.04f))TakeJourneyDamage(1.3f,Element.Wood);return;}
    if(WoodCharger(k)&&move==1){e.chargeUntil=clock+.6f;return;}if(k==4){QueueWoodZone(e.strikePoint,1);return;}WoodFan(e,e.attackDirection,k==2?7:5,k==2?90:64);if(k==7)QueueWoodZone(e.strikePoint,1.05f);return;
   }
   if(k==9&&move==1){e.chargeUntil=clock+.9f;return;}
   if(move==2){QueueWoodZone(e.strikePoint,1);if(e.secondPhase)QueueWoodZone(e.strikePoint+new Vector2(2.2f,0),1.7f);WoodFan(e,e.attackDirection,e.secondPhase?17:13,135,4);return;}
   int count=k==11?18+WoodKnotsAlive(e)*2:k==10?22:18;if(e.secondPhase)count+=4;
   WoodFan(e,e.attackDirection,count,160);
   if(e.secondPhase)woodVolleys.Add(new WoodVolley{owner=e,aim=e.attackDirection,count=count-2,when=clock+.75f,warning=WoodFanWarning(e,e.attackDirection,count-2,160)});
  }
  private void QueueWoodZone(Vector2 desired,float delay)
  {
   if(!woodlandMode||woodZones.Count>=3)return;Vector2 at=NearestEnemyFooting(desired,.75f);
   if(IntersectsTransit(at,Vector2.one*2.4f)||ProjectilePathBlocked(at,at,.8f)||Blocked(at,.7f))return;
   var root=WarningCircle(at,.9f);root.name="Hostile spore patch, orange warning edge";woodZones.Add(new WoodZone{at=at,born=clock,active=clock+delay,until=clock+delay+3.2f,nextHit=clock+delay,root=root});
  }
  private void TickWoodCombat()
  {
   TickForestStrikes();
   for(int i=woodVolleys.Count-1;i>=0;i--){var v=woodVolleys[i];if(v.owner.hp<=0||!enemies.Contains(v.owner)){if(v.warning!=null)Destroy(v.warning);woodVolleys.RemoveAt(i);continue;}if(clock<v.when)continue;if(v.warning!=null)Destroy(v.warning);WoodFan(v.owner,v.aim,v.count,160,4.5f);woodVolleys.RemoveAt(i);}
   for(int i=woodZones.Count-1;i>=0;i--){var z=woodZones[i];if(clock>=z.until){if(z.root!=null)Destroy(z.root);woodZones.RemoveAt(i);continue;}if(clock<z.active)continue;
    if(z.root!=null)z.root.GetComponent<SpriteRenderer>().color=new Color(.49f,.59f,.19f,.26f);
    if(clock>=z.nextHit&&Vector2.Distance(position,z.at)<1.1f&&!ProjectilePathBlocked(z.at,position,.03f)){z.nextHit=clock+.85f;TakeJourneyDamage(.9f,Element.Wood);}
   }
   foreach(Enemy e in new List<Enemy>(enemies))if(e.hp>0&&clock<e.woodPoisonUntil&&clock>=e.woodPoisonNext){e.woodPoisonNext=clock+.7f;DamageEnemy(e,SkillDamage(3.5f));}
  }
  private int BurnWoodZones(Vector2 from,Vector2 to,float width)
  {
   if(!woodlandMode)return 0;int n=0;for(int i=woodZones.Count-1;i>=0;i--){var z=woodZones[i];if(DistanceToSegment(z.at,from,to)>width+.9f||ProjectilePathBlocked(from,z.at,.03f))continue;if(z.root!=null)Destroy(z.root);SpellSpark(z.at,C("ffdba0"),.35f);woodZones.RemoveAt(i);n++;}return n;
  }
  private void ClearWoodCombat(){CancelForestStrikes();foreach(var z in woodZones)if(z.root!=null)Destroy(z.root);woodZones.Clear();foreach(var v in woodVolleys)if(v.warning!=null)Destroy(v.warning);woodVolleys.Clear();}
 }
}
