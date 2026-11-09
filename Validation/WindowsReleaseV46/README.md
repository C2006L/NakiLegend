# Windows portable release verification

Unity 2022.3.62f3 / Windows x64 / Mono / built-in 2D rendering.

Build with `NakiLegend.Prototype.Editor.NakiWindowsBuild.Build` (see root README).
The generated scene starts at island 1 with opening enabled, no developer profile, no practice flags.

Run the actual packaged executable in a separate process:

```powershell
NakiLegend.exe -batchmode -screen-width 1600 -screen-height 900 -nakiSmokeReport <absolute-workspace-report.txt> -logFile <absolute-workspace-player.log>
```

The opt-in check verifies the first-island entry, the packaged sprite shader, pause/replay menu and all 21 authored boss sprites/HP across seven islands. It writes a report and camera render, then exits. In batch mode it does not write gameplay checkpoints or ending collection. Normal launches do not execute this check.

Editor regression references: `Validation/PauseReplayV46`, `Validation/ReplayV45`, `Validation/ReturnV44`, `Validation/BalanceV43`.
