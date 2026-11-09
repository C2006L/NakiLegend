"""Original starfall cues. Does not change existing music or weapon effects."""
from pathlib import Path
import math,random,wave,struct,uuid,json
root=Path(__file__).resolve().parents[1]
out=root/'Assets/NakiPrototype/Resources/NakiCombatAudioV3'
records=[]
for phase in ['cast','hit']:
 for variant in range(3):
  sr=22050;length=.43 if phase=='cast' else .38;rng=random.Random(8571+variant);smooth=0;samples=[]
  for i in range(round(sr*length)):
   t=i/sr;n=rng.uniform(-1,1);smooth=.68*smooth+.32*n
   if phase=='cast':
    envelope=min(1,t/.018)*max(0,1-t/length)**1.8
    cue=.19*(n-smooth)*envelope+.055*math.sin(2*math.pi*(660+variant*12)*t)*math.exp(-t*12)
   else:
    envelope=min(1,t/.003)*math.exp(-t*18)
    cue=(.32*smooth+.15*math.sin(2*math.pi*(92+variant*3)*t))*envelope
    cue+=.07*(n-smooth)*math.exp(-t*37)
   samples.append(max(-.85,min(.85,cue)))
  path=out/f'skill_starfall_{phase}_{variant}.wav'
  with wave.open(str(path),'wb') as w:
   w.setnchannels(1);w.setsampwidth(2);w.setframerate(sr);w.writeframes(b''.join(struct.pack('<h',round(s*32767)) for s in samples))
  template=(out/'skill_soulseal_hit_0.wav.meta').read_text(encoding='utf-8')
  import re
  (Path(str(path)+'.meta')).write_text(re.sub(r'guid: [0-9a-f]+','guid: '+uuid.uuid5(uuid.NAMESPACE_URL,'nakilegend/audio/'+path.name).hex,template),encoding='utf-8')
  records.append({'file':path.name,'duration':length,'peak':max(abs(s) for s in samples)})
dest=root/'Art/StarfallV1';dest.mkdir(exist_ok=True)
(dest/'audio.json').write_text(json.dumps({'author':'Original project-generated audio','assets':records},indent=2),encoding='utf-8')
print('Wrote 6 new starfall cast / impact cues; existing audio untouched')
