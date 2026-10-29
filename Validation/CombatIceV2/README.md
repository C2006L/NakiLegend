# Combat audio V3 and ice expansion validation

Unity 2022.3.62f3 in the isolated smoke project. `build_smoke.py` derives `Smoke.cs` from the previous ice sample harness, adjusts the revised map expectations, and adds new functional coverage. Execute `NakiCombatIceV2Smoke.Run` in the QA project with current runtime sources and resources.

Final native result: **PASS — 654 checks**, including actual HUD screenshots without runtime errors. Screenshot review caught and fixed the negative-coordinate branch reaching the map legend; the final capture confirms centered cells with clear legend space. New enemy/prop atlas regions, silhouettes and relative scale were also inspected in the real Game view.

Coverage includes old island regressions plus:

- Five rooms, four main-corridor waves, three waves per optional branch and nonduplicating rewards.
- Reachable paths from room center to each actual doorway, collision-free arrivals and differentiated fissure/solid-cover behavior.
- The visible upper part of a cargo crate also blocks bullets.
- Hound windup and locked lunge; archer delayed locked-direction arrows, including arrow sprite; rune warning, escapable old location, active damage and cleanup.
- Soul eligibility for living creatures versus constructs.
- Original three-room checkpoint migration preserving main/Boss clears, skills, cooldowns, equipment and resources; exact old file backup on next save.
- All 69 new clips load ahead of legacy banks; short durations and correct electric-staff routing.
- Map cells fit both minimap and full-map content bounds, including the negative-coordinate branch.

`runtime/` is ignored and contains the native log, PASS result, source-audio protection hashes and actual screenshots. All 82 protected old audio files remain SHA-256 identical. The new WAVs have zero clipped samples and peak at or below 0.70; longest clip is 0.27 seconds. Listening preference is assessed separately using the supplied audition.
