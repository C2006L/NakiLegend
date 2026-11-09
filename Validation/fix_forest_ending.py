from pathlib import Path
r=Path('D:/NakiLegend/Repository');a=r/'Assets/NakiPrototype';changed=[]
def edit(name,fn):
 p=a/('NakiPrototype.'+name+'.cs');s=p.read_text(encoding='utf-8-sig');t=fn(s)
 if t!=s:p.write_text(t,encoding='utf-8');changed.append(p)
edit('Woodland',lambda s:s.replace('n.id==16?3:1','n.id==16?3:0').replace('观察旧叶中的幼芽','检查旧叶下的活种根').replace('空果壳和幼芽','空果壳和活种根').replace('厚叶间，一枚幼芽仍朝光伸展。少年移开压住它的枯枝，把还活着的根记进林册。','厚叶下的种根仍有生机，却迟迟不能萌发。少年将它记进林册：菌丝夺走了养分，尚未到新芽破土的时候。').replace('case 201:if(chapterStone&&chapterLampGiven)','case 201:if(WoodCanEnd)').replace('case 206:EnterExplorationRoom(4,-1);','case 206:if(!WoodFinalBossCleared)return;EnterExplorationRoom(4,-1);').replace('case 207:QueueOriginMoment(4);','case 207:if(!WoodFinalBossCleared||currentRoom!=4){JourneyHint("先解除树冠旧庭中古树的寄生，再回旧叶种床栽种");return;}QueueOriginMoment(4);').replace('return chapterEnding?"新芽有声 · 木岛尾声完成"','return chapterEnding&&WoodCanEnd?"新芽有声 · 青栖林海尾声完成"'))
edit('WoodlandV2',lambda s:s.replace('幼芽旧床','休眠种根').replace('  private int WoodClues', '  private bool WoodFinalBossCleared {get{return woodlandMode&&journeyRooms.Count==45&&journeyRooms[31].cleared&&chapterStone;}}\n  private bool WoodCanEnd {get{return WoodFinalBossCleared&&chapterLampGiven;}}\n  private int WoodClues'))
edit('ForgeGuidance',lambda s:s.replace('幼芽旧床','休眠种根'))
edit('ThunderChapterSave',lambda s:s.replace('            fireHeartRecovered=s.fireHeart;', '            if(woodlandMode&&!WoodFinalBossCleared){chapterStone=chapterLampGiven=chapterEnding=woodEndingCgSeen=false;}\n            fireHeartRecovered=s.fireHeart;').replace('chapterCg=s.cgPending||','chapterCg=(!woodlandMode||WoodCanEnd)&&(s.cgPending||').replace('!iceEndingCgSeen);chapterSavePending=false;','!iceEndingCgSeen));chapterSavePending=false;'))
edit('ThunderStory',lambda s:s.replace('{if(woodlandMode)woodEndingCgSeen=true;', '{if(woodlandMode&&!WoodCanEnd){chapterCg=false;JourneyHint("先取回木石并回旧叶种床栽种，再向阿苓报平安");return;}if(woodlandMode)woodEndingCgSeen=true;'))
edit('WorldProps',lambda s:s.replace('DrawWorldSign(at,chapterFocus.transform);AddChapterAction(action,at+Vector2.down*.4f,label);', '''// Use the island's existing rooted arch, rather than a floating arrow board.
   var arch=Draw("Forest return hollow",ForestQuestSprite(2),at,Vector2.one,Color.white,DepthAt(at.y));arch.transform.SetParent(chapterFocus.transform,true);
   AddChapterAction(action,at+Vector2.down*.4f,label);'''))
for p in a.glob('*.cs'):
 s=p.read_text(encoding='utf-8-sig');t=s.replace('木岛','青栖林海')
 if t!=s:p.write_text(t,encoding='utf-8');changed.append(p)
(r/'Validation/forest-ending-files.txt').write_text('\n'.join(str(p.relative_to(r)) for p in set(changed)),encoding='utf-8');print('Updated',len(set(changed)),'files')
