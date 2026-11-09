from pathlib import Path
import shutil
root=Path(__file__).resolve().parents[2]
for i in range(3):
 rel=f'Assets/NakiPrototype/Resources/NakiCombatAudioV3/warrior_dash_{i}.wav.meta'
 src=root.parent/'Validation/DesertV2/UnitySmokeProject'/rel;dst=root/rel
 assert src.exists()
 if dst.exists():assert src.read_bytes()==dst.read_bytes()
 else:shutil.copy2(src,dst)
s=(root/'Validation/OriginCombatV2/sync_runtime.py').read_text(encoding='utf-8').replace('Validation/OriginCombatV2/runtime/pre-origin-sync','Validation/MingshaPolishV2/runtime/pre-mingsha-sync')
(root/'Validation/MingshaPolishV2/sync_runtime.py').write_text(s,encoding='utf-8')
print('Prepared stable audio GUIDs and guarded runtime sync')
