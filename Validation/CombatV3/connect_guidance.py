from pathlib import Path
import shutil
root=Path(__file__).resolve().parents[2];base=root/'Assets/NakiPrototype'
dest=base/'Resources/NakiArt/FireWorkshop/readability-v2.png'
shutil.copy2('C:/Users/15853/.codex/generated_images/01a1764d-2b16-7840-a400-e14f3ffe5ffd/exec-6b000eb7-4eb9-481d-8e13-958499dfbd33.png',dest)
def edit(name,a,b):
 p=base/name;s=p.read_text(encoding='utf-8-sig');assert a in s,(name,a);p.write_text(s.replace(a,b),encoding='utf-8')
edit('NakiPrototype.IceChapter.cs','var o=Stretch("Expedition camp gate",square,at,size,C("8e7b63"),DepthAt(at.y));o.transform.SetParent(chapterFocus.transform,true);Rect r=new Rect(at-size*.5f,size);iceCampBlocks.Add(r);obstacles.Add(r);','DrawCampRailing(at,size);')
edit('NakiPrototype.ThunderStory.cs','ClearIceCampBlocks();chapterActions.Clear();','ClearIceCampBlocks();ClearForgeFenceShots();chapterActions.Clear();')
edit('NakiPrototype.FireWorkshop.cs','var pit=Stretch("Ash chute beneath evacuation bridge",square,at,new Vector2(3.2f,.65f),C("18171a"),-981);','var pit=Stretch("Ash chute beneath evacuation bridge",ForgeFixture(2),at,new Vector2(3.5f,1.0f),Color.white,-981);')
edit('NakiPrototype.FireWorkshop.cs','if(!forgeBridgeOpen)IceCampWall(at,new Vector2(3.2f,.65f));','if(!forgeBridgeOpen)TrackCampCollision(at,new Vector2(3.2f,.65f),false);')
edit('NakiPrototype.FireCoast.cs','if(n.stage==1){BuildWorkshopActions(n);return;}','if(n.stage==1){BuildWorkshopActions(n);BuildWorkshopGuide(n);return;}')
edit('NakiPrototype.FireCoast.cs','!forgeBridgeOpen?"炉匠已获救 · 放下工坊撤离桥"','!forgeBridgeOpen?"回到撤离桥头 · 靠近左侧绞盘按 F 放桥（M 标出目标）"')
edit('NakiPrototype.FireWorkshop.cs','"阿铎：想让火停下来，先给它一道边界。让掌心的热意绕过你，再收回来。"','"阿铎：想让火停下来，先给它一道边界。让掌心的热意绕过你，再收回来。","阿铎指向炉道图：回到撤离桥头，在桥西侧的绞盘旁按 F。放下桥，通往巨匠的门才会打开。我已替你标在地图上。"')
edit('NakiPrototype.FireWorkshop.cs','"地心赤炉的压力门"','"地心压力门 · 后续区域尚未开放"')
edit('NakiPrototype.Equipment.cs','if(thunderChapterMode&&TryChapterInteraction())return true;','if(ExplainNearbyWorkshopGate())return true;if(thunderChapterMode&&TryChapterInteraction())return true;')
edit('NakiPrototype.Exploration.cs','if(ExplorationDoorLocked(currentRoom,direction))continue;','if(ExplorationDoorLocked(currentRoom,direction)){if(Vector2.Dot(delta,n)>-.8f&&Mathf.Abs(Vector2.Dot(delta,new Vector2(-n.y,n.x)))<1.2f){string reason=WorkshopGateReason(currentRoom,direction);if(reason!=null)JourneyHint(reason);}continue;}')
edit('NakiPrototype.JourneyHud.cs','fireChapterMode?"赤炉群岛 · 黑砂海岸"','fireChapterMode?"赤炉群岛 · "+(node.stage==1?"锻链工坊":"黑砂海岸")')
edit('NakiPrototype.JourneyHud.cs','string label=n.visited?', 'string label=n.id==WorkshopGoal?"目标 · "+(n.id==20?"阿铎":n.id==23?"撤离桥":n.id==27?"巨匠":"回港"):n.visited?')
edit('NakiPrototype.JourneyHud.cs','n.id==currentRoom?C("f2d49c"):C("6c8180")','n.id==currentRoom?C("f2d49c"):n.id==WorkshopGoal?C("ffe19d"):C("6c8180")')
edit('NakiPrototype.JourneyHud.cs','else if(n.visited&&n.stairsTo>=0&&n.cleared)CenterLabel', 'else if(n.id==WorkshopGoal)CenterLabel(box,"!",new GUIStyle(journeySmallStyle){fontSize=12});else if(n.visited&&n.stairsTo>=0&&n.cleared)CenterLabel')
edit('NakiPrototype.Runtime.cs','workshopArt.Clear();ClearFireHazards();','workshopArt.Clear();forgeFixtureSprites.Clear();ClearFireHazards();')
print('Connected readable fixtures, quest route and locked-door explanations')
