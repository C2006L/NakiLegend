using System.Collections.Generic;
using UnityEngine;

namespace NakiLegend.Prototype
{
    public sealed partial class NakiPrototype
    {
        private readonly bool[] gateSandCleared = new bool[2];
        private bool gateRepairCommitted, gateRewardCreated, gateTotalUnlocked, gateRainReturned;
        private float gateWorkRemaining, gateWorkDuration;
        private int gateWorkKind = -1;
        private readonly List<GameObject> gateWaterVisuals = new List<GameObject>();
        private readonly List<GameObject> gateRainVisuals = new List<GameObject>();
        private readonly GameObject[] gateSandVisuals = new GameObject[2];
        private GameObject gateWheelVisual, gateWorkRing, gateRotor;
        private Sprite gateWheelArt, gateWheelBaseArt, gateWheelFaceArt, gateSandArt, gateChannelArt;
        private static readonly Vector2[] GateActionPoints = {
            new Vector2(-4.4f,2.4f), new Vector2(2.4f,2.3f), new Vector2(-2.6f,2.7f),
            new Vector2(6.8f,0), new Vector2(-2.6f,2.2f), new Vector2(-2.6f,1.4f)
        };
        private static readonly string[] GateRoomNames = { "来路营地", "旧渠闸室", "分水总闸", "检修庭院", "观雨台" };

        private void LoadGatehouseArt()
        {
            gateWheelArt=GatehouseSprite("wheel",new Rect(322,32,889,976),Vector2.one*.5f);
            gateWheelBaseArt=GatehouseSprite("wheel",new Rect(468,32,600,322),Vector2.one*.5f);
            Texture2D face=Resources.Load<Texture2D>("NakiArt/Gatehouse/wheel-face");
            if(face!=null){gateWheelFaceArt=Sprite.Create(face,new Rect(0,0,face.width,face.height),Vector2.one*.5f,face.width,0,SpriteMeshType.FullRect);ownedAssets.Add(gateWheelFaceArt);}
            gateSandArt=GatehouseSprite("sand",new Rect(237,242,1082,491),new Vector2(.5f,.2f));
            gateChannelArt=GatehouseSprite("channel",new Rect(0,266,1983,268),Vector2.one*.5f);
        }
        private Sprite GatehouseSprite(string name,Rect rect,Vector2 pivot)
        {
            Texture2D t=Resources.Load<Texture2D>("NakiArt/Gatehouse/"+name);if(t==null)return null;
            Sprite s=Sprite.Create(t,rect,pivot,240,0,SpriteMeshType.FullRect);s.name="Gatehouse "+name;ownedAssets.Add(s);return s;
        }

        private void BuildGatehouseGraph()
        {
            journeyRooms.Clear(); bossPracticeMode=lightningTrialMode=false;
            ResetGatehouseProgress();AddGatehouseRooms(0,1);
            for(int i=0;i<3;i++){stageStarts[i]=0;stageEnds[i]=2;}
            journeyRooms[0].discovered=true;
        }

        private bool GatehouseDoorLocked(int room,int direction)
        {
            if(room<0||room>=journeyRooms.Count)return false;
            int local=journeyRooms[room].gatehouseIndex;if(local<0)return false;
            bool shortcut=(local==1&&direction==2)||(local==3&&direction==3);
            return shortcut&&(!gateRepairCommitted||gateWorkRemaining>0||!GatehouseRoom(1).cleared||!GatehouseRoom(3).cleared);
        }

