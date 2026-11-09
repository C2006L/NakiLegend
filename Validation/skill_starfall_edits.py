from pathlib import Path
import uuid
root=Path(__file__).resolve().parents[1];base=root/'Assets/NakiPrototype'
def edit(file,old,new):
 p=base/('NakiPrototype.'+file+'.cs');s=p.read_text(encoding='utf-8');assert old in s,(file,old[:90]);p.write_text(s.replace(old,new),encoding='utf-8')
edit('Powers','private void ResetPowers(){','private void ResetPowers(){ResetSkillLoadout();')
edit('Powers','if(pool==PowerPool.Elemental)cost=ElementCost(cost);','if(cost<=0)return true;if(pool==PowerPool.Elemental)cost=ElementCost(cost);')
edit('Powers','energy-=cost;','energy=Mathf.Max(0,energy-cost);')
edit('Powers','elementPower-=cost;else darkPower-=cost;','elementPower=Mathf.Max(0,elementPower-cost);else darkPower=Mathf.Max(0,darkPower-cost);')
edit('OriginWeapons','float power=w.pool==PowerPool.Light?energy:w.pool==PowerPool.Dark?darkPower:elementPower;','if(WeaponCost(w,extra)<=0)return true;float power=w.pool==PowerPool.Light?energy:w.pool==PowerPool.Dark?darkPower:elementPower;')
edit('Equipment','cost=weapon.cost+(charge>.15f?1:0);','cost=weapon.cost+(weapon.cost>0&&charge>.15f?1:0);')
edit('CombatV3','1.70f,1.90f,2.07f','1.70f,2.11f,2.07f')
edit('CombatV3','4.80f,5.58f,6.05f','4.80f,6.14f,6.05f')
edit('CombatV3','1.28f,1.38f,1.48f','1.28f,1.43f,1.48f')
edit('CombatV3','.90f,.88f,.86f','.90f,.82f,.86f')
# Skill selection is a player decision. Story learning only unlocks entries.
edit('FireCoast','fireBoltKnown=true;chapterSkillSlots[1]=6;','fireBoltKnown=true;')
edit('FireCoast','已学会火种投射 · I 释放，T 可配置','已学会火种投射 · T 自选装备，保留当前技能')
edit('FireWorkshop','emberRingKnown=true;chapterSkillSlots[1]=7;','emberRingKnown=true;')
edit('FireWorkshop','已学会烬环 · I 释放，T 调整','已学会烬环 · T 自选装备，保留当前技能')
edit('IceChapter','iceSpikeKnown=iceRingKnown=true;chapterSkillSlots[1]=4;','iceSpikeKnown=iceRingKnown=true;AnnounceDiscovery("已领悟冰锥与霜环 · T 自选装备，保留当前技能");')
edit('IceSample','chapterSkillSlots[0]=chapterMarkKnown?2:0;chapterSkillSlots[1]=4;TellStory','TellStory')
edit('IceSample','已领悟冰锥与霜环 · I 冰锥；T 可调整','已领悟冰锥与霜环 · T 自选装备，保留当前技能')
edit('IceSample','chapterSkillSlots[0]=mark?2:0;chapterSkillSlots[1]=arc?3:1;','')
edit('FireCoast','chapterSkillSlots[0]=spike?4:0;chapterSkillSlots[1]=ring?5:1;','')
edit('FireCoreVoyage','chapterSkillSlots[0]=6;chapterSkillSlots[1]=4;','')
edit('WorldAtlas','if(!loaded){chapterSkillSlots[0]=bladeLearned?0:iceSpikeKnown?4:0;chapterSkillSlots[1]=waveLearned?1:iceRingKnown?5:1;}','RestoreSkillLoadout(player.loadout,true);')
edit('IceSample','private string[] ActiveSkillNames {get{return ExtendedSkillNames(fireChapterMode?FireSkills:iceSampleMode?IceSkills:woodlandMode?new[]{"风之刃","护身风波","根刺","孢种","冰锥","霜环","火种投射","烬环"}:ChapterSkillNames);}}','private string[] ActiveSkillNames {get{return GlobalSkillNames;}}')
edit('JourneySkills','int count=fireChapterMode||woodlandMode?8:iceSampleMode?6:4;return id>=0&&(id<count||id==8&&talents!=null&&talents.Length==6&&talents[0]>=0||id==9&&talents!=null&&talents.Length==6&&talents[1]>=0);','return id>=0&&id<12&&(id!=8&&id!=9||talents!=null&&talents.Length==6&&talents[id==8?0:1]>=0);')
edit('JourneySkills','string[] original=fireChapterMode?FireSkills:iceSampleMode?IceSkills:woodlandMode?new[]{"风之刃","护身风波","根刺","孢种","冰锥","霜环","火种投射","烬环"}:ChapterSkillNames;','string[] original=GlobalSkillNames;')
edit('JourneySkills','var ids=new List<int>();for(int i=0;i<original.Length;i++)ids.Add(i);ids.Add(8);ids.Add(9);','var ids=new List<int>();for(int i=0;i<original.Length;i++)if(i<2||ChapterSkillKnown(i)||chapterSkillSlots[0]==i||chapterSkillSlots[1]==i)ids.Add(i);')
edit('JourneySkills','n/2*87','n/2*75')
p=base/'NakiPrototype.ThunderSkills.cs';s=p.read_text(encoding='utf-8');start=s.index('        private bool ChapterSkillKnown');end=s.index('        private void CastChapterSkill',start)
s=s[:start]+'''        private bool ChapterSkillKnown(int skill){return ElementSkillKnown(skill);}
        private float ChapterSkillReady(int skill){switch(skill){case 0:return bladeReadyAt;case 1:return waveReadyAt;case 2:return chapterMarkReady;case 3:return chapterArcReady;case 4:return iceSpikeReady;case 5:return iceRingReady;case 6:return fireBoltReady;case 7:return emberRingReady;case 8:return returnBladeReady;case 9:return guardPulseReady;case 10:return forestRootReady;case 11:return forestSporeReady;default:return 0;}}
        private float ChapterSkillCost(int skill){return skill>=0&&skill<12?new float[]{6,10,8,10,6,12,6,12,6,10,7,9}[skill]:0;}
'''+s[end:]
start=s.index('            if(chapterSkillSlots[slot]>=8)');end=s.index('            if(skill==0)',start)
s=s[:start]+'''            int skill=chapterSkillSlots[slot];if(!ChapterSkillKnown(skill)){JourneyHint("这项元素技能尚未学会");return;}
            if(skill==8||skill==9){CastJourneySkill(skill);return;}if(skill>=10){CastWoodSkill(slot);return;}
            if(skill==7){CastEmberRing();return;}if(skill==6){CastFireBolt();return;}if(skill==4||skill==5){CastIceSkill(slot);return;}
'''+s[end:];p.write_text(s,encoding='utf-8')
edit('Woodland','if(skill==3){','if(skill==11){')
edit('Woodland','chapterArcReady=clock+2.4f','forestSporeReady=clock+2.4f')
edit('Woodland','SkillSound(skill==2?','SkillSound(skill==10?')
edit('Woodland','if(skill==2){chapterMarkReady=clock+1.6f','if(skill==10){forestRootReady=clock+1.6f')
edit('Woodland','case 202:chapterMarkKnown=true;','case 202:chapterMarkKnown=true;learnedElementSkills|=1<<10;')
edit('Woodland','case 205:chapterArcKnown=true;','case 205:chapterArcKnown=true;learnedElementSkills|=1<<11;')
# Skill snapshots travel with the player, while island quests remain local.
edit('ThunderSave','public float originSecond,originThird,lightRegen;','public SkillLoadout loadout;public float originSecond,originThird,lightRegen;')
edit('ThunderChapterSave','return new ThunderCheckpoint{','return new ThunderCheckpoint{loadout=CaptureSkillLoadout(),')
edit('ThunderChapterSave','ThunderCheckpoint p=s.player;','ThunderCheckpoint p=s.player;ValidateSkillLoadout(p.loadout);')
edit('ThunderChapterSave','MigrateWoodlandCheckpoint(candidate);ValidateChapterCheckpoint','MigrateWoodlandCheckpoint(candidate);MigrateLegacySkillLoadout(candidate);ValidateChapterCheckpoint')
edit('ThunderChapterSave','Array.Copy(s.skills,chapterSkillSlots,2);','Array.Copy(s.skills,chapterSkillSlots,2);if(s.player.loadout!=null)RestoreSkillLoadout(s.player.loadout);else if(woodlandMode){if(s.mark)learnedElementSkills|=1<<10;if(s.arc)learnedElementSkills|=1<<11;}')
edit('ThunderChapterSave','lastDamageAt=p.lastDamage;','lastDamageAt=p.lastDamage;RestoreSkillLoadout(p.loadout);')
edit('Talents','MergeOriginJourney(journey);MergeVoyageTalents(source);','MergeOriginJourney(journey);MergeVoyageTalents(source);RestoreSkillLoadout(journey.loadout,true);')
edit('Checkpoints','public bool knownMark,','public SkillLoadout loadout;public bool knownMark,')
edit('Checkpoints','var s=new JourneyCheckpoint {','var s=new JourneyCheckpoint {loadout=CaptureSkillLoadout(),')
edit('Checkpoints','ValidateTalents(s==null?null:s.talents);','ValidateSkillLoadout(s==null?null:s.loadout);ValidateTalents(s==null?null:s.talents);')
edit('Checkpoints','desertStoneRecovered=s.stone;','desertStoneRecovered=s.stone;RestoreSkillLoadout(s.loadout);')
edit('OriginLegacy','primarySkill=Mathf.Clamp(source.primary,0,2);RequestCheckpoint();','primarySkill=Mathf.Clamp(source.primary,0,2);RestoreSkillLoadout(source.loadout,true);RequestCheckpoint();')
edit('WorldAtlas','bool inheritedMark=chapterMarkKnown,inheritedArc=chapterArcKnown;','bool inheritedMark=chapterMarkKnown,inheritedArc=chapterArcKnown;') # unchanged context assertion
# All islands use the same user-selected slots, including return trips to Mingsha.
p=base/'NakiPrototype.cs';s=p.read_text(encoding='utf-8');s=s.replace("if(thunderChapterMode)CastChapterSkill(0);else CastWindBlade();","CastChapterSkill(0);").replace("if(thunderChapterMode)CastChapterSkill(1);else CastWind();","CastChapterSkill(1);");p.write_text(s,encoding='utf-8')
edit('JourneyHud','if(thunderChapterMode){for(int i=0;i<2;i++)','if(enableDesertJourney){for(int i=0;i<2;i++)')
# New P ability has no projectile-volley dependency; clear its delayed impacts on room/reset.
p=base/'NakiPrototype.OriginLegacy.cs';s=p.read_text(encoding='utf-8');start=s.index('   if(!OriginAbilityKnown(2)',s.index('private void CastEmpoweredMeteor'));end=s.index('\n  }',start);s=s[:start]+'   CastStarfall();'+s[end:];s=s.replace('五颗强化追踪球 · 21 贤者之石能量','三处落点预警后坠星，造成范围伤害并引爆蚀魂印 · 21 贤者之石能量');p.write_text(s,encoding='utf-8')
edit('OriginCombat','new[]{.8f,4.5f,1.2f}','new[]{.8f,4.5f,4.5f}')
edit('OriginCombat','private void TickOriginVisuals()\n  {','private void TickOriginVisuals()\n  {\n   TickStarfall();')
edit('OriginCombat','private void ResetOriginCombat()\n  {','private void ResetOriginCombat()\n  {\n   ClearStarfall();')
edit('OriginCombat','private void ResetOriginRoomEffects()\n  {','private void ResetOriginRoomEffects()\n  {\n   ClearStarfall();')
edit('Checkpoints','&&!OriginActionActive&&!challengeActive','&&!OriginActionActive&&fallingStars.Count==0&&!challengeActive')
for name in ['SkillLoadout','Starfall']:
 p=base/('NakiPrototype.'+name+'.cs.meta');p.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid5(uuid.NAMESPACE_URL,'nakilegend/'+name+'-v1').hex+'\nMonoImporter:\n  externalObjects: {}\n  serializedVersion: 2\n  defaultReferences: []\n  executionOrder: 0\n  icon: {instanceID: 0}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n',encoding='utf-8')
print('Applied zero-cost, skill ownership, ice pacing and falling-star integration')
