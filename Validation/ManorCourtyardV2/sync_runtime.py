"""Sync manor scripts and new assets, preserving existing scenes, player saves and independent edits."""
from pathlib import Path
import shutil,hashlib,json,subprocess,uuid
repo=Path(__file__).resolve().parents[2];workspace=repo.parent
actual=workspace/'UnityPrototype/Assets/NakiPrototype/NakiLegend';legacy=workspace/'UnityPrototype'
qa=workspace/'Validation/DesertV2/UnitySmokeProject';out=repo/'Validation/ManorCourtyardV2/runtime'
assert (actual/'ProjectSettings/ProjectVersion.txt').exists()
assert 'd922c3c6bbc91794ea00f6c8c00f1911' in (actual/'Assets/NakiPrototype/NakiPrototype.cs.meta').read_text()
for name in ['NakiManorCourtyard.unity','NakiManorCaptainPractice.unity']:
 for suffix in ['', '.meta']:
  src=qa/'Assets/Scenes'/(name+suffix);dst=repo/'Assets/Scenes'/(name+suffix)
  assert src.exists()
  if dst.exists():assert dst.read_bytes()==src.read_bytes(), 'Refuse to overwrite independent scene: '+str(dst)
  else:shutil.copy2(src,dst)
folder=repo/'Assets/NakiPrototype/Resources/NakiArt/ManorCourtyardV2'
if not Path(str(folder)+'.meta').exists():Path(str(folder)+'.meta').write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n',encoding='utf-8')
def git(*args):return subprocess.run(['git',*args],cwd=repo,check=True,capture_output=True,text=True).stdout.splitlines()
files=git('diff','--name-only')+git('ls-files','--others','--exclude-standard')
scripts={f for f in files if f.startswith('Assets/NakiPrototype/') and (f.endswith('.cs') or f.endswith('.cs.meta'))}
assets={str(p.relative_to(repo)).replace('\\','/') for p in folder.rglob('*') if p.is_file()}
assets.add(str(Path(str(folder)+'.meta').relative_to(repo)).replace('\\','/'))
for phase in ['cast','hit']:
 for suffix in ['', '.meta']:assets.add('Assets/NakiPrototype/Resources/NakiCombatAudioV3/weapon_132_'+phase+'.wav'+suffix)
scenes={'Assets/Scenes/'+name+suffix for name in ['NakiManorCourtyard.unity','NakiManorCaptainPractice.unity'] for suffix in ['', '.meta']}
files=sorted(assets)+sorted(scenes)+sorted(scripts,key=lambda f:(not f.endswith('.meta'),f))
def normalized(p):
 data=p.read_bytes()
 try:return '\n'.join(line.rstrip() for line in data.decode('utf-8-sig').replace('\r\n','\n').splitlines()).encode()
 except UnicodeDecodeError:return data
jobs=[]
for rel in files:
 src=repo/rel
 for target,label in [(actual,'actual'),(legacy,'legacy')]:
  if target==legacy and rel.startswith('Assets/Scenes/'):continue
  dst=target/rel;assert dst.resolve().is_relative_to(workspace.resolve())
  if dst.exists() and normalized(dst)==normalized(src):continue
  if dst.exists():
   if rel in assets or rel in scenes:raise RuntimeError('Preserved independent new asset: '+str(dst))
   peer=(legacy if target==actual else actual)/rel
   assert peer.exists() and normalized(dst)==normalized(peer),'Preserved independent script edit: '+str(dst)
  jobs.append((src,dst,rel,label))
records=[]
for src,dst,rel,label in jobs:
 if dst.exists():
  backup=out/'pre-manor-sync'/label/rel;backup.parent.mkdir(parents=True,exist_ok=True)
  if not backup.exists():shutil.copy2(dst,backup)
 dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dst);assert src.read_bytes()==dst.read_bytes()
 records.append(dict(destination=str(dst),sha256=hashlib.sha256(dst.read_bytes()).hexdigest()))
out.mkdir(exist_ok=True)
if records:(out/'sync-manifest.json').write_text(json.dumps(records,ensure_ascii=False,indent=2),encoding='utf-8')
print(f'Synced {len(records)} byte-verified files; existing scenes and all player save files preserved')
