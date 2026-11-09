"""Original vowel-choir and laser cues. No thrown-weapon/air foley.
Only replaces the three light abilities and their sustained channel.
"""
from pathlib import Path
import numpy as np,json,shutil,hashlib
from scipy.io import wavfile
R=Path(__file__).resolve().parents[1];bank=R/'Assets/NakiPrototype/Resources/NakiSoundV25';qa=R.parent/'Validation/LightAudioV27';qa.mkdir(parents=True,exist_ok=True);SR=22050
def choir(t,variant=0):
    y=np.zeros_like(t)
    for f,g in [(147,1),(185,.65),(220,.75),(294,.35)]:
        # Quantized frequencies and cyclic vibrato make the four-second loop periodic.
        f+=variant*.25;phase=2*np.pi*f*t+.025*np.sin(2*np.pi*5*t)
        voice=np.zeros_like(t)
        for h in range(1,25):
            hz=f*h;form=.06+np.exp(-((hz-720)/210)**2)+.45*np.exp(-((hz-1150)/280)**2)
            voice+=np.sin(h*phase)*form/h
        y+=voice*g
    return y*.23
def laser(t):return .32*np.sin(2*np.pi*110*t)+.12*np.sin(2*np.pi*220*t)+.035*np.sin(2*np.pi*660*t)*(1+.12*np.sin(2*np.pi*8*t))
def tail(x):
    y=x.copy()
    for delay,g in [(.043,.13),(.079,.08),(.119,.04)]:
        n=int(delay*SR)
        if n<len(x):y[n:]+=x[:-n]*g
    return y
records=[]
profiles={'lightbeam_cast':(.65,.10,.22,.55),'lightbeam_hit':(.19,.008,.13,.15),'lightbeam_end':(.28,.012,.23,.25),'lightguard_cast':(.8,.13,.36,1),'lightguard_hit':(.34,.015,.25,.75),'lightguard_end':(.45,.025,.35,.9),'solar_cast':(.64,.1,.32,.8),'solar_pulse':(.27,.015,.19,.45),'solar_hit':(.23,.008,.17,.3)}
for key,(seconds,attack,release,choral) in profiles.items():
    for variant in range(3):
        t=np.arange(round(seconds*SR))/SR;env=np.minimum(t/attack,1)*np.minimum((seconds-t)/release,1)
        x=tail((choir(t,variant)*choral+laser(t)*(1-choral*.7))*env)
        x*=np.minimum((seconds-t)/.035,1);x-=x.mean();x*=min(.105/max(.001,np.sqrt(np.mean(x*x))),.7/max(.001,np.abs(x).max()))
        name='skill_'+key+'_'+str(variant)+'.wav';p=bank/name;backup=qa/'before'/name;backup.parent.mkdir(exist_ok=True)
        if not backup.exists():shutil.copy2(p,backup)
        wavfile.write(p,SR,(x*32767).astype(np.int16));records.append({'file':name,'peak':float(np.abs(x).max()),'rms':float(np.sqrt(np.mean(x*x))),'seconds':seconds,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()})
t=np.arange(4*SR)/SR;x=choir(t)*.8+laser(t)*.55;x-=x.mean();x*=.065/np.sqrt(np.mean(x*x));p=bank/'light_channel_loop.wav';shutil.copy2(p,qa/'before/light_channel_loop.wav') if not (qa/'before/light_channel_loop.wav').exists() else None;edge=110;x[:edge]*=np.sin(np.linspace(0,np.pi/2,edge))**2;x[-edge:]*=np.cos(np.linspace(0,np.pi/2,edge))**2;wavfile.write(p,SR,(x*32767).astype(np.int16));records.append({'file':p.name,'seconds':4,'peak':float(np.abs(x).max()),'loopJump':float(abs(x[-1]-x[0])),'sha256':hashlib.sha256(p.read_bytes()).hexdigest()})
assert np.max(np.abs(x))<.7 and abs(x[-1]-x[0])<.015
(qa/'assets.json').write_text(json.dumps(records,indent=2),encoding='utf-8')
credit=bank/'credits.json';data=json.loads(credit.read_text(encoding='utf-8'));data['lightRevisionV27']={'method':'Original additive vowel-formant choir and layered laser tones, no air/weapon samples','files':[r['file'] for r in records]};credit.write_text(json.dumps(data,ensure_ascii=False,indent=2),encoding='utf-8')
parts=[]
for key in ['skill_lightbeam_cast_0','light_channel_loop','skill_lightbeam_hit_0','skill_lightguard_cast_0','skill_lightguard_hit_0','skill_solar_cast_0','skill_solar_pulse_0']:
    rate,x=wavfile.read(bank/(key+'.wav'));parts.extend([x,np.zeros(SR//3,np.int16)])
wavfile.write(qa/'light-skills-preview.wav',SR,np.concatenate(parts));print('Built and measured',len(records),'choir/laser clips')
