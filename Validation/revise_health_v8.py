from pathlib import Path
r=Path('D:/NakiLegend/Repository');a=r/'Assets/NakiPrototype'
p=a/'NakiPrototype.CombatV3.cs';s=p.read_text(encoding='utf-8-sig').replace('boss?.82f:.85f','boss?.65f:.75f');p.write_text(s,encoding='utf-8')
p=a/'NakiPrototype.Journey.cs';s=p.read_text(encoding='utf-8-sig').replace('(node.stage==0?650:1050)*.82f','(node.stage==0?650:1050)*.65f');p.write_text(s,encoding='utf-8')

