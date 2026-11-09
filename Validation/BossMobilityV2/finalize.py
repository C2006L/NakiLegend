from pathlib import Path
import shutil
root=Path(__file__).resolve().parents[2]
rel='Assets/NakiPrototype/NakiPrototype.BossFootwork.cs.meta'
src=root.parent/'Validation/DesertV2/UnitySmokeProject'/rel;dst=root/rel
assert src.exists(),src
if dst.exists():assert src.read_bytes()==dst.read_bytes()
else:shutil.copy2(src,dst)
print('Preserved imported BossFootwork script GUID')
