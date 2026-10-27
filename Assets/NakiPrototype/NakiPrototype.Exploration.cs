using System.Collections.Generic;
using UnityEngine;

namespace NakiLegend.Prototype
{
    public sealed partial class NakiPrototype
    {
        private static readonly Vector2Int[] Cardinal = { Vector2Int.up,Vector2Int.down,Vector2Int.right,Vector2Int.left };
        private readonly int[] stageStarts = new int[3], stageEnds = new int[3];
        private readonly List<GameObject> doorSeals = new List<GameObject>();
        private readonly List<Rect> doorShotBlocks = new List<Rect>();
        private readonly List<PendingSpawn> pendingSpawns = new List<PendingSpawn>();
        private sealed class PendingSpawn { public EnemyKind kind; public bool champion;public Vector2 at; public float when; public GameObject mark; }
        private readonly Sprite[] floorModules=new Sprite[4],wallModules=new Sprite[4],sideModules=new Sprite[4],corridorModules=new Sprite[4];
        private int transitionTarget=-1,transitionEntry=-1;
        private float doorTransition,doorCooldownUntil;
        private bool transitionSwitched;
        private GameObject stairs;
        private Vector2 stairPosition;
        private static readonly string[] StageNames={"沙丘边缘","断渠遗迹","古渠核心"};

        private Rect CurrentRoomBounds { get { RoomNode n=journeyRooms[currentRoom];return new Rect(-n.halfWidth,-n.halfHeight,n.halfWidth*2,n.halfHeight*2); } }

        private void BuildSpatialGraph(System.Random random)
        {
            journeyRooms.Clear();int[] sizes={12,13,14};
            for(int stage=0;stage<3;stage++)
            {
                int start=journeyRooms.Count;stageStarts[stage]=start;
                var occupied=new Dictionary<Vector2Int,int>();
                AddSpatialNode(stage,new Vector2Int(2,2),random);occupied[new Vector2Int(2,2)]=start;
                while(journeyRooms.Count-start<sizes[stage])
                {
                    int baseId=start+random.Next(journeyRooms.Count-start);RoomNode from=journeyRooms[baseId];
                    int direction=random.Next(4);Vector2Int cell=from.cell+Cardinal[direction];
                    if(cell.x<0||cell.y<0||cell.x>5||cell.y>5||occupied.ContainsKey(cell))continue;
                    int id=journeyRooms.Count;AddSpatialNode(stage,cell,random);occupied[cell]=id;ConnectRooms(baseId,id);
                }
                foreach(var pair in occupied)
                    foreach(Vector2Int direction in Cardinal)
                    {int other;if(occupied.TryGetValue(pair.Key+direction,out other)&&other>pair.Value&&random.NextDouble()<.16&&!journeyRooms[pair.Value].links.Contains(other))ConnectRooms(pair.Value,other);}
                var queue=new Queue<int>();var distances=new Dictionary<int,int>();queue.Enqueue(start);distances[start]=0;
                while(queue.Count>0){int id=queue.Dequeue();foreach(int next in journeyRooms[id].links)if(!distances.ContainsKey(next)){distances[next]=distances[id]+1;queue.Enqueue(next);}}
                int end=start;foreach(var pair in distances)if(pair.Value>distances[end])end=pair.Key;stageEnds[stage]=end;
                foreach(var pair in distances){RoomNode n=journeyRooms[pair.Key];n.depth=stage*5+pair.Value;n.waves=n.kind==RoomKind.Combat?2+(stage==2?1:0):1;}
                journeyRooms[start].kind=RoomKind.Entrance;journeyRooms[end].kind=stage==2?RoomKind.Boss:RoomKind.Guardian;journeyRooms[end].waves=1;
                var candidates=new List<int>();for(int id=start+1;id<journeyRooms.Count;id++)if(id!=end)candidates.Add(id);
                for(int i=candidates.Count-1;i>0;i--){int j=random.Next(i+1);int a=candidates[i];candidates[i]=candidates[j];candidates[j]=a;}
                RoomKind[] specials={RoomKind.Rest,RoomKind.Event,RoomKind.Reward,RoomKind.Elite};
                for(int i=0;i<specials.Length;i++){RoomNode n=journeyRooms[candidates[i]];n.kind=specials[i];n.waves=n.kind==RoomKind.Elite?3+(stage==2?1:0):1;}
                if(stage>0){RoomNode n=journeyRooms[candidates[4]];n.kind=RoomKind.Elite;n.waves=3;}
            }
            for(int stage=0;stage<2;stage++){journeyRooms[stageEnds[stage]].stairsTo=stageStarts[stage+1];journeyRooms[stageStarts[stage+1]].stairsTo=stageEnds[stage];}
            journeyRooms[stageStarts[0]].discovered=true;
        }

