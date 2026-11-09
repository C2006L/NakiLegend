using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private int additionalCoverCount;private int densityRoom=-1,densityWave=-1;private bool densityAdding;
  private void AddWaveCompanion()
  {
   if(densityAdding||currentRoom<0||currentRoom>=journeyRooms.Count)return;
   RoomNode room=journeyRooms[currentRoom];
   if(room.cleared||pendingSpawns.Count==0||room.kind!=RoomKind.Combat&&room.kind!=RoomKind.Elite||CurrentIsland.ordinal==1&&clearedCombatRooms<2||densityRoom==currentRoom&&densityWave==currentWave)return;
   densityRoom=currentRoom;densityWave=currentWave;densityAdding=true;
   try{QueueTacticalCompanions(room);}
   finally{densityAdding=false;}ShapeIntentFormation(room);
  }
  private void AddBossFightCover(RoomNode room)
  {
   foreach(Vector2 at in new[]{new Vector2(-3,1.8f),new Vector2(3,-1.8f),new Vector2(-4,-1.8f),new Vector2(4,1.8f),new Vector2(0,2.6f),new Vector2(0,-2.6f),new Vector2(-5,2.6f),new Vector2(5,-2.6f),new Vector2(-2,-2.6f),new Vector2(2,2.6f)}){
    Rect foot=new Rect(at.x-.5f,at.y-.1f,1,.5f),lane=Rect.MinMaxRect(foot.xMin-1,foot.yMin-1,foot.xMax+1,foot.yMax+1);
    if(!CurrentRoomBounds.Contains(lane.min)||!CurrentRoomBounds.Contains(lane.max)||IntersectsTransit(lane.center,lane.size))continue;bool blocked=false;foreach(Rect other in obstacles)if(lane.Overlaps(other)){blocked=true;break;}if(blocked)continue;
    Sprite art=manorCourtyardMode?ManorSprite("prop-3",2):woodlandMode?WoodlandNature(1):fireChapterMode?(room.stage==2?CoreArt(3):room.stage==1?WorkshopProp(1):FireProp(1)):iceSampleMode?IceSprite(4):ThunderActive?PressureProp(1):pillarArtwork;
    if(art==null)continue;Draw("Boss refuge cover",art,at,Vector2.one*.7f,Color.white,DepthAt(at.y));obstacles.Add(foot);projectileObstacles.Add(foot);additionalCoverCount++;break;
   }
  }
  private void AddEncounterCover(RoomNode room)
  {
   additionalCoverCount=0;if(room.kind==RoomKind.Boss||room.kind==RoomKind.Guardian){AddBossFightCover(room);return;}if(manorCourtyardMode||woodlandMode)return;if(room.kind!=RoomKind.Combat&&room.kind!=RoomKind.Elite)return;
   foreach(Vector2 at in new[]{new Vector2(-3.4f,-2.7f),new Vector2(3.4f,2.7f),new Vector2(3.4f,-2.7f),new Vector2(-3.4f,2.7f)})
   {
    if(additionalCoverCount>=2)break;
    if(IntersectsTransit(at,new Vector2(2,2))||Blocked(at,.95f)||ProjectilePathBlocked(at,at,.95f))continue;
    if(!fireChapterMode&&!iceSampleMode&&!woodlandMode&&ThunderActive){AddThunderRock(at);additionalCoverCount++;continue;}
    Sprite art=woodlandMode?WoodlandNature(room.stage%2):fireChapterMode?(room.stage==2?CoreArt(3):room.stage==1?WorkshopProp(1):FireProp(1)):iceSampleMode?IceSprite(4):pillarArtwork;
    if(art==null)continue;
    Draw("Additional encounter cover",art,at,Vector2.one*.75f,Color.white,DepthAt(at.y));
    obstacles.Add(new Rect(at.x-.45f,at.y-.12f,.9f,.38f));
    additionalCoverCount++;projectileObstacles.Add(new Rect(at+(Vector2)art.bounds.min*.75f,(Vector2)art.bounds.size*.75f));
   }
  }
 }
}
