# Ice sample native integration validation

Unity 2022.3.62f3, isolated project `D:/NakiLegend/Validation/DesertV2/UnitySmokeProject`.

`Smoke.cs` extends the established regression harness with the ice map, armor, spells, phase transitions and checkpoint/voyage checks. `build_smoke.py` reproducibly derives it from `Validation/RevisionA/Smoke.cs`. Copy the runtime sources/resources and harness to the isolated QA project, then run `-batchmode -noUpm -disable-assembly-updater -executeMethod NakiIceSampleSmoke.Run`. Do not use the user's live scene for this automation.

Native result: **PASS, 524 checks**, including actual HUD rendering without logged runtime exceptions. The initial run caught a test fixture that changed profession on an already-equipped warrior but incorrectly expected starting-archer gear; the fixture now explicitly equips its bow before testing voyage preservation. Production equipment-selection behavior was retained. A final cross-island check also verifies that returning without a previous thunder save clears ice cooldowns before starting a new thunder journey.

New checks include:

- Three-room identity, reciprocal doors and safe arrival positions; three corridor waves.
- Crystal-armor reduction, lightning cracks and stagger window; soul eligibility.
- Charge/ground-impact warnings lock before damage; half-health armor phase.
- Boss defeat leaves the journal investigation; ice skills cannot be collected early.
- Ice-spike resource/CD/audio tag and collision slow; frost-ring cost/slow/transition cleanup.
- Twelve imported spell audio variants.
- Independent ice saves, learned/equipped skills, corrupt-primary backup recovery and practice write protection.
- Thunder rejects ice namespace, and voyage preserves player origin, soul reserve, weapon upgrades, gear and profession.
- Existing desert canal ending, thunder/woodland progression, old save normalization, dark rebalance, cage rescue, professions, audio, and multiple HUD sizes.

Ignored `runtime/` contains the result and actual HD screenshots: `ice-harbor.png`, `ice-corridor.png`, `ice-beast.png`, `ice-skills.png`, plus the existing regression captures. New ice captures were visually inspected: no neighboring atlas subjects in sprites, complete horns/feet, readable warning lane, six skills visible inside the menu. The reused mossy highland wall is documented as provisional art.
