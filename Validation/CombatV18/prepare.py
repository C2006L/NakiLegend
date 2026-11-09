from pathlib import Path
import shutil,json
r=Path('D:/NakiLegend/Repository');out=r/'Validation/CombatV18';qa=r.parent/'Validation/DesertV2/UnitySmokeProject';paths=json.loads((out/'files.json').read_text())
for rel in paths:
 dst=qa/rel;dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(r/rel,dst)
for p in (qa/'Assets/NakiPrototype/Editor').glob('*Smoke.cs'):
 if p.name=='NakiCombatV18Smoke.cs':continue
 dst=p.with_suffix('.cs.v18-disabled')
 if not dst.exists():p.rename(dst)
s=(r/'Validation/CoverPolishV15/Smoke.cs').read_text(encoding='utf-8').replace('CoverPolishV15','CombatV18')
start=s.index('   if(stage==0){',s.index(' static void Tick()'));end=s.index('  }catch(Exception e)',start)
s=s[:start]+'''   if(stage==0){p.enabled=false;CombatV18Tests();LightBurstMeasurements();StoneMeasurements();Time.timeScale=0;CombatV18Preview();stage=1;ticks=0;return;}
   if(stage==1){ScreenCapture.CaptureScreenshot(Out+"combat-preview.png");stage=2;ticks=0;return;}
   Check(errors.Count==0,"No runtime errors");File.WriteAllText(Out+"smoke-result.txt","PASS "+checks+" checks: actual boss health, light capacity/save, skill cost, repeated impacts, slow/fast bullets, moving recovery and exact laser cover.\\n");End(0);
'''+s[end:]
combo=qa/'Assets/NakiPrototype/Editor/NakiStoneBudgetV17Smoke.cs.v18-disabled'
comboSource=combo.read_text(encoding='utf-8')
methods=[]
for name in ['StoneMeasurements','LightBurstMeasurements']:
 begin=comboSource.index(' static void '+name+'(');end=comboSource.index('\n static void ',begin+10);methods.append(comboSource[begin:end])
s=s.replace(' static void StudyTests(){',(out/'Tests.cs.txt').read_text(encoding='utf-8')+'\n'+'\n'.join(methods)+'\n static void StudyTests(){');(out/'Smoke.cs').write_text(s,encoding='utf-8');shutil.copy2(out/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/NakiCombatV18Smoke.cs')
rsp=(r/'Validation/CoverPolishV15/compile.rsp').read_text(encoding='utf-8-sig').replace('CoverPolishV15','CombatV18')+'\n"D:\\NakiLegend\\Repository\\Assets\\NakiPrototype\\NakiPrototype.EncounterBalanceV18.cs"\n'
(out/'compile.rsp').write_text(rsp,encoding='utf-8')
(r/'Validation/combat-v18-files.txt').write_text('\n'.join(paths)+'\n',encoding='utf-8')
sync=(r/'Validation/sync_manor_study.py').read_text().replace('ManorStudyV14','CombatV18').replace('manor-study-files','combat-v18-files');(r/'Validation/sync_combat_v18.py').write_text(sync,encoding='utf-8')
print('Staged',len(paths),'files in isolated QA only')
