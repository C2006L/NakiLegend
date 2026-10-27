"""Verify destinations against Git HEAD before syncing the playable delivery."""
from pathlib import Path
import subprocess,shutil,json,hashlib
repo=Path.cwd()
qa=Path('D:/NakiLegend/Validation/DesertV2/UnitySmokeProject')
actual=Path('D:/NakiLegend/UnityPrototype/Assets/NakiPrototype/NakiLegend')
legacy=Path('D:/NakiLegend/UnityPrototype/Assets/NakiPrototype')
def normalized(data):return data.replace(b'\r\n',b'\n')
def sha(data):return hashlib.sha256(data).hexdigest()
# Unity wrote importer and scene metadata; preserve those GUIDs in the repository.
for file in (qa/'Assets/NakiPrototype/Resources/NakiArt/ThunderChapter').glob('*.meta'):
 shutil.copy2(file,repo/'Assets/NakiPrototype/Resources/NakiArt/ThunderChapter'/file.name)
for name in ['NakiThunderChapter.unity','NakiThunderChapter.unity.meta','NakiThunderBossPractice.unity','NakiThunderBossPractice.unity.meta']:
 shutil.copy2(qa/'Assets/Scenes'/name,repo/'Assets/Scenes'/name)
# Normalize authored text without changing pixels, GUIDs or content.
status=subprocess.check_output(['git','-c','core.quotepath=false','status','--porcelain','--untracked-files=all']).decode('utf-8')
files=[repo/line[3:] for line in status.splitlines() if line[3:].startswith(('Assets/','Art/ThunderChapterV1/','Docs/','Validation/ThunderChapterV1/'))]
files.append(repo/'README.md')
for file in files:
 if file.is_file() and file.suffix in ['.cs','.meta','.unity','.json','.md','.txt','.py']:
  s=file.read_text('utf-8-sig');file.write_text('\n'.join(x.rstrip() for x in s.splitlines())+'\n',encoding='utf-8',newline='\n')
sources=[]
runtime=repo/'Assets/NakiPrototype'
for pattern in ['*.cs','*.cs.meta','Editor/*.cs','Editor/*.cs.meta','Resources/NakiArt/ThunderChapter.meta','Resources/NakiArt/ThunderChapter/*']:
 sources.extend(f for f in runtime.glob(pattern) if f.is_file())
copies=[];conflicts=[]
for src in sorted(set(sources)):
 rel=src.relative_to(runtime)
 for root in [actual/'Assets/NakiPrototype',legacy]:copies.append((src,root/rel))
for name in ['NakiThunderChapter.unity','NakiThunderChapter.unity.meta','NakiThunderBossPractice.unity','NakiThunderBossPractice.unity.meta']:
 copies.append((repo/'Assets/Scenes'/name,actual/'Assets/Scenes'/name))
for src,dst in copies:
 if not dst.exists() or normalized(dst.read_bytes())==normalized(src.read_bytes()):continue
 head=subprocess.run(['git','show','HEAD:'+src.relative_to(repo).as_posix()],capture_output=True)
 if head.returncode!=0 or normalized(head.stdout)!=normalized(dst.read_bytes()):conflicts.append(str(dst))
if conflicts:raise RuntimeError('Preserved unexpected destination edits: '+json.dumps(conflicts,ensure_ascii=False))
for src,dst in copies:
 dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dst)
for src,dst in copies:assert src.read_bytes()==dst.read_bytes(),dst
report=dict(copied=len(copies),actual_project=str(actual),main_script_guid='d922c3c6bbc91794ea00f6c8c00f1911',verified_hashes={str(src.relative_to(repo)):sha(src.read_bytes()) for src,_ in copies},note='Existing files checked against HEAD or delivery before copying; user SampleScene and user save files untouched.')
(repo/'Validation/ThunderChapterV1/delivery.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print('Verified and copied',len(copies),'files to actual Unity project and legacy script distribution.')
