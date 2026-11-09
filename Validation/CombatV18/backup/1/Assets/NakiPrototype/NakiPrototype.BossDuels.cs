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
   if(e.kind==EnemyKind.ManorCaptain)return; // Approved reference encounter stays unchanged.
   Vector2 aim=(IntentLead(.3f)-e.position).normalized;if(aim.sqrMagnitude<.01f)aim=Vector2.down;
   Vector2 side=new Vector2(-aim.y,aim.x);
   // Each existing signature keeps its identity; this follow-up creates a readable combination.
   if(profile==1||profile==4||profile==7||profile==10||profile==11||profile==14||profile>=15){
    int beams=profile==7||profile==15?2:profile==16&&e.difficultyPhase>0?2:1;
    for(int i=0;i<beams;i++){
     Vector2 dir=Quaternion.Euler(0,0,(i-(beams-1)*.5f)*55)*aim;
     int before=signatureZones.Count;AddSignatureZone(e,8,e.position+dir*(e.radius+.3f),dir,11,3f);
     if(signatureZones.Count>before){var zone=signatureZones[signatureZones.Count-1];zone.gap=(profile==4||profile==7||profile==15?0:profile==11?-1:1)*22;zone.root.name="Boss sustained beam / "+profile;}
    }
   }
   if(profile==2||profile==5||profile==8||profile==9||profile==12||profile==13){
    Vector2 at=NearestEnemyFooting(IntentLead(.35f)+side*(profile%2==0?1.6f:-1.6f),.5f);
    AddSignatureZone(e,profile==13?7:4,at,Vector2.zero,profile==8?1.15f:1,2.5f);
   }
   duelSalvos.Add(new DuelSalvo{owner=e,profile=profile,aim=aim,left=(profile==0?3:4)+(e.difficultyPhase>=2?1:0),next=clock+.9f});
  }
  private void TickDuelSalvos()
  {
   for(int i=duelSalvos.Count-1;i>=0;i--){var s=duelSalvos[i];Enemy e=s.owner;
    if(e.hp<=0||!enemies.Contains(e)){duelSalvos.RemoveAt(i);continue;}
    if(clock<s.next)continue;
    // Never store an off-screen shot to surprise the player later.
    if(!EnemyOnScreen(e.position,.25f)){duelSalvos.RemoveAt(i);continue;}
    int before=shots.Count;Vector2 aim=s.aim;
    // Re-aim once between beats, rather than tracking every bullet in a fan.
    // Constant circular movement no longer escapes the entire committed sequence.
    if(s.index==2){Vector2 lead=IntentLead(.2f)-e.position;if(lead.sqrMagnitude>.01f)s.aim=aim=lead.normalized;}
    if(s.profile==0||s.profile==6||s.profile==11||s.profile==12){
     int count=(s.profile==0?14:18)+e.difficultyPhase*2;float gap=360f/count;
     for(int n=0;n<count;n++){if(n==count/2||n==count/2+1)continue;Vector2 dir=Quaternion.Euler(0,0,n*gap+s.index*gap*.5f)*aim;PressureFan(e.position+dir*(e.radius+.2f),dir,1,0,4.1f,e.attackElement,1.05f);}
    }else{
     aim=Quaternion.Euler(0,0,(s.index%2==0?-1:1)*(s.profile==3||s.profile==8?24:12))*aim;
     int count=s.profile==2?7:s.profile==5?9:11;
     PressureFan(e.position+aim*(e.radius+.2f),aim,count,8,4.5f,e.attackElement,1.1f);
    }
    if(s.left==1&&s.profile!=0&&s.profile!=6&&s.profile!=11&&s.profile!=12){
     Vector2 flank=Quaternion.Euler(0,0,(s.index%2==0?1:-1)*65)*s.aim;
     PressureFan(e.position+flank*(e.radius+.2f),flank,5,10,4.0f,e.attackElement,1.05f);
    }
    for(int n=before;n<shots.Count;n++){var shot=shots[n];shot.life=4.3f;shot.root.GetComponent<SpriteRenderer>().color=SpellTint(e.attackElement);}
    s.index++;s.left--;s.next=clock+.42f;if(s.left<=0)duelSalvos.RemoveAt(i);
   }
  }
  private bool DuelAttackFinishing(Enemy e)
  {
   foreach(var s in duelSalvos)if(s.owner==e)return true;
   foreach(var z in signatureZones)if(z.owner==e&&z.mode==8&&clock<z.end)return true;
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
   Vector2 dir=Quaternion.Euler(0,0,Mathf.Max(0,clock-z.active)*z.gap)*z.dir;
   ClipSignatureBeam(z,dir,.36f);float length=z.left.transform.localScale.x;
   Vector2 end=z.at+dir*length;float angle=Mathf.Atan2(dir.y,dir.x)*Mathf.Rad2Deg;
   var beam=z.left.GetComponent<SpriteRenderer>();beam.sprite=square;beam.color=SpellTint(z.owner.attackElement);
   z.left.transform.position=(z.at+end)*.5f;z.left.transform.rotation=Quaternion.Euler(0,0,angle);z.left.transform.localScale=new Vector3(length,.36f,1);
   beam.sortingOrder=620;
   if(z.right==null){z.right=Stretch("Beam bright core",square,(z.at+end)*.5f,new Vector2(length,.09f),C("fff9e8"),621);z.right.transform.SetParent(z.root.transform,true);}
   z.right.transform.position=z.left.transform.position;z.right.transform.rotation=z.left.transform.rotation;z.right.transform.localScale=new Vector3(length,.09f,1);
   // The emitter starts outside the body: moving in to the flank remains useful for melee.
   if(length>.01f&&clock>=z.nextHit&&DistanceToSegment(position,z.at,end)<.38f&&!ProjectilePathBlocked(z.at,position,.035f)){z.nextHit=clock+.7f;TakeJourneyDamage(1.25f,z.owner.attackElement);}
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
