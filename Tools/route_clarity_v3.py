from pathlib import Path
b=Path(__file__).resolve().parents[1]/'Assets/NakiPrototype'
def edit(name,a,z):
 p=b/f'NakiPrototype.{name}.cs';s=p.read_text(encoding='utf-8');assert s.count(a)==1,(name,a,s.count(a));p.write_text(s.replace(a,z),encoding='utf-8')
edit('IceChapter','"冰港与覆雪货仓","裂潮冰峡","极夜晶庭"','"冰港沿岸","裂潮冰峡","寒潮晶庭"')
edit('IceChapter','journeyRooms[28].stairsTo=26;','journeyRooms[28].stairsTo=26;RebuildIceRoute();')
edit('IceRouteV2','n.waves=n.stage==0?3:4;','n.waves=n.stage<2?3:4;')
edit('IceSample','"ice-ch3-v1":"ice-s2-v1"','"ice-ch3-v2":"ice-s2-v1"')
edit('IceChapter','private void MigrateIceChapter(ChapterCheckpoint s){','private void MigrateIceChapter(ChapterCheckpoint s){if(s!=null&&s.island=="ice_sample"&&s.map=="ice-ch3-v1"&&s.rooms!=null&&s.rooms.Length==42){s.map="ice-ch3-v2";return;}')
edit('IceChapter','s.map="ice-ch3-v1";','s.map="ice-ch3-v2";')
edit('IceChapter','int count=Mathf.Min(6,2+n.stage+(currentWave>1?1:0));','int count=Mathf.Min(7,4+n.stage+(currentWave>=3?1:0));')
edit('IceChapter','if(range&&ranged++>=2)','if(range&&ranged++>=(n.stage==0?2:3))')
start='  private void IceChapterRest(RoomNode n,bool first)';p=b/'NakiPrototype.IceChapter.cs';s=p.read_text(encoding='utf-8');a=s.index(start);e=s.index('\n',a);s=s[:a]+'  private void IceChapterRest(RoomNode n,bool first){if(n.kind==RoomKind.Reward&&!n.lootSeeded)IceChapterRoomCleared(n);}' +s[e:];p.write_text(s,encoding='utf-8')
edit('IceChapter','   if(n.id==26&&!chapterRanger)DrawIceCampSign(n);','   // The objective chevron already guides rescue; no redundant wooden arrow board.')
edit('IceChapter','   if(n.kind==RoomKind.Rest&&n.id!=0&&n.id!=41)AddChapterAction(315,new Vector2(-2,1),"整备与补给",IceSprite(3));','   BuildIceQuietRoom(n);')
edit('IceChapter','   if(a==315){RoomNode n=journeyRooms[currentRoom];if(!n.claimed){n.claimed=true;health=Mathf.Min(6,health+2);elementPower=Mathf.Min(ElementMaximum,elementPower+35);}JourneyHint("安全营地 · 已保存；T 配置技能");}','   if(a==315){RoomNode n=journeyRooms[currentRoom];if(!IceRestRoom(n))return;if(!n.claimed){n.claimed=true;health=Mathf.Min(6,health+2);shield=Mathf.Min(EffectiveMaximumShield,shield+2);elementPower=Mathf.Min(ElementMaximum,elementPower+25);JourneyHint("营地整备 · 生命+2、护盾+2、元素+25");}else JourneyHint("本处物资已领取 · 清房后会自动保存");}if(a==316)ReadIceRouteRecord();')
edit('Navigation','(iceSampleMode&&iceFullChapter?IceStages:woodlandMode?WoodlandStages:ThunderStages)[journeyRooms[n.stairsTo].stage]','ActiveLayerName(journeyRooms[n.stairsTo].stage)')
edit('JourneyHud','private string RoomHeading(RoomNode node){return thunderChapterMode?','private string RoomHeading(RoomNode node){return thunderChapterMode?ActiveLayerName(node.stage):') if False else None
p=b/'NakiPrototype.JourneyHud.cs';s=p.read_text(encoding='utf-8');a=s.index('  private string RoomHeading(');e=s.index('\n',a);s=s[:a]+'  private string RoomHeading(RoomNode node){return thunderChapterMode?ActiveLayerName(node.stage):thunderPrototypeMode?ThunderTitle:IsGatehouseRoom(node)?"鸣沙洲 · 尾声 · 归渠水道":"鸣沙洲 · 第 "+(node.stage+1)+" 层 · "+StageNames[node.stage];}'+s[e:];p.write_text(s,encoding='utf-8')
p=b/'NakiPrototype.ThunderChapter.cs';s=p.read_text(encoding='utf-8');a=s.index('        private string ChapterTitle');e=s.index('\n',a);s=s[:a]+'        private string ChapterTitle { get { return ActiveLayerName(journeyRooms[currentRoom].stage); } }'+s[e:];p.write_text(s,encoding='utf-8')
edit('WorldAtlasHud','AtlasNames[here]+" · 第 "+(stage+1)+" 层 · "+(thunderChapterMode?chapterNames[currentRoom]:JourneyRoomTitle(journeyRooms[currentRoom]))','LayerName(here,stage)+" · "+(thunderChapterMode?chapterNames[currentRoom]:JourneyRoomTitle(journeyRooms[currentRoom]))')
# Replace the floating freestanding shelter door and exposed survivor with a wall-side refuge.
p=b/'NakiPrototype.FireCoast.cs';s=p.read_text(encoding='utf-8');a=s.index('if(n.id==4){AddChapterAction');e=s.index('\n',a);s=s[:a]+'if(n.id==4)BuildCoastShelter(n);'+s[e:];s=s.replace(',FireProp(3));\n  }',');\n  }');s=s.replace('if(a==401){if(chapterRanger)','if(a==401){if(currentRoom!=4||!journeyRooms[4].cleared){JourneyHint("先清理避难所外的敌人");return;}if(chapterRanger)');p.write_text(s,encoding='utf-8')
edit('FireCoast','private void ReserveFireProps(RoomNode n){ReserveRefuge(n);','private void ReserveFireProps(RoomNode n){ReserveRefuge(n);if(n.id==4){Vector2 at=new Vector2(-3,n.halfHeight-2);transitReservations.Add(new Rect(at-new Vector2(2.2f,1),new Vector2(4.4f,3.6f)));}')
# Stage/room names in dialogue and objective prompts now share the atlas vocabulary.
for p in b.glob('*.cs'):
 s=p.read_text(encoding='utf-8');old=s
 for a,z in [('冰港与覆雪货仓','冰港沿岸'),('极夜晶庭','寒潮晶庭'),('极夜阶梯','晶庭石阶')]:s=s.replace(a,z)
 if s!=old:p.write_text(s,encoding='utf-8')
print('Rebuilt 42-room ice routing, three camps, three caches, consistent layer labels and wall-side Fire rescue')
