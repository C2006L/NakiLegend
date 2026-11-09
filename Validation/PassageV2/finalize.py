from pathlib import Path
import shutil
root=Path(__file__).resolve().parents[2]
qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
rels=['NakiPrototype.PassageArt.cs','NakiPrototype.JourneySession.cs','Editor/NakiProgressReset.cs','Resources/NakiArt/PassageV2']
rels += ['Resources/NakiArt/PassageV2/'+name for name in ['island-gates.png','storm-props.png','rain-harbor.png','coastal-harbor.png']]
for rel in rels:
 source=qa/'Assets/NakiPrototype'/(rel+'.meta');destination=root/'Assets/NakiPrototype'/(rel+'.meta')
 assert source.exists(),source
 if not destination.exists():shutil.copy2(source,destination)
 else:assert source.read_bytes()==destination.read_bytes(), 'Preserve stable GUID: '+rel
script=(root/'Validation/VoyagerStory/sync_runtime.py').read_text(encoding='utf-8').replace('Validation/VoyagerStory/runtime/pre-story-sync','Validation/PassageV2/runtime/pre-passage-sync')
(root/'Validation/PassageV2/sync_runtime.py').write_text(script,encoding='utf-8')
print('Preserved passage art and script metadata; guarded sync prepared')
