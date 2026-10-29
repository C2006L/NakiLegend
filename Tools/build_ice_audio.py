"""Create only the ice sample's new spell sounds from existing CC0 derivatives.
Never overwrites the accepted score, sword recordings, or meteor sounds.
"""
from pathlib import Path
import json,hashlib
import numpy as np
from scipy.signal import resample_poly,butter,sosfilt
from scipy.io.wavfile import read,write

root=Path(__file__).resolve().parents[1]
out=root/'Assets/NakiPrototype/Resources/NakiSkills'
records=[]
for skill,source,pitch in [('ice_spike','orb',1.55),('ice_ring','wave',1.2)]:
    for phase in ['cast','hit']:
        for variant in range(3):
            path=out/f'skill_{source}_{phase}_{variant}.wav'
            rate,data=read(path);x=data.astype(float)/32768
            x=resample_poly(x,100,round(pitch*(1+variant*.025)*100))
            x=sosfilt(butter(2,650,btype='highpass',fs=rate,output='sos'),x)
            if skill=='ice_ring':
                delay=int(rate*.075);echo=np.zeros(len(x)+delay);echo[:len(x)]+=x;echo[delay:]+=x*.28;x=echo
            fade=min(300,len(x)//4);x[:fade]*=np.linspace(0,1,fade);x[-fade:]*=np.linspace(1,0,fade)
            gain=min(.13/max(.001,np.sqrt(np.mean(x*x))),.7/max(.001,np.max(np.abs(x))));x*=gain
            name=f'skill_{skill}_{phase}_{variant}.wav';write(out/name,rate,(x*32767).astype(np.int16))
            records.append({'file':name,'source_derivative':path.name,'source_sha256':hashlib.sha256(path.read_bytes()).hexdigest(),'seconds':len(x)/rate})
(out/'ice-credits.json').write_text(json.dumps({'author':'JaggedStone','license':'CC0-1.0','source_page':'https://opengameart.org/content/magic-spell-sfx','source_manifest':'credits.json','edits':'Resampled, high-pass filtered, short echo on frost ring; faded and level matched. Existing audio untouched.','clips':records},ensure_ascii=False,indent=2),encoding='utf-8')
print('Created 12 ice spell variants')
