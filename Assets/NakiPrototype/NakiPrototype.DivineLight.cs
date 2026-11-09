using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private sealed class DivineLightFamily
  {
   public int remaining=3;
   public readonly HashSet<Enemy> hitTargets=new HashSet<Enemy>();
  }
  private int divinePulsesRemaining;
  private float divineSplitSoundReady;
  private void EmitDivineLightPulse()
  {
   if(!solarCharging||divinePulsesRemaining<=0)return;
   if(solarChargeVisual!=null)Destroy(solarChargeVisual);solarChargeVisual=null;
   int pulse=3-divinePulsesRemaining;float phase=pulse*20;
   for(int i=0;i<6;i++){
    Vector2 direction=Quaternion.Euler(0,0,phase+i*60)*Vector2.right;
    SpawnDivineLight(position+direction*.48f,direction,OriginDamage(22),new DivineLightFamily(),0,1.25f,position);
   }
   SpellSpark(position+Vector2.up*.2f,C("ffe8a1"),.65f);SkillSound("solar","pulse");
   divinePulsesRemaining--;solarCharging=divinePulsesRemaining>0;solarReleaseAt=clock+.18f;castPoseUntil=clock+.16f;
   lightRegenReady=Mathf.Max(lightRegenReady,clock+LightRegenDelay);RequestCheckpoint();
  }
  private bool SpawnDivineLight(Vector2 at,Vector2 direction,float damage,DivineLightFamily family,int generation,float lifetime,Vector2 originAt)
  {
   int before=shots.Count;SpawnShot(at,direction,false,null,originAt);if(shots.Count==before)return false;
   Shot shot=shots[shots.Count-1];shot.divineFamily=family;shot.divineGeneration=generation;
   shot.element=Element.Light;shot.skillAudio="solar";shot.damage=damage;shot.speed=8.5f;shot.life=lifetime;shot.tracking=false;shot.hitsLeft=1;
   shot.root.name=generation==0?"Divine light primary orb":"Divine light split orb";
   float size=generation==0?.24f:.17f;var renderer=shot.root.GetComponent<SpriteRenderer>();renderer.sprite=disc;renderer.color=C("fff8da");shot.root.transform.localScale=Vector3.one*size;
   GameObject halo=Draw("Warm gold light halo",softDisc??disc,at,Vector2.one*size*1.7f,new Color(1,.73f,.24f,.4f),renderer.sortingOrder-1);halo.transform.SetParent(shot.root.transform,true);
   AddShotRibbon(shot,C("ffd778"),generation==0?.055f:.035f);return true;
  }
  private void SplitDivineLight(Shot shot,Vector2 at,bool wall)
  {
   DivineLightFamily family=shot.divineFamily;
   if(family==null||shot.divineSplitResolved)return;shot.divineSplitResolved=true;
   if(family.remaining<=0)return;
   int active=0;foreach(Shot live in shots)if(live.divineFamily!=null)active++;
   if(active>=48)return;
   family.remaining--;Vector2 forward=wall?-shot.direction:shot.direction;
   Vector2 originAt=wall?at-shot.direction*.2f:at;
   for(int i=0;i<2&&active<48;i++){
    Vector2 direction=Quaternion.Euler(0,0,i==0?-38:38)*forward;
    if(SpawnDivineLight(originAt+direction*.16f,direction,shot.damage*.72f,family,shot.divineGeneration+1,Mathf.Max(.45f,Mathf.Min(.8f,shot.life)),originAt))active++;
   }
   SpellSpark(at,C("fff0b0"),.25f);
   if(clock>=divineSplitSoundReady){divineSplitSoundReady=clock+.12f;SkillSound("lightbeam","hit");}
  }
 }
}
