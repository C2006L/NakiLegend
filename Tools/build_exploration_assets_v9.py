from pathlib import Path
import shutil,json
import numpy as np
from PIL import Image
from scipy.ndimage import label,find_objects,binary_dilation
ROOT=Path(__file__).resolve().parents[1];ART=ROOT/'Art/ExplorationV9';OUT=ROOT/'Assets/NakiPrototype/Resources/NakiArt';manifest={}
for kind,cols in [('movement',4),('actions',4),('walk',6)]:
 im=Image.open(ART/f'hero-body-{kind}-v9-original.png').convert('RGBA');a=np.array(im);labels,n=label(a[:,:,3]>64);rows=[[] for _ in range(4)]
 for idx,sl in enumerate(find_objects(labels),1):
  if sl is None or np.count_nonzero(labels[sl]==idx)<300:continue
  y,x=sl;row=min(3,int((y.start+y.stop)/2/(im.height/4)));rows[row].append((x.start,y.start,x.stop,y.stop,idx))
 atlas=Image.new('RGBA',(256*cols,1024));records=[]
 for row,boxes in enumerate(rows):
  boxes.sort();assert len(boxes)==cols,(kind,row,len(boxes))
  idle_height=boxes[0][3]-boxes[0][1]
  for col,(x0,y0,x1,y1,idx) in enumerate(boxes):
   crop=a[y0:y1,x0:x1].copy();mask=binary_dilation(labels[y0:y1,x0:x1]==idx,iterations=1);crop[~mask]=0
   band=crop[15:min(60,y1-y0)];hair=(band[:,:,3]>128)&(band[:,:,:3].mean(axis=2)<105);xs=np.nonzero(hair)[1];head=float(np.median(xs)) if xs.size else (x1-x0)/2
   scale=140/(y1-y0) if kind=='walk' or kind=='movement' and col<3 else 140/idle_height if kind=='movement' else manifest['movement'][row*cols]['scale']
   size=(round((x1-x0)*scale),round((y1-y0)*scale));sprite=Image.fromarray(crop).resize(size,Image.Resampling.LANCZOS);px=round(128-head*scale);py=232-size[1]
   assert px>=4 and py>=4 and px+size[0]<=252,(kind,row,col,px,py,size)
   atlas.alpha_composite(sprite,(col*256+px,row*256+py));records.append(dict(row=row,col=col,box=[x0,y0,x1,y1],scale=scale,offset=[px,py]))
 a=np.array(atlas);a[a[:,:,3]<8]=0;atlas=Image.fromarray(a)
 for row in range(4):
  for col in range(cols):
   q=a[row*256:(row+1)*256,col*256:(col+1)*256,3];assert not(q[:4].any() or q[-4:].any() or q[:,:4].any() or q[:,-4:].any())
 atlas.save(OUT/f'hero-body-{kind}-v9.png');manifest[kind]=records
for name,maxsize in [('equipment-spear',384),('equipment-staff',384),('equipment-bow',256),('lesson-altar',210)]:
 im=Image.open(ART/f'{name}-v9-original.png').convert('RGBA');a=np.array(im);mask=a[:,:,3]>64;ys,xs=np.nonzero(mask);box=(xs.min(),ys.min(),xs.max()+1,ys.max()+1);im=im.crop(box);ratio=maxsize/max(im.size);im=im.resize((round(im.width*ratio),round(im.height*ratio)),Image.Resampling.LANCZOS);a=np.array(im);a[a[:,:,3]<16]=0;Image.fromarray(a).save(OUT/f'{name}-v9.png')
(ART/'atlas-layout.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8');print('56 isolated body poses and four equipment/altar assets exported.')
