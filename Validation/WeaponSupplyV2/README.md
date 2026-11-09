# Weapon Supply V2 validation

Unity 2022.3.62f3 native regression: **PASS 3229 checks**. Runtime C# compilation passes with the existing optional test-directory CS0649 warning.

The suite derives from the frozen FiveIslandPressureV2 harness and keeps all existing story, combat, boss-only reward, inventory, saves and difficulty checks. Added coverage samples 450 cache combinations across five islands, three professions and thirty seeds: exactly one melee, one staff and one bow family; profession-first placement for fresh caches; no future-island or signature leakage; concrete weapon diversity; no reroll after reopening or changing profession.

It also checks minimal repair of ranged-only and melee-only unclaimed cached offers, unchanged claimed offers, saved offer persistence, actual melee descriptions and three sets of 100 ordinary drop samples. All three samples match the selected profession 71 times, consistent with the 70% branch. Native Fire and Forest supply screenshots were reviewed for card readability.

Run `build.py`, then `NakiWeaponSupplyV2Smoke.Run` in the isolated Unity smoke project. `sync_runtime.py` performs the guarded backed-up copy into actual and legacy projects without editing player saves. No new weapon IDs, graphics or account growth data are introduced.
