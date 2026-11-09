using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private readonly Dictionary<int,Sprite> forestQuestArt=new Dictionary<int,Sprite>();
  private sealed class ForestStrike {public Enemy owner;public Vector2 at,aim;public float when,length;public bool root;public GameObject warning;}
  private readonly List<ForestStrike> forestStrikes=new List<ForestStrike>();
  private Sprite ForestQuestSprite(int id)
  {
   Sprite s;if(forestQuestArt.TryGetValue(id,out s)&&s!=null)return s;
   var t=Resources.Load<Texture2D>("NakiArt/WoodlandV3/quest-props");if(t==null)throw new System.InvalidOperationException("Missing forest quest props");t.filterMode=FilterMode.Point;
   Rect cell=new Rect(id%3*t.width/3f,(1-id/3)*t.height/2f,t.width/3f,t.height/2f);
   // Cell padding is retained to avoid filtering neighbouring sprites. Visible foot sits at 12%.
   s=Sprite.Create(t,cell,new Vector2(.5f,.12f),cell.width/(id==2?2.45f:id==4?1.9f:2.05f),0,SpriteMeshType.FullRect);
   s.name="forest-quest-"+id;ownedAssets.Add(s);forestQuestArt[id]=s;return s;
  }
  private void ForestQuestProp(int id,Vector2 at,string label)
  {Draw(label,ForestQuestSprite(id),at,Vector2.one,Color.white,DepthAt(at.y)).transform.SetParent(chapterFocus.transform,true);}
  private void ForestQuestAction(int kind,int art,Vector2 at,string label)
  {ForestQuestProp(art,at,label);AddChapterAction(kind,at+Vector2.down*.55f,label);}
  private void BuildForestAftermath(RoomNode n)
  {
   if(n.id==31&&chapterStone){
    // Dialogue pauses the game clock: retire the defeated pose before staging its living counterpart.
    for(int i=fadingBodies.Count-1;i>=0;i--){var body=fadingBodies[i];if(body.owner==null||body.owner.kind!=WoodKinds[11]||!body.owner.boss)continue;if(body.root!=null){body.root.SetActive(false);Destroy(body.root);}fadingBodies.RemoveAt(i);}
    Draw("Resting ancient guardian",enemyActionArt[WoodKinds[11]][0],new Vector2(0,2.1f),Vector2.one*.85f,new Color(.76f,.87f,.72f),DepthAt(2.1f)).transform.SetParent(chapterFocus.transform,true);
    if(chapterLampGiven)ForestQuestProp(1,new Vector2(-3,2.2f),"First shoots beneath the resting guardian");
   }
   if(n.id==0&&chapterLampGiven&&chapterRanger){
    Draw("Collector returned safely to A Ling",ResidentSprite(6),new Vector2(-5.7f,1.6f),Vector2.one,Color.white,DepthAt(1.6f)).transform.SetParent(chapterFocus.transform,true);
    // Restored seedlings belong to the old seedbed, not the harbor water.
   }
  }
  private bool ForestBossTell(Enemy e,int k)
  {
   if(!e.boss)return false;int move=e.attackNumber%3;
   if(k==9&&move==2){e.warning=WoodFanWarning(e,e.attackDirection,e.secondPhase?15:13,300);return true;}
   if(k==10&&move==1){e.warning=WarningCircle(e.strikePoint,2.5f);return true;}
   if(k==11&&move==1){e.warning=ChapterLineWarning(e.position,e.attackDirection,7,.55f);return true;}
   return false;
  }
  private bool ResolveForestBossMove(Enemy e,int k)
  {
   if(!e.boss)return false;int move=e.attackNumber%3;
   if(k==9&&move==2){WoodFan(e,e.attackDirection,e.secondPhase?15:13,300,3.8f);e.recoveryUntil=clock+1.6f;return true;}
   if(k==10&&move==1){
    foreach(Vector2 off in new[]{new Vector2(-2,0),new Vector2(2,0),new Vector2(0,1.8f)})QueueForestStrike(e,e.strikePoint+off,Vector2.zero,false,1.1f+forestStrikes.Count*.15f);
    e.recoveryUntil=clock+1.8f;return true;
   }
   if(k==11&&move==1){
    Vector2 side=new Vector2(-e.attackDirection.y,e.attackDirection.x);
    foreach(float offset in new[]{-1.85f,0,1.85f})QueueForestStrike(e,NearestEnemyFooting(IntentLead(.25f)-e.attackDirection*2+side*offset,.3f),e.attackDirection,true,1.2f);
    e.recoveryUntil=clock+2.1f;return true;
   }
   return false;
  }
  private void QueueForestStrike(Enemy owner,Vector2 at,Vector2 aim,bool root,float delay)
  {
   if(!woodlandMode||owner==null||owner.hp<=0||forestStrikes.Count>=6)return;
   if(!root){
    Vector2 desired=at;bool found=false;
    for(int ring=0;ring<=4&&!found;ring++)for(int j=0;j<(ring==0?1:8)&&!found;j++){
     Vector2 point=NearestEnemyFooting(desired+(Vector2)(Quaternion.Euler(0,0,j*45)*Vector2.right)*(ring*.65f),.6f);
     if(IntersectsTransit(point,Vector2.one*2.3f)||Blocked(point,.6f))continue;
     bool overlap=false;foreach(var pending in forestStrikes)if(!pending.root&&Vector2.Distance(pending.at,point)<1.3f){overlap=true;break;}if(overlap)continue;at=point;found=true;
    }
    if(!found)return;
   }
   else if(Blocked(at,.3f))return;
   var tell=root?ChapterLineWarning(at,aim,7,1.1f):WarningCircle(at,1.05f);tell.name=root?"Ancient root lane warning":"Fungal pod warning";
   forestStrikes.Add(new ForestStrike{owner=owner,at=at,aim=aim,root=root,length=7,when=clock+Mathf.Max(1.1f,delay),warning=tell});
  }
  private void TickForestStrikes()
  {
   TickAncientRootAttacks();
   for(int i=forestStrikes.Count-1;i>=0;i--){var s=forestStrikes[i];
    if(s.owner.hp<=0||!enemies.Contains(s.owner)){if(s.warning!=null)Destroy(s.warning);forestStrikes.RemoveAt(i);continue;}
    if(clock<s.when)continue;if(s.warning!=null)Destroy(s.warning);forestStrikes.RemoveAt(i);
    if(s.root){
     BeginAncientRootAttack(s);
    }else{
     var visual=Draw("Spore pod bursts",ForestQuestSprite(5),s.at,Vector2.one*.65f,Color.white,DepthAt(s.at.y));fadingBodies.Add(new FadingBody{root=visual,renderer=visual.GetComponent<SpriteRenderer>(),expires=clock+.6f});
     // Each pod leaves an outward escape sector; all three origins were locked during preparation.
     Vector2 aim=(s.owner.position-s.at).normalized;if(aim.sqrMagnitude<.01f)aim=Vector2.up;
     int count=s.owner.secondPhase?12:9;for(int j=0;j<count;j++){Vector2 d=Quaternion.Euler(0,0,(j/(float)(count-1)-.5f)*260)*aim;int before=shots.Count;SpawnShot(s.at+d*.35f,d,true,null,s.at);if(shots.Count==before)continue;Shot shot=shots[shots.Count-1];shot.emitter=s.owner;shot.element=Element.Wood;shot.damage=1.3f;shot.speed=3.8f;shot.life=3.5f;shot.root.GetComponent<SpriteRenderer>().color=C("e5ba71");}
     SkillSound("spore","hit");
    }
   }
  }
  private void CancelForestStrikes(Enemy owner=null)
  {
   for(int i=forestStrikes.Count-1;i>=0;i--)if(owner==null||forestStrikes[i].owner==owner){if(forestStrikes[i].warning!=null)Destroy(forestStrikes[i].warning);forestStrikes.RemoveAt(i);}
   ClearAncientRootAttacks(owner);if(owner!=null)CancelBossSignature(owner);
  }
  private void CastForestRoots(Enemy first,Vector2 aim)
  {
   var targets=new List<Enemy>{first};
   foreach(Enemy e in enemies){Vector2 delta=e.position-position;float along=Vector2.Dot(delta,aim);
    if(e!=first&&e.hp>0&&along>0&&along<9&&Mathf.Abs(delta.x*aim.y-delta.y*aim.x)<.7f&&!ProjectilePathBlocked(position,e.position,.04f))targets.Add(e);}
   targets.Sort((a,b)=>(a.position-position).sqrMagnitude.CompareTo((b.position-position).sqrMagnitude));
   int hits=0;foreach(Enemy e in targets){if(hits++>=3)break;Vector2 point=e.position;ApplySkillHit(e,SkillDamage(e==first?24:16),Element.Wood,"root");
    if(e.hp>0){e.recoveryUntil=Mathf.Max(e.recoveryUntil,clock+.6f);if(!e.boss){e.chargeUntil=0;e.windup=0;if(e.warning!=null)Destroy(e.warning);e.nextAttack=Mathf.Max(e.nextAttack,clock+.85f);}}
    var visual=Draw("Friendly root snare",WoodlandNature(2),point,Vector2.one*.33f,C("bce7a4"),DepthAt(point.y)+1);fadingBodies.Add(new FadingBody{root=visual,renderer=visual.GetComponent<SpriteRenderer>(),expires=clock+.55f});
   }
  }
 }
}
