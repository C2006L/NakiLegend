"""Build dry weapon voices from the existing licensed Foley library; no skill/music edits."""
from pathlib import Path
import json,hashlib
import numpy as np
from scipy.io import wavfile
from scipy.signal import butter,sosfilt,resample_poly
root=Path(__file__).resolve().parents[2];resources=root/'Assets/NakiPrototype/Resources';out=resources/'NakiCombatAudioV3';rate=44100
rows=json.loads((root/'Art/WeaponIdentityV3/catalog.json').read_text(encoding='utf-8'));records=[]
def read(key):
 for folder in ['NakiCombatAudioV3','NakiAudioV2','NakiAudio']:
  p=resources/folder/(key+'.wav')
  if p.exists():
   sr,a=wavfile.read(p);a=a.astype(float)/(32768 if a.dtype.kind in 'iu' else 1)
   if a.ndim>1:a=a.mean(axis=1)
   if sr!=rate:a=resample_poly(a,rate,sr)
   return a,str(p.relative_to(root))
 raise RuntimeError(key)
hashes=set()
for w in rows:
 i=w['id'];rng=np.random.default_rng(98231+i*331);name=w['name'];kind=w['type'];heavy='锤' in name or '槌' in name
 base='hammer' if heavy else 'spear' if kind=='Spear' else 'heavy' if kind=='Sword' and ('重' in name) else 'sword' if kind=='Sword' else 'crossbow' if '弩' in name else 'bow' if kind=='Bow' else 'staff_electric' if w['element']=='Lightning' else 'staff'
 for phase in ['cast','hit']:
  key=base if phase=='cast' else 'hammer_hit' if heavy else 'clash' if kind in ['Sword','Spear'] else 'wood' if w['element']=='Wood' else 'stone' if w['element']=='Ice' else 'shell' if kind=='Bow' else 'flesh'
  a,source=read(key+f'_{i%3}');duration=(.17 if phase=='cast' else .14)+(.07 if heavy else .035 if kind=='Staff' else 0)+(i%5)*.008
  n=int(rate*duration);t=np.arange(n)/rate
  # Changing contact size, layered materials and timing differentiates voices without tonal whistles.
  tilt=2400+(i%7)*420 if heavy else 4200+(i%9)*410
  dry=sosfilt(butter(2,tilt,fs=rate,output='sos'),a)
  f=np.zeros(n);take=min(n,len(dry));f[:take]=dry[:take]*np.exp(-t[:take]/(duration*.75))
  secondary,secondSource=read(('stone' if w['element']=='Ice' else 'wood' if w['element']=='Wood' or kind=='Bow' else 'clash')+f'_{(i+1)%3}')
  delay=int(rate*(.008+.003*(i%6)));take=min(n-delay,len(secondary));f[delay:delay+take]+=secondary[:take]*np.exp(-t[:take]/.037)*(.19 if phase=='cast' else .31)
  # Small broadband grit: icy chips, flame crackle, electric snaps, or a short blunt body.
  noise=rng.normal(0,1,n);lo={'Ice':2600,'Fire':850,'Lightning':1700,'Wind':1400,'Wood':700}.get(w['element'],1100)
  grit=sosfilt(butter(2,[lo,min(11000,lo+3200+(i%4)*500)],btype='bandpass',fs=rate,output='sos'),noise)
  env=np.exp(-t/(.014+(i%6)*.004))
  f+=grit*env*(.045 if kind=='Staff' else .015)
  if heavy:f+=np.sin(2*np.pi*(78+(i%4)*17)*t)*np.exp(-t/.024)*.10
  f*=np.minimum(1,t/.0012)*np.minimum(1,(duration-t)/.018)
  peak=.36 if phase=='cast' else .29;f=f/max(abs(f).max(),.001)*peak
  path=out/f'weapon_{i:03}_{phase}.wav';wavfile.write(path,rate,np.int16(np.clip(f,-1,1)*32767))
  digest=hashlib.sha256(path.read_bytes()).hexdigest();assert digest not in hashes;hashes.add(digest)
  records.append(dict(id=i,phase=phase,file=path.name,source=source,secondary=secondSource,sha256=digest,peak=float(abs(f).max()),rms=float(np.sqrt(np.mean(f*f)))))
(root/'Art/WeaponIdentityV3/audio-audit.json').write_text(json.dumps(dict(license='Derived from the existing CC0 Foley sources; see NakiAudioV2/credits.json and NakiCombatAudioV3/credits.json. No third-party game audio copied.',method='Dry material layering, contact timing, filtered transients and short decay. Existing music and skill sounds unchanged.',uniqueFiles=len(hashes),records=records),indent=2),encoding='utf-8')
print(f'{len(hashes)} unique dry attack/contact voices built; peak <= 0.36, no clipping')