        private void BuildGatehouseScenery(RoomNode room)
        {
            int local=room.gatehouseIndex;
            gateWaterVisuals.Clear();gateRainVisuals.Clear();gateWheelVisual=gateWorkRing=gateRotor=null;
            gateSandVisuals[0]=gateSandVisuals[1]=null;
            if(local==1)
            {
                AddJourneyPillar(new Vector2(-3.3f,0));AddJourneyPillar(new Vector2(3.3f,0));
                BuildGateChannel(1.2f,13.6f);
                BuildTurningWheel(GateActionPoints[2],1,gateRepairCommitted);
                AddGateWheelCollision(GateActionPoints[2],new Vector2(1.35f,1.2f));
                gateWorkRing=Draw("Handwheel turning glow",windRingSprite,GateActionPoints[2],Vector2.one*.72f,C("80dccc"),DepthAt(2.3f)+2);
                gateWorkRing.SetActive(false);
                for(int i=0;i<2;i++)if(!gateSandCleared[i])gateSandVisuals[i]=Stretch("Sand clog "+i,gateSandArt??disc,GateActionPoints[i],new Vector2(1.1f,.63f),gateSandArt!=null?Color.white:C("d8ab64"),DepthAt(GateActionPoints[i].y));
            }
            else
            {
                foreach(Vector2 at in new[]{new Vector2(-4.8f,-.5f),new Vector2(4.7f,1.1f)})if(!IntersectsTransit(at,new Vector2(1.3f,2.2f)))AddJourneyPillar(at);
                BuildGateChannel(.8f,11.8f);
                if(local==2){BuildTurningWheel(GateActionPoints[4],1.1f,gateTotalUnlocked);AddGateWheelCollision(GateActionPoints[4],new Vector2(1.5f,1.3f));}
                if(local==4)Draw("Rain watching bell",lessonArtwork??windRingSprite,GateActionPoints[5],Vector2.one,C("c3dfcf"),DepthAt(1.4f));
            }
            RefreshGatehouseWater();
        }
        private void AddGateWheelCollision(Vector2 at,Vector2 size)
        {
            obstacles.Add(new Rect(at.x-.32f,at.y-size.y*.47f,.64f,.25f));
            projectileObstacles.Add(new Rect(at-size*.5f,size));
        }
        private void BuildTurningWheel(Vector2 at,float scale,bool turned)
        {
            if(gateWheelFaceArt==null){gateWheelVisual=Stretch("Canal handwheel",gateWheelArt??windRingSprite,at,new Vector2(1.35f,1.2f)*scale,Color.white,DepthAt(at.y-.4f));return;}
            gateWheelVisual=Stretch("Fixed wheel pedestal",gateWheelBaseArt,at+Vector2.down*.29f*scale,new Vector2(.9f,.58f)*scale,Color.white,DepthAt(at.y-.4f));
            var projection=new GameObject("Handwheel plane");projection.transform.SetParent(world,false);projection.transform.position=at+Vector2.up*.27f*scale;projection.transform.localScale=new Vector3(1.45f*scale,.88f*scale,1);
            gateRotor=Draw("Rotating bronze handwheel",gateWheelFaceArt,Vector2.zero,Vector2.one,Color.white,DepthAt(at.y-.4f)+2);gateRotor.transform.SetParent(projection.transform,false);
            gateRotor.transform.localRotation=Quaternion.Euler(0,0,turned?90:0);
        }

        private void BuildGateChannel(float y,float width)
        {
            if(gateChannelArt!=null)for(float x=-width/2;x<width/2-.01f;x+=2.4f)
            {float length=Mathf.Min(2.4f,width/2-x);Stretch("Dry channel section",gateChannelArt,new Vector2(x+length/2,y),new Vector2(length,.6f),Color.white,-980);}
            else Stretch("Dry canal bed",square,new Vector2(0,y),new Vector2(width,.55f),C("4f5c59"),-980);
            GameObject water=Stretch("Canal water",square,new Vector2(0,y),new Vector2(width-.1f,.18f),new Color(.22f,.64f,.65f,.64f),-978);gateWaterVisuals.Add(water);
            for(int i=0;i<8;i++)gateWaterVisuals.Add(Stretch("Water ripple",square,new Vector2(-width/2+.8f+i*width/8,y+.02f),new Vector2(.3f,.025f),new Color(.72f,.95f,.88f,.7f),-977));
        }

        private void RefreshGatehouseWater()
        {
            bool wet=gateRepairCommitted&&gateTotalUnlocked;
            for(int i=0;i<gateWaterVisuals.Count;i++)if(gateWaterVisuals[i]!=null){gateWaterVisuals[i].SetActive(wet);if(i==0)gateWaterVisuals[i].GetComponent<SpriteRenderer>().color=gateRainReturned?new Color(.26f,.7f,.72f,.8f):new Color(.24f,.55f,.57f,.42f);}
            if(gateRainReturned&&gateRainVisuals.Count==0)
                for(int i=0;i<28;i++){GameObject drop=Stretch("Gentle returning rain",square,Vector2.zero,new Vector2(.014f,.17f),new Color(.64f,.85f,.93f,.24f),700);drop.transform.rotation=Quaternion.Euler(0,0,-12);gateRainVisuals.Add(drop);}
        }
        private void TickGatehouseEnvironment()
        {
            if(!GatehouseActive)return;
            for(int i=1;i<gateWaterVisuals.Count;i++)if(gateWaterVisuals[i]!=null&&gateWaterVisuals[i].activeSelf)
            {Vector3 at=gateWaterVisuals[i].transform.position;at.x=-5.6f+Mathf.Repeat(i*1.35f+clock*.32f,11.2f);gateWaterVisuals[i].transform.position=at;}
            for(int i=0;i<gateRainVisuals.Count;i++)if(gateRainVisuals[i]!=null)gateRainVisuals[i].transform.position=new Vector2(-7.5f+Mathf.Repeat(i*2.731f+clock*.35f,15),4.2f-Mathf.Repeat(i*1.137f+clock*3.3f,8.4f));
        }

