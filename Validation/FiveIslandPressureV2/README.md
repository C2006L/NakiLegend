# Five Island Pressure V2 validation

Unity 2022.3.62f3 native batch: **PASS 1862 checks**. Standalone runtime compilation passes with the pre-existing CS0649 warning for the optional chapter test save-directory field.

`build.py` derives from the frozen MingshaPolishV2 harness. Old fixed two-companion and five-shot assertions are updated to the new explicit formation count and seven warned shots. Existing attack-delay comparison now includes the island recovery multiplier while requiring the original windup. The preceding five-island story, atlas, saved inventory, Light/Dark, checkpoint, shore layout and audio checks remain active.

Added tests exercise all five actual island presets, separate normal/Boss health tables, attack recovery, live enemy caps, real once-per-wave companion injection, >=1.4 second companion warnings, seven-shot Boss warning with no premature projectiles, initial teaching exceptions and final-layer Fire/Forest formations. Existing tests verify bounded live release, spawn relocation at player feet, interruption, support/shield behavior, optional challenges, once-only phase reinforcement, forty-element Boss relief and sustained actual combat.

Run `python Validation/FiveIslandPressureV2/build.py`, then `NakiFiveIslandPressureV2Smoke.Run` in `D:/NakiLegend/Validation/DesertV2/UnitySmokeProject`. Outputs are under `runtime/`. The scripted pressure probes measure limited automated scenarios; difficulty and pacing still require player feedback.

The tests and guarded `sync_runtime.py` preserve real player saves. No growth currency, account stats, profile or camp is implemented in this patch. The design resides in `Docs/OUT_OF_RUN_GROWTH_PLAN.md`.
