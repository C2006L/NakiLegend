# Thunder Warden and Manor blueprint validation

Native Unity 2022.3.62f3 regression: **PASS 7366 checks**. Existing five-island story, pressure, loot and saved progress tests remain. Added coverage runs the Warden's actual UpdateEnemy path to verify a telegraphed lunge moves more than two units, close/far move selection, increased phase fans, two auxiliaries per phase with no repeated summons, lazy Thunder rock reload, and all 39 Thunder rooms' rendered sprites excluding desert rock and pillar textures.

Run `build.py`, then `NakiThunderWardenV2Smoke.Run` in the isolated project. `NakiLegend.Prototype.Editor.NakiManorBlueprint.ValidateBatch` separately imports and verifies 45 reachable rooms with quest dependencies and paired stairs, eight enemy specifications, three ending branches, safe NPC placement and opening art. Manor content remains a blueprint and concept-art preview, not a playable sixth-island chapter.

`finalize.py` preserves imported metadata and records original image hashes. `sync_runtime.py` uses guarded backed-up copies and byte verification, leaving player saves and existing scenes untouched.
