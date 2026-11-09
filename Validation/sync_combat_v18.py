from pathlib import Path
import shutil, json
r=Path('D:/NakiLegend/Repository')
out=r/'Validation/CombatV18'
paths=set((r/'Validation/combat-v18-files.txt').read_text(encoding='utf-8').splitlines())
targets=[r.parent/'UnityPrototype',r.parent/'UnityPrototype/Assets/NakiPrototype/NakiLegend']
records=[]
def content(p):
    if p.suffix=='.png': return p.read_bytes()
    return '\n'.join(x.rstrip() for x in p.read_text(encoding='utf-8-sig').splitlines())
# Check every destination before writing any files.
for rel in sorted(paths):
    src=r/rel
    assert src.is_file(),str(src)
    for i,t in enumerate(targets):
        dst=t/rel; peer=targets[1-i]/rel
        assert dst.resolve().is_relative_to((r.parent/'UnityPrototype').resolve())
        if dst.exists() and peer.exists():
            assert content(dst)==content(src) or content(dst)==content(peer) or content(peer)==content(src),'Independent edit: '+str(dst)
for rel in sorted(paths):
    src=r/rel
    for i,t in enumerate(targets):
        dst=t/rel
        if dst.exists() and content(dst)==content(src): continue
        if dst.exists():
            bak=out/'backup'/str(i)/rel
            bak.parent.mkdir(parents=True,exist_ok=True)
            if not bak.exists(): shutil.copy2(dst,bak)
        dst.parent.mkdir(parents=True,exist_ok=True)
        shutil.copy2(src,dst)
        assert src.read_bytes()==dst.read_bytes()
        records.append(str(dst))
(out/'sync.json').write_text(json.dumps(records,ensure_ascii=False,indent=2),encoding='utf-8')
print('Verified synced files:',len(records),'; scenes and player saves untouched')
