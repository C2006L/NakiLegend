using System.Collections.Generic;
using UnityEngine;

namespace NakiLegend.Prototype
{
    public sealed partial class NakiPrototype
    {
        [Header("Desert island journey")]
        public bool enableDesertJourney = true;
        public int journeySeed;
        [Range(-1,2)] public int bossOverride = -1;
        public bool bossPracticeMode;
        private enum RoomKind { Entrance,Combat,Elite,Rest,Event,Reward,Guardian,Boss }
        private sealed class RoomNode
        {
            public int id,stage,depth,layout,floor,waves,stairsTo=-1;
            public Vector2Int cell;
            public float halfWidth,halfHeight;
            public RoomKind kind;
            public bool visited,discovered,cleared,claimed,lootSeeded;
            public readonly List<int> links=new List<int>();
            public readonly List<StoredWeapon> drops=new List<StoredWeapon>();
            public readonly List<StoredEssence> essence=new List<StoredEssence>();
        }
        private readonly List<RoomNode> journeyRooms=new List<RoomNode>();
        private readonly Sprite[] journeyFloors=new Sprite[4];
        private Sprite journeyRock;
        private Texture2D rainCG;
        private int activeSeed,currentRoom,selectedBoss,currentWave,routeChoice,restoredCanals;
        private float nextWaveAt=-1,journeyEnergyRegen=4;
        private float initialSwordDamage,initialSpellDamage;
        private bool tuningCaptured,mapMenu,rewardMenu,showRainCG,journeyComplete,pendingBossClear,pendingRainCG;
        private string[] storyLines;
        private int storyIndex;
        private Font journeyFont;
        private GUIStyle journeyTitleStyle,journeyTextStyle,journeySmallStyle,journeyButtonStyle;
        private static readonly string[] RoomNames={"风蚀庭院","断渠回廊","埋沙柱厅","干涸蓄水池","砂岩拱场","旧井围庭","季风露台","枯根遗址"};
        private static readonly string[] BossNames={"噬雨风蝎","古渠巨像","沙原猿王"};

