from pathlib import Path
import shutil

root = Path(__file__).resolve().parents[2]
qa = root.parent / 'Validation/DesertV2/UnitySmokeProject'
rel = Path('Assets/NakiPrototype/NakiPrototype.VoyagerStory.cs.meta')
source = qa / rel
destination = root / rel
assert source.exists(), source
if not destination.exists():
    shutil.copy2(source, destination)
else:
    assert source.read_bytes() == destination.read_bytes(), 'Preserve established script GUID'

script = (root / 'Validation/QuayMotion/sync_runtime.py').read_text(encoding='utf-8')
script = script.replace('Validation/QuayMotion/runtime/pre-quay-sync', 'Validation/VoyagerStory/runtime/pre-story-sync')
(root / 'Validation/VoyagerStory/sync_runtime.py').write_text(script, encoding='utf-8')
print('Preserved VoyagerStory GUID; prepared guarded runtime sync')
