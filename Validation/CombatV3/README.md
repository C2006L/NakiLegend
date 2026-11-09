# Combat V3 native verification

Unity 2022.3.62f3 native run: **PASS 788 checks**, exit code 0. No captured Error, Exception or Assert. Execution uses an isolated project with a unique validation product name, so player checkpoints are not touched.

`build.py` derives the existing chapter harness and adds `tests.txt`. It retains story, navigation, Ice chapter, boss mechanics, themed stairs, Fire Coast, Forge Workshop, save migration and island roundtrip checks. Historical audio/growth suites with superseded numeric assumptions are not counted as rerun; their old result counts must not be added to 788.

New meaningful checks cover:

- K preserves/pause-times bow charge; mid-dash release waits and fires once; full bow/crossbow damage; menus cancel buffered actions.
- Five staff types emit three projectiles, pay once, and do not fire or spend resources when the held weapon is rendered.
- Primary and elemental five-shot volleys, independent energy pools, two primary-group limit, actual separate weapon/skill audio assets, blocked casts spending nothing.
- Per-island HP and damage multipliers, bounded hostile count, room transition cleanup.
- Illustrated rescue rails/trench with collision, rescue objective discovery, BFS exit direction, both arrow legs facing the destination, locked-gate reason, F bridge progression.
- Forge second-phase 41-shot pattern, opaque hostile cores, and at least twenty real hostile projectiles surviving 0.7 seconds of collision-aware movement. Screenshot timing freezes only after actual simulation.

Native outputs in ignored `runtime/`:

- `smoke-result.txt`: 788 passed checks.
- `rescued-smith-rails.png`: actual rescue rail and route arrow.
- `bridge-trench-guide.png`: illustrated trench and F handwheel label.
- `bridge-objective-map.png`: objective marker on the actual map.
- `dense-combat.png`: native hostile/friendly projectiles, not a concept mockup.

`final-unity.log` is the native log. Existing spell/boss numeric assertions are explicitly updated in `build.py` to the new declared specification. Final screenshots were visually inspected. This run does not certify low-end frame rate or replace playtesting of difficulty and sound preferences.

`sync_runtime.py` preflights local runtime copies, backs up changed destinations, preserves existing scenes, copies meta first and byte-verifies each result. The sync manifest and previous runtime files are under ignored `runtime/`. One-time `connect.py` and `connect_guidance.py` record implementation transformations; do not rerun them.
