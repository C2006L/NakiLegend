# Origin Combat V2 validation

Unity 2022.3.62f3 native batch run: **PASS 1809 checks**, no captured runtime exceptions. Standalone C# compilation passes; the pre-existing optional `chapterBatchSaveDirectory` field produces CS0649.

Run `python Validation/OriginCombatV2/build.py`, then execute `NakiOriginCombatV2Smoke.Run` in the isolated project at `D:/NakiLegend/Validation/DesertV2/UnitySmokeProject`. `RunAtlasOnly` runs the origin-specific checks and visual captures. Player saves in the actual project are not read, erased or written by these tests.

The suite builds on the frozen Passage V2 five-island regression harness. Old orb and laser assertions are replaced with real channel, guard, solar and soul-seal contracts. The old stationary Light bot was not a valid driver for the new held-input action: the updated Light probe moves, calls the actual origin-input path before weapons, uses normal recovery, and attempts the learned advanced kit. Its assertion checks sustained actual damage and cast/resource activity, rather than requiring the old orb kill time. Dark probes retain their original defeat/kill assertions. The CSV records outcomes; this scripted probe is not a player balance assessment.

Additional coverage:

- L/O/P uniqueness and actual Input System O/P dispatch.
- Actual initial lesson; no advanced grant from island ordinal; Thunder and Fire record prerequisites.
- Startup/drain, maximum 2.5 seconds, release gate, delayed recovery, zero-energy shutdown.
- Two-target falloff, third-target exclusion, real cover, corpse reacquisition.
- Dodge, pause/overlay and room cleanup cancel the channel.
- Guard reduction and bounded refund; solar windup, once-only piercing hit and independent timers.
- Dark seal costs, once per meteor group and expiry; legacy equipped-primary values do not redirect L.
- Cooldown and recovery save round trips, original five-island atlas/backpack/progress regressions.
- Importable separate sound clips and native HUD/menu captures.

Reviewed captures: `runtime/light-channel.png`, `light-guard.png`, `solar-piercing.png`, `origin-key-guide.png`, `soul-seal.png`. The existing HUD rail fits five distinct skill slots without reducing the world viewport further.

`finalize.py` preserves 26 imported Unity GUIDs. `sync_runtime.py` checks local changes before copying canonical runtime assets into the actual and legacy distribution projects, with backups and byte verification. It preserves scenes and does not reset progress.
