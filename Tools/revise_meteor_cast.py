"""Revise only the three meteor CAST clips, using the cached CC0 spell sources.
No network calls. All other skill, music and weapon audio is left untouched.
"""
from pathlib import Path
import sys,subprocess,json,hashlib
import numpy as np
from scipy.signal import butter,sosfilt,resample_poly
from scipy.io.wavfile import read,write
sys.path.append('D:/NakiLegend/Tools/audio_runtime')
import imageio_ffmpeg
repo=Path(__file__).resolve().parents[1];cache=repo/'References/SkillAudio';out=repo/'Assets/NakiPrototype/Resources/NakiSkills'
preview=repo/'Art/MeteorAudioV2';preview.mkdir(parents=True,exist_ok=True)
rate=44100;ffmpeg=imageio_ffmpeg.get_ffmpeg_exe()
def decode(name):
 p=cache/name
 data=subprocess.run([ffmpeg,'-v','error','-i',str(p),'-f','f32le','-ac','1','-ar',str(rate),'pipe:1'],capture_output=True,check=True).stdout
 return np.frombuffer(data,np.float32).copy(),hashlib.sha256(p.read_bytes()).hexdigest()
core,corehash=decode('magical_6_0.ogg');edge,edgehash=decode('magical_7_0.ogg')
old=read(out/'skill_meteor_cast_0.wav')[1].astype(float)/32768
def layer(source,speed,duration,low,high):
 x=resample_poly(source,100,int(speed*100))
 # Select a strong short attack, discarding the long pitched sequence.
 width=int(rate*duration);limit=min(len(x)-width,int(rate*.7))
 starts=range(0,max(1,limit),220)
 start=max(starts,key=lambda a:np.mean(x[a:a+width]**2));x=x[start:start+width].copy()
 x=sosfilt(butter(2,[low,high],btype='bandpass',fs=rate,output='sos'),x)
 phase=np.linspace(0,1,len(x));env=np.minimum(phase/.055,1)*(1-phase)**1.6
 x*=env;return x/max(np.sqrt(np.mean(x*x)),.0001)
results=[]
for variant in range(3):
 x=np.zeros(int(rate*.31));speed=1+variant*.035
 a=layer(core,.88*speed,.17,120,1250);b=layer(edge,1.4*speed,.13,650,4300)
 x[:len(a)]+=a*.09
 for at,gain in [(.035,.085),(.085,.06)]:
  start=int(at*rate);x[start:start+len(b)]+=b*gain
 # Very short, damped release. No long rumble, hiss bed, or note sequence.
 x*=min(1,.72/max(np.max(np.abs(x)),.001));x[-440:]*=np.linspace(1,0,440)
 p=out/f'skill_meteor_cast_{variant}.wav';write(p,rate,(x*32767).astype(np.int16))
 results.append({'file':p.name,'seconds':len(x)/rate,'peak':float(np.max(np.abs(x))),'sourceSha256':[corehash,edgehash]})
records=json.loads((out/'credits.json').read_text(encoding='utf-8'))
records['meteorCastRevision']={'description':'Short filtered core and staggered twin launch layers; original cast only, impact unchanged.','sources':['https://opengameart.org/sites/default/files/magical_6_0.ogg','https://opengameart.org/sites/default/files/magical_7_0.ogg'],'clips':results}
for clip in records['clips']:
 if clip['file'].startswith('skill_meteor_cast_'):
  clip['seconds']=.31;clip['source']='https://opengameart.org/sites/default/files/magical_6_0.ogg';clip['additionalSource']='https://opengameart.org/sites/default/files/magical_7_0.ogg'
(out/'credits.json').write_text(json.dumps(records,indent=2),encoding='utf-8')
mix=np.zeros(rate*5)
for i in range(3):
 new=read(out/f'skill_meteor_cast_{i}.wav')[1].astype(float)/32768;start=int((.3+i*1.2)*rate);mix[start:start+len(new)]+=new
write(preview/'meteor-cast-v2.wav',rate,(mix*32767).astype(np.int16))
comparison=np.zeros(rate*3);comparison[:len(old)]=old;new=read(out/'skill_meteor_cast_0.wav')[1]/32768;comparison[int(rate*1.5):int(rate*1.5)+len(new)]=new
write(preview/'before-after.wav',rate,(np.clip(comparison,-.98,.98)*32767).astype(np.int16))
(preview/'revision.json').write_text(json.dumps(results,indent=2),encoding='utf-8')
print('Updated only 3 meteor cast clips. New duration 0.31 seconds. Auditions exported.')
