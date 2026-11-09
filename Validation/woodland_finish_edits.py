from pathlib import Path
import shutil
root=Path(__file__).resolve().parents[1]
def edit(name, pairs):
 p=root/'Assets/NakiPrototype'/name
 s=p.read_text(encoding='utf-8-sig')
 for before,after in pairs:
  assert before in s,(name,before[:80])
  s=s.replace(before,after,1)
 p.write_text(s,encoding='utf-8')
edit('NakiPrototype.WoodlandCombatV2.cs',[
 ('boss.windup=0;if(boss.warning!=null)', 'CancelForestStrikes(boss);boss.windup=0;boss.chargeUntil=0;if(boss.warning!=null)'),
 ('else{e.chargeUntil=0;e.recoveryUntil=clock+1.25f;}', 'else{e.chargeUntil=0;e.recoveryUntil=clock+1.25f;}if(e.chargeUntil>clock&&(e.position-old).sqrMagnitude<.000001f){e.chargeUntil=0;e.recoveryUntil=clock+1.25f;}'),
 ('e.warning=k==8?', 'if(!ForestBossTell(e,k))e.warning=k==8?'),
 ('PoseWorkshopEnemy(e,old,dt);if(clock<e.phaseUntil', 'PoseWorkshopEnemy(e,old,dt);if(e.chargeUntil>clock||e.windup>0)e.body.flipX=e.attackDirection.x<0;if(clock<e.phaseUntil'),
 ('int k=ArrayIndexWood(e.kind),move=e.attackNumber%3;e.recoveryUntil=clock+(e.boss?1.45f:.75f);', 'int k=ArrayIndexWood(e.kind),move=e.attackNumber%3;e.recoveryUntil=clock+(e.boss?1.45f:.75f);if(ResolveForestBossMove(e,k))return;'),
 ('private void TickWoodCombat()\n  {', 'private void TickWoodCombat()\n  {\n   TickForestStrikes();'),
 ('private void ClearWoodCombat(){', 'private void ClearWoodCombat(){CancelForestStrikes();'),
])
edit('NakiPrototype.ThunderSkills.cs',[
 ('{ClearRelicCombat();if(originBeam', '{ClearWoodCombat();ClearRelicCombat();if(originBeam'),
])
edit('NakiPrototype.Woodland.cs',[
 ('chapterMarkReady=clock+1.6f;ApplySkillHit(target,SkillDamage(24),Element.Wood,"root");target.recoveryUntil=Mathf.Max(target.recoveryUntil,clock+.6f);SpellSpark(target.position,C("a4e589"),.7f);', 'chapterMarkReady=clock+1.6f;CastForestRoots(target,aim);'),
 ('WoodPropAction(202,2,', 'ForestQuestAction(202,4,'),
 ('WoodPropAction(204,n.id==14?4:n.id==16?6:3,', 'ForestQuestAction(204,n.id==14?2:n.id==16?3:1,'),
 ('WoodPropAction(205,7,', 'ForestQuestAction(205,5,'),
 ('WoodProp(3,new Vector2(-2,0),chapterLampGiven?', 'ForestQuestProp(chapterLampGiven?1:0,new Vector2(-2,0),chapterLampGiven?'),
 ('   BuildWorkshopGuide(n);', '   BuildForestAftermath(n);BuildWorkshopGuide(n);'),
 ('case 201:if(chapterStone&&chapterLampGiven){chapterCg=true;storyLines=null;}else TellStory', 'case 201:if(chapterStone&&chapterLampGiven){chapterCg=true;storyLines=null;}else if(chapterStone)TellStory("阿苓：林册带回来了，种子呢？那块旧种床，还在等你。","少年低头看了看木盒：先让它回到泥土里，再把好消息带回来。");else TellStory'),
 ('"采种人：别把白丝当成树根。树根知道什么时候停，它不知道。"', '"采种人：别把白丝当成树根。树根知道什么时候停，它不知道。","采种人指着林册：去看看封丝树孔、空果壳和幼芽，找出白丝夺走了什么。我先沿清过的路去码头报平安。"'),
 ('"已领悟根刺 · T 装备到 U / I"', '"已领悟根刺 · 自动锁敌，直线最多缠住三只 · T 装备"'),
 ('"已领悟孢种 · T 配置"', '"已领悟孢种 · 追踪孢种附加腐解伤害 · T 配置"'),
 ('private string WoodObjective(){return chapterEnding?', 'private string WoodObjective(){if(!chapterStone&&currentRoom==6&&journeyRooms[6].cleared&&!chapterMarkKnown)return "翻阅落叶中的林册 · 领悟根刺，再沿树洞进入腹地";if(!chapterStone&&currentRoom==18&&journeyRooms[18].cleared&&!chapterArcKnown)return "拾取菌冠留下的孢种 · 领悟法术，再登树冠旧庭";return chapterEnding?'),
])
# Store the generated alpha atlas unchanged; all slicing happens in Unity.
asset=root/'Assets/NakiPrototype/Resources/NakiArt/WoodlandV3'
asset.mkdir(parents=True,exist_ok=True)
shutil.copy2(Path('C:/Users/15853/.codex/generated_images/01a1764d-2b16-7840-a400-e14f3ffe5ffd/exec-4c61668f-3c44-41a0-a357-4972e9b1c334.png'),asset/'quest-props.png')
print('Forest runtime and unchanged generated atlas staged')
