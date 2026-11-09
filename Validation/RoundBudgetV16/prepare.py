from pathlib import Path
import shutil
r=Path('D:/NakiLegend/Repository');out=r/'Validation/RoundBudgetV16';qa=r.parent/'Validation/DesertV2/UnitySmokeProject'
out.mkdir(parents=True,exist_ok=True)
s=(r/'Validation/CoverPolishV15/Smoke.cs').read_text(encoding='utf-8').replace('CoverPolishV15','RoundBudgetV16')
start=s.index('   if(stage==0){',s.index(' static void Tick()'));end=s.index('  }catch(Exception e)',start)
s=s[:start]+'''   p.enabled=false;BudgetMeasurements();Check(errors.Count==0,"No runtime errors");File.WriteAllText(Out+"smoke-result.txt","PASS "+checks+" checks: actual full beam cost and unarmoured damage in all six chapters.\\n");End(0);
'''+s[end:]
method=''' static void BudgetMeasurements(){
  var rows=new List<string>{"island,mastery,full_beam_damage,full_beam_cost,seconds,stone_second_cost,stone_third_cost,regen_per_second,stone_volley_at_100,stone_four_volley_window"};
  for(int island=0;island<6;island++){
   MasteryFixture(island);object dummy=Spawn(island==5?"ManorPatrol":"SandScorpion",Vector2.zero);
   Set(dummy,"affinity",EnumVal("Element","Physical"));Set(dummy,"hp",100000f);Set(dummy,"maximum",100000f);Set(dummy,"radius",1.05f);Set(dummy,"boss",true);Set(dummy,"tacticRole",0);Set(dummy,"phaseUntil",0f);Set(dummy,"iceArmor",0);
   float before=(float)Get(dummy,"hp"),power=F("energy");Call("BeginLightChannel");
   for(int i=0;i<25;i++){Set("clock",F("clock")+.1f);Call("TickLightChannel",.1f,true);}
   float damage=before-(float)Get(dummy,"hp"),cost=power-F("energy");
   Check(damage>0,"Full beam actually hits the target in chapter "+(island+1));Check(Mathf.Abs(cost-49)<.02f,"Full beam costs 49 light in chapter "+(island+1));
   Check(!(bool)Get("lightChannel"),"Full beam ends after its 2.5 second cap");
   Call("StopOriginAction",true);ClearShots();Set("origin",EnumVal("Origin","Dark"));Set("darkPower",100f);Set("energy",0f);Set("nextSpell",0f);before=(float)Get(dummy,"hp");Call("CastMeteorVolley");
   Check(List("shots").Count==5,"Stone casts five tracking meteors at chapter "+(island+1));
   for(int i=0;i<80&&List("shots").Count>0;i++){Set("clock",F("clock")+.025f);for(int shot=List("shots").Count-1;shot>=0;shot--)if(shot<List("shots").Count)Call("UpdateShot",shot,.025f);}
   float stone=before-(float)Get(dummy,"hp");Check(stone>0,"Actual grouped meteor volley hits the same dummy");
   rows.Add(string.Join(",",island+1,Get("CombatMasteryTier"),damage,cost,2.5f,Call("OriginSkillCost",1),Call("OriginSkillCost",2),4,stone,stone*4));
  }
  File.WriteAllLines(Out+"beam-measurements.csv",rows);
 }
'''
s=s.replace(' static void StudyTests(){',method+'\n static void StudyTests(){')
(out/'Smoke.cs').write_text(s,encoding='utf-8')
for p in (qa/'Assets/NakiPrototype/Editor').glob('*Smoke.cs'):
 if p.name=='NakiRoundBudgetV16Smoke.cs':continue
 dst=p.with_suffix('.cs.v16-disabled')
 if not dst.exists():p.rename(dst)
shutil.copy2(out/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/NakiRoundBudgetV16Smoke.cs')
print('Prepared isolated measurement; production scripts and player saves unchanged')
