from pathlib import Path
root=Path(__file__).resolve().parents[1]
def edit(file,old,new):
 p=root/'Assets/NakiPrototype'/file;s=p.read_text(encoding='utf-8-sig');assert old in s,(file,old[:100]);p.write_text(s.replace(old,new),encoding='utf-8')
edit('NakiPrototype.Elements.cs','ResetEquipment();ResetPowers();ResetBackpack();','ResetEquipment();ResetPowers();ResetBackpack();ResetChapterCooldowns();')
edit('NakiPrototype.Runtime.cs','rainQuayArt=null;','rainQuayArt=null;passageGates=stormFixtures=null;voyagerJournal=false;')
p=root/'Assets/NakiPrototype/NakiPrototype.Navigation.cs';s=p.read_text(encoding='utf-8');a=s.index('   float angle=direction==0?');b=s.index('\n  private Rect DoorSealCollision',a)
s=s[:a]+'   BuildIslandGate(direction);\n  }\n'+s[b:];s=s.replace('new Vector2(2.4f,2.3f):new Vector2(2.3f,2.4f)','new Vector2(2.8f,2.3f):new Vector2(2.3f,2.8f)')
s=s.replace('return NearestSafeFooting(entry>=0?DoorPoint(entry,-1.9f):new Vector2(0,-journeyRooms[currentRoom].halfHeight+1.6f));','return NearestSafeFooting(entry>=0?DoorPoint(entry,-1.9f):CoastalHarborRoom||RainQuayRoom?new Vector2(-1,journeyRooms[currentRoom].halfHeight*.28f):new Vector2(0,-journeyRooms[currentRoom].halfHeight+1.6f));');p.write_text(s,encoding='utf-8')
p=root/'Assets/NakiPrototype/NakiPrototype.Thunder.cs';s=p.read_text(encoding='utf-8');a=s.index('            foreach(float sign in new[]{-1f,1f})',s.index('private void BuildThunderGate'));b=s.index('\n        private void AddThunderRock',a)
s=s[:a]+'            BuildIslandGate(d);\n        }\n'+s[b:];s=s.replace('new Vector2(2.4f,2.3f):new Vector2(2.3f,2.4f)','new Vector2(2.8f,2.3f):new Vector2(2.3f,2.8f)');p.write_text(s,encoding='utf-8')
edit('NakiPrototype.Exploration.cs','RainQuayRoom&&direction==3?Vector2.up*.9f:Vector2.zero','RainQuayRoom?(direction>=2?Vector2.up*(n.halfHeight*.28f):direction==1?new Vector2(-.022f*n.halfWidth,n.halfHeight*.38f):Vector2.zero):Vector2.zero')
edit('NakiPrototype.Exploration.cs','side<=1.15f-radius','side<=1.4f-radius')
edit('NakiPrototype.Exploration.cs','foreach(Rect r in doorShotBlocks)projectileObstacles.Remove(r);doorShotBlocks.Clear();','foreach(Rect r in doorShotBlocks){projectileObstacles.Remove(r);obstacles.Remove(r);}doorShotBlocks.Clear();')
edit('NakiPrototype.Exploration.cs','doorShotBlocks.Add(block);projectileObstacles.Add(block);','doorShotBlocks.Add(block);projectileObstacles.Add(block);obstacles.Add(block);')
edit('NakiPrototype.Exploration.cs','Vector2.Dot(delta,n)>.75f&&Mathf.Abs(Vector2.Dot(delta,new Vector2(-n.y,n.x)))<=1.15f','Vector2.Dot(delta,n)>(CoastalHarborRoom||RainQuayRoom?.35f:.6f)&&Mathf.Abs(Vector2.Dot(delta,new Vector2(-n.y,n.x)))<=1.4f')
# Dynamic doorway strips must be the full usable width, rather than narrow normalized slivers.
p=root/'Assets/NakiPrototype/NakiPrototype.QuayNavigation.cs';s=p.read_text(encoding='utf-8');a=s.index('   if(HasDoor(0))');b=s.index('   var xs=',a)
s=s[:a]+'''   RoomNode node=journeyRooms[currentRoom];
   for(int d=0;d<4;d++)if(HasDoor(d)&&d!=1){
    Vector2 c=DoorPoint(d);float u=c.x/(node.halfWidth*2)+.5f,v=.5f-c.y/(node.halfHeight*2);
    float half=1.48f/(d<2?node.halfWidth*2:node.halfHeight*2);
    if(d==0)land.Add(new Rect(u-half,0,half*2,.25f));
    else land.Add(new Rect(d==3?0:.84f,v-half,.16f,half*2));
   }
''' +s[b:]
a=s.index('   for(int d=0;d<4;d++)if(HasDoor(d)){',s.index('private void BuildRainQuay'));b=s.index('   EnsureThunderDock();',a)
s=s[:a]+'''   BuildPaintedQuayApproaches(rainQuayArt.texture,true);
''' +s[b:]
insert='''  private void BuildPaintedQuayApproaches(Texture2D texture,bool rain)
  {
   // The paving joins the dry courtyard at the actual door height, including its outer corridor.
   Rect sample=rain?new Rect(.38f,.62f,.08f,.11f):new Rect(.44f,.65f,.10f,.10f);
   Sprite paving=Sprite.Create(texture,new Rect(texture.width*sample.x,texture.height*sample.y,texture.width*sample.width,texture.height*sample.height),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);ownedAssets.Add(paving);
   for(int d=0;d<4;d++)if(HasDoor(d)&&d!=1){
    Vector2 normal=DoorNormal(d),at=DoorPoint(d)-normal*.15f;
    var renderer=Draw("Continuous dry quay exit",paving,at,Vector2.one,Color.white,-995).GetComponent<SpriteRenderer>();
    renderer.drawMode=SpriteDrawMode.Tiled;renderer.size=d<2?new Vector2(2.85f,3.5f):new Vector2(4.1f,2.85f);
   }
  }
'''
s=s.replace('  private void BuildRainQuay(RoomNode room)',insert+'  private void BuildRainQuay(RoomNode room)');p.write_text(s,encoding='utf-8')
p=root/'Assets/NakiPrototype/NakiPrototype.ThunderStory.cs';s=p.read_text(encoding='utf-8');a=s.index('            for(int d=0;d<4;d++)if(HasDoor(d)){');b=s.index('\n        private void BuildHarbor',a)
s=s[:a]+'            BuildPaintedQuayApproaches(texture,false);\n        }\n'+s[b:]
a=s.index('                    GameObject post=Stretch("Wooden trail waymarker"');b=s.index('\n                }',a)
s=s[:a]+'''                    GameObject marker=StormFixture(0,points[i],1.25f,chapterFocus.transform);
                    if((chapterWaymarks&(1<<i))==0)marker.transform.rotation=Quaternion.Euler(0,0,12);
''' +s[b:]
s=s.replace('if(n.id==14||n.id==16)AddChapterAction(n.id==14?3:4,ThunderLeverAt,"复位引雷手柄");','if(n.id==14||n.id==16){AddChapterAction(n.id==14?3:4,ThunderLeverAt,"复位引雷手柄");StormFixture(1,ThunderLeverAt,1.45f,chapterFocus.transform);}')
s=s.replace('if(n.id==21)AddChapterAction(13,new Vector2(-2,1.3f),n.claimed?"避雨棚已修好":"扶正避雨棚梁",thunderRock);','if(n.id==21){AddChapterAction(13,new Vector2(-2,1.3f),n.claimed?"避雨棚已修好":"扶正避雨棚梁");StormFixture(2,new Vector2(-2,1.3f),2.0f,chapterFocus.transform);}')
s=s.replace('DrawWorldSign(new Vector2(-2.5f,-1.1f),chapterFocus.transform);','StormFixture(0,new Vector2(-2.5f,-1.1f),1.1f,chapterFocus.transform);')
s=s.replace('if(n.id==36)AddChapterAction(15,new Vector2(-2,1.3f),"远望山口的灯",lessonArtwork);','if(n.id==36){AddChapterAction(15,new Vector2(-2,1.3f),"翻阅巡路记录");StormFixture(3,new Vector2(-2,1.3f),1.45f,chapterFocus.transform);}')
p.write_text(s,encoding='utf-8')
# Diagnostic validation remains strict: the cause must be reset/rebase, not an ignored invalid timer.
edit('NakiPrototype.ThunderChapterSave.cs','throw new InvalidDataException("Invalid saved cooldown");','throw new InvalidDataException("Invalid saved cooldown: "+ready+" / clock "+p.clock);')
print('Applied gate art, full-width dry approaches, physical door blocking and cooldown reset')
