using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private const float LightRegenRate=5,LightRegenDelay=1.5f,LightBeamDrain=16,LightBeamMaximum=2.5f;
  private static readonly char[] OriginKeys={'l','o','p'};
  private float originSecondReady,originThirdReady,lightRegenReady,lightBeamElapsed,lightBeamDamageTime;
  private float solarReleaseAt,solarFlashUntil,originGuardRefundLeft,originMarkUntil,originMarkProcReady;
  private bool lightChannel,solarCharging,lightNeedsRelease;
  private Enemy lightBeamTarget,solarTarget,originMarkedTarget;
  private int originMarkLastGroup=-1,originMarkPulses;
  private GameObject originBeamEdge,originBeamCore,originBeamTip,originBeamMuzzle,originGuardVisual,originMarkVisual,solarChargeVisual;
  private AudioSource originChannelAudio;
  private bool OriginActionActive {get{return lightChannel||solarCharging;}}
  private string OriginSkillName(int id){return origin==Origin.Dark?new[]{"流星","蚀魂印","星陨"}[id]:new[]{"辉光束","守心光幕","别·天神"}[id];}
  private float OriginSkillCost(int id){return origin==Origin.Dark?new[]{1f,6,16}[id]:new[]{4f,20,35}[id];}
  private float OriginSkillCooldown(int id){return origin==Origin.Dark?new[]{.8f,5f,5f}[id]:new[]{.35f,5,5}[id];}
  private float OriginSkillReady(int id){return id==0?nextSpell:id==1?originSecondReady:originThirdReady;}
  private string OriginLearningPlace(int id){return id==0?"鸣沙洲 · 研读古井碑":id==1?"雷雨高原 · 首层守关后的巡路记录":OriginAbilityKnown(1)?"赤炉群岛 · 炉匠火种训练":"先领悟 O 战技，再研读赤炉淬炼图";}
  private void TickLightRecovery(float dt)
  {
   if(origin==Origin.Dark){energy=0;return;}
   if(origin==Origin.Light&&!OriginActionActive&&clock>=lightRegenReady)energy=Mathf.Min(LightMaximum,energy+LightRegenRate*dt);
  }
  private Enemy OriginTarget(Enemy keep,float range)
  {
   if(keep!=null&&keep.hp>0&&enemies.Contains(keep)&&Vector2.Distance(position,keep.position)<=range&&!ProjectilePathBlocked(position,keep.position,.06f))return keep;
   Enemy best=null;float distance=range*range;
   foreach(Enemy e in enemies){float d=(e.position-position).sqrMagnitude;if(e.hp>0&&d<distance&&!ProjectilePathBlocked(position,e.position,.06f)){best=e;distance=d;}}
   return best;
  }
  private Vector2 OriginAim(Enemy target){Vector2 dir=target!=null?target.position-position:facing;return dir.sqrMagnitude>.001f?dir.normalized:Vector2.up;}
  private void TickOriginInput(float dt,bool held,bool second,bool third)
  {
   if(!held)lightNeedsRelease=false;
   if(clock<dashUntil){StopOriginAction(held);return;}
   if(second&&CastOriginSlot(1))return;if(third&&CastOriginSlot(2))return;
   if(lightChannel){TickLightChannel(dt,held);return;}
   if(solarCharging){if(clock>=solarReleaseAt)ReleaseSolarBeam();else if(solarChargeVisual!=null){solarChargeVisual.transform.position=position+Vector2.up*.25f;solarChargeVisual.transform.localScale=Vector3.one*Mathf.Lerp(1.2f,.3f,Mathf.Clamp01(1-(solarReleaseAt-clock)/.25f));}return;}
   if(held&&!lightNeedsRelease&&clock>=swordUntil&&clock>=castPoseUntil&&!bowCharging){if(origin==Origin.Dark)CastMeteorVolley();else if(origin==Origin.Light)CastLightVolley();}
  }
  private bool CastOriginSlot(int id)
  {
   if(origin==Origin.Unchosen||!OriginAbilityKnown(id)){JourneyHint("尚未领悟 · "+OriginLearningPlace(id));return false;}
   if(clock<OriginSkillReady(id)||clock<dashUntil||clock<swordUntil)return false;
   if(id==0){if(origin==Origin.Dark)CastMeteorVolley();else CastLightVolley();return true;}
   if(origin==Origin.Dark&&id==2){CastEmpoweredMeteor();return true;}
   Enemy target=origin==Origin.Dark?OriginTarget(null,9.5f):null;
   if(origin==Origin.Dark&&target==null){JourneyHint("附近没有可见的蚀魂印目标");return false;}
   if((origin==Origin.Light?energy:darkPower)<OriginSkillCost(id))return false;
   StopOriginAction(true);CancelBowCharge();SpendPower(origin==Origin.Dark?PowerPool.Dark:PowerPool.Light,OriginSkillCost(id));RecordEncounterCast();
   if(id==1){
    originSecondReady=clock+OriginSkillCooldown(1);
    if(origin==Origin.Light){originGuardUntil=clock+LightGuardDuration;originGuardReturnAt=0;originGuardRefundLeft=8;CreateGuardVisual();SkillSound("lightguard","cast");}
    else{originMarkedTarget=target;originMarkUntil=clock+8;originMarkProcReady=clock;originMarkLastGroup=-1;originMarkPulses=0;if(originMarkVisual!=null)Destroy(originMarkVisual);originMarkVisual=Draw("Soul seal sigil",windRingSprite??disc,target.position+Vector2.up*.55f,Vector2.one*.8f,C("c38dee"),DepthAt(target.position.y)+20);SkillSound("soulseal","cast");ApplySkillHit(target,OriginDamage(18)*DarkStrength,Element.Dark,"soulseal");AddFloatingHit("蚀魂印 · 自发脉冲，命中提前引爆",target.position+Vector2.up*1.15f,C("d0a5f4"));}
   }else{
    originThirdReady=clock+OriginSkillCooldown(2);solarCharging=true;divinePulsesRemaining=3;solarReleaseAt=clock+.25f;solarTarget=OriginTarget(null,12);castDirection=OriginAim(solarTarget);castPoseUntil=solarReleaseAt;
    solarChargeVisual=Draw("Gathering sunlight",windRingSprite??disc,position+Vector2.up*.25f,Vector2.one,C("ffdd88"),810);SkillSound("solar","cast");
   }
   RequestCheckpoint();return true;
  }
  private void BeginLightChannel()
  {
   if(lightChannel||solarCharging||lightNeedsRelease||!OriginAbilityKnown(0)||clock<nextSpell||clock<dashUntil||energy<4)return;
   if(ProjectilePathBlocked(position,position+OriginAim(OriginTarget(null,8.5f))*.45f,.05f))return;
   SpendPower(PowerPool.Light,4);RecordEncounterCast();lightChannel=true;lightBeamElapsed=lightBeamDamageTime=0;lightBeamTarget=OriginTarget(null,8.5f);originBeamDirection=OriginAim(lightBeamTarget);
   castDirection=originBeamDirection;castPoseUntil=clock+.08f;CreateLightBeam(false);RenderLightBeam(false,0);SkillSound("lightbeam","cast");UpdateOriginAudio();
  }
  private void TickLightChannel(float dt,bool held)
  {
   if(!held||clock<dashUntil||health<=0){StopOriginAction(held);return;}
   float paid=Mathf.Min(Mathf.Max(0,dt),LightBeamMaximum-lightBeamElapsed,energy/LightBeamDrain);
   if(paid<=.00001f){StopOriginAction(true);return;}
   SpendPower(PowerPool.Light,LightBeamDrain*paid);lightBeamElapsed+=paid;lightBeamDamageTime+=paid;
   lightBeamTarget=OriginTarget(lightBeamTarget,8.5f);originBeamDirection=OriginAim(lightBeamTarget);castDirection=originBeamDirection;castPoseUntil=clock+.08f;
   while(lightBeamDamageTime>=.09999f){lightBeamDamageTime-=.1f;RenderLightBeam(false,4.5f);}
   RenderLightBeam(false,0);
   if(lightBeamElapsed>=LightBeamMaximum-.0001f||energy<=.0001f)StopOriginAction(true);
  }
  private void CreateLightBeam(bool solar)
  {
   DestroyLightBeam();originBeam=new GameObject(solar?"Solar piercing beam":"Continuous golden light beam");originBeam.transform.SetParent(world,false);
   originBeamEdge=Stretch("Soft gold beam sheath",square,position,Vector2.one,new Color(1,.73f,.25f,.3f),805);
   originBeamCore=Stretch("White gold beam core",square,position,Vector2.one,C("fff8d4"),807);
   originBeamTip=Draw("Light impact flare",disc,position,Vector2.one*.2f,C("fff2b5"),809);
   originBeamMuzzle=Draw("Light focus rune",windRingSprite??disc,position,Vector2.one*.35f,C("ffe5a1"),808);
   foreach(GameObject part in new[]{originBeamEdge,originBeamCore,originBeamTip,originBeamMuzzle})part.transform.SetParent(originBeam.transform,true);
  }
  private void RenderLightBeam(bool solar,float damage)
  {
   if(originBeam==null)return;Vector2 dir=originBeamDirection.normalized,start=position+dir*.42f;
   Vector2 visualOffset=pixelHeroReady&&sword!=null&&Vector2.Distance(sword.transform.position,position)<1.5f?(Vector2)sword.transform.position+dir*.16f-start:Vector2.up*.4f;
   start+=visualOffset;if(!solar&&lightBeamTarget!=null&&lightBeamTarget.hp>0)dir=(lightBeamTarget.position+Vector2.up*.3f-start).normalized;Vector2 end=CoverBeamEnd(start,dir,solar?12:8.5f,solar?.34f:.13f);
   if(ProjectilePathBlocked(position,start,.06f))end=start;
   if(Vector2.Dot(end-start,dir)<0)end=start;
   var hits=new List<Enemy>();foreach(Enemy e in enemies)if((end-start).sqrMagnitude>.0001f&&e.hp>0&&DistanceToSegment(e.position+Vector2.up*.3f,start,end)<e.radius+(solar?.27f:.10f)&&Vector2.Dot(e.position-start,dir)>=-e.radius&&!ProjectilePathBlocked(start,e.position+Vector2.up*.3f,.05f))hits.Add(e);
   hits.Sort((a,b)=>Vector2.Dot(a.position-start,dir).CompareTo(Vector2.Dot(b.position-start,dir)));
   if(!solar&&hits.Count>2){float limit=Mathf.Min((end-start).magnitude,Vector2.Dot(hits[1].position-start,dir)+hits[1].radius);end=start+dir*limit;hits.RemoveRange(2,hits.Count-2);}
   Vector2 delta=end-start;float angle=Mathf.Atan2(dir.y,dir.x)*Mathf.Rad2Deg;
   foreach(GameObject part in new[]{originBeamEdge,originBeamCore}){part.transform.position=(start+end)*.5f;part.transform.rotation=Quaternion.Euler(0,0,angle);}
   float pulse=1+.06f*Mathf.Sin(clock*31);originBeamEdge.transform.localScale=new Vector3(delta.magnitude,(solar?.64f:.24f)*pulse,1);originBeamCore.transform.localScale=new Vector3(delta.magnitude,solar?.20f:.065f,1);
   originBeamMuzzle.transform.position=start;originBeamMuzzle.transform.Rotate(0,0,solar?8:4);originBeamTip.transform.position=end;originBeamTip.transform.localScale=Vector3.one*(solar?.38f:.17f)*pulse;
   if(damage>0){int index=0;foreach(Enemy e in hits){float falloff=solar?Mathf.Max(.4f,Mathf.Pow(.85f,index++)):index++==0?1:.6f;bool old=skillImpactActive;skillImpactActive=true;try{ApplyElementDamage(e,OriginDamage(damage*falloff),Element.Light);}finally{skillImpactActive=old;}if(e.root!=null&&fadingBodies.Count<90){ImpactStroke(e.position+Vector2.up*.2f,dir,.22f,.04f,C("ffe4a0"));}}
    if(hits.Count>0)SkillSound(solar?"solar":"lightbeam","hit");}
  }
  private void ReleaseSolarBeam()
  {
   // Retain the private callback name for old editor tooling; P now emits radial pulses.
   EmitDivineLightPulse();
  }
  private void StopOriginAction(bool requireRelease=true)
  {
   if(lightChannel&&!AudioSuspended&&!AudioNarrative)SkillSound("lightbeam","end");
   bool active=OriginActionActive;if(requireRelease&&active)lightNeedsRelease=true;
   if(lightChannel)nextSpell=Mathf.Max(nextSpell,clock+.35f);if(active&&origin==Origin.Light)lightRegenReady=Mathf.Max(lightRegenReady,clock+LightRegenDelay);
   lightChannel=solarCharging=false;divinePulsesRemaining=0;lightBeamTarget=solarTarget=null;lightBeamDamageTime=0;solarFlashUntil=0;
   if(solarChargeVisual!=null)Destroy(solarChargeVisual);solarChargeVisual=null;DestroyLightBeam();if(originChannelAudio!=null)originChannelAudio.Stop();
   if(active){castPoseUntil=clock;RequestCheckpoint();}
  }
  private void DestroyLightBeam(){if(originBeam!=null)Destroy(originBeam);originBeam=originBeamEdge=originBeamCore=originBeamTip=originBeamMuzzle=null;}
  private void CreateGuardVisual()
  {
   if(originGuardVisual!=null)Destroy(originGuardVisual);originGuardVisual=new GameObject("Steady light guard");originGuardVisual.transform.SetParent(world,false);originGuardVisual.transform.position=position;
   GameObject ring=Draw("Guard rune ring",windRingSprite??disc,position+Vector2.up*.25f,Vector2.one*1.15f,new Color(1,.89f,.6f,.7f),806);ring.transform.SetParent(originGuardVisual.transform,true);
   for(int i=0;i<4;i++){float a=i*Mathf.PI*.5f;Vector2 at=position+Vector2.up*.25f+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*.48f;var plate=Stretch("Guard light plate",square,at,new Vector2(.22f,.045f),C("fff2ca"),807);plate.transform.rotation=Quaternion.Euler(0,0,i*90+90);plate.transform.SetParent(originGuardVisual.transform,true);}
  }
  private void RefundGuardLight()
  {
   if(clock<originGuardReturnAt||originGuardRefundLeft<=0)return;float refund=Mathf.Min(2,originGuardRefundLeft);originGuardRefundLeft-=refund;energy=Mathf.Min(LightMaximum,energy+refund);originGuardReturnAt=clock+.4f;SkillSound("lightguard","hit");
  }
  private void TrySoulSealHit(Shot shot,Enemy target)
  {
   if(shot.hostile)return;TriggerSoulSeal(target,shot.orb?shot.meteorGroup:-1);
  }
  private void TriggerSoulSeal(Enemy target,int group=-1)
  {
   if(origin!=Origin.Dark||target!=originMarkedTarget||clock>=originMarkUntil||clock<originMarkProcReady||group>=0&&group==originMarkLastGroup||originMarkPulses>=6)return;
   if(group>=0)originMarkLastGroup=group;originMarkProcReady=clock+.8f;originMarkPulses++;Vector2 at=target.position;
   SpellSpark(at,C("c59bea"),1.8f);for(int i=0;i<6;i++){Vector2 ray=Quaternion.Euler(0,0,i*60)*Vector2.right;ImpactStroke(at+ray*.28f,ray,.28f,.045f,C("ceabf0"));}
   foreach(Enemy e in new List<Enemy>(enemies))if(e.hp>0&&Vector2.Distance(e.position,at)<2.2f+e.radius&&!ProjectilePathBlocked(at,e.position,.04f)){bool old=skillImpactActive;skillImpactActive=true;try{ApplyElementDamage(e,OriginDamage(16)*DarkStrength,Element.Dark);}finally{skillImpactActive=old;}}
   AddFloatingHit("蚀魂爆发 "+originMarkPulses+" / 6",at+Vector2.up*1.15f,C("d0a5f4"));SkillSound("soulseal","hit");
  }
  private void TransferSoulSeal()
  {
   if(originMarkedTarget==null||originMarkedTarget.hp>0&&enemies.Contains(originMarkedTarget)||clock>=originMarkUntil||originMarkPulses>=6)return;
   Vector2 at=originMarkedTarget.position;Enemy next=null;float range=3.8f;
   foreach(Enemy e in enemies)if(e.hp>0&&Vector2.Distance(e.position,at)<range&&!ProjectilePathBlocked(at,e.position,.04f)){next=e;range=Vector2.Distance(e.position,at);}
   if(next!=null){originMarkedTarget=next;AddFloatingHit("蚀魂印转移",next.position+Vector2.up*1.15f,C("d0a5f4"));}
  }
  private void TickOriginVisuals()
  {
   TickStarfall();TransferSoulSeal();
   if(originMarkedTarget!=null&&originMarkedTarget.hp>0&&clock>=originMarkProcReady+.45f)TriggerSoulSeal(originMarkedTarget);
   if(originGuardVisual!=null){if(clock>=originGuardUntil){SkillSound("lightguard","end");Destroy(originGuardVisual);originGuardVisual=null;}else{originGuardVisual.transform.position=position;}}
   if(originMarkVisual!=null){if(clock>=originMarkUntil||originMarkPulses>=6||originMarkedTarget==null||originMarkedTarget.hp<=0||!enemies.Contains(originMarkedTarget)){Destroy(originMarkVisual);originMarkVisual=null;originMarkedTarget=null;}else{originMarkVisual.transform.position=originMarkedTarget.position+Vector2.up*.55f;originMarkVisual.transform.rotation=Quaternion.Euler(0,0,clock*28);}}
   if(!lightChannel&&!solarCharging&&originBeam!=null){if(clock>=solarFlashUntil)DestroyLightBeam();else{foreach(var r in originBeam.GetComponentsInChildren<SpriteRenderer>()){Color c=r.color;c.a=Mathf.Min(c.a,(solarFlashUntil-clock)/.35f);r.color=c;}}}
  }
  private void UpdateOriginAudio()
  {
   bool audible=lightChannel&&!AudioSuspended&&!AudioNarrative&&!openingActive&&health>0;
   if(!audible){if(originChannelAudio!=null){if(AudioSuspended)originChannelAudio.Pause();else originChannelAudio.Stop();}return;}
   if(originChannelAudio==null){originChannelAudio=AudioBus("Naki audio light channel",true);originChannelAudio.clip=AudioAsset("light_channel_loop");}
   originChannelAudio.volume=audioMaster*audioEffects*.18f;if(!originChannelAudio.isPlaying&&originChannelAudio.clip!=null){originChannelAudio.UnPause();if(!originChannelAudio.isPlaying)originChannelAudio.Play();}
  }
  private void ResetOriginCombat()
  {
   ClearStarfall();
   lightChannel=solarCharging=lightNeedsRelease=false;divinePulsesRemaining=0;divineSplitSoundReady=0;lightBeamElapsed=lightBeamDamageTime=0;originSecondReady=originThirdReady=lightRegenReady=solarReleaseAt=solarFlashUntil=originGuardRefundLeft=0;
   originMarkedTarget=lightBeamTarget=solarTarget=null;originMarkUntil=originMarkProcReady=0;originMarkLastGroup=-1;originMarkPulses=0;DestroyLightBeam();
   foreach(GameObject g in new[]{originGuardVisual,originMarkVisual,solarChargeVisual})if(g!=null)Destroy(g);originGuardVisual=originMarkVisual=solarChargeVisual=null;if(originChannelAudio!=null)originChannelAudio.Stop();
  }
  private void ResetOriginRoomEffects()
  {
   ClearStarfall();
   originGuardUntil=originGuardReturnAt=originGuardRefundLeft=originMarkUntil=originMarkProcReady=0;originMarkedTarget=null;originMarkLastGroup=-1;originMarkPulses=0;
   foreach(GameObject g in new[]{originGuardVisual,originMarkVisual})if(g!=null)Destroy(g);originGuardVisual=originMarkVisual=null;
  }
 }
}
