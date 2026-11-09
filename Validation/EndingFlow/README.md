# Ice ending-flow regression

`build.py` copies the current runtime into the isolated Unity smoke project and adds the focused regression to the established Arsenal harness. It never uses player save files; voyages use `runtime/save/voyage` and the harness assigns a unique validation product name.

Run `python Validation/EndingFlow/build.py`, then Unity batch mode with `-executeMethod NakiEndingFlowSmoke.Run` against `D:/NakiLegend/Validation/DesertV2/UnitySmokeProject`.

Covered cases:

- Boss victory alone cannot unlock CG or talent; real rescue, sluice and return-light sequence opens the existing illustrated Ice finale.
- Pending CG survives a disk reload; completed CG gates talents and remains on Ice.
- Talent selection neither changes island nor queues a trip; world interactions wait for release and debounce.
- Return/forward travel require a separate destination confirmation; cancel retains Ice; the opening press cannot immediately confirm.
- Legacy completed Ice JSON without `iceCgSeen` replays CG once, retains the existing talent and does not duplicate rewards or loop on subsequent reload.
- Deliberate forward voyage creates a separate Fire checkpoint and carries the Ice talent; returning from Fire retains CG completion.
- Deliberate backtracking loads the separate Thunder safety point only after confirmation.
- Old Ice sample cannot masquerade as completed full chapter.
- Existing Arsenal balance, checkpoint, encounter and talent regressions run in the same session.

Native screenshots: `runtime/ice-ending-cg.png`, `runtime/ice-voyage-confirm.png`, `runtime/ice-after-talent.png`. Result and Unity log are generated locally. These tests check state transitions and actual UI rendering; the original user's exact key sequence was not reproduced.

Final Unity 2022.3.62f3 run: **PASS 439 checks**, process exit 0, no captured runtime errors. Roslyn compilation also passed (the existing reflection-assigned batch save directory emits CS0649).
