# Weapon identity V3 validation

Native Unity 2022.3.62f3: **PASS 9529 checks**. Standalone C# compilation succeeds; only the pre-existing optional checkpoint-test-directory CS0649 warning remains. Full native result: `runtime/smoke-result.txt`.

The preceding BossMobilityV2 five-island, seventeen encounter families/three phases, route, save, supply, Light/stone ability and rendering audits are retained. Legacy assertions that specifically required the old ranged atlas or the old staff sound key are updated to the new identity mapping. The old Wood-staff test now resolves an actual hit and checks poison duration, rather than inferring poison from an audio key.

The new audit checks all 132 names, held/bag/drop sprite regions, normalized size and per-ID projectile pixel hashes; all 264 unique attack/contact audio files; actual weapon input handlers and projectile source IDs. It checks exactly 20% lower existing Wind-staff volley damage, nine weapons spending only the declared resource once per action, opposite-route and empty-pool rejection, source retention after switching during delayed fire, Wood poison, dagger fourth-contact bonus, soul crossbow marking/consumption, ordinary-enemy-only scythe bonus, two tracking soul flames, single piercing sun lance, charged star-arrow penetration and the three-bolt prism fan.

New IDs 123–131 and old ID 93 are saved and loaded through actual six-slot chapter validation alongside an uncapped 1195-point stone reserve. Supply candidates exclude the opposite route. The gallery's native audio audition API is verified available in this Unity version. Captures include `needle-staff.png`, `lantern-staff.png`, `light-staff.png` and `sage-lantern.png`.

Raster analysis in `Tools/WeaponIdentityV3/prepare.py` reads alpha/bounds and checks unique hashes; Unity slices the preserved generated PNGs. `Art/WeaponIdentityV3/audit.json` records all ten source atlas hashes, and `audio-audit.json` records 264 source combinations, durations and signal levels. No copyrighted game sounds were downloaded.

Run `build.py`, then `NakiWeaponIdentityV3Smoke.Run` in the isolated smoke project. Each run uses a separate validation product and test save paths. `sync_runtime.py` backs up replaced runtime files, rejects independent local edits, preserves scenes and verifies destination bytes. It never clears player progress.

These are scripted behavioral checks, not a player difficulty or audio preference study. The user can compare appearance and audition the voices in `Tools → Naki Legend → Design → Weapon Gallery`.
