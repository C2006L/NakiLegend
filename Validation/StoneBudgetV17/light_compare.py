from pathlib import Path
r=Path('D:/NakiLegend/Repository');qa=r.parent/'Validation/DesertV2/UnitySmokeProject/Assets/NakiPrototype';p=qa/'Editor/NakiStoneBudgetV17Smoke.cs';s=p.read_text(encoding='utf-8')
method=''' static void LightBurstMeasurements(){
  var rows=new List<string>{"island,burst_damage,spent"};
  for(int island=0;island<6;island++){
   MasteryFixture(island);Set("energy",100f+island*20);Set("swordUntil",0f);Set("castPoseUntil",0f);object dummy=Spawn(island==5?"ManorPatrol":"SandScorpion",Vector2.zero);
   Set(dummy,"affinity",EnumVal("Element","Physical"));Set(dummy,"hp",100000f);Set(dummy,"maximum",100000f);Set(dummy,"radius",1.05f);Set(dummy,"boss",true);Set(dummy,"tacticRole",0);Set(dummy,"phaseUntil",0f);Set(dummy,"iceArmor",0);
   float before=F("energy");if(island>0)Check((bool)Call("CastOriginSlot",1),"Light can activate defensive O before its burst");
   for(int tick=0;tick<350;tick++){
    Set("clock",100f+tick*.01f);if(tick==5&&island>=3)Check((bool)Call("CastOriginSlot",2),"Light can activate P before its beam");
    Call("TickOriginInput",.01f,tick>=90,false,false);
    for(int shot=List("shots").Count-1;shot>=0;shot--)if(shot<List("shots").Count)Call("UpdateShot",shot,.01f);
   }
   float damage=100000f-(float)Get(dummy,"hp");Check(damage>0,"Light complete window deals damage");rows.Add(string.Join(",",island+1,damage,before-F("energy")));
  }
  File.WriteAllLines(Out+"light-bursts.csv",rows);
 }
'''
s=s.replace('p.enabled=false;StoneMeasurements();','p.enabled=false;LightBurstMeasurements();StoneMeasurements();')
s=s.replace(' static void StudyTests(){',method+'\n static void StudyTests(){');p.write_text(s,encoding='utf-8')
print('Added matched Light burst benchmark in isolated QA')
