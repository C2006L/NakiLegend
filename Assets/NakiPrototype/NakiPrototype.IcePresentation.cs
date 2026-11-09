using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private void RemoveRoundBulletDecorations(Shot shot)
  {
   foreach(string name in new[]{"Hostile bullet outline","Hostile bullet core"})
   {
    Transform child=shot.root.transform.Find(name);if(child==null)continue;
    child.gameObject.SetActive(false);Destroy(child.gameObject);
   }
   AddShotRibbon(shot,C("bde8ff"),.025f);
  }

  private void DrawIceCampSign(RoomNode node)
  {
   int next=-1;var pending=new Queue<int>();var first=new Dictionary<int,int>();
   pending.Enqueue(node.id);first[node.id]=-1;
   while(pending.Count>0&&next<0)
   {
    int id=pending.Dequeue();foreach(int link in journeyRooms[id].links)
    {
     if(first.ContainsKey(link)||journeyRooms[link].stage!=node.stage)continue;
     first[link]=id==node.id?link:first[id];
     if(link==18){next=first[link];break;}pending.Enqueue(link);
    }
   }
   if(next<0)return;
   int direction=NeighborDirection(node,journeyRooms[next]);Vector2 normal=DoorNormal(direction);
   Vector2 at=DoorPoint(direction,-1.3f)+new Vector2(-normal.y,normal.x)*1.45f;
   DrawWorldSign(at,chapterFocus.transform,normal);
   AddChapterAction(311,at+Vector2.down*.4f,"破冰队营地 · 沿此出口返回");
  }
 }
}
