using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private sealed class SignatureState {public float next,hold,shell;public int count;public Vector2 guardDirection;public float visibleUntil;public GameObject castFx;public SpriteRenderer[] glyphs;}
  private sealed class SignatureZone {public Enemy owner;public int mode;public Vector2 at,dir;public float born,active,end,nextHit,radius,gap;public GameObject root,left,right;public Rect foot,shot;public bool solid;public SpriteRenderer[] motifs;}
  private readonly Dictionary<Enemy,SignatureState> bossSignatures=new Dictionary<Enemy,SignatureState>();
  private readonly List<SignatureZone> signatureZones=new List<SignatureZone>();
  private sealed class ReturningBossShot {public Enemy owner;public Shot shot;public float turn;}
  private readonly List<ReturningBossShot> returningBossShots=new List<ReturningBossShot>();
  private Sprite signatureWaveSprite;
  private Sprite SignatureWaveSprite
  {
   get {if(signatureWaveSprite==null){var source=MakeSprite(new[]{"....ww......ww..","..wwIIww..wwIIww","wwIItIIIwwIItIII","IItTtttIIItTtttI","tTTttttttTTttttt","TTttttttTTtttttt","tttttttttttttttt","..tttt....tttt.."},new Dictionary<char,Color>{{'w',C("f1fcff")},{'I',C("a9edfa")},{'t',C("287fac")},{'T',C("45b9df")}},20);source.texture.wrapMode=TextureWrapMode.Repeat;signatureWaveSprite=Sprite.Create(source.texture,source.rect,new Vector2(.5f,.5f),20,0,SpriteMeshType.FullRect);ownedAssets.Add(signatureWaveSprite);}return signatureWaveSprite;}
  }
  private void DrawSignatureWave(GameObject obj,Vector2 at,float width)
  {var r=obj.GetComponent<SpriteRenderer>();r.sprite=SignatureWaveSprite;r.drawMode=SpriteDrawMode.Tiled;r.size=new Vector2(Mathf.Max(.01f,width),.55f);r.color=Color.white;obj.transform.localScale=Vector3.one;obj.transform.position=at+Vector2.up*Mathf.Sin(clock*9)*.025f;}
  private int BossIdentity(Enemy e)
  {
   if(woodlandMode)return ArrayIndexWood(e.kind)==9?12:ArrayIndexWood(e.kind)==10?13:14;
   switch(e.kind){case EnemyKind.WindScorpion:return 0;case EnemyKind.Colossus:return 1;case EnemyKind.ApeKing:return 2;case EnemyKind.CopperWarden:return 3;case EnemyKind.ThunderSentinel:return 4;case EnemyKind.StormStag:case EnemyKind.WoodlandDeer:return 5;case EnemyKind.IceBeast:return 6;case EnemyKind.FrostMirror:return 7;case EnemyKind.TideGuardian:return 8;case EnemyKind.RedSpineLizard:return 9;case EnemyKind.ForgeMaster:return 10;case EnemyKind.MagmaTortoise:return 11;default:return 1;}
  }
  private static readonly string[] SignatureNames={"噬雨涡心","石柱封路","回旋石刃","锁链牵引","双极雷场","逐雷追迹","晶脊裂阵","镜面夹击","开隙潮墙","熔火留痕","交错锻链","熔壳回缩","孵化巡猎","孢床蔓生","供养根须"};
  private bool TickBossSignature(Enemy e,float dt)
  {
   SignatureState state;if(!bossSignatures.TryGetValue(e,out state)){state=new SignatureState{next=clock+1.6f+BossIdentity(e)*.03f};bossSignatures[e]=state;}
   if(clock<state.hold){e.nextAttack=Mathf.Max(e.nextAttack,state.hold+.3f);UpdateJourneyEnemyPose(e,false);return true;}
   if(clock>=state.next&&EnemyOnScreen(e.position,.5f)&&e.windup<=0&&e.chargeUntil<=clock&&e.recoveryUntil<=clock&&e.phaseUntil<=clock){StartBossSignature(e,state);return true;}
   return false;
  }
  private void StartBossSignature(Enemy e,SignatureState state)
  {
   int id=BossIdentity(e);state.count++;state.next=clock+(e.difficultyPhase>=2?3.7f:4.2f);state.hold=clock+.8f;e.nextAttack=state.hold+.6f;
   Vector2 aim=(IntentLead(.35f)-e.position).normalized;if(aim.sqrMagnitude<.01f)aim=Vector2.down;Vector2 side=new Vector2(-aim.y,aim.x);
   state.visibleUntil=clock+new float[]{4.85f,5.35f,3.6f,2.05f,4.65f,1.8f,4.65f,1.6f,6.85f,4.65f,2.35f,4.05f,4.35f,4.85f,1.7f}[id];BeginBossCastVisual(e,state);
   e.actionStarted=clock;e.attackDirection=aim;AddFloatingHit(SignatureNames[id],e.position+Vector2.up*1.6f,SpellTint(e.attackElement));
   switch(id){
    case 0:AddSignatureZone(e,0,NearestEnemyFooting(IntentLead(.4f),.6f),Vector2.zero,1.6f,4);break;
    case 1:foreach(float offset in new[]{-2f,2f})AddSignatureZone(e,1,NearestEnemyFooting(IntentLead(.3f)+side*offset,.6f),Vector2.zero,.55f,4.5f);break;
    case 2:{int before=shots.Count;PressureFan(e.position+aim*(e.radius+.2f),aim,3,25,5.5f,e.attackElement,1.25f);for(int i=before;i<shots.Count;i++){shots[i].life=3.6f;var stone=shots[i].root.GetComponent<SpriteRenderer>();stone.sprite=SignatureGlyph(5);stone.color=C("e2b87e");shots[i].root.transform.localScale=new Vector3(.55f,.32f,1);RemoveRoundBulletDecorations(shots[i]);returningBossShots.Add(new ReturningBossShot{owner=e,shot=shots[i],turn=clock+.85f});}break;}
    case 3:AddSignatureZone(e,2,e.position,aim,5.5f,1.2f);break;
    case 4:foreach(int sign in new[]{-1,1})AddSignatureZone(e,3,NearestEnemyFooting(e.position+side*sign*2.3f,.6f),Vector2.zero,1.15f,3.8f);break;
    case 5:for(int i=0;i<3;i++)AddChapterStrike(SafeChapterLightning(IntentLead(.4f)+side*(i-1)*1.8f),.85f+i*.42f);break;
    case 6:AddSignatureZone(e,1,NearestEnemyFooting(e.position+side*2.2f,.6f),Vector2.zero,.55f,3.8f);AddSignatureZone(e,4,NearestEnemyFooting(IntentLead(.35f),.6f),Vector2.zero,1.05f,2.7f);break;
    case 7:{bool found=false;foreach(Enemy lens in enemies)if(lens.kind==EnemyKind.FrostLens&&lens.hp>0&&lens.iceOwner==e){Vector2 ray=(IntentLead(.2f)-lens.position).normalized;PressureFan(lens.position+ray*.6f,ray,3,12,4.5f,Element.Ice,1.2f);found=true;}if(!found)SpawnFrostMirrors(e);break;}
    case 8:AddSignatureZone(e,5,new Vector2(0,-journeyRooms[currentRoom].halfHeight+.55f),Vector2.up,1,6);break;
    case 9:for(int i=0;i<3;i++)AddSignatureZone(e,4,NearestEnemyFooting(IntentLead(.25f)-aim*i*1.45f,.6f),Vector2.zero,.9f,3.5f+i*.15f);break;
    case 10:foreach(float offset in new[]{-1.5f,1.5f})AddSignatureZone(e,2,NearestEnemyFooting(e.position+side*offset,.4f),aim,6,1.5f);break;
    case 11:state.shell=clock+4.05f;state.guardDirection=aim;AddSignatureZone(e,6,e.position,aim,2,3.2f);break;
    case 12:if(ActiveCombatants<EncounterEnemyLimit-1){SummonJourneyAdds(WoodKinds[1],1);SummonJourneyAdds(WoodKinds[3],1);}AddSignatureZone(e,4,NearestEnemyFooting(e.position+side*2,.6f),Vector2.zero,1.1f,3.5f);break;
    case 13:foreach(float offset in new[]{-2f,0,2f})AddSignatureZone(e,7,NearestEnemyFooting(IntentLead(.3f)+side*offset,.6f),Vector2.zero,.8f,4);break;
    case 14:QueueForestStrike(e,NearestEnemyFooting(IntentLead(.3f)-side*2,.5f),aim,true,1.1f);QueueForestStrike(e,NearestEnemyFooting(IntentLead(.3f)+side*2,.5f),aim,true,1.1f);if(WoodKnotsAlive(e)>0){e.tacticWardUntil=clock+3;SpellSpark(e.position,C("a5db9a"),2);}break;
   }
   BeginDuelMechanic(e,id);
  }
  private void AddSignatureZone(Enemy owner,int mode,Vector2 at,Vector2 dir,float radius,float duration)
  {
   if(signatureZones.Count>=8)return;var root=new GameObject("Boss signature "+SignatureNames[BossIdentity(owner)]);root.transform.SetParent(world,false);
   var zone=new SignatureZone{owner=owner,mode=mode,at=at,dir=dir,radius=radius,born=clock,active=clock+.85f,end=clock+.85f+duration,nextHit=clock+.85f,root=root,gap=Mathf.Clamp(position.x,-journeyRooms[currentRoom].halfWidth+1.8f,journeyRooms[currentRoom].halfWidth-1.8f)};
   GameObject tell=mode==2||mode==8?ChapterLineWarning(at,dir,radius,.65f):mode==5?Stretch("Tide entering arena",square,at,new Vector2(journeyRooms[currentRoom].halfWidth*2,.12f),C("99e5f4"),-630):WarningCircle(at,radius);
   tell.transform.SetParent(root.transform,true);zone.left=tell;
   if(mode==5){zone.right=Stretch("Tide second bank",square,at,Vector2.one,C("71cfe0"),-628);zone.right.transform.SetParent(root.transform,true);}
   if(mode==6)for(int i=-2;i<=2;i++){Vector2 radial=Quaternion.Euler(0,0,i*30)*dir;var plate=Stretch("Frontal shell plate",square,at+radial*1.3f,new Vector2(.08f,.5f),C("ffcf89"),DepthAt(at.y)+6);plate.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(radial.y,radial.x)*Mathf.Rad2Deg);plate.transform.SetParent(root.transform,true);}
   BuildSignatureVisuals(zone);signatureZones.Add(zone);
  }
  private void TickBossSignatures(float dt)
  {
   TickBossCastVisuals();TickDuelSalvos();
   for(int i=returningBossShots.Count-1;i>=0;i--){var b=returningBossShots[i];if(b.owner.hp<=0||!enemies.Contains(b.owner)||!shots.Contains(b.shot)){if(shots.Contains(b.shot)){if(b.shot.root!=null)Destroy(b.shot.root);shots.Remove(b.shot);}returningBossShots.RemoveAt(i);continue;}b.shot.root.transform.Rotate(0,0,dt*420);if(clock>=b.turn){Vector2 toward=b.owner.position-b.shot.position;b.shot.direction=Vector2.Lerp(b.shot.direction,toward.normalized,Mathf.Clamp01(dt*6)).normalized;if(toward.magnitude<.5f)b.shot.life=0;}}
   for(int i=signatureZones.Count-1;i>=0;i--){var z=signatureZones[i];if(z.owner.hp<=0||!enemies.Contains(z.owner)||clock>=z.end){RemoveSignatureZone(z);signatureZones.RemoveAt(i);continue;}UpdateSignatureVisuals(z);if(z.mode==2||z.mode==8)ClipSignatureBeam(z,z.dir,.65f);if(clock<z.active)continue;
    Color tint=SpellTint(z.owner.attackElement);bool ready=clock>=z.nextHit;
    if(z.mode==1&&!z.solid){
     // Never erect a solid column under the player or another actor.
     bool occupied=Vector2.Distance(position,z.at)<1;foreach(Enemy e in enemies)if(Vector2.Distance(e.position,z.at)<e.radius+.75f)occupied=true;
     if(occupied){z.end=clock;continue;}Destroy(z.left);Sprite art=iceSampleMode?IceSprite(4):pillarArtwork;z.left=Draw("Temporary signature pillar",art??disc,z.at,Vector2.one*.7f,Color.white,DepthAt(z.at.y));z.left.transform.SetParent(z.root.transform,true);
     z.foot=new Rect(z.at.x-.43f,z.at.y-.15f,.86f,.5f);z.shot=new Rect(z.at.x-.5f,z.at.y-.15f,1,1.4f);obstacles.Add(z.foot);projectileObstacles.Add(z.shot);z.solid=true;
    }else if(z.mode==5){
     float y=z.at.y+(clock-z.active)*1.8f,half=journeyRooms[currentRoom].halfWidth,gap=2.8f;
     float leftWidth=Mathf.Max(0,z.gap-gap/2+half),rightWidth=Mathf.Max(0,half-z.gap-gap/2);
     DrawSignatureWave(z.left,new Vector2(-half+leftWidth*.5f,y),leftWidth);
     DrawSignatureWave(z.right,new Vector2(half-rightWidth*.5f,y),rightWidth);
     if(ready&&Mathf.Abs(position.y-y)<.43f&&Mathf.Abs(position.x-z.gap)>gap*.5f){z.nextHit=clock+.8f;TakeJourneyDamage(1.6f,Element.Ice);}
    }else if(z.mode==8){TickDuelBeam(z);}
    else if(z.mode==2){z.left.GetComponent<SpriteRenderer>().color=new Color(tint.r,tint.g,tint.b,.14f);if(ready&&z.left.transform.localScale.x>.01f&&DistanceToSegment(position,z.at,z.at+z.dir*z.left.transform.localScale.x)<.52f&&!ProjectilePathBlocked(z.at,position,.04f)){z.nextHit=clock+.75f;TakeJourneyDamage(1.5f,z.owner.attackElement);position=Move(position,-z.dir*.35f,.3f);}}
    else if(z.mode!=1){
     if(z.mode==6){Vector2 travel=z.owner.position-z.at;z.at=z.owner.position;foreach(Transform child in z.root.transform)child.position+=(Vector3)travel;}
     float radius=z.mode==7?Mathf.Lerp(z.radius,1.9f,Mathf.Clamp01((clock-z.active)/3)):z.radius;
     z.left.transform.localScale=Vector3.one*radius*2;z.left.GetComponent<SpriteRenderer>().color=new Color(tint.r,tint.g,tint.b,z.mode==6?.12f:.5f);
     if(ready){z.nextHit=clock+(z.mode==0||z.mode==3||z.mode==6?1:.75f);
      if(z.mode==0||z.mode==3||z.mode==6){int count=z.mode==3?4:8;Vector2 aim=Quaternion.Euler(0,0,(clock-z.active)*40)*Vector2.right;PressureFan(z.at,aim,count,360f/count,3.7f,z.owner.attackElement,1.2f);}
      if(z.mode!=6&&Vector2.Distance(position,z.at)<radius&&!ProjectilePathBlocked(z.at,position,.04f))TakeJourneyDamage(z.mode==0?1:1.25f,z.owner.attackElement);
     }
    }
   }
  }
  private float SignatureDamageMultiplier(Enemy e)
  {SignatureState state;return bossSignatures.TryGetValue(e,out state)&&clock<state.shell&&Vector2.Dot((position-e.position).normalized,state.guardDirection)>.25f?.55f:1;}
  private void RemoveSignatureZone(SignatureZone z){if(z.solid){obstacles.Remove(z.foot);projectileObstacles.Remove(z.shot);}if(z.root!=null)Destroy(z.root);}
  private void CancelBossSignature(Enemy owner)
  {duelSalvos.RemoveAll(s=>s.owner==owner);for(int i=signatureZones.Count-1;i>=0;i--)if(signatureZones[i].owner==owner){RemoveSignatureZone(signatureZones[i]);signatureZones.RemoveAt(i);}for(int i=returningBossShots.Count-1;i>=0;i--)if(returningBossShots[i].owner==owner){returningBossShots[i].shot.life=0;returningBossShots.RemoveAt(i);}SignatureState state;if(bossSignatures.TryGetValue(owner,out state)){if(state.castFx!=null)Destroy(state.castFx);state.visibleUntil=state.hold=state.shell=0;state.next=Mathf.Max(state.next,clock+2);}}
  private void ClearBossSignatures(){duelSalvos.Clear();foreach(var z in signatureZones)RemoveSignatureZone(z);signatureZones.Clear();foreach(var state in bossSignatures.Values)if(state.castFx!=null)Destroy(state.castFx);bossSignatures.Clear();foreach(var b in returningBossShots)if(b.shot.root!=null)b.shot.life=0;returningBossShots.Clear();}
 }
}
