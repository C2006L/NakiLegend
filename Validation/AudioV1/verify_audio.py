from pathlib import Path
import json, hashlib
import numpy as np
from scipy.io.wavfile import read,write
repo=Path(__file__).resolve().parents[2]
root=repo/'Assets/NakiPrototype/Resources/NakiAudio'
checks=[]
for p in sorted(root.glob('*.wav')):
 rate,data=read(p)
 assert rate==22050 and data.ndim==1 and len(data)>1000,p
 assert 0<np.max(np.abs(data.astype(float)))<32760,p
 checks.append({'file':p.name,'seconds':len(data)/rate,'peak':float(np.max(np.abs(data.astype(float)))/32768)})
assert len(checks)==95
for island in ['desert','thunder','woodland']:
 assert len({read(root/(island+'_'+state+'.wav'))[1].shape for state in ['explore','combat','boss']})==1
for name in ['sword','bow','thunder','shield']:
 assert len({hashlib.sha256((root/(name+'_'+str(i)+'.wav')).read_bytes()).hexdigest() for i in range(3)})==3
out=Path(__file__).parent
(out/'assets-result.json').write_text(json.dumps({'passed':True,'assets':checks},indent=2),encoding='utf-8')
# A clearly labelled offline audition mix, not a recording of Unity output.
mix=np.zeros(22050*18)
for i,island in enumerate(['desert','thunder','woodland']):
 data=read(root/(island+'_combat.wav'))[1].astype(float)/32768
 segment=data[:22050*6].copy();fade=np.minimum(np.arange(len(segment))/2205,1)*np.minimum((len(segment)-1-np.arange(len(segment)))/2205,1)
 mix[i*22050*6:(i+1)*22050*6]+=segment*fade*.65
for at,name in [(1,'sword'),(2,'shell'),(4,'bow'),(7,'warn'),(8,'thunder'),(10,'shield'),(13,'root'),(15,'spore'),(16,'pickup')]:
 data=read(root/(name+'_0.wav'))[1].astype(float)/32768
 mix[at*22050:at*22050+len(data)]+=data*.7
preview=repo/'Art/AudioV1';preview.mkdir(parents=True,exist_ok=True)
write(preview/'three-islands-audition.wav',22050,(np.clip(mix,-.95,.95)*32767).astype(np.int16))
print('PASS: 95 non-silent WAVs; no clipping; matched music loop lengths; effect variations; audition exported.')
