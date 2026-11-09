# Armed Enemies V2

**PASS 1059 checks.** Native Unity 2022.3.62f3 verification for weapon-matched attacks, themed rewards and stronger boss identity effects.

`build.py` builds on the previous enemy-intent harness, adds `tests-extra.txt`, compiles reflection tests and stages runtime scripts into the isolated smoke project. It fixes the camera fixture to an explicitly large room rather than depending on a random entrance size.

Coverage: spear contact and lance projectile shape/element; single-hit melee with visible swing; arcing coal with delayed damage; five-island supply offers across 12 seeds and three professions; local-element elite drops at all three chapter floors with no duplicate callbacks; repair of unclaimed old chests while preserving claimed history; light-path rare offers; one-time fire departure armory and actual save/load; tide-gap markers, shell plates and actual HUD hint; plus prior intent/camera/skill/starfall/ice ending regressions.

Six native screenshots in `runtime`: weapon contact, tide wall, tortoise shell, forge chains, sustained fire patches and departure equipment menu. Rendering fixtures may clear geometry and freeze time to inspect the specific effects. They are not balance playthroughs.

Entry point: `NakiArmedEnemiesV2Smoke.Run`. Result: `runtime/smoke-result.txt`. Runtime sync preserves scenes and player saves, verifies destination hashes and backs up changed scripts.
