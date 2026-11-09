# Island supplies and packbeast validation

Unity 2022.3.62f3 native run: **PASS 1080 checks**, exit code 0 (`runtime/smoke-result.txt`, `native-unity.log`). The harness extends Forest V2's existing Ice/Fire/Forest and equipment/talent regression rather than testing only the new UI.

New coverage:

- Append nine ranged weapons as IDs 33–41 without changing existing identities; separately cropped atlas regions, including visual upgrades to older island ranged equipment.
- Fire, Forest, Ice and Thunder supplies each offer three distinct weapons, with Staff and Bow choices regardless of profession. Fire/Ice/Forest use their own element pool (the Forest bamboo light crossbow intentionally remains physical).
- Recovery grants +2 health, +2 shield and +35 elemental energy, capped independently and without touching Light/Dark energy. A consumed cache cannot heal repeatedly or also grant equipment.
- Weapon selection produces exactly one pickup, preserves health, and survives a checkpoint round trip with the claimed chest state.
- The actual staff/charged-crossbow attacks consume energy and produce the correct projectile counts; ice rail crossbow pierces four targets, and old/new Wood staves apply the described short poison.
- Tortoise victory stays in its court; invalid early harbor travel is rejected. Both evacuation sections remain required, final escort stays in the passage, and only explicit return travels to harbor. Updated dialogue and early-return objective track current progress; actual hearth activation still precedes CG and talent.
- Boar wind-up keeps position and locked aim; the dash does not flip toward a passed target. Collision stops it at cover and keeps recovery stationary; one rush deals at most one contact hit even with player invulnerability disabled in the test.
- Native rendered evidence: `fire-supply.png`, `forest-supply.png`, `packbeast-telegraph.png`, plus the inherited Forest/Fire/Ice presentation screenshots. No captured runtime exceptions.

All writes are redirected into validation-owned files, never real player checkpoints. `build.py` stages canonical files into `D:/NakiLegend/Validation/DesertV2/UnitySmokeProject`; the active harness is `NakiIslandSuppliesSmoke.Run`. The count includes parameterized inherited checks and can vary with layout seeds. These checks do not replace a manual full-island combat-balance playthrough.

Runtime/editor-source standalone compilation uses Unity Mono and Roslyn with `-noconfig -nostdlib+ @Validation/IslandSupplies/compile.rsp` (disable default compiler framework references because this response file uses Unity's .NET Standard references).

`sync_runtime.py` checks both distributed runtime copies, preserves independent local changes, backs up replaced assets and verifies copied bytes. It excludes existing scenes and player saves. Image prompts and original source provenance are recorded in `Art/IslandArmory/prompts.json`.
