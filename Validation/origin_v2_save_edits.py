from pathlib import Path
root=Path(__file__).resolve().parents[1]/'Assets/NakiPrototype'
def edit(file,old,new):
 p=root/file;s=p.read_text(encoding='utf-8-sig');assert old in s,(file,old[:100]);p.write_text(s.replace(old,new),encoding='utf-8')
for file in ['NakiPrototype.Checkpoints.cs','NakiPrototype.ThunderSave.cs']:
 edit(file,'public int[] talents;public int profession,lore,knowledge,primary,queuedLore,routeOrigin;','public float originSecond,originThird,lightRegen;\n            public int[] talents;public int profession,lore,knowledge,primary,queuedLore,routeOrigin;')
edit('NakiPrototype.ThunderChapterSave.cs','return new ThunderCheckpoint{returnReady=','return new ThunderCheckpoint{originSecond=originSecondReady,originThird=originThirdReady,lightRegen=lightRegenReady,returnReady=')
edit('NakiPrototype.ThunderSave.cs','var s=new ThunderCheckpoint{talents=','var s=new ThunderCheckpoint{originSecond=originSecondReady,originThird=originThirdReady,lightRegen=lightRegenReady,talents=')
edit('NakiPrototype.Checkpoints.cs','var s=new JourneyCheckpoint {','var s=new JourneyCheckpoint {\n                    originSecond=originSecondReady,originThird=originThirdReady,lightRegen=lightRegenReady,')
edit('NakiPrototype.Checkpoints.cs','s.markTimer,s.arcTimer,','s.originSecond,s.originThird,s.lightRegen,s.markTimer,s.arcTimer,')
edit('NakiPrototype.Checkpoints.cs','RestoreOriginLegacy(s.lore,s.knowledge,s.primary,s.queuedLore);','RestoreOriginLegacy(s.lore,s.knowledge,s.primary,s.queuedLore);originSecondReady=s.originSecond;originThirdReady=s.originThird;lightRegenReady=s.lightRegen;')
edit('NakiPrototype.ThunderChapterSave.cs','p.nextSword,p.nextSpell,p.nextDash,','p.originSecond,p.originThird,p.lightRegen,p.nextSword,p.nextSpell,p.nextDash,')
edit('NakiPrototype.ThunderChapterSave.cs','clock=p.clock;repairTalentReady=','clock=p.clock;originSecondReady=p.originSecond;originThirdReady=p.originThird;lightRegenReady=p.lightRegen;repairTalentReady=')
edit('NakiPrototype.WorldAtlas.cs','nextSword+=shift;nextSpell+=shift;','originSecondReady+=shift;originThirdReady+=shift;lightRegenReady+=shift;nextSword+=shift;nextSpell+=shift;')
print('Persisted independent O/P cooldowns and light recovery delay without changing save versions')
