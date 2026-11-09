from pathlib import Path
import random,math,wave,struct
root=Path(__file__).resolve().parents[2]
p=root/'Assets/NakiPrototype/NakiPrototype.Gatehouse.cs'
s=p.read_text(encoding='utf-8').replace('先沿北路','先沿南路').replace('向北进入旧渠闸室','向南进入旧渠闸室').replace('再向南清理庭院','再向北沿岸路清理庭院').replace('向南清理检修庭院','向北沿岸路清理检修庭院').replace('可以沿北门前进','可以沿南门前进').replace('沿北路清理检修庭院','沿南路清理检修庭院')
p.write_text(s,encoding='utf-8')
p=root/'Assets/NakiPrototype/NakiPrototype.EncounterDirector.cs';s=p.read_text(encoding='utf-8')
s=s.replace('e.nextAttack=clock+tell+Mathf.Max(.35f,(e.nextAttack-clock-tell)*pace);','if(e.boss&&CurrentIsland.ordinal==1)pace*=.9f;e.nextAttack=clock+tell+Mathf.Max(.35f,(e.nextAttack-clock-tell)*pace);')
s=s.replace('int count=e.difficultyPhase>=2?5:3;','int count=(e.difficultyPhase>=2?5:3)+(CurrentIsland.ordinal==1?2:0);')
p.write_text(s,encoding='utf-8')
out=root/'Assets/NakiPrototype/Resources/NakiCombatAudioV3'
for variant in range(3):
 rng=random.Random(714+variant);low=0;rate=22050;duration=.24;data=[]
 for i in range(round(rate*duration)):
  t=i/rate;phase=t/duration;noise=rng.uniform(-1,1);low=.78*low+.22*noise
  # Fast body-weight launch, short leather/armor rattle; no sustained whistle or musical sweep.
  attack=min(1,t/.006);tail=(1-phase)**2
  thump=.36*math.sin(2*math.pi*(95+variant*4)*t)*math.exp(-t*30)
  rush=(noise-low)*.20*math.sin(math.pi*phase)**1.3
  armor=.08*math.sin(2*math.pi*1350*t)*math.exp(-t*48)
  x=(thump+rush+armor)*attack*tail
  data.append(struct.pack('<h',round(max(-.95,min(.95,x))*32767)))
 with wave.open(str(out/f'warrior_dash_{variant}.wav'),'wb') as w:
  w.setnchannels(1);w.setsampwidth(2);w.setframerate(rate);w.writeframes(b''.join(data))
print('Updated directional hints, first-island boss cadence and three original warrior dash cues')
