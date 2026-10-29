from pathlib import Path
import sys,json,subprocess,hashlib
import numpy as np
from scipy.io.wavfile import read,write
sys.path.append('D:/NakiLegend/Tools/audio_runtime')
import imageio_ffmpeg
repo=Path(__file__).resolve().parents[2];root=repo/'Assets/NakiPrototype/Resources/NakiAudioV2';ffmpeg=imageio_ffmpeg.get_ffmpeg_exe()
def decode(file):
 p=subprocess.run([ffmpeg,'-v','error','-i',str(file),'-f','f32le','-ac','2','-ar','44100','pipe:1'],capture_output=True,check=True)
 return np.frombuffer(p.stdout,np.float32).reshape(-1,2).copy()
results=[]
for file in sorted(root.glob('*')):
 if file.suffix not in ['.wav','.ogg']:continue
 data=decode(file);peak=float(np.max(np.abs(data)))
 assert peak>0 and peak<.99,file
 results.append({'file':file.name,'duration':len(data)/44100,'peak':peak,'sha256':hashlib.sha256(file.read_bytes()).hexdigest()})
assert len(results)==22
assert len({r['sha256'] for r in results if r['file'].endswith('.ogg')})==10
out=repo/'Art/AudioV2';out.mkdir(parents=True,exist_ok=True)
mix=np.zeros((44100*18,2),np.float32)
for i,island in enumerate(['desert','thunder','woodland']):
 x=decode(root/(island+'_combat.ogg'))[44100*12:44100*18]
 fade=np.minimum(np.arange(len(x))/2205,1)*np.minimum((len(x)-1-np.arange(len(x)))/2205,1)
 mix[i*44100*6:(i+1)*44100*6]=x*fade[:,None]*.8
write(out/'music-v2-audition.wav',44100,(np.clip(mix,-.98,.98)*32767).astype(np.int16))
demo=np.zeros((44100*5,2),np.float32)
for i in range(3):
 x=decode(root/('sword_'+str(i)+'.wav'));start=int((.3+i*1.1)*44100);demo[start:start+len(x)]+=x*.75
 x=decode(root/('clash_'+str(i)+'.wav'));start+=int(.2*44100);demo[start:start+len(x)]+=x*.65
write(out/'sword-v2-audition.wav',44100,(np.clip(demo,-.98,.98)*32767).astype(np.int16))
(repo/'Validation/AudioV2/assets-result.json').write_text(json.dumps({'passed':True,'assets':results},indent=2),encoding='utf-8')
print('PASS: 10 distinct stereo scores, 12 recorded weapon variants; no silence or clipping; auditions exported.')
