# Skill loadout and starfall native regression

Unity 2022.3.62f3 completed **PASS 599 checks**. See `runtime/smoke-result.txt`. Runtime and smoke assemblies compile; the remaining warnings are pre-existing test reflection comparisons and the batch-save-directory field assigned through reflection.

Run `build.py` to stage the isolated smoke project and compiler response files. The Unity entry point is `NakiSkillStarfallV1Smoke.Run`. Test saves use a workspace directory and a unique validation product name, never the player's save directory.

Coverage includes actual zero-cost melee and full-charge ranged attacks, separate resource pools, repeated first-time/return atlas voyages, skill identity and cooldown preservation, Mingsha and chapter save round trips, legacy forest skill-ID migration, learning without auto-equip, starfall's three fixed warning circles, visible descent before damage, once-only area impacts, soul-seal proc timing, resource failure, room cancellation, and all six new audio imports.

The suite also runs the full 42-room ice encounter/ending regression, ice boss mechanics, and the prior route/Fire-stairs/rescue/mark checks. Four native PNG captures cover warning, descent, impact and the twelve-entry skill menu. Their presentation is visually inspected; final difficulty remains a player-testing question.

Runtime synchronization preserves scenes and player saves, guards independent local edits, backs up replaced scripts, and only adds the six new starfall sounds and their metadata. Other texture/audio import settings are excluded.
