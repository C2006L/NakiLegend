"""Sync only runtime/editor scripts after checking destinations against Git HEAD."""
from pathlib import Path
import hashlib
import json
import shutil
import subprocess

repo = Path(__file__).resolve().parents[2]
runtime = repo / 'Assets/NakiPrototype'
actual = Path('D:/NakiLegend/UnityPrototype/Assets/NakiPrototype/NakiLegend')
legacy = Path('D:/NakiLegend/UnityPrototype/Assets/NakiPrototype')
report_path = repo / 'Validation/AdventureV15/delivery.json'
previous = json.loads(report_path.read_text('utf-8')).get('verified_hashes', {}) if report_path.exists() else {}

def normalized(data):
    return data.replace(b'\r\n', b'\n')

sources = sorted({f for pattern in ['*.cs', '*.cs.meta', 'Editor/*.cs', 'Editor/*.cs.meta']
                  for f in runtime.glob(pattern) if f.is_file()})
copies = [(src, root / src.relative_to(runtime)) for src in sources
          for root in [actual / 'Assets/NakiPrototype', legacy]]
conflicts = []
for src, dst in copies:
    if not dst.exists() or normalized(dst.read_bytes()) == normalized(src.read_bytes()):
        continue
    if hashlib.sha256(dst.read_bytes()).hexdigest() == previous.get(str(src.relative_to(repo))):
        continue
    head = subprocess.run(['git', 'show', 'HEAD:' + src.relative_to(repo).as_posix()],
                          cwd=repo, capture_output=True)
    if head.returncode or normalized(head.stdout) != normalized(dst.read_bytes()):
        conflicts.append(str(dst))
if conflicts:
    raise RuntimeError('Preserved unexpected destination edits: ' + json.dumps(conflicts, ensure_ascii=False))
main_meta = runtime / 'NakiPrototype.cs.meta'
assert 'guid: d922c3c6bbc91794ea00f6c8c00f1911' in main_meta.read_text('utf-8')
for src, dst in copies:
    dst.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(src, dst)
for src, dst in copies:
    assert src.read_bytes() == dst.read_bytes(), dst
report = {
    'copied': len(copies),
    'actual_project': str(actual),
    'main_script_guid': 'd922c3c6bbc91794ea00f6c8c00f1911',
    'verified_hashes': {str(src.relative_to(repo)): hashlib.sha256(src.read_bytes()).hexdigest()
                        for src in sources},
    'note': 'Checked destinations against HEAD, current source or previous verified delivery before copying. Scenes, artwork and user saves untouched.'
}
report_path.write_text(
    json.dumps(report, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
print('Verified and copied', len(copies), 'script/metadata files to actual Unity project and legacy distribution.')
