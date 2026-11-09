# Mingsha Polish V2

Native Unity 2022.3.62f3: PASS 1814 checks. Run `build.py`, then `NakiMingshaPolishV2Smoke.Run` in the isolated smoke project. Builds from the prior OriginCombatV2 harness, preserving its five-island regressions and adding first-island-only tuning, dry bank doorways, pier non-transition, idempotent old-layout migration, checkpoint roundtrip and actual warrior audio playback checks.

Native screenshots reviewed: `runtime/rain-quay-banks.png`, `runtime/inspection-court.png`. Existing data and GUIDs are preserved. `finalize.py` copies three imported audio GUIDs and prepares a guarded sync with backup. No real player progress files are reset or edited by the tests or sync.

Three dash assets are original generated 22050 Hz mono PCM, 0.24 seconds, with deterministic filtered noise, short low-frequency impact and a decaying armor transient; no third-party samples.
