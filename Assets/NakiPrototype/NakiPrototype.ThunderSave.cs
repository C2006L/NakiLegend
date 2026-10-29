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
        [Serializable] private sealed class ThunderRoomSave { public bool cleared;public CheckpointWeapon[] drops;public CheckpointEssence[] essence; }
        [Serializable] private sealed class ThunderCheckpoint
        {
            public int profession;
            public int version=1,preset,room,equipped,healing,elementSupplies;
            public float clock,health,shield,light,dark,element,nextSword,nextSpell,nextDash,bladeReady,waveReady,lastDamage;
            public bool repair,reward;public Vector2 position;public int[] carried,reserves;public ThunderRoomSave[] rooms;
        }
        private bool thunderSavePending;
        private string thunderCheckpointPathOverride=null,thunderSaveStatus="尚无安全原型存档";
        private string ThunderCheckpointPath { get { return string.IsNullOrEmpty(thunderCheckpointPathOverride)?Path.Combine(Application.persistentDataPath,"NakiLegend","thunder-prototype-v1.json"):thunderCheckpointPathOverride; } }
        private bool ThunderDiskEnabled { get { return thunderPrototypeMode&&(!Application.isBatchMode||allowBatchCheckpoints); } }
        private bool SafeThunderCheckpoint { get { return thunderPrototypeMode&&!thunderSetupMenu&&SafeCheckpointRoom&&thunderWorkRemaining<=0&&thunderStrikes.Count==0; } }
        private bool SaveThunderCheckpoint(bool notify)
        {
            if(!ThunderDiskEnabled)return false;
            if(!SafeThunderCheckpoint){if(notify)JourneyHint("原型请在清场后保存；战斗不覆盖最近安全存档");return false;}
            try
            {
                var s=new ThunderCheckpoint{profession=(int)profession,preset=(int)thunderPreset,room=currentRoom,equipped=equippedSlot,healing=healingSupplies,elementSupplies=elementSupplies,
                    clock=clock,health=health,shield=shield,light=energy,dark=darkPower,element=elementPower,position=position,repair=thunderRepaired,reward=thunderRewardCreated,
                    carried=(int[])carriedWeapons.Clone(),reserves=reserveWeapons.ToArray(),nextSword=nextSword,nextSpell=nextSpell,nextDash=nextDash,
                    bladeReady=bladeReadyAt,waveReady=waveReadyAt,lastDamage=lastDamageAt,rooms=new ThunderRoomSave[2]};
                for(int i=0;i<2;i++)
                {
                    var weapons=new List<CheckpointWeapon>();foreach(var w in journeyRooms[i].drops)weapons.Add(new CheckpointWeapon{id=w.id,at=w.at});
                    var crystals=new List<CheckpointEssence>();foreach(var e in journeyRooms[i].essence)crystals.Add(new CheckpointEssence{at=e.at,value=e.value,born=e.born});
                    s.rooms[i]=new ThunderRoomSave{cleared=journeyRooms[i].cleared,drops=weapons.ToArray(),essence=crystals.ToArray()};
                }
                ValidateThunderCheckpoint(s);string path=ThunderCheckpointPath;Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path+".tmp",JsonUtility.ToJson(s,true));
                if(File.Exists(path))File.Replace(path+".tmp",path,path+".bak");else File.Move(path+".tmp",path);
                thunderSavePending=false;thunderSaveStatus="已存 "+ThunderTitle;if(notify)JourneyHint("原型已保存 · F9 继续，不影响沙漠旅程");return true;
            }
            catch(Exception e){thunderSaveStatus="原型存档写入失败";if(notify)JourneyHint(thunderSaveStatus);Debug.LogWarning("Thunder checkpoint: "+e.Message);return false;}
        }
        private void ValidateThunderCheckpoint(ThunderCheckpoint s)
        {
            if(s==null||s.version!=1||s.preset<0||s.preset>2||s.room<0||s.room>1||s.rooms==null||s.rooms.Length!=2||s.rooms[s.room]==null||!s.rooms[s.room].cleared)throw new InvalidDataException("Invalid prototype progress");
            if(s.carried==null||s.carried.Length!=2||s.reserves==null||s.reserves.Length>4||s.equipped<0||s.equipped>1||s.carried[s.equipped]<0)throw new InvalidDataException("Invalid prototype equipment");
            foreach(int w in s.carried)if(w< -1||w>=weaponCatalog.Length)throw new InvalidDataException("Invalid weapon");
            foreach(int w in s.reserves)if(w<0||w>=weaponCatalog.Length)throw new InvalidDataException("Invalid reserve");
            if(!FiniteRange(s.clock,0,1e9f)||!FiniteRange(s.health,.01f,6)||!FiniteRange(s.shield,0,maximumShield)||!FiniteRange(s.light,0,100)||!FiniteRange(s.dark,0,100)||!FiniteRange(s.element,0,100)||!FiniteRange(s.position.x,-12,12)||!FiniteRange(s.position.y,-8,8)||s.healing<0||s.elementSupplies<0)throw new InvalidDataException("Invalid prototype resources");
            foreach(float ready in new[]{s.nextSword,s.nextSpell,s.nextDash,s.bladeReady,s.waveReady,s.lastDamage})if(!FiniteRange(ready,-100,s.clock+1000))throw new InvalidDataException("Invalid cooldown");
            foreach(var room in s.rooms)
            {
                if(room==null||room.drops==null||room.essence==null)throw new InvalidDataException("Invalid room state");
                foreach(var w in room.drops)if(w==null||w.id<0||w.id>=weaponCatalog.Length||!FiniteRange(w.at.x,-12,12)||!FiniteRange(w.at.y,-8,8))throw new InvalidDataException("Invalid drop");
                foreach(var e in room.essence)if(e==null||!FiniteRange(e.value,.01f,100)||!FiniteRange(e.at.x,-12,12)||!FiniteRange(e.at.y,-8,8)||!FiniteRange(e.born,0,s.clock))throw new InvalidDataException("Invalid crystal");
            }
            if(s.repair!=s.reward||s.repair&&!s.rooms[1].cleared)throw new InvalidDataException("Invalid repair reward");
        }
        private bool LoadThunderCheckpoint(bool notify)
        {
            if(!ThunderDiskEnabled)return false;ThunderCheckpoint s=null;
            foreach(string path in new[]{ThunderCheckpointPath,ThunderCheckpointPath+".bak"})
            {
                if(!File.Exists(path))continue;
                try{var candidate=JsonUtility.FromJson<ThunderCheckpoint>(File.ReadAllText(path));ValidateThunderCheckpoint(candidate);s=candidate;break;}catch(Exception){}
            }
            if(s==null){if(notify)JourneyHint("没有完整的安全原型存档；当前状态保持原样");return false;}
            BeginThunderSample((ThunderPreset)s.preset,s.room);
            for(int i=0;i<2;i++)
            {
                RoomNode n=journeyRooms[i];n.cleared=s.rooms[i].cleared;n.visited=n.discovered=n.cleared;n.drops.Clear();n.essence.Clear();
                foreach(var w in s.rooms[i].drops)n.drops.Add(new StoredWeapon{id=w.id,at=w.at});
                foreach(var e in s.rooms[i].essence)n.essence.Add(new StoredEssence{at=e.at,value=e.value,born=e.born});
            }
            professionFromSave=true;profession=(Profession)Mathf.Clamp(s.profession,0,3);clock=s.clock;health=s.health;shield=s.shield;energy=s.light;darkPower=s.dark;elementPower=s.element;
            healingSupplies=s.healing;elementSupplies=s.elementSupplies;equippedSlot=s.equipped;Array.Copy(s.carried,carriedWeapons,2);reserveWeapons.Clear();reserveWeapons.AddRange(s.reserves);
            thunderRepaired=s.repair;thunderRewardCreated=s.reward;nextSword=s.nextSword;nextSpell=s.nextSpell;nextDash=s.nextDash;bladeReadyAt=s.bladeReady;waveReadyAt=s.waveReady;lastDamageAt=s.lastDamage;
            EnterExplorationRoom(s.room,-1);position=NearestSafeFooting(s.position);UpdatePixelHero(Vector2.zero);storyLines=null;thunderSavePending=false;
            thunderSaveStatus="已读 "+ThunderTitle;JourneyHint("已继续独立原型存档");return true;
        }
        private bool HandleThunderCheckpointKeys()
        {
            if(!thunderPrototypeMode||thunderSetupMenu)return false;bool save=false,load=false;
#if ENABLE_INPUT_SYSTEM
            if(Keyboard.current!=null){save=Keyboard.current.f5Key.wasPressedThisFrame;load=Keyboard.current.f9Key.wasPressedThisFrame;}
#elif ENABLE_LEGACY_INPUT_MANAGER
            save=Input.GetKeyDown(KeyCode.F5);load=Input.GetKeyDown(KeyCode.F9);
#endif
            if(save){SaveThunderCheckpoint(true);return true;}if(load){LoadThunderCheckpoint(true);return true;}return false;
        }
    }
}
