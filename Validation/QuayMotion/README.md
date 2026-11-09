# Quay geometry and enemy motion regression

Full native regression: **PASS 1623 checks**, exit code 0, no captured runtime exceptions or compilation errors. Standalone compilation also passed (one existing warning for the test-injected save-directory field).

Runs in Unity 2022.3.62f3 against the isolated QA project. Player saves are never used; checkpoint reproduction uses this validation directory.

The targeted native run passed **103 checks**. It covers four chapter-harbor shoreline profiles, dry exit reachability, repeated interaction rebuilding without collision duplication, recovery of a legacy saved position in water, the authored Desert lookout, central boardwalk versus sea, and collision against projectile-only cover.

Motion probes exercise **33 enemy archetypes** across all five islands for 240 ticks each, including melee, ranged, charging and boss enemies. They measure facing flips during committed attacks and frame-to-frame navigation displacement. Charge cover collision is also tested directly for straight-line stopping, recovery and absence of next-frame footing snapback.

`build.py` freezes the prior WorldAtlas harness and adds these checks. `RunAtlasOnly` executes the targeted tests; `Run` includes the full existing five-island story, combat, checkpoint, reward, ending and atlas regressions. Native result and log are stored in `runtime/smoke-result.txt` and `native-unity.log`.

Actual rendered evidence: `runtime/ice-quay-shore.png` and `runtime/desert-rain-quay.png`. The test scene explicitly marks its preview talent as chosen so the lookout screenshot is not hidden by a talent menu; it does not alter player progress.

`finalize.py` copies Unity-generated metadata identities and prepares guarded runtime sync. Existing scene files and real saves are excluded. These automated probes test state and navigation stability; they do not replace human assessment of every animation.
