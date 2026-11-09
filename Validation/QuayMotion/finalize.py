from pathlib import Path
import shutil
root=Path(__file__).resolve().parents[2];qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
for rel in ['NakiPrototype.QuayNavigation.cs','NakiPrototype.EnemyMotion.cs','Resources/NakiArt/QuayPolish','Resources/NakiArt/QuayPolish/rain-quay.png']:
 src=qa/'Assets/NakiPrototype'/(rel+'.meta');dst=root/'Assets/NakiPrototype'/(rel+'.meta')
 assert src.exists(),src
 if not dst.exists():shutil.copy2(src,dst)
s=(root/'Validation/WorldAtlas/sync_runtime.py').read_text(encoding='utf-8').replace('Validation/WorldAtlas/runtime/pre-atlas-sync','Validation/QuayMotion/runtime/pre-quay-sync')
(root/'Validation/QuayMotion/sync_runtime.py').write_text(s,encoding='utf-8')
print('Unity GUIDs preserved; guarded harbor/motion sync prepared')
