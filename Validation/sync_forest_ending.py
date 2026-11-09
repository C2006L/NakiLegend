from pathlib import Path
import shutil,hashlib,json
r=Path('D:/NakiLegend/Repository');out=r/'Validation/ForestEndingV7';paths=set((r/'Validation/forest-ending-files.txt').read_text(encoding='utf-8').splitlines());targets=[r.parent/'UnityPrototype',r.parent/'UnityPrototype/Assets/NakiPrototype/NakiLegend'];records=[]
def norm(p):return '\n'.join(x.rstrip() for x in p.read_text(encoding='utf-8-sig').splitlines())
for rel in sorted(paths):
 src=r/rel
 for i,t in enumerate(targets):
  dst=t/rel;peer=targets[1-i]/rel
  assert dst.resolve().is_relative_to((r.parent/'UnityPrototype').resolve())
  if dst.exists() and norm(dst)==norm(src):continue
  if dst.exists() and peer.exists():assert norm(dst)==norm(peer) or norm(peer)==norm(src),'Independent edit: '+str(dst)
  if dst.exists():
   bak=out/'backup'/str(i)/rel;bak.parent.mkdir(parents=True,exist_ok=True)
   if not bak.exists():shutil.copy2(dst,bak)
  shutil.copy2(src,dst);assert src.read_bytes()==dst.read_bytes();records.append(str(dst))
(out/'sync.json').write_text(json.dumps(records,ensure_ascii=False,indent=2),encoding='utf-8');print('Synced',len(records),'verified scripts; no player saves deleted')
