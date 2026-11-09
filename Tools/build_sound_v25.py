"""Build separately routed chapter scores and dry action cues, preserving old banks.

Source pages, CC0 evidence and hashes are retained outside Unity. Run with --fetch
to cache authored music, then without arguments for reproducible local processing.
"""
from pathlib import Path
import sys,json,hashlib,re,html,subprocess,shutil,math
from concurrent.futures import ThreadPoolExecutor
import numpy as np
from scipy.signal import butter,sosfilt,resample_poly
from scipy.io import wavfile
ROOT=Path(__file__).resolve().parents[1]; TASK=ROOT.parent/'Validation/AudioV25'
sys.path.insert(0,str(TASK/'runtime_lib'))
import imageio_ffmpeg
FF=imageio_ffmpeg.get_ffmpeg_exe(); RATE=22050
OUT=ROOT/'Assets/NakiPrototype/Resources/NakiSoundV25';OUT.mkdir(parents=True,exist_ok=True)
CACHE=ROOT/'References/AudioV25';CACHE.mkdir(parents=True,exist_ok=True)
SOURCES={
 'winter':('short-loop-winter-theme','yd','rememberWinter.ogg'),
 'labyrinth':('great-labyrinthorchestral-dungeon-music','MintoDog','great_labyrinth_bpm160.ogg'),
 'fire_explore':('hard-dungeon','MintoDog','hard_dungeon_bpm140.ogg'),
 'fire_combat':('hard-battle-1','MintoDog',''),
 'fire_boss':('hard-boss-battle-1','MintoDog',''),
 'palace':('a-soundtrack','Spring Spring','castle 2.ogg'),
 'palace_combat':('a-soundtrack','Spring Spring',"Into Depths! Keep in mind, it's forced.ogg"),
 'palace_boss':('a-soundtrack','Spring Spring','Final or Regular Boss Theme.ogg'),
}
def digest(p):return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def fetch(item):
 import requests
 key,(slug,author,filename)=item;page='https://opengameart.org/content/'+slug
 ev=CACHE/(slug+'.html')
 if ev.exists():text=ev.read_text(encoding='utf-8')
 else:
  response=requests.get(page,timeout=45);response.raise_for_status();text=response.text;ev.write_text(text,encoding='utf-8')
 assert 'creativecommons.org/publicdomain/zero' in text or '>CC0<' in text, 'Unverified license: '+slug
 from urllib.parse import unquote
 links=sorted(set(html.unescape(u) for u in re.findall(r'href="([^"]+)"',text) if '/sites/default/files/' in u and u.lower().endswith('.ogg')))
 matches=[u for u in links if not filename or unquote(u.rsplit('/',1)[1]).casefold()==filename.casefold() or re.sub(r'_\d+(?=\.ogg$)','',unquote(u.rsplit('/',1)[1])).casefold()==filename.casefold()]
 assert matches,(key,filename,links)
 link=matches[0];dest=CACHE/(key+'.ogg')
 if not dest.exists():
  response=requests.get(link,timeout=90);response.raise_for_status();assert len(response.content)>1000;dest.write_bytes(response.content)
 return {'id':key,'author':author,'license':'CC0-1.0','page':page,'download':link,'sha256':digest(dest)}
def decode(path,stereo=False):
 data=subprocess.run([FF,'-v','error','-i',str(path),'-ar',str(RATE),'-ac','2' if stereo else '1','-f','f32le','pipe:1'],capture_output=True,check=True).stdout
 x=np.frombuffer(data,dtype=np.float32).copy();return x.reshape(-1,2) if stereo else x
