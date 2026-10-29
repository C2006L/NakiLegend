"""Short, dry combat cues from licensed foley; keeps all original banks intact."""
from pathlib import Path
import sys,zipfile,json,hashlib,subprocess
import numpy as np
from scipy.signal import butter,sosfilt,resample_poly
from scipy.io.wavfile import write
sys.path.insert(0,'D:/NakiLegend/Tools/combat_audio_runtime')
sys.path.insert(0,'D:/NakiLegend/Tools/audio_runtime')
import py7zr,imageio_ffmpeg
ROOT=Path(__file__).resolve().parents[1];CACHE=ROOT/'References/CombatAudioV3';OUT=ROOT/'Assets/NakiPrototype/Resources/NakiCombatAudioV3';OUT.mkdir(parents=True,exist_ok=True)
RATE=44100;ffmpeg=imageio_ffmpeg.get_ffmpeg_exe()
protected={str(p.relative_to(ROOT)):hashlib.sha256(p.read_bytes()).hexdigest() for folder in ['NakiAudioV2','NakiSkills'] for p in (ROOT/'Assets/NakiPrototype/Resources'/folder).iterdir() if p.suffix in ['.ogg','.wav']}
evidence=ROOT/'Validation/CombatIceV2/runtime';evidence.mkdir(parents=True,exist_ok=True);(evidence/'protected-audio.json').write_text(json.dumps(protected,indent=2),encoding='utf-8')
selected=['English Longbow Shoot.wav','Scythian Recurve Shoot.wav','Crossbow Shoot.wav','Arrow Fletching.wav','Katana Swing.wav']
recorded=CACHE/'recorded'
if not all((recorded/name).exists() for name in selected):
    with py7zr.SevenZipFile(CACHE/'medieval_sfx_textures_1_of_2.7z') as z:z.extract(path=recorded,targets=selected)
def decode(source):
    raw=isinstance(source,bytes)
    data=subprocess.run([ffmpeg,'-v','error','-i','pipe:0' if raw else str(source),'-f','f32le','-ac','1','-ar',str(RATE),'pipe:1'],input=source if raw else None,capture_output=True,check=True).stdout
    return np.frombuffer(data,np.float32).astype(float)
with zipfile.ZipFile(CACHE/'80-CC0-RPG-SFX_0.zip') as z:
    sounds={n:decode(z.read(n)) for n in z.namelist() if n.endswith('.ogg') and any(k in n for k in ['blade','spell','stones_','item_wood','item_gem'])}
for name in selected:sounds[name]=decode(recorded/name)
for name in ['spark.wav','continuousspark.wav']:sounds[name]=decode(CACHE/name)
def source(name,seconds,variant=0,high=100,low=6500):
    x=sounds[name].copy();nz=np.flatnonzero(np.abs(x)>max(.003,np.max(np.abs(x))*.045));assert len(nz),name
    # Recorded weapons contain multiple isolated takes. Select an attack, never
    # stretch a long bowed/string tail into an artificial musical note.
    if name.endswith('Shoot.wav'):
        windows=np.array([np.max(np.abs(x[i:i+220])) for i in range(0,len(x),220)])
        active=np.flatnonzero(windows>max(.015,windows.max()*.15));starts=[int(active[0])*220]
        for a,b in zip(active,active[1:]):
            if b-a>30:starts.append(int(b)*220)
        start=max(0,starts[variant%len(starts)]-150)
    else:start=max(0,int(nz[0])-100)
    x=x[start:start+int(seconds*RATE)]
    x=sosfilt(butter(2,[high,min(low,RATE*.45)],btype='bandpass',fs=RATE,output='sos'),x)
    if len(x):x/=max(.001,np.sqrt(np.mean(x*x)))
    return x
def layer(parts,duration,variant=0):
    result=np.zeros(round(duration*RATE))
    for name,gain,delay,high,low in parts:
        x=source(name,duration-delay,variant,high,low);start=round(delay*RATE);length=min(len(x),len(result)-start)
        result[start:start+length]+=x[:length]*gain
    return result
