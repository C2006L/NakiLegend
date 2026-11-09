from pathlib import Path
import shutil,hashlib,json
r=Path(__file__).resolve().parents[1];out=r/'Validation/ManorPressureV6';paths=set((r/'Validation/manor-pressure-files.txt').read_text(encoding='utf-8').splitlines())
for rel in ['Assets/NakiPrototype/NakiPrototype.Chapters.cs','Assets/NakiPrototype/Editor/NakiSampleScenes.cs']:
 p=r/rel;s=p.read_text(encoding='utf-8-sig')
 for old,new in [('冰山岛','霜潮群岛'),('烈焰岛','赤炉群岛'),('大骑士的府邸','暮钟岛'),('6 大骑士府邸','6 暮钟岛')]:s=s.replace(old,new)
 p.write_text(s,encoding='utf-8');paths.add(rel)
targets=[r.parent/'UnityPrototype',r.parent/'UnityPrototype/Assets/NakiPrototype/NakiLegend'];records=[]
def norm(p):return '\n'.join(x.rstrip() for x in p.read_text(encoding='utf-8-sig').splitlines())
for rel in sorted(paths):
 src=r/rel
 for i,t in enumerate(targets):
  dst=t/rel;peer=targets[1-i]/rel
  assert dst.resolve().is_relative_to((r.parent/'UnityPrototype').resolve())
  if dst.exists() and norm(dst)==norm(src):continue
  if dst.exists() and peer.exists():assert norm(dst)==norm(peer) or norm(peer)==norm(src),'Independent local edit: '+str(dst)
  if dst.exists():
   bak=out/'backup'/str(i)/rel;bak.parent.mkdir(parents=True,exist_ok=True)
   if not bak.exists():shutil.copy2(dst,bak)
  dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dst);assert src.read_bytes()==dst.read_bytes()
  records.append({'path':str(dst),'sha256':hashlib.sha256(dst.read_bytes()).hexdigest()})
(out/'sync.json').write_text(json.dumps(records,ensure_ascii=False,indent=2),encoding='utf-8');print('Synced and verified',len(records),'scripts; saves and scenes preserved')
