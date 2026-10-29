using System;
using System.Collections.Generic;
using UnityEngine;

namespace NakiLegend.Prototype
{
    public sealed partial class NakiPrototype
    {
        [Serializable] private sealed class ChapterArtRecord { public string key="";public float ppu=100;public ThunderFrame[] frames=Array.Empty<ThunderFrame>(); }
        [Serializable] private sealed class ChapterArtManifest { public ChapterArtRecord[] records=Array.Empty<ChapterArtRecord>(); }
        private readonly Dictionary<string,Sprite[]> chapterArt=new Dictionary<string,Sprite[]>();
        private readonly Sprite[] chapterFloors=new Sprite[3];
        private bool chapterSkillsMenu;
        private readonly int[] chapterSkillSlots={0,1};
        private float chapterMarkReady,chapterArcReady;
        private sealed class ChapterMark { public Vector2 at;public float when;public GameObject root; }
        private readonly List<ChapterMark> chapterMarks=new List<ChapterMark>();
        private static readonly string[] ChapterSkillNames={"风之刃","护身风波","雷标","跃电"};
        private void LoadChapterActors()
        {
            chapterArt.Clear();TextAsset text=Resources.Load<TextAsset>("NakiArt/ThunderChapter/art");if(text==null)return;
            ChapterArtManifest m=JsonUtility.FromJson<ChapterArtManifest>(text.text);
            foreach(ChapterArtRecord record in m.records)
            {
                Texture2D t=Resources.Load<Texture2D>("NakiArt/ThunderChapter/"+record.key);if(t==null)continue;t.filterMode=FilterMode.Point;
                Sprite[] frames=new Sprite[record.frames.Length];
                for(int i=0;i<frames.Length;i++){ThunderFrame f=record.frames[i];frames[i]=Sprite.Create(t,new Rect(f.x,t.height-f.y-f.height,f.width,f.height),new Vector2(f.pivotX,f.pivotY),record.ppu,0,SpriteMeshType.FullRect);ownedAssets.Add(frames[i]);}
                chapterArt[record.key]=frames;
            }
            string[] keys={"roller","sentinel","warden","sentinel","stag"};
            EnemyKind[] kinds={EnemyKind.CopperRoller,EnemyKind.ThunderStone,EnemyKind.CopperWarden,EnemyKind.ThunderSentinel,EnemyKind.StormStag};
            for(int i=0;i<keys.Length;i++){Sprite[] a;if(chapterArt.TryGetValue(keys[i],out a))enemyActionArt[kinds[i]]=a;}
            chapterFloors[0]=thunderFloor;
            chapterFloors[1]=ChapterImage("floor2",1);chapterFloors[2]=ChapterImage("floor3",1);
        }
        private Sprite ChapterImage(string key,float height)
        {Sprite[] a;return chapterArt.TryGetValue(key,out a)&&a.Length>0?a[0]:null;}
        private void LoadThunderChapterEquipment()
        {
            if(!thunderChapterMode||weaponCatalog==null||weaponCatalog.Length>8)return;
            var items=new List<WeaponDef>(weaponCatalog);
            items.Add(Weapon("巡路手弩",WeaponType.Bow,Element.Physical,12,.65f,10,0,1.15f,ChapterImage("crossbow",1)??weaponCatalog[2].sprite,"ffffff"));
            items.Add(Weapon("铜环雷杖",WeaponType.Staff,Element.Lightning,14,.7f,10,2,1.4f,ChapterImage("staff",1)??weaponCatalog[6].sprite,"ffffff"));
            items.Add(Weapon("裂雷短戟",WeaponType.Spear,Element.Lightning,22,.55f,1.95f,1,1.6f,ChapterImage("fork",1)??weaponCatalog[1].sprite,"ffffff"));
            items.Add(Weapon("沉铜破阵锤",WeaponType.Sword,Element.Lightning,32,.95f,1.6f,3,1.1f,ChapterImage("hammer",1)??weaponCatalog[5].sprite,"ffffff"));
            items.Add(Weapon("鸣弦长弓",WeaponType.Bow,Element.Lightning,20,.9f,11,2,1.0f,ChapterImage("bow",1)??weaponCatalog[2].sprite,"ffffff"));
            items.Add(Weapon("岚纹细剑",WeaponType.Sword,Element.Wind,16,.3f,1.3f,1,1.1f,ChapterImage("rapier",1)??weaponCatalog[4].sprite,"ffffff"));
            weaponCatalog=items.ToArray();weaponCatalog[8].scatter=4;weaponCatalog[9].scatter=1;weaponCatalog[12].scatter=.6f;weaponCatalog[11].maxHits=2;
        }
        private bool ChapterSkillKnown(int skill)
        {return skill==0?bladeLearned:skill==1?waveLearned:skill==2?chapterMarkKnown:chapterArcKnown;}
        private float ChapterSkillReady(int skill){return skill==0?bladeReadyAt:skill==1?waveReadyAt:skill==2?chapterMarkReady:chapterArcReady;}
        private float ChapterSkillCost(int skill){return skill==0?6:skill==1?10:woodlandMode?(skill==2?7:9):skill==2?8:10;}
        private void CastChapterSkill(int slot)
        {
            if(woodlandMode){CastWoodSkill(slot);return;}
            int skill=chapterSkillSlots[slot];if(!ChapterSkillKnown(skill)){JourneyHint("这项元素技能尚未学会");return;}
            if(skill==0){CastWindBlade();return;}if(skill==1){CastWindWave();return;}
            if(clock<ChapterSkillReady(skill)||clock<dashUntil)return;
            Enemy target;Vector2 aim=AutoAim(9,9,0,out target);if(target==null){JourneyHint("视线内没有可锁定的敌人");return;}
            float cost=ChapterSkillCost(skill);if(elementPower+.001f<ElementCost(cost)){JourneyHint("元素之力不足 · 拾取结晶或使用元素药剂");return;}
            if(skill==2)
            {
                if(!SpendPower(PowerPool.Elemental,cost))return;chapterMarkReady=clock+2;SkillSound("mark","cast");
                Vector2 at=target.position;GameObject circle=WarningCircle(at,chapterStone?.75f:.6f);
                foreach(SpriteRenderer r in circle.GetComponentsInChildren<SpriteRenderer>())r.color=new Color(.62f,.86f,1,.6f);
                chapterMarks.Add(new ChapterMark{at=at,when=clock+.65f,root=circle});
            }
            else
            {
                int count=shots.Count;SpawnShot(position+aim*.45f,aim,false,null,position);if(shots.Count==count)return;
                SpendPower(PowerPool.Elemental,cost);chapterArcReady=clock+3;SkillSound("arc","cast");
                Shot shot=shots[shots.Count-1];shot.tracking=false;shot.element=Element.Lightning;shot.damage=20;shot.speed=9;shot.life=1.4f;shot.chapterArc=true;shot.skillAudio="arc";
                shot.damage=SkillDamage(20);shot.root.GetComponent<SpriteRenderer>().color=C("a8eaff");
                AddShotRibbon(shot,C("a8eaff"),.055f);SpellSpark(position+aim*.45f,C("a8eaff"),.22f);
            }
            castDirection=aim;castPoseUntil=clock+.3f;
        }
        private void TickChapterSkills()
        {
            if(!thunderChapterMode)return;
            for(int i=chapterMarks.Count-1;i>=0;i--)
            {
                ChapterMark mark=chapterMarks[i];if(clock<mark.when)continue;int hits=0;SkillSound("mark","hit");
                GameObject flash=Stretch("Friendly mark lightning",square,mark.at+Vector2.up*.65f,new Vector2(.035f,1.3f),C("c6f1ff"),570);
                fadingBodies.Add(new FadingBody{root=flash,renderer=flash.GetComponent<SpriteRenderer>(),expires=clock+.14f});SpellSpark(mark.at,C("a8eaff"),.45f);
                for(int k=enemies.Count-1;k>=0;k--){Enemy e=enemies[k];if(Vector2.Distance(e.position,mark.at)<(chapterStone?.75f:.6f)+e.radius&&!ProjectilePathBlocked(mark.at,e.position,.03f)){ApplySkillHit(e,SkillDamage(22),Element.Lightning,"mark");if(++hits==3)break;}}
                if(mark.root!=null)Destroy(mark.root);chapterMarks.RemoveAt(i);
            }
        }
        private void ResolveChapterArc(Shot shot,Enemy first)
        {
            if(!shot.chapterArc)return;shot.chapterArc=false;Enemy best=null;float distance=2.5f;
            foreach(Enemy e in enemies)if(e!=first&&e.hp>0&&!shot.hitTargets.Contains(e))
            {float d=Vector2.Distance(e.position,first.position);if(d<distance&&!ProjectilePathBlocked(first.position,e.position,.05f)){best=e;distance=d;}}
            if(best==null)return;Vector2 off=best.position-first.position;
            GameObject beam=Stretch("Single arc leap",square,(first.position+best.position)*.5f,new Vector2(off.magnitude,.05f),C("b2ebff"),570);
            beam.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(off.y,off.x)*Mathf.Rad2Deg);fadingBodies.Add(new FadingBody{root=beam,renderer=beam.GetComponent<SpriteRenderer>(),expires=clock+.16f});
            ApplySkillHit(best,SkillDamage(chapterStone?15:12),Element.Lightning,"arc");
        }
        private void ClearChapterSkillEffects()
        {foreach(ChapterMark mark in chapterMarks)if(mark.root!=null)Destroy(mark.root);chapterMarks.Clear();}
        private void EquipChapterSkill(int skill,int slot)
        {
            if(!journeyRooms[currentRoom].cleared){JourneyHint("战斗中只能查看，清房后再调整配置");return;}
            if(!ChapterSkillKnown(skill)){JourneyHint("继续探索以学习这项技能");return;}
            int other=1-slot;if(chapterSkillSlots[other]==skill)chapterSkillSlots[other]=chapterSkillSlots[slot];chapterSkillSlots[slot]=skill;RequestCheckpoint();
        }
        private void DrawChapterSkills(float w,float h)
        {
            HudRect(new Rect(w/2-455,h/2-282,910,564),C("10232d"));CenterLabel(new Rect(w/2-430,h/2-263,860,40),"元素技能 · U / I 配置",journeyTitleStyle);
            CenterLabel(new Rect(w/2-430,h/2-212,860,35),"U "+ChapterSkillNames[chapterSkillSlots[0]]+"    I "+ChapterSkillNames[chapterSkillSlots[1]]+"    L 保留主属性技能",journeyTextStyle);
            string[] detail=woodlandMode?new[]{"风刃 · 保留已习得的风元素能力","抵挡普通飞弹，保护近身空间","7元素 / 1.6秒 · 木伤害与短暂束缚","9元素 / 2.4秒 · 自动追踪孢种"}:new[]{"风克雷 ×1.3；三次风命中可破势","消普通飞弹，短暂减雷伤；不消地雷","锁定敌人旧位置，延迟小范围落雷","命中后向附近可见敌人跳一次"};
            for(int i=0;i<4;i++)
            {
                float y=h/2-150+i*82;bool known=ChapterSkillKnown(i);GUI.Label(new Rect(w/2-420,y,560,28),(woodlandMode?new[]{"风之刃","护身风波","根刺","孢种"}:ChapterSkillNames)[i]+" · "+(known?ElementCost(ChapterSkillCost(i)).ToString("0.##")+"元素":"未学会"),journeyTextStyle);
                GUI.Label(new Rect(w/2-420,y+29,560,29),detail[i],journeySmallStyle);
                if(FlatButton(new Rect(w/2+160,y,115,53),"装备 U",chapterSkillSlots[0]==i))EquipChapterSkill(i,0);
                if(FlatButton(new Rect(w/2+290,y,115,53),"装备 I",chapterSkillSlots[1]==i))EquipChapterSkill(i,1);
            }
            GUI.Label(new Rect(w/2-420,h/2+199,840,58),"换装不重置冷却；技能和武器消耗元素之力，光暗单独计算。\nB / ESC 返回 · "+(journeyRooms[currentRoom].cleared?"当前安全，可以调整":"战斗中，只能查看"),journeySmallStyle);
        }
    }
}
