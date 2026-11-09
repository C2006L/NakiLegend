# Tactical encounter and difficulty validation

Prepare with `python Validation/PressureV1/build.py`, then run Unity 2022.3.62f3 against the isolated `D:/NakiLegend/Validation/DesertV2/UnitySmokeProject` with execute method `NakiPressureV1Smoke.Run`. The harness owns editor lifecycle and redirects all opted-in test saves under this directory. It does not use real play saves.

The harness derives the five-island LegacyV1 regression and updates only superseded timing and reinforcement-count expectations. Additional tests in `tests.txt` exercise actual tactical behavior, projectile collisions, cover removal, overlap scheduling, once-only boss reinforcements, source-volley damage, actual optional challenge waves and persisted rewards.

Outputs: `runtime/smoke-result.txt`, `runtime/combat-probes.csv`, screenshots of the tactical room, optional challenge menu and late boss, plus inherited native UI/route/CG screenshots. Check count varies slightly with generated layouts.

Combat probes run real enemy AI, shots, defenses, energy and room ticks at 0.05-second simulation steps. They compare a stationary no-dodge dark player with damage-immune sustained-output dark/light profiles. They diagnose burst duration and survival, and do not simulate human decision-making or every possible build. The light probe also includes natural light recovery.

Property import and screenshots are visually checked. `sync_runtime.py` audits independent local edits, backs up prior runtime, installs Unity-generated metadata and byte-verifies both user and legacy projects, while preserving scenes and player save files.
