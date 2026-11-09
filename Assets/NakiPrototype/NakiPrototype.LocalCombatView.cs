using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private bool LocalCombatView {get{return enableDesertJourney&&journeyRooms.Count>currentRoom&&currentRoom>=0;}}
  private Vector3 LocalCombatCenter()
  {
   RoomNode room=journeyRooms[currentRoom];float height=view!=null?view.orthographicSize:4.15f,width=height*(view!=null?view.aspect:1.8f);
   Vector2 desired=position+Vector2.ClampMagnitude(intentPlayerVelocity*.15f,.6f)+Vector2.up*.35f;
   float x=Mathf.Max(0,room.halfWidth+.65f-width),y=Mathf.Max(0,room.halfHeight+.8f-height);
   return new Vector3(Mathf.Clamp(desired.x,-x,x),Mathf.Clamp(desired.y,-y,y),-10);
  }
  private bool EnemyOnScreen(Vector2 at,float margin=0)
  {
   if(!LocalCombatView||view==null)return true;Vector2 center=view.transform.position;
   return Mathf.Abs(at.x-center.x)<=view.orthographicSize*view.aspect-margin&&Mathf.Abs(at.y-center.y)<=view.orthographicSize-margin;
  }
  private void ApplyLocalCombatView()
  {
   if(!LocalCombatView)return;RoomNode n=journeyRooms[currentRoom];bool boss=n.kind==RoomKind.Boss||n.kind==RoomKind.Guardian;
   view.orthographicSize=boss?4.6f:IsCombatRoom(n)?4.15f:Mathf.Min(4.15f,(n.halfWidth+.25f)/Mathf.Max(.1f,view.aspect));
  }
 }
}
