# Fire Core native regression

Prepare with `python Validation/FireCore/build.py`. Execute Unity 2022.3.62f3 against the isolated `D:/NakiLegend/Validation/DesertV2/UnitySmokeProject` with `-batchmode -noUpm -disable-assembly-updater -executeMethod NakiFireCoreSmoke.Run` and a dedicated log file. The harness assigns a unique validation product name and uses workspace-local voyage/checkpoint paths; it never accesses actual player saves.

Final run: **PASS 672 checks**, process exit 0, no captured runtime errors. This includes the established Arsenal and Ice ending regressions, plus:

- 45-room graph, distinct third-floor cells, reciprocal cardinal doors and both staircase directions.
- All third-floor combat rooms, four-wave structure, increased spawn count with ranged/construct caps, 1.4-second reinforcement warnings, safe arrivals and paths to exits.
- Magma Tortoise identity/health, paired 40/48-projectile volleys, delayed pulses, open-sector ring and Ice shell break.
- All three professions × both Light/Dark paths using inherited Ice skills in the third floor.
- Exact legacy 30-room JSON migration preserving old room/resources/Forge victory and the original `.bak`, without awarding new-room completion.
- Both actual evacuation sections, residents walking during dialogue and leaving through the safe exit, harbor hearth interaction, CG pending reload, talent ordering/persistence and rejection of an incomplete ending checkpoint.
- Deliberate Forest voyage, independent saves, inherited shield cap 8, talent ledger and selectable/castable Fire/Ice skill IDs after reload.
- Twelve Boss kinds across existing islands: no early drop, one fixed 40-element crystal on the first surviving one-third threshold crossing, no duplicate from subsequent damage, no ordinary-enemy phase supply, no talent amplification, and partial pickup retaining excess on the ground.
- Actual native rendering of the third-floor room, guardian burst, ending CG and talent cards.

Screenshots: `runtime/core-encounter.png`, `runtime/magma-tortoise.png`, `runtime/fire-hearth-cg.png`, `runtime/fire-talent.png`. Result: `runtime/smoke-result.txt`. Logs, screenshots and fixture saves are generated artifacts. The checks establish functional behavior; combat difficulty remains subject to human playtesting.
