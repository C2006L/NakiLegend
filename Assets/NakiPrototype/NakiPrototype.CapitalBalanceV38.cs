using UnityEngine;
namespace NakiLegend.Prototype {
 public sealed partial class NakiPrototype {
  private bool CapitalBeamActive(Enemy owner){if(!IsCapitalBoss(owner.kind))return false;foreach(var z in signatureZones)if(z.owner==owner&&z.mode==8&&clock<z.end)return true;return false;}
  private bool CommanderChargeActive(Enemy owner){CapitalFight s;return owner.kind==EnemyKind.CapitalCommander&&capitalFights.TryGetValue(owner,out s)&&(owner.chargeUntil>clock||owner.windup>0&&s.move==0);}
  private void CapitalShieldSweep(Enemy owner){
   Vector2 delta=position-owner.position;
   if(delta.magnitude<2.8f&&Vector2.Dot(delta.normalized,owner.attackDirection)>.25f&&!ProjectilePathBlocked(owner.position,position,.04f))TakeJourneyDamage(1.4f,Element.Physical);
   var sweep=Draw("Harbour commander committed shield sweep",EnemyCrescent,owner.position,Vector2.one*5.2f,C("e8d5b3"),DepthAt(owner.position.y)+8);sweep.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(owner.attackDirection.y,owner.attackDirection.x)*Mathf.Rad2Deg);fadingBodies.Add(new FadingBody{root=sweep,renderer=sweep.GetComponent<SpriteRenderer>(),expires=clock+.28f});Sfx("spear",false,.5f);owner.recoveryUntil=clock+.9f;
  }
  private void CreateMagistrateBeams(Enemy owner,Vector2 side){
   foreach(int sign in new[]{-1,1}){
    Vector2 offset=side*sign*1.8f;int before=signatureZones.Count;
    AddSignatureZone(owner,8,owner.position+offset+owner.attackDirection*(owner.radius+.3f),owner.attackDirection,8,2.2f);
    if(signatureZones.Count>before){var beam=signatureZones[signatureZones.Count-1];beam.anchorOffset=offset;beam.gap=0;beam.root.name="Magistrate parallel decree beam / "+sign;}
   }
  }
  private void CapitalNodeDisrupted(Enemy owner,Enemy node){
   if(!owner.boss)return;
   if(owner.kind==EnemyKind.CapitalMagistrate){
    SignatureZone nearest=null;float distance=float.PositiveInfinity;
    foreach(var zone in signatureZones)if(zone.owner==owner&&zone.mode==8){float d=Vector2.Distance(zone.at,node.position);if(d<distance){distance=d;nearest=zone;}}
    if(nearest!=null){RemoveSignatureZone(nearest);signatureZones.Remove(nearest);}
   }
   CapitalFight s=CapitalState(owner);s.shield=0;owner.recoveryUntil=Mathf.Max(owner.recoveryUntil,clock+.65f);owner.nextAttack=Mathf.Max(owner.nextAttack,clock+.8f);AddFloatingHit(owner.kind==EnemyKind.CapitalCommander?"弩架摧毁 · 绕侧反击":"王印节点断开 · 封锁减弱",owner.position+Vector2.up*1.8f,C("f4dfa1"));
  }
  private void InterruptCapitalZones(Enemy owner){
   // Destroying every node interrupts its powered hazards; already-fired projectiles keep travelling.
   for(int i=signatureZones.Count-1;i>=0;i--)if(signatureZones[i].owner==owner){RemoveSignatureZone(signatureZones[i]);signatureZones.RemoveAt(i);}
   for(int i=manorImpacts.Count-1;i>=0;i--)if(manorImpacts[i].owner==owner){var impact=manorImpacts[i];if(impact.ring!=null)Destroy(impact.ring);if(impact.orb!=null)Destroy(impact.orb);manorImpacts.RemoveAt(i);}
  }
 }
}