# Components are real movement, fire/stone/wood foley and recorded Tesla sparks.
# There are no oscillators, pitch sweeps, vocals, artificial reverb or long echoes.
profiles={
 'blade_cast':(.17,[('Katana Swing.wav',1,0,300,8500)]),
 'blade_hit':(.13,[('blade_03.ogg',.65,0,350,6500),('stones_01.ogg',.2,0,120,2500)]),
 'wave_cast':(.27,[('Katana Swing.wav',.7,0,180,7000),('blade_01.ogg',.25,.025,300,7000)]),
 'wave_hit':(.15,[('blade_02.ogg',.6,0,250,6000)]),
 'meteor_cast':(.19,[('spell_fire_01.ogg',.65,0,100,1900),('item_wood_01.ogg',.25,0,100,1600)]),
 'meteor_hit':(.21,[('stones_02.ogg',.7,0,80,2400),('spell_fire_02.ogg',.25,0,150,2600)]),
 'mark_cast':(.12,[('spark.wav',.75,0,250,6000)]),
 'mark_hit':(.19,[('continuousspark.wav',.7,0,180,6000),('stones_01.ogg',.3,0,90,2000)]),
 'arc_cast':(.14,[('continuousspark.wav',.6,0,400,7500),('spark.wav',.25,.018,250,6500)]),
 'arc_hit':(.15,[('spark.wav',.7,0,250,7500)]),
 'orb_cast':(.19,[('blade_01.ogg',.55,0,400,4300),('item_gem_01.ogg',.15,.015,700,4000)]),
 'orb_hit':(.16,[('stones_01.ogg',.35,0,250,3400),('item_gem_01.ogg',.2,0,700,4200)]),
 'root_cast':(.18,[('item_wood_01.ogg',.55,0,120,4200),('stones_03.ogg',.25,.018,100,2300)]),
 'root_hit':(.15,[('item_wood_02.ogg',.65,0,150,4500)]),
 'spore_cast':(.17,[('item_wood_03.ogg',.4,0,300,5200),('blade_02.ogg',.2,.015,600,5000)]),
 'spore_hit':(.13,[('item_wood_01.ogg',.5,0,250,5000)]),
 'ice_spike_cast':(.17,[('stones_04.ogg',.6,0,450,7200)]),
 'ice_spike_hit':(.16,[('stones_03.ogg',.6,0,350,6500),('item_gem_01.ogg',.1,0,1000,6000)]),
 'ice_ring_cast':(.25,[('stones_04.ogg',.45,0,300,6500),('Katana Swing.wav',.25,.025,200,5000)]),
 'ice_ring_hit':(.17,[('stones_02.ogg',.5,0,300,6500)]),
}
clips={};records=[]
def output(name,data,components):
    x=data.copy();fade=min(180,len(x)//4);x[:fade]*=np.linspace(0,1,fade)
    # Fast release kills the rubber-string ring and repetitive nasal tails.
    x*=np.exp(-np.arange(len(x))/RATE*7);x[-min(700,len(x)) :]*=np.linspace(1,0,min(700,len(x)))
    gain=min(.135/max(.001,np.sqrt(np.mean(x*x))),.7/max(.001,np.max(np.abs(x))));x*=gain
    write(OUT/(name+'.wav'),RATE,(x*32767).astype(np.int16));clips[name]=x
    records.append({'file':name+'.wav','seconds':len(x)/RATE,'peak':float(np.max(np.abs(x))),'components':components})
for key,(duration,parts) in profiles.items():
    for variant in range(3):
        x=layer(parts,duration,variant)
        # Tiny time/level variations, not conspicuous octave pitch shifts.
        if variant:x=resample_poly(x,100,100+variant*2)[:round(duration*RATE)]
        output('skill_'+key+'_'+str(variant),x,[p[0] for p in parts])
for variant in range(3):
    parts=[('English Longbow Shoot.wav' if variant!=1 else 'Scythian Recurve Shoot.wav',.8,0,180,6500),('Arrow Fletching.wav',.3,.014,650,8500)]
    output('bow_'+str(variant),layer(parts,.19,variant),[p[0] for p in parts])
    parts=[('Crossbow Shoot.wav',.8,0,200,7000),('Arrow Fletching.wav',.18,.01,650,8500)]
    output('crossbow_'+str(variant),layer(parts,.17,variant),[p[0] for p in parts])
    output('staff_electric_'+str(variant),layer([('spark.wav',.7,0,300,7000)],.14,variant),['spark.wav'])
sources=[s for s in json.loads((CACHE/'sources.json').read_text(encoding='utf-8')) if s['file']!='rpg_sound_pack.zip']
(OUT/'credits.json').write_text(json.dumps({'sources':sources,'edits':'Short isolated takes, band-pass filtering, layered foley and a fast dry release. No material from commercial games.','clips':records},ensure_ascii=False,indent=2),encoding='utf-8')
preview=ROOT/'Art/CombatAudioV3';preview.mkdir(parents=True,exist_ok=True);mix=np.zeros(RATE*18)
for i,key in enumerate(['bow','skill_blade_cast','skill_meteor_cast','skill_mark_hit','skill_arc_cast','skill_ice_ring_cast']):
    for offset,variant in [(0,0),(.6,1)]:
        x=clips[key+'_'+str(variant)];start=round((i*2+offset)*RATE);mix[start:start+len(x)]+=x*.8
for i,key in enumerate(['bow','skill_blade_cast','skill_meteor_hit','skill_arc_hit']*5):
    x=clips[key+'_'+str(i%3)];start=round((12+i*.25)*RATE);mix[start:start+len(x)]+=x*.55
write(preview/'combat-audio-v3-preview.wav',RATE,(np.clip(mix,-.95,.95)*32767).astype(np.int16))
for path,digest in protected.items():assert hashlib.sha256((ROOT/path).read_bytes()).hexdigest()==digest,path
assert len(records)==69
print('Built 69 dry combat cues; original music, sword and prior banks are byte-identical.')
