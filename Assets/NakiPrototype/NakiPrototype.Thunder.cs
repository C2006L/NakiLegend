using System.Collections.Generic;
using UnityEngine;

namespace NakiLegend.Prototype
{
    public sealed partial class NakiPrototype
    {
        public enum ThunderPreset { Light, Dark, BasicWeapons }
        [Header("Thunder Plateau two-room prototype")]
        public bool thunderPrototypeMode;
        public ThunderPreset thunderPreset;
        [Range(0,1)] public int thunderStartRoom;
        private bool thunderSetupMenu, thunderRepaired, thunderRewardCreated;
        private float thunderWorkRemaining, thunderNextStrike, thunderNextWindup;
        private Sprite thunderFloor, thunderRock, thunderNode, thunderWall, thunderGrass, thunderChannel;
        private GameObject thunderNodeVisual, thunderLamp, thunderLever;
        private readonly List<GameObject> thunderRain = new List<GameObject>();
        private readonly List<GameObject> thunderFlow = new List<GameObject>();
        private static readonly Vector2 ThunderNodeAt = new Vector2(5.6f,2.3f);
        private static readonly Vector2 ThunderLeverAt = new Vector2(4.5f,2.1f);
        private string ThunderTitle { get { return currentRoom==0?"P01 · 雨中草坡":"P02 · 西引雷坪"; } }

