using System;
using System.Collections.Generic;
using UnityEngine;

namespace NakiLegend.Prototype
{
    public sealed partial class NakiPrototype
    {
        [Header("Thunder Plateau chapter")]
        public bool thunderChapterMode;
        [Range(0,2)] public int thunderChapterStartFloor;
        [Range(-1,2)] public int thunderChapterPracticeBoss=-1;
        private bool chapterSetup,chapterWest,chapterEast,chapterRanger,chapterStone,chapterEnding,chapterCg;
        private bool chapterMarkKnown,chapterArcKnown,chapterLampCarried,chapterLampGiven;
        private int chapterWaymarks,chapterShortcuts,chapterFocusedAction;
        private float chapterWork;
        private readonly List<ChapterAction> chapterActions=new List<ChapterAction>();
        private readonly List<ChapterShortcut> chapterDoors=new List<ChapterShortcut>();
        private string[] chapterNames;
        private Sprite chapterLampkeeper,chapterRangerArt;
        private Texture2D chapterEndingArt;
        private GameObject chapterFocus;
        private bool ThunderActive { get { return thunderPrototypeMode||thunderChapterMode; } }
        private static readonly string[] ThunderStages={"雨中草坡","引雷遗址","雷鸣圣所"};
        private bool HasThunderConductor { get { return thunderChapterMode?currentRoom==14||currentRoom==16:currentRoom==1; } }
        private sealed class ChapterAction { public int kind;public Vector2 at;public string label; }
        [Serializable] private sealed class ChapterRoomDef { public int id=0,stage=0,kind=0,waves=0,layout=0,x=0,y=0,stairsTo=-1;public string title="",key="";public float width=8,height=5;public int[] links=Array.Empty<int>(); }
        [Serializable] private sealed class ChapterShortcut { public int a=0,b=0,code=0; }
        [Serializable] private sealed class ChapterGraph { public ChapterRoomDef[] rooms=Array.Empty<ChapterRoomDef>();public ChapterShortcut[] gates=Array.Empty<ChapterShortcut>(); }
        private void BuildThunderChapterGraph()
        {
            TextAsset data=Resources.Load<TextAsset>("NakiArt/ThunderChapter/rooms");
            if(data==null)throw new InvalidOperationException("Missing Thunder Plateau room data");
            ChapterGraph g=JsonUtility.FromJson<ChapterGraph>(data.text);
            if(g.rooms.Length!=39)throw new InvalidOperationException("Thunder Plateau needs 39 rooms");
            journeyRooms.Clear();chapterDoors.Clear();chapterDoors.AddRange(g.gates);chapterNames=new string[39];
            foreach(ChapterRoomDef d in g.rooms)
            {
                Vector2Int cell=new Vector2Int(d.x,d.y);int rotation=(activeSeed+d.stage)%4;
                for(int k=0;k<rotation;k++)cell=new Vector2Int(4-cell.y,cell.x);
                if((activeSeed&4)!=0)cell.x=4-cell.x;
                RoomNode n=new RoomNode{id=d.id,stage=d.stage,kind=(RoomKind)d.kind,depth=d.stage*4,layout=(d.layout+activeSeed%6)%6,
                    floor=d.stage,cell=cell,halfWidth=d.width,halfHeight=d.height,waves=d.waves,stairsTo=d.stairsTo};
                n.links.AddRange(d.links);journeyRooms.Add(n);chapterNames[d.id]=d.title;
            }
            stageStarts[0]=0;stageStarts[1]=12;stageStarts[2]=25;stageEnds[0]=6;stageEnds[1]=18;stageEnds[2]=31;
        }
        private void BeginThunderChapter(bool newSeed)
        {
            gatehouseSampleMode=bossPracticeMode=lightningTrialMode=thunderPrototypeMode=false;
            if(!tuningCaptured){initialSwordDamage=swordDamage;initialSpellDamage=spellDamage;tuningCaptured=true;}
            swordDamage=initialSwordDamage;spellDamage=initialSpellDamage;
            if(newSeed||activeSeed==0)activeSeed=journeySeed!=0&&!newSeed?journeySeed:(Environment.TickCount&0x7fffffff);
            if(activeSeed==0)activeSeed=28001;
            ResetGatehouseProgress();ResetPlayerProgression();LoadThunderChapterEquipment();BuildThunderChapterGraph();
            chapterWest=chapterEast=chapterRanger=chapterStone=chapterEnding=chapterCg=chapterMarkKnown=chapterArcKnown=chapterLampCarried=chapterLampGiven=false;
            chapterWaymarks=chapterShortcuts=0;chapterWork=0;chapterSavePending=false;chapterSkillsMenu=false;chapterSkillSlots[0]=0;chapterSkillSlots[1]=1;
            clock=0;health=6;shield=EffectiveMaximumShield;elementPower=100;origin=thunderPreset==ThunderPreset.Dark?Origin.Dark:Origin.Light;
            energy=origin==Origin.Light?100:0;darkPower=origin==Origin.Dark?24:0;orbLearned=bladeLearned=waveLearned=windEmpowered=true;
            carriedWeapons[1]=6;nextSword=nextSpell=nextDash=dashUntil=invulnerableUntil=flashUntil=bladeReadyAt=waveReadyAt=chapterMarkReady=chapterArcReady=0;
            journeyComplete=pendingBossClear=won=false;doorTransition=0;chapterSaveStatus="本轮尚未存档";
            if(thunderPreset==ThunderPreset.BasicWeapons){orbLearned=bladeLearned=waveLearned=false;elementPower=energy=darkPower=0;carriedWeapons[1]=8;}
            int room=stageStarts[Mathf.Clamp(thunderChapterStartFloor,0,2)];
            if(thunderChapterPracticeBoss>=0)
            {
                chapterWest=chapterEast=true;chapterMarkKnown=chapterArcKnown=true;room=stageEnds[Mathf.Clamp(thunderChapterPracticeBoss,0,2)];
                chapterSkillSlots[1]=1;
            }
            else if(thunderChapterStartFloor>0)
            {
                // Quick starts are explicitly test presets; they unlock earlier
                // mandatory nodes while keeping the rest of the island explorable.
                journeyRooms[6].cleared=true;chapterMarkKnown=true;
                if(thunderChapterStartFloor>1){chapterWest=chapterEast=chapterRanger=chapterArcKnown=true;journeyRooms[18].cleared=true;journeyRooms[14].cleared=journeyRooms[14].lootSeeded=journeyRooms[16].cleared=journeyRooms[16].lootSeeded=journeyRooms[17].cleared=true;}
            }
            EnterExplorationRoom(room,-1);
            if(room==0)TellStory("灯匠：以前这场雨过了，巡路的人就该回来。现在，雷声总停在同一个山头。","少年：我去看看。若能把路接上，也替你捎个平安。沿石路上山，先找到旧路标。");
            else TellStory(thunderChapterPracticeBoss>=0?"雷雨高原 Boss 练习：N 选择起点，R 重试；练习不覆盖正式章节存档。":"雷雨高原快速起点：此前必要节点已开放，可继续探索。N 可重选；F5 / F9 使用第二岛独立存档。");
        }
        private string ChapterTitle { get { return "第 "+(journeyRooms[currentRoom].stage+1)+" 层 · "+ThunderStages[journeyRooms[currentRoom].stage]; } }
        private string ChapterMapLabel(RoomNode n)
        {
            if(n.id==14||n.id==16)return (n.id==14?chapterWest:chapterEast)?"已修节点":"引雷节点";
            if(n.id==17)return "巡路员";
            if(n.id==0)return "灯棚";
            if(n.kind==RoomKind.Event)return "路迹";
            if(n.kind==RoomKind.Guardian)return "守卫";
            return JourneyRoomType(n.kind);
        }
        private void LoadThunderChapterArt()
        {
            if(!thunderChapterMode)return;
            LoadChapterActors();chapterLampkeeper=ChapterImage("lampkeeper",1.15f);chapterRangerArt=ChapterImage("ranger",1.15f);
            chapterEndingArt=Resources.Load<Texture2D>("NakiArt/ThunderChapter/ending");
            LoadThunderChapterEquipment();
        }
        private void BuildChapterCover(RoomNode n)
        {
            Vector2[][] variants={new[]{new Vector2(-4,-1),new Vector2(4,1)},new[]{new Vector2(-5,1),new Vector2(4,-1.8f)},
                new[]{new Vector2(-4,-2),new Vector2(3.7f,1.2f),new Vector2(-4,1.2f)},new[]{new Vector2(-5,0),new Vector2(5,0)},
                new[]{new Vector2(-3.5f,-1.6f),new Vector2(4,-.3f)},new[]{new Vector2(-5,-1.8f),new Vector2(4,1.7f)}};
            if(n.kind==RoomKind.Boss||n.kind==RoomKind.Guardian)
            {foreach(Vector2 p in new[]{new Vector2(-6.5f,0),new Vector2(6.5f,0)})if(!IntersectsTransit(p,new Vector2(1.8f,1.2f)))AddThunderRock(p);return;}
            foreach(Vector2 p in variants[n.layout%6])if(!IntersectsTransit(p,new Vector2(1.8f,1.4f)))AddThunderRock(p);
        }
        private void ReserveChapterProps(RoomNode n)
        {
            if(!thunderChapterMode)return;
            var points=new List<Vector2>();
            if(n.id==0)points.Add(new Vector2(-3.4f,1.4f));
            if(n.id==17)points.Add(new Vector2(-3,1.6f));
            if(n.id==14||n.id==16)points.Add(ThunderLeverAt);
            if(n.id==6||n.id==16||n.kind==RoomKind.Rest)points.Add(new Vector2(-2.2f,-.8f));
            if(n.id==21||n.id==22||n.id==36)points.Add(new Vector2(-2,1.3f));
            if(n.id==9)points.AddRange(new[]{new Vector2(-3,1),new Vector2(0,1.4f),new Vector2(3,-.8f)});
            foreach(Vector2 at in points)transitReservations.Add(new Rect(at-Vector2.one,Vector2.one*2));
        }
        private bool ChapterGateReady(int code)
        {
            switch(code)
            {
                case 1:return journeyRooms[5].cleared&&journeyRooms[8].cleared;
                case 2:return journeyRooms[17].cleared&&journeyRooms[20].cleared&&chapterWest&&chapterEast;
                case 3:return journeyRooms[17].cleared;
                case 4:return journeyRooms[29].cleared&&journeyRooms[35].cleared;
                default:return chapterWest&&chapterEast;
            }
        }
        private bool ChapterDoorLocked(int room,int direction)
        {
            if(!thunderChapterMode)return false;
            foreach(ChapterShortcut g in chapterDoors)
            {
                int other=g.a==room?g.b:g.b==room?g.a:-1;if(other<0||NeighborDirection(journeyRooms[room],journeyRooms[other])!=direction)continue;
                return g.code==5?!ChapterGateReady(5):(chapterShortcuts&(1<<g.code))==0;
            }
            return false;
        }
        private bool ExplorationDoorLocked(int room,int direction){return GatehouseDoorLocked(room,direction)||ChapterDoorLocked(room,direction);}
        private void SpawnChapterWave(RoomNode n)
        {
            var random=new System.Random(activeSeed+n.id*191+currentWave*31);
            if(n.kind==RoomKind.Boss||n.kind==RoomKind.Guardian)
            {
                EnemyKind kind=n.stage==0?EnemyKind.CopperWarden:n.stage==1?EnemyKind.ThunderSentinel:EnemyKind.StormStag;
                SpawnJourneyEnemy(kind,new Vector2(0,.1f),n.depth);return;
            }
            if(n.kind==RoomKind.Elite&&currentWave==1)
            {Enemy e=SpawnJourneyEnemy(n.id==34?EnemyKind.SandKite:EnemyKind.ThunderStone,FindJourneySpawn(random,.55f),n.depth);MakeChampion(e);e.affinity=e.attackElement=Element.Lightning;}
            int count=n.stage==0?currentWave==1?2:3:n.stage==1?3+currentWave%2:4+currentWave%2;
            for(int i=0;i<count;i++)
            {
                EnemyKind k=n.id==1?EnemyKind.ChargeMoth:n.id==2?EnemyKind.StormGuard:
                    n.stage==0?(i%2==0?EnemyKind.ChargeMoth:EnemyKind.StormGuard):
                    i==0&&n.stage>0?EnemyKind.CopperRoller:i==1&&n.stage==2?EnemyKind.ThunderStone:i%2==0?EnemyKind.ChargeMoth:EnemyKind.StormGuard;
                QueueEnemy(k,FindJourneySpawn(random,k==EnemyKind.ThunderStone?.55f:.34f));
                pendingSpawns[pendingSpawns.Count-1].when=clock+(currentWave==1?1.5f:.8f)+i*.18f;
            }
        }
        private void OnChapterRoomCleared(RoomNode n)
        {
            ClearThunderHazards();chapterSavePending=true;
            if(n.id==5&&!n.lootSeeded){n.lootSeeded=true;ChapterDrop(8,new Vector2(1.6f,-1.5f));}
            if(n.id==3&&!n.claimed){n.claimed=true;TellStory("旧路标的铜尺上，有和灯匠工具相同的三道刻痕。","少年：有人刚修过这里。她走的是山上那条路。");}
            if(n.id==17&&!chapterRanger){chapterRanger=true;TellStory("巡路员：下面的灯终于亮了……师父还在等？","少年：他一直擦着那盏灯。","巡路员：我把撤离的路标立好。山顶那头鹿，请你别把它当成只会伤人的怪物。");}
            BuildChapterActions();
        }
        private void CompleteChapterBoss()
        {
            foreach(Enemy e in enemies){if(e.warning!=null)Destroy(e.warning);Destroy(e.root);if(e.shadow!=null)Destroy(e.shadow);}enemies.Clear();
            foreach(PendingSpawn s in pendingSpawns)if(s.mark!=null)Destroy(s.mark);pendingSpawns.Clear();
            foreach(Shot s in shots)if(s.root!=null)Destroy(s.root);shots.Clear();ClearThunderHazards();
            ClearJourneyRoom();health=Mathf.Max(1,health);RoomNode n=journeyRooms[currentRoom];
            if(n.stage==0)TellStory("铜卫背后的灯变成通行纹，警戒雷声停了。","少年：它守的原本是路，不是困住人的门。靠近铜纹台可学习雷标，再沿亮起的石阶前往引雷遗址。");
            else if(n.stage==1)TellStory("铜环缓缓落下，山顶石阶的封锁解除了。","巡路员：上面是古道守护兽。那些紫线不属于它。","少年：我看见了。先让它停下来。");
            else
            {
                chapterStone=true;journeyComplete=true;chapterWork=0;
                TellStory("巨鹿撑住前腿，胸前的紫色束缚逐段断开。少年取出雷元素碎片。","少年：够了，剩下的路交给我。","守护兽仍活着，雷云终于越过山脊。靠近山路风铃按 F，沿已修古道回山口。");
            }
            UpdateExplorationDoors();BuildChapterActions();RequestCheckpoint();SaveChapterCheckpoint(false);
        }
        private void ChapterDrop(int weapon,Vector2 desired)
        {
            RoomNode n=journeyRooms[currentRoom];var stored=new StoredWeapon{id=weapon,at=FindFreeInteractionPoint(desired)};n.drops.Add(stored);CreateWeaponDropVisual(stored);
        }
        private string ChapterObjective()
        {
            if(chapterWork>0)return "正在复位手柄 · ESC 取消";
            RoomNode n=journeyRooms[currentRoom];if(!n.cleared)return "第 "+currentWave+" / "+n.waves+" 波 · 剩余 "+(enemies.Count+pendingSpawns.Count);
            if(chapterStone)return chapterEnding?"灯下有归人 · 可回访高原支路":currentRoom==0?"回到灯匠身边 · F 迎接归人":"雷石已回收 · 山路风铃 F 返回灯棚";
            if(n.stage==2)return "沿石路前往圣所 · 解除巡雷巨鹿的束缚";
            if(n.stage==1)return !chapterWest?"复位西引雷节点 · 东北侧铜手柄 F":!chapterEast?"继续探索，复位东引雷节点":"找到巡路员，再前往山顶石阶";
            return "沿石路上山 · 支路可寻补给与旧路标";
        }
    }
}
