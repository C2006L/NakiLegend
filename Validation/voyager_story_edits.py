from pathlib import Path
root=Path(__file__).resolve().parents[1]
def edit(name,pairs):
 p=root/'Assets/NakiPrototype'/name;s=p.read_text(encoding='utf-8-sig')
 for a,b in pairs:
  assert a in s,(name,a[:60]);s=s.replace(a,b,1)
 p.write_text(s,encoding='utf-8')
p=root/'Assets/NakiPrototype/NakiPrototype.OriginLegacy.cs';s=p.read_text(encoding='utf-8-sig');a=s.index('     string[] light=');b=s.index('if(i==1||i==3)',a);s=s[:a]+'     TellStory(VoyagerLines(i));'+s[b:];s=s.replace('!inventoryMenu&&!mapMenu&&!talentMenu','!inventoryMenu&&!mapMenu&&!talentMenu&&!voyagerJournal&&!worldAtlasMenu');p.write_text(s,encoding='utf-8')
edit('NakiPrototype.Backpack.cs',[
 ('if(inventoryMenu)\n   {','if(inventoryMenu)\n   {\n    if(pendingWeaponPickup==null&&Pressed(\'v\')){OpenVoyagerJournal();return true;}'),
 ('GUI.Label(new Rect(panel.x+28,panel.y+18,840,35),"行囊 · 双武器与备用装备",journeyTitleStyle);','GUI.Label(new Rect(panel.x+28,panel.y+18,620,35),"行囊 · 双武器与备用装备",journeyTitleStyle);\n   if(pendingWeaponPickup==null&&FlatButton(new Rect(panel.x+674,panel.y+18,168,35),"V 航海手记")){OpenVoyagerJournal();return;}'),
])
edit('NakiPrototype.Journey.cs',[('if(HandleOptionalChallengeMenu()||','if(HandleVoyagerJournal()||HandleOptionalChallengeMenu()||')])
edit('NakiPrototype.JourneyHud.cs',[('if(worldAtlasMenu){DrawWorldAtlas(w,h);return;}','if(voyagerJournal){DrawVoyagerJournal(w,h);return;}\n    if(worldAtlasMenu){DrawWorldAtlas(w,h);return;}')])
edit('NakiPrototype.Audio.cs',[('return worldAtlasMenu||challengeMenu','return voyagerJournal||worldAtlasMenu||challengeMenu')])
edit('NakiPrototype.WorldAtlas.cs',[('!chapterSkillsMenu&&!challengeMenu','!chapterSkillsMenu&&!voyagerJournal&&!challengeMenu')])
edit('NakiPrototype.Powers.cs',[('originAbilityMenu=false;originGuardUntil','originAbilityMenu=voyagerJournal=false;originGuardUntil')])
edit('NakiPrototype.FireAftermath.cs',[
 ('"少年：往后让火留在炉膛里。下一趟船，我会带着这份热意去林海。"','"守岸人把一封旧回信放在面包旁：北方府邸催过药和粮，最近却只肯让运淬炼器具的船靠岸。","少年：先把这里的人照顾好。这封信我收着，下一趟船，也该去问问那些没能回家的人。"'),
])
edit('NakiPrototype.IceChapter.cs',[
 ('"守塔人：潮声回来了，灯下的人也回来了。去吧，南方还有人在等一场及时的雨。"','"守塔人：潮声回来了，灯下的人也回来了。旧船单上的那位旅人，却一直没有归票。","守塔人递来封好的药单：若有机会去暮钟岛，替我们问问收药的人，为什么后来连回信也没有了。"'),
])
edit('NakiPrototype.Woodland.cs',[
 ('"少年：还有根活着，就还有办法。先带我看看那块种床。"','"阿苓：以前送往北方府邸的种子，也收过这样的回信。采种人还没回来，林册也失在里面了。","少年：先救人，再看看根出了什么问题。那封信，也替我留着。"'),
 ('"林册还在行囊里。沿种床边的小径再按 F 返回码头，把新芽的消息告诉阿苓。"','"林册夹页里落出一封北方旧信，信封写着‘暮钟岛·受封者府邸’。少年把信收进航海手记。","林册还在行囊里。沿种床边的小径再按 F 返回码头，把新芽的消息告诉阿苓。"'),
])
print('Connected five-island evidence, route-specific reflections and journal access')
