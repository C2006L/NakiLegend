# V18 profession and lightning validation

Runtime source compile uses the existing Unity Roslyn response file in Validation/DesertV12.
Smoke.cs runs in the isolated UnitySmokeProject, never in the user's active Unity project.
The harness extends V17 narrative, ending, layout, checkpoint, and weapon regression coverage with profession selection, mage spending, archer rate, swap recovery, warrior one-hit dash, strike warning timing/radius, impact boundary, and profession save round-trip tests.

Native entry: NakiProfessionV18Smoke.Run. Results and screenshots are generated under runtime/ (ignored).
Deliver.ps1 validates destinations against Git HEAD or current source before copying only the changed runtime scripts into the actual project and legacy runtime distribution. It does not modify scenes or player save JSON.
