from pathlib import Path
import shutil
r=Path('D:/NakiLegend/Repository');o=r/'Validation/ForestEndingV7';o.mkdir(exist_ok=True);(o/'runtime').mkdir(exist_ok=True)
p=r/'Assets/NakiPrototype/NakiPrototype.WoodlandFinish.cs';s=p.read_text(encoding='utf-8-sig').replace('ForestQuestProp(1,new Vector2(-3,2.2f),"First shoots beneath the resting guardian");','if(chapterLampGiven)ForestQuestProp(1,new Vector2(-3,2.2f),"First shoots beneath the resting guardian");');p.write_text(s,encoding='utf-8')
f=r/'Validation/forest-ending-files.txt';f.write_text(f.read_text()+'\nAssets/NakiPrototype/NakiPrototype.WoodlandFinish.cs',encoding='utf-8')
s=(r/'Validation/ManorPressureV6/Smoke.cs').read_text(encoding='utf-8').replace('NakiManorPressureV6Smoke','NakiForestEndingV7Smoke').replace('Naki.ManorPressureV6','Naki.ForestEndingV7').replace('Validation/ManorPressureV6/runtime/','Validation/ForestEndingV7/runtime/').replace('CaptainPressureTests();DivineLightTests();MasteryProgressionTests();ArmedWeaponTests();ManorTests();','ForestSeedFlowV7Tests();')
extra=''' static void ForestSeedFlowV7Tests(){
 ForestPose(21);Quiet();Set("chapterStone",false);Set("chapterLampGiven",false);Set("chapterEnding",false);Set(Room(31),"cleared",false);Call("BuildChapterActions");
 Check(!UnityEngine.Object.FindObjectsOfType<SpriteRenderer>().Any(x=>x.sprite!=null&&x.sprite.name=="forest-quest-1"),"Second-floor investigation has no ending seedling art");
 Check(((string[])Get("chapterNames"))[21]=="休眠种根","Second-floor clue clearly names dormant root");
 Set("chapterEnding",true);Check(!((string)Call("WoodObjective")).Contains("尾声完成"),"Stale ending flag cannot announce victory before final boss");
 Set("currentRoom",4);Call("WoodAction",207);Check(!(bool)Get("chapterLampGiven"),"Planting cannot bypass final boss");Quiet();
 Set("chapterCg",true);Call("FinishChapterEnding");Check(!(bool)Get("chapterCg")&&!(bool)Get("woodEndingCgSeen"),"CG cannot complete before planting and final boss");Quiet();
 Set("chapterStone",true);Set(Room(31),"cleared",true);Set("chapterEnding",false);Call("WoodAction",207);Check((bool)Get("chapterLampGiven"),"Defeated final boss permits planting in original seedbed");Quiet();
 Set("currentRoom",0);Call("WoodAction",201);Check((bool)Get("chapterCg")&&!(bool)Get("chapterEnding"),"Returning to A Ling starts ending CG before completion");Call("FinishChapterEnding");Check((bool)Get("chapterEnding")&&(bool)Get("woodEndingCgSeen"),"Only completing the CG finishes the forest storyline");
 Check(((string)Call("WoodObjective")).Contains("青栖林海")&&!((string)Call("WoodObjective")).Contains("木岛"),"Completion objective uses atlas island name");
 Enter(31);Check(!UnityEngine.Object.FindObjectsOfType<Transform>().Any(x=>x.name=="Trail direction sign"),"Final boss return uses hollow arch rather than placeholder board");
 }
'''
s=s.replace(' static void Tick()',extra+' static void Tick()');(o/'Smoke.cs').write_text(s,encoding='utf-8')
qa=r.parent/'Validation/DesertV2/UnitySmokeProject'
old=qa/'Assets/NakiPrototype/Editor/ManorPressureV6Smoke.cs'
if old.exists():old.rename(old.with_suffix('.cs.disabled'))
for src in (r/'Assets/NakiPrototype').rglob('*.cs'):shutil.copy2(src,qa/'Assets/NakiPrototype'/src.relative_to(r/'Assets/NakiPrototype'))
shutil.copy2(o/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/ForestEndingV7Smoke.cs')

