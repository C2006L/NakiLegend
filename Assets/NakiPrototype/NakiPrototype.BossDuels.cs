using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private sealed class DuelSalvo {public Enemy owner;public float next;public int left,index,profile;public Vector2 aim;}
  private readonly List<DuelSalvo> duelSalvos=new List<DuelSalvo>();
  private void BeginDuelMechanic(Enemy e,int profile)
  {
   if(e.kind==EnemyKind.ManorCaptain||profile==12)return;
   if(profile==8){Vector2 tideAim=(IntentLead(.2f)-e.position).normalized;duelSalvos.Add(new DuelSalvo{owner=e,profile=8,aim=tideAim,left=4,next=clock+1.05f});return;}if(profile==1)SpawnHeavyMother(e); // Approved reference encounter stays unchanged.
   Vector2 aim=(IntentLead(.3f)-e.position).normalized;if(aim.sqrMagnitude<.01f)aim=Vector2.down;
   Vector2 side=new Vector2(-aim.y,aim.x);
   // Each existing signature keeps its identity; this follow-up creates a readable combination.
   if(profile==1||profile==4||profile==7||profile==10||profile==11||profile==14||profile>=15){
    int beams=profile==7||profile==15?2:profile==16&&e.difficultyPhase>0?2:1;
    for(int i=0;i<beams;i++){
     Vector2 dir=Quaternion.Euler(0,0,(i-(beams-1)*.5f)*55)*aim;
     int before=signatureZones.Count;AddSignatureZone(e,8,e.position+dir*(e.radius+.3f),dir,11,3f);
     if(signatureZones.Count>before){var zone=signatureZones[signatureZones.Count-1];zone.gap=(profile==4||profile==7||profile==15?0:profile==11?-1:1)*14;zone.root.name="Boss sustained beam / "+profile;if(EarlyEncounter)earlyBeamOffsets[zone]=(i-(beams-1)*.5f)*55;}
    }
   }
   if(profile==2||profile==5||profile==8||profile==9||profile==12||profile==13){
    Vector2 at=NearestEnemyFooting(IntentLead(.35f)+side*(profile%2==0?1.6f:-1.6f),.5f);
    AddSignatureZone(e,profile==13?7:4,at,Vector2.zero,profile==8?1.15f:1,2.5f);
   }
   duelSalvos.Add(new DuelSalvo{owner=e,profile=profile,aim=aim,left=(profile==2?3:profile==0?5:6)+(profile!=2&&e.difficultyPhase>=2?1:0),next=clock+.9f});
  }
  private void TickDuelSalvos()
  {
   for(int i=duelSalvos.Count-1;i>=0;i--){var s=duelSalvos[i];Enemy e=s.owner;
    if(e.hp<=0||!enemies.Contains(e)){duelSalvos.RemoveAt(i);continue;}
    if(clock<s.next)continue;
    // Never store an off-screen shot to surprise the player later.
    if(!EnemyOnScreen(e.position,.25f)){duelSalvos.RemoveAt(i);continue;}
    Enemy previousEmitter=attackEmitter;attackEmitter=e;try{
    int before=shots.Count;Vector2 aim=s.aim;
    // Re-aim once between beats, rather than tracking every bullet in a fan.
    // Constant circular movement no longer escapes the entire committed sequence.
    if(s.index==2||s.index==4){Vector2 lead=IntentLead(.2f)-e.position;if(lead.sqrMagnitude>.01f)s.aim=aim=lead.normalized;}
    if(s.profile==0||s.profile==6||s.profile==11||s.profile==12){
     int count=(s.profile==0?14:18)+e.difficultyPhase*2;float gap=360f/count;
     for(int n=0;n<count;n++){if(n==count/2||n==count/2+1)continue;Vector2 dir=Quaternion.Euler(0,0,n*gap+s.index*gap*.5f)*aim;PressureFan(e.position+dir*(e.radius+.2f),dir,1,0,4.1f,e.attackElement,1.05f);}
    }else{
     aim=Quaternion.Euler(0,0,(s.index%2==0?-1:1)*(s.profile==3||s.profile==8?24:12))*aim;
     int count=s.profile==8?(e.difficultyPhase>=2?7:5):s.profile==2?5:s.profile==5?9:11;
     PressureFan(e.position+aim*(e.radius+.2f),aim,count,8,4.5f,e.attackElement,1.1f);
    }
    if(s.left==1&&s.profile!=8&&s.profile!=2&&s.profile!=0&&s.profile!=6&&s.profile!=11&&s.profile!=12){
     Vector2 flank=Quaternion.Euler(0,0,(s.index%2==0?1:-1)*65)*s.aim;
     PressureFan(e.position+flank*(e.radius+.2f),flank,5,10,4.0f,e.attackElement,1.05f);
    }
    if(s.index%2==1&&s.profile!=2){int precise=shots.Count;PressureFan(e.position+s.aim*(e.radius+.2f),(IntentLead(.2f)-e.position).normalized,EarlyEncounter&&CurrentIsland.ordinal>=3&&e.difficultyPhase>0?3:2,12,5.6f,e.attackElement,1.1f);
     if((s.profile==2||s.profile==5||s.profile==9||s.profile==13)&&s.index==3)for(int n=precise;n<shots.Count;n++)ArmBriefHoming(shots[n],.6f);
    }
    if(s.left==1&&s.profile==12)PressureFan(e.position+s.aim*(e.radius+.2f),s.aim,2,12,5.6f,e.attackElement,1.1f);
    for(int n=before;n<shots.Count;n++){var shot=shots[n];shot.life=4.3f;shot.root.GetComponent<SpriteRenderer>().color=SpellTint(e.attackElement);}
    s.index++;s.left--;s.next=clock+.58f;if(s.left<=0)duelSalvos.RemoveAt(i);
    }finally{attackEmitter=previousEmitter;}
   }
  }
  private bool DuelAttackFinishing(Enemy e)
  {
   foreach(var s in duelSalvos)if(s.owner==e)return true;
   foreach(var z in signatureZones)if(z.owner==e&&(z.mode==8||z.mode==5)&&clock<z.end)return true;
   return false;
  }
  private string ActiveDuelBeamTitle(Enemy e)
  {
   foreach(var z in signatureZones)if(z.owner==e&&z.mode==8&&clock<z.end){
    int id=e.kind==EnemyKind.ManorSeal?15:e.kind==EnemyKind.ManorKnight?16:BossIdentity(e);
    string name=id==1?"石像凝视":id==4?"双极连线":id==7?"折镜光束":id==10?"炉芯切割":id==11?"熔壳吐息":id==14?"根脉扫光":id==15?"王印交叉":"誓甲裂光";
    return name+(clock<z.active?" · 蓄势，绕开光路":" · 掩体可阻挡，绕侧近身");
   }
   return null;
  }
  private void TickDuelBeam(SignatureZone z)
  {
   Vector2 dir=PositionDuelBeam(z);float width=DuelBeamWidth(z),length=z.left.transform.localScale.x;
   Vector2 end=z.at+dir*length;BeamContactAudio(z,end,length<z.radius-.1f);float angle=Mathf.Atan2(dir.y,dir.x)*Mathf.Rad2Deg;
   var beam=z.left.GetComponent<SpriteRenderer>();beam.sprite=square;Color tint=z.royalAim?C("ff3b52"):SpellTint(z.owner.attackElement);float fade=Mathf.Clamp01((clock-z.active)/.16f)*Mathf.Clamp01((z.end-clock)/.2f);beam.color=new Color(tint.r,tint.g,tint.b,.9f*fade);
   z.left.transform.position=(z.at+end)*.5f;z.left.transform.rotation=Quaternion.Euler(0,0,angle);z.left.transform.localScale=new Vector3(length,width,1);
   beam.sortingOrder=620;
   if(z.right==null){z.right=Stretch("Beam bright core",square,(z.at+end)*.5f,new Vector2(length,z.royalAim?.14f:.09f),C("fff5ef"),621);z.right.transform.SetParent(z.root.transform,true);}
   z.right.transform.position=z.left.transform.position;z.right.transform.rotation=z.left.transform.rotation;z.right.transform.localScale=new Vector3(length,z.royalAim?.14f:.09f,1);
   if(z.beamEdge==null){z.beamEdge=Stretch("Beam contrast outline",square,z.at,Vector2.one,C("282032"),619);z.beamTip=Draw("Beam clipped contact",disc,end,Vector2.one*.22f,tint,622);z.beamMuzzle=Draw("Beam visible muzzle",windRingSprite??disc,z.at,Vector2.one*.46f,tint,622);foreach(GameObject part in new[]{z.beamEdge,z.beamTip,z.beamMuzzle})part.transform.SetParent(z.root.transform,true);}
   z.beamEdge.transform.position=z.left.transform.position;z.beamEdge.transform.rotation=z.left.transform.rotation;z.beamEdge.transform.localScale=new Vector3(length,width+.07f,1);
   z.beamTip.transform.position=end;z.beamMuzzle.transform.position=z.at;
   foreach(GameObject part in new[]{z.beamEdge,z.beamTip,z.beamMuzzle,z.right}){var sr=part.GetComponent<SpriteRenderer>();Color c=sr.color;c.a=length>.01f?fade:0;sr.color=c;}
   // Damage begins after the beam has visibly formed. Cover stops both the picture and hit path.
   if(length>.01f&&clock>=z.active+.16f&&clock>=z.nextHit&&DistanceToSegment(position,z.at,end)<width*.5f+.28f&&!ProjectilePathBlocked(z.at,position,.035f)){z.nextHit=clock+.8f;TakeJourneyDamage(ProfileBeamDamage(z.owner)/EnemyAttackScale,z.owner.attackElement);}
  }
  private bool CommittedWarriorStrike {get{return enableDesertJourney&&profession==Profession.Warrior&&(CurrentWeapon.type==WeaponType.Sword||CurrentWeapon.type==WeaponType.Spear)&&clock<swordUntil;}}
  private void CutMeleeProjectiles()
  {
   int cut=0;
   for(int i=shots.Count-1;i>=0&&cut<3;i--){Shot shot=shots[i];Vector2 delta=shot.position-position;
    if(!shot.hostile||!shot.dispellable||delta.magnitude>Mathf.Min(1.6f,CurrentWeapon.range+.1f)||Vector2.Dot(swingDirection,delta.normalized)<.55f||ProjectilePathBlocked(position,shot.position,.04f))continue;
    if(CurrentWeapon.type==WeaponType.Spear&&DistanceToSegment(shot.position,position,position+swingDirection*CurrentWeapon.range)>.32f)continue;
    SpellSpark(shot.position,C("e4e8e9"),.15f);if(shot.root!=null)Destroy(shot.root);shots.RemoveAt(i);cut++;
   }
  }
 }
}
