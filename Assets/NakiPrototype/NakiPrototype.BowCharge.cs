using UnityEngine;
namespace NakiLegend.Prototype {
 public sealed partial class NakiPrototype {
  private bool bowCharging,bowHeldPrevious,bowNeedsRelease,bowReadyShown;
  private int chargingWeapon=-1;
  private float bowChargeStarted,bowShotCharge,bowPausedTime,bowDashPauseAt=-1,bowDashPauseEnd;
  private void PauseBowChargeForDodge(){if(!bowCharging)return;if(bowDashPauseAt>=0)bowPausedTime+=Mathf.Clamp(clock-bowDashPauseAt,0,bowDashPauseEnd-bowDashPauseAt);bowDashPauseAt=clock;bowDashPauseEnd=dashUntil;}
  private GameObject bowChargeBar,bowChargeFill;
  private float BowChargeTime {get{return (CurrentWeapon.name.Contains("弩")?1.1f:.8f)/(profession==Profession.Archer?1.15f:1)*(HasTalent(3)?.75f:1);}}
  private float BowChargeFraction {get{return bowCharging?Mathf.Clamp01((clock-bowChargeStarted-bowPausedTime-(bowDashPauseAt>=0?Mathf.Clamp(clock-bowDashPauseAt,0,bowDashPauseEnd-bowDashPauseAt):0))/BowChargeTime):0;}}
  private void CancelBowCharge(){if(bowCharging||bowHeldPrevious)bowNeedsRelease=true;bowCharging=bowHeldPrevious=bowReadyShown=false;chargingWeapon=-1;bowPausedTime=0;bowDashPauseAt=-1;if(bowChargeBar!=null)Destroy(bowChargeBar);bowChargeBar=bowChargeFill=null;}
  private void TickBowInput(bool held){
   if(bowNeedsRelease){bowHeldPrevious=held;if(!held){bowNeedsRelease=bowHeldPrevious=false;}return;}
   if(weaponCatalog==null||carriedWeapons[equippedSlot]<0||CurrentWeapon.type!=WeaponType.Bow){CancelBowCharge();return;}
   bowHeldPrevious=held;
   if(clock<castPoseUntil||clock<pixelHurtUntil){CancelBowCharge();return;}
   if(bowCharging&&chargingWeapon!=carriedWeapons[equippedSlot]){CancelBowCharge();return;}
   if(clock<dashUntil){if(bowCharging)UpdateBowChargeBar();return;}
   if(held&&!bowCharging&&clock>=nextSword){bowCharging=true;bowChargeStarted=clock;bowPausedTime=0;bowDashPauseAt=-1;chargingWeapon=carriedWeapons[equippedSlot];bowReadyShown=false;}
   if(!bowCharging)return;
   if(!held){float strength=BowChargeFraction;CancelBowCharge();bowNeedsRelease=bowHeldPrevious=false;bowShotCharge=strength;try{UseEquippedWeapon();}finally{bowShotCharge=0;}return;}
   Enemy target;castDirection=AutoAim(CurrentWeapon.range,11,0,out target);UpdateBowChargeBar();
  }
  private void UpdateBowChargeBar(){float t=BowChargeFraction;if(bowChargeBar==null){bowChargeBar=Stretch("Bow charge background",square,position+Vector2.up*1.4f,new Vector2(.9f,.08f),C("1b2635"),650);bowChargeFill=Stretch("Bow charge power",square,position+Vector2.up*1.4f,new Vector2(.01f,.05f),C("e9c676"),651);bowChargeFill.transform.SetParent(bowChargeBar.transform,true);}
   bowChargeBar.transform.position=position+Vector2.up*1.4f;bowChargeFill.transform.position=position+new Vector2((t-1)*.43f,1.4f);bowChargeFill.transform.localScale=new Vector3(Mathf.Max(.01f,.86f*t)/.9f,.625f,1);bowChargeFill.GetComponent<SpriteRenderer>().color=t>=1?C("8cf4cf"):C("e9c676");
   if(t>=1&&!bowReadyShown){bowReadyShown=true;SpellSpark(position+castDirection*.6f,C("c8fff0"),.16f);}
  }
 }
}
