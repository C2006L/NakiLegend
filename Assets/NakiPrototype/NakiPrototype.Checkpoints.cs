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
        [Serializable] private sealed class CheckpointRoom
        {
            public int id,stage,depth,layout,floor,waves,stairsTo,gatehouseIndex,kind;
            public Vector2Int cell;public float halfWidth,halfHeight;
            public bool visited,discovered,cleared,claimed,lootSeeded;
            public int[] links;public CheckpointWeapon[] drops;public CheckpointEssence[] essence;
        }
        [Serializable] private sealed class CheckpointWeapon { public int id;public Vector2 at; }
        [Serializable] private sealed class CheckpointEssence { public Vector2 at;public float value,born; }
        [Serializable] private sealed class JourneyCheckpoint
        {
            public int version=1,seed,selectedBoss,room,origin,equipped,clearedRooms,restored,healSupplies,elementSupplies,canalStart;
            public float clock,health,shield,light,element,dark,swordDamage,baseSword,spellDamage,baseSpell;
            public float nextSword,nextSpell,nextDash,bladeReady,waveReady,lastDamage;
            public bool orb,blade,wave,wind,stone,repair,reward,total,rain,canalComplete,complete,endingPending;
            public bool[] sand;public Vector2 position;public int[] carried,reserves,starts,ends;
            public CheckpointRoom[] rooms;
        }
        private bool checkpointPending,loadingCheckpoint;
        // Editor verification opts in using a workspace-owned path; batch runs never
        // read or write the player's real checkpoint by default.
        private bool allowBatchCheckpoints=false;
        private string checkpointPathOverride=null,checkpointStatus="尚未存档";
        private bool CheckpointsEnabled { get { return enableDesertJourney&&!ThunderActive&&!gatehouseSampleMode&&!bossPracticeMode&&!lightningTrialMode&&(!Application.isBatchMode||allowBatchCheckpoints); } }
        private string CheckpointPath { get { return string.IsNullOrEmpty(checkpointPathOverride)?Path.Combine(Application.persistentDataPath,"NakiLegend","journey-v1.json"):checkpointPathOverride; } }
        private void RequestCheckpoint(){if(thunderChapterMode){chapterSavePending=true;return;}if(thunderPrototypeMode){thunderSavePending=true;return;}if(!loadingCheckpoint&&CheckpointsEnabled&&origin!=Origin.Unchosen)checkpointPending=true;}
        private bool SafeCheckpointRoom { get { return world!=null&&journeyRooms.Count>currentRoom&&currentRoom>=0&&journeyRooms[currentRoom].cleared&&health>0&&enemies.Count==0&&pendingSpawns.Count==0&&gateWorkRemaining<=0&&doorTransition<=0; } }
        private void TickCheckpoint(){if(checkpointPending&&SafeCheckpointRoom){checkpointPending=false;SaveCheckpoint(false);}}
        private bool SaveCheckpoint(bool notify)
        {
            if(!CheckpointsEnabled||loadingCheckpoint||origin==Origin.Unchosen||!SafeCheckpointRoom)
            {if(notify)JourneyHint("请在已清理的安全房间存档；战斗中会保留上一个存档点");return false;}
            try
            {
                var s=new JourneyCheckpoint {
                    seed=activeSeed,selectedBoss=selectedBoss,room=currentRoom,origin=(int)origin,equipped=equippedSlot,
                    clearedRooms=clearedCombatRooms,restored=restoredCanals,healSupplies=healingSupplies,elementSupplies=elementSupplies,
                    canalStart=canalEpilogueStart,clock=clock,health=health,shield=shield,light=energy,element=elementPower,dark=darkPower,
                    swordDamage=swordDamage,baseSword=initialSwordDamage,spellDamage=spellDamage,baseSpell=initialSpellDamage,
                    nextSword=nextSword,nextSpell=nextSpell,nextDash=nextDash,bladeReady=bladeReadyAt,waveReady=waveReadyAt,lastDamage=lastDamageAt,
                    orb=orbLearned,blade=bladeLearned,wave=waveLearned,wind=windEmpowered,stone=desertStoneRecovered,
                    repair=gateRepairCommitted,reward=gateRewardCreated,total=gateTotalUnlocked,rain=gateRainReturned,
                    canalComplete=canalEpilogueComplete,complete=journeyComplete,endingPending=pendingRainCG||showRainCG,
                    sand=(bool[])gateSandCleared.Clone(),position=position,carried=(int[])carriedWeapons.Clone(),
                    reserves=reserveWeapons.ToArray(),starts=(int[])stageStarts.Clone(),ends=(int[])stageEnds.Clone()
                };
                var rooms=new List<CheckpointRoom>();
                foreach(RoomNode n in journeyRooms)
                {
                    var drops=new List<CheckpointWeapon>();foreach(StoredWeapon w in n.drops)drops.Add(new CheckpointWeapon{id=w.id,at=w.at});
                    var essence=new List<CheckpointEssence>();foreach(StoredEssence d in n.essence)essence.Add(new CheckpointEssence{at=d.at,value=d.value,born=d.born});
                    rooms.Add(new CheckpointRoom{id=n.id,stage=n.stage,depth=n.depth,layout=n.layout,floor=n.floor,waves=n.waves,stairsTo=n.stairsTo,gatehouseIndex=n.gatehouseIndex,kind=(int)n.kind,cell=n.cell,halfWidth=n.halfWidth,halfHeight=n.halfHeight,visited=n.visited,discovered=n.discovered,cleared=n.cleared,claimed=n.claimed,lootSeeded=n.lootSeeded,links=n.links.ToArray(),drops=drops.ToArray(),essence=essence.ToArray()});
                }
                s.rooms=rooms.ToArray();ValidateCheckpoint(s);
                string path=CheckpointPath;Directory.CreateDirectory(Path.GetDirectoryName(path));
                string temp=path+".tmp";File.WriteAllText(temp,JsonUtility.ToJson(s,true));
                if(File.Exists(path))
                {
                    try{File.Replace(temp,path,path+".bak");}
                    catch(PlatformNotSupportedException){File.Copy(path,path+".bak",true);File.Copy(temp,path,true);File.Delete(temp);}
                }
                else File.Move(temp,path);
                checkpointStatus="已存："+JourneyRoomTitle(journeyRooms[currentRoom]);checkpointPending=false;
                if(notify)JourneyHint("旅程已保存 · F9 可回到这个安全房间");return true;
            }
            catch(Exception e){checkpointStatus="存档失败";if(notify)JourneyHint("存档未能写入，请检查文件权限");Debug.LogWarning("Naki checkpoint save: "+e.Message);return false;}
        }
        private static bool FiniteRange(float n,float lo,float hi){return !float.IsNaN(n)&&!float.IsInfinity(n)&&n>=lo&&n<=hi;}
        private void ValidateCheckpoint(JourneyCheckpoint s)
        {
            if(s==null||s.version!=1||s.rooms==null||(s.rooms.Length!=39&&s.rooms.Length!=44)||s.room<0||s.room>=s.rooms.Length||s.origin<1||s.origin>2||s.selectedBoss<0||s.selectedBoss>2)throw new InvalidDataException("Unsupported checkpoint");
            if(s.carried==null||s.carried.Length!=2||s.reserves==null||s.reserves.Length>4||s.equipped<0||s.equipped>1||s.carried[s.equipped]<0||s.starts==null||s.starts.Length!=3||s.ends==null||s.ends.Length!=3||s.sand==null||s.sand.Length!=2)throw new InvalidDataException("Invalid inventory or map");
            foreach(int w in s.carried)if(w< -1||w>=weaponCatalog.Length)throw new InvalidDataException("Invalid weapon");
            foreach(int w in s.reserves)if(w<0||w>=weaponCatalog.Length)throw new InvalidDataException("Invalid reserve");
            if(!FiniteRange(s.health,.01f,6)||!FiniteRange(s.shield,0,maximumShield)||!FiniteRange(s.light,0,100)||!FiniteRange(s.element,0,100)||!FiniteRange(s.dark,0,100)||!FiniteRange(s.clock,0,1e9f)||!FiniteRange(s.swordDamage,0,10000)||!FiniteRange(s.spellDamage,0,10000)||s.healSupplies<0||s.elementSupplies<0)throw new InvalidDataException("Invalid resource values");
            if(!FiniteRange(s.position.x,-100,100)||!FiniteRange(s.position.y,-100,100))throw new InvalidDataException("Invalid player position");
            for(int i=0;i<s.rooms.Length;i++)
            {
                var n=s.rooms[i];
                if(n==null||n.id!=i||n.stage<0||n.stage>3||n.kind<0||n.kind>7||n.waves<1||n.waves>4||n.links==null||n.drops==null||n.essence==null||!FiniteRange(n.halfWidth,5,12)||!FiniteRange(n.halfHeight,3,8)||n.stairsTo< -1||n.stairsTo>=s.rooms.Length||n.gatehouseIndex!=(i>=39?i-39:-1))throw new InvalidDataException("Invalid room definition");
                if(n.layout<0||n.layout>63||n.depth<0||(i<39?n.stage>2:n.stage!=3))throw new InvalidDataException("Invalid room theme");
                foreach(int link in n.links)
                {
                    if(link<0||link>=s.rooms.Length)throw new InvalidDataException("Invalid map link");
                    var other=s.rooms[link];
                    if(other==null||other.stage!=n.stage||other.links==null||!Array.Exists(other.links,id=>id==i)||Mathf.Abs(n.cell.x-other.cell.x)+Mathf.Abs(n.cell.y-other.cell.y)!=1)throw new InvalidDataException("Invalid connected doorway");
                }
                foreach(var drop in n.drops)if(drop.id<0||drop.id>=weaponCatalog.Length)throw new InvalidDataException("Invalid ground weapon");
                foreach(var drop in n.essence)if(!FiniteRange(drop.value,.01f,100))throw new InvalidDataException("Invalid crystal");
            }
            foreach(int id in s.starts)if(id<0||id>=s.rooms.Length)throw new InvalidDataException("Invalid entrance");
            foreach(int id in s.ends)if(id<0||id>=s.rooms.Length)throw new InvalidDataException("Invalid end room");
            if(!s.rooms[s.room].cleared||(s.rooms.Length==44? s.canalStart!=39||!s.stone:s.canalStart!=-1))throw new InvalidDataException("Invalid checkpoint progression");
        }
        private bool LoadCheckpoint(bool notify)
        {
            if(!CheckpointsEnabled)return false;
            JourneyCheckpoint s=null;
            foreach(string file in new[]{CheckpointPath,CheckpointPath+".bak"})
            {
                if(!File.Exists(file))continue;
                try{var candidate=JsonUtility.FromJson<JourneyCheckpoint>(File.ReadAllText(file));ValidateCheckpoint(candidate);s=candidate;break;}
                catch(Exception){checkpointStatus="存档损坏，尝试备份";}
            }
            if(s==null){if(notify)JourneyHint("没有可读取的完整存档，当前旅程保持原状");return false;}
            loadingCheckpoint=true;
            try
            {
                StopOpeningCinematic();ResetPlayerProgression();journeyRooms.Clear();
                foreach(var n in s.rooms)
                {
                    var room=new RoomNode{id=n.id,stage=n.stage,depth=n.depth,layout=n.layout,floor=n.floor,waves=n.waves,stairsTo=n.stairsTo,gatehouseIndex=n.gatehouseIndex,kind=(RoomKind)n.kind,cell=n.cell,halfWidth=n.halfWidth,halfHeight=n.halfHeight,visited=n.visited,discovered=n.discovered,cleared=n.cleared,claimed=n.claimed,lootSeeded=n.lootSeeded};
                    room.links.AddRange(n.links);foreach(var w in n.drops)room.drops.Add(new StoredWeapon{id=w.id,at=w.at});foreach(var d in n.essence)room.essence.Add(new StoredEssence{at=d.at,value=d.value,born=d.born});journeyRooms.Add(room);
                }
                activeSeed=s.seed;selectedBoss=s.selectedBoss;origin=(Origin)s.origin;equippedSlot=s.equipped;
                clearedCombatRooms=s.clearedRooms;restoredCanals=s.restored;healingSupplies=s.healSupplies;elementSupplies=s.elementSupplies;
                canalEpilogueStart=s.canalStart;clock=s.clock;health=s.health;shield=s.shield;energy=s.light;elementPower=s.element;darkPower=s.dark;NormalizeOriginPower();
                swordDamage=s.swordDamage;initialSwordDamage=s.baseSword;spellDamage=s.spellDamage;initialSpellDamage=s.baseSpell;tuningCaptured=true;
                nextSword=s.nextSword;nextSpell=s.nextSpell;nextDash=s.nextDash;bladeReadyAt=s.bladeReady;waveReadyAt=s.waveReady;lastDamageAt=s.lastDamage;
                orbLearned=s.orb;bladeLearned=s.blade;waveLearned=s.wave;windEmpowered=s.wind;desertStoneRecovered=s.stone;
                gateRepairCommitted=s.repair;gateRewardCreated=s.reward;gateTotalUnlocked=s.total;gateRainReturned=s.rain;
                canalEpilogueComplete=s.canalComplete;journeyComplete=s.complete;Array.Copy(s.sand,gateSandCleared,2);
                Array.Copy(s.carried,carriedWeapons,2);reserveWeapons.Clear();reserveWeapons.AddRange(s.reserves);Array.Copy(s.starts,stageStarts,3);Array.Copy(s.ends,stageEnds,3);
                gateWorkRemaining=0;gateWorkKind=-1;flashUntil=0;doorTransition=0;
                EnterExplorationRoom(s.room,-1);position=NearestSafeFooting(s.position);UpdatePixelHero(Vector2.zero);storyLines=null;
                if(s.endingPending&&DesertEndingReady){TellStory("雨云越过山口，旧渠重新响起水声。","甘泽归沙 · 第一瓢水，留给树根。");pendingRainCG=true;}
                checkpointPending=false;checkpointStatus="已读："+JourneyRoomTitle(journeyRooms[currentRoom]);JourneyHint("已继续上次旅程 · N 开始新旅程");return true;
            }
            finally{loadingCheckpoint=false;}
        }
        private bool HandleCheckpointKeys()
        {
            if(HandleChapterSaveKeys()||HandleThunderCheckpointKeys())return true;
            if(!CheckpointsEnabled)return false;bool save=false,load=false;
#if ENABLE_INPUT_SYSTEM
            if(Keyboard.current!=null){save=Keyboard.current.f5Key.wasPressedThisFrame;load=Keyboard.current.f9Key.wasPressedThisFrame;}
#elif ENABLE_LEGACY_INPUT_MANAGER
            save=Input.GetKeyDown(KeyCode.F5);load=Input.GetKeyDown(KeyCode.F9);
#endif
            if(save){SaveCheckpoint(true);return true;}if(load){LoadCheckpoint(true);return true;}return false;
        }
        private void TryResumeAtStartup(){if(CheckpointsEnabled&&File.Exists(CheckpointPath))LoadCheckpoint(false);}
        private void OnApplicationQuit(){if(thunderChapterMode){SaveChapterCheckpoint(false);return;}if(thunderPrototypeMode){SaveThunderCheckpoint(false);return;}if(CheckpointsEnabled&&SafeCheckpointRoom)SaveCheckpoint(false);}
    }
}