        private void AddSpatialNode(int stage,Vector2Int cell,System.Random random)
        {
            int layout=random.Next(64);float width=7.5f+(float)random.NextDouble()*1.6f,height=3.55f+(float)random.NextDouble()*.9f;
            journeyRooms.Add(new RoomNode{id=journeyRooms.Count,stage=stage,cell=cell,layout=layout,floor=stage==0?random.Next(2)*3:stage==1?1+random.Next(2):random.Next(4),
                kind=RoomKind.Combat,halfWidth=width,halfHeight=height,stairsTo=-1});
        }

        private int NeighborDirection(RoomNode from,RoomNode to)
        {Vector2Int d=to.cell-from.cell;for(int i=0;i<4;i++)if(d==Cardinal[i])return i;return -1;}
        private bool HasDoor(int direction)
        {RoomNode node=journeyRooms[currentRoom];foreach(int link in node.links)if(NeighborDirection(node,journeyRooms[link])==direction)return true;return false;}
        private Vector2 DoorNormal(int direction){return new Vector2(Cardinal[direction].x,Cardinal[direction].y);}
        private Vector2 DoorPoint(int direction,float outward=0)
        {RoomNode n=journeyRooms[currentRoom];Vector2 normal=DoorNormal(direction);return normal*((direction<2?n.halfHeight:n.halfWidth)+outward);}

        private bool CanStandInExploration(Vector2 at,float radius)
        {
            Rect r=CurrentRoomBounds;
            if(at.x>=r.xMin+radius&&at.x<=r.xMax-radius&&at.y>=r.yMin+radius&&at.y<=r.yMax-radius)return true;
            if(!journeyRooms[currentRoom].cleared)return false;
            for(int d=0;d<4;d++)if(HasDoor(d))
            {
                Vector2 normal=DoorNormal(d),delta=at-DoorPoint(d);float outDistance=Vector2.Dot(delta,normal),side=Mathf.Abs(Vector2.Dot(delta,new Vector2(-normal.y,normal.x)));
                if(outDistance>=-radius-.1f&&outDistance<=1.9f-radius&&side<=1.15f-radius)return true;
            }
            return false;
        }

        private Vector2 MoveInExploration(Vector2 from,Vector2 delta,float radius)
        {
            if(!CanStandInExploration(from,radius)||Blocked(from,radius)){from=NearestSafeFooting(from);arrivalRecoveries++;}
            Vector2 next=from,candidate=new Vector2(from.x+delta.x,from.y);
            if(CanStandInExploration(candidate,radius)&&!Blocked(candidate,radius))next.x=candidate.x;
            candidate=new Vector2(next.x,from.y+delta.y);if(CanStandInExploration(candidate,radius)&&!Blocked(candidate,radius))next.y=candidate.y;
            return next;
        }
        private bool WithinProjectileBounds(Vector2 at)
        {Rect r=CurrentRoomBounds;return at.x>=r.xMin-2.5f&&at.x<=r.xMax+2.5f&&at.y>=r.yMin-2.5f&&at.y<=r.yMax+2.5f;}

        private void BuildRoomModules()
        {
            for(int i=0;i<4;i++)if(journeyFloors[i]!=null)
            {
                Texture2D t=journeyFloors[i].texture;
                floorModules[i]=ModuleSprite(t,new Rect(.14f,.19f,.72f,.55f));
                wallModules[i]=ModuleSprite(t,new Rect(.12f,.018f,.20f,.10f));
                corridorModules[i]=ModuleSprite(t,new Rect(.22f,.43f,.13f,.17f));
                sideModules[i]=ModuleSprite(t,new Rect(.008f,.3f,.032f,.38f));
            }
        }
        private Sprite ModuleSprite(Texture2D texture,Rect uv)
        {Sprite s=Sprite.Create(texture,new Rect(uv.x*texture.width,uv.y*texture.height,uv.width*texture.width,uv.height*texture.height),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);ownedAssets.Add(s);return s;}
        private GameObject Stretch(string name,Sprite sprite,Vector2 at,Vector2 size,Color color,int order)
        {Sprite art=sprite??square;return Draw(name,art,at,new Vector2(size.x/art.bounds.size.x,size.y/art.bounds.size.y),color,order);}

