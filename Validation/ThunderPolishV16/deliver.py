"""Deliver this patch after verifying both destinations still match HEAD or source."""
from pathlib import Path
import hashlib
import json
import shutil
import subprocess

repo = Path(__file__).resolve().parents[2]
runtime = repo / 'Assets/NakiPrototype'
roots = [Path('D:/NakiLegend/UnityPrototype/Assets/NakiPrototype/NakiLegend/Assets/NakiPrototype'),
         Path('D:/NakiLegend/UnityPrototype/Assets/NakiPrototype')]
files = ['NakiPrototype.Enemies.cs', 'Resources/NakiArt/ThunderChapter/rooms.json']
copies = [(runtime / rel, root / rel) for rel in files for root in roots]
def normalized(data):
    return data.replace(b'\r\n', b'\n')
for src, dst in copies:
    if not dst.exists() or normalized(dst.read_bytes()) == normalized(src.read_bytes()):
        continue
    head = subprocess.run(['git', 'show', 'HEAD:' + src.relative_to(repo).as_posix()], cwd=repo, capture_output=True)
    if head.returncode or normalized(head.stdout) != normalized(dst.read_bytes()):
        raise RuntimeError('Preserved unexpected destination edits: ' + str(dst))
for src, dst in copies:
    dst.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(src, dst)
    assert src.read_bytes() == dst.read_bytes(), dst
report = {'copied': len(copies), 'verified_hashes': {
    str(src.relative_to(repo)): hashlib.sha256(src.read_bytes()).hexdigest() for src, _ in copies},
    'note': 'Only enemy death code and second-island wave data changed. Scenes and saves preserved.'}
(repo / 'Validation/ThunderPolishV16/delivery.json').write_text(json.dumps(report, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
print('Verified and copied', len(copies), 'files to the actual Unity project and legacy distribution.')
