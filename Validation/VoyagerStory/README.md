# Five-island voyager story regression

Unity 2022.3.62f3 full native run: **PASS 1665 checks**, exit 0. Targeted story run: **PASS 45 checks**, exit 0. Standalone runtime compilation passes with the existing CS0649 warning for the test-injected checkpoint-directory field.

`build.py` freezes the QuayMotion harness and adds `tests.txt` to generate `Smoke.cs`. It stages canonical runtime assets into the isolated QA project; real project scenes and player saves are not used. `NakiVoyagerStorySmoke.Run` runs the full suite; `RunAtlasOnly` runs the targeted story checks and journal screenshot.

New coverage includes the Mingsha display name, shared evidence in both Light and Dark routes, once-only milestone narration, revised records from legacy lore bits, unchanged resource reserves when reviewing, genuinely unknown pages after free atlas travel, the actual final Forest boss → planting → northern-letter chain, current and legacy save round trips, and rejection of inferred skill eligibility as narrative evidence.

The Forest save fixture defeats the final boss before planting; it must not fabricate the stone flag, which intentionally fails the existing progression validator.

Actual rendered UI: `runtime/voyager-journal.png`. Text has been visually checked for clipping. Native output: `runtime/smoke-result.txt`, `native-quick.log`, `native-unity.log`. Batch checkpoint paths are confined to this validation directory.

`finalize.py` preserves Unity-generated metadata and prepares `sync_runtime.py`. Sync checks for independent local edits, backs up replaced runtime files and compares copied bytes. Existing scenes and real saves remain outside its scope.
