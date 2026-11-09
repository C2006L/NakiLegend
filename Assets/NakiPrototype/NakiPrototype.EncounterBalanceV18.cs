using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private Enemy attackEmitter;
  private Sprite slowBubbleSprite;
  private Sprite SlowBubbleSprite {get{
   if(slowBubbleSprite==null)slowBubbleSprite=MakeSprite(new[]{
    ".....dddd.....","...ddggggdd...","..dghhhhgggd..",".dghwwhhggggd.",".dghwwhgggggd.","dghhhhgggggggd","dghhhggggggggd","dggggggggggggd","dggggggggggggd",".dggggggggggd.",".dggggggggggd.","..dggggggggd..","...ddggggdd...",".....dddd....."},
    new System.Collections.Generic.Dictionary<char,Color>{{'d',C("492435")},{'g',new Color(.75f,.22f,.32f,.6f)},{'h',C("f58996")},{'w',C("fff3ed")}},14);
   return slowBubbleSprite;
  }}
  private float LightMaximum {get{return new[]{90f,105,120,135,150,170}[CombatMasteryTier];}}
  private static readonly float[,] BossLifeBudget={{390,900,1060},{1290,1400,1570},{1620,1790,2020},{2240,2410,2690},{2800,2970,3300}};
  // Apply once after authored budgets; guardian promotion uses this after its final HP override.
  private void ApplyBossLifeIncrease(Enemy enemy)
  {
   if(!enableDesertJourney||!enemy.boss)return;
   float multiplier=CurrentIsland.ordinal<=4?1.10f:1.18f;
   enemy.hp*=multiplier;enemy.maximum*=multiplier;
  }
  private float NormalLifeBudget {get{return new[]{30f,43,58,75,95,125,145}[EncounterChapterTier];}}
  private bool RecoveryWindow(Enemy e){BossBreath s;return e.boss&&bossBreaths.TryGetValue(e,out s)&&clock<s.until;}
  private float BossActiveSeconds(Enemy e){return e.kind==EnemyKind.CapitalKing?14:e.kind==EnemyKind.ManorCaptain?7.5f:CurrentIsland.ordinal<=2?10:12;}
  private void ApplyEncounterLife(Enemy e)
  {
   if(!enableDesertJourney)return;
   if(IsCapitalEnemy(e.kind)){ConfigureCapitalEnemy(e);return;}
   if(e.boss){if(manorCourtyardMode){if(e.kind!=EnemyKind.ManorCaptain)e.hp=e.maximum=e.maximum*1.12f;}else e.hp=e.maximum=woodlandMode&&e.kind==WoodKinds[9]?MotherLifeBudget:BossLifeBudget[Mathf.Clamp(CurrentIsland.ordinal-1,0,4),Mathf.Clamp(journeyRooms[currentRoom].stage,0,2)];return;}
   if(CombatFixture(e)){e.hp=e.maximum=e.kind==EnemyKind.WoodParasite?60:40;return;}
   bool heavy=e.kind==EnemyKind.StoneSentinel||e.kind==EnemyKind.CopperRoller||e.kind==EnemyKind.ThunderStone||e.kind==EnemyKind.IceGuard||e.kind==EnemyKind.SnowCrab||e.kind==EnemyKind.MoltenCrab||e.kind==EnemyKind.ManorShield||e.kind==EnemyKind.ManorHammer;
   bool fragile=NaturalIntent(e)==EnemyIntent.Artillery||NaturalIntent(e)==EnemyIntent.Support||e.kind==EnemyKind.SandKite||e.kind==EnemyKind.IceWisp||e.kind==EnemyKind.ManorMage;
   e.hp=e.maximum=(ApprovedCaptainEncounter?85:NormalLifeBudget)*(heavy?1.65f:fragile?.8f:1);
  }
  private void ApplySummonedLife(Enemy e)
  {if(!e.boss&&e.summoned&&!CombatFixture(e)){e.hp=e.maximum=(MotherEncounter?Mathf.Min(e.maximum,31.5f):e.maximum*.7f);e.iceArmor=0;e.elite=false;e.tacticWardUntil=0;}}
  private void NormalizeHostileShot(Shot s)
  {
   if(!s.hostile||s.hostileNormalized||!enableDesertJourney)return;s.hostileNormalized=true;
   if(s.emitter!=null&&s.emitter.kind==EnemyKind.ManorCaptain)return;
   bool referenceAdds=ApprovedCaptainEncounter,fast=s.speed>=5.3f;
   if(referenceAdds&&fast){int active=0;foreach(Shot other in shots)if(other!=s&&other.hostile&&other.hostileNormalized&&other.hostileFast)active++;if(active>=6)fast=false;}
   s.hostileFast=fast;s.speed=referenceAdds?(fast?5f:2.25f):fast?5f+EncounterChapterTier*.15f:2.1f+EncounterChapterTier*.07f;
   if(fast&&s.emitter!=null&&s.emitter.boss)s.speed=Mathf.Min(s.speed,Mathf.Max(3.8f,Vector2.Distance(s.position,position)/.45f));
   s.damage=ProfileShotDamage(s,fast)/EnemyAttackScale;s.life=s.emitter!=null&&s.emitter.boss?Mathf.Clamp(s.life,fast?2.4f:4.2f,fast?2.9f:5.5f):Mathf.Min(s.life,fast?2.4f:referenceAdds?5f:4.5f);s.tracking=false;
   var renderer=s.root.GetComponent<SpriteRenderer>();
   // Normalize combat timing independently from weapon identity. Keep authored arrows and spear bolts.
   if(renderer.sprite!=HostileBulletSprite&&renderer.sprite!=ReadableEnemyBullet(true)&&renderer.sprite!=ReadableEnemyBullet(false)){RemoveRoundBulletDecorations(s);return;}
   renderer.color=fast?C("ffc27a"):C("e9b878");
   if(fast){RemoveRoundBulletDecorations(s);renderer.sprite=RoyalProjectile(s)?RoyalProjectileArt(true):ReadableEnemyBullet(true);renderer.color=Color.white;s.root.transform.localScale=Vector3.one*.46f;s.root.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(s.direction.y,s.direction.x)*Mathf.Rad2Deg);}
   else{
    renderer.sprite=RoyalProjectile(s)?RoyalProjectileArt(false):ElementHostileOrb(s.element);renderer.color=Color.white;s.root.transform.localScale=Vector3.one*(s.heavyParent?.62f:.32f);s.root.transform.rotation=Quaternion.identity;
    foreach(Transform child in s.root.transform)if(child.name=="Hostile bullet outline"||child.name=="Hostile bullet core")child.gameObject.SetActive(false);
    var trail=s.root.GetComponent<TrailRenderer>();if(trail!=null){trail.emitting=false;trail.Clear();}
   }
  }
  private void LimitHostileFan(ref int count,ref float spacing,float speed)
  {
   if(attackEmitter!=null&&attackEmitter.kind==EnemyKind.ManorCaptain)return;
   int limit=attackEmitter!=null&&attackEmitter.boss?EncounterChapterTier<2?3:EncounterChapterTier<5?4:5:3;
   if(speed<5.3f||count<=limit)return;float span=(count-1)*spacing;count=limit;spacing=Mathf.Min(26,span/(count-1));
  }
 }
}
