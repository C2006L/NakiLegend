"""Create a reviewable, offline manor layout. Design artifacts only; no Unity/save changes."""
from pathlib import Path
import json, html

root=Path(__file__).resolve().parents[1]
out=root/'Art/ManorDesignV1';out.mkdir(parents=True,exist_ok=True)
# Coordinates are plan coordinates: positive y is south. Every passage is reciprocal and orthogonal.
floors=[
 {'name':'归功外庭','tone':'灰白砂岩、褪蓝军旗、铜色暮钟','main':[0,1,2,7,8,9,14],
  'edges':[(0,1),(1,2),(2,7),(7,8),(8,9),(9,14),(2,3),(3,4),(4,9),(1,6),(5,6),(6,7),(5,10),(10,11),(11,12),(12,13),(13,14)],
  'rooms':[
   ('暮钟渡口','安全','登岛、记下返航位置；仅东侧岸路接外庭，南侧栈桥只用于乘船。','船匠与关港公告'),
   ('石狮夹道','战斗','在低墙与马车之间突破巡卫；先展示单个盾卫的转身窗口。','巡卫、盾卫'),
   ('勇者纪念庭','事件','查看碑文与旧佩剑式样；取得委任书年代线索。','固定物证，不随机掉落'),
   ('钟廊巡路','战斗','借断柱避开弩手，观察瞄准线结束后的换位。','弩手、巡卫'),
   ('废弩塔','精英','选择挑战弩手队长，换取一箱可选装备。','精英弩手、盾卫'),
   ('旧马厩','战斗','识别猎犬压低身体后的直线扑咬。','猎犬、巡卫'),
   ('退役营房','安全','短暂恢复、查看庄园手绘图，得知居民被困旧村。','守粮人留下的通行记录'),
   ('受封登记处','事件','读到同样的王命格式；旧航线与前五岛逐步对应。','委任书副本；漏读可在卫长战后补看'),
   ('封粮门道','战斗','清出运输道路，看到满粮仓与空粥桶的对照。','盾卫、弩手、巡卫'),
   ('誓庭前室','安全','整理装备、保存卫长战前恢复点；提示身后其他路线可回访。','可补读委任书副本'),
   ('翻车石径','战斗','绕行倒塌运粮车，处理两侧错时进攻。','猎犬、弩手'),
   ('巡卫补给间','奖励','近战、法杖、弓弩、恢复四选一。','职业方向优先，但保留全部方向'),
   ('荒芜花庭','战斗','低花坛形成绕侧路线，避免贴着整排盾卫正面输出。','盾卫、猎犬'),
   ('断旗台','精英','打断执役的护印，获得额外补给或历史记录。','护印执役、精英巡卫'),
   ('誓庭卫长','首领','击败卫长、取得旧村通行权；他缴械跪地，指认命令来自府内。','卫长；一次专武奖励')
  ]},
 {'name':'断粮旧村与淬炼工坊','tone':'潮灰砖墙、麦秆黄、药房暖灯、少量紫色刻印','main':[0,5,6,1,2,7,8,9,14],
  'edges':[(0,5),(5,6),(6,1),(1,2),(2,7),(7,8),(8,9),(9,14),(0,1),(2,3),(3,4),(4,9),(5,10),(10,11),(11,6),(11,12),(12,7),(12,13),(13,8)],
  'rooms':[
   ('旧村避难院','安全','与守粮人见面；得知城内有存粮，却禁止运输。','居民位于有门墙保护的区域'),
   ('扣粮岗','战斗','清除堵住粮道的守军；放行后安全侧道承担运粮。','巡卫、锁链狱卒'),
   ('封存粮仓','战斗','清场后亲手打开仓闸，居民开始接运物资。','盾卫、弩手；仓闸只在安全时交互'),
   ('守粮账房','事件','核对赤炉货单与冰岛药单，发现补给一直被扣留。','洛恩；对话随放粮状态变化'),
   ('药材储间','奖励','选择装备或药箱补给；额外故事不强制索取资源。','四选一补给'),
   ('诊疗屋','安全','医师缇娅说明完整意志与战斗残响的差别。','医师与居民；战斗时院门关闭'),
   ('锁链广场','精英','切断钩链造成的封路，避免站在收钩路径上。','精英狱卒、巡卫'),
   ('药运走廊','战斗','打通诊疗屋的药物转运道路，安全后居民从侧门接走药箱。','弩手、狱卒'),
   ('淬炼车间','战斗','击破运行中的人工容器；战后查看容器与源石的区别。','空甲傀儡、掌印执役'),
   ('封印档案室','事件','用画像、佩剑编号和受封档案确认前代身份，播放画像 CG。','固定关键线索；前往机关的必经房'),
   ('居民后巷','战斗','从高低断墙之间绕行，救出尚未进入避难院的工匠。','猎犬、巡卫'),
   ('钟匠工棚','安全','伊芙解释远处钟声在同步控制甲胄；提供一次整备。','可重读关键规则，不能无限领取补给'),
   ('账房暗格','奖励','探索支路获得装备，发现前代未寄出的家书。','普通补给与非必需往事'),
   ('印痕回廊','精英','优先击倒执役，拆散护印与盾卫的组合。','精英执役、盾卫'),
   ('掌印机关缄令者','首领','逐个破坏刻印臂，停止封锁命令；带走带封蜡的契约原令。','机关首领；一次专武奖励')
  ]},
 {'name':'无昼内庭','tone':'青黑石地、剥落金边、冷白月光、受控紫光','main':[0,1,6,7,8,9,14],
  'edges':[(0,1),(1,6),(6,7),(7,8),(8,9),(9,14),(1,2),(2,3),(3,4),(4,9),(0,5),(5,10),(10,11),(11,6),(11,12),(12,7),(12,13),(13,8)],
  'rooms':[
   ('受封回廊','安全','看见前代的爵位与没有启程日期的返乡船票。','通往旧村的楼梯可以往返'),
   ('武备庭','战斗','处理盾卫与狙击者配合，利用双侧廊绕后。','盾卫、弩手、巡卫'),
   ('无人宴会厅','战斗','长桌提供掩体，同时限制过度直线追击。','傀儡、巡卫'),
   ('旧航海室','事件','看到修渠拓印、护送灯、药单、炉印、空种盒放在同一张桌上。','前五岛物证的对应副本'),
   ('值守塔','精英','面对能重新选位的弩手精英，预演大骑士远近切换。','精英弩手、猎犬'),
   ('断柱侧廊','战斗','借断柱分开近战追兵与执役视线。','执役、巡卫'),
   ('封存军械库','奖励','在最后数场战斗前选择近战、法杖、弓弩或恢复。','保底三方向，不要求固定装备'),
   ('契约柱廊','战斗','拆掉两根命令柱，短暂关闭封路与护印；任意武器都能破坏。','傀儡、残誓回声'),
   ('暮钟前庭','精英','处理两类敌人的错时攻击，抵达最终准备室。','精英巡卫、执役、回声'),
   ('最后灯室','安全','独立战前存档、装备技能整备、规则回顾；可返回旧村或航图。','最终仪式前始终允许离开'),
   ('停钟地窖','事件','选择关闭一处钟声共鸣，减少一场支路战的护印。','可选操作，不取消最终 Boss 阶段'),
   ('退役兵器间','奖励','偏门补给；每房只结算一次。','四选一补给'),
   ('无昼长廊','战斗','在短时显形的回声与实体巡卫之间切换目标。','残誓回声、巡卫'),
   ('回声长厅','战斗','观察回声显形与攻击的节奏，练习在窗口内集中输出。','回声、傀儡'),
   ('暮钟大厅','首领','三阶段对战暗黑大骑士；战斗结束后进入明确的光暗收尾。','大骑士；战斗与结局奖励分开结算')
  ]}
]

