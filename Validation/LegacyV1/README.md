# Residents, relics and origin legacy regression

Build: `python Validation/LegacyV1/build.py` stages only into the isolated Unity smoke project. The harness derives existing five-island gameplay regressions and adds tests from `tests.txt`.

Run with Unity 2022.3.62f3, isolated project `D:/NakiLegend/Validation/DesertV2/UnitySmokeProject`, execute method `NakiLegacyV1Smoke.Run`. The harness owns editor lifecycle and exits when finished; batch saves are explicitly redirected under this validation directory.

Result: `runtime/smoke-result.txt`. Native screenshots include `refuge-during-combat.png`, `refuge-safe.png`, `fire-supply.png`, `unbounded-dark-power.png`, `origin-ability-menu.png`, and the inherited five-island UI/ending captures. Check count can vary slightly with generated room layouts.

Coverage: prior route/CG/talent/save/cover regressions; eight resident sprite crops and protective shelter; seventeen signature reward families and real boss completion integration; once-only grants across reload; 123 stable weapon definitions; forty random-offer seeds and persisted offers; unbounded dark reserve and multi-volley costs; large-number save and HUD; voyage into older poorer/richer saves without resource refill; guard/beam lifecycle; real light/dark hits and beam wall blocking; five route memories and progression unlocks; delayed molten-core damage, chain pull, venom outside Forest and bounded mirror reflection.

`sync_runtime.py` preflights independent local edits, backs up changed runtime files, copies metadata before assets and byte-verifies both the actual user project and legacy distribution. It excludes scenes and player save locations. Backups and sync manifest are inside `runtime/`.

Automated gameplay checks do not measure subjective balance, audio quality or artistic preference. Native screenshots are visually inspected separately.
