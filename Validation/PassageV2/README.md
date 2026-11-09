# Passage, voyage-save and replay validation

Final full native Unity 2022.3.62f3 regression: **PASS 1757 checks**, exit 0, no captured runtime exceptions or compiler errors. The targeted run passed **90 checks** before the final harbor artwork and stone sockets were applied; the full run then exercised those final changes. Standalone compilation passes with the existing test-injected-directory CS0649 warning.

New tests reproduce a high-clock Fire run followed by a fresh Thunder run and immediate save, all six bag slots surviving travel and restart, inherited elemental skills persisting through Desert save/load, the main adventure restoring its last saved island, and actual movement through harbor exits at left/center/right offsets. Every available harbor graph edge is checked, alongside per-island authored gate sprites and the new Thunder waymarkers.

The existing shoreline regression point at (0.95, 0.47) is now within an intentionally widened, paved eastern doorway approach. The water test and legacy unsafe-position fixture therefore move to (0.95, 0.58), beyond that new approach; sea collision and recovery remain mandatory.

Native runs use an isolated QA project and validation-only save directories. Real save reset was performed by the explicitly requested Editor operation after runtime synchronization: stop Play, copy and byte-check the complete named save set, then delete that set. All 12 original files are retained in the active project's `ProgressBackups/20261102-172227-052`. The Editor log confirms completion and no progress files remain active. Audio preferences and Unity project assets are preserved.

`build.py` freezes the VoyagerStory harness and adds `tests.txt`; `RunAtlasOnly` is the focused invocation and `Run` includes the whole five-island suite. `finalize.py` preserves Unity-generated GUIDs and prepares guarded sync. Existing scenes are not overwritten.

Rendered evidence: `runtime/rain-exit.png`, `runtime/ice-quay-shore.png`, `runtime/storm-gates.png`, `runtime/storm-waymarkers.png`. Image source prompts are recorded in `Art/PassageV2/prompts.json` and assets are under `Resources/NakiArt/PassageV2`.
