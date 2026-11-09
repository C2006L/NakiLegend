"""Original short origin-skill cues and a seamless low-volume light-beam loop.

No music or existing weapon/element samples are changed. 22050 Hz, mono PCM16.
"""
from pathlib import Path
import math,random,wave,struct,json
root=Path(__file__).resolve().parents[1]
out=root/'Assets/NakiPrototype/Resources/NakiCombatAudioV3'
rate=22050
def render(path,seconds,fn):
 samples=[max(-.95,min(.95,fn(i/rate))) for i in range(round(seconds*rate))]
 with wave.open(str(path),'wb') as w:
  w.setnchannels(1);w.setsampwidth(2);w.setframerate(rate);w.writeframes(b''.join(struct.pack('<h',round(x*32767)) for x in samples))
 return {'file':path.name,'seconds':seconds,'peak':round(max(abs(x) for x in samples),4),'rms':round(math.sqrt(sum(x*x for x in samples)/len(samples)),4)}
records=[]
for skill,base,length in [('lightbeam',220,.16),('lightguard',330,.22),('solar',165,.30),('soulseal',196,.18)]:
 for phase in ['cast','hit']:
  for variant in range(3):
   frequency=base*(1+variant*.025);duration=length*(.65 if phase=='hit' else 1)
   def sound(t,f=frequency,d=duration,kind=skill,hit=phase=='hit'):
    attack=min(1,t/.008);tail=max(0,1-t/d)**2
    body=.12*math.sin(2*math.pi*f*t)+.055*math.sin(2*math.pi*f*2*t)+.025*math.sin(2*math.pi*f*3*t)
    if kind=='solar':body+=.045*math.sin(2*math.pi*82.5*t)
    if kind=='lightguard':body+=.045*math.sin(2*math.pi*f*4*t)*math.exp(-t*22)
    if kind=='soulseal':body+=.03*math.sin(2*math.pi*f*1.5*t)*math.exp(-t*18)
    return body*attack*tail*(.65 if hit else 1)
   records.append(render(out/f'skill_{skill}_{phase}_{variant}.wav',duration,sound))
records.append(render(out/'light_channel_loop.wav',1,lambda t:.10*math.sin(2*math.pi*180*t)+.036*math.sin(2*math.pi*360*t)+.018*math.sin(2*math.pi*720*t)))
art=root/'Art/OriginCombatV2';art.mkdir(parents=True,exist_ok=True)
(art/'audio.json').write_text(json.dumps({'author':'Original project-generated tonal effects','license':'Original project assets','sample_rate':rate,'notes':'No pitch sweeps; harmonic soft onset and quiet seamless loop. Existing music and weapon audio unchanged.','assets':records},ensure_ascii=False,indent=2),encoding='utf-8')
print(f'Wrote {len(records)} original origin sound assets')
