from pathlib import Path
root=Path(__file__).resolve().parents[2]
p=root/'Assets/NakiPrototype/NakiPrototype.ThunderChapterSave.cs';s=p.read_text(encoding='utf-8-sig')
a='chapterNames[currentRoom]+(fireChapterMode?'
b='chapterNames[currentRoom]+(manorCourtyardMode?(chapterStone?" · 卫长已卸武":" · 外庭探索中"):fireChapterMode?'
if b not in s:
 assert a in s;s=s.replace(a,b,1);p.write_text(s,encoding='utf-8-sig')
print('Manor checkpoint status now uses its own progression names')
