from pathlib import Path
r=Path('D:/NakiLegend/Repository');s=(r/'Validation/sync_forest_ending.py').read_text(encoding='utf-8-sig').replace('ForestEndingV7','BossOffenseV9').replace('forest-ending-files.txt','boss-offense-files.txt');(r/'Validation/sync_boss_offense.py').write_text(s,encoding='utf-8')
