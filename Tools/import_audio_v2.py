"""Import authored CC0 scores and recorded sword attacks, preserving source evidence.
Requires requests, imageio-ffmpeg, numpy, scipy. Original assets cached outside Unity.
"""
from pathlib import Path
import sys, re, html, json, hashlib, zipfile, io
from concurrent.futures import ThreadPoolExecutor
import requests
import numpy as np
from scipy.signal import resample_poly
sys.path.append('D:/NakiLegend/Tools/audio_runtime')
import subprocess, imageio_ffmpeg
from scipy.io.wavfile import write
FFMPEG=imageio_ffmpeg.get_ffmpeg_exe()
def decode(source):
 args=[FFMPEG,"-v","error","-i",str(source) if not isinstance(source,bytes) else "pipe:0","-f","f32le","-ac","2","-ar","44100","pipe:1"]
 data=subprocess.run(args,input=source if isinstance(source,bytes) else None,capture_output=True,check=True).stdout
 return np.frombuffer(data,dtype=np.float32).reshape(-1,2).copy(),44100
ROOT=Path(__file__).resolve().parents[1]
CACHE=ROOT/'References/AudioV2';CACHE.mkdir(parents=True,exist_ok=True)
OUT=ROOT/'Assets/NakiPrototype/Resources/NakiAudioV2';OUT.mkdir(parents=True,exist_ok=True)
TRACKS={
 'desert_explore':'heat-adventure','desert_combat':'heat-battle','desert_boss':'heat-boss-battle',
 'thunder_explore':'heavy-dungeon','thunder_combat':'heavy-battle-1','thunder_boss':'heavy-boss-battle-1',
 'woodland_explore':'fairy-adventure','woodland_combat':'fairy-battles','woodland_boss':'fairy-boss-battles'}
def source(slug,extension):
 url='https://opengameart.org/content/'+slug
 response=requests.get(url,timeout=60);response.raise_for_status();page=response.text
 assert 'creativecommons.org/publicdomain/zero' in page or '>CC0<' in page, 'CC0 not found: '+slug
 (CACHE/(slug+'.html')).write_text(page,encoding='utf-8')
 links=sorted(set(html.unescape(x) for x in re.findall(r'href="([^"]+)"',page) if '/sites/default/files/' in x and x.lower().endswith(extension)))
 assert links,(slug,extension)
 result=[]
 for link in links:
  name=html.unescape(link.rsplit('/',1)[1]);file=CACHE/name
  if not file.exists():
   r=requests.get(link,timeout=120);r.raise_for_status();file.write_bytes(r.content)
  result.append((file,link))
 return url,result
def master(data):
 # Preserve arrangement/dynamics; constant gain only, peak ceiling prevents clipping.
 data=np.asarray(data,dtype=np.float32)
 rms=np.sqrt(np.mean(data**2));gain=min(.15/max(rms,.001),.82/max(float(np.max(np.abs(data))),.001))
 return data*gain
def music(item):
 key,slug=item;page,files=source(slug,'.ogg');file,url=files[0]
 data,rate=decode(file)
 subprocess.run([FFMPEG,'-v','error','-y','-i',str(file),'-af','loudnorm=I=-18:TP=-2:LRA=11','-ar','44100','-c:a','libvorbis','-q:a','5',str(OUT/(key+'.ogg'))],check=True)
 print(key,round(len(data)/rate,2),'seconds',flush=True)
 return {'key':key,'title':slug,'author':'MintoDog','license':'CC0-1.0','page':page,'download':url,'sourceSha256':hashlib.sha256(file.read_bytes()).hexdigest(),'seconds':len(data)/rate,'stereo':data.shape[1]==2}
records=list(ThreadPoolExecutor(max_workers=4).map(music,TRACKS.items()))
page,archives=source('20-sword-sound-effects-attacks-and-clashes','.zip')
for file,url in archives:
 clash='clash' in file.name
 with zipfile.ZipFile(file) as z:
  entries=[n for n in sorted(z.namelist()) if n.lower().endswith(('.wav','.ogg')) and not n.startswith('__MACOSX')]
  assert len(entries)>=3
  print('sword archive:',entries,flush=True)
  for i,entry in enumerate(entries[:3]):
   x,rate=decode(z.read(entry));x=x.mean(axis=1)
   threshold=max(.002,np.max(np.abs(x))*.02);nz=np.flatnonzero(np.abs(x)>threshold);assert len(nz)
   x=x[max(0,nz[0]-int(rate*.004)):min(len(x),nz[-1]+int(rate*.025))]
   # Keep attack short enough that continuous J never becomes a wash of hiss.
   x=x[:int(rate*(.30 if clash else .18))]
   fade=min(int(rate*.008),len(x)//4);x[:fade]*=np.linspace(0,1,fade);x[-fade:]*=np.linspace(1,0,fade)
   x=master(x);key=('clash_' if clash else 'sword_')+str(i);write(OUT/(key+'.wav'),rate,(np.clip(x,-.95,.95)*32767).astype(np.int16))
   if not clash:
    for kind in ['spear','heavy']:
     # Spear is a tighter attack; heavy retains a longer and lower blade sound.
     y=resample_poly(x,5,6) if kind=='spear' else resample_poly(x,6,5)
     write(OUT/(kind+'_'+str(i)+'.wav'),rate,(np.clip(y,-.95,.95)*32767).astype(np.int16))
   records.append({'key':key,'author':'StarNinjas','title':entry,'license':'CC0-1.0','page':page,'download':url,'sourceSha256':hashlib.sha256(file.read_bytes()).hexdigest(),'seconds':len(x)/rate})
page,items=source('forest-whisper-theme','.zip')
file,url=items[0]
with zipfile.ZipFile(file) as z:
 entry=next(n for n in z.namelist() if n.lower().endswith('.ogg') and not n.startswith('__MACOSX'))
 raw=CACHE/'ending-original.ogg';raw.write_bytes(z.read(entry))
 subprocess.run([FFMPEG,'-v','error','-y','-i',str(raw),'-af','loudnorm=I=-20:TP=-2:LRA=11','-ar','44100','-c:a','libvorbis','-q:a','5',str(OUT/'ending.ogg')],check=True)
 records.append({'key':'ending','author':'Cleyton Kauffman','title':'Forest Whisper Theme','license':'CC0-1.0','page':page,'download':url,'sourceSha256':hashlib.sha256(file.read_bytes()).hexdigest()})
(OUT/'credits.json').write_text(json.dumps(records,ensure_ascii=False,indent=2),encoding='utf-8')
print('Imported authored music and recorded sword sounds.',flush=True)
