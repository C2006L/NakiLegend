# Encounter Intent V1 verification

Native Unity 2022.3.62f3 run: **PASS 653 checks**. See `runtime/smoke-result.txt` and the three PNG captures.

`build.py` stages canonical runtime scripts and builds the reflection test harness from the previous skill/starfall regression harness. The isolated project is `D:/NakiLegend/Validation/DesertV2/UnitySmokeProject`. It does not open the user's active project or reset player saves.

Coverage includes seven intent mappings; actual ranged retreat and anchor movement; waypoint commitment; finding a route/firing lane around solid cover; real projectiles without ordinary trajectory warnings; locked charge direction; off-screen fire suppression; mixed smaller wave budgets; camera follow/clamping; fifteen distinct main boss signatures; delayed hazard cleanup; a damaging tide wall with a safe gap; fixed frontal shell protection; and a 32.5-second circular-path simulation with actual incoming collisions. Previous resource/loadout/save, starfall, ice chapter and route regressions run in the same session.

The combat screenshot fixtures clear obstacle lists to isolate behavior and rendering. The separate navigation test installs a real solid rectangle. The circle simulation uses five indestructible enemies and a prescribed player path; it is a targeted kiting regression, not a full balance playthrough.

Reproduce:

1. `python Validation/EncounterIntentV1/build.py`
2. Compile using the generated `compile.rsp` and `test-compile.rsp` with Unity's bundled Roslyn compiler.
3. Launch the isolated project in batch mode with `-executeMethod NakiEncounterIntentV1Smoke.Run -logFile <absolute output path>`.

`sync_runtime.py` synchronizes audited scripts and their metadata into the actual nested Unity project and legacy distribution, backs up changed destinations, checks for independent edits, and verifies written bytes. It never copies scenes, resets saves or changes texture/audio import settings.
