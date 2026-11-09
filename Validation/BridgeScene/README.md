# Evacuation crossing native verification

Unity 2022.3.62f3: **PASS 1104 checks**, exit code 0, no captured runtime Error / Exception / Assert. Actual `bridge-closed.png` and `bridge-open.png` under ignored `runtime/` were visually inspected.

The harness derives CombatPolish and retains its story, navigation, save migration, combat, and existing chapter coverage. Previous decorative-pit assertions are relocated to the actual new crossing at (3, 0). New checks cover:

- Closed trench blocking at nineteen positions along its height.
- Reachable winch / north / south bank approaches and a genuinely disconnected opposite bank before deployment.
- Safe arrivals from all four doors, real F interaction at the new winch position, and reachable opposite bank after lowering.
- Continued collision outside the deployed bridge.
- Both workers' collision-safe paths sampled at 121 time positions, followed by completion on the western bank. These sampled checks are included in 1104, not additional suites.
- Disk save/load preserving bridge completion, workers remaining evacuated on reload, and repeated interaction rebuilds not accumulating collision rectangles.

The result is `runtime/smoke-result.txt`; `final-unity.log` records the run. The player's saves are untouched: the harness uses a unique validation product name and workspace checkpoint paths. No new save schema or map version is introduced.

Build with `python Validation/BridgeScene/build.py`, then execute `NakiBridgeSceneSmoke.Run` in the isolated validation Unity project. `--reuse-runtime` uses the existing parent harness and only updates changed runtime C# files for local iteration. `sync_runtime.py` preserves scenes and independently edited destinations, backs up old runtime files and byte-verifies the sync.

Scene art reuses existing atlases; no new generated raster assets or audio replacements. Worker movement uses the existing character sprites and path interpolation; it does not introduce new frame animation artwork.
