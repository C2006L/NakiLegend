from pathlib import Path
import re,json
root=Path(__file__).resolve().parents[2];base=root/'Assets/NakiPrototype'
rows=[]
for name in ['Equipment','ThunderSkills','Arsenal','WoodlandStoryV2']:
 s=(base/f'NakiPrototype.{name}.cs').read_text(encoding='utf-8')
 rows += [dict(name=n,type=t,element=e) for n,t,e in re.findall(r'Weapon\("([^"]+)",\s*WeaponType\.(\w+),\s*Element\.(\w+)',s)]
assert len(rows)==33,len(rows)
s=(base/'NakiPrototype.IslandSupplies.cs').read_text(encoding='utf-8');names=re.search(r'string\[\] names=\{([^}]+)',s)[1];names=re.findall(r'"([^"]+)"',names)
for i,n in enumerate(names):rows.append(dict(name=n,type='Staff' if i%3==0 else 'Bow',element=['Fire','Ice','Wood'][i//3]))
assert len(rows)==42
s=(base/'NakiPrototype.Relics.cs').read_text(encoding='utf-8')
normal=re.findall(r'"([^"]+)"',re.search(r'string\[\] normal=\{([^}]+)',s)[1]);bosses=re.findall(r'"([^"]+)"',re.search(r'string\[\] bosses=\{([^}]+)',s)[1]);types=re.findall(r'WeaponType\.(\w+)',re.search(r'WeaponType\[\] types=\{([^}]+)',s)[1])
for i,n in enumerate(normal):
 t='Bow' if i in [1,3,4,7,9] else 'Staff';e=['Wind','Lightning','Ice','Fire','Wood'][i//2]
 for v in range(3):rows.append(dict(name=n+['','·散华','·凝聚'][v],type=t,element=e))
for i,n in enumerate(bosses):
 island=0 if i<5 else 1 if i<8 else 2 if i<11 else 3 if i<14 else 4;e='Wood' if i==2 else ['Wind','Lightning','Ice','Fire','Wood'][island]
 for v in range(3):rows.append(dict(name=n+['','·散华','·凝聚'][v],type=types[i],element=e))
assert len(rows)==123
for i,r in enumerate(rows):
 r['id']=i;r['sheet']=i//18;r['cell']=i%18
 if i>=42:
  v=(i-42)%3
  if v:r['name']=r['name'].split('·')[0]+{'Staff':['','·簇射','·贯流'],'Bow':['','·翼矢','·重弦'],'Sword':['','·回锋','·裂刃'],'Spear':['','·分岚','·穿阵']}[r['type']][v]
assert len({r['name'] for r in rows})==123
for i,(n,t,e) in enumerate([('光之杖','Staff','Light'),('光剑','Sword','Light'),('暗黑低语匕首','Sword','Dark'),('曙光长枪','Spear','Light'),('晨星弓','Bow','Light'),('棱镜弩','Bow','Light'),('贤者魂灯','Staff','Dark'),('缄魂弩','Bow','Dark'),('渡魂镰','Sword','Dark')]):
 rows.append(dict(id=123+i,name=n,type=t,element=e,sheet=7+i//3,cell=i%3))
out=root/'Art/WeaponIdentityV3';out.mkdir(parents=True,exist_ok=True);(out/'catalog.json').write_text(json.dumps(rows,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(rows[:18],ensure_ascii=False))
