# Full boss mobility and combat pressure audit

Unity 2022.3.62f3 native regression: **PASS 7476 checks**. Standalone C# compilation passes with the pre-existing optional test-directory CS0649 warning.

The added audit runs the real `UpdateEnemy` dispatch for all 17 authored encounter families in three health phases, for 13 simulated seconds per phase. `runtime/boss-mobility.csv` contains 51 rows. The 48 mobile phases each travelled at least 0.91 world units; all phases started at least four warned attacks. The three rooted-tree phases are deliberately exempt from travel and still attack. No artificial `nextAttack` delays or direct footwork calls are injected into this movement audit.

Specific tests cover active combat shield delay/rate, cleared-room recovery, a two-base-damage Thunder hit costing 2.56 shield, 0.55 second hit protection, and no movement/aim change during an existing telegraph. Existing coverage retains collision, phase auxiliaries, once-per-charge contact, once-only energy drops, weapon availability, 39-room Thunder artwork audit, Light/Dark costs and saves.

The legacy boar contact assertion now checks exactly one hit using the current damage multiplier, rather than an old shield threshold. Attack events are counted through actual windup transitions, because boss phase and distance selection may reset the authored move counter.

Run `build.py`, then `NakiBossMobilityV2Smoke.Run` in the isolated smoke project. `finalize.py` preserves the new runtime script GUID. The guarded `sync_runtime.py` preserves scenes and real player progress. Scripted probes provide evidence of functioning mechanics; actual player feedback remains necessary for final difficulty balance.
