using System.Collections.Generic;
using UnityEngine;

namespace NakiLegend.Prototype
{
    public sealed partial class NakiPrototype
    {
        private static readonly Vector2 EvacuationWinchAt = new Vector2(.2f, -1.5f);
        private readonly List<GameObject> evacuationWorkers = new List<GameObject>();
        private float evacuationStarted = -1;

        private void BridgePart(GameObject part)
        {
            part.transform.SetParent(chapterFocus.transform, true);
        }

        private void BuildEvacuationCrossing(RoomNode room)
        {
            evacuationWorkers.Clear();
            evacuationStarted = -1;
            float height = room.halfHeight * 2 + 1.6f;
            // The channel reaches the walls; the intact floor underneath is fully
            // covered. All north/south door approaches stay on the western bank.
            var trench = Stretch("Ash chute beneath evacuation bridge", ForgeFixture(2),
                new Vector2(3, 0), new Vector2(height, 2.8f), Color.white, -981);
            trench.transform.rotation = Quaternion.Euler(0, 0, 90);
            BridgePart(trench);
            if (!forgeBridgeOpen)
                TrackCampCollision(new Vector2(3, 0), new Vector2(2.8f, height), false);
            else
            {
                foreach (float sign in new[] { -1f, 1f })
                {
                    float end = room.halfHeight + .8f;
                    TrackCampCollision(new Vector2(3, sign * (end + 1.05f) * .5f),
                        new Vector2(2.8f, end - 1.05f), false);
                }
                BridgePart(Stretch("Lowered evacuation bridge", WorkshopProp(3), new Vector2(3, .12f),
                    new Vector2(3.9f, 2.15f), Color.white, -975));
            }
            // Safety rails follow the two real banks, leaving the crossing clear.
            foreach (float x in new[] { 1.45f, 4.55f })
                foreach (float sign in new[] { -1f, 1f })
                {
                    float end = room.halfHeight + .2f;
                    DrawCampRailing(new Vector2(x, sign * (end + 1.3f) * .5f),
                        new Vector2(.12f, end - 1.3f));
                }
            AddChapterAction(411, EvacuationWinchAt,
                forgeBridgeOpen ? "撤离桥已放下 · 工匠已抵达西岸" : "西岸绞盘 · 放下撤离桥", WorkshopProp(2));
            for (int i = 0; i < 2; i++)
            {
                Vector2 at = forgeBridgeOpen ? new Vector2(-3.5f - i * 1.1f, 1.1f) : new Vector2(6.1f, .9f - i * 1.8f);
                GameObject worker = Draw("Evacuating forge worker " + i,
                    ResidentSprite(i == 0 ? 1 : 7), at, Vector2.one,
                    Color.white, DepthAt(at.y));
                BridgePart(worker);
                evacuationWorkers.Add(worker);
            }
            if (Blocked(position, .3f)) position = NearestSafeFooting(position);
        }

        private void BeginWorkerEvacuation()
        {
            evacuationStarted = Time.unscaledTime;
            for (int i = 0; i < evacuationWorkers.Count; i++)
                evacuationWorkers[i].transform.position = new Vector2(6.1f, .9f - i * 1.8f);
        }

        private void TickWorkerEvacuation()
        {
            if (!fireChapterMode || currentRoom != 23 || !forgeBridgeOpen || evacuationStarted < 0) return;
            UpdateWorkerEvacuation(Time.unscaledTime - evacuationStarted);
        }

        private void UpdateWorkerEvacuation(float elapsed)
        {
            bool finished = true;
            for (int i = 0; i < evacuationWorkers.Count; i++)
            {
                GameObject worker = evacuationWorkers[i];
                if (worker == null) continue;
                float distance = Mathf.Max(0, elapsed - 1.1f - i * 1.1f) * 1.5f;
                Vector2[] route = { new Vector2(6.1f, .9f - i * 1.8f), new Vector2(5.4f, 0),
                    new Vector2(.5f, 0), new Vector2(-3.5f - i * 1.1f, 1.1f) };
                Vector2 at = route[0];
                for (int leg = 1; leg < route.Length; leg++)
                {
                    float length = Vector2.Distance(route[leg - 1], route[leg]);
                    if (distance <= length) { at = Vector2.Lerp(route[leg - 1], route[leg], distance / length); finished = false; break; }
                    distance -= length;
                    at = route[leg];
                }
                worker.transform.position = at;
                var renderer = worker.GetComponent<SpriteRenderer>();
                renderer.flipX = true;
                renderer.sortingOrder = DepthAt(at.y);
            }
            if (finished) evacuationStarted = -1;
        }
    }
}
