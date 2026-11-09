from pathlib import Path
import subprocess,difflib
repo=Path(__file__).resolve().parents[2]
workspace=repo.parent
actual=workspace/'UnityPrototype/Assets/NakiPrototype/NakiLegend'
legacy=workspace/'UnityPrototype'
files=subprocess.check_output(['git','diff','--name-only'],cwd=repo,text=True).splitlines()+subprocess.check_output(['git','ls-files','--others','--exclude-standard'],cwd=repo,text=True).splitlines()
def normalized(p):
    data=p.read_bytes()
    try:return '\n'.join(line.rstrip() for line in data.decode('utf-8-sig').replace('\r\n','\n').splitlines()).encode()
    except UnicodeDecodeError:return data
for name in sorted(set(files)):
    if not name.startswith('Assets/NakiPrototype/') or name.endswith('.meta'):continue
    a=actual/name;b=legacy/name;src=repo/name
    if a.exists() and normalized(a)!=normalized(src):
        same=b.exists() and normalized(a)==normalized(b)
        print(f'{name}: actual/legacy identical={same}')
        if src.suffix=='.cs':
            diff=list(difflib.unified_diff(a.read_text(encoding='utf-8-sig').splitlines(),src.read_text(encoding='utf-8-sig').splitlines(),n=0))
            print('\n'.join(line for line in diff if line.startswith('@@')))
