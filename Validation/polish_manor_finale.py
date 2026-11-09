from pathlib import Path
import shutil
r=Path('D:/NakiLegend/Repository');a=r/'Assets/NakiPrototype';shutil.copy2('C:/Users/15853/.codex/generated_images/01a1764d-2b16-7840-a400-e14f3ffe5ffd/exec-c525bfb5-feec-4f3c-8cea-70aca2041a4b.png',a/'Resources/NakiArt/ManorFinaleV3/endings.png')
p=a/'NakiPrototype.ManorCourtyard.cs';s=p.read_text(encoding='utf-8-sig').replace('cast=i==0?"hammer":"sword",hit=i==0?"heavy":"hit"','cast=i==0?"weapon_011_cast":"weapon_000_cast",hit=i==0?"weapon_011_hit":"weapon_000_hit"').replace('   if(IsCombatRoom(n)){','   BuildManorLaterScenery(n);\n   if(IsCombatRoom(n)){');p.write_text(s,encoding='utf-8')
p=a/'NakiPrototype.ManorFinale.cs';s=p.read_text(encoding='utf-8-sig').replace('   private','   private')
pos=s.index('  private void BuildManorLaterActions(')
s=s[:pos]+'''  private void BuildManorLaterScenery(RoomNode n)
  {
   if(n.stage!=1||n.kind==RoomKind.Combat||n.kind==RoomKind.Elite||n.kind==RoomKind.Boss)return;
   var t=Resources.Load<Texture2D>("NakiArt/LegacyV1/refuge");if(t==null)return;
   Vector2 at=new Vector2(-5,3);var sprite=Sprite.Create(t,new Rect(0,0,t.width,t.height),new Vector2(.5f,.14f),t.width/3.4f,0,SpriteMeshType.FullRect);ownedAssets.Add(sprite);Draw("Sheltered village house",sprite,at,Vector2.one,C("b1b3b7"),DepthAt(at.y));
   Rect footprint=new Rect(-6.45f,3,2.9f,1.5f);obstacles.Add(footprint);projectileObstacles.Add(footprint);
  }
'''+s[pos:]
s=s.replace('if(n.id==43)AddChapterAction','if(n.id==23||n.id==38||n.id==39)AddChapterAction(620,new Vector2(-2,0),"查看这里留下的生活痕迹",ManorSprite("prop-7",1.4f));\n    if(n.id==28)AddChapterAction(619,new Vector2(-2,0),n.claimed?"查看工坊行路记录":"工坊整备 · 本处补给一次",ManorSprite("prop-5",1.8f));\n    if(n.id==43)AddChapterAction')
s=s.replace('else if(action>=720&&action<=722)', '''else if(action==619){var n=journeyRooms[currentRoom];if(!n.claimed){n.claimed=true;health=Mathf.Min(6,health+1.5f);shield=Mathf.Min(EffectiveMaximumShield,shield+2);elementPower=Mathf.Min(ElementMaximum,elementPower+30);}TellStory("工坊的炉子终于停下来。把工具留给仍在修补房屋的人，少年沿着掌印前室继续向里。 ");}
   else if(action==620)TellStory(currentRoom==23?"门后的饭碗洗得很干净。墙上还留着一行旧字：‘受封的少年说过，会把粮道打开。’":currentRoom==38?"花园里留下五种来自不同岛屿的旧花盆。没有一只花盆被当作淬炼容器。": "断链旁有一道被反复划过的刻痕：‘如果回来的人已经不是我，别让他替你作决定。’");
   else if(action>=720&&action<=722)''')
s=s.replace('储存的战斗余烬无法替代完整意志。','铠甲已破，契约却仍能重新驱动它。储存的战斗余烬无法替代完整意志。');p.write_text(s,encoding='utf-8')
p=a/'NakiPrototype.Thunder.cs';s=p.read_text(encoding='utf-8-sig').replace('sr.size=size;sr.color=fireChapterMode','sr.size=size;sr.color=manorCourtyardMode?(journeyRooms[currentRoom].stage==1?new Color(.82f,.87f,.8f):journeyRooms[currentRoom].stage==2?new Color(.76f,.72f,.9f):new Color(.9f,.94f,1)):fireChapterMode');p.write_text(s,encoding='utf-8')
f=r/'Validation/manor-finale-files.txt';f.write_text(f.read_text(encoding='utf-8')+'\nAssets/NakiPrototype/NakiPrototype.Thunder.cs',encoding='utf-8')