        private void OnGatehouseRoomEntered()
        {
            // Editor room-entry tools may interrupt animation. Committed repairs stay
            // complete; unfinished sand interactions are cancelled without a reward.
            if(gateWorkRemaining>0){gateWorkRemaining=0;gateWorkKind=-1;UpdateExplorationDoors();}
            RefreshGatehouseWater();
        }

        private void SpawnGatehouseWave(RoomNode room)
        {
            EnemyKind[] kinds=room.gatehouseIndex==3?new[]{EnemyKind.SandScorpion,EnemyKind.SpearGuard}:
                currentWave==1?new[]{EnemyKind.SandScorpion,EnemyKind.SandScorpion,EnemyKind.SpearGuard,EnemyKind.SpearGuard}:
                new[]{EnemyKind.SandScorpion,EnemyKind.SandScorpion,EnemyKind.SpearGuard,EnemyKind.SandKite};
            Vector2[] desired={new Vector2(-5,1.8f),new Vector2(4,-2.6f),new Vector2(-4,-.9f),new Vector2(5.2f,2.8f)};
            var random=new System.Random(activeSeed+room.id*37+currentWave*197);
            for(int i=0;i<kinds.Length;i++)
            {
                Vector2 at=desired[i];if(!GatehouseSpawnValid(at))
                {
                    bool found=false;
                    for(int n=0;n<300;n++){at=new Vector2(Mathf.Lerp(-6.8f,6.8f,(float)random.NextDouble()),Mathf.Lerp(-3.2f,3.2f,(float)random.NextDouble()));if(GatehouseSpawnValid(at)){found=true;break;}}
                    if(!found)throw new System.InvalidOperationException("No safe gatehouse reinforcement point");
                }
                QueueEnemy(kinds[i],at);
            }
        }

        private bool GatehouseSpawnValid(Vector2 at)
        {
            if(Blocked(at,.38f)||ProjectilePathBlocked(at,at,.38f)||Vector2.Distance(at,position)<3.5f||IntersectsTransit(at,Vector2.one)||SpawnTooClose(at,.38f))return false;
            if(CurrentGatehouseIndex==1)for(int i=0;i<3;i++)if(Vector2.Distance(at,GateActionPoints[i])<1.45f)return false;
            return true;
        }

        private int NearbyGatehouseAction()
        {
            int best=-1;float distance=1.35f;
            int begin=CurrentGatehouseIndex==1?0:CurrentGatehouseIndex==2?4:CurrentGatehouseIndex==4?5:6,end=CurrentGatehouseIndex==1?4:begin+1;
            for(int i=begin;i<end&&i<6;i++)
            {
                if(i<2&&gateSandCleared[i])continue;
                float d=Vector2.Distance(position,GateActionPoints[i]);if(d<distance){distance=d;best=i;}
            }
            return best;
        }

        private bool TryGatehouseInteraction()
        {
            if(gateWorkRemaining>0)return true;
            int kind=NearbyGatehouseAction();if(kind<0)return false;
            if(!journeyRooms[currentRoom].cleared){JourneyHint("先清理本房间的敌人，再检查机关");return true;}
            if(kind<2){gateWorkKind=kind;gateWorkDuration=.6f;gateWorkRemaining=.6f;return true;}
            if(kind==2)
            {
                if(gateRepairCommitted){JourneyHint("手轮已经修好，检修闸需两端都安全才开放");return true;}
                if(!gateSandCleared[0]||!gateSandCleared[1]){JourneyHint("轮齿被沙卡住了 · 先清理两处沙堆");return true;}
                gateRepairCommitted=true;CreateGatehouseReward();gateWorkKind=2;gateWorkDuration=2;gateWorkRemaining=2;
                if(gateWorkRing!=null)gateWorkRing.SetActive(true);UpdateExplorationDoors();return true;
            }
            if(kind==3){JourneyHint(GatehouseDoorLocked(GatehouseRoom(1).id,2)?gateRepairCommitted?"检修闸已修好 · 先沿北路清理另一侧庭院":"检修闸被卡住 · 清沙后转动手轮":"检修通路已开启 · 沿门向东前进");return true;}
            if(kind==4)
            {
                if(gateTotalUnlocked){JourneyHint("总闸已打开 · 已修复的小渠可以通过存水");return true;}
                gateTotalUnlocked=true;RequestCheckpoint();if(gateRotor!=null)gateRotor.transform.localRotation=Quaternion.Euler(0,0,90);RefreshGatehouseWater();TellStory("少年：总闸能动了。先让留下的水慢慢流过去。","总闸已解锁。修好闸室后回访，可见少量存水。向东绕行可以清理检修庭院。");return true;
            }
            if(!gatehouseSampleMode)return TryCompleteCanalEpilogue();
            if(!gateTotalUnlocked){JourneyHint("先去分水总闸打开阀门，再回来守候初雨");return true;}
            if(gateRainReturned){JourneyHint("云已越过山口 · 回访闸室查看雨后的水渠");return true;}
            gateRainReturned=true;RefreshGatehouseWater();TellStory("少年：山口的云，终于朝这边来了。","样板已切换雨后回访景象。正式章节会由取回风石推进此变化。");return true;
        }

