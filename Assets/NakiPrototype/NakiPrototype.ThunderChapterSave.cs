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
            public bool cleared,visited,discovered,claimed,loot;
            public CheckpointWeapon[] weapons;public CheckpointEssence[] crystals;
        }
        [Serializable] private sealed class ChapterCheckpoint
        {
            public int version=1,seed,room,origin,waymarks,shortcuts;
            public string island="thunder_plateau",map="d3-v1";
            public ThunderCheckpoint player;
            public bool west,east,ranger,stone,ending,cgPending,mark,arc,lampCarried,lampGiven,orb,blade,wave,wind;
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
        private string chapterSaveStatus="尚无第二岛存档",chapterCheckpointPathOverride=null;
        private string ChapterCheckpointPath { get { return string.IsNullOrEmpty(chapterCheckpointPathOverride)?Path.Combine(Application.persistentDataPath,"NakiLegend",woodlandMode?"woodland-chapter-v1.json":"thunder-chapter-v1.json"):chapterCheckpointPathOverride; } }
        private bool ChapterDiskEnabled { get { return thunderChapterMode&&thunderChapterPracticeBoss<0&&(!Application.isBatchMode||allowBatchCheckpoints); } }
        private bool SafeChapterCheckpoint { get { return thunderChapterMode&&!chapterSetup&&chapterWork<=0&&chapterMarks.Count==0&&thunderStrikes.Count==0&&SafeCheckpointRoom; } }
        private ThunderCheckpoint CaptureChapterPlayer()
        {
            return new ThunderCheckpoint{profession=(int)profession,preset=(int)thunderPreset,room=currentRoom,equipped=equippedSlot,healing=healingSupplies,elementSupplies=elementSupplies,
                clock=clock,health=health,shield=shield,light=energy,dark=darkPower,element=elementPower,position=position,
                carried=(int[])carriedWeapons.Clone(),reserves=reserveWeapons.ToArray(),nextSword=nextSword,nextSpell=nextSpell,nextDash=nextDash,
                bladeReady=bladeReadyAt,waveReady=waveReadyAt,lastDamage=lastDamageAt};
        }
        private bool SaveChapterCheckpoint(bool notify)
        {
            if(!ChapterDiskEnabled){if(notify)JourneyHint("Boss 练习不覆盖第二岛旅程存档");return false;}
            if(!SafeChapterCheckpoint){if(notify)JourneyHint("请在清房后保存；战斗和未完成机关保留上一个安全点");return false;}
            try
            {
                var s=new ChapterCheckpoint{island=woodlandMode?"woodland":"thunder_plateau",map=woodlandMode?"w3-v1":"d3-v1",seed=activeSeed,room=currentRoom,origin=(int)origin,waymarks=chapterWaymarks,shortcuts=chapterShortcuts,player=CaptureChapterPlayer(),
                    west=chapterWest,east=chapterEast,ranger=chapterRanger,stone=chapterStone,ending=chapterEnding,cgPending=chapterCg,mark=chapterMarkKnown,arc=chapterArcKnown,
                    lampCarried=chapterLampCarried,lampGiven=chapterLampGiven,orb=orbLearned,blade=bladeLearned,wave=waveLearned,wind=windEmpowered,
                    markReady=chapterMarkReady,arcReady=chapterArcReady,swordBonus=swordDamage-initialSwordDamage,spellBonus=spellDamage-initialSpellDamage,
                    skills=(int[])chapterSkillSlots.Clone(),rooms=new ChapterRoomState[woodlandMode?45:39]};
                for(int i=0;i<(woodlandMode?45:39);i++)
                {
                    RoomNode n=journeyRooms[i];var weapons=new List<CheckpointWeapon>();foreach(StoredWeapon w in n.drops)weapons.Add(new CheckpointWeapon{id=w.id,at=w.at});
                    var crystals=new List<CheckpointEssence>();foreach(StoredEssence e in n.essence)crystals.Add(new CheckpointEssence{at=e.at,value=e.value,born=e.born});
                    s.rooms[i]=new ChapterRoomState{cleared=n.cleared,visited=n.visited,discovered=n.discovered,claimed=n.claimed,loot=n.lootSeeded,weapons=weapons.ToArray(),crystals=crystals.ToArray()};
                }
                ValidateChapterCheckpoint(s);string path=ChapterCheckpointPath;Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllText(path+".tmp",JsonUtility.ToJson(s,true));
                if(File.Exists(path))File.Replace(path+".tmp",path,path+".bak");else File.Move(path+".tmp",path);
                chapterSavePending=false;chapterSaveStatus="已存 · 第"+(journeyRooms[currentRoom].stage+1)+"层 · "+chapterNames[currentRoom]+(chapterEnding?" · 已通关":chapterStone?" · 巨鹿已击败":"");if(notify)JourneyHint("第二岛旅程已保存 · F9 读取");return true;
            }
            catch(Exception e){chapterSaveStatus="第二岛存档写入失败";if(notify)JourneyHint(chapterSaveStatus);Debug.LogWarning("Thunder chapter save: "+e.Message);return false;}
        }
        private void ValidateChapterCheckpoint(ChapterCheckpoint s)
        {
            if(s==null||s.version!=1||s.island!=(woodlandMode?"woodland":"thunder_plateau")||s.map!=(woodlandMode?"w3-v1":"d3-v1")||s.seed<1||s.room<0||s.room>=(woodlandMode?45:39)||s.origin<1||s.origin>2||s.rooms==null||s.rooms.Length!=(woodlandMode?45:39)||s.player==null)throw new InvalidDataException("Invalid chapter checkpoint");
            ThunderCheckpoint p=s.player;
            if(p.carried==null||p.carried.Length!=2||p.reserves==null||p.reserves.Length>4||p.equipped<0||p.equipped>1||p.carried[p.equipped]<0||p.preset<0||p.preset>2)throw new InvalidDataException("Invalid chapter inventory");
            foreach(int id in p.carried)if(id< -1||id>=weaponCatalog.Length)throw new InvalidDataException("Invalid equipped weapon");
            foreach(int id in p.reserves)if(id<0||id>=weaponCatalog.Length)throw new InvalidDataException("Invalid reserve weapon");
            if(!FiniteRange(p.clock,0,1e9f)||!FiniteRange(p.health,.01f,6)||!FiniteRange(p.shield,0,EffectiveMaximumShield)||!FiniteRange(p.light,0,100)||!FiniteRange(p.dark,0,100)||!FiniteRange(p.element,0,100)||!FiniteRange(p.position.x,-15,15)||!FiniteRange(p.position.y,-10,10)||p.healing<0||p.elementSupplies<0)throw new InvalidDataException("Invalid chapter resources");
            foreach(float ready in new[]{p.nextSword,p.nextSpell,p.nextDash,p.bladeReady,p.waveReady,p.lastDamage,s.markReady,s.arcReady})if(!FiniteRange(ready,-100,p.clock+1000))throw new InvalidDataException("Invalid saved cooldown");
            if(s.skills==null||s.skills.Length!=2||s.skills[0]<0||s.skills[0]>3||s.skills[1]<0||s.skills[1]>3||s.skills[0]==s.skills[1]||s.waymarks<0||s.waymarks>7||s.shortcuts<0||s.shortcuts>30||!FiniteRange(s.swordBonus,0,10000)||!FiniteRange(s.spellBonus,0,10000))throw new InvalidDataException("Invalid chapter skills");
            foreach(ChapterRoomState n in s.rooms)
            {
                if(n==null||n.weapons==null||n.crystals==null)throw new InvalidDataException("Invalid room state");
                foreach(CheckpointWeapon w in n.weapons)if(w==null||w.id<0||w.id>=weaponCatalog.Length||!FiniteRange(w.at.x,-15,15)||!FiniteRange(w.at.y,-10,10))throw new InvalidDataException("Invalid ground equipment");
                foreach(CheckpointEssence e in n.crystals)if(e==null||!FiniteRange(e.value,.01f,100)||!FiniteRange(e.born,0,p.clock)||!FiniteRange(e.at.x,-15,15)||!FiniteRange(e.at.y,-10,10))throw new InvalidDataException("Invalid crystal");
            }
            if(!s.rooms[s.room].cleared||s.stone!=s.rooms[31].cleared||s.ending&&!s.stone||s.cgPending&&!s.stone||s.lampCarried&&s.lampGiven)throw new InvalidDataException("Invalid chapter progression");
        }
        private bool LoadChapterCheckpoint(bool notify)
        {
            if(!thunderChapterMode||Application.isBatchMode&&!allowBatchCheckpoints)return false;ChapterCheckpoint s=null;
            foreach(string path in new[]{ChapterCheckpointPath,ChapterCheckpointPath+".bak"})
            {
                if(!File.Exists(path))continue;try{var candidate=JsonUtility.FromJson<ChapterCheckpoint>(File.ReadAllText(path));ValidateChapterCheckpoint(candidate);s=candidate;break;}catch(Exception){}
            }
            if(s==null){if(notify)JourneyHint("没有完整的第二岛安全存档，当前状态保持原样");return false;}
            thunderChapterPracticeBoss=-1;thunderChapterStartFloor=0;thunderPreset=(ThunderPreset)s.player.preset;chapterSetup=false;activeSeed=s.seed;BeginThunderChapter(false);
            for(int i=0;i<(woodlandMode?45:39);i++)
            {
                RoomNode n=journeyRooms[i];ChapterRoomState saved=s.rooms[i];n.cleared=saved.cleared;n.visited=saved.visited;n.discovered=saved.discovered;n.claimed=saved.claimed;n.lootSeeded=saved.loot;n.drops.Clear();n.essence.Clear();
                foreach(CheckpointWeapon w in saved.weapons)n.drops.Add(new StoredWeapon{id=w.id,at=w.at});foreach(CheckpointEssence e in saved.crystals)n.essence.Add(new StoredEssence{at=e.at,value=e.value,born=e.born});
            }
            ApplyChapterPlayer(s.player);origin=(Origin)s.origin;NormalizeOriginPower();chapterWest=s.west;chapterEast=s.east;chapterRanger=s.ranger;chapterStone=s.stone;chapterEnding=s.ending;
            chapterMarkKnown=s.mark;chapterArcKnown=s.arc;chapterWaymarks=s.waymarks;chapterShortcuts=s.shortcuts;chapterLampCarried=s.lampCarried;chapterLampGiven=s.lampGiven;
            orbLearned=s.orb;bladeLearned=s.blade;waveLearned=s.wave;windEmpowered=s.wind;chapterMarkReady=s.markReady;chapterArcReady=s.arcReady;
            Array.Copy(s.skills,chapterSkillSlots,2);swordDamage=initialSwordDamage+s.swordBonus;spellDamage=initialSpellDamage+s.spellBonus;
            EnterExplorationRoom(s.room,-1);position=NearestSafeFooting(s.player.position);UpdatePixelHero(Vector2.zero);storyLines=null;chapterCg=s.cgPending;chapterSavePending=false;
            chapterSaveStatus="已读 · "+chapterNames[currentRoom];JourneyHint("已继续第二岛旅程");return true;
        }
        private void ApplyChapterPlayer(ThunderCheckpoint p)
        {
            profession=(Profession)Mathf.Clamp(p.profession,0,3);
            clock=p.clock;health=p.health;shield=p.shield;energy=p.light;darkPower=p.dark;elementPower=p.element;healingSupplies=p.healing;elementSupplies=p.elementSupplies;
            equippedSlot=p.equipped;Array.Copy(p.carried,carriedWeapons,2);reserveWeapons.Clear();reserveWeapons.AddRange(p.reserves);
            nextSword=p.nextSword;nextSpell=p.nextSpell;nextDash=p.nextDash;bladeReadyAt=p.bladeReady;waveReadyAt=p.waveReady;lastDamageAt=p.lastDamage;
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
            if(LoadChapterCheckpoint(false))return;
            BeginThunderChapter(false);ApplyChapterPlayer(carried);origin=oldOrigin;NormalizeOriginPower();orbLearned=oldOrb;bladeLearned=oldBlade;waveLearned=oldWave;
            swordDamage=initialSwordDamage+oldSword;spellDamage=initialSpellDamage+oldSpell;shield=Mathf.Min(EffectiveMaximumShield,shield+1);
            EnterExplorationRoom(0,-1);TellStory("船靠上湿石码头。你将风石的微光引进护具，纹路沿着肩甲亮起，又慢慢隐入衣襟。","山口有一盏暖灯，雷声却始终停在同一座山头。少年收紧行装，走向守灯的人。");AnnounceDiscovery("风石共鸣 · 护盾上限 +1，技能基础威力 +15%");RequestCheckpoint();
        }
        private void ReturnToDesertCheckpoint()
        {
            SaveChapterCheckpoint(false);thunderChapterMode=thunderPrototypeMode=false;chapterSetup=chapterCg=chapterSkillsMenu=false;
            BeginJourney();if(!LoadCheckpoint(false))JourneyHint("没有完整沙漠存档，已回到村口");
        }
        private static readonly Vector2 ThunderDockAt=new Vector2(-4.8f,-1.5f);
        private void EnsureThunderDock()
        {
            if(!GatehouseActive||gatehouseSampleMode||!canalEpilogueComplete||journeyRooms[currentRoom].gatehouseIndex!=4)return;
            if(world.Find("Boat to Thunder Plateau")!=null)return;
            GameObject dock=new GameObject("Boat to Thunder Plateau");dock.transform.SetParent(world,false);
            GameObject bell=Draw("Highland departure windbell",lessonArtwork,ThunderDockAt,Vector2.one,Color.white,DepthAt(ThunderDockAt.y));bell.transform.SetParent(dock.transform,true);
            transitReservations.Add(new Rect(ThunderDockAt-Vector2.one,new Vector2(2,2)));
        }
    }
}
