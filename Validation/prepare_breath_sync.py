from pathlib import Path
r=Path('D:/NakiLegend/Repository');s=(r/'Validation/sync_forest_ending.py').read_text(encoding='utf-8-sig').replace('ForestEndingV7','BossBreathingV8').replace('forest-ending-files.txt','boss-breath-files.txt');(r/'Validation/sync_boss_breath.py').write_text(s,encoding='utf-8')
