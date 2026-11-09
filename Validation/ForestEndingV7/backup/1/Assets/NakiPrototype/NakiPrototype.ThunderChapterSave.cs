using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NakiLegend.Prototype
{
    public sealed partial class NakiPrototype
    {
        [Serializable] private sealed class ChapterRoomState
        {
            public bool cleared,visited,discovered,claimed,loot;public bool bossWeaponGranted,challengeWon,departureArmoryClaimed;public int coverBroken;public int[] supplyOffers;
            public CheckpointWeapon[] weapons;public CheckpointEssence[] crystals;
        }
        [Serializable] private sealed class ChapterCheckpoint
        {
            public int version=1,seed,room,origin,waymarks,shortcuts;
            public int routeVersion,islandOrdinal;
            public string island="thunder_plateau",map="d3-v1";
            public ThunderCheckpoint player;
            // Optional in older saves: false schedules one recovery replay of a completed Ice finale.
            public bool iceCgSeen,woodCgSeen;
            public bool west,east,ranger,stone,ending,cgPending,mark,arc,lampCarried,lampGiven,orb,blade,wave,wind;
            public float woodShield;public bool fireHeart,fireHearth;public int fireEvac;
            public bool forgeRescued,forgeBridge,emberRing;public float emberReady;public bool iceSpike,iceRing,fireBolt;public float iceSpikeReady,iceRingReady,fireReady,fireShield=7;
            public float markReady,arcReady,swordBonus,spellBonus;
            public int[] skills;public ChapterRoomState[] rooms;
        }
        private bool chapterSavePending;
        private bool chapterConfirmRestart;
        private bool HasChapterSave { get { return (!Application.isBatchMode||allowBatchCheckpoints)&&(File.Exists(ChapterCheckpointPath)||File.Exists(ChapterCheckpointPath+".bak")); } }
        private void StartChapterFromSetup(bool replace=false)
        {
            if(HasChapterSave&&!replace){chapterConfirmRestart=true;return;}
            if(HasChapterSave)
            {
                string archive=ChapterCheckpointPath+"."+DateTime.UtcNow.ToString("yyyyMMddHHmmssfff")+".previous";
                try
                {
                    if(File.Exists(ChapterCheckpointPath))File.Copy(ChapterCheckpointPath,archive,false);
                    if(File.Exists(ChapterCheckpointPath+".bak"))File.Copy(ChapterCheckpointPath+".bak",archive+".bak",false);
                }
                catch(Exception){JourneyHint("原存档未能备份，已保留当前进度，请稍后再试");return;}
            }
            chapterConfirmRestart=false;thunderChapterPracticeBoss=-1;chapterSetup=false;BeginThunderChapter(false);
        }
        private string chapterSaveStatus="尚无本岛存档",chapterCheckpointPathOverride=null;
        // Native regression voyages use a workspace directory, never the player's real saves.
        private string chapterBatchSaveDirectory;
        private string ChapterCheckpointPath { get { return string.IsNullOrEmpty(chapterCheckpointPathOverride)?Path.Combine(Application.isBatchMode&&!string.IsNullOrEmpty(chapterBatchSaveDirectory)?chapterBatchSaveDirectory:Path.Combine(Application.persistentDataPath,"NakiLegend"),CurrentIsland.saveKey):chapterCheckpointPathOverride; } }
        private bool ChapterDiskEnabled { get { return thunderChapterMode&&thunderChapterPracticeBoss<0&&(!Application.isBatchMode||allowBatchCheckpoints); } }
        private bool SafeChapterCheckpoint { get { return thunderChapterMode&&!chapterSetup&&chapterWork<=0&&chapterMarks.Count==0&&thunderStrikes.Count==0&&SafeCheckpointRoom; } }
        private ThunderCheckpoint CaptureChapterPlayer()
        {
            return new ThunderCheckpoint{loadout=CaptureSkillLoadout(),originSecond=originSecondReady,originThird=originThirdReady,lightRegen=lightRegenReady,returnReady=returnBladeReady,guardReady=guardPulseReady,talents=CopyTalents(),profession=(int)profession,lore=originLoreBits,knowledge=originKnowledge,primary=primarySkill,queuedLore=queuedOriginChapter,routeOrigin=(int)origin,preset=(int)thunderPreset,room=currentRoom,equipped=equippedSlot,healing=healingSupplies,elementSupplies=elementSupplies,
                clock=clock,health=health,shield=shield,light=energy,dark=darkPower,element=elementPower,position=position,
                carried=(int[])carriedWeapons.Clone(),reserves=reserveWeapons.ToArray(),nextSword=nextSword,nextSpell=nextSpell,nextDash=nextDash,
                bladeReady=bladeReadyAt,waveReady=waveReadyAt,lastDamage=lastDamageAt};
        }
        private bool SaveChapterCheckpoint(bool notify)
        {
            if(!ChapterDiskEnabled){if(notify)JourneyHint("Boss 练习不覆盖本岛旅程存档");return false;}
            if(!SafeChapterCheckpoint){if(notify)JourneyHint("请在清房后保存；战斗和未完成机关保留上一个安全点");return false;}
            try
            {
                var s=new ChapterCheckpoint{woodCgSeen=woodEndingCgSeen,woodShield=woodlandShieldFloor,iceCgSeen=iceEndingCgSeen,routeVersion=2,islandOrdinal=CurrentIsland.ordinal,island=ChapterIslandKey,map=ChapterMapKey,seed=activeSeed,room=currentRoom,origin=(int)origin,waymarks=chapterWaymarks,shortcuts=chapterShortcuts,player=CaptureChapterPlayer(),
                    west=chapterWest,east=chapterEast,ranger=chapterRanger,stone=chapterStone,ending=chapterEnding,cgPending=chapterCg,mark=chapterMarkKnown,arc=chapterArcKnown,
                    lampCarried=chapterLampCarried,lampGiven=chapterLampGiven,orb=orbLearned,blade=bladeLearned,wave=waveLearned,wind=windEmpowered,
                    fireHeart=fireHeartRecovered,fireHearth=fireHearthLit,fireEvac=fireEvacuation,forgeRescued=forgeRescued,forgeBridge=forgeBridgeOpen,emberRing=emberRingKnown,emberReady=emberRingReady,fireBolt=fireBoltKnown,fireReady=fireBoltReady,fireShield=fireShieldFloor,iceSpike=iceSpikeKnown,iceRing=iceRingKnown,iceSpikeReady=iceSpikeReady,iceRingReady=iceRingReady,markReady=chapterMarkReady,arcReady=chapterArcReady,swordBonus=swordDamage-initialSwordDamage,spellBonus=spellDamage-initialSpellDamage,
                    skills=(int[])chapterSkillSlots.Clone(),rooms=new ChapterRoomState[ChapterRoomCount]};
                for(int i=0;i<(ChapterRoomCount);i++)
                {
                    RoomNode n=journeyRooms[i];var weapons=new List<CheckpointWeapon>();foreach(StoredWeapon w in n.drops)weapons.Add(new CheckpointWeapon{id=w.id,at=w.at});
                    var crystals=new List<CheckpointEssence>();foreach(StoredEssence e in n.essence)crystals.Add(new CheckpointEssence{at=e.at,value=e.value,born=e.born});
                    s.rooms[i]=new ChapterRoomState{cleared=n.cleared,visited=n.visited,discovered=n.discovered,claimed=n.claimed,loot=n.lootSeeded,departureArmoryClaimed=n.departureArmoryClaimed,bossWeaponGranted=n.bossWeaponGranted,challengeWon=n.challengeWon,coverBroken=n.coverBroken,supplyOffers=n.supplyOffers,weapons=weapons.ToArray(),crystals=crystals.ToArray()};
                }
                ValidateChapterCheckpoint(s);string path=ChapterCheckpointPath;Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllText(path+".tmp",JsonUtility.ToJson(s,true));
                if(File.Exists(path))File.Replace(path+".tmp",path,path+".bak");else File.Move(path+".tmp",path);
                RecordJourneySession();chapterSavePending=false;chapterSaveStatus="已存 · "+ActiveLayerName(journeyRooms[currentRoom].stage)+" · "+chapterNames[currentRoom]+(manorCourtyardMode?(chapterStone?" · 卫长已卸武":" · 外庭探索中"):fireChapterMode?(chapterEnding?" · 已通关":fireHearthLit?" · 炉火已点燃":fireHeartRecovered?" · 火石已寻回":journeyRooms[27].cleared?" · 工坊已安定":fireBoltKnown?" · 火种已学习":chapterStone?" · 熔蜥已击退":""):chapterEnding?iceSampleMode?(iceFullChapter?" · 已通关":" · 样板完成"):" · 已通关":chapterStone?iceSampleMode?(iceFullChapter?" · 冰石已寻回":" · 甲兽已击退"):woodlandMode?" · 古树已解救":" · 巨鹿已击败":"");if(notify)JourneyHint(CurrentIsland.name+"已保存 · F9 读取");return true;
            }
            catch(Exception e){chapterSaveStatus="本岛存档写入失败";if(notify)JourneyHint(chapterSaveStatus);Debug.LogWarning("Thunder chapter save: "+e.Message);return false;}
        }
        private void ValidateChapterCheckpoint(ChapterCheckpoint s)
        {
            if(woodlandMode)AddWoodWeapons();ValidateTalents(s==null||s.player==null?null:s.player.talents);if(s==null||s.version!=1||s.island!=ChapterIslandKey||s.map!=ChapterMapKey||s.seed<1||s.room<0||s.room>=(ChapterRoomCount)||s.origin<1||s.origin>2||s.rooms==null||s.rooms.Length!=(ChapterRoomCount)||s.player==null)throw new InvalidDataException("Invalid chapter checkpoint");
            ThunderCheckpoint p=s.player;ValidateSkillLoadout(p.loadout);
            if(!FiniteRange(s.woodShield,0,8))throw new InvalidDataException("Invalid inherited woodland shield");
            if(p.carried==null||p.carried.Length!=2||p.reserves==null||p.reserves.Length>4||p.equipped<0||p.equipped>1||p.carried[p.equipped]<0||p.preset<0||p.preset>2)throw new InvalidDataException("Invalid chapter inventory");
            foreach(int id in p.carried)if(id< -1||id>=weaponCatalog.Length)throw new InvalidDataException("Invalid equipped weapon");
            foreach(int id in p.reserves)if(id<0||id>=weaponCatalog.Length)throw new InvalidDataException("Invalid reserve weapon");
            if(!ValidOriginLegacy(p.lore,p.knowledge,p.primary,p.queuedLore))throw new InvalidDataException("Invalid origin memory");
            if(!FiniteRange(p.clock,0,1e9f)||!FiniteRange(p.health,.01f,6)||!FiniteRange(p.shield,0,fireChapterMode?Mathf.Max(maximumShield+2,s.fireShield,s.rooms[27]!=null&&s.rooms[27].cleared?8:0):iceSampleMode&&iceFullChapter?maximumShield+3:woodlandMode?Mathf.Max(maximumShield+2,s.woodShield)+((s.rooms.Length>18&&s.rooms[18]!=null&&s.rooms[18].cleared)?1:0):EffectiveMaximumShield)||!FiniteRange(p.light,0,100)||!ValidDarkPower(p.dark)||!FiniteRange(p.element,0,SavedElementMaximum(p.talents))||!FiniteRange(p.position.x,-15,15)||!FiniteRange(p.position.y,-10,10)||p.healing<0||p.elementSupplies<0)throw new InvalidDataException("Invalid chapter resources");
            foreach(float ready in new[]{p.originSecond,p.originThird,p.lightRegen,p.nextSword,p.nextSpell,p.nextDash,p.bladeReady,p.waveReady,p.lastDamage,s.markReady,s.arcReady,s.iceSpikeReady,s.iceRingReady,s.fireReady,s.emberReady,p.returnReady,p.guardReady})if(!FiniteRange(ready,-100,p.clock+1000))throw new InvalidDataException("Invalid saved cooldown: "+ready+" / clock "+p.clock);
            if(s.skills==null||s.skills.Length!=2||!ValidSavedSkill(s.skills[0],p.talents)||!ValidSavedSkill(s.skills[1],p.talents)||s.skills[0]==s.skills[1]||s.waymarks<0||s.waymarks>7||s.shortcuts<0||s.shortcuts>30||!FiniteRange(s.swordBonus,0,10000)||!FiniteRange(s.spellBonus,0,10000))throw new InvalidDataException("Invalid chapter skills");
            foreach(ChapterRoomState n in s.rooms)
            {
                if(n==null||n.coverBroken<0||n.coverBroken>3||!ValidSupplyOffers(n.supplyOffers)||n.weapons==null||n.crystals==null)throw new InvalidDataException("Invalid room state");
                foreach(CheckpointWeapon w in n.weapons)if(w==null||w.id<0||w.id>=weaponCatalog.Length||!FiniteRange(w.at.x,-15,15)||!FiniteRange(w.at.y,-10,10))throw new InvalidDataException("Invalid ground equipment");
                foreach(CheckpointEssence e in n.crystals)if(e==null||!FiniteRange(e.value,.01f,100)||!FiniteRange(e.born,0,p.clock)||!FiniteRange(e.at.x,-15,15)||!FiniteRange(e.at.y,-10,10))throw new InvalidDataException("Invalid crystal");
            }
            // Learned skills travel between islands. They do not assert that this island's rescue/boss quests are complete.
            bool inheritedFire=p.loadout!=null&&(p.loadout.known&64)!=0,inheritedEmber=p.loadout!=null&&(p.loadout.known&128)!=0;
            if(fireChapterMode&&(s.forgeRescued&&(!s.fireBolt||!s.stone)||s.forgeBridge&&!s.forgeRescued||s.emberRing&&!s.forgeRescued&&!inheritedEmber||s.rooms[27].cleared&&(!s.forgeRescued||!s.forgeBridge)))throw new InvalidDataException("Incomplete forge evacuation route");
            if(fireChapterMode&&(!FiniteRange(s.fireShield,6,8)||s.fireBolt&&!s.stone&&!inheritedFire||s.stone&&!s.ranger))throw new InvalidDataException("Invalid Fire Coast progression");
            if(fireChapterMode&&(s.fireHeart!=s.rooms[42].cleared||s.fireHeart&&(!s.rooms[27].cleared||!s.rooms[12].cleared)||s.fireEvac<0||s.fireEvac>3||s.fireEvac!=0&&!s.fireHeart||(s.fireEvac&1)!=0&&!s.rooms[43].cleared||(s.fireEvac&2)!=0&&((s.fireEvac&1)==0||!s.rooms[44].cleared)||s.fireHearth&&(!s.fireHeart||s.fireEvac!=3)||(s.ending||s.cgPending)&&!s.fireHearth))throw new InvalidDataException("Incomplete Fire Core ending");
            if(woodlandMode&&((s.ending||s.cgPending)&&(!s.stone||!s.lampGiven)||s.lampGiven&&!s.stone||s.woodCgSeen&&!s.ending))throw new InvalidDataException("Incomplete woodland ending");
            if(iceSampleMode&&iceFullChapter&&((s.west||s.east)&&!s.stone||s.lampCarried&&(!s.west||!s.east||!s.ranger)||((s.ending||s.cgPending)&&(!s.west||!s.east||!s.ranger||!s.lampCarried))))throw new InvalidDataException("Incomplete ice ending tasks");
            if(!s.rooms[s.room].cleared||s.stone!=s.rooms[FinalChapterBossRoom].cleared||s.ending&&!s.stone||s.cgPending&&!s.stone||s.lampCarried&&s.lampGiven)throw new InvalidDataException("Invalid chapter progression");
            ValidateManorCheckpoint(s);
        }
        private bool LoadChapterCheckpoint(bool notify)
        {
            if(!thunderChapterMode||Application.isBatchMode&&!allowBatchCheckpoints)return false;ChapterCheckpoint s=null;
            foreach(string path in new[]{ChapterCheckpointPath,ChapterCheckpointPath+".bak"})
            {
                if(!File.Exists(path))continue;try{var candidate=JsonUtility.FromJson<ChapterCheckpoint>(File.ReadAllText(path));MigrateIceSampleCheckpoint(candidate);MigrateFireCheckpoint(candidate);MigrateWoodlandCheckpoint(candidate);MigrateLegacySkillLoadout(candidate);ValidateChapterCheckpoint(candidate);s=candidate;break;}catch(Exception){}
            }
            if(s==null){if(notify)JourneyHint("没有完整的本岛安全存档，当前状态保持原样");return false;}
            thunderChapterPracticeBoss=-1;thunderChapterStartFloor=0;thunderPreset=(ThunderPreset)s.player.preset;chapterSetup=false;activeSeed=s.seed;if(fireChapterMode)fireShieldFloor=s.fireShield;if(woodlandMode)woodlandShieldFloor=s.woodShield;BeginThunderChapter(false);
            for(int i=0;i<(ChapterRoomCount);i++)
            {
                RoomNode n=journeyRooms[i];ChapterRoomState saved=s.rooms[i];n.cleared=saved.cleared;n.visited=saved.visited;n.discovered=saved.discovered;n.claimed=saved.claimed;n.lootSeeded=saved.loot;n.departureArmoryClaimed=saved.departureArmoryClaimed;n.bossWeaponGranted=saved.bossWeaponGranted;n.challengeWon=saved.challengeWon;n.coverBroken=saved.coverBroken;n.supplyOffers=saved.supplyOffers;n.drops.Clear();n.essence.Clear();
                foreach(CheckpointWeapon w in saved.weapons)n.drops.Add(new StoredWeapon{id=w.id,at=w.at});foreach(CheckpointEssence e in saved.crystals)n.essence.Add(new StoredEssence{at=e.at,value=e.value,born=e.born});
            }
            ApplyChapterPlayer(s.player);origin=(Origin)s.origin;NormalizeOriginPower();chapterWest=s.west;chapterEast=s.east;chapterRanger=s.ranger;chapterStone=s.stone;chapterEnding=s.ending;iceEndingCgSeen=s.iceCgSeen;woodEndingCgSeen=s.woodCgSeen;
            chapterMarkKnown=s.mark;chapterArcKnown=s.arc;chapterWaymarks=s.waymarks;chapterShortcuts=s.shortcuts;chapterLampCarried=s.lampCarried;chapterLampGiven=s.lampGiven;
            fireHeartRecovered=s.fireHeart;fireHearthLit=s.fireHearth;fireEvacuation=s.fireEvac;forgeRescued=s.forgeRescued;forgeBridgeOpen=s.forgeBridge;emberRingKnown=s.emberRing;emberRingReady=s.emberReady;fireBoltKnown=s.fireBolt;fireBoltReady=s.fireReady;iceSpikeKnown=s.iceSpike;iceRingKnown=s.iceRing;iceSpikeReady=s.iceSpikeReady;iceRingReady=s.iceRingReady;orbLearned=s.orb;bladeLearned=s.blade;waveLearned=s.wave;windEmpowered=s.wind;chapterMarkReady=s.markReady;chapterArcReady=s.arcReady;
            Array.Copy(s.skills,chapterSkillSlots,2);if(s.player.loadout!=null)RestoreSkillLoadout(s.player.loadout);else if(woodlandMode){if(s.mark)learnedElementSkills|=1<<10;if(s.arc)learnedElementSkills|=1<<11;forestRootReady=s.markReady;forestSporeReady=s.arcReady;}swordDamage=initialSwordDamage+s.swordBonus;spellDamage=initialSpellDamage+s.spellBonus;
            EnterExplorationRoom(s.room,-1);position=NearestSafeFooting(s.player.position);UpdatePixelHero(Vector2.zero);storyLines=null;chapterCg=s.cgPending||(woodlandMode&&s.ending&&!woodEndingCgSeen)||(iceSampleMode&&iceFullChapter&&s.ending&&!iceEndingCgSeen);chapterSavePending=false;
            if(manorCourtyardMode){manorIntro=!s.lampGiven;manorIntroBegan=Time.unscaledTime;}
            chapterSaveStatus="已读 · "+ActiveLayerName(journeyRooms[currentRoom].stage)+" · "+chapterNames[currentRoom];JourneyHint("已继续"+CurrentIsland.name+"旅程");return true;
        }
        private void ApplyChapterPlayer(ThunderCheckpoint p)
        {
            RestoreTalents(p.talents);RestoreOriginLegacy(p.lore,p.knowledge,p.primary,p.queuedLore);professionFromSave=true;profession=(Profession)Mathf.Clamp(p.profession,0,3);
            clock=p.clock;originSecondReady=p.originSecond;originThirdReady=p.originThird;lightRegenReady=p.lightRegen;repairTalentReady=clock+8;returnBladeReady=p.returnReady;guardPulseReady=p.guardReady;health=p.health;shield=p.shield;energy=p.light;darkPower=p.dark;elementPower=p.element;healingSupplies=p.healing;elementSupplies=p.elementSupplies;
            equippedSlot=p.equipped;Array.Copy(p.carried,carriedWeapons,2);reserveWeapons.Clear();reserveWeapons.AddRange(p.reserves);
            nextSword=p.nextSword;nextSpell=p.nextSpell;nextDash=p.nextDash;bladeReadyAt=p.bladeReady;waveReadyAt=p.waveReady;lastDamageAt=p.lastDamage;RestoreSkillLoadout(p.loadout);
        }
        private bool HandleChapterSaveKeys()
        {
            if(!thunderChapterMode||chapterSetup)return false;bool save=false,load=false;
#if ENABLE_INPUT_SYSTEM
            if(Keyboard.current!=null){save=Keyboard.current.f5Key.wasPressedThisFrame;load=Keyboard.current.f9Key.wasPressedThisFrame;}
#elif ENABLE_LEGACY_INPUT_MANAGER
            save=Input.GetKeyDown(KeyCode.F5);load=Input.GetKeyDown(KeyCode.F9);
#endif
            if(save){SaveChapterCheckpoint(true);return true;}if(load){LoadChapterCheckpoint(true);return true;}return false;
        }
        private void EmbarkThunderChapter()
        {
            if(!canalEpilogueComplete||!SafeCheckpointRoom)return;SaveCheckpoint(false);
            ThunderCheckpoint carried=CaptureChapterPlayer();Origin oldOrigin=origin;bool oldOrb=orbLearned,oldBlade=bladeLearned,oldWave=waveLearned;
            float oldSword=swordDamage-initialSwordDamage,oldSpell=spellDamage-initialSpellDamage;
            thunderChapterMode=true;thunderPrototypeMode=false;thunderChapterPracticeBoss=-1;thunderChapterStartFloor=0;chapterSetup=false;
            LoadThunderArt();thunderPreset=oldOrigin==Origin.Dark?ThunderPreset.Dark:ThunderPreset.Light;
            if(LoadVoyageChapter(carried.talents,carried))return;
            BeginThunderChapter(false);ApplyChapterPlayer(carried);origin=oldOrigin;NormalizeOriginPower();orbLearned=oldOrb;bladeLearned=oldBlade;waveLearned=oldWave;
            swordDamage=initialSwordDamage+oldSword;spellDamage=initialSpellDamage+oldSpell;shield=Mathf.Min(EffectiveMaximumShield,shield+1);
            EnterExplorationRoom(0,-1);TellStory("船靠上湿石码头。你将风石的微光引进护具，纹路沿着肩甲亮起，又慢慢隐入衣襟。","山口有一盏暖灯，雷声却始终停在同一座山头。少年收紧行装，走向守灯的人。");AnnounceDiscovery("风石共鸣 · 护盾上限 +1，技能基础威力 +15%");RequestCheckpoint();
        }
        private void ReturnToDesertCheckpoint()
        {
            var journeyPlayer=CaptureChapterPlayer();int[] carriedTalents=CopyTalents();SaveChapterCheckpoint(false);thunderChapterMode=thunderPrototypeMode=false;chapterSetup=chapterCg=chapterSkillsMenu=false;
            BeginJourney();if(!LoadCheckpoint(false))JourneyHint("没有完整沙漠存档，已回到村口");MergeOriginJourney(journeyPlayer);MergeVoyageTalents(carriedTalents);
        }
        private static readonly Vector2 ThunderDockAt=new Vector2(.8f,.8f);
        private void EnsureThunderDock()
        {
            if(!GatehouseActive||gatehouseSampleMode||!canalEpilogueComplete||journeyRooms[currentRoom].gatehouseIndex!=4)return;
            if(world.Find("Boat to Thunder Plateau")!=null)return;
            GameObject dock=new GameObject("Boat to Thunder Plateau");dock.transform.SetParent(world,false);
            dock.transform.position=ThunderDockAt; // Bell, boardwalk and floating boat are authored into the quay image.
            transitReservations.Add(new Rect(ThunderDockAt-Vector2.one,new Vector2(2,2)));
        }
    }
}
