using UnityEngine;
namespace NakiLegend.Prototype {
 public sealed partial class NakiPrototype {
  private float RoyalAttackDamage(Enemy source,float normal,float royal){return source!=null&&(source.kind==EnemyKind.CapitalKing||source.kind==EnemyKind.CapitalConduit&&source.iceOwner!=null&&source.iceOwner.kind==EnemyKind.CapitalKing)?royal:normal;}
  private bool RoyalRangedActive(Enemy e,CapitalFight s){if(!IsCapitalBoss(e.kind))return false;if(s.salvo>0||s.trapAfterBeamAt>0||CapitalBeamActive(e))return true;foreach(var z in signatureZones)if(z.owner==e&&z.mode==4&&clock<z.end)return true;foreach(var impact in manorImpacts)if(impact.owner==e&&clock<impact.when)return true;return false;}
  private bool TickRoyalCombo(Enemy e,CapitalFight s,float dt){
   if(e.kind!=EnemyKind.CapitalKing||s.royalCombo<=0||e.chargeUntil>clock||clock<s.royalAt)return false;
   if(!s.royalWindup){
    s.royalWindup=true;e.attackDirection=(IntentLead(.25f)-e.position).normalized;if(e.attackDirection.sqrMagnitude<.001f)e.attackDirection=Vector2.down;
    e.windup=.6f;s.royalAt=clock+.6f;e.warning=WarningSector(e.position,e.attackDirection,2.8f);AddFloatingHit("王剑追斩 · 横移闪避",e.position+Vector2.up*2,C("f1c38b"));
   }else{
    if(e.warning!=null)Destroy(e.warning);e.warning=null;e.windup=0;e.actionStarted=clock;
    Vector2 to=position-e.position;if(to.magnitude<3&&Vector2.Dot(to.normalized,e.attackDirection)>.25f&&!ProjectilePathBlocked(e.position,position,.04f))TakeJourneyDamage(2.4f,Element.Physical);
    var slash=Draw("Royal sword committed sweep",EnemyCrescent,e.position,Vector2.one*5.6f,C("ffe3b0"),DepthAt(e.position.y)+8);slash.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(e.attackDirection.y,e.attackDirection.x)*Mathf.Rad2Deg);fadingBodies.Add(new FadingBody{root=slash,renderer=slash.GetComponent<SpriteRenderer>(),expires=clock+.25f});
    PressureFan(e.position+e.attackDirection*.85f,e.attackDirection,3,22,3.4f,Element.Light,.85f);s.royalCombo--;s.royalWindup=false;s.royalAt=clock+.28f;
    e.recoveryUntil=clock+.3f;e.nextAttack=Mathf.Max(e.nextAttack,clock+.8f);Sfx("spear",false,.55f);
   }
   UpdateJourneyEnemyPose(e,false);return true;
  }
 }
}
