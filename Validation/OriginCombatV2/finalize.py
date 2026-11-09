"""Preserve imported Unity GUIDs and prepare guarded runtime sync; never touch saves."""
from pathlib import Path
import shutil

root = Path(__file__).resolve().parents[2]
qa = root.parent / 'Validation/DesertV2/UnitySmokeProject/Assets/NakiPrototype'
assets = root / 'Assets/NakiPrototype'
new_assets = ['NakiPrototype.OriginCombat.cs']
new_assets += ['Resources/NakiCombatAudioV3/' + p.name for p in (assets / 'Resources/NakiCombatAudioV3').glob('*.wav') if p.name.startswith(('skill_lightbeam_', 'skill_lightguard_', 'skill_solar_', 'skill_soulseal_', 'light_channel_loop'))]
assert len(new_assets) == 26
for rel in new_assets:
    source, destination = qa / (rel + '.meta'), assets / (rel + '.meta')
    assert source.exists(), source
    if destination.exists():
        assert source.read_bytes() == destination.read_bytes(), 'Preserve stable GUID: ' + rel
    else:
        shutil.copy2(source, destination)
script = (root / 'Validation/PassageV2/sync_runtime.py').read_text(encoding='utf-8')
script = script.replace('Validation/PassageV2/runtime/pre-passage-sync', 'Validation/OriginCombatV2/runtime/pre-origin-sync')
(root / 'Validation/OriginCombatV2/sync_runtime.py').write_text(script, encoding='utf-8')
print('Preserved 26 imported script/audio GUIDs; guarded sync prepared without save changes')
