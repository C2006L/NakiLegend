# Route Clarity V3 validation

Unity 2022.3.62f3 native play-mode regression completed with **PASS 10008 checks**. See `runtime/smoke-result.txt`.

This suite retains the prior weapon, save, voyage, enemy and boss regressions and adds:

- All 42 ice rooms, their waves, three bosses, rescue, sluice and ending flow.
- Fourteen connected rooms per floor, reciprocal cardinal exits, one boss/camp/cache per floor, and minimum boss distances of 8 / 8 / 9 transitions.
- Staggered second-floor combat composition, actual journal/winch interactions, once-only camp supplies and removal of the frost boss arrow board.
- Canonical five-island layer names and actual forward/reverse Fire stair destinations.
- Wall-side Fire shelter with civilians hidden during combat and safely visible after rescue.
- Soul-seal initial damage, actual melee and bow detonation, proc spacing and expiry.
- Migration of the existing 42-room ice checkpoint without losing health, equipped weapon or completed boss state.

Five PNG captures in `runtime` were visually inspected. These checks establish behavior and regression coverage; encounter enjoyment and final balance still need player testing.

`sync_runtime.py` copies scripts and script metadata only, first checking for independent local edits. Existing files are backed up, copied bytes are verified, scenes and player saves are preserved, and texture/audio importer settings are excluded. The sync manifest records changed destinations and SHA-256 hashes.
