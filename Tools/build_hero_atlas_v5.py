"""Extract the 16 actual sprites, rather than dividing an irregular AI sheet.

Requires Pillow, numpy and scipy. Original artwork is retained unchanged.
"""
from pathlib import Path
import json
import numpy as np
from PIL import Image
from scipy.ndimage import label, find_objects, binary_dilation

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'Art/PolishedHeroV5'
OUT.mkdir(parents=True, exist_ok=True)
CELL, BASELINE, HEIGHT = 256, 232, 140
manifest = {'cell': CELL, 'pivot': [0.5, (CELL-BASELINE)/CELL],
            'pixelsPerUnit': 130, 'sheets': {}}

for kind in ['movement', 'actions']:
    source = ROOT / f'Art/PolishedHeroV4/hero-{kind}-v4-original.png'
    im = Image.open(source).convert('RGBA')
    rgba = np.asarray(im)
    labels, _ = label(rgba[:, :, 3] > 64)
    components = []
    for index, slices in enumerate(find_objects(labels), 1):
        if slices is None or np.count_nonzero(labels[slices] == index) < 300:
            continue
        y, x = slices
        components.append((x.start, y.start, x.stop, y.stop))
    assert len(components) == 16, (source, len(components))
    rows = [[] for _ in range(4)]
    for box in components:
        row = min(range(4), key=lambda r: abs(box[3] - [305, 605, 905, 1200][r]))
        rows[row].append(box)
    for row in rows:
        row.sort(key=lambda b: b[0])
        assert len(row) == 4
    atlas = Image.new('RGBA', (CELL*4, CELL*4))
    records = []
    for row, boxes in enumerate(rows):
        for col, box in enumerate(boxes):
            x0, y0, x1, y1 = box
            # Hair in the top 80 source pixels gives a stable body anchor;
            # centering the whole silhouette would slide the body when the cape moves.
            band = rgba[y0+20:min(y0+80,y1), x0:x1]
            mask = (band[:, :, 3] > 128) & (band[:, :, :3].mean(axis=2) < 105)
            xs = np.nonzero(mask)[1]
            head_x = x0 + float(np.median(xs))
            if kind == 'movement' and col < 3:
                scale = HEIGHT / (y1-y0)
            else:
                idle = records[row*4] if kind == 'movement' else manifest['sheets']['movement'][row*4]
                scale = HEIGHT / (idle['sourceBox'][3]-idle['sourceBox'][1])
            # Only the component's own bounding rectangle is copied. Neighbouring
            # figures are separated by transparent gaps in the original sheet.
            crop = im.crop(box)
            resized = crop.resize((round(crop.width*scale), round(crop.height*scale)), Image.Resampling.LANCZOS)
            px = round(CELL/2 - (head_x-x0)*scale)
            py = BASELINE-resized.height
            assert px >= 4 and py >= 4 and px+resized.width <= CELL-4
            atlas.alpha_composite(resized, (col*CELL+px,row*CELL+py))
            records.append({'row':row,'column':col,'sourceBox':list(map(int,box)),
                            'headAnchorX':head_x,'scale':scale,'offset':[px,py]})
    pixels = np.array(atlas)
    pixels[pixels[:, :, 3] < 8] = 0
    atlas = Image.fromarray(pixels)
    atlas.save(OUT/f'hero-{kind}-v5.png')
    manifest['sheets'][kind] = records
    # Padding on every edge is essential: sprites must never contain pixels from
    # an adjacent row, even with filtering or texture-coordinate rounding.
    for y in range(4):
        for x in range(4):
            a = np.array(atlas.crop((x*CELL,y*CELL,(x+1)*CELL,(y+1)*CELL)))[:,:,3]
            assert not (a[:4].any() or a[-4:].any() or a[:,:4].any() or a[:,-4:].any())

# The walking-only sheet has six genuine poses per direction. Its positions are
# discovered from alpha components too, so generated gutters are never trusted.
im = Image.open(OUT/'hero-walk-v5-original.png').convert('RGBA')
rgba = np.array(im)
labels, _ = label(rgba[:,:,3] > 64)
rows = [[] for _ in range(4)]
for index, slices in enumerate(find_objects(labels), 1):
    if slices is None or np.count_nonzero(labels[slices] == index) < 300:
        continue
    y, x = slices
    row = min(3, int((y.start+y.stop)/2 / (im.height/4)))
    rows[row].append((x.start,y.start,x.stop,y.stop,index))
atlas = Image.new('RGBA',(CELL*6,CELL*4))
records = []
for row, boxes in enumerate(rows):
    boxes.sort()
    assert len(boxes) == 6
    for col, (x0,y0,x1,y1,index) in enumerate(boxes):
        band = rgba[y0+15:min(y0+55,y1),x0:x1]
        mask = (band[:,:,3]>128) & (band[:,:,:3].mean(axis=2)<105)
        head_x = x0 + float(np.median(np.nonzero(mask)[1]))
        scale = HEIGHT/(y1-y0)
        # Retain the character and its immediate antialias edge; discard any
        # semitransparent decoration outside that character's alpha component.
        crop = rgba[y0:y1,x0:x1].copy()
        keep = binary_dilation(labels[y0:y1,x0:x1] == index, iterations=1)
        crop[~keep] = 0
        resized = Image.fromarray(crop).resize((round((x1-x0)*scale),round((y1-y0)*scale)),Image.Resampling.LANCZOS)
        px = round(CELL/2-(head_x-x0)*scale)
        py = BASELINE-resized.height
        assert px>=8 and py>=8 and px+resized.width<=CELL-8
        atlas.alpha_composite(resized,(col*CELL+px,row*CELL+py))
        records.append({'row':row,'column':col,'sourceBox':[x0,y0,x1,y1],
                        'headAnchorX':head_x,'scale':scale,'offset':[px,py]})
pixels=np.array(atlas);pixels[pixels[:,:,3]<8]=0
Image.fromarray(pixels).save(OUT/'hero-walk-v5.png')
manifest['sheets']['walk']=records
(OUT/'atlas-layout.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
print('Extracted 56 isolated sprites with padding and consistent ground anchors.')
