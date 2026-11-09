from pathlib import Path
import re,shutil
root=Path(__file__).resolve().parents[2]
qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
paths=[Path('Assets/NakiPrototype/NakiPrototype.FireCoast.cs.meta'),Path('Assets/NakiPrototype/NakiPrototype.FireCombat.cs.meta'),Path('Assets/NakiPrototype/Resources/NakiArt/FireCoast.meta')]
paths+=list(Path('Assets/NakiPrototype/Resources/NakiArt/FireCoast')/name for name in ['creatures.png.meta','environment.png.meta','harbor.png.meta'])
paths+=list(Path('Assets/Scenes')/name for name in ['NakiFireCoast.unity','NakiFireCoast.unity.meta','NakiFireBossPractice.unity','NakiFireBossPractice.unity.meta'])
for rel in paths:
    src=qa/rel;dst=root/rel
    assert src.is_file(),src
    if dst.exists():continue
    dst.parent.mkdir(parents=True,exist_ok=True)
    data=src.read_text(encoding='utf-8-sig')
    data='\n'.join(line.rstrip() for line in data.splitlines())+'\n'
    dst.write_text(data,encoding='utf-8')
for rel in [Path('Assets/Scenes/NakiFireCoast.unity'),Path('Assets/Scenes/NakiFireBossPractice.unity')]:
    assert 'guid: d922c3c6bbc91794ea00f6c8c00f1911' in (root/rel).read_text(encoding='utf-8')
print('New scene and asset GUIDs preserved from Unity serializer')
