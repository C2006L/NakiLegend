from pathlib import Path
from PIL import Image
import json, uuid, shutil
root=Path(__file__).resolve().parents[2]
art=root/'Art/ManorCourtyardV2'
dest=root/'Assets/NakiPrototype/Resources/NakiArt/ManorCourtyardV2'
dest.mkdir(parents=True,exist_ok=True)
def meta(p):
    m=Path(str(p)+'.meta')
    if m.exists():return
    if p.suffix=='.png':
        sample=(root/'Assets/NakiPrototype/Resources/NakiArt/IceSample/atlas.png.meta').read_text()
        import re
        sample=re.sub(r'guid: [0-9a-f]+','guid: '+uuid.uuid4().hex,sample, count=1)
        sample=re.sub(r'isReadable: \d','isReadable: 1',sample)
        m.write_text(sample,encoding='utf-8')
    else:m.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\n',encoding='utf-8')
def save(im,name):
    p=dest/(name+'.png');im.save(p);meta(p)
# Sprite extraction only: retain generated alpha and color, no synthesized/repainted subjects.
for source,cols,prefix in [('actors-source.png',6,'actor'),('extra-source.png',5,'extra')]:
    if not (art/source).exists():continue
    sheet=Image.open(art/source).convert('RGBA');cw=sheet.width/cols;ch=sheet.height/8
    for c in range(cols):
        cells=[]
        for r in range(8):
            cell=sheet.crop((round(c*cw),round(r*ch),round((c+1)*cw),round((r+1)*ch)))
            # Equal cell bounds/PPU keep the actor and defeat pose the same size.
            cells.append(cell)
        for r,im in enumerate(cells):save(im,f'{prefix}-{c}-{r}')
sheet=Image.open(art/'environment-source.png')
# Inner paving excludes the generated tile outline; props preserve the authored alpha.
regions=[(35,35,325,325),(399,35,687,325),(735,86,1074,296),(1096,102,1448,298),
         (12,379,345,714),(366,371,714,714),(767,362,1059,714),(1138,375,1448,714),
         (3,731,358,1084),(366,731,723,1084),(785,731,1059,1084),(1093,731,1447,1084)]
for i,r in enumerate(regions):save(sheet.crop(r),f'prop-{i}')
save(Image.open(art/'harbor-source.png'),'harbor')
if (art/'spear-source.png').exists():
    im=Image.open(art/'spear-source.png').convert('RGBA');bbox=im.getchannel('A').getbbox();save(im.crop(bbox),'captain-spear')
print('Extracted generated manor sprites, preserving alpha and fixed animation cell sizes')
