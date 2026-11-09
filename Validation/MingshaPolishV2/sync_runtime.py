"""Sync audited five-island ending routes and objective guidance; preserve existing scenes and player saves."""
from pathlib import Path
import subprocess,shutil,json,hashlib
repo=Path(__file__).resolve().parents[2]
workspace=repo.parent
actual=workspace/'UnityPrototype/Assets/NakiPrototype/NakiLegend'
legacy=workspace/'UnityPrototype'
backup=repo/'Validation/MingshaPolishV2/runtime/pre-mingsha-sync'
def git(*args):return subprocess.run(['git',*args],cwd=repo,check=True,capture_output=True,text=True).stdout.splitlines()
files=git('diff','--name-only')+git('ls-files','--others','--exclude-standard')
files=sorted({f for f in files if f.startswith('Assets/NakiPrototype/')},key=lambda f:(not f.endswith('.meta'),f))
def normalized(p):
    data=p.read_bytes()
    try:return '\n'.join(line.rstrip() for line in data.decode('utf-8-sig').replace('\r\n','\n').splitlines()).encode()
    except UnicodeDecodeError:return data
assert (actual/'ProjectSettings/ProjectVersion.txt').exists()
assert 'd922c3c6bbc91794ea00f6c8c00f1911' in (actual/'Assets/NakiPrototype/NakiPrototype.cs.meta').read_text()
jobs=[]
for rel in files:
    src=repo/rel
    for target in [actual,legacy]:
        if target==legacy and rel.startswith('Assets/Scenes/'):continue
        dst=target/rel
        assert dst.resolve().is_relative_to(workspace.resolve())
        if dst.exists() and normalized(dst)==normalized(src):continue
        if dst.exists():
            if rel.startswith('Assets/Scenes/'):
                raise RuntimeError('Preserved local scene: '+str(dst))
            peer=(legacy if target==actual else actual)/rel
            assert peer.exists() and normalized(dst)==normalized(peer),'Preserved independent local edit: '+str(dst)
        jobs.append((src,dst,rel,'actual' if target==actual else 'legacy'))
manifest=[]
for src,dst,rel,label in jobs:
    if dst.exists():
        previous=backup/label/rel
        previous.parent.mkdir(parents=True,exist_ok=True)
        if not previous.exists():shutil.copy2(dst,previous)
    dst.parent.mkdir(parents=True,exist_ok=True)
    shutil.copy2(src,dst)
    assert src.read_bytes()==dst.read_bytes()
    manifest.append({'destination':str(dst),'sha256':hashlib.sha256(dst.read_bytes()).hexdigest()})
backup.parent.mkdir(parents=True,exist_ok=True)
if manifest:(backup.parent/'sync-manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
print(f'Synced and byte-verified {len(manifest)} runtime/new scene files; existing scenes preserved')
