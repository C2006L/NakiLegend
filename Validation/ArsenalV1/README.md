# Arsenal and talent native verification

Unity 2022.3.62f3: **PASS 409 checks**, exit code 0, no captured Error / Exception / Assert. Actual `runtime/talent-choice.png` and `runtime/journey-skills.png` were visually inspected. Final runtime compilation also passed after a UI-priority adjustment allowing audio settings over talent selection.

The harness reuses existing scene/setup helpers but deliberately replaces historical fixed-damage, fixed-mana and fixed-projectile-count assertions with the new specification. It runs `NarrativeAndPower`, `EndingGate` and `ArsenalTests`. It does **not** claim to rerun all 1171 historical assertions unchanged or add their count to this run.

New behavior checked:

- Full-charge bow 72 → 48 and crossbow 78 → 52; elemental and light costs reduced while pools stay separate.
- Three distinct talent offers, one recorded choice per completed island, no repeated reward after selection or reload.
- Actual shield-overflow protection, damage with no shield, timed paid repair and no free repair with empty mana, front-only reflection and reflected-arrow orientation.
- Charge time, 140-point capacity, efficiency, skill damage and crystal yield talents.
- Chapter and Desert disk saves with talents / 139 mana, legacy talent-free checkpoint loading, ledger merge, and new skill-slot roundtrip.
- Returning wind blades turning back, paid guard pulse and cooldown, appended weapon IDs, charged piercing crossbow, chain staff and slowing hammer.
- Separate real hammer audio assets.
- Traversable arrivals and doorway routes in every normal / elite room of the five generated validation island layouts, plus actual additional-cover generation. This checks current generated seeds, not every possible random seed.
- One companion per wave, no frame-by-frame duplication or cleared-room respawning, and eight-percent faster enemy scheduling without shortening windup.

`runtime/smoke-result.txt` and `final-unity.log` hold the native result. Outputs/logs are ignored by Git. Runtime verification uses unique validation product names and workspace save paths, leaving player saves untouched.

Run `python Validation/ArsenalV1/build.py`, then `NakiArsenalSmoke.Run` in the isolated QA project. `--reuse-runtime` is for local iteration with the existing parent helper harness already prepared; changed runtime assets are copied only when bytes differ. Source snapshots and old assertions remain for history.

The six hammer audio variants are rebuilt by `Tools/build_hammer_audio.py`. Existing music and sword banks were not edited. `sync_runtime.py` preflights actual / legacy copies, preserves scenes and independent edits, backs up changed destinations, copies meta first and byte-verifies results.

Subjective balance and sound preference still need player feedback. New weapon sprites reuse existing art; this run does not certify new animation art or low-end frame rate.
