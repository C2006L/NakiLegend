from pathlib import Path
import re,shutil
root=Path(__file__).resolve().parents[1]
def edit(name,pairs):
 p=root/'Assets/NakiPrototype'/name;s=p.read_text(encoding='utf-8-sig')
 for a,b in pairs:
  assert a in s,(name,a[:70]);s=s.replace(a,b,1)
 p.write_text(s,encoding='utf-8')
p=root/'Assets/NakiPrototype/NakiPrototype.ThunderStory.cs';s=p.read_text(encoding='utf-8-sig')
a=s.index('            Rect water=new Rect(');b=s.index('            // Actual navigation',a)
s=s[:a]+'            BuildQuayFooting();\n'+s[b:]
# Use a crop of the actual quay paving for exit approaches; never overwrite the southern wooden pier.
a=s.index('                if(fireChapterMode){Sprite paving=');b=s.index('\n            }',a)
s=s[:a]+'''                if(d==1)continue;
                Sprite paving=Sprite.Create(texture,new Rect(texture.width*.42f,texture.height*.64f,texture.width*.12f,texture.height*.10f),Vector2.one*.5f,texture.width/(room.halfWidth*2),0,SpriteMeshType.FullRect);ownedAssets.Add(paving);
                var approach=Draw("Quay paving exit approach",paving,DoorPoint(d,-.1f),Vector2.one,Color.white,-999).GetComponent<SpriteRenderer>();approach.drawMode=SpriteDrawMode.Tiled;approach.size=d<2?new Vector2(2.3f,2.1f):new Vector2(2.1f,2.3f);'''+s[b:]
p.write_text(s,encoding='utf-8')
edit('NakiPrototype.Exploration.cs',[
 ('+(fireChapterMode&&currentRoom==0&&direction==2?Vector2.up*1.5f:Vector2.zero);', '+(CoastalHarborRoom?(direction>=2?Vector2.up*(n.halfHeight*.32f):direction==1?new Vector2(((fireChapterMode?.48f:.629f)-.5f)*n.halfWidth*2,n.halfHeight*.95f):Vector2.zero):RainQuayRoom&&direction==3?Vector2.up*.9f:Vector2.zero);'),
 ('doorSeals.Clear();doorSealDirections.Clear();doorShotBlocks.Clear();\n            TileFloor', 'doorSeals.Clear();doorSealDirections.Clear();doorShotBlocks.Clear();\n            if(RainQuayRoom){BuildRainQuay(node);return;}\n            TileFloor'),
])
edit('NakiPrototype.ThunderChapterSave.cs',[
 ('private static readonly Vector2 ThunderDockAt=new Vector2(-4.8f,-1.5f);','private static readonly Vector2 ThunderDockAt=new Vector2(.8f,.8f);'),
 ('GameObject bell=Draw("Boat at the lookout",WorldPropSprite(5),ThunderDockAt,Vector2.one,Color.white,DepthAt(ThunderDockAt.y));bell.transform.SetParent(dock.transform,true);','dock.transform.position=ThunderDockAt; // Bell, boardwalk and floating boat are authored into the quay image.'),
])
edit('NakiPrototype.CanalEpilogue.cs',[
 ('甘泽归沙 · 望台西侧风铃 F 乘船前往雷雨高原','甘泽归沙 · 栈桥岸边 F 展开全域航图，自选目的地'),
])
edit('NakiPrototype.Equipment.cs',[
 ('"F 乘船前往第二岛 · 雷雨高原"','"F 展开全域航图 · 推荐雷雨高原"'),
]) if '"F 乘船前往第二岛 · 雷雨高原"' in (root/'Assets/NakiPrototype/NakiPrototype.Equipment.cs').read_text(encoding='utf-8-sig') else None
edit('NakiPrototype.cs',[
 ('if(enableDesertJourney){UpdateTacticalEnemy(enemy,dt);return;}','if(enableDesertJourney){bool previousFlip=enemy.body!=null&&enemy.body.flipX;UpdateTacticalEnemy(enemy,dt);StabilizeEnemyFacing(enemy,previousFlip);return;}'),
])
edit('NakiPrototype.EnemyMovement.cs',[
 ('if(!Blocked(at,radius))return at;','if(IceNavigationPoint(at,radius))return at;'),
 ('if(d<score&&!Blocked(p,radius))','if(d<score&&IceNavigationPoint(p,radius))'),
 ('if(!Blocked(p,radius))at.x=p.x;','if(IceNavigationLine(at,p,radius))at.x=p.x;'),
 ('if(!Blocked(p,radius))at.y=p.y;','if(IceNavigationLine(at,p,radius))at.y=p.y;'),
])
# Every authored direct charge uses one swept, non-sliding movement path.
count=0
for p in (root/'Assets/NakiPrototype').glob('NakiPrototype*.cs'):
 if p.name=='NakiPrototype.EnemyMotion.cs':continue
 s=p.read_text(encoding='utf-8-sig');old=s
 # Capture a balanced call rather than applying a regex across nested C# arguments.
 needle='MoveEnemyWithinRoom('
 start=0
 while True:
  at=s.find(needle,start)
  if at<0:break
  i=at+len(needle);depth=1
  while depth:
   if s[i]=='(':depth+=1
   elif s[i]==')':depth-=1
   i+=1
  call=s[at:i]
  m=re.match(r'MoveEnemyWithinRoom\((e|enemy)\.position,\s*\1\.attackDirection\s*\*',call)
  if m:
   var=m.group(1);suffix=','+(' ' if ', '+var+'.radius)' in call else '')+var+'.radius)'
   assert call.endswith(suffix),call
   delta=call[call.index(',')+1:-len(suffix)].strip();replacement='MoveEnemyCharge('+var+','+delta+')'
   s=s[:at]+replacement+s[i:];start=at+len(replacement);count+=1
  else:start=i
 if s!=old:p.write_text(s,encoding='utf-8')
assert count==10,count
edit('NakiPrototype.Runtime.cs',[
 ('forestQuestArt.Clear();worldAtlasMenu','forestQuestArt.Clear();rainQuayArt=null;worldAtlasMenu'),
])
asset=root/'Assets/NakiPrototype/Resources/NakiArt/QuayPolish';asset.mkdir(parents=True,exist_ok=True)
shutil.copy2('C:/Users/15853/.codex/generated_images/01a1764d-2b16-7840-a400-e14f3ffe5ffd/exec-9cc52a3e-140f-4969-af95-eac600e814d3.png',asset/'rain-quay.png')
print('Harbor geometry, lookout art and',count,'charge paths updated')
