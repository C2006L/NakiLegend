from pathlib import Path
root=Path(__file__).resolve().parents[2]
base=root/'Assets/NakiPrototype'
def edit(name,changes):
    p=base/name;s=p.read_text(encoding='utf-8-sig')
    for a,b in changes:
        assert a in s,(name,a)
        s=s.replace(a,b)
    p.write_text(s,encoding='utf-8')
edit('NakiPrototype.FireCoast.cs',[
 ('"火种投射"};','"火种投射","烬环"};'),
 ('chapterFloors[0]=FireProp(0);','chapterFloors[0]=FireProp(0);LoadWorkshopArt();'),
 ('stageEnds[i]=12;}','stageEnds[i]=12;}AddFireWorkshop();'),
 ('fireBoltKnown=false;fireBoltReady=', 'forgeRescued=forgeBridgeOpen=emberRingKnown=false;emberRingReady=0;fireBoltKnown=false;fireBoltReady='),
 ('if(thunderChapterPracticeBoss>=0){chapterRanger=true;fireBoltKnown=true;}EnterExplorationRoom(thunderChapterPracticeBoss>=0?12:0,-1);', 'if(thunderChapterPracticeBoss>=0||thunderChapterStartFloor>0){chapterRanger=true;fireBoltKnown=true;}if(thunderChapterPracticeBoss>0||thunderChapterStartFloor>0){chapterStone=true;journeyRooms[12].cleared=true;}if(thunderChapterPracticeBoss>0){forgeRescued=forgeBridgeOpen=emberRingKnown=true;}EnterExplorationRoom(thunderChapterPracticeBoss>=0?(thunderChapterPracticeBoss>0?27:12):thunderChapterStartFloor>0?15:0,-1);'),
 ('"赤脊熔蜥练习：冰击会冷却熔壳，普通武器也能破壳。练习不覆盖正式存档。"', '(thunderChapterPracticeBoss>0?"锻链巨匠练习：先破坏铸模可减少折射弹幕，锤击后炉芯暴露。练习不写正式存档。":"赤脊熔蜥练习：冰击会冷却熔壳，普通武器也能破壳。练习不覆盖正式存档。")'),
 ('"船夫：黑砂滩上的炉火还亮着，可镇上的灶已经冷了三天。阿铎在等最后一批撤离的人。"', '(thunderChapterStartFloor>0?"工坊测试起点：黑砂海岸的必经事件已完成，沿炉道寻找工匠阿铎。":"船夫：黑砂滩上的炉火还亮着，可镇上的灶已经冷了三天。阿铎在等最后一批撤离的人。")'),
 ('if(n.id==4||n.id==13||n.id==0)', 'if(n.id==4||n.id==13||n.id==0||n.id==20||n.id==23||n.id==28)'),
 ('private void BuildFireCover(RoomNode n){', 'private void BuildFireCover(RoomNode n){if(n.stage==1){BuildWorkshopCover(n);return;}'),
 ('private void SpawnFireWave(RoomNode n){', 'private void SpawnFireWave(RoomNode n){if(n.stage==1){SpawnWorkshopWave(n);return;}'),
 ('else if(n.kind==RoomKind.Elite)ChapterDrop(19,', 'else if(n.kind==RoomKind.Elite)ChapterDrop(n.stage==1?22:19,'),
 ('private void BuildFireActions(RoomNode n){', 'private void BuildFireActions(RoomNode n){if(n.stage==1){BuildWorkshopActions(n);return;}'),
 ('"通往锻链工坊的封闭炉门"', 'fireBoltKnown?"工坊炉道 · 沿石阶向上":"工坊炉道 · 先学会火种"'),
 ('private void FireAction(int a){', 'private void FireAction(int a){if(a>=410){WorkshopAction(a);return;}'),
 ('if(a==406)TellStory("工坊深处的热浪尚未退去，门边立着阿铎新留下的路标。","当前可探索黑砂海岸第一层。锻链工坊与地心赤炉仍在制作，火元素碎片尚未找回。");', 'if(a==406){if(!chapterStone||!fireBoltKnown)JourneyHint("先找到熔蜥身后的安全训练炉");else JourneyHint("靠近本房间石阶按 F，前往锻链工坊");}'),
 ('if(a==401)PlayWinchTurn(new Vector2(-3,0));RequestCheckpoint();', 'if(a==401)PlayWinchTurn(new Vector2(-3,0));UpdateExplorationDoors();RequestCheckpoint();'),
 ('ClearChapterSkillEffects();ClearJourneyRoom();chapterStone=true;journeyComplete=false;', 'ClearChapterSkillEffects();float oldCap=EffectiveMaximumShield;ClearJourneyRoom();chapterStone=true;journeyComplete=false;if(journeyRooms[currentRoom].stage==1){shield=Mathf.Min(EffectiveMaximumShield,shield+Mathf.Max(0,EffectiveMaximumShield-oldCap));TellStory("巨匠胸前的炉芯暗了下来。锁链停在半空，刚刚落稳的撤离桥没有再被热浪掀起。","阿铎：工坊的路通了。我替你补好护具，接下来的地心炉门，还得靠我们自己去打开。");AnnounceDiscovery("炉锻护具 · 护盾上限 +1");BuildChapterActions();UpdateExplorationDoors();RequestCheckpoint();SaveChapterCheckpoint(false);return;}'),
 ('private string FireObjective(){return ', 'private string FireObjective(){if(journeyRooms[currentRoom].stage==1)return !forgeRescued?"寻找工匠阿铎 · 打开避难间":!forgeBridgeOpen?"炉匠已获救 · 放下工坊撤离桥":!journeyRooms[27].cleared?"沿炉道寻找锻链巨匠 · 破坏铸模可削弱弹幕":"工坊撤离路已打通 · 护盾强化，可回港";return '),
 ('"黑砂海岸第一层完成 · 可回港或返回冰岛"','"前往工坊前哨 · 沿石阶进入第二层"'),
 ('private bool HandleFireSetup(){if(chapterConfirmRestart){if(Pressed(\'e\'))chapterConfirmRestart=false;return true;}', 'private bool HandleFireSetup(){if(chapterConfirmRestart){if(Pressed(\'e\'))chapterConfirmRestart=false;if(Pressed(\'f\'))StartChapterFromSetup(true);return true;}if(Pressed(\'a\')||Pressed(\'d\'))thunderChapterStartFloor=1-Mathf.Clamp(thunderChapterStartFloor,0,1);'),
 ('"第四岛 · 黑砂海岸 · 第一层"','"第四岛 · 赤炉群岛 · 两层试玩"'),
 ('"15 个房间 · 救援避难者 → 赤脊熔蜥 → 安全训练炉\\n独立测试配置已备冰技能和 7 护盾；从冰岛乘船则继承实际装备与技能。"','"30 个房间 · 黑砂海岸 → 锻链工坊\\n救援居民与工匠，放下撤离桥，挑战两位首领。"'),
 ('for(int i=0;i<2;i++)if(FlatButton(new Rect(w/2-300+i*310,285,290,55)', 'if(chapterConfirmRestart){CenterLabel(new Rect(w/2-440,260,880,65),"另起测试会先备份原火岛存档。继续旧进度请选择返回，再按 F 继续。",journeyTextStyle);if(FlatButton(new Rect(w/2-310,360,300,60),"返回，保留当前进度"))chapterConfirmRestart=false;if(FlatButton(new Rect(w/2+10,360,300,60),"F 备份后另起测试"))StartChapterFromSetup(true);return;}for(int i=0;i<2;i++)if(FlatButton(new Rect(w/2-300+i*310,285,290,55)'),
 ('"F 开始黑砂海岸"','"F 从所选楼层开始"'),
 ('CenterLabel(new Rect(w/2-460,490,920,70),"清房与事件后自动保存；F5 / F9 存读；T 配置技能。\\n工坊、地心与火岛结尾 CG 尚未开放，不会误算整岛通关。",journeySmallStyle);', 'if(FlatButton(new Rect(w/2-310,465,300,55),thunderChapterStartFloor==0?"A / D 起点：黑砂海岸":"A / D 起点：锻链工坊"))thunderChapterStartFloor=1-Mathf.Clamp(thunderChapterStartFloor,0,1);if(FlatButton(new Rect(w/2+10,465,300,55),"从所选起点另起测试"))StartChapterFromSetup();CenterLabel(new Rect(w/2-460,550,920,70),"已有进度按 F 继续；另起测试先备份。地心与结尾 CG 尚未开放。\\n独立测试保底 7 护盾；正式乘船继承实际装备与技能。",journeySmallStyle);'),
 ('for(int i=0;i<7;i++){float y=h/2-235+i*64;', 'for(int i=0;i<8;i++){float y=h/2-235+i*55;'),
])
edit('NakiPrototype.FireCombat.cs',[
 ('kind<=EnemyKind.RedSpineLizard','kind<=EnemyKind.ForgeMold'),
 ('private void ConfigureFireEnemy(Enemy e){','private void ConfigureFireEnemy(Enemy e){if(IsWorkshopEnemy(e.kind)){ConfigureWorkshopEnemy(e);return;}'),
 ('private void UpdateFireEnemy(Enemy e,float dt){','private void UpdateFireEnemy(Enemy e,float dt){if(IsWorkshopEnemy(e.kind)){UpdateWorkshopEnemy(e,dt);return;}'),
 ('private void ClearFireHazards(){','private void ClearFireHazards(){ClearWorkshopEffects();'),
 ('private void TickFireCombat(){','private void TickFireCombat(){TickWorkshopEffects();'),
])
edit('NakiPrototype.IceSample.cs',[( 'fireChapterMode?15:', 'fireChapterMode?30:'),('fireChapterMode?"fire-coast-v1"','fireChapterMode?"fire-forge-v2"')])
edit('NakiPrototype.Progression.cs',[('Mathf.Max(maximumShield+2,fireShieldFloor):','Mathf.Max(maximumShield+2,fireShieldFloor)+WorkshopShieldBonus:')])
edit('NakiPrototype.Enemies.cs',[
 ('CoalThrower, RedSpineLizard }','CoalThrower, RedSpineLizard, ForgeGuard, FurnaceCore, MoltenCrab, ForgeMaster, ForgeMold }'),
 ('"赤脊熔蜥" };','"赤脊熔蜥", "锻火枪卫", "炉芯傀儡", "裂壳熔蟹", "锻链巨匠", "折射铸模" };')])
