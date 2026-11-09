"""Read-only raster inspection; Unity performs native sprite slicing, no raster rewriting."""
from pathlib import Path
import json, hashlib
from PIL import Image
root=Path(__file__).resolve().parents[2]
folder=root/'Assets/NakiPrototype/Resources/NakiArt/WeaponIdentityV3'
catalog=json.loads((root/'Art/WeaponIdentityV3/catalog.json').read_text(encoding='utf-8'))
images=[Image.open(folder/f'sheet-{i}.png').convert('RGBA') for i in range(10)]
for i,im in enumerate(images):
 assert im.getchannel('A').getextrema()[0]==0,('Background must be transparent',i)
entries=[];hashes=set()
for row in catalog:
 im=images[row['sheet']];cols=6 if row['sheet']<7 else 3;lines=3 if row['sheet']<7 else 1;col=row['cell']%cols;line=row['cell']//cols
 x0=round(col*im.width/cols);x1=round((col+1)*im.width/cols);y0=round(line*im.height/lines);y1=round((line+1)*im.height/lines)
 if row['sheet']==8:
  edges=[0,round(im.width*.40),round(im.width*.65),im.width];x0,x1=edges[col:col+2]
 # Alpha analysis only. Tiny, detached generation speckles do not define sprite bounds.
 pixels=im.load();points={(x,y) for y in range(y0,y1) for x in range(x0,x1) if pixels[x,y][3]>=48}
 components=[]
 while points:
  seed=points.pop();group=[seed];stack=[seed]
  while stack:
   x,y=stack.pop()
   for q in ((x-1,y),(x+1,y),(x,y-1),(x,y+1)):
    if q in points:points.remove(q);stack.append(q);group.append(q)
  if len(group)>=25:components.extend(group)
 assert len(components)>200,('Empty or implausibly small weapon',row)
 bx0=max(x0,min(x for x,y in components)-2);bx1=min(x1,max(x for x,y in components)+3)
 by0=max(y0,min(y for x,y in components)-2);by1=min(y1,max(y for x,y in components)+3)
 digest=hashlib.sha256(bytes(v for y in range(by0,by1) for x in range(bx0,bx1) for v in pixels[x,y])).hexdigest()
 assert digest not in hashes,('Duplicate artwork',row['id']);hashes.add(digest)
 name=row['name'];kind=row['type'];cross='弩' in name;heavy='锤' in name or '槌' in name
 form='hammer' if heavy else 'thrust' if kind=='Spear' else 'arc' if kind=='Sword' else 'bolt' if cross else 'arrow' if kind=='Bow' else {'Wind':'crescent','Ice':'crystal','Fire':'flame','Wood':'seed','Lightning':'needle','Light':'star'}.get(row['element'],'star')
 if '角灯' in name:form='lantern'
 if '砂心' in name or '流沙' in name:form='stone'
 if '镜花' in name:form='mirror'
 if '回泉' in name:form='spring'
 if '菌冠' in name:form='spore'
 if row['id']==123:form='sunray'
 if row['id']==129:form='soul'
 pivotx=.19 if kind!='Bow' else .28 if cross else .53
 entries.append(dict(id=row['id'],displayName=name,sheet=f'NakiArt/WeaponIdentityV3/sheet-{row["sheet"]}',x=bx0,y=im.height-by1,width=bx1-bx0,height=by1-by0,pivotX=pivotx,pivotY=.5,form=form,accent=row['id']%7,cast=f'weapon_{row["id"]:03}_cast',hit=f'weapon_{row["id"]:03}_hit',pixelHash=digest))
(folder/'manifest.json').write_text(json.dumps(dict(entries=entries),ensure_ascii=False,indent=2),encoding='utf-8')
(root/'Art/WeaponIdentityV3/audit.json').write_text(json.dumps(dict(weapons=len(entries),uniqueNames=len({x['displayName'] for x in entries}),uniqueArtwork=len(hashes),sheets=[dict(sheet=i,width=im.width,height=im.height,sha256=hashlib.sha256((folder/f'sheet-{i}.png').read_bytes()).hexdigest()) for i,im in enumerate(images)]),indent=2),encoding='utf-8')
print(f'Verified {len(entries)} unique names and alpha-bounded artwork regions across {len(images)} transparent atlases')