        private void BuildExplorationRoom(RoomNode node)
        {
            int floor=node.stage;Rect b=CurrentRoomBounds;ReserveTransitSpace(node);
            doorSeals.Clear();doorShotBlocks.Clear();
            TileFloor("Tiled island paving",Vector2.zero,new Vector2(b.width,b.height),node.stage,-1000);
            // A continuous inset border keeps room edges readable without baking scenery into tiles.
            foreach(float sign in new[]{-1f,1f})
            {
                Stretch("Floor inset",square,new Vector2(0,sign*(node.halfHeight-.17f)),new Vector2(b.width-.3f,.045f),new Color(.25f,.48f,.47f,.36f),-992);
                Stretch("Floor inset",square,new Vector2(sign*(node.halfWidth-.17f),0),new Vector2(.045f,b.height-.3f),new Color(.25f,.48f,.47f,.36f),-992);
            }
            for(int d=0;d<4;d++)
            {
                bool doorway=HasDoor(d);float span=d<2?node.halfWidth:node.halfHeight;
                if(doorway)BuildReadableGate(d,node.stage);
                float begin=-span;
                while(begin<span-.01f)
                {
                    float end=Mathf.Min(span,begin+2.1f);
                    if(doorway&&begin<1.2f&&end>-1.2f){if(begin<-1.2f)BuildWallPiece(d,begin,-1.2f,floor);if(end>1.2f)BuildWallPiece(d,1.2f,end,floor);}
                    else BuildWallPiece(d,begin,end,floor);
                    begin=end;
                }
            }
            Vector2[] positions=JourneyCoverPositions(node.layout%16);
            float flip=(node.layout&16)!=0?-1:1,offset=(node.layout&32)!=0?.18f:0;
            foreach(Vector2 source in positions)
            {
                Vector2 at=new Vector2(source.x*(node.halfWidth-1.35f)/7*flip,source.y*(node.halfHeight-1.2f)/2.4f+offset);
                if(IntersectsTransit(at,new Vector2(1.3f,2.2f)))continue;
                AddJourneyPillar(at);
            }
            if(node.layout%3==0&&journeyRock!=null)
            {
                Vector2 at=FindFreeInteractionPoint(new Vector2(3.7f,-.2f));if(IntersectsTransit(at,new Vector2(2.1f,1.2f)))return;float scale=1.8f/journeyRock.bounds.size.x;
                Draw("Solid fallen cover",journeyRock,at,Vector2.one*scale,Color.white,DepthAt(at.y));obstacles.Add(new Rect(at.x-.83f,at.y-.24f,1.66f,.48f));
                Bounds rock=journeyRock.bounds;projectileObstacles.Add(new Rect(at.x+rock.min.x*scale,at.y+rock.min.y*scale,rock.size.x*scale,rock.size.y*scale));
            }
        }

        private void BuildWallPiece(int direction,float begin,float end,int floor)
        {
            if(end-begin<.02f)return;RoomNode node=journeyRooms[currentRoom];bool horizontal=direction<2;float mid=(begin+end)*.5f;
            Vector2 at=direction==0?new Vector2(mid,node.halfHeight+.42f):direction==1?new Vector2(mid,-node.halfHeight-.42f):direction==2?new Vector2(node.halfWidth+.4f,mid):new Vector2(-node.halfWidth-.4f,mid);
            Vector2 size=horizontal?new Vector2(end-begin,.9f):new Vector2(.85f,end-begin);
            GameObject wall=Draw("Masonry perimeter",terrainV12[0]??wallModules[floor],at,Vector2.one,new Color(.77f,.70f,.58f),DepthAt(at.y));
            var renderer=wall.GetComponent<SpriteRenderer>();renderer.drawMode=SpriteDrawMode.Tiled;renderer.size=size;
            Vector2 normal=DoorNormal(direction);
            Stretch("Wall interior shadow",square,at-normal*.38f,horizontal?new Vector2(size.x,.10f):new Vector2(.10f,size.y),new Color(.13f,.12f,.10f,.55f),DepthAt(at.y)+1);
            Stretch("Wall cap light",square,at+normal*.20f,horizontal?new Vector2(size.x,.055f):new Vector2(.055f,size.y),new Color(.95f,.79f,.5f,.42f),DepthAt(at.y)+2);
            projectileObstacles.Add(new Rect(at.x-size.x*.5f,at.y-size.y*.5f,size.x,size.y));
        }

