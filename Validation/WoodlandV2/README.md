# Forest V2 validation

Unity 2022.3.62f3 native editor run: **PASS 965 checks**, exit code 0. See `runtime/smoke-result.txt`, `final-unity.log` and `Smoke.cs`.

Standalone runtime/editor-source compilation also passed. Invoke Unity's Mono and Roslyn `csc.exe` with `-noconfig -nostdlib+ @Validation/WoodlandV2/compile.rsp`; this response file references Unity's .NET Standard assemblies, so the compiler's default framework references must be disabled. Output: `standalone-compile.log` (one expected warning for the test-injected checkpoint-directory field).

`build.py` extends the established Fire Core regression harness. It stages canonical sources and Forest artwork only into the isolated `D:/NakiLegend/Validation/DesertV2/UnitySmokeProject`. Player saves are never used; checkpoint writes are redirected into this validation directory.

Coverage includes:

- Existing equipment, talents, Ice and Fire ending regressions; append-only catalog now contains 33 weapons.
- All 45 Forest room links, arrivals and accessible exit paths with natural covers; three independent terrain textures.
- Staged wave composition, ranged/charger caps and spawn warnings; Forest-specific AI, directional plates and moth ward.
- Spore warning versus actual damage, expiry, three-zone cap, doorway exclusions, fire-path removal and actual Ember Ring removal; bounded poison damage.
- Three targetable Ancient Guardian knots, interrupted queued volley, recovery and no soul/crystal farming.
- Rescue, distinct clue requirements and no repeat reward farming; zero-energy planting, ledger return, illustrated CG, then exactly one talent.
- Historical `w3-v1` weapon migration and missing-CG recovery without resetting clears or duplicating talents; original save backup preserved.
- Explicit return cancellation, missing Fire save fallback and successful return to a separate Fire safe point with Forest talent retained.
- Actual HUD, dialogue, inventory, three environments, Mother boss, Ancient Guardian, harbor, CG and talent rendering without captured runtime exceptions.

Rendered evidence is in `runtime/`: `woodland-edge.png`, `woodland-fungal.png`, `thorn-mother.png`, `ancient-guardian.png`, `forest-harbor.png`, `woodland-ending-cg.png`, `woodland-talent.png`. The checks exercise game state and selected combat scenarios; they do not constitute a manual balance playthrough of every wave in every room.

`sync_runtime.py` preflights both distributed copies, preserves independently changed local files, backs up replaced runtime assets and verifies copied bytes. Existing scene files and player saves are excluded.
