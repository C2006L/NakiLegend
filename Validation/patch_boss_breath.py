from pathlib import Path
import uuid
r=Path('D:/NakiLegend/Repository');a=r/'Assets/NakiPrototype';files=['BossBreathing']
def edit(n,fn):
 p=a/('NakiPrototype'+('.'+n if n else '')+'.cs');s=p.read_text(encoding='utf-8-sig');t=fn(s);assert t!=s,n;p.write_text(t,encoding='utf-8');files.append(n)
edit('EncounterDirector',lambda s:s.replace('if(e.kind==EnemyKind.ManorCaptain){TickManorCaptain(e,dt);return;}','if(TickBossBreathing(e))return;\n   if(e.kind==EnemyKind.ManorCaptain){TickManorCaptain(e,dt);return;}').replace('ClearBossSignatures();ResetEnemyIntents();','bossBreaths.Clear();ClearBossSignatures();ResetEnemyIntents();'))
edit('CombatV3',lambda s:s.replace('if(manorCourtyardMode)return 1;','if(manorCourtyardMode)return boss?.82f:.85f;').replace('return scale;}','return scale*(boss?.82f:.85f);}').replace('LearningEncounter?1.05f:1.12f+.0325f*Mathf.Clamp(CurrentIsland.ordinal-1,0,4)','(LearningEncounter?1.05f:1.12f+.0325f*Mathf.Clamp(CurrentIsland.ordinal-1,0,4))*(enemies.Exists(e=>e.boss&&e.hp>0)?.75f:1)'))
edit('',lambda s:s.replace('if(hostile&&!EnemyOnScreen','if(hostile&&BossResting)return;\n            if(hostile&&!EnemyOnScreen'))
edit('Elements',lambda s:s.replace('if (clock < invulnerableUntil || health <= 0) return;','if (BossResting || clock < invulnerableUntil || health <= 0) return;'))
edit('Journey',lambda s:s.replace('guardian.hp=guardian.maximum=node.stage==0?650:1050;','guardian.hp=guardian.maximum=(node.stage==0?650:1050)*.82f;'))
meta=a/'NakiPrototype.BossBreathing.cs.meta';meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\n')
(r/'Validation/boss-breath-files.txt').write_text('\n'.join('Assets/NakiPrototype/NakiPrototype'+('.'+n if n else '')+'.cs' for n in files)+'\nAssets/NakiPrototype/NakiPrototype.BossBreathing.cs.meta',encoding='utf-8')
