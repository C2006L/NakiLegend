# Ice presentation and bag replacement verification

Unity 2022.3.62f3 native run: **PASS 1171 checks**, exit code 0, no captured Error / Exception / Assert. The run retains the existing BridgeScene / CombatPolish chapter, progression, save migration, navigation and combat coverage, and adds 67 checks for the requested fixes.

New checks cover all six replacement targets, each of the six resulting slots, exact old-item return to ground, active-slot preservation, repeat-confirm duplication prevention; actual rescue entrance collision and plank removal; snowy sign placement beside an exit, retained camp interaction and removal after rescue; actual two-arrow enemy attack with round decorations inactive and icy trails present.

The actual bag picker, rescue camp and moved sign captures under ignored `runtime/` were visually inspected:

- `bag-replace-any-slot.png`
- `ice-rescue-open.png`
- `ice-camp-sign.png`

`runtime/smoke-result.txt` and `final-unity.log` record the result. Build with `python Validation/IceBagPolish/build.py` and execute `NakiIceBagPolishSmoke.Run` in the isolated QA Unity project. `--reuse-runtime` regenerates the harness only when parent harness/runtime copies are already current.

Validation uses workspace paths and a unique product name instead of player saves. The existing checkpoint schema is unchanged. Runtime sync backs up changed files, preserves scenes and independent edits, then verifies destination bytes. No new audio or raster assets were generated.
