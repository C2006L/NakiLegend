from pathlib import Path
import shutil
root=Path(__file__).resolve().parents[1]
def edit(name,pairs):
 p=root/'Assets/NakiPrototype'/name;s=p.read_text(encoding='utf-8-sig')
 for a,b in pairs:
  assert a in s,(name,a[:65]);s=s.replace(a,b,1)
 p.write_text(s,encoding='utf-8')
edit('NakiPrototype.WorldAtlas.cs',[
 ('if(source!=island){','Vector2 arrival=position;\n   if(source!=island){'),
 ('int[] destinationTalents=CopyTalents();','arrival=position;int[] destinationTalents=CopyTalents();'),
 ('NormalizeOriginPower();MergeVoyageTalents(destinationTalents);','NormalizeOriginPower();RestoreTalents(destinationTalents);MergeVoyageTalents(player.talents);'),
 ('EnterExplorationRoom(room,-1);chapterCg=pendingCg;','EnterExplorationRoom(room,-1);if(floor<0){position=NearestSafeFooting(arrival);UpdatePixelHero(Vector2.zero);}chapterCg=pendingCg;'),
])
edit('NakiPrototype.cs',[('if(HandleIceVoyage())return;','if(HandleWorldAtlas())return;\n            if(HandleIceVoyage())return;')])
edit('NakiPrototype.Audio.cs',[('return challengeMenu||originAbilityMenu','return worldAtlasMenu||challengeMenu||originAbilityMenu')])
edit('NakiPrototype.JourneyHud.cs',[
 ('if(audioSettingsMenu){DrawAudioSettings(w,h);return;}','if(worldAtlasMenu){DrawWorldAtlas(w,h);return;}\n    if(audioSettingsMenu){DrawAudioSettings(w,h);return;}'),
 ('"M 地图",journeySmallStyle','"M 房间 · G 全域",journeySmallStyle'),
 ('ESC 暂停   M 地图   F5','ESC 暂停   M 房间 G 全域   F5'),
 ('RoomHeading(journeyRooms[currentRoom])+" · 探索记录",journeyTitleStyle);','RoomHeading(journeyRooms[currentRoom])+" · 探索记录",journeyTitleStyle);\n   if(FlatButton(new Rect(w/2+180,h/2-270,155,35),"G 全域航图")){OpenWorldAtlas();return;}'),
 ('new Rect(w/2-334,h/2-270,670,38)','new Rect(w/2-334,h/2-270,500,38)'),
])
edit('NakiPrototype.FireCoast.cs',[
 ('if(a==400){ReturnFromFire();return;}','if(a==400){OpenWorldAtlas(chapterEnding?4:3);return;}'),
 ('"乘船返回霜潮群岛"','"展开 Naki 航图 · 自选目的地"'),
 ('"继续主线 · 前往青栖林海"','"展开航图 · 推荐青栖林海"'),
 ('"火岛旅程完成 · 右侧码头可前往青栖林海"','"火岛旅程完成 · G 展开航图，自选目的地"'),
])
edit('NakiPrototype.FireCore.cs',[('if(a==507){RequestFireForestVoyage();return;}','if(a==507){OpenWorldAtlas(4);return;}')])
edit('NakiPrototype.IceChapter.cs',[
 ('if(a==340){RequestIceVoyage(a);return;}','if(a==340){OpenWorldAtlas(3);return;}'),
 ('if(a==304){RequestIceVoyage(a);return;}','if(a==304){OpenWorldAtlas(2);return;}'),
])
edit('NakiPrototype.IceSample.cs',[('if(action==304){RequestIceVoyage(action);return;}','if(action==304){OpenWorldAtlas(2);return;}')])
edit('NakiPrototype.Woodland.cs',[('case 209:RequestWoodReturn();return;','case 209:OpenWorldAtlas(4);return;'),('"返航上一岛 · 赤炉群岛（需确认）"','"展开 Naki 航图 · 自选目的地"')])
edit('NakiPrototype.ThunderStory.cs',[
 ('if(a.kind==305){EmbarkIceSample();return true;}','if(a.kind==305){OpenWorldAtlas(2);return true;}if(a.kind==16){OpenWorldAtlas(1);return true;}'),
 ('"左侧码头 · 乘船前往冰港"','"展开航图 · 推荐冰港"'),
 ('"右侧码头 · 返回沙漠"','"全域航图 · 回访与探索"'),
])
edit('NakiPrototype.Equipment.cs',[('Vector2.Distance(position,ThunderDockAt)<1.4f){EmbarkThunderChapter();return true;}','Vector2.Distance(position,ThunderDockAt)<1.4f){OpenWorldAtlas(1);return true;}')])
edit('NakiPrototype.Runtime.cs',[('woodlandNature.Clear();ClearFireHazards();','woodlandNature.Clear();forestQuestArt.Clear();worldAtlasMenu=atlasConfirm=false;ClearFireHazards();')])
asset=root/'Assets/NakiPrototype/Resources/NakiArt/WorldAtlas';asset.mkdir(parents=True,exist_ok=True)
shutil.copy2(Path('C:/Users/15853/.codex/generated_images/01a1764d-2b16-7840-a400-e14f3ffe5ffd/exec-4eb7506a-448e-4c9d-94c5-fafcd7b985be.png'),asset/'naki-region.png')
print('Atlas integrated with all player-facing harbor actions')
