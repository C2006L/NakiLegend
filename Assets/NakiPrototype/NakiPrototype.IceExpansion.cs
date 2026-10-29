using System;
using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private readonly Dictionary<int,Sprite> iceExpansionArt=new Dictionary<int,Sprite>();
  private readonly List<Rect> iceFissures=new List<Rect>();
  private sealed class IcePatch {public Vector2 at;public float expires,nextHit;public GameObject root;}
  private readonly List<IcePatch> icePatches=new List<IcePatch>();
  private static bool IsExpandedIceEnemy(EnemyKind kind){return kind>=EnemyKind.FrostHound&&kind<=EnemyKind.IceRunecaster;}
  private Sprite IceExpansionSprite(int index)
  {
   Sprite sprite;if(iceExpansionArt.TryGetValue(index,out sprite)&&sprite!=null)return sprite;
   Texture2D texture=Resources.Load<Texture2D>("NakiArt/IceSample/expansion-atlas");if(texture==null)throw new InvalidOperationException("Missing ice expansion atlas");texture.filterMode=FilterMode.Point;
   Rect[] regions={new Rect(45,150,410,280),new Rect(535,105,320,325),new Rect(950,60,335,375),new Rect(1395,115,315,320),new Rect(95,530,250,295),new Rect(495,445,310,425),new Rect(850,620,490,170),new Rect(1370,555,380,265)};
   Rect source=regions[index];Rect cell=new Rect(source.x*texture.width/1774f,texture.height-(source.y+source.height)*texture.height/887f,source.width*texture.width/1774f,source.height*texture.height/887f);
   float height=index==0?1.2f:index==1||index==2?1.6f:index==5?2:index==6?.7f:index==7?1:1.1f;
   sprite=Sprite.Create(texture,cell,index==6?Vector2.one*.5f:new Vector2(.5f,.15f),cell.height/height,0,SpriteMeshType.FullRect);ownedAssets.Add(sprite);iceExpansionArt[index]=sprite;return sprite;
  }
  private void LoadIceExpansionArt()
  {
   for(int i=0;i<3;i++){var frames=new Sprite[16];for(int k=0;k<frames.Length;k++)frames[k]=IceExpansionSprite(i);enemyActionArt[(EnemyKind)((int)EnemyKind.FrostHound+i)]=frames;}
  }
  private void AddIceBranches()
  {
   chapterNames=new[]{"冰港码头","冰封回廊","裂冰甲兽 · 晶甲庭","覆雪货仓","霜镜瞭台"};
   foreach(int id in new[]{3,4}){var n=new RoomNode{id=id,stage=0,floor=0,cell=new Vector2Int(1,id==3?1:-1),halfWidth=id==3?8.5f:8,halfHeight=4.8f,kind=RoomKind.Combat,waves=3,stairsTo=-1,depth=8,layout=id};n.links.Add(1);journeyRooms.Add(n);journeyRooms[1].links.Add(id);}
   journeyRooms[1].waves=4;
  }
  private void MigrateIceSampleCheckpoint(ChapterCheckpoint saved)
  {
   if(!iceSampleMode||saved==null||saved.island!="ice_sample"||saved.map!="ice-s1-v1"||saved.rooms==null||saved.rooms.Length!=3)return;
   var expanded=new ChapterRoomState[5];Array.Copy(saved.rooms,expanded,3);
   for(int i=3;i<5;i++)expanded[i]=new ChapterRoomState{discovered=saved.rooms[1]!=null&&saved.rooms[1].visited,weapons=Array.Empty<CheckpointWeapon>(),crystals=Array.Empty<CheckpointEssence>()};
   saved.rooms=expanded;saved.map="ice-s2-v1";
  }
  private void BuildIceExpansionCover(RoomNode room)
  {
   iceFissures.Clear();
   Vector2[][] positions={
    new[]{new Vector2(-5.8f,2.7f),new Vector2(5.6f,2.6f),new Vector2(5.2f,-1),new Vector2(-5,-2.7f)},
    new[]{new Vector2(-5,2.7f),new Vector2(5,-2.6f),new Vector2(-3,-1.5f),new Vector2(3,1.8f),new Vector2(0,1.2f)},
    new[]{new Vector2(-6,3),new Vector2(6,3),new Vector2(-5,-3),new Vector2(5,-3)},
    new[]{new Vector2(-5,2.4f),new Vector2(-3,2.4f),new Vector2(3,-1.9f),new Vector2(5,1.9f),new Vector2(3,1.8f),new Vector2(-4,-2)},
    new[]{new Vector2(-4.8f,2.3f),new Vector2(4.8f,2.3f),new Vector2(-3,-1),new Vector2(3,-1),new Vector2(0,1.1f)}};
   int[][] types={new[]{3,3,4,7},new[]{5,5,3,4,6},new[]{5,5,7,7},new[]{3,3,3,4,4,7},new[]{5,5,4,4,6}};
   for(int i=0;i<positions[room.id].Length;i++)AddIceObstacle(types[room.id][i],positions[room.id][i]);
  }
  private void AddIceObstacle(int type,Vector2 at)
  {
   Vector2 feet=type==6?new Vector2(2.5f,.5f):type==7?new Vector2(1.7f,.45f):type==5?new Vector2(.65f,.5f):new Vector2(1,.5f);
   Vector2 reservation=feet+new Vector2(.5f,.65f);if(IntersectsTransit(at,reservation))return;
   Rect block=new Rect(at-new Vector2(feet.x/2,type==6?feet.y/2:.1f),feet);
   foreach(Rect existing in obstacles)if(existing.Overlaps(block))return;
   string[] names={"","","","Snow cargo crate","Frozen supply barrel","Tall ice spire","Open ice fissure","Driftwood barricade"};
   Sprite art=IceExpansionSprite(type);Vector2 scale=Vector2.one;if(type==6)scale.x=3/art.bounds.size.x;if(type==7)scale.x=2/art.bounds.size.x;
   Draw(names[type],art,at,scale,Color.white,type==6?-990:DepthAt(at.y));obstacles.Add(block);
   if(type==6)iceFissures.Add(block);else projectileObstacles.Add(new Rect(at+Vector2.Scale(art.bounds.min,scale),Vector2.Scale(art.bounds.size,scale)));
  }
  private void SpawnExpandedIceWave(RoomNode room)
  {
   EnemyKind g=EnemyKind.IceGuard,w=EnemyKind.IceWisp,h=EnemyKind.FrostHound,a=EnemyKind.IceArcher,c=EnemyKind.IceRunecaster;
   EnemyKind[][] corridor={new[]{g,g},new[]{g,w,g},new[]{h,a,g},new[]{a,h,w}};
   EnemyKind[][] cargo={new[]{h,a},new[]{h,g,a},new[]{a,h,h}};
   EnemyKind[][] lookout={new[]{w,c},new[]{g,c,w},new[]{c,a,g}};
   EnemyKind[] wave=(room.id==3?cargo:room.id==4?lookout:corridor)[currentWave-1];var random=new System.Random(activeSeed+room.id*271+currentWave*113);
   for(int i=0;i<wave.Length;i++){QueueEnemy(wave[i],FindJourneySpawn(random,.45f));pendingSpawns[pendingSpawns.Count-1].when=clock+1.2f+i*.32f;}
  }
  private void IceBranchCleared(RoomNode room)
  {
   if(room.id<3||room.lootSeeded)return;room.lootSeeded=true;
   var essence=new StoredEssence{at=FindFreeInteractionPoint(new Vector2(2,.5f)),value=25,born=clock};room.essence.Add(essence);DrawEssence(essence);
   ChapterDrop(room.id==3?12:11,new Vector2(-2,.5f));
   TellStory(room.id==3?"货仓的绳索上还结着一层薄霜，封好的箭袋却没有被潮水浸湿。少年收好还能用的装备。":"瞭台的晶纹暗了下来。望向湾口，守塔人的灯仍穿过风雪，落在归航的方向。");
  }
  private void ConfigureExpandedIceEnemy(Enemy enemy)
  {
   bool hound=enemy.kind==EnemyKind.FrostHound,archer=enemy.kind==EnemyKind.IceArcher;
   enemy.hp=enemy.maximum=hound?56:archer?64:96;enemy.radius=hound?.28f:archer?.3f:.36f;enemy.speed=hound?2.1f:archer?1.3f:.85f;enemy.iceArmor=hound||archer?0:1;enemy.soulEligible=hound||archer;
  }
  private void UpdateExpandedIceEnemy(Enemy e,float dt)
  {
   bool hound=e.kind==EnemyKind.FrostHound,archer=e.kind==EnemyKind.IceArcher;Vector2 old=e.position,to=position-e.position;float slow=clock<e.iceSlowUntil?.45f:1;
   if(e.chargeUntil>0){if(clock<e.chargeUntil){e.position=MoveEnemyWithinRoom(e.position,e.attackDirection*7*slow*dt,e.radius);if((e.position-old).sqrMagnitude<.00001f)e.chargeUntil=clock;if(DistanceToSegment(position,old,e.position)<.55f&&!ProjectilePathBlocked(e.position,position,.03f))TakeJourneyDamage(1,Element.Ice);}else{e.chargeUntil=0;e.recoveryUntil=clock+.65f;}}
   else if(e.windup>0){e.windup-=dt;if(e.windup<=0){if(e.warning!=null)Destroy(e.warning);ResolveExpandedIceMove(e);}}
   else if(clock>=e.recoveryUntil)
   {
    bool visible=!ProjectilePathBlocked(e.position,position,.07f);float range=hound?3.5f:8;
    if(clock>=e.nextAttack&&to.magnitude<range&&visible){e.attackDirection=to.sqrMagnitude>.001f?to.normalized:Vector2.down;e.strikePoint=position;e.windup=hound?.65f:archer?.85f:1.05f;e.nextAttack=clock+e.windup+(hound?1.5f:archer?2:3);e.actionStarted=clock;e.attackNumber++;e.warning=hound||archer?ChapterLineWarning(e.position,e.attackDirection,hound?3.5f:8,hound?.9f:.35f):WarningCircle(e.strikePoint,.95f);}
    else if(!visible||to.magnitude>(hound?1.1f:archer?4.5f:5))e.position=MoveEnemyWithinRoom(e.position,to.normalized*e.speed*slow*dt,e.radius);
    else if(!hound&&to.magnitude<2)e.position=MoveEnemyWithinRoom(e.position,-to.normalized*e.speed*slow*dt,e.radius);
   }
   e.velocity=dt>0?(e.position-old)/dt:Vector2.zero;Vector2 look=e.windup>0||e.chargeUntil>0?e.attackDirection:to;
   e.root.transform.position=e.position+Vector2.up*(e.velocity.sqrMagnitude>.01f?.03f*Mathf.Sin(clock*(hound?16:11)):0);e.body.flipX=look.x<0;e.body.sortingOrder=DepthAt(e.position.y);e.body.color=clock<e.hurtUntil?C("ffe5dd"):e.iceArmor>0?C("bddfff"):Color.white;if(e.shadow!=null)e.shadow.transform.position=e.position;
  }
  private void ResolveExpandedIceMove(Enemy e)
  {
   e.recoveryUntil=clock+.6f;if(e.kind==EnemyKind.FrostHound){e.chargeUntil=clock+.4f;return;}
   if(e.kind==EnemyKind.IceArcher){for(int i=0;i<2;i++){Vector2 direction=Quaternion.Euler(0,0,(i==0?-4:4))*e.attackDirection;int count=shots.Count;SpawnShot(e.position+direction*.45f,direction,true,null,e.position);if(shots.Count>count){Shot shot=shots[shots.Count-1];shot.element=Element.Ice;shot.damage=1;shot.speed=7;shot.life=1.6f;var renderer=shot.root.GetComponent<SpriteRenderer>();renderer.sprite=arrowSprite;renderer.color=C("c7e8ee");shot.root.transform.localScale=new Vector3(.65f,.45f,1);shot.root.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg);}}Sfx("bow",false,.25f);return;}
   SkillSound("ice_ring","hit");SpellSpark(e.strikePoint,C("c4efff"),1.2f);
   GameObject root=Draw("Dangerous frost rune",windRingSprite??disc,e.strikePoint,Vector2.one*1.9f,C("85d9ff"),-635);icePatches.Add(new IcePatch{at=e.strikePoint,root=root,expires=clock+1.6f,nextHit=clock});
  }
  private void TickIcePatches()
  {
   for(int i=icePatches.Count-1;i>=0;i--){IcePatch patch=icePatches[i];if(clock>=patch.expires){if(patch.root!=null)Destroy(patch.root);icePatches.RemoveAt(i);continue;}if(clock>=patch.nextHit){patch.nextHit=clock+.4f;if(Vector2.Distance(position,patch.at)<.95f&&!ProjectilePathBlocked(patch.at,position,.04f))TakeJourneyDamage(1,Element.Ice);}}
  }
  private void ClearIcePatches(){foreach(IcePatch patch in icePatches)if(patch.root!=null)Destroy(patch.root);icePatches.Clear();}
 }
}
