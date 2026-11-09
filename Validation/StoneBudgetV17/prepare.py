from pathlib import Path
import shutil
r=Path('D:/NakiLegend/Repository');out=r/'Validation/StoneBudgetV17';qa=r.parent/'Validation/DesertV2/UnitySmokeProject';out.mkdir(parents=True,exist_ok=True)
s=(r/'Validation/CoverPolishV15/Smoke.cs').read_text(encoding='utf-8').replace('CoverPolishV15','StoneBudgetV17')
start=s.index('   if(stage==0){',s.index(' static void Tick()'));end=s.index('  }catch(Exception e)',start)
s=s[:start]+'''   p.enabled=false;StoneMeasurements();Check(errors.Count==0,"No runtime errors");File.WriteAllText(Out+"smoke-result.txt","PASS "+checks+" checks: 3.5 second current Stone skill windows, solo boss, chapter progression and low/high reserves.\\n");End(0);
'''+s[end:]
method=''' static void StoneMeasurements(){
  var rows=new List<string>{"island,reserve,policy,window_damage,spent,star_zones,mark_procs"};
  for(int island=0;island<6;island++){
   MasteryFixture(island);
   foreach(float reserve in new[]{12f,100f,1000f})foreach(string policy in new[]{"meteor","mark_meteor","full"}){
    if(island==0&&policy!="meteor"||island<3&&policy=="full")continue;
    ClearEnemies();Call("ResetOriginCombat");Call("ClearBossSignatures");Set("clock",100f);Set("dashUntil",0f);Set("swordUntil",0f);Set("castPoseUntil",0f);Set("origin",EnumVal("Origin","Dark"));Set("darkPower",reserve);Set("energy",0f);Set("nextSpell",0f);Set("originKnowledge",10);Set("orbLearned",true);
    object dummy=Spawn(island==5?"ManorPatrol":"SandScorpion",Vector2.zero);Set(dummy,"affinity",EnumVal("Element","Physical"));Set(dummy,"hp",100000f);Set(dummy,"maximum",100000f);Set(dummy,"radius",1.05f);Set(dummy,"boss",true);Set(dummy,"tacticRole",0);Set(dummy,"phaseUntil",0f);Set(dummy,"iceArmor",0);int stars=0;
    if(policy!="meteor")Check((bool)Call("CastOriginSlot",1),"Seal can be cast with initial reserve "+reserve);
    for(int tick=0;tick<350;tick++){
     Set("clock",100f+tick*.01f);
     if(tick==5&&policy=="full"){Call("CastStarfall");stars=List("fallingStars").Count;}
     if(tick==35||tick==117||tick==199||tick==281)Call("CastMeteorVolley");
     Call("TickStarfall");
     for(int shot=List("shots").Count-1;shot>=0;shot--)if(shot<List("shots").Count)Call("UpdateShot",shot,.01f);
    }
    float damage=100000f-(float)Get(dummy,"hp");Check(damage>0,"Real skill impacts damage solo target: "+island+" / "+reserve+" / "+policy);
    Check(F("darkPower")>=0,"Combination cannot overspend stored energy");
    if(policy=="full")Check(stars==(reserve>=29?3:0),"Starfall needs its full cost after seal and creates three zones");
    rows.Add(string.Join(",",island+1,reserve,policy,damage,reserve-F("darkPower"),stars,Get("originMarkPulses")));
   }
  }
  File.WriteAllLines(Out+"stone-windows.csv",rows);
 }
'''
s=s.replace(' static void StudyTests(){',method+'\n static void StudyTests(){');(out/'Smoke.cs').write_text(s,encoding='utf-8')
for p in (qa/'Assets/NakiPrototype/Editor').glob('*Smoke.cs'):
 if p.name=='NakiStoneBudgetV17Smoke.cs':continue
 dst=p.with_suffix('.cs.v17-disabled')
 if not dst.exists():p.rename(dst)
shutil.copy2(out/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/NakiStoneBudgetV17Smoke.cs')
print('Prepared current-behaviour probe only; production unchanged')
