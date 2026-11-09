from pathlib import Path
r=Path(__file__).resolve().parents[1]; a=r/'Assets/NakiPrototype'
changed=[]
def edit(n,fn):
 p=a/('NakiPrototype.'+n+'.cs');s=p.read_text(encoding='utf-8-sig');t=fn(s)
 if t!=s:p.write_text(t,encoding='utf-8');changed.append(p)
edit('ManorCombat',lambda s:s.replace('public float nextDust;', 'public float nextDust,nextBurst,shieldUntil;public int bursts;')
 .replace('Vector2 old=e.position;e.position=', '''// Staggered salvos leave gaps; no trajectory warning lines for normal bullets.
   if(s.bursts>0&&clock>=s.nextBurst){s.bursts--;s.nextBurst=clock+.24f;Vector2 aim=(position-e.position).normalized;PressureFan(e.position+aim*.8f,aim,9+phase*2,12,5.2f+phase*.4f,Element.Dark,1.2f);}
   e.tacticRole=clock<s.shieldUntil?5:0;
   Vector2 old=e.position;e.position=''')
 .replace('EnemyKind.ManorCrossbow,2)', 'EnemyKind.ManorCrossbow,3)')
 .replace('e.attackNumber%4','e.attackNumber%5')
 .replace('phase==2?.85f:1.15f','phase==2?.65f:.95f')
 .replace('Vector2 offset=position-e.position;', 'Vector2 offset=position-e.position;if(clock<s.shieldUntil){MoveIcePursuit(e,e.speed*1.35f*dt);SpellSpark(e.position+e.attackDirection*.7f,C("b6b3d5"),.18f);}')
 .replace('e.chargeUntil=clock+.62f;', 'e.chargeUntil=clock+.72f;s.bursts=e.difficultyPhase>0?2:1;s.nextBurst=clock+.18f;')
 .replace('Sfx("spear",false,.65f);return;\n   }\n   if(move==3)', 'PressureFan(e.position,e.attackDirection,7+e.difficultyPhase*2,17,5.5f,Element.Physical,1.15f);Sfx("spear",false,.65f);return;\n   }\n   if(move==3)')
 .replace('PressureFan(e.position+e.attackDirection*.8f,e.attackDirection,e.difficultyPhase>0?9:7,10,5.5f,Element.Physical,1.2f);', '''if(move==4){s.shieldUntil=clock+1.5f;e.tacticRole=5;e.recoveryUntil=clock+.3f;AddFloatingHit("举盾推进 · 绕侧反击",e.position+Vector2.up,C("ded6ed"));s.bursts=2;s.nextBurst=clock+.35f;return;}
   int count=14+e.difficultyPhase*4;for(int i=0;i<count;i++){float angle=(i+.5f)*Mathf.PI*2/count;Vector2 ray=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));PressureFan(e.position+ray*.8f,ray,1,0,4.5f,Element.Dark,1.2f);}s.bursts=2+e.difficultyPhase;s.nextBurst=clock+.3f;'''))
# Sprite rectangles include transparent margins and tree crowns: only the footprint is solid.
edit('WoodlandV2',lambda s:s.replace('obstacles.Add(body);projectileObstacles.Add(body);woodlandCoverLanes.Add(lane);', '''Rect solid=Rect.MinMaxRect(body.xMin+body.width*.16f,body.yMin+body.height*.08f,body.xMax-body.width*.16f,body.yMin+body.height*(style==1?.67f:.48f));
   obstacles.Add(solid);projectileObstacles.Add(solid);woodlandCoverLanes.Add(lane);'''))
edit('ManorCourtyard',lambda s:s.replace('obstacles.Add(new Rect(at.x-.8f,at.y-.12f,1.6f,.55f));projectileObstacles.Add(new Rect(at.x-.9f,at.y-.1f,1.8f,prop==4?1.5f:.9f));','Rect footprint=new Rect(at.x-.65f,at.y-.08f,1.3f,.48f);obstacles.Add(footprint);projectileObstacles.Add(footprint);'))
edit('WorldAtlas',lambda s:s.replace('"青栖林海","大骑士府邸"','"青栖林海","暮钟岛"'))
edit('LocationNames',lambda s:s.replace('"大骑士府邸 · 第 1 层 · 归功外庭"','"暮钟岛 · 第 1 层 · 归功外庭"'))
edit('WorldAtlasHud',lambda s:s.replace('主线推荐：大骑士府邸','主线推荐：暮钟岛'))
# Replace legacy island nicknames in player-facing runtime strings, not identifiers/save keys.
for p in a.glob('*.cs'):
 s=p.read_text(encoding='utf-8-sig');t=s
 for old,new in [('沙漠岛','鸣沙洲'),('冰霜岛','霜潮群岛'),('冰岛','霜潮群岛'),('火岛','赤炉群岛'),('林岛','青栖林海'),('第二岛独立存档','雷雨高原独立存档'),('尚无第二岛存档','尚无本岛存档')]:t=t.replace(old,new)
 if t!=s:p.write_text(t,encoding='utf-8');changed.append(p)
(r/'Validation/manor-pressure-files.txt').write_text('\n'.join(str(p.relative_to(r)) for p in set(changed)),encoding='utf-8')
print('Changed',len(set(changed)),'runtime scripts')
