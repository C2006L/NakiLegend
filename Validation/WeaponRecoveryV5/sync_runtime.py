"""Sync this turn's scripts to both local Unity copies, preserving scene and save files."""
from pathlib import Path
import hashlib,json,shutil
repo=Path(__file__).resolve().parents[2];workspace=repo.parent
actual=workspace/'UnityPrototype/Assets/NakiPrototype/NakiLegend';legacy=workspace/'UnityPrototype'
out=repo/'Validation/WeaponRecoveryV5/runtime';assert (actual/'ProjectSettings/ProjectVersion.txt').exists()
names=['CombatBalanceV4','OriginWeapons','Backpack','CombatV3','ManorCombat','WoodlandV2','EncounterRefresh','Relics','TacticalProps','CombatMastery','SkillLoadout','Journey','OriginLegacy','OriginCombat','Starfall','JourneyHud','DivineLight','CombatPolish','Origin','VoyagerStory','PixelHero','']
def normalize(p):return '\n'.join(line.rstrip() for line in p.read_text(encoding='utf-8-sig').splitlines())
jobs=[]
for name in names:
 for suffix in ['.cs.meta','.cs']:
  rel=Path('Assets/NakiPrototype/NakiPrototype'+('.'+name if name else '')+suffix);src=repo/rel;assert src.exists()
  for target,label in [(actual,'actual'),(legacy,'legacy')]:
   dst=target/rel;assert dst.resolve().is_relative_to(workspace.resolve())
   if dst.exists() and normalize(dst)==normalize(src):continue
   if dst.exists():
    peer=(legacy if target==actual else actual)/rel
    assert peer.exists() and normalize(dst)==normalize(peer),'Preserved independent script edit: '+str(dst)
   jobs.append((src,dst,label,rel))
records=[]
for src,dst,label,rel in jobs:
 if dst.exists():
  backup=out/'pre-v5-sync'/label/rel;backup.parent.mkdir(parents=True,exist_ok=True)
  if not backup.exists():shutil.copy2(dst,backup)
 dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dst);assert dst.read_bytes()==src.read_bytes()
 records.append({'destination':str(dst),'sha256':hashlib.sha256(dst.read_bytes()).hexdigest()})
out.mkdir(exist_ok=True)
if records:(out/'sync-manifest.json').write_text(json.dumps(records,ensure_ascii=False,indent=2),encoding='utf-8')
print('Synced',len(records),'verified runtime files; scenes and player saves preserved')
