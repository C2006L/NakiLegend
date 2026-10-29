using UnityEngine;

namespace NakiLegend.Prototype
{
    public sealed partial class NakiPrototype
    {
        private bool chapterRewardMenu;
        private int chapterRewardChoice;
        private void AddChapterAction(int kind,Vector2 at,string label,Sprite sprite=null)
        {
            chapterActions.Add(new ChapterAction{kind=kind,at=at,label=label});
            transitReservations.Add(new Rect(at-new Vector2(.8f,.5f),new Vector2(1.6f,1.5f)));
            if(sprite!=null){GameObject o=Draw(label,sprite,at,Vector2.one,Color.white,DepthAt(at.y));o.transform.SetParent(chapterFocus.transform,true);}
        }
        private void BuildChapterActions()
        {
            if(!thunderChapterMode)return;chapterActions.Clear();if(chapterFocus!=null){chapterFocus.SetActive(false);Destroy(chapterFocus);}
            chapterFocus=new GameObject("Highland residents and interactions");chapterFocus.transform.SetParent(world,false);
            RoomNode n=journeyRooms[currentRoom];
            if(woodlandMode){BuildWoodActions(n);return;}
            if(n.id==0)AddChapterAction(1,new Vector2(-3.4f,1.4f),"灯匠",chapterLampkeeper);
            if(n.id==0&&chapterStone&&chapterRanger)AddChapterAction(2,new Vector2(-1.8f,1.4f),"归来的巡路员",chapterRangerArt);
            if(n.id==0&&chapterEnding)AddChapterAction(208,new Vector2(1,-2),"森林岛试航 · 第五岛",WorldPropSprite(5));
            if(n.id==0&&chapterEnding)AddChapterAction(16,new Vector2(3.4f,-1.4f),"登船返回沙漠",WorldPropSprite(5));
            if(n.id==17&&n.cleared)AddChapterAction(2,new Vector2(-3,1.6f),"巡路员",chapterRangerArt);
            if(n.id==14||n.id==16)AddChapterAction(n.id==14?3:4,ThunderLeverAt,"复位引雷手柄");
            if(n.cleared&&(n.id==6||n.kind==RoomKind.Rest))
            {
                if(journeyRooms[6].cleared&&!chapterMarkKnown)AddChapterAction(5,new Vector2(-2.2f,-.8f),"学习雷标",lessonArtwork);
                else if(chapterEast&&!chapterArcKnown)AddChapterAction(6,new Vector2(-2.2f,-.8f),"学习跃电",lessonArtwork);
            }
            if(n.id==16&&chapterEast&&!chapterArcKnown)AddChapterAction(6,new Vector2(-2.2f,-.8f),"学习跃电",lessonArtwork);
            if(n.id==31&&chapterStone){AddChapterAction(7,new Vector2(-2.5f,-1.5f),"沿已修古道回山口");DrawWorldSign(new Vector2(-2.5f,-1.1f),chapterFocus.transform);}
            if(n.id==31&&chapterStone)
            {
                Sprite[] poses;if(chapterArt.TryGetValue("stag",out poses)&&poses.Length>7){GameObject guardian=Draw("Living guardian resting",poses[7],new Vector2(1,.1f),Vector2.one,Color.white,DepthAt(.1f));guardian.transform.SetParent(chapterFocus.transform,true);}
            }
            if(n.kind==RoomKind.Rest)AddChapterAction(9,new Vector2(2.5f,.4f),"整理元素技能",chapterRangerArt);
            if(n.id==9)
            {
                Vector2[] points={new Vector2(-3,1),new Vector2(0,1.4f),new Vector2(3,-.8f)};
                for(int i=0;i<3;i++)
                {
                    AddChapterAction(10+i,points[i],(chapterWaymarks&(1<<i))!=0?"路标已扶正":"扶正旧路标");
                    GameObject post=Stretch("Wooden trail waymarker",square,points[i]+Vector2.up*.4f,new Vector2(.12f,.8f),C("7f725b"),DepthAt(points[i].y));post.transform.SetParent(chapterFocus.transform,true);
                    GameObject arrow=Stretch("Trail waymarker arrow",square,points[i]+Vector2.up*.65f,new Vector2(.6f,.15f),C("b6ad8a"),DepthAt(points[i].y)+1);arrow.transform.SetParent(chapterFocus.transform,true);
                    if((chapterWaymarks&(1<<i))==0){post.transform.rotation=Quaternion.Euler(0,0,22);arrow.transform.rotation=Quaternion.Euler(0,0,22);}
                }
            }
            if(n.id==21)AddChapterAction(13,new Vector2(-2,1.3f),n.claimed?"避雨棚已修好":"扶正避雨棚梁",thunderRock);
            if(n.id==22)AddChapterAction(14,new Vector2(-2,1.3f),chapterLampCarried||chapterLampGiven?"灯罩已取走":"收下送往山口的灯罩",thunderNode);
            if(n.id==36)AddChapterAction(15,new Vector2(-2,1.3f),"远望山口的灯",lessonArtwork);
            foreach(ChapterShortcut g in chapterDoors)if(g.code<5&&(g.a==n.id||g.b==n.id))
            {
                int other=g.a==n.id?g.b:g.a,d=NeighborDirection(n,journeyRooms[other]);
                if((chapterShortcuts&(1<<g.code))==0)AddChapterAction(100+g.code,DoorPoint(d,-1.5f),"打通检修捷径");
            }
        }
        private void OnChapterRoomEntered(RoomNode n,bool first)
        {
            chapterWork=0;chapterRewardMenu=false;chapterSkillsMenu=false;
            if(first&&n.kind==RoomKind.Rest&&!n.claimed)
            {n.claimed=true;health=Mathf.Min(6,health+2);elementPower=Mathf.Min(100,elementPower+30);TellStory(woodlandMode?"阿苓留下了一壶草药茶。你扫开厚叶，靠着仍有温度的树根歇息。":"避雨棚里留着一壶温水。你抖掉斗篷上的雨，重新系紧护具，听远处的雷声渐渐移向山顶。");AnnounceDiscovery("休憩 · 生命 +2，元素 +30；安全时可整理技能与保存");}
            BuildChapterActions();
        }
        private ChapterAction NearestChapterAction()
        {
            ChapterAction best=null;float distance=1.4f;
            foreach(ChapterAction a in chapterActions){float d=Vector2.Distance(position,a.at);if(d<distance){best=a;distance=d;}}
            return best;
        }
        private string ChapterPrompt()
        {
            ChapterAction a=NearestChapterAction();if(a==null)return "";
            return !journeyRooms[currentRoom].cleared?"先清理本房间，再检查设施":"F "+a.label;
        }
        private bool TryChapterInteraction()
        {
            RoomNode n=journeyRooms[currentRoom];ChapterAction a=NearestChapterAction();
            if(a==null)
            {
                if(n.kind==RoomKind.Reward&&!n.claimed&&rewardChest!=null&&Vector2.Distance(position,chestPosition)<1.4f)
                {chapterRewardMenu=true;chapterRewardChoice=0;return true;}
                return false;
            }
            if(!n.cleared){JourneyHint(woodlandMode&&a.kind==203?"先击败看守，取得牢笼钥匙":"先确保房间安全，再检查路标与设施");return true;}
            if(woodlandMode){WoodAction(a.kind);return true;}
            if(a.kind==208){EmbarkWoodland();return true;}
            if(a.kind>=100)
            {
                int code=a.kind-100;if(!ChapterGateReady(code)){JourneyHint("检修通路两端尚未清理；引雷节点也需复位。主路仍可通行。");return true;}
                chapterShortcuts|=1<<code;UpdateExplorationDoors();BuildChapterActions();RequestCheckpoint();JourneyHint("检修便道已开放 · 以后直接走门即可");return true;
            }
            switch(a.kind)
            {
                case 1:
                    if(chapterLampCarried&&!chapterLampGiven){chapterLampGiven=true;chapterLampCarried=false;RequestCheckpoint();}
                    if(chapterStone&&!chapterEnding){storyLines=null;chapterCg=true;SaveChapterCheckpoint(false);}
                    else TellStory(chapterEnding?"灯匠：灯没白亮。路也回来了，往后的雨会有它自己的季节。":"灯匠：沿石路上山，铜卫守着旧引雷设施。若见到巡路的人，告诉她山口的灯还亮着。");break;
                case 2:if(chapterStone)TellStory("巡路员：明早我会再上山，把回程的路标立稳。这次，先在灯下歇一晚。");else TellStory("巡路员：路上的节点接回去了，山顶却还在聚雷。紫色束缚不属于守护兽。","少年：我会把困住它的东西取出来。");break;
                case 3:case 4:
                    if(a.kind==3?chapterWest:chapterEast){JourneyHint("这处节点已经复位，雷流沿边槽通行");break;}
                    chapterFocusedAction=a.kind;chapterWork=1.2f;break;
                case 5:chapterMarkKnown=true;TellStory("铜纹台上的凹痕，一处记风向，一处记落雷。你把手停在刻痕上，试着先留下一点电光。","远处的石面亮了一瞬，雷声才随后抵达。");AnnounceDiscovery("已领悟雷标 · 8元素，2秒冷却；T 整理技能，保留当前配置");BuildChapterActions();RequestCheckpoint();break;
                case 6:chapterArcKnown=true;TellStory("巡路记录里画着两枚相连的铜环：电光越过一处，还能借着余势，抵达另一处。","少年沿铜环轻轻一划，两点微光便接成了短暂的雷弧。");AnnounceDiscovery("已领悟跃电 · 10元素，3秒冷却；T 整理技能，保留当前配置");BuildChapterActions();RequestCheckpoint();break;
                case 7:EnterExplorationRoom(0,-1);TellStory("雷云越过山脊，巡路员沿修好的古道回到了灯棚。","靠近灯匠按 F，迎接归家的人。");RequestCheckpoint();break;
                case 9:chapterSkillsMenu=true;break;
                case 10:case 11:case 12:
                    chapterWaymarks|=1<<(a.kind-10);if(chapterWaymarks==7)n.claimed=true;BuildChapterActions();RequestCheckpoint();JourneyHint(chapterWaymarks==7?"三块路标都已扶正，回程的方向清楚了":"路标已扶正");break;
                case 13:
                    if(n.claimed){TellStory("棚梁稳住了，归来的驮兽可以在这里避雨。");break;}chapterFocusedAction=13;chapterWork=1.2f;break;
                case 14:
                    if(chapterLampCarried||chapterLampGiven){JourneyHint("灯罩已经收好，不占武器格");break;}
                    chapterLampCarried=true;n.claimed=true;TellStory("油布里包着修好的灯罩。纸签写着：给山口那盏。","你收好灯罩，准备回程交给灯匠。任务物不占武器背包。");RequestCheckpoint();BuildChapterActions();break;
                case 15:n.claimed=true;TellStory("远处山口的灯仍亮着。巡路员的旧记录写着：两人出发，一人留灯。","少年：山下的灯还在。我们也快些。");RequestCheckpoint();break;
                case 208:EmbarkWoodland();break;
                case 16:ReturnToDesertCheckpoint();break;
            }
            return true;
        }
        private void FinishChapterWork()
        {
            RoomNode n=journeyRooms[currentRoom];chapterWork=0;
            if(chapterFocusedAction==13){n.claimed=true;TellStory("你扶正松动的棚梁，雨不再灌进歇脚的地方。牧人开始铺下干草。");RequestCheckpoint();return;}
            if(chapterFocusedAction==3)chapterWest=true;else chapterEast=true;
            thunderRepaired=true;ClearThunderHazards();RefreshThunderRepair();
            if(!n.lootSeeded){n.lootSeeded=true;var crystal=new StoredEssence{at=FindFreeInteractionPoint(ThunderLeverAt+Vector2.down),value=20,born=clock};n.essence.Add(crystal);DrawEssence(crystal);}
            TellStory(chapterFocusedAction==3?"少年：两条铜线接回去了。雷流沿边槽走，中间的山路可以通行。":"旧路灯依次亮起，山巅却仍有电光。少年：路上的雷散了，源头还没停。");
            UpdateExplorationDoors();BuildChapterActions();RequestCheckpoint();
        }
        private int[] ChapterRewardChoices()
        {return currentRoom==11?new[]{6,9}:currentRoom==24?new[]{10,12}:new[]{11,13};}
        private void ClaimChapterWeapon(int choice)
        {
            RoomNode n=journeyRooms[currentRoom];if(n.claimed)return;int[] ids=ChapterRewardChoices();
            n.claimed=n.lootSeeded=true;chapterRewardMenu=false;ChapterDrop(ids[Mathf.Clamp(choice,0,1)],position+Vector2.right*.8f);
            if(rewardChest!=null)Destroy(rewardChest);rewardChest=null;RequestCheckpoint();JourneyHint("装备已放在身旁 · F 拾取，满背包时可替换或留地");
        }
        private bool TickChapterOverlay(float dt)
        {
            if(!thunderChapterMode)return false;
            if(chapterSetup)
            {
                if(Pressed('1'))thunderPreset=ThunderPreset.Light;if(Pressed('2'))thunderPreset=ThunderPreset.Dark;
                if(Pressed('a'))thunderChapterStartFloor=(thunderChapterStartFloor+2)%3;if(Pressed('d'))thunderChapterStartFloor=(thunderChapterStartFloor+1)%3;
                if(chapterConfirmRestart){if(Pressed('e'))chapterConfirmRestart=false;return true;}
                if(Pressed('f')){if(HasChapterSave)LoadChapterCheckpoint(true);else StartChapterFromSetup();}return true;
            }
            if(chapterCg)
            {if(Pressed('f')||Pressed('e'))FinishChapterEnding();return true;}
            if(chapterWork>0)
            {
                if(Pressed('e')){chapterWork=0;JourneyHint("已取消检修，未消耗资源");return true;}
                chapterWork=Mathf.Max(0,chapterWork-Mathf.Min(dt,.05f));if(chapterWork<=0)FinishChapterWork();return true;
            }
            if(chapterRewardMenu)
            {if(Pressed('a')||Pressed('d'))chapterRewardChoice=1-chapterRewardChoice;if(Pressed('f'))ClaimChapterWeapon(chapterRewardChoice);if(Pressed('e'))chapterRewardMenu=false;return true;}
            if(chapterSkillsMenu){if(Pressed('b')||Pressed('e'))chapterSkillsMenu=false;return true;}
            return false;
        }
        private void FinishChapterEnding()
        {chapterCg=false;chapterEnding=true;BuildChapterActions();RequestCheckpoint();SaveChapterCheckpoint(false);}
        private void DrawChapterSetup(float w,float h)
        {
            HudRect(new Rect(0,0,w,h),new Color(.02f,.04f,.06f,.94f));CenterLabel(new Rect(w/2-420,75,840,45),woodlandMode?"青栖林海 · 不再发芽的种子":"雷雨高原 · 困在山巅的雷声",journeyTitleStyle);
            CenterLabel(new Rect(w/2-430,125,860,30),HasChapterSave?"已有"+CurrentIsland.name+"安全存档 · F 继续原进度":"尚无本岛存档 · 选择起点开始旅程",journeyTextStyle);
            bool backgroundEnabled=GUI.enabled;if(chapterConfirmRestart)GUI.enabled=false;
            for(int i=0;i<2;i++)if(FlatButton(new Rect(w/2-290+i*300,181,280,65),i==0?"1 光之力 · 元气弹":"2 暗之力 · 流星",(int)thunderPreset==i))thunderPreset=(ThunderPreset)i;
            for(int i=0;i<3;i++)
            {
                int floor=i;
                if(FlatButton(new Rect(w/2-430+i*290,277,275,73),"从第"+(i+1)+"层开始\n"+(woodlandMode?WoodlandStages:ThunderStages)[i],thunderChapterStartFloor==i)){thunderChapterStartFloor=floor;}
                if(FlatButton(new Rect(w/2-430+i*290,375,275,73),"直接练习\n"+(woodlandMode?new[]{"棘甲母虫","菌冠孢囊","古树守望者"}:new[]{"守路铜卫","引雷石卫长","巡雷巨鹿"})[i])){thunderChapterPracticeBoss=i;chapterSetup=false;BeginThunderChapter(false);}
            }
            if(FlatButton(new Rect(w/2-285,485,270,60),"开始选定楼层（新旅程）"))StartChapterFromSetup();
            bool enabled=GUI.enabled;GUI.enabled=enabled&&HasChapterSave;
            if(FlatButton(new Rect(w/2+15,485,270,60),"F 继续本岛存档",HasChapterSave))LoadChapterCheckpoint(true);GUI.enabled=enabled;
            CenterLabel(new Rect(w/2-420,565,840,60),"A / D 选楼层 · F 优先继续存档 · N 回到此页\n清房与 Boss 后自动保存；F5 手动保存，F9 读取",journeySmallStyle);
            GUI.enabled=backgroundEnabled;
            if(chapterConfirmRestart)
            {
                HudRect(new Rect(w/2-360,220,720,260),C("10232d"));CenterLabel(new Rect(w/2-330,240,660,90),"已有本岛进度。确认开始新旅程？\n原存档会先保留一份历史副本。",journeyTextStyle);
                if(FlatButton(new Rect(w/2-315,365,300,65),"保留进度，继续存档")){chapterConfirmRestart=false;LoadChapterCheckpoint(true);}
                if(FlatButton(new Rect(w/2+15,365,300,65),"确认另起旅程"))StartChapterFromSetup(true);
            }
        }
        private void DrawChapterCg(float w,float h)
        {
            HudRect(new Rect(0,0,w,h),Color.black);if(chapterEndingArt!=null)GUI.DrawTexture(new Rect(0,0,w,h),chapterEndingArt,ScaleMode.ScaleToFit);
            HudRect(new Rect(0,h-130,w,130),new Color(.02f,.04f,.06f,.88f));CenterLabel(new Rect(30,h-119,w-60,36),"灯下有归人",journeyTitleStyle);
            CenterLabel(new Rect(30,h-78,w-60,32),chapterLampGiven?"灯匠：灯没白亮。巡路员：路也没白修。":"少年：路回来了，就会有人回家。",journeyTextStyle);
            GUI.Label(new Rect(30,h-37,600,28),"F / ESC 继续 · 雷元素碎片已保存",journeySmallStyle);
        }
        private void DrawChapterReward(float w,float h)
        {
            HudRect(new Rect(w/2-360,h/2-155,720,310),C("10232d"));CenterLabel(new Rect(w/2-330,h/2-138,660,36),"巡路装备箱 · 选择一件",journeyTitleStyle);
            int[] ids=ChapterRewardChoices();for(int i=0;i<2;i++){WeaponDef weapon=weaponCatalog[ids[i]];if(FlatButton(new Rect(w/2-330+i*340,h/2-77,320,145),weapon.name+"\n"+WeaponSummary(weapon)+"\n间隔 "+weapon.interval+"秒",chapterRewardChoice==i))ClaimChapterWeapon(i);}
            CenterLabel(new Rect(w/2-330,h/2+97,660,32),"A / D 选择 · F 确认 · ESC 返回",journeySmallStyle);
        }
    }
}
