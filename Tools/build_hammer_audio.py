from pathlib import Path
import numpy as np,json,hashlib
from scipy.io import wavfile
from scipy.signal import butter,sosfilt,resample_poly
root=Path(__file__).resolve().parents[1];out=root/'Assets/NakiPrototype/Resources/NakiCombatAudioV3';rate=44100
records=[]
for v in range(3):
 src=root/f'Assets/NakiPrototype/Resources/NakiAudioV2/clash_{v}.wav'
 sr,x=wavfile.read(src);denom=32768 if x.dtype.kind in 'iu' else 1;x=x.astype(float)/denom
 if x.ndim>1:x=x.mean(axis=1)
 if sr!=rate:x=resample_poly(x,rate,sr)
 start=np.flatnonzero(abs(x)>.005);x=x[start[0]:] if len(start) else x
 for key,duration,gain in [('hammer',.17,.33),('hammer_hit',.24,.7)]:
  n=int(rate*duration);f=np.zeros(n);metal=sosfilt(butter(2,4200,fs=rate,output='sos'),x[:n]);f[:len(metal)]=metal*np.exp(-np.arange(len(metal))/rate/.065)*.5
  t=np.arange(n)/rate;f+=np.sin(2*np.pi*(105*t-65*t*t))*np.exp(-t/.036)*.35
  f*=np.minimum(1,t/.001)*np.minimum(1,(duration-t)/.02);f=f/max(abs(f).max(),.001)*gain
  wavfile.write(out/f'{key}_{v}.wav',rate,np.int16(np.clip(f,-1,1)*32767))
 records.append({'source':str(src.relative_to(root)),'sha256':hashlib.sha256(src.read_bytes()).hexdigest(),'license':'CC0-1.0; existing NakiAudioV2 credits.json attribution retained','variant':v})
p=root/'Art/ArsenalV1';p.mkdir(exist_ok=True);(p/'hammer-audio-credits.json').write_text(json.dumps({'method':'existing licensed metal contact, lowpass, dry envelope and short authored impact body; no sword swing samples','sources':records},ensure_ascii=False,indent=2),encoding='utf-8')
