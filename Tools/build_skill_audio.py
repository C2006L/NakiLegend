"""Create dedicated spell cast/impact variants from JaggedStone's CC0 Magic Spell SFX.
Does not write music, sword recordings, or any NakiAudioV2 resource.
Requires requests, numpy, scipy, and workspace imageio-ffmpeg.
"""
from pathlib import Path
import sys,subprocess,json,hashlib
import requests,numpy as np
from scipy.signal import resample_poly,butter,sosfilt
from scipy.io.wavfile import write
sys.path.append('D:/NakiLegend/Tools/audio_runtime')
import imageio_ffmpeg
repo=Path(__file__).resolve().parents[1];cache=repo/'References/SkillAudio';cache.mkdir(parents=True,exist_ok=True)
out=repo/'Assets/NakiPrototype/Resources/NakiSkills';out.mkdir(parents=True,exist_ok=True)
ffmpeg=imageio_ffmpeg.get_ffmpeg_exe();rate=44100
page='https://opengameart.org/content/magic-spell-sfx'
html=requests.get(page,timeout=40).text;assert '>CC0<' in html or 'publicdomain/zero' in html
sources=[];clips=[]
for n in range(1,8):
 name=f'magical_{n}'+('_0' if n in [1,6,7] else '')+'.ogg';url='https://opengameart.org/sites/default/files/'+name;p=cache/name
 if not p.exists():r=requests.get(url,timeout=50);r.raise_for_status();p.write_bytes(r.content)
 data=subprocess.run([ffmpeg,'-v','error','-i',str(p),'-f','f32le','-ac','1','-ar',str(rate),'pipe:1'],capture_output=True,check=True).stdout
 x=np.frombuffer(data,np.float32).copy();nz=np.flatnonzero(np.abs(x)>.003);x=x[max(0,nz[0]-150):nz[-1]+1];clips.append(x)
 sources.append({'author':'JaggedStone','license':'CC0-1.0','page':page,'download':url,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()})
profiles={'orb':(0,1.15),'meteor':(3,.65),'blade':(6,1.35),'wave':(4,.85),'mark':(1,1.45),'arc':(5,1.7),'root':(2,.55),'spore':(4,1.3)}
records=[]
for key,(index,pitch) in profiles.items():
 for phase in ['cast','hit']:
  for variant in range(3):
   speed=pitch*(1+variant*.035)*(1.12 if phase=='hit' else 1)
   x=resample_poly(clips[index],100,int(speed*100));duration=(.30 if phase=='hit' else .48) if key not in ['meteor','wave'] else (.4 if phase=='hit' else .65)
   if phase=='hit':x=x[int(len(x)*.18):]
   x=x[:int(rate*duration)].copy()
   if key in ['root','meteor']:x=sosfilt(butter(2,1600,fs=rate,output='sos'),x)
   if key=='arc':x*=.7+.3*np.sin(np.arange(len(x))/rate*2*np.pi*35)
   if key=='spore':x*=np.exp(-np.arange(len(x))/rate*6)
   fade=min(440,len(x)//4);x[:fade]*=np.linspace(0,1,fade);x[-fade:]*=np.linspace(1,0,fade)
   gain=min(.19/max(np.sqrt(np.mean(x*x)),.001),.8/max(np.max(np.abs(x)),.001));x*=gain
   name=f'skill_{key}_{phase}_{variant}';write(out/(name+'.wav'),rate,(x*32767).astype(np.int16));records.append({'file':name+'.wav','seconds':len(x)/rate,'source':sources[index]['download']})
(out/'credits.json').write_text(json.dumps({'sources':sources,'edits':'Trimmed, pitch-shifted, filtered and level-matched cast/impact variants.','clips':records},indent=2),encoding='utf-8')
subprocess.run([sys.executable,str(repo/'Tools/revise_meteor_cast.py')],check=True)
preview=repo/'Art/SkillAudio';preview.mkdir(parents=True,exist_ok=True)
from scipy.io.wavfile import read
mix=np.zeros(rate*16)
for i,key in enumerate(profiles):
 for offset,phase in [(.1,'cast'),(.9,'hit')]:
  x=read(out/f'skill_{key}_{phase}_0.wav')[1]/32768
  start=int((i*2+offset)*rate);mix[start:start+len(x)]+=x*.8
write(preview/'skills-cast-and-impact.wav',rate,(np.clip(mix,-.95,.95)*32767).astype(np.int16))
print('Generated 48 dedicated spell variants and a cast/impact audition.')
