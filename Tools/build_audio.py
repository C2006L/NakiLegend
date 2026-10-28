"""Original offline score and sound synthesis. No samples or third-party recordings.
Regenerate with Python, numpy and scipy. Output: Unity Resources/NakiAudio.
"""
from pathlib import Path
import numpy as np
from scipy.signal import butter, sosfilt
from scipy.io.wavfile import write

ROOT=Path(__file__).resolve().parents[1]/'Assets/NakiPrototype/Resources/NakiAudio'
ROOT.mkdir(parents=True,exist_ok=True)
SR=22050
rng=np.random.default_rng(19019)
manifest=[]
def save(name,x):
    x=np.nan_to_num(x); peak=float(np.max(np.abs(x)))
    if peak>.85:x=x*.85/peak
    write(ROOT/(name+'.wav'),SR,(x*32767).astype(np.int16))
    manifest.append((name,len(x)/SR,float(np.max(np.abs(x)))))
def tone(freq,dur,kind='pluck'):
    t=np.arange(int(SR*dur))/SR
    if kind=='flute': y=np.sin(2*np.pi*freq*t+.02*np.sin(2*np.pi*5*t))+.12*np.sin(4*np.pi*freq*t); env=np.minimum(t/.08,1)*np.minimum((dur-t)/.16,1)
    elif kind=='pad':y=sum(np.sin(2*np.pi*freq*k*t)/k**2 for k in [1,2,3]);env=np.minimum(t/.35,1)*np.minimum((dur-t)/.5,1)
    else:y=sum(np.sin(2*np.pi*freq*k*t)*np.exp(-t*(2+k)) / k**1.7 for k in [1,2,3,4]);env=np.minimum(t/.008,1)*np.minimum((dur-t)/.08,1)
    return y*np.clip(env,0,1)
def noise(n,cut=1600):return sosfilt(butter(2,cut,fs=SR,output='sos'),rng.normal(0,1,n))
def add(x,y,at,gain=1):
    start=int(at*SR);end=min(len(x),start+len(y))
    if end>start:x[start:end]+=y[:end-start]*gain
def midi(n):return 440*2**((n-69)/12)
# Shared eight-bar melody: the same young hero, with island-specific orchestration.
theme=[0,4,7,9,7,4,2,0, 2,5,9,7,5,2,4,7]
for island,key,scale in [('desert',50,[0,2,3,5,7,8,10]),('thunder',47,[0,2,3,5,7,9,10]),('woodland',52,[0,2,4,5,7,9,11])]:
    beat=.5;duration=32
    for state in ['explore','combat','boss']:
        x=np.zeros(SR*duration)
        for bar in range(16):
            root=key+[0,5,3,7][(bar//2)%4]
            for interval in [0,7,12]:add(x,tone(midi(root+interval),2.6,'pad'),bar*2,.048)
            for b in range(4):
                note=key+12+theme[(bar*2+b//2)%len(theme)]
                if b%2==0:add(x,tone(midi(note),.8,'flute' if island!='thunder' else 'pluck'),bar*2+b*beat,.10)
                add(x,tone(midi(root+scale[(bar+b)%7]),.48),bar*2+b*beat,.07)
                if state!='explore':
                    t=np.arange(int(.22*SR))/SR
                    kick=np.sin(2*np.pi*(52*t+10*(1-np.exp(-t*25))))*np.exp(-t*20)
                    add(x,kick,bar*2+b*beat,.12 if b%2==0 else .055)
                    add(x,noise(int(.08*SR),3600)*np.exp(-np.arange(int(.08*SR))/SR*65),bar*2+b*beat+.25,.035)
                if state=='boss':
                    add(x,tone(midi(root-12),.45),bar*2+b*beat,.18)
                    add(x,tone(midi(key+24+scale[(bar+b*2)%7]),.21),bar*2+b*beat+.25,.045)
        # All layers share a 32-second grid; fade only the edge to avoid clicks.
        fade=np.minimum(np.arange(len(x))/1100,1)*np.minimum((len(x)-1-np.arange(len(x)))/1100,1)
        save(island+'_'+state,x*fade*.65)
x=np.zeros(SR*16)
for i,n in enumerate([60,64,67,72,71,67,64,62,60]):add(x,tone(midi(n),2.5,'flute'),i*1.4,.19)
save('ending',x)
for island in ['desert','thunder','woodland','water']:
    n=SR*12;t=np.arange(n)/SR
    x=noise(n,350 if island=='desert' else 2200)*(.08 if island=='thunder' else .045)
    x*=.7+.3*np.sin(2*np.pi*t/12)**2
    if island=='woodland':
        for at in [1,3.7,7,9.5]:add(x,tone(1800,.12,'flute'),at,.045);add(x,tone(2300,.08,'flute'),at+.16,.03)
    if island=='water':x+=noise(n,600)*.07
    fade=np.minimum(t/.15,1)*np.minimum((12-t)/.15,1);save('ambient_'+island,x*fade)
for name in ['sword','heavy','spear','bow','crossbow','staff','flesh','shell','stone','wood','shield','break','hurt','death','dodge','ram','light','dark','wind','root','spore','electric','warn','thunder','pickup','clear','recover']:
    for variant in range(3):
        dur=1.1 if name=='thunder' else .55 if name in ['dark','wind','break','clear'] else .22
        t=np.arange(int(SR*dur))/SR;f=1+variant*.055
        hiss=noise(len(t),4000);low=noise(len(t),450)
        env=np.exp(-t*(5 if dur>.5 else 22))*np.minimum(t/.004,1)
        if name in ['sword','spear','dodge','wind']:y=hiss*np.sin(np.pi*np.minimum(t/dur,1))**2*.5+np.sin(2*np.pi*(300*t+250*t*t))*env*.08
        elif name in ['bow','crossbow']:y=tone(160*f,dur)*.35+hiss*env*(.22 if name=='crossbow' else .09)
        elif name in ['heavy','stone','ram','thunder']:y=(low*.9+np.sin(2*np.pi*65*f*t)*.3)*env+hiss*env*.1
        elif name in ['shield','break','shell','electric','warn']:y=sum(np.sin(2*np.pi*k*f*t) for k in [480,737,1131])*.12*env+hiss*env*.12
        elif name in ['light','dark','staff','spore','pickup','recover','clear']:y=sum(np.sin(2*np.pi*k*f*t) for k in ([180,270,361] if name=='dark' else [660,880,1320]))*.12*env
        else:y=low*env*.55+np.sin(2*np.pi*125*f*t)*env*.15
        save(name+'_'+str(variant),y*.7)
(ROOT/'manifest.txt').write_text('\n'.join(f'{n}\t{d:.3f}s\tpeak={p:.4f}' for n,d,p in manifest),encoding='utf-8')
print(f'Generated {len(manifest)} original WAV assets in {ROOT}')
