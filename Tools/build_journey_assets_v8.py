"""Build isolated runtime atlases from eight-pose sheets; originals stay intact."""
from pathlib import Path
import json
import shutil
import numpy as np
from PIL import Image
from scipy.ndimage import label, find_objects, binary_dilation

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / 'Art/DesertJourneyV8'
TARGET = ROOT / 'Assets/NakiPrototype/Resources/NakiArt'
TARGET.mkdir(parents=True, exist_ok=True)
NAMES = ['scorpion-actions', 'skeleton-actions', 'tumbleweed-actions', 'kite-actions',
         'sentinel-actions', 'wind-scorpion-actions', 'colossus-actions', 'ape-king-actions']
manifest = {'cell': 512, 'pivot': [.5, 32/512], 'pixelsPerUnit': 130, 'enemies': {}}
for kind, name in enumerate(NAMES):
    image = Image.open(SOURCE / (name+'-original.png')).convert('RGBA')
    rgba = np.array(image)
    solid = rgba[:,:,3] > 96
    labels, _ = label(binary_dilation(solid, iterations=2))
    rows = [[], []]
    for index, slices in enumerate(find_objects(labels), 1):
        if slices is None or np.count_nonzero(solid[slices] & (labels[slices] == index)) < 300:
            continue
        y, x = slices
        row = min(1, int((y.start+y.stop)/2 / (image.height/2)))
        rows[row].append((x.start,y.start,x.stop,y.stop,index))
    for row in rows:
        row.sort()
        assert len(row) == 4, (name, row)
    boxes = rows[0]+rows[1]
    base_height = max(b[3]-b[1] for b in boxes[:4]) if kind == 3 else boxes[0][3]-boxes[0][1]
    target_height = 300 if kind >= 5 else 170 if kind == 4 else 140
    atlas = Image.new('RGBA',(2048,1024))
    records = []
    for pose, (x0,y0,x1,y1,index) in enumerate(boxes):
        crop = rgba[y0:y1,x0:x1].copy()
        # Bboxes can overlap even when sprites do not. Copy only this connected
        # component, never the neighbouring spear, wing tip or head in that box.
        own = labels[y0:y1,x0:x1] == index
        crop[~own] = 0
        height = y1-y0
        if kind in [1,4,6,7]:
            band0, band1 = int(height*.04), int(height*.18)
        else:
            band0, band1 = int(height*.38), int(height*.62)
        band = crop[band0:max(band0+1,band1),:,3] > 96
        xx = np.nonzero(band)[1]
        anchor = float(np.median(xx)) if xx.size else (x1-x0)/2
        scale = target_height / (height if pose < 4 and kind != 3 else base_height)
        art = Image.fromarray(crop).resize((round((x1-x0)*scale),round(height*scale)),Image.Resampling.LANCZOS)
        px, py = round(256-anchor*scale), 480-art.height
        assert px>=8 and py>=8 and px+art.width<=504,(name,pose,px,py,art.size)
        atlas.alpha_composite(art, (pose%4*512+px,pose//4*512+py))
        records.append({'pose':pose,'sourceBox':[x0,y0,x1,y1],'anchorX':anchor,'scale':scale,'offset':[px,py]})
    pixels=np.array(atlas); pixels[pixels[:,:,3]<8]=0
    atlas=Image.fromarray(pixels)
    for pose in range(8):
        alpha=np.array(atlas.crop((pose%4*512,pose//4*512,pose%4*512+512,pose//4*512+512)))[:,:,3]
        assert not(alpha[:8].any() or alpha[-8:].any() or alpha[:,:8].any() or alpha[:,-8:].any())
        labs,_=label(alpha>96); areas=np.bincount(labs.ravel())[1:]
        assert len(areas[areas>30])==1,(name,pose,'detached fragment')
    atlas.save(TARGET/('journey-'+name+'.png'))
    manifest['enemies'][name]=records

for name in ['journey-courtyard','journey-canal','journey-cistern','journey-terrace']:
    source=SOURCE/(name+'-original.png')
    if source.exists(): shutil.copy2(source,TARGET/(name+'.png'))
source=SOURCE/'journey-cover-rock-original.png'
if source.exists():
    image=Image.open(source).convert('RGBA')
    alpha=image.getchannel('A').point(lambda a:255 if a>64 else 0)
    box=alpha.getbbox();art=image.crop(box)
    art.thumbnail((300,160),Image.Resampling.LANCZOS)
    result=Image.new('RGBA',(320,192));result.alpha_composite(art,((320-art.width)//2,176-art.height))
    result.save(TARGET/'journey-cover-rock.png')
shutil.copy2(ROOT/'Art/DesertStoryV1/the-wind-brings-rain-home.png',TARGET/'desert-rain-cg.png')
(SOURCE/'runtime-layout.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
print('PASS: 64 action poses isolated, transparent padding verified, no detached large fragments.')
