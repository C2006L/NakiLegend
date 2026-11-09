"""Deterministic planning model. Not a dodge bot or a claim about human difficulty."""
from pathlib import Path
import csv,json,math
out=Path(__file__).parent
def energy_run(cap,regen,drain,attack,policy,third=True,guard=True):
 dt=.01;power=float(cap);ready=0.;channel=False;charging_until=-1.;beam_end=-1.;full=0;short=0;skipped=0;beams=[];casts=0;spent=0;minimum=power
 for rnd in range(1,11):
  start=(rnd-1)*(attack+3.5)
  use_o=guard and policy!='beam' and (policy=='all' or rnd%2==0)
  use_p=third and policy!='beam' and (policy=='all' or rnd%3==0)
  events={round((attack-1)*100):'guard'} if use_o else {}
  if use_p:events[round(attack*100)]='third'
  beam_at=attack+(.7 if use_p else .1);events[round(beam_at*100)]='beam'
  duration=0.;began=False
  for tick in range(round((attack+3.5)*100)):
   t=start+tick*dt;event=events.get(tick)
   if event in ('guard','third'):
    cost=20 if event=='guard' else 35
    if power+1e-6>=cost:
     power-=cost;spent+=cost;ready=t+1.5;casts+=1
     if event=='third':charging_until=t+.61;ready=charging_until+1.5
    else:skipped+=1
   if event=='beam':
    if power>=4:
     power-=4;spent+=4;ready=t+1.5;channel=True;beam_end=t+2.5;began=True
    else:skipped+=1
   if channel:
    if t>=beam_end-1e-6 or power<=1e-6:channel=False;ready=t+1.5
    else:
     paid=min(dt,power/drain);power-=paid*drain;spent+=paid*drain;duration+=paid;ready=t+1.5
   if not channel and t>=charging_until and t>=ready:power=min(cap,power+regen*dt)
   minimum=min(minimum,power)
  if duration>=2.49:full+=1
  elif began:short+=1
  beams.append(round(duration,3))
 return dict(full_beams=full,short_beams=short,skipped_skills=skipped,end_power=round(power,2),minimum=round(minimum,2),auxiliary_casts=casts,beam_seconds=beams)
caps=[100,120,140,160,180,200]
configs={'current':([100]*6,[4]*6,18),'cap_only':(caps,[4]*6,18),'cap_regen':(caps,[4,4.2,4.4,4.6,4.8,5],18),'cap_regen_efficient_beam':(caps,[4,4.2,4.4,4.6,4.8,5],16),'cap_5regen':(caps,[5]*6,18),'cap_5regen_efficient_beam':(caps,[5]*6,16)}
rows=[]
for name,(limits,rates,drain) in configs.items():
 for island in range(6):
  for attack in [10,12,14]:
   for policy in ['beam','mixed','all']:
    result=energy_run(limits[island],rates[island],drain,attack,policy,third=island>=3,guard=island>=1)
    rows.append(dict(config=name,island=island+1,cap=limits[island],regen=rates[island],drain=drain,attack_seconds=attack,policy=policy,**result))
(out/'energy-scenarios.json').write_text(json.dumps(rows,indent=2),encoding='utf-8')
for name in configs:
 selected=[r for r in rows if r['config']==name and r['attack_seconds']==12 and r['policy']=='mixed']
 print(name,[(r['island'],r['full_beams'],r['skipped_skills'],r['end_power']) for r in selected])
measure=out/'runtime/beam-measurements.csv'
if measure.exists():
 data=list(csv.DictReader(measure.open(encoding='utf-8-sig')));beams=[float(r['full_beam_damage']) for r in data]
 # Lower/typical/upper: beam uptime 70/80/90%; weapon contribution 15/20/30%
 # of an unobstructed full beam per round. P adds 1/2/3 primary orb hits every third round.
 hp=[[350,800,950],[1150,1250,1400],[1450,1600,1800],[2000,2150,2400],[2500,2650,2950]]
 forecasts=[]
 for island in range(5):
  for floor,health in enumerate(hp[island]):
   cases={}
   for label,hit,weapon,orb_hits in [('low',.70,.15,1),('typical',.80,.20,2),('high',.90,.30,3)]:
    accumulated=0;rounds=0
    while accumulated<health and rounds<30:
     rounds+=1;accumulated+=beams[island]*(hit+weapon)
     if island>=3 and rounds%3==0:accumulated+=beams[island]/125*22*orb_hits
    cases[label]=rounds
   stone=float(data[island]['stone_volley_at_100'])*4*.8+beams[island]*.2
   stone_planned=float(data[island]['stone_volley_at_100'])*4*.8*.75+beams[island]*.2
   forecasts.append(dict(island=island+1,floor=floor+1,hp=health,full_beam=beams[island],stone_current_primary_rounds=math.ceil(health/stone),stone_planned_primary_rounds=math.ceil(health/stone_planned),**cases))
 (out/'health-forecast.json').write_text(json.dumps(forecasts,indent=2),encoding='utf-8')
 print('Health forecast',json.dumps(forecasts))
assert energy_run(200,5,16,12,'beam')['full_beams']==10
assert energy_run(100,4,18,12,'beam',False,False)['full_beams']==10
print('Energy model invariant checks passed; scenarios:',len(rows))
