from pathlib import Path
import shutil,uuid
r=Path('D:/NakiLegend/Repository');out=r/'Validation/CoverPolishV15';qa=r.parent/'Validation/DesertV2/UnitySmokeProject'
names=['CoverPolish','ManorFinale','ManorCourtyard','BossBreathing','BossDuels','BossSignatures','BossSignatureVisuals','OriginCombat','OriginLegacy','EncounterRefresh','TacticalProps','WoodlandV2','FireCoast','FireCore','FireWorkshop','IceSample','Thunder','Exploration','Journey','CombatMastery','Starfall']
paths=['Assets/NakiPrototype/NakiPrototype.cs']+['Assets/NakiPrototype/NakiPrototype.'+n+'.cs' for n in names]
meta=r/'Assets/NakiPrototype/NakiPrototype.CoverPolish.cs.meta'
if not meta.exists():meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\n',encoding='utf-8')
paths.append(str(meta.relative_to(r)).replace('\\','/'))
(r/'Validation/cover-polish-files.txt').write_text('\n'.join(paths)+'\n',encoding='utf-8')
for rel in paths:
 dst=qa/rel;dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(r/rel,dst)
for p in (qa/'Assets/NakiPrototype/Editor').glob('*Smoke.cs'):
 if p.name=='NakiCoverPolishV15Smoke.cs':continue
 dst=p.with_suffix('.cs.v15-disabled')
 if not dst.exists():p.rename(dst)
s=(r/'Validation/ManorStudyV14/Smoke.cs').read_text(encoding='utf-8-sig').replace('ManorStudyV14','CoverPolishV15')
start=s.index('   if(stage==0){',s.index(' static void Tick()'))
end=s.index('  }catch(Exception e)',start)
s=s[:start]+'''   if(stage==0){p.enabled=false;PolishTests();Time.timeScale=0;Call("ConfigureJourneyView");((Camera)Get("view")).transform.position=(Vector3)Get("JourneyViewCenter");stage=1;ticks=0;return;}
   if(stage==1){p.enabled=false;ScreenCapture.CaptureScreenshot(Out+"knight-poses.png");stage=2;ticks=0;return;}
   Check(errors.Count==0,"No runtime errors");File.WriteAllText(Out+"smoke-result.txt","PASS "+checks+" checks: actor silhouette extraction, exact laser clipping, recovery suppression and rounded cover navigation.\\n");End(0);
'''+s[end:]
s=s.replace(' static void StudyTests(){',(out/'Tests.cs.txt').read_text(encoding='utf-8')+'\n static void StudyTests(){')
(out/'Smoke.cs').write_text(s,encoding='utf-8');shutil.copy2(out/'Smoke.cs',qa/'Assets/NakiPrototype/Editor/NakiCoverPolishV15Smoke.cs')
rsp=(r/'Validation/ManorStudyV14/compile.rsp').read_text(encoding='utf-8-sig').replace('ManorStudyV14','CoverPolishV15')
rsp+='\n"D:\\NakiLegend\\Repository\\Assets\\NakiPrototype\\NakiPrototype.CoverPolish.cs"\n'
(out/'compile.rsp').write_text(rsp,encoding='utf-8')
sync=(r/'Validation/sync_manor_study.py').read_text().replace('ManorStudyV14','CoverPolishV15').replace('manor-study-files','cover-polish-files')
(r/'Validation/sync_cover_polish.py').write_text(sync,encoding='utf-8')
print('Prepared isolated QA and sync manifest',len(paths))
