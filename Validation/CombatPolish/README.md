# Combat polish native verification

Unity 2022.3.62f3: **826 regression checks passed**, exit code 0, no captured Error / Exception / Assert. A separate **2-check native impact preview** also passed and its screenshot was visually inspected. These counts are separate runs, not a combined full-suite count.

`build.py` derives the Combat V3 harness, retains its story / chapter / navigation / migration coverage and adds the cases in `tests.txt`. New cases exercise actual `UpdateShot`, damage and checkpoint paths:

- Fast hostile swept contact and a wall protecting the player.
- Near-to-far contact ordering independent of spawn order, an enemy before a wall, a protected enemy behind it, piercing through a kill and repeated-overlap protection.
- Homing retarget without extending life, cover and rear-target rejection, reacquisition after the line clears, ordinary non-homing shots unchanged.
- Three actual hits retaining their full damage while aggregating display numbers, later hits separated, counter-element labels throttled.
- Six element impact styles created on contact and cleaned up after expiration; staff bright cores retaining weapon audio context.
- An eight-point crystal filling a one-point deficit, exact seven-point remainder saved to disk and loaded, then used later; solid-cover pickup and magnet blocking.
- Ado's dialogue changing after rescue, bridge lowering and boss completion.

The final regression result is `runtime/smoke-result.txt`; its log is `final-unity.log`. The visual check result is `runtime/preview-result.txt` with `preview-unity.log`; `runtime/element-impacts.png` is the actual native render. Other chapter screenshots remain under `runtime/`. Runtime outputs and logs are ignored by Git.

Run `python Validation/CombatPolish/build.py`, then Unity's `NakiCombatPolishSmoke.Run` against the isolated validation project. `--reuse-runtime` only regenerates the harness when runtime sources and the parent harness are already current; it is for test-only iterations. `RunImpactPreview` runs just the native impact capture, not the gameplay suite. Validation uses a unique product name and workspace save paths, leaving player checkpoints alone.

`sync_runtime.py` preflights the actual / legacy runtime copies, preserves scenes and independently edited destinations, backs up previous files under `runtime/pre-polish-sync`, copies meta first, and verifies destination bytes. The manifest is `runtime/sync-manifest.json`.

This verifies behavior and rendering, not subjective difficulty or low-end performance. No new audio assets or generated image assets were introduced in this round.
