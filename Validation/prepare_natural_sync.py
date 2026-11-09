from pathlib import Path
r=Path('D:/NakiLegend/Repository');s=(r/'Validation/sync_forest_ending.py').read_text(encoding='utf-8-sig').replace('ForestEndingV7','NaturalRecoveryV10').replace('forest-ending-files.txt','natural-recovery-files.txt');(r/'Validation/sync_natural_recovery.py').write_text(s,encoding='utf-8')
