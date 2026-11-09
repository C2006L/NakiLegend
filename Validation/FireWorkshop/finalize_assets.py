from pathlib import Path
root=Path(__file__).resolve().parents[2]
qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
paths=['Assets/NakiPrototype/NakiPrototype.FireWorkshop.cs.meta','Assets/NakiPrototype/NakiPrototype.ForgeCombat.cs.meta','Assets/NakiPrototype/Resources/NakiArt/FireWorkshop.meta','Assets/NakiPrototype/Resources/NakiArt/FireWorkshop/creatures.png.meta','Assets/NakiPrototype/Resources/NakiArt/FireWorkshop/environment.png.meta']
for name in ['NakiFireWorkshop','NakiForgeBossPractice']:
    paths+=['Assets/Scenes/'+name+'.unity','Assets/Scenes/'+name+'.unity.meta']
for relative in paths:
    dst=root/relative
    if dst.exists():continue
    source=qa/relative
    assert source.is_file(),source
    data=source.read_text(encoding='utf-8-sig')
    data='\n'.join(line.rstrip() for line in data.splitlines())+'\n'
    dst.parent.mkdir(parents=True,exist_ok=True)
    dst.write_text(data,encoding='utf-8')
for name in ['NakiFireWorkshop','NakiForgeBossPractice']:
    assert 'guid: d922c3c6bbc91794ea00f6c8c00f1911' in (root/('Assets/Scenes/'+name+'.unity')).read_text(encoding='utf-8')
print('Workshop scenes and original Unity metadata are ready')
