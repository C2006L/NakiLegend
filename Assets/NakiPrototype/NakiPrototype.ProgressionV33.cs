using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype {
 public sealed partial class NakiPrototype {
  private const float MotherLifeBudget=2960;
  private static readonly float[] VanguardLifeByChapter={85,120,155,195,240};
  private readonly HashSet<Enemy> earlyVanguards=new HashSet<Enemy>();
  private readonly Dictionary<SignatureZone,float> earlyBeamOffsets=new Dictionary<SignatureZone,float>();
  private bool EarlyEncounter {get{return enableDesertJourney&&!manorCourtyardMode&&!capitalMode&&CurrentIsland.ordinal<=5;}}
  private void ResetEarlyEncounterGroups(){earlyVanguards.Clear();earlyBeamOffsets.Clear();}
  private bool ShapeEarlyFormation(RoomNode room){
   if(!EarlyEncounter||LearningEncounter||pendingSpawns.Count<3)return false;
   EnemyKind[] formation=woodlandMode?new[]{WoodKinds[5],WoodKinds[3],WoodKinds[12],WoodKinds[0],WoodKinds[2],WoodKinds[8]}:
    fireChapterMode?(room.stage==0?new[]{EnemyKind.EmberLizard,EnemyKind.AshRaider,EnemyKind.AshBat,EnemyKind.CoalThrower,EnemyKind.EmberLizard,EnemyKind.AshBat}:new[]{EnemyKind.MoltenCrab,EnemyKind.ForgeGuard,EnemyKind.AshBat,EnemyKind.CoalThrower,EnemyKind.FurnaceCore,EnemyKind.EmberAcolyte}):
    iceSampleMode?new[]{EnemyKind.IceGuard,EnemyKind.FrostHound,EnemyKind.SnowMoth,EnemyKind.IceArcher,EnemyKind.IceRunecaster,EnemyKind.IceWisp}:
    ThunderActive?new[]{EnemyKind.ThunderStone,EnemyKind.StormGuard,EnemyKind.CopperRoller,EnemyKind.ChargeMoth,EnemyKind.StormGuard,EnemyKind.ChargeMoth}:
    new[]{EnemyKind.SpearGuard,EnemyKind.SandScorpion,EnemyKind.Tumbleweed,EnemyKind.SandKite,EnemyKind.StoneSentinel,EnemyKind.SandKite};
   int front=Mathf.Min(3,pendingSpawns.Count-(pendingSpawns.Count==3?1:2));
   for(int i=0;i<pendingSpawns.Count;i++){
    PendingSpawn spawn=pendingSpawns[i];int slot=i<6?i:3+(i+currentWave+room.stage)%3;
    spawn.kind=formation[slot];spawn.role=0;spawn.vanguard=i==0;spawn.flank=i>=front;spawn.when=clock+(spawn.flank?1.65f+(i-front)*.18f:1+i*.22f);
    PlaceFormationSpawn(spawn,i,spawn.flank);if(spawn.mark!=null)spawn.mark.transform.position=spawn.at;
   }
   return true;
  }
  private void PlaceFormationSpawn(PendingSpawn spawn,int index,bool flank){
   Vector2 forward=(-position).normalized;if(forward.sqrMagnitude<.01f)forward=facing.sqrMagnitude>.01f?facing.normalized:Vector2.up;
   Vector2 side=new Vector2(-forward.y,forward.x);Rect room=CurrentRoomBounds;float extent=(Mathf.Abs(side.x)*(room.width*.5f-1.5f)+Mathf.Abs(side.y)*(room.height*.5f-1.5f))*.75f;
   Vector2 desired=flank?forward*1.1f+side*((index&1)==0?extent:-extent):-forward*1.5f+side*(index==0?0:index==1?-2.1f:2.1f);
   for(int attempt=0;attempt<9;attempt++){
    Vector2 candidate=desired+(attempt==0?Vector2.zero:(Vector2)(Quaternion.Euler(0,0,(attempt-1)*45)*Vector2.right)*.7f);
    candidate=new Vector2(Mathf.Clamp(candidate.x,room.xMin+1.1f,room.xMax-1.1f),Mathf.Clamp(candidate.y,room.yMin+1.1f,room.yMax-1.1f));
    if(Vector2.Distance(candidate,position)<2.5f||Blocked(candidate,.45f)||ProjectilePathBlocked(candidate,candidate,.45f)||IntersectsTransit(candidate,Vector2.one))continue;
    bool occupied=false;foreach(Enemy enemy in enemies)if(Vector2.Distance(enemy.position,candidate)<enemy.radius+.65f){occupied=true;break;}
    foreach(PendingSpawn other in pendingSpawns)if(other!=spawn&&Vector2.Distance(other.at,candidate)<1.05f){occupied=true;break;}
    if(!occupied){spawn.at=candidate;return;}
   }
  }
  private bool FlankCanEnter(PendingSpawn spawn){
   if(!spawn.flank||!EarlyEncounter)return true;
   // Contact, an enemy being attacked, or a bounded fallback opens the flank. Player skill cooldowns are irrelevant.
   if(clock>=encounterWaveStartedAt+3.2f)return true;
   foreach(Enemy enemy in enemies)if(!enemy.boss&&!enemy.summoned&&(enemy.hp<enemy.maximum-.01f||enemy.windup>0||Vector2.Distance(enemy.position,position)<5.2f))return true;
   return false;
  }
  private void ApplyEarlyVanguard(Enemy enemy,PendingSpawn spawn){
   if(!spawn.vanguard||spawn.summoned||!EarlyEncounter||enemy.boss)return;
   float life=VanguardLifeByChapter[Mathf.Clamp(CurrentIsland.ordinal-1,0,4)]*(spawn.champion?1.1f:1);
   enemy.hp=enemy.maximum=Mathf.Max(enemy.maximum,life);AssignTacticRole(enemy,5);earlyVanguards.Add(enemy);
  }
  private float EarlySignatureInterval(Enemy enemy,int profile){
   if(profile==12)return 10;
   if(!EarlyEncounter)return enemy.difficultyPhase>=2?3.7f:4.2f;
   float interval=EarlyEncounter?new[]{4.2f,4.1f,4f,3.9f,3.9f}[Mathf.Clamp(CurrentIsland.ordinal-1,0,4)]:4.2f;
   return interval-(enemy.difficultyPhase>=2?.5f:0);
  }
  private void AimEarlyBeam(SignatureZone zone){
   float offset;if(!EarlyEncounter||clock>=zone.active-.16f||!earlyBeamOffsets.TryGetValue(zone,out offset))return;
   Vector2 direction=position-zone.owner.position;if(direction.sqrMagnitude>.01f)zone.dir=Quaternion.Euler(0,0,offset)*direction.normalized;
  }
 }
}
