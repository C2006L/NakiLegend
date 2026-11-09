using System.Collections.Generic;
using UnityEngine;

namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private sealed class EncounterRecord {public string island,room;public float began,damage;public int hits,kills,casts;public bool boss,trial;}
  private EncounterRecord encounterRecord;
  private string lastEncounterReport;
  private readonly List<string> encounterReports=new List<string>();
  private void BeginEncounterRecord()
  {
   if(encounterRecord!=null)return;RoomNode n=journeyRooms[currentRoom];
   encounterRecord=new EncounterRecord{island=CurrentIsland.name,room=JourneyRoomTitle(n),began=clock,boss=n.kind==RoomKind.Boss||n.kind==RoomKind.Guardian,trial=challengeActive};
  }
  private void RecordEncounterHit(float damage){if(encounterRecord!=null){encounterRecord.hits++;encounterRecord.damage+=damage;}}
  private void RecordEnemyDefeat(Enemy e){if(encounterRecord!=null&&!CombatFixture(e))encounterRecord.kills++;}
  private void RecordEncounterCast(){if(encounterRecord!=null)encounterRecord.casts++;}
  private void FinishEncounterRecord(bool cleared)
  {
   if(encounterRecord==null)return;var r=encounterRecord;encounterRecord=null;
   lastEncounterReport="[Naki Encounter] "+r.island+" / "+r.room+(r.trial?" / 挑战":"")+" / "+(cleared?"完成":"倒下")+" / "+(clock-r.began).ToString("0.0")+"秒 / 受击"+r.hits+" / 伤害"+r.damage.ToString("0.0")+" / 击败"+r.kills+" / 技能组"+r.casts;
   encounterReports.Add(lastEncounterReport);if(encounterReports.Count>24)encounterReports.RemoveAt(0);Debug.Log(lastEncounterReport);
  }
 }
}