        private void LoadJourneyAssets()
        {
            string[] floors={"journey-courtyard","journey-canal","journey-cistern","journey-terrace"};
            for(int i=0;i<floors.Length;i++)journeyFloors[i]=JourneySprite(floors[i],100,Vector2.one*.5f);
            journeyRock=JourneySprite("journey-cover-rock",130,new Vector2(.5f,.08f));
            rainCG=Resources.Load<Texture2D>("NakiArt/desert-rain-cg");
            journeyFont=Resources.Load<Font>("Fonts/FusionPixel");
            LoadEnemyActionArt();LoadEquipmentArt();BuildRoomModules();LoadDesertV12Art();SetupJourneyAudio();
        }
        private Sprite JourneySprite(string name,float ppu,Vector2 pivot)
        {
            Texture2D texture=Resources.Load<Texture2D>("NakiArt/"+name);if(texture==null)return null;texture.wrapMode=TextureWrapMode.Clamp;
            Sprite sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),pivot,ppu,0,SpriteMeshType.FullRect);sprite.name=name;ownedAssets.Add(sprite);return sprite;
        }

        private void BeginJourney(bool newSeed=false)
        {
            StopOpeningCinematic();
            if(!tuningCaptured){initialSwordDamage=swordDamage;initialSpellDamage=spellDamage;tuningCaptured=true;}
            swordDamage=initialSwordDamage;spellDamage=initialSpellDamage;
            int previous=activeSeed;
            if(newSeed||activeSeed==0)activeSeed=!newSeed&&journeySeed!=0?journeySeed:(System.Environment.TickCount^(activeSeed*31))&0x7fffffff;
            if(newSeed&&activeSeed==previous)activeSeed=(activeSeed+7919)&0x7fffffff;if(activeSeed==0)activeSeed=1;
            var random=new System.Random(activeSeed);selectedBoss=bossOverride>=0?Mathf.Clamp(bossOverride,0,2):random.Next(3);BuildJourneyGraph(random);
            clock=0;health=6;energy=100;journeyComplete=false;restoredCanals=0;doorTransition=0;
            nextSword=nextSpell=nextDash=dashUntil=invulnerableUntil=flashUntil=0;ResetPlayerProgression();
            EnterExplorationRoom(bossPracticeMode?stageEnds[2]:stageStarts[0],-1);
            if(bossPracticeMode)TellStory(lightningTrialMode?"雷印试炼：同一位守关者已标记为雷属性。比较 L 元气弹、U 风刃和 I 风波的效果。":"守关者试炼：三种技能已开放。R 重试，N 重抽守关者，H 切换武器。");
            else if(!TryStartOpeningCinematic()) TellStory("老人：风曾把海上的云带过山口，如今只卷走井底最后一点凉气。","少年：我会让风重新认得回家的路。先沿着门后的旧渠，去寻找风铃台。");
        }
        private void BuildJourneyGraph(System.Random random){BuildSpatialGraph(random);}
        private void ConnectRooms(int a,int b){journeyRooms[a].links.Add(b);journeyRooms[b].links.Add(a);}
        private void EnterJourneyRoom(int id){EnterExplorationRoom(id,-1);}

        private void EnterExplorationRoom(int id,int entryDirection)
        {
            RoomNode node=journeyRooms[id];bool first=!node.visited;currentRoom=id;node.visited=node.discovered=true;
            foreach(int link in node.links)journeyRooms[link].discovered=true;
            if(world!=null){world.gameObject.SetActive(false);Destroy(world.gameObject);}world=new GameObject("Explorable room "+id).transform;world.SetParent(transform,false);
            enemies.Clear();shots.Clear();obstacles.Clear();projectileObstacles.Clear();pendingSpawns.Clear();ClearJourneyEffects();
            paused=won=false;defeated=0;bossSpawned=node.kind==RoomKind.Boss||node.kind==RoomKind.Guardian;
            swordHitPending=false;swordUntil=castPoseUntil=pixelHurtUntil=clock;dashUntil=clock;invulnerableUntil=Mathf.Max(invulnerableUntil,clock+.4f);
            mapMenu=rewardMenu=showRainCG=pendingBossClear=pendingRainCG=inventoryMenu=false;pendingWeaponPickup=null;storyLines=null;storyIndex=0;routeChoice=0;
            lessonAltar=rewardChest=activeWindWave=stairs=combatReticle=null;combatTarget=null;essenceDrops.Clear();
            position=entryDirection>=0?DoorPoint(entryDirection,-.85f):new Vector2(0,-node.halfHeight+.9f);
            facing=entryDirection>=0?-DoorNormal(entryDirection):Vector2.up;
            BuildExplorationRoom(node);
            position=SafeArrivalPoint(entryDirection);invulnerableUntil=Mathf.Max(invulnerableUntil,clock+1.4f);roomBannerUntil=clock+2.8f;
            hero=Draw("Young adventurer",heroSprite,position,Vector2.one,Color.white,10);heroBody=hero.GetComponent<SpriteRenderer>();
            sword=Draw("Equipped weapon",square,position,Vector2.one,Color.white,30);sword.SetActive(false);SetupIllustratedHero();
            currentWave=0;nextWaveAt=-1;
            if(!node.cleared)
            {
                if(IsCombatRoom(node))SpawnJourneyWave();
                else
                {
                    node.cleared=true;
                    if(first&&!node.claimed)
                    {
                        if(node.kind==RoomKind.Rest){health=Mathf.Min(6,health+2);energy=100;elementPower=Mathf.Min(100,elementPower+40);node.claimed=true;TellStory("你在树荫下补充饮水，恢复2生命、光之力，并补充40元素之力。");}
                        if(node.kind==RoomKind.Event){restoredCanals++;health=Mathf.Min(6,health+1);energy=Mathf.Min(100,energy+20);elementPower=Mathf.Min(100,elementPower+20);node.claimed=true;TellStory("你清开堵住旧渠的沙石，将水车的卡槽重新对齐。","孩子：等找到水，能先给树根留一瓢吗？","少年：好。也给村里的人留一份。");}
                        if(node.kind==RoomKind.Reward)SeedRoomLoot(node);
                    }
                }
            }
            DrawRoomLoot(node);RestoreEssence(node);UpdateJourneyPortal();doorCooldownUntil=clock+.8f;
        }
        private static bool IsCombatRoom(RoomNode node){return node.kind==RoomKind.Combat||node.kind==RoomKind.Elite||node.kind==RoomKind.Guardian||node.kind==RoomKind.Boss;}

        private void AddJourneyPillar(Vector2 at)
        {
            obstacles.Add(new Rect(at.x-.42f,at.y-.24f,.84f,.48f));
            if(pillarArtwork==null){projectileObstacles.Add(new Rect(at.x-.42f,at.y-.24f,.84f,1.8f));return;}
            Bounds b=pillarArtwork.bounds;projectileObstacles.Add(new Rect(at.x+b.min.x,at.y+b.min.y,b.size.x,b.size.y));
            Draw("Pillar shadow",softDisc??disc,at+new Vector2(.1f,-.03f),new Vector2(1.2f,.4f),new Color(.12f,.08f,.04f,.3f),-800);
            Draw("Solid sandstone pillar",pillarArtwork,at,Vector2.one,Color.white,DepthAt(at.y));
        }
        private static Vector2[] JourneyCoverPositions(int layout)
        {
            Vector2[][] patterns={
                new[]{new Vector2(-6,-2.1f),new Vector2(6,-2.1f),new Vector2(-6,1.15f),new Vector2(6,1.15f)},
                new[]{new Vector2(-4,-1.6f),new Vector2(4,1.1f)},new[]{new Vector2(-6,0),new Vector2(0,0),new Vector2(6,0)},
                new[]{new Vector2(-4,-1.8f),new Vector2(4,-.3f),new Vector2(-4,1.1f)},
                new[]{new Vector2(-6,-1.8f),new Vector2(6,-1.8f),new Vector2(-3,1.1f),new Vector2(3,1.1f)},
                new[]{new Vector2(-5,-1),new Vector2(0,1),new Vector2(5,-1)},
                new[]{new Vector2(-6,-1.8f),new Vector2(-2,1.1f),new Vector2(2,-1.8f),new Vector2(6,1.1f)},new[]{new Vector2(-7,0),new Vector2(-3,0),new Vector2(3,0),new Vector2(7,0)}};
            Vector2[] source=patterns[layout%8],result=new Vector2[source.Length];for(int i=0;i<result.Length;i++)result[i]=layout<8?source[i]:new Vector2(-source[i].x,source[i].y+.35f);return result;
        }

        private void SpawnJourneyWave()
        {
            RoomNode node=journeyRooms[currentRoom];currentWave++;nextWaveAt=-1;
            var random=new System.Random(activeSeed^(currentRoom*503+currentWave*19));
            if(node.kind==RoomKind.Boss||node.kind==RoomKind.Guardian)
            {
                EnemyKind kind=node.kind==RoomKind.Boss?(EnemyKind)(5+selectedBoss):node.stage==0?EnemyKind.StoneSentinel:(EnemyKind)(5+(selectedBoss+1)%3);
                Enemy guardian=SpawnJourneyEnemy(kind,FindJourneySpawn(random,.9f),node.depth);
                if(node.kind==RoomKind.Guardian){guardian.boss=true;guardian.affinity=lightningTrialMode?Element.Lightning:Element.Wind;guardian.hp=guardian.maximum=node.stage==0?190:290;guardian.root.transform.localScale=Vector3.one*(node.stage==0?1.2f:1);}
                return;
            }
            if(node.kind==RoomKind.Elite&&(currentWave==1||currentWave==node.waves))QueueEnemy(EnemyKind.StoneSentinel,FindJourneySpawn(random,.65f));
            int count=node.kind==RoomKind.Elite?3+node.stage:4+node.stage+random.Next(2);
            int types=clearedCombatRooms<2?1:clearedCombatRooms<4?2:clearedCombatRooms<7?3:4;
            for(int i=0;i<count;i++){int kind=i==0?types-1:i<types?(i+currentWave-2)%types:random.Next(types);QueueEnemy((EnemyKind)kind,FindJourneySpawn(random,.38f),node.kind==RoomKind.Elite&&i==count-1);}
        }
        private Vector2 FindJourneySpawn(System.Random random,float radius)
        {
            Rect bounds=CurrentRoomBounds;
            for(int i=0;i<160;i++)
            {
                Vector2 at=new Vector2(Mathf.Lerp(bounds.xMin+1,bounds.xMax-1,(float)random.NextDouble()),Mathf.Lerp(bounds.yMin+1,bounds.yMax-1,(float)random.NextDouble()));
                if(!Blocked(at,radius)&&!ProjectilePathBlocked(at,at,radius)&&Vector2.Distance(at,position)>3.5f&&!IntersectsTransit(at,Vector2.one*1.0f)&&!SpawnTooClose(at,radius))return at;
            }
            for(float y=bounds.yMax-1;y>bounds.yMin+.6f;y-=1)for(float x=bounds.xMin+1;x<bounds.xMax-1;x+=1.2f){Vector2 at=new Vector2(x,y);if(!Blocked(at,radius)&&!ProjectilePathBlocked(at,at,radius)&&Vector2.Distance(at,position)>3&&!SpawnTooClose(at,radius))return at;}
            throw new System.InvalidOperationException("No safe enemy spawn in room "+currentRoom);
        }
        private bool SpawnTooClose(Vector2 at,float radius)
        {foreach(Enemy e in enemies)if(Vector2.Distance(e.position,at)<radius+e.radius+.3f)return true;foreach(PendingSpawn p in pendingSpawns)if(Vector2.Distance(p.at,at)<radius+.65f)return true;return false;}

        private void TickJourney(float dt)
        {
            TickJourneyEffects(dt);
            if(pendingBossClear){pendingBossClear=false;CompleteJourneyBoss();return;}
            TickPendingSpawns();TickGroundShocks();
            RoomNode node=journeyRooms[currentRoom];
            if(!node.cleared&&enemies.Count==0&&pendingSpawns.Count==0)
            {
                if(currentWave<node.waves){if(nextWaveAt<0)nextWaveAt=clock+1.1f;else if(clock>=nextWaveAt)SpawnJourneyWave();}
                else ClearJourneyRoom();
            }
            if(Pressed('f'))TryRoomInteraction();TryAutomaticExit();
        }
        private void ClearJourneyRoom()
        {
            RoomNode node=journeyRooms[currentRoom];if(node.cleared)return;node.cleared=true;
            if(IsCombatRoom(node))clearedCombatRooms++;
            foreach(Shot shot in shots)if(shot.root!=null)Destroy(shot.root);shots.Clear();
            SeedRoomLoot(node);CheckPendingLesson();UpdateJourneyPortal();
        }
        private void UpdateJourneyPortal(){UpdateExplorationDoors();}
        private void FinishJourneyBoss(){pendingBossClear=true;}
        private void CompleteJourneyBoss()
        {
            foreach(Enemy e in enemies){if(e.warning!=null)Destroy(e.warning);if(e.shadow!=null)Destroy(e.shadow);Destroy(e.root);}enemies.Clear();
            foreach(PendingSpawn spawn in pendingSpawns)if(spawn.mark!=null)Destroy(spawn.mark);pendingSpawns.Clear();
            foreach(var g in groundShocks)if(g.root!=null)Destroy(g.root);groundShocks.Clear();
            RoomNode node=journeyRooms[currentRoom];ClearJourneyRoom();health=Mathf.Max(1,health);swordUntil=castPoseUntil=pixelHurtUntil=clock;UpdatePixelHero(Vector2.zero);
            if(node.kind==RoomKind.Guardian)
            {
                elementSupplies++;
                if(node.stage==0&&!bladeLearned){clearedCombatRooms=Mathf.Max(clearedCombatRooms,6);}
                if(node.stage==1&&!waveLearned){clearedCombatRooms=Mathf.Max(clearedCombatRooms,10);}
                CheckPendingLesson();TellStory(node.stage==0?"守井者停下重锤，古井碑上的风纹亮了起来。靠近按 F 学习，再走向标注“前往第2层”的石阶，靠近按 F 前往断渠遗迹。":"风铃重新响起。先寻找风铃台学习风波，标注“前往第3层”的石阶通向古渠核心，靠近按 F 进入。");return;
            }
            journeyComplete=true;windEmpowered=true;elementPower=Mathf.Min(100,elementPower+30);
            TellStory("紫色裂纹熄灭，风元素碎片从守关者体内脱离。山口的云终于朝村庄而来。",restoredCanals>0?"你和村民修好的旧渠重新响起水声。孩子：井里有声音了！":"村民开始疏通旧渠。孩子伸手：这是……雨吗？","少年：风把它带回来了。第一瓢，记得留给树根。","获得风元素碎片：风刃强化，风波冷却缩短。愿长风所到之处，皆有甘泽。");pendingRainCG=true;
        }
        private void TellStory(params string[] lines){storyLines=lines;storyIndex=0;}
        private bool StoryActive{get{return storyLines!=null&&storyIndex<storyLines.Length;}}
        private void AdvanceJourneyStory(bool skip=false){storyIndex=skip?storyLines.Length:storyIndex+1;if(pendingRainCG&&(storyIndex>=2||!StoryActive)){showRainCG=true;pendingRainCG=false;}}
        private bool HandleJourneyOverlayInput()
        {
            if(TickOpeningCinematic(Time.unscaledDeltaTime))return true;
            if(TickDoorTransition(Time.deltaTime))return true;
            if(HandleBackpackInput())return true;
            if(StoryActive){if(Pressed('f'))AdvanceJourneyStory();if(Pressed('e'))AdvanceJourneyStory(true);return true;}
            if(showRainCG){if(Pressed('f')||Pressed('e'))showRainCG=false;return true;}
            if(rewardMenu){if(Pressed('a'))routeChoice=(routeChoice+2)%3;if(Pressed('d'))routeChoice=(routeChoice+1)%3;if(Pressed('f'))ClaimJourneyReward(routeChoice);return true;}
            if(mapMenu){if(Pressed('m')||Pressed('e'))mapMenu=false;return true;}
            if(health>0&&Pressed('m')){mapMenu=true;return true;}return false;
        }
        private void ClaimJourneyReward(int choice)
        {
            RoomNode node=journeyRooms[currentRoom];if(node.claimed){rewardMenu=false;return;}node.claimed=true;rewardMenu=false;
            if(choice==0){health=Mathf.Min(6,health+2);healingSupplies++;}else if(choice==1)swordDamage+=2;else{elementPower=Mathf.Min(100,elementPower+35);elementSupplies++;}
            if(rewardChest!=null)Destroy(rewardChest);rewardChest=null;
        }
        private void CastWind(){CastWindWave();}
    }
}