total=0
for layer,f in enumerate(floors):
 assert len(f['rooms'])==15
 links={i:[] for i in range(15)}
 for a,b in f['edges']:
  assert abs(a%5-b%5)+abs(a//5-b//5)==1,(layer,a,b)
  assert b not in links[a]
  links[a].append(b);links[b].append(a)
 seen={0};todo=[0]
 while todo:
  for n in links[todo.pop()]:
   if n not in seen:seen.add(n);todo.append(n)
 assert len(seen)==15
 assert all(b in links[a] for a,b in zip(f['main'],f['main'][1:]))
 assert f['rooms'][14][1]=='首领' and sum(r[1]=='首领' for r in f['rooms'])==1
 if layer==0:assert links[0]==[1], 'Dock has one dry east exit, no sea-pier doorway'
 if layer==1:assert links[14]==[9], 'Identity archive cannot be bypassed on the way to the seal boss'
 if layer==2:assert links[14]==[9], 'Final preparation precedes the final boss'
 f['rooms']=[dict(id=layer*15+i,local=i,name=r[0],kind=r[1],objective=r[2],content=r[3],x=i%5,y=i//5,links=[layer*15+j for j in links[i]],stairsTo=(layer+1)*15 if i==14 and layer<2 else layer*15-1 if i==0 and layer>0 else None) for i,r in enumerate(f['rooms'])]
 total+=len(f['rooms'])
assert total==45
data={'status':'design-only','chapter':'暮钟岛 大骑士府邸','note':'房间拓扑与制作规格，不是 Unity 可玩场景。实装前仍保持航图未开放。','floors':floors}
(out/'room-layout.json').write_text(json.dumps(data,ensure_ascii=False,indent=2),encoding='utf-8')
colors={'安全':'#69bdad','战斗':'#d5a56e','精英':'#cf796d','事件':'#a6a9e0','奖励':'#d3c96b','首领':'#be7fbb'}
def svg(f):
 lines=['<svg viewBox="0 0 1100 560" xmlns="http://www.w3.org/2000/svg" role="img" aria-label="'+f['name']+'房间路线">']
 main={tuple(sorted((a,b))) for a,b in zip(f['main'],f['main'][1:])}
 for a,b in f['edges']:
  x1,y1=110+a%5*220,85+a//5*185;x2,y2=110+b%5*220,85+b//5*185
  active=tuple(sorted((a,b))) in main
  lines.append(f'<line x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}" stroke="{ "#e7c788" if active else "#4b6977"}" stroke-width="{5 if active else 3}"/>')
 for r in f['rooms']:
  x,y=20+r['x']*220,38+r['y']*185;c=colors[r['kind']]
  lines.append(f'<g class="room" data-id="{r["id"]}" role="button" tabindex="0" aria-label="{r["name"]}"><rect x="{x}" y="{y}" width="180" height="94" rx="10" fill="#152d37" stroke="{c}" stroke-width="2"/><text x="{x+13}" y="{y+25}" fill="{c}" font-size="15">{r["local"]+1:02d} · {r["kind"]}</text><text x="{x+90}" y="{y+61}" fill="#f1e8d5" text-anchor="middle" font-size="19">{r["name"]}</text></g>')
 lines.append('</svg>');return ''.join(lines)
svgs=[svg(f) for f in floors]
for i,s in enumerate(svgs):
 (out/f'floor-{i+1}-layout.svg').write_text(s.replace('<svg ', '<svg style="background:#0b1b24;font-family:Microsoft YaHei,sans-serif" ',1),encoding='utf-8')
template='''<!doctype html><html lang="zh-CN"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>暮钟岛府邸 房间设计</title><style>
*{box-sizing:border-box}body{margin:0;background:#0b1b24;color:#e8e6dc;font:16px/1.65 "Microsoft YaHei",sans-serif}main{max-width:1450px;margin:auto;padding:35px}h1{font-size:29px;color:#e7c788;margin:0 0 8px}p{margin:6px 0 18px;color:#afc1c8}nav{display:flex;gap:12px;flex-wrap:wrap;margin:24px 0}button{background:#172e39;color:#e8e6dc;border:1px solid #587382;padding:12px 20px;cursor:pointer;border-radius:5px;font:inherit}button[aria-selected=true]{border-color:#e7c788;color:#e7c788}.layout{display:grid;grid-template-columns:minmax(0,1fr) 300px;gap:25px}.map{background:#0f242e;border:1px solid #2b4754;padding:12px;overflow:auto}svg{width:100%;min-width:650px}aside{border:1px solid #2b4754;background:#102630;padding:22px}h2{font-size:21px;color:#e7c788;margin-top:0}.room{cursor:pointer}.room:hover rect,.room:focus rect{fill:#2b4650;stroke-width:4}.room.selected rect{fill:#304a50;stroke:#fff0ba;stroke-width:4}.legend{display:flex;flex-wrap:wrap;gap:18px;margin-top:18px}.dot{width:9px;height:9px;display:inline-block;border-radius:50%;margin-right:7px}small{color:#9db4bd}.badge{padding:3px 10px;border:1px solid #5d7985;display:inline-block;margin:12px 0;font-size:13px}@media(max-width:900px){.layout{grid-template-columns:1fr}main{padding:18px}}
</style><main><h1>暮钟岛 · 大骑士府邸</h1><p>三层 45 房设计。点击房间查看任务与敌人；金色连线表示推荐路线，蓝灰连线表示可探索支路。</p><span class="badge">设计稿 · 尚未接入 Unity</span><nav id="tabs"></nav><div class="layout"><div><div class="map" id="map"></div><div class="legend">LEGEND</div><p id="tone"></p></div><aside id="detail" aria-live="polite"></aside></div><small>房间间的普通门可以双向通行；跨层楼梯使用 F 明确交互。海面不承担通向室内的普通房间出口。最终灯室保存独立恢复点，结局不覆盖它。</small></main><script>
const data=DATA, maps=MAPS;let floor=0;const esc=s=>String(s).replaceAll('&','&amp;').replaceAll('<','&lt;').replaceAll('>','&gt;');function select(id){let r=data.floors.flatMap(f=>f.rooms).find(r=>r.id===id);document.querySelectorAll('.room').forEach(e=>e.classList.toggle('selected',+e.dataset.id===id));document.getElementById('detail').innerHTML='<h2>'+esc(r.name)+'</h2><p>第 '+(floor+1)+' 层 · '+esc(r.kind)+'</p><p>'+esc(r.objective)+'</p><p>'+esc(r.content)+'</p><small>相邻：'+esc(r.links.map(id=>data.floors[floor].rooms.find(n=>n.id===id).name).join('、'))+(r.stairsTo!==null?'<br>跨层楼梯：'+(r.stairsTo<r.id?'返回上一层':'前往下一层'):'')+'</small>';}function draw(i){floor=i;document.getElementById('map').innerHTML=maps[i];document.getElementById('tone').textContent='美术方向：'+data.floors[i].tone;document.querySelectorAll('nav button').forEach((b,k)=>b.setAttribute('aria-selected',k===i));document.querySelectorAll('.room').forEach(g=>{g.onclick=()=>select(+g.dataset.id);g.onkeydown=e=>{if(e.key==='Enter'||e.key===' '){e.preventDefault();select(+g.dataset.id)}}});select(i*15);}data.floors.forEach((f,i)=>{let b=document.createElement('button');b.textContent=(i+1)+' · '+f.name;b.onclick=()=>draw(i);document.getElementById('tabs').append(b)});draw(0);
</script></html>'''
legend=''.join(f'<span><i class="dot" style="background:{c}"></i>{k}</span>' for k,c in colors.items())
template=template.replace('LEGEND',legend).replace('DATA',json.dumps(data,ensure_ascii=False)).replace('MAPS',json.dumps(svgs,ensure_ascii=False))
(out/'manor-layout.html').write_text(template,encoding='utf-8')
rows=['# 暮钟岛府邸逐房设计','', '此表由 `Tools/build_manor_design.py` 生成，房间坐标与连线见交互规划图。均为设计稿，尚未实现。','']
for floor,f in enumerate(floors):
 rows+=['## '+f['name'],'','| 房间 | 类型 | 目标与交互 | 敌人与内容 |','| --- | --- | --- | --- |']
 for r in f['rooms']:rows.append(f'| {floor+1}-{r["local"]+1:02d} {r["name"]} | {r["kind"]} | {r["objective"]} | {r["content"]} |')
 rows+=['']
(root/'Docs/MANOR_ROOM_CATALOG.md').write_text('\n'.join(rows),encoding='utf-8')
print('Validated 45 unique rooms, 3 connected orthogonal graphs, reciprocal passages, 3 main routes, safe dock exit and required archive/preparation approaches. Wrote offline interactive map, JSON, three SVG maps and room catalog.')
