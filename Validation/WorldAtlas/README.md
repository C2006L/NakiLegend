# World atlas and Forest completion verification

Final full native run: **PASS 1517 checks**, exit code 0, without captured runtime exceptions or compiler errors. Standalone runtime/editor-source compilation also passed (one existing warning for the test-injected save-directory field).

Unity 2022.3.62f3 is run against the isolated `D:/NakiLegend/Validation/DesertV2/UnitySmokeProject`; every save used by these checks belongs to this validation folder. No player files or active user scenes are used.

`build.py` extends the existing FlowAudit regression and stages the canonical runtime. `RunAtlasOnly` runs 35 new checks plus actual atlas and seedbed rendering. The full `Run` also exercises previous five-island equipment, combat, story, endings, rewards and save migrations.

New checks cover:

- The former Fire return interaction opens the map without changing island, with Forest suggested after completion.
- Destination selection has a separate confirmation; all five playable islands and explicit floor entries are supported.
- Real dark reserves, health and elemental balances carry across islands without resurrecting historical saved balances. First-time rest-room rewards remain once-only.
- A loaded destination resumes its safe room and safe footing; source Fire completion survives return voyages.
- Skipping to the Forest third floor does not grant previous rescue, boss victory, fragments or talents.
- Both unreadable destination files are preserved and source state is restored; unavailable Manor travel is rejected.
- The map can be inspected during an encounter, while actual travel remains unavailable until safe.
- Ancient root lanes deal damage only on their announced impact; destroying a parasite knot cancels pending root attacks.
- Spore Crown places delayed bursts on valid non-doorway points, with real projectiles after the warning and cleanup on room exit.
- Natural investigation props and planted/unplanted seedbed sprites load correctly.

Native screenshots: `runtime/naki-world-map.png`, `runtime/forest-seedbed.png`, plus the inherited regression screenshots. Native logs and final counts are in `native-unity.log` and `runtime/smoke-result.txt`. Automatic state/rendering checks are not a complete human difficulty playthrough.

`finalize.py` preserves Unity-generated GUIDs and prepares the guarded sync. `sync_runtime.py` checks for independently changed local files, backs up replaced runtime files and verifies copied bytes. Existing scenes and real save files are excluded.
