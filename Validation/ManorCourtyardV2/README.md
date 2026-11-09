# Manor Courtyard V2 verification

**PASS 1401 native checks.** Runtime exceptions and chapter save warnings: none in the final release run.

Native Unity 2022.3.62f3 runner: `NakiManorCourtyardV2Smoke.Run`.

Tests run in the isolated workspace smoke project with workspace-owned saves, never player saves. The self-contained harness includes the previous armed enemy, themed reward, intent/camera, skill ownership/starfall and full Ice progression checks. It adds the 15-room manor graph, quay sea collision, evidence-gated captain, ten armed enemy roles, single low-frequency meteor timing, interruptibility, wave diversity, one-time supplies, actual captain pursuit/phase/energy/surrender, exclusive spear and save/load/atlas return tests.

`prepare_assets.py` slices source artwork while preserving generated alpha and equal source cell sizes. Runtime extracts the main connected actor rectangle with **unchanged PPU and original cell foot anchor**, excluding detached neighboring-frame fragments. Native tests assert the original anchor across every action.

`prepare_audio.py` derives two captain spear Foley clips from the existing CC0 library. `prepare_tests.py` assembles the test harness. `build.py` stages it in the isolated project. `refine_qa.py` keeps backups of obsolete self-contained test runners outside compilation to avoid repeatedly compiling them all.

Screenshots in `runtime`: quay, five new foes, smoky meteor warning, captain charge, opening CG, held spear and atlas. These are deterministic rendering fixtures, not a human balance playthrough. An initial fixture omitted captain prerequisites and the profession selection; it was corrected to match valid progression before the final run.

`sync_runtime.py` backs up changed scripts and byte-verifies copied scripts/new artwork/audio/new scenes in the actual Unity project. Existing scenes and all player saves are preserved.

See `runtime/smoke-result.txt` and the final `runtime/unity-release.log` for the executed result.
