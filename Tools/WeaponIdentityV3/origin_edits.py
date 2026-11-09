from pathlib import Path
base=Path(__file__).resolve().parents[2]/'Assets/NakiPrototype'
def edit(name,old,new):
 p=base/f'NakiPrototype.{name}.cs';s=p.read_text(encoding='utf-8');assert s.count(old)==1,(name,old,s.count(old));p.write_text(s.replace(old,new),encoding='utf-8')
edit('Equipment','public WeaponEffect effect;','public PowerPool pool=PowerPool.Elemental;public WeaponEffect effect;')
edit('Equipment','if(!SpendPower(PowerPool.Elemental,weapon.cost))','if(!CanUseWeapon(weapon)||!SpendPower(weapon.pool,weapon.cost))')
edit('Equipment','if(elementPower+.001f<ElementCost(cost)){JourneyHint("元素之力不足");','if(!CanUseWeapon(weapon,cost-weapon.cost)){')
edit('Equipment','SpendPower(PowerPool.Elemental,cost);','SpendPower(weapon.pool,cost);')
edit('CombatV3','WeaponDef w=CurrentWeapon;if(elementPower+.001f<ElementCost(w.cost)){JourneyHint("元素之力不足");','WeaponDef w=CurrentWeapon;if(!CanUseWeapon(w)){')
edit('CombatV3','int count=w.effect==WeaponEffect.Needle||w.variant==2?1:w.variant==1||w.effect==WeaponEffect.FlameSpray?5:3;','int count=StaffVolleyCount(w);')
edit('CombatV3','w.damage*(w.effect==WeaponEffect.FlameSpray?.75f:1)*(count==1?(w.effect==WeaponEffect.Needle?1.6f:2.4f):count==5?.55f:.8f)','w.damage*StaffPelletScale(w,count)')
edit('CombatV3','SpendPower(PowerPool.Elemental,w.cost);','SpendPower(w.pool,w.cost);')
edit('Relics','ExpandedWeaponCount=123','ExpandedWeaponCount=132')
edit('Relics','weaponCatalog=items.ToArray();if','AppendOriginWeapons(items);weaponCatalog=items.ToArray();if')
edit('Relics','if(w.signature||WeaponSourceIsland(id)>island)continue;','if(w.signature||WeaponSourceIsland(id)>island||!OriginWeaponAllowed(w))continue;')
edit('CombatPolish','TrySoulSealHit(shot,target);','OriginProjectileHit(shot,target,contact);TrySoulSealHit(shot,target);')
edit('PixelHero','ApplyElementDamage(enemy,CurrentMeleeDamage,CurrentWeapon.element); else','{ApplyElementDamage(enemy,CurrentMeleeDamage,CurrentWeapon.element);OriginMeleeHit(enemy);} else')
edit('Arsenal','IdentityMeleeEffect();RelicMeleeImpact();','IdentityMeleeEffect();OriginMeleeWave();RelicMeleeImpact();')
edit('RelicCombat','private void ClearRelicCombat(){','private void ClearRelicCombat(){whisperHits.Clear();soulCrossMarks.Clear();')
edit('Backpack','case Element.Dark:return "暗";','case Element.Dark:return "贤者之石";')
edit('Backpack','ElementName(weapon.element)+" · 伤害 "+(weapon.damage*(weapon.type==WeaponType.Bow?2f/3:1)).ToString("0.#")+" · "+ElementCost(weapon.cost).ToString("0.##")+" 元素之力"','ElementName(weapon.element)+" · "+WeaponDamageLabel(weapon)+" · "+WeaponCostLabel(weapon)')
edit('Backpack','ElementName(weaponCatalog[id].element)+" · 伤害 "+(weaponCatalog[id].damage*(weaponCatalog[id].type==WeaponType.Bow?2f/3:1)).ToString("0.#")','ElementName(weaponCatalog[id].element)+" · "+WeaponDamageLabel(weaponCatalog[id])')
edit('Backpack','"消耗 "+ElementCost(weaponCatalog[id].cost).ToString("0.##")+" 元素之力"','"消耗 "+WeaponCostLabel(weaponCatalog[id])')
edit('Backpack','"三弹扇射 · 每弹80%基础伤害 · 消耗仅计一次"','StaffVolleyCount(weapon)+" 弹 · 每弹"+(weapon.damage*StaffPelletScale(weapon,StaffVolleyCount(weapon))).ToString("0.#")+"伤害 · 每轮付费一次"')
edit('Backpack','if(weapon.effect!=WeaponEffect.None){','if(selectedId>=123||weapon.effect!=WeaponEffect.None){')
edit('Backpack','RelicDescription(weapon),journeySmallStyle,true);','selectedId>=123?OriginWeaponDescription(selectedId):RelicDescription(weapon),journeySmallStyle,true);')
# Replace player-facing resource terminology, preserving internal enum/serialized field names and weapon proper names.
for p in base.rglob('*.cs'):
 s=p.read_text(encoding='utf-8');old=s
 for a,b in [('暗之力','贤者之石能量'),('暗能','贤者之石能量'),('暗力','贤者之石能量'),('光暗','光之力与贤者之石'),('暗路线','贤者之石路线'),('暗线','贤者之石路线')]:s=s.replace(a,b)
 s=s.replace('机械没有灵魂，武器仍用元素之力','机械没有灵魂；普通武器用元素之力，魂器消耗石中能量')
 if s!=old:p.write_text(s,encoding='utf-8')
print('Appended nine route weapons; shared staff output reduced by 20%; resource UI terminology unified')
