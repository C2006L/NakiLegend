"""Copy original generated PNGs; author metadata only, never alter source pixels."""
from pathlib import Path
from PIL import Image
import numpy as np
from scipy.ndimage import label,find_objects
import shutil,json,uuid
source=Path('C:/Users/15853/.codex/generated_images/01a1764d-2b16-7840-a400-e14f3ffe5ffd')
out=Path('Assets/NakiPrototype/Resources/NakiArt/ThunderChapter')
assets={'warden':'f2ff7187-3ebf-4633-b037-08ba9296e90d','sentinel':'085d3784-b3e3-455c-90e7-7deea34ebf65','stag':'09fd0936-5442-4587-9545-6970b82ca819','roller':'3fdc457e-540d-4f32-90e9-0e83b46526c7','lampkeeper':'001627bf-8380-40e7-811a-54433bc8b1ab','ranger':'34637ca2-7c85-4667-a985-ec310977145f','ending':'ba8de437-cfad-4ed2-acd5-c4c3023876a9','crossbow':'883d2fe6-62f1-4e81-a011-aa6547785aed','staff':'e6f2968b-8f5f-44a1-a447-6e9e990d6e8f','fork':'253fdec7-2ea5-46b3-b05e-60326060823a','hammer':'2ae09288-517c-44f4-8514-c21343312b97','bow':'75bb696f-aaeb-446e-8c78-82c640fbd789','rapier':'1aaa0a06-ee72-4844-b0c3-8f74314af5dd','floor2':'88a4767d-deb4-4727-b421-1a5540f11f1c','floor3':'cbf49c6b-842f-41a1-be31-61f3c7794aac'}
records=[]
def frame(box,pivot):
 x,y,r,b=map(int,box)
 return dict(x=x,y=y,width=r-x,height=b-y,pivotX=float(pivot[0]),pivotY=float(pivot[1]))
for key,ident in assets.items():
 file=out/(key+'.png');shutil.copy2(source/('exec-'+ident+'.png'),file)
 im=Image.open(file);w,h=im.size
 if key in ['ending','floor2','floor3']:
  if key!='ending':records.append(dict(key=key,ppu=w/4,frames=[frame((0,0,w,h),(.5,.5))]))
  continue
 alpha=np.asarray(im)[:,:,3];mask=alpha>80;labs,num=label(mask);parts=[]
 for i,s in enumerate(find_objects(labs)):
  if s is not None and (labs[s]==i+1).sum()>200:parts.append((int((labs[s]==i+1).sum()),(s[1].start,s[0].start,s[1].stop,s[0].stop)))
 if key in ['warden','sentinel','stag','roller']:
  boxes=[p[1] for p in sorted(parts,reverse=True)[:8]]
  assert len(boxes)==8,(key,boxes)
  boxes.sort(key=lambda b:(int((b[1]+b[3])/2>h/2),b[0]))
  height={'warden':2.0,'sentinel':2.4,'stag':2.85,'roller':1.0}[key]
  ppu=(boxes[0][3]-boxes[0][1])/height
  frames=[]
  for box in boxes:
   x,y,r,b=box
   # Anchor on the visible foot contact, excluding spear tips and antlers.
   bottom=mask[max(y,b-22):b,x:r];ys,xs=np.nonzero(bottom)
   px=float(np.median(xs))/(r-x) if len(xs) else .5
   frames.append(frame(box,(px,0)))
 elif key in ['lampkeeper','ranger']:
  ys,xs=np.nonzero(mask);box=(xs.min(),ys.min(),xs.max()+1,ys.max()+1);ppu=(box[3]-box[1])/1.25
  frames=[frame(box,(.5,0))]
 else:
  ys,xs=np.nonzero(mask);box=(xs.min(),ys.min(),xs.max()+1,ys.max()+1);ppu=box[2]-box[0]
  # Author real grasp points, in original source pixel coordinates.
  grip={'crossbow':(250,650),'staff':(310,360),'fork':None,'hammer':None,'bow':(862,505),'rapier':None}[key]
  pivot=((grip[0]-box[0])/(box[2]-box[0]),1-(grip[1]-box[1])/(box[3]-box[1])) if grip else (.15,.5)
  frames=[frame(box,pivot)]
 records.append(dict(key=key,ppu=round(float(ppu),5),frames=frames))
 print(key,'frames',len(frames),'ppu',round(ppu,2),'rects',[(f['x'],f['y'],f['width'],f['height']) for f in frames])
(out/'art.json').write_text(json.dumps(dict(records=records),ensure_ascii=False,indent=2),encoding='utf-8')
for file in [*out.glob('*'),*Path('Assets/NakiPrototype').glob('*.cs')]:
 if file.is_file() and file.suffix!='.meta' and not Path(str(file)+'.meta').exists():Path(str(file)+'.meta').write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\n',encoding='utf-8')
