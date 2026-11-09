from pathlib import Path
import numpy as np
from scipy.io import wavfile
from scipy.signal import resample_poly,butter,sosfilt
import uuid,json,hashlib
root=Path(__file__).resolve().parents[2];out=root/'Assets/NakiPrototype/Resources/NakiCombatAudioV3'
records=[]
for phase,source in [('cast','spear_1'),('hit','clash_2')]:
    paths=[root/'Assets/NakiPrototype/Resources'/folder/(source+'.wav') for folder in ['NakiCombatAudioV3','NakiAudioV2','NakiAudio']]
    src=next(p for p in paths if p.exists());rate,a=wavfile.read(src)
    a=a.astype(np.float64)/32768
    if a.ndim>1:a=a.mean(axis=1)
    a=resample_poly(a,19,21);a=sosfilt(butter(2,4300,fs=rate,output='sos'),a)
    n=min(len(a),int(rate*.24));a=a[:n];t=np.arange(n)/rate
    a=a*np.exp(-t/.12)+.035*np.sin(2*np.pi*173*t)*np.exp(-t/.025)
    a*=np.minimum(1,t/.002)*np.minimum(1,(n/rate-t)/.018)
    a*=.32/max(.001,abs(a).max());dst=out/f'weapon_132_{phase}.wav';wavfile.write(dst,rate,np.int16(a*32767))
    meta=Path(str(dst)+'.meta')
    if not meta.exists():meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\n',encoding='utf-8')
    records.append(dict(file=dst.name,source=str(src.relative_to(root)),sha256=hashlib.sha256(dst.read_bytes()).hexdigest()))
(root/'Art/ManorCourtyardV2/audio-credits.json').write_text(json.dumps(dict(license='Derived from project CC0 Foley. See NakiAudioV2/credits.json and NakiCombatAudioV3/credits.json.',method='Dry spear body and brass contact, filtered/resampled with a short metal transient.',records=records),indent=2),encoding='utf-8')
print('Built two unique captain spear Foley clips; existing music/skills unchanged')
