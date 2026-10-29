"""Sync verified changed runtime assets without replacing user-edited scenes."""
from pathlib import Path
import subprocess,shutil

repo=Path(__file__).resolve().parents[2]
workspace=repo.parent
actual=workspace/'UnityPrototype/Assets/NakiPrototype/NakiLegend'
legacy=workspace/'UnityPrototype'
assert (actual/'ProjectSettings/ProjectVersion.txt').exists(),actual
changed=subprocess.check_output(['git','diff','--name-only'],cwd=repo,text=True).splitlines()
changed+=subprocess.check_output(['git','ls-files','--others','--exclude-standard'],cwd=repo,text=True).splitlines()
assets=[f for f in changed if f.startswith('Assets/NakiPrototype/') or f.startswith('Assets/Scenes/NakiIce')]
conflicts=[];copied=[]
def normalized(data):
    try:return '\n'.join(line.rstrip() for line in data.decode('utf-8-sig').replace('\r\n','\n').split('\n')).encode()
    except UnicodeDecodeError:return data
for target in [actual,legacy]:
    for relative in sorted(set(assets)):
        if target==legacy and relative.startswith('Assets/Scenes/'):continue
        src=repo/relative;dst=target/relative
        assert dst.resolve().is_relative_to(workspace.resolve())
        data=src.read_bytes()
        if dst.exists() and normalized(dst.read_bytes())!=normalized(data):
            old=subprocess.run(['git','show','HEAD:'+relative],cwd=repo,capture_output=True)
            if old.returncode!=0 or normalized(dst.read_bytes())!=normalized(old.stdout):
                conflicts.append(str(dst));continue
        dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dst)
        assert dst.read_bytes()==data
        copied.append(str(dst))
print(f'Synced and byte-verified {len(copied)} runtime/scene files')
if conflicts:
    print('Preserved local differences:');print('\n'.join(conflicts));raise SystemExit(1)