        private void CreateGatehouseReward()
        {
            if(gateRewardCreated)return;gateRewardCreated=true;
            var data=new StoredEssence{at=FindFreeInteractionPoint(GateActionPoints[2]+Vector2.right*1.1f-Vector2.up*.2f),value=20,born=clock};
            GatehouseRoom(1).essence.Add(data);DrawEssence(data);
        }

        private bool TickGatehouseInteraction(float dt)
        {
            if(!GatehouseActive||gateWorkRemaining<=0)return false;
            if(Pressed('e')){FinishGatehouseInteraction(gateWorkKind==2);return true;}
            gateWorkRemaining=Mathf.Max(0,gateWorkRemaining-Mathf.Min(dt,.05f));
            if(gateWorkKind==2&&gateWorkRing!=null)gateWorkRing.transform.rotation=Quaternion.Euler(0,0,(1-gateWorkRemaining/gateWorkDuration)*180);
            if(gateWorkKind==2&&gateRotor!=null)gateRotor.transform.localRotation=Quaternion.Euler(0,0,(1-gateWorkRemaining/gateWorkDuration)*90);
            if(gateWorkRemaining<=0)FinishGatehouseInteraction(true);return true;
        }

        private void FinishGatehouseInteraction(bool apply)
        {
            int kind=gateWorkKind;gateWorkRemaining=0;gateWorkKind=-1;
            if(apply&&kind>=0&&kind<2){gateSandCleared[kind]=true;if(gateSandVisuals[kind]!=null)Destroy(gateSandVisuals[kind]);JourneyHint(gateSandCleared[0]&&gateSandCleared[1]?"沙已清开 · 靠近手轮按 F":"一处沙堆已清开 · 再检查另一处");}
            if(apply)RequestCheckpoint();
            if(kind==2){if(gateWorkRing!=null)gateWorkRing.SetActive(false);if(gateRotor!=null)gateRotor.transform.localRotation=Quaternion.Euler(0,0,90);RefreshGatehouseWater();UpdateExplorationDoors();JourneyHint("手轮已修复 · 留下一枚20元素结晶，检修闸需两端安全");}
        }

        private string GatehousePrompt()
        {
            int k=NearbyGatehouseAction();if(k<0)return "";
            if(!journeyRooms[currentRoom].cleared)return "先清理敌人，再检查机关";
            if(k<2)return "F 清开沙堆";
            if(k==2)return gateRepairCommitted?"手轮已修复":gateSandCleared[0]&&gateSandCleared[1]?"F 转动手轮 · 获得20元素结晶":"F 检查卡住的手轮";
            if(k==3)return "F 查看检修闸";
            if(k==5&&!gatehouseSampleMode)return CanalCompletionPrompt();
            return k==4?gateTotalUnlocked?"总闸已打开":"F 打开分水总闸":gateRainReturned?"雨已归来":"F 守候初雨 · 样板景象预览";
        }

        private string GatehouseObjective()
        {
            if(gateWorkRemaining>0)return gateWorkKind==2?"转动手轮 · ESC 跳过演出":"清开沙堆 · ESC 取消";
            if(!journeyRooms[currentRoom].cleared)return "第 "+currentWave+" / "+journeyRooms[currentRoom].waves+" 波 · 剩余 "+(enemies.Count+pendingSpawns.Count);
            if(!gatehouseSampleMode&&CurrentGatehouseIndex==4)return CanalCompletionHint();
            if(CurrentGatehouseIndex==0)return "向北进入旧渠闸室 · 清理后可修闸或继续探索";
            if(CurrentGatehouseIndex==2)return gateTotalUnlocked?"向东绕行观雨台，再向南清理庭院":"靠近分水总闸按 F · 打开设施总锁";
            if(CurrentGatehouseIndex==4)return "向南清理检修庭院 · 可在铃台预览雨后回访";
            if(!gateRepairCommitted)return "可以沿北门前进 · 清开两处沙堆后转动手轮";
            return GatehouseDoorLocked(GatehouseRoom(1).id,2)?"手轮已修好 · 沿北路清理检修庭院以开放捷径":"检修捷径已开启 · 可回访水渠与未捡结晶";
        }
    }
}
