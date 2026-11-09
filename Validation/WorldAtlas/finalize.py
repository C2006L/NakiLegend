from pathlib import Path
import shutil
root=Path(__file__).resolve().parents[2]
qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
paths=['NakiPrototype.WoodlandFinish.cs','NakiPrototype.WorldAtlas.cs','NakiPrototype.WorldAtlasHud.cs','Resources/NakiArt/WoodlandV3','Resources/NakiArt/WoodlandV3/quest-props.png','Resources/NakiArt/WorldAtlas','Resources/NakiArt/WorldAtlas/naki-region.png']
for rel in paths:
 src=qa/'Assets/NakiPrototype'/(rel+'.meta');dst=root/'Assets/NakiPrototype'/(rel+'.meta')
 assert src.exists(),src
 if not dst.exists():shutil.copy2(src,dst)
sync=(root/'Validation/FlowAudit/sync_runtime.py').read_text(encoding='utf-8').replace('Validation/FlowAudit/runtime/pre-flow-sync','Validation/WorldAtlas/runtime/pre-atlas-sync')
(root/'Validation/WorldAtlas/sync_runtime.py').write_text(sync,encoding='utf-8')
print('Preserved Unity-generated asset identities; prepared guarded sync')