def band(x,low=60,high=4000):return sosfilt(butter(2,[low,min(high,RATE*.46)],btype='bandpass',fs=RATE,output='sos'),x,axis=0)
def fade(x,attack=.008,release=.045):
 x=x.copy();a=min(int(attack*RATE),len(x)//2);b=min(int(release*RATE),len(x)//2)
 if a:x[:a]*=np.linspace(0,1,a).reshape((-1,)+(1,)*(x.ndim-1))
 if b:x[-b:]*=np.linspace(1,0,b).reshape((-1,)+(1,)*(x.ndim-1))
 return x
STATS=[]
def wav(key,x,rms=.12,loop=False):
 if not loop:x=fade(x)
 x=x-np.mean(x,axis=0);gain=min(rms/max(.0001,float(np.sqrt(np.mean(x*x)))),.77/max(.001,float(np.max(np.abs(x)))));x=x*gain
 wavfile.write(OUT/(key+'.wav'),RATE,(np.clip(x,-.8,.8)*32767).astype(np.int16))
 STATS.append({'key':key,'seconds':len(x)/RATE,'peak':float(np.max(np.abs(x))),'rms':float(np.sqrt(np.mean(x*x))),'loop':loop})
def ogg(key,x):
 raw=TASK/(key+'.f32');raw.parent.mkdir(parents=True,exist_ok=True);x.astype(np.float32).tofile(raw)
 subprocess.run([FF,'-v','error','-y','-f','f32le','-ar',str(RATE),'-ac','2','-i',str(raw),'-af','loudnorm=I=-20:TP=-2:LRA=10,alimiter=limit=0.75:level=0','-ar','44100','-c:a','libvorbis','-q:a','5',str(OUT/(key+'.ogg'))],check=True)
 raw.unlink();STATS.append({'key':key,'seconds':len(x)/RATE,'stereo':True,'loop':True})
def pulse(length,bpm,strength=1):
 x=np.zeros(length);beat=60/bpm
 for i,at in enumerate(np.arange(0,length/RATE,beat/2)):
  n=min(int(RATE*.2),length-int(at*RATE));t=np.arange(n)/RATE
  hit=np.sin(2*np.pi*(58*t+18*(1-np.exp(-t*22))/22))*np.exp(-t*25)*(.8 if i%2==0 else .22)
  x[int(at*RATE):int(at*RATE)+n]+=hit*strength
 return np.column_stack((x,x))
def loop_seam(x):
 # Remove only the click at a non-looped source boundary, retaining the phrase.
 n=min(int(.08*RATE),len(x)//100);w=np.linspace(0,1,n)[:,None];edge=(x[-n:]+x[:n])*.5;x[:n]=edge*(1-w)+x[:n]*w;x[-n:]=x[-n:]*(1-w)+edge*w
 x[-n:]-=np.linspace(0,1,n)[:,None]*(x[-1]-x[0]);return x
def music():
 sources={k:decode(CACHE/(k+'.ogg'),True) for k in SOURCES}
 for k in sources:sources[k]=loop_seam(sources[k])
 old=ROOT/'Assets/NakiPrototype/Resources/NakiAudioV2'
 chapters={'ice':('winter','winter','winter',80),'fire':('fire_explore','fire_combat','fire_boss',160),'manor':('labyrinth','labyrinth','labyrinth',160),'capital':('palace','palace_combat','palace_boss',120)}
 arrangements=[]
 for region,(explore,combat,boss,bpm) in chapters.items():
  for state,source in zip(['explore','combat','boss'],[explore,combat,boss]):
   x=sources[source].copy();x*=.16/max(.01,float(np.sqrt(np.mean(x*x))))
   if source in ('winter','labyrinth') and state!='explore':x+=pulse(len(x),bpm,.055 if state=='combat' else .1)
   ogg(region+'_'+state,x);arrangements.append({'key':region+'_'+state,'source':source,'edit':'level matching, click-free loop seam; percussion arrangement for shared motifs'})
  x=sources[boss];wav(region+'_pressure',pulse(len(x),bpm,.12),.085,True)
 for region,bpm in [('desert',165),('thunder',190),('woodland',185)]:
  x=decode(old/(region+'_boss.ogg'),True);wav(region+'_pressure',pulse(len(x),bpm,.12),.075,True)
 # A short original four-note journey motif links letters, return and endings.
 def theme(seconds,mood):
  x=np.zeros((int(seconds*RATE),2));notes=[62,65,69,67] if mood!='rebirth' else [62,65,69,74]
  for i,at in enumerate(np.arange(0,seconds,2.5)):
   t=np.arange(min(int(RATE*5),len(x)-int(at*RATE)))/RATE;f=440*2**((notes[i%4]-69)/12)
   v=(np.sin(2*np.pi*f*t)+.28*np.sin(2*np.pi*2*f*t)+.12*np.sin(2*np.pi*3*f*t))*np.minimum(t/.04,1)*np.exp(-t*.9)*.07
   p=int(at*RATE);x[p:p+len(t),0]+=v;x[p:p+len(t),1]+=v*.9
  return loop_seam(x)
 for key,mood in [('capital_return','warm'),('capital_story','somber'),('manor_story','somber'),('ending_keep','warm'),('ending_rebirth','rebirth'),('capital_victory','warm'),('defeat','somber')]:
  x=theme(40,mood)
  if mood=='somber':x=band(x,70,1800)*.8
  ogg(key,x)
 return arrangements
def cues():
 bank=ROOT/'Assets/NakiPrototype/Resources/NakiCombatAudioV3';fallback=ROOT/'Assets/NakiPrototype/Resources/NakiAudio'
 def samplepath(name):
  return next(folder/(name+'_0.wav') for folder in [bank,ROOT/'Assets/NakiPrototype/Resources/NakiAudioV2',fallback] if (folder/(name+'_0.wav')).exists())
 def sample(name):return decode(samplepath(name))
 material={k:sample(v) for k,v in {'air':'spear','stone':'stone','metal':'hammer_hit','fire':'skill_meteor_cast','crack':'skill_ice_spike_hit','electric':'staff_electric','wood':'wood','dash':'warrior_dash','impact':'heavy','tear':'skill_mark_hit'}.items()}
 def layer(parts,seconds,variant=0):
  y=np.zeros(int(seconds*RATE))
  for source,level,delay,low,high in parts:
   x=material[source];x=resample_poly(x,100,100+variant*3);x=band(x,low,high);x/=max(.001,float(np.sqrt(np.mean(x*x))));start=int(delay*RATE);n=min(len(x),len(y)-start)
   if n>0:y[start:start+n]+=x[:n]*level*np.exp(-np.arange(n)/(RATE*.25))
  return y
 profiles={
 'skill_lightbeam_cast':(.25,[('air',.5,0,350,4200),('electric',.2,.01,250,2600)]),
 'skill_lightbeam_end':(.16,[('air',.3,0,180,1500)]),
 'skill_lightbeam_hit':(.12,[('electric',.3,0,250,3200),('metal',.25,0,150,1800)]),
 'skill_lightguard_cast':(.42,[('air',.35,0,100,1600),('metal',.4,.025,90,1300)]),
 'skill_lightguard_hit':(.18,[('metal',.55,0,150,2500),('electric',.15,0,200,1600)]),
 'skill_lightguard_end':(.23,[('air',.25,0,300,1900)]),
 'skill_solar_cast':(.25,[('air',.4,0,180,2200),('electric',.15,.02,300,1800)]),
 'skill_solar_pulse':(.19,[('air',.6,0,300,3300),('metal',.2,.01,200,1400)]),
 'skill_solar_hit':(.16,[('electric',.25,0,350,2800),('stone',.3,0,120,1800)]),
 'skill_meteor_cast':(.22,[('fire',.6,0,80,1500),('air',.25,0,100,2400)]),
 'skill_meteor_hit':(.28,[('stone',.65,0,65,1700),('fire',.3,.01,100,1500)]),
 'skill_soulseal_cast':(.32,[('tear',.3,0,80,1300),('wood',.35,.015,100,1700)]),
 'skill_soulseal_hit':(.32,[('tear',.7,0,80,2400),('stone',.35,.01,60,1600)]),
 'skill_starfall_cast':(.35,[('air',.4,0,80,1700),('tear',.25,.02,100,1800)]),
 'skill_starfall_fall':(.33,[('air',.7,0,100,2400),('fire',.3,0,80,1500)]),
 'skill_starfall_hit':(.58,[('impact',.55,0,45,1200),('stone',.65,.02,90,2300)]),
 'enemy_fast':(.09,[('air',.65,0,500,4000)]),'enemy_slow':(.16,[('fire',.4,0,100,1900)]),
 'enemy_beam':(.4,[('electric',.4,0,150,2400)]),'beam_stone':(.3,[('stone',.5,0,180,2800),('electric',.2,0,300,2200)]),
 'boss_laser_tell':(.5,[('electric',.25,0,120,1400),('metal',.35,.06,100,1800)]),
 'boss_recover':(.25,[('metal',.35,0,90,1000),('air',.3,0,90,1500)]),
 'paper_turn':(.23,[('wood',.35,0,400,3500),('air',.18,0,800,4500)]),
 }
 for family,mat in [('royal','electric'),('armor','metal'),('insect','wood'),('forge','impact'),('ice','crack'),('storm','electric'),('stone','stone')]:
  for action,duration in [('tell',.38),('phase',.65),('step',.18)]:
   parts=[(mat,.55,0,60,1500 if family=='royal' else 3000 if family=='storm' else 2300),('air',.22,.01,120,1500)]
   if family=='royal':parts.append(('metal',.28,.03,90,1400))
   if family=='forge':parts.append(('fire',.25,.025,80,2000))
   if family=='insect':parts.append(('tear',.2,.015,100,2100))
   profiles['boss_'+family+'_'+action]=(duration,parts)
 for key,(duration,parts) in profiles.items():
  for v in range(3):
   x=layer(parts,duration,v)
   if 'starfall_hit' in key or 'soulseal_hit' in key:
    t=np.arange(len(x))/RATE;x+=np.sin(2*np.pi*67*t)*np.exp(-t*18)*.35
   wav(key+'_'+str(v),x,.115 if 'step' not in key else .07)
 # An integer number of cycles, with a smooth periodic envelope, avoids loop clicks.
 t=np.arange(2*RATE)/RATE;x=(np.sin(2*np.pi*110*t)*.065+np.sin(2*np.pi*220*t)*.025)*(1+.07*np.sin(2*np.pi*2*t));wav('light_channel_loop',x,.055,True)
 for region in ['ice','fire','manor','capital','study']:
  rng=np.random.default_rng(sum(region.encode()));seconds=16;n=int(seconds*RATE);x=band(rng.normal(0,1,n),70,900 if region=='ice' else 1500);x*=.025/max(.001,float(np.sqrt(np.mean(x*x))))
  if region in ('manor','capital'):
   for at in [2,9]:
    length=int(RATE*3);tt=np.arange(length)/RATE;start=int(at*RATE);x[start:start+length]+=np.sin(2*np.pi*165*tt)*np.exp(-tt*1.8)*.05
  if region=='study':x=band(x,150,2200)*.35
  seam=int(.1*RATE);w=np.linspace(0,1,seam);edge=(x[:seam]+x[-seam:])*.5;x[:seam]=edge*(1-w)+x[:seam]*w;x[-seam:]=x[-seam:]*(1-w)+edge*w
  wav('ambient_'+region,x,.02,True)
 return {k: digest(samplepath(v)) for k,v in {'air':'spear','stone':'stone','metal':'hammer_hit','fire':'skill_meteor_cast','crack':'skill_ice_spike_hit','electric':'staff_electric','wood':'wood','dash':'warrior_dash','impact':'heavy','tear':'skill_mark_hit'}.items()}
def main():
 if '--fetch' in sys.argv:
  records=list(ThreadPoolExecutor(max_workers=3).map(fetch,SOURCES.items()));(CACHE/'sources.json').write_text(json.dumps(records,ensure_ascii=False,indent=2),encoding='utf-8');print('Cached',len(records),'verified CC0 music sources',flush=True);return
 arrangements=music() if '--cues' not in sys.argv else json.loads((OUT/'credits.json').read_text(encoding='utf-8'))['arrangements'];upstream=cues();credits={'musicSources':json.loads((CACHE/'sources.json').read_text(encoding='utf-8')),'arrangements':arrangements,'soundEffects':'New edits of previously attributed CC0 foley in NakiCombatAudioV3/NakiAudio; original low-frequency support, ambience and four-note story motif. No commercial-game samples.','upstreamHashes':upstream,'stats':STATS}
 if '--cues' in sys.argv:credits['stats']=[x for x in json.loads((OUT/'credits.json').read_text(encoding='utf-8'))['stats'] if not x['key'].startswith(('skill_','enemy_','beam_','boss_','paper_','ambient_','light_channel'))]+STATS
 (OUT/'credits.json').write_text(json.dumps(credits,ensure_ascii=False,indent=2),encoding='utf-8')
 import uuid
 for p in list(OUT.iterdir())+[OUT]:
  if p.suffix=='.meta':continue
  m=Path(str(p)+'.meta')
  if not m.exists():m.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\n',encoding='utf-8')
 print('Built',len(STATS),'assets; statistics and source evidence retained',flush=True)
if __name__=='__main__':main()