edit('NakiPrototype.ThunderBosses.cs',[('kind==EnemyKind.RedSpineLizard;','kind==EnemyKind.RedSpineLizard||kind==EnemyKind.ForgeMaster;')])
edit('NakiPrototype.ThunderChapter.cs',[
 ('fireChapterMode?"第四岛 · 黑砂海岸"','fireChapterMode?"第四岛 · "+(journeyRooms[currentRoom].stage==0?"黑砂海岸":"锻链工坊")'),
 ('if(fireChapterMode)return n.id==0?', 'if(fireChapterMode)return n.id==20?"炉匠":n.id==23?"撤离桥":n.id==27?"巨匠":n.id==0?'),
 ('&&link==12&&!chapterRanger', '&&(link==12&&!chapterRanger||link==27&&(!forgeRescued||!forgeBridgeOpen))'),
])
edit('NakiPrototype.Exploration.cs',[('RoomNode node=journeyRooms[currentRoom];if(!node.cleared||node.stairsTo<0||stairs!=null)return;', 'RoomNode node=journeyRooms[currentRoom];if(fireChapterMode&&node.id==14&&(!chapterStone||!fireBoltKnown))return;if(!node.cleared||node.stairsTo<0||stairs!=null)return;')])
edit('NakiPrototype.ThunderSkills.cs',[
 ('weaponCatalog=items.ToArray();','items.Add(Weapon("炉锻重剑",WeaponType.Sword,Element.Fire,38,.9f,2,2,1.4f,weaponCatalog[5].sprite,"ffb873"));weaponCatalog=items.ToArray();'),
 ('return fireChapterMode&&skill==6?fireBoltKnown:', 'return fireChapterMode&&skill==7?emberRingKnown:fireChapterMode&&skill==6?fireBoltKnown:'),
 ('return fireChapterMode&&skill==6?fireBoltReady:', 'return fireChapterMode&&skill==7?emberRingReady:fireChapterMode&&skill==6?fireBoltReady:'),
 ('return fireChapterMode&&skill==6?6:', 'return fireChapterMode&&skill==7?12:fireChapterMode&&skill==6?6:'),
 ('if(fireChapterMode&&chapterSkillSlots[slot]==6)', 'if(fireChapterMode&&chapterSkillSlots[slot]==7){CastEmberRing();return;}if(fireChapterMode&&chapterSkillSlots[slot]==6)'),
])
edit('NakiPrototype.Elements.cs',[('if(enemy.kind==EnemyKind.TideGuardian&&clock<tideCoreUntil)', 'if(enemy.kind==EnemyKind.ForgeMaster&&clock<enemy.recoveryUntil)amount*=1.2f;if(enemy.kind==EnemyKind.TideGuardian&&clock<tideCoreUntil)')])
edit('NakiPrototype.Runtime.cs',[('firePropArt.Clear();ClearFireHazards();','firePropArt.Clear();workshopArt.Clear();ClearFireHazards();')])
edit('NakiPrototype.ThunderChapterSave.cs',[
 ('public bool iceSpike,iceRing,fireBolt;', 'public bool forgeRescued,forgeBridge,emberRing;public float emberReady;public bool iceSpike,iceRing,fireBolt;'),
 ('fireBolt=fireBoltKnown,','forgeRescued=forgeRescued,forgeBridge=forgeBridgeOpen,emberRing=emberRingKnown,emberReady=emberRingReady,fireBolt=fireBoltKnown,'),
 ('s.fireReady})','s.fireReady,s.emberReady})'),
 ('fireChapterMode?Mathf.Max(maximumShield+2,s.fireShield):', 'fireChapterMode?Mathf.Max(maximumShield+2,s.fireShield)+(s.rooms[27]!=null&&s.rooms[27].cleared?1:0):'),
 ('fireChapterMode?6:iceSampleMode?5:3','fireChapterMode?7:iceSampleMode?5:3'),
 ('if(fireChapterMode&&(!FiniteRange', 'if(fireChapterMode&&(s.forgeRescued&&(!s.fireBolt||!s.stone)||s.forgeBridge&&!s.forgeRescued||s.emberRing&&!s.forgeRescued||s.rooms[27].cleared&&(!s.forgeRescued||!s.forgeBridge)))throw new InvalidDataException("Incomplete forge evacuation route");\n            if(fireChapterMode&&(!FiniteRange'),
 ('MigrateIceSampleCheckpoint(candidate);ValidateChapterCheckpoint(candidate);','MigrateIceSampleCheckpoint(candidate);MigrateFireCheckpoint(candidate);ValidateChapterCheckpoint(candidate);'),
 ('fireBoltKnown=s.fireBolt;', 'forgeRescued=s.forgeRescued;forgeBridgeOpen=s.forgeBridge;emberRingKnown=s.emberRing;emberRingReady=s.emberReady;fireBoltKnown=s.fireBolt;'),
 ('fireChapterMode?(fireBoltKnown?', 'fireChapterMode?(journeyRooms[27].cleared?" · 工坊已安定":fireBoltKnown?'),
])
print('Connected two-floor Fire chapter')
