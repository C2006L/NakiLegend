"""Cache licensed source recordings for the short combat sound redesign."""
from pathlib import Path
import re,html,json,hashlib,zipfile
from concurrent.futures import ThreadPoolExecutor
import requests
ROOT=Path(__file__).resolve().parents[1];CACHE=ROOT/'References/CombatAudioV3';CACHE.mkdir(parents=True,exist_ok=True)
sources=[('electricity-sound-effects-0','BMacZero','.wav'),('80-cc0-rpg-sfx','rubberduck','.zip'),('medieval-sound-effects-weapon-textures','Ben Jaszczak & Brian Nelson','.7z')]
def fetch(item):
    slug,author,extension=item;page='https://opengameart.org/content/'+slug
    response=requests.get(page,timeout=60);response.raise_for_status();body=response.text
    assert 'creativecommons.org/publicdomain/zero' in body or '>CC0<' in body
    (CACHE/(slug+'.html')).write_text(body,encoding='utf-8')
    links=sorted(set(html.unescape(x) for x in re.findall(r'href="([^"]+)"',body) if '/sites/default/files/' in x and x.lower().endswith(extension)))
    if extension=='.7z':links=links[:1]
    records=[]
    for url in links:
        name=url.rsplit('/',1)[-1];path=CACHE/name
        if not path.exists():r=requests.get(url,timeout=150);r.raise_for_status();path.write_bytes(r.content)
        records.append({'file':name,'page':page,'url':url,'author':author,'license':'CC0-1.0','sha256':hashlib.sha256(path.read_bytes()).hexdigest()})
        if extension=='.zip':
            with zipfile.ZipFile(path) as z:
                print(name+': '+str([n for n in z.namelist() if any(k in n.lower() for k in ['bow','spell','blade','stone','explosion'])]),flush=True)
        print(name+': '+str(path.stat().st_size)+' bytes',flush=True)
    return records
records=[]
for group in ThreadPoolExecutor(max_workers=4).map(fetch,sources):records+=group
(CACHE/'sources.json').write_text(json.dumps(records,ensure_ascii=False,indent=2),encoding='utf-8')