        private void UpdateExplorationDoors()
        {
            bool open=journeyRooms[currentRoom].cleared;
            foreach(GameObject seal in doorSeals)if(seal!=null)seal.SetActive(!open);
            if(open){foreach(Rect r in doorShotBlocks)projectileObstacles.Remove(r);doorShotBlocks.Clear();}
            CreateStairsIfReady();
        }

        private void CreateStairsIfReady()
        {
            RoomNode node=journeyRooms[currentRoom];if(!node.cleared||node.stairsTo<0||stairs!=null)return;
            stairPosition=NearestSafeFooting(PreferredStairPoint(node));
            stairs=Stretch("Illuminated descending stairs",stairsV12??windRingSprite,stairPosition+Vector2.up*.45f,new Vector2(2.15f,2.15f),Color.white,-650);
            doorCooldownUntil=Mathf.Max(doorCooldownUntil,clock+1);
        }

        private void TryAutomaticExit()
        {
            RoomNode node=journeyRooms[currentRoom];if(!node.cleared||clock<doorCooldownUntil||doorTransition>0)return;
            foreach(int link in node.links)
            {
                int direction=NeighborDirection(node,journeyRooms[link]);Vector2 n=DoorNormal(direction),delta=position-DoorPoint(direction);
                if(Vector2.Dot(delta,n)>1.0f&&Mathf.Abs(Vector2.Dot(delta,new Vector2(-n.y,n.x)))<.55f){BeginDoorTransition(link,direction^1);return;}
            }
            // Stairs use explicit F interaction; normal room doors remain automatic.
        }

        private void BeginDoorTransition(int target,int entry)
        {transitionTarget=target;transitionEntry=entry;transitionSwitched=false;doorTransition=.24f;}
        private bool TickDoorTransition(float dt)
        {
            if(doorTransition<=0)return false;doorTransition-=Mathf.Min(dt,.05f);
            if(!transitionSwitched&&doorTransition<=.12f)
            {transitionSwitched=true;EnterExplorationRoom(transitionTarget,transitionEntry);doorCooldownUntil=clock+.8f;}
            if(doorTransition<=0){doorTransition=0;transitionTarget=-1;}return true;
        }

        private void TickPendingSpawns()
        {
            for(int i=pendingSpawns.Count-1;i>=0;i--)
                if(clock>=pendingSpawns[i].when){PendingSpawn spawn=pendingSpawns[i];Destroy(spawn.mark);Enemy e=SpawnJourneyEnemy(spawn.kind,spawn.at,journeyRooms[currentRoom].depth);if(spawn.champion)MakeChampion(e);pendingSpawns.RemoveAt(i);}
        }

        private void QueueEnemy(EnemyKind kind,Vector2 at,bool champion=false)
        {
            GameObject mark=Draw("Reinforcement warning",disc,at,Vector2.one*.75f,new Color(.9f,.38f,.2f,.4f),-650);
            pendingSpawns.Add(new PendingSpawn{kind=kind,champion=champion,at=at,when=clock+.8f,mark=mark});
        }

        private void FollowExplorationCamera(float dt)
        {
            RoomNode node=journeyRooms[currentRoom];float maxX=Mathf.Max(0,node.halfWidth+1.9f-view.orthographicSize*view.aspect),maxY=Mathf.Max(0,node.halfHeight+1.9f-view.orthographicSize);
            Vector3 target=new Vector3(Mathf.Clamp(position.x*.35f,-maxX,maxX),Mathf.Clamp(position.y*.25f,-maxY,maxY),-10);
            view.transform.position=Vector3.Lerp(view.transform.position,target,Mathf.Clamp01(dt*6));
            if(clock<cameraShakeUntil)view.transform.position+=Vector3.right*Mathf.Sin(clock*180)*.025f;
        }
    }
}