        private Sprite ThunderSprite(string name,float ppu,Vector2 pivot)
        {
            Texture2D t=Resources.Load<Texture2D>("NakiArt/ThunderS1/"+name);if(t==null)return null;
            t.filterMode=FilterMode.Point;t.wrapMode=TextureWrapMode.Clamp;
            Sprite s=Sprite.Create(t,new Rect(0,0,t.width,t.height),pivot,ppu,0,SpriteMeshType.FullRect);ownedAssets.Add(s);return s;
        }
        private void LoadThunderArt()
        {
            if(!ThunderActive)return;
            thunderFloor=ThunderSprite("floor",256,Vector2.one*.5f);
            thunderRock=ThunderSprite("rock",720,new Vector2(.5f,.17f));
            thunderNode=ThunderSprite("node",550,new Vector2(.5f,.17f));
            thunderWall=ThunderRegion("wall",new Rect(0,192,2172,304),405,Vector2.one*.5f);
            thunderGrass=ThunderRegion("grass",new Rect(355,282,864,433),900,new Vector2(.5f,.05f));
            thunderChannel=ThunderRegion("channel",new Rect(355,0,177,1774),280,Vector2.one*.5f);
            LoadThunderEnemyArt();LoadThunderChapterArt();
        }
        private Sprite ThunderRegion(string name,Rect rect,float ppu,Vector2 pivot)
        {
            Texture2D t=Resources.Load<Texture2D>("NakiArt/ThunderS1/"+name);if(t==null)return null;t.filterMode=FilterMode.Point;
            Sprite s=Sprite.Create(t,rect,pivot,ppu,0,SpriteMeshType.FullRect);ownedAssets.Add(s);return s;
        }
        private void BuildThunderGraph()
        {
            journeyRooms.Clear();gatehouseSampleMode=bossPracticeMode=lightningTrialMode=false;
            thunderRepaired=thunderRewardCreated=false;thunderWorkRemaining=0;thunderSavePending=false;
            for(int i=0;i<2;i++)journeyRooms.Add(new RoomNode{id=i,stage=0,cell=new Vector2Int(2,2+i),kind=RoomKind.Combat,
                halfWidth=9,halfHeight=i==0?5:5.5f,waves=i==0?3:2,layout=i,depth=0});
            ConnectRooms(0,1);for(int i=0;i<3;i++){stageStarts[i]=0;stageEnds[i]=1;}
        }
        private void ConfigureThunderPlayer()
        {
            origin=thunderPreset==ThunderPreset.Dark?Origin.Dark:Origin.Light;
            energy=origin==Origin.Light?100:0;darkPower=origin==Origin.Dark?6:0;
            orbLearned=true;bladeLearned=waveLearned=windEmpowered=thunderPreset!=ThunderPreset.BasicWeapons;
            if(thunderPreset==ThunderPreset.BasicWeapons){orbLearned=false;energy=elementPower=darkPower=0;}
            carriedWeapons[1]=thunderPreset==ThunderPreset.BasicWeapons?-1:6;
        }
        private void BeginThunderSample(ThunderPreset preset,int room)
        {
            thunderChapterMode=false;thunderPreset=preset;thunderStartRoom=Mathf.Clamp(room,0,1);thunderSetupMenu=false;BeginJourney();
        }
        private void ThunderTile(string name,Vector2 at,Vector2 size,int order)
        {
            Sprite floor=thunderChapterMode?(chapterFloors[journeyRooms[currentRoom].stage]??thunderFloor):thunderFloor;
            GameObject o=Draw(name,floor??terrainV12[1],at,Vector2.one,Color.white,order);
            var sr=o.GetComponent<SpriteRenderer>();sr.drawMode=SpriteDrawMode.Tiled;sr.size=size;sr.color=currentRoom==0?new Color(.87f,.96f,.9f):new Color(.9f,.94f,1);
        }
        private void BuildThunderRoom(RoomNode node)
        {
            if(thunderChapterMode)thunderRepaired=currentRoom==14?chapterWest:currentRoom==16&&chapterEast;
            ReserveTransitSpace(node);ReserveChapterProps(node);doorSeals.Clear();doorSealDirections.Clear();doorShotBlocks.Clear();
            thunderRain.Clear();thunderFlow.Clear();thunderNodeVisual=thunderLamp=thunderLever=null;
            thunderStrikes.Clear();thunderWorkRemaining=0;thunderNextStrike=clock+2.4f;thunderNextWindup=clock+1.5f;
            ThunderTile("Wet highland paving",Vector2.zero,new Vector2(node.halfWidth*2,node.halfHeight*2),-1000);
            for(int d=0;d<4;d++)
            {
                bool door=HasDoor(d);float span=d<2?node.halfWidth:node.halfHeight;
                if(door)BuildThunderGate(d);
                for(float x=-span;x<span-.01f;x+=1.6f)
                {
                    float end=Mathf.Min(span,x+1.6f);
                    if(door&&x<1.3f&&end>-1.3f){if(x<-1.3f)ThunderBoundary(d,x,-1.3f);if(end>1.3f)ThunderBoundary(d,1.3f,end);}
                    else ThunderBoundary(d,x,end);
                }
            }
            if(thunderChapterMode)BuildChapterCover(node);else{AddThunderRock(new Vector2(-3.7f,-.3f));AddThunderRock(new Vector2(3.3f,-1.3f));}
            if(HasThunderConductor)
            {
                thunderNodeVisual=Draw("Copper lightning conductor",thunderNode??lessonArtwork,ThunderNodeAt,Vector2.one,Color.white,DepthAt(ThunderNodeAt.y));
                obstacles.Add(new Rect(ThunderNodeAt+new Vector2(-.56f,-.13f),new Vector2(1.12f,.32f)));projectileObstacles.Add(new Rect(ThunderNodeAt+new Vector2(-.7f,-.08f),new Vector2(1.4f,1.7f)));
                // The generated conductor already includes a real copper lever; the
                // interaction sits beside it rather than drawing a second floating stick.
                thunderLamp=Draw("Repaired trail lamp",disc,new Vector2(6.3f,2.6f),Vector2.one*.14f,C("5d7b83"),360);
                Stretch("Inlaid copper lightning conduit",thunderChannel,new Vector2(6.8f,-.5f),new Vector2(.65f,6.9f),Color.white,-985);
                for(int i=0;i<11;i++)thunderFlow.Add(Stretch("Rerouted current",square,new Vector2(6.8f,-3.7f+i*.65f),new Vector2(.045f,.16f),C("7cd9e1"),-980));
                RefreshThunderRepair();
            }
            for(int i=0;i<26;i++)
            {
                float x=i<13?-node.halfWidth+.7f:node.halfWidth-.7f;
                Vector2 at=new Vector2(x,-node.halfHeight+.6f+(i%13)*(node.halfHeight*2-1.2f)/12);
                if(!IntersectsTransit(at,new Vector2(.8f,.5f)))Draw("Highland grass and flowers",thunderGrass,at,Vector2.one*(node.id==0?1:.7f),Color.white,DepthAt(at.y));
            }
            foreach(Vector2 at in new[]{new Vector2(-4.6f,.25f),new Vector2(4.2f,-.8f),new Vector2(-6.5f,3.5f)})Draw("Grass beside low stones",thunderGrass,at,Vector2.one*.7f,Color.white,DepthAt(at.y));
            if(thunderChapterMode)BuildChapterActions();
            for(int i=0;i<32;i++)
            {
                GameObject drop=Stretch("Highland rain",square,Vector2.zero,new Vector2(.012f,.15f),new Color(.65f,.82f,.88f,.17f),-610);
                drop.transform.rotation=Quaternion.Euler(0,0,-13);thunderRain.Add(drop);
            }
        }
        private void ThunderBoundary(int d,float begin,float end)
        {
            if(end-begin<.02f)return;Vector2 normal=DoorNormal(d),tangent=new Vector2(normal.y,-normal.x);
            Vector2 at=DoorPoint(d,.35f)+tangent*((begin+end)*.5f);bool horizontal=d<2;
            GameObject o=Draw("Mossy highland wall",thunderWall??thunderFloor,at,Vector2.one,new Color(.82f,.9f,.9f),DepthAt(at.y));
            SpriteRenderer sr=o.GetComponent<SpriteRenderer>();sr.drawMode=SpriteDrawMode.Tiled;sr.size=new Vector2(end-begin,.72f);
            if(!horizontal)o.transform.rotation=Quaternion.Euler(0,0,90);
            projectileObstacles.Add(new Rect(at-(horizontal?new Vector2((end-begin)/2,.36f):new Vector2(.36f,(end-begin)/2)),horizontal?new Vector2(end-begin,.72f):new Vector2(.72f,end-begin)));
        }
        private void BuildThunderGate(int d)
        {
            Vector2 c=DoorPoint(d),n=DoorNormal(d),t=new Vector2(-n.y,n.x);bool horizontal=d<2;
            ThunderTile("Highland doorway path",c+n*.9f,horizontal?new Vector2(2.4f,2.3f):new Vector2(2.3f,2.4f),-994);
            foreach(float sign in new[]{-1f,1f})
            {
                Vector2 at=c+t*sign*1.55f;ThunderTile("Door stone footing",at,Vector2.one*.65f,DepthAt(at.y));
                Rect b=new Rect(at-Vector2.one*.32f,Vector2.one*.64f);obstacles.Add(b);projectileObstacles.Add(b);
                Stretch("Copper doorway cap",square,at+Vector2.up*.25f,new Vector2(.62f,.08f),C("bc945f"),DepthAt(at.y)+2);
            }
            GameObject gate=new GameObject("Highland copper portcullis");gate.transform.SetParent(world,false);
            for(int i=-4;i<=4;i++)
            {
                Vector2 at=c+t*i*.23f;GameObject bar=Stretch("Copper gate bar",square,at,horizontal?new Vector2(.065f,.8f):new Vector2(.8f,.065f),C("a68154"),610);bar.transform.SetParent(gate.transform,true);
            }
            GameObject cross=Stretch("Gate brace",square,c,horizontal?new Vector2(2.15f,.11f):new Vector2(.11f,2.15f),C("c2a370"),612);cross.transform.SetParent(gate.transform,true);
            doorSeals.Add(gate);doorSealDirections.Add(d);
        }
        private void AddThunderRock(Vector2 at)
        {
            if(woodlandMode){DecorateWoodCover(at);return;}
            Draw("Mossy low stone cover",thunderRock??journeyRock,at,Vector2.one,Color.white,DepthAt(at.y));
            obstacles.Add(new Rect(at.x-.78f,at.y-.10f,1.56f,.28f));projectileObstacles.Add(new Rect(at.x-.78f,at.y-.08f,1.56f,.84f));
        }
        private void SpawnThunderWave(RoomNode node)
        {
            EnemyKind m=EnemyKind.ChargeMoth,g=EnemyKind.StormGuard;
            EnemyKind[] types=node.id==0?(currentWave==1?new[]{m,m}:currentWave==2?new[]{g,g}:new[]{m,m,g,g}):new[]{m,g,g};
            var random=new System.Random(activeSeed+node.id*101+currentWave*71);
            for(int i=0;i<types.Length;i++)
            {
                QueueEnemy(types[i],FindJourneySpawn(random,.35f));
                pendingSpawns[pendingSpawns.Count-1].when=clock+(currentWave==1?1.5f:1.0f)+i*.25f;
            }
        }
        private void ClearThunderHazards()
        {foreach(ThunderStrike s in thunderStrikes)if(s.root!=null)Destroy(s.root);thunderStrikes.Clear();}
        private void TickThunderEnvironment(float dt)
        {
            if(!ThunderActive)return;
            for(int i=0;i<thunderRain.Count;i++)if(thunderRain[i]!=null)thunderRain[i].transform.position=new Vector2(-8.8f+Mathf.Repeat(i*2.731f+clock*.25f,17.6f),5.2f-Mathf.Repeat(i*1.137f+clock*3,10.4f));
            if(HasThunderConductor&&!thunderRepaired&&!journeyRooms[currentRoom].cleared&&clock>=thunderNextStrike&&thunderStrikes.Count==0)
            {
                Vector2 at=NearestEnemyFooting(position+Vector2.up*.65f,.85f);
                if(!IntersectsTransit(at,Vector2.one*1.7f)&&!ProjectilePathBlocked(at,at,.85f))QueueThunderStrike(at);
                thunderNextStrike=clock+4.5f;
            }
            TickThunderStrikes();TickChapterSkills();
            if(thunderChapterMode&&chapterSavePending&&SafeChapterCheckpoint){chapterSavePending=false;SaveChapterCheckpoint(false);}
            if(thunderSavePending&&SafeThunderCheckpoint){thunderSavePending=false;SaveThunderCheckpoint(false);}
        }
        private bool TryThunderInteraction()
        {
            if(currentRoom!=1||Vector2.Distance(position,ThunderLeverAt)>1.35f)return false;
            if(!journeyRooms[1].cleared){JourneyHint("先清理引雷坪，再复位检修手柄");return true;}
            if(thunderRepaired){JourneyHint("雷流已改道 · 原型完成，可沿南门回访草坡");return true;}
            thunderWorkRemaining=1.2f;return true;
        }
        private bool TickThunderOverlay(float dt)
        {
            if(!thunderPrototypeMode)return false;
            if(thunderSetupMenu)
            {
                if(Pressed('1'))thunderPreset=ThunderPreset.Light;if(Pressed('2'))thunderPreset=ThunderPreset.Dark;if(Pressed('3'))thunderPreset=ThunderPreset.BasicWeapons;
                if(Pressed('a')||Pressed('d'))thunderStartRoom=1-thunderStartRoom;
                if(Pressed('f'))BeginThunderSample(thunderPreset,thunderStartRoom);return true;
            }
            if(thunderWorkRemaining<=0)return false;
            if(Pressed('e')){CancelThunderRepair();return true;}
            thunderWorkRemaining=Mathf.Max(0,thunderWorkRemaining-Mathf.Min(dt,.05f));
            if(thunderLever!=null)thunderLever.transform.rotation=Quaternion.Euler(0,0,(1-thunderWorkRemaining/1.2f)*-55);
            if(thunderWorkRemaining<=0)
            {
                thunderRepaired=true;ClearThunderHazards();
                if(!thunderRewardCreated)
                {
                    thunderRewardCreated=true;var drop=new StoredEssence{at=FindFreeInteractionPoint(ThunderLeverAt+Vector2.down),value=20,born=clock};
                    journeyRooms[1].essence.Add(drop);DrawEssence(drop);
                }
                RefreshThunderRepair();SaveThunderCheckpoint(false);TellStory("少年：这次，雷流沿着边槽走了。中间的山路可以通行。","两房原型完成：可沿南门回到草坡检查回访，F5 保存原型，F9 读取。N 打开测试配置，R 重试当前配置。正式主旅程存档保持独立。");
            }
            return true;
        }
        private void CancelThunderRepair(){thunderWorkRemaining=0;RefreshThunderRepair();JourneyHint("检修已取消，未消耗资源");}
        private void RefreshThunderRepair()
        {
            foreach(GameObject o in thunderFlow)if(o!=null)o.SetActive(thunderRepaired);
            if(thunderLamp!=null)thunderLamp.GetComponent<SpriteRenderer>().color=thunderRepaired?C("f5d28d"):C("5d7b83");
            if(thunderNodeVisual!=null)thunderNodeVisual.GetComponent<SpriteRenderer>().color=thunderRepaired?new Color(.8f,.95f,.9f):Color.white;
            if(thunderLever!=null)thunderLever.transform.rotation=Quaternion.Euler(0,0,thunderRepaired?-55:0);
        }
        private string ThunderObjective()
        {
            if(thunderWorkRemaining>0)return "检修手柄 · ESC 取消";
            if(!journeyRooms[currentRoom].cleared)return "第 "+currentWave+" / "+journeyRooms[currentRoom].waves+" 波 · 普通雷弹可用风波消除，地雷请走开";
            return currentRoom==0?"草坡已清理 · 沿北门前往引雷坪":thunderRepaired?"雷流已改道 · 沿南门回访，或 F5 保存":"清理完成 · 靠近东北侧铜手柄按 F 复位";
        }
        private string ThunderPrompt()
        {
            if(currentRoom==1&&Vector2.Distance(position,ThunderLeverAt)<1.5f)return !journeyRooms[1].cleared?"先清理敌人，再检修":thunderRepaired?"引雷设施已复位":"F 免费复位手柄 · 1.2 秒";
            return "";
        }
        private void DrawThunderSetup(float w,float h)
        {
            HudRect(new Rect(0,0,w,h),new Color(.02f,.04f,.06f,.9f));
            CenterLabel(new Rect(w/2-420,h/2-212,840,40),"雷雨高原 · 两房可玩原型",journeyTitleStyle);
            string[] labels={"1 光路线\n元气弹＋风刃＋风波","2 暗路线\n流星＋风刃＋风波","3 基础武器\n无技能、零元素，检查通行"};
            for(int i=0;i<3;i++)if(FlatButton(new Rect(w/2-420+i*285,h/2-145,270,125),labels[i],(int)thunderPreset==i))thunderPreset=(ThunderPreset)i;
            CenterLabel(new Rect(w/2-410,h/2+5,820,35),"A / D 起点："+(thunderStartRoom==0?"P01 草坡战斗房":"P02 引雷机关房"),journeyTextStyle);
            if(FlatButton(new Rect(w/2-285,h/2+60,270,60),"F 开始当前配置"))BeginThunderSample(thunderPreset,thunderStartRoom);
            if(FlatButton(new Rect(w/2+15,h/2+60,270,60),"继续原型存档"))LoadThunderCheckpoint(true);
            CenterLabel(new Rect(w/2-410,h/2+150,820,55),"独立原型：三层、Boss、雷技能尚未接入\nF5 / F9 使用原型存档；正常沙漠旅程不受影响",journeySmallStyle);
        }
    }
}
