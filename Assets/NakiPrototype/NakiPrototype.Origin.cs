using UnityEngine;

namespace NakiLegend.Prototype
{
    public sealed partial class NakiPrototype
    {
        private enum Origin { Unchosen, Light, Dark }
        private Origin origin;
        private enum Profession { Unchosen, Warrior, Mage, Archer }
        private Profession profession;
        private int professionChoice;
        private readonly System.Collections.Generic.HashSet<Enemy> ramHits=new System.Collections.Generic.HashSet<Enemy>();
        private bool ProfessionReady { get { return enableDesertJourney&&profession==Profession.Unchosen&&!openingActive&&!StoryActive&&!showRainCG&&!chapterSetup&&!thunderSetupMenu&&health>0; } }
        private string ProfessionName { get { return profession==Profession.Warrior?"战士":profession==Profession.Mage?"法师":profession==Profession.Archer?"弓箭手":"待选专长"; } }
        private void ChooseProfession(int choice)
        {
            profession=(Profession)(Mathf.Clamp(choice,0,2)+1);
            // Only fill an empty slot: existing equipment and progress remain intact.
            if(carriedWeapons[1]<0&&profession!=Profession.Warrior)carriedWeapons[1]=profession==Profession.Mage?3:2;
            RequestCheckpoint();JourneyHint(ProfessionName+" · H 切换武器；所有专长都能使用短剑");
        }
        private bool HandleProfessionChoice()
        {
            if(!ProfessionReady)return false;
            if(Pressed('a'))professionChoice=(professionChoice+2)%3;
            if(Pressed('d'))professionChoice=(professionChoice+1)%3;
            if(Pressed('f'))ChooseProfession(professionChoice);
            return true;
        }
        private void DrawProfessionChoice(float w,float h)
        {
            HudRect(new Rect(0,0,w,h),new Color(.015f,.025f,.04f,.9f));
            CenterLabel(new Rect(w/2-420,h/2-195,840,45),"战斗专长 · 武器与光暗选择不受限制",journeyTitleStyle);
            string[] cards={"战士\n\nK 冲撞 · 每目标12物理伤害\n普通敌人轻微击退\n保留闪避保护","法师\n\n元素消耗降低15%\n光与贤者之石消耗不变\n空武器栏补发旧木法杖","弓箭手\n\n弓类散射降低50%\n弓类射速提高10%\n空武器栏补发猎沙短弓"};
            for(int i=0;i<3;i++)if(FlatButton(new Rect(w/2-450+i*305,h/2-120,290,220),cards[i],professionChoice==i))ChooseProfession(i);
            CenterLabel(new Rect(w/2-450,h/2+125,900,50),"A / D 选择 · F 确认 · 旧存档保留进度与装备；H 可切刀反击",journeySmallStyle);
        }
        private void TickWarriorRam(Vector2 from)
        {
            if(profession!=Profession.Warrior||clock>=dashUntil)return;
            foreach(Enemy e in enemies.ToArray())
            {
                if(e.hp<=0||ramHits.Contains(e)||DistanceToSegment(e.position,from,position)>e.radius+.3f||ProjectilePathBlocked(position,e.position,.02f))continue;
                ramHits.Add(e);Vector2 at=e.position;DamageEnemy(e,12);
                SpellSpark(at,C("f6cf80"),.35f);
                if(e.hp>0&&!e.boss)e.position=MoveEnemyWithinRoom(e.position,dashDirection*.35f,e.radius);
            }
        }
        private int originChoice;
        private bool OriginChoiceReady { get { return enableDesertJourney&&!ThunderActive&&!gatehouseSampleMode&&!bossPracticeMode&&origin==Origin.Unchosen&&!openingActive&&!StoryActive&&!showRainCG&&health>0; } }
        private void ChooseOrigin(int choice)
        {
            if(origin!=Origin.Unchosen)return;
            origin=choice==1?Origin.Dark:Origin.Light;
            energy=origin==Origin.Light?100:0;darkPower=origin==Origin.Dark?24:0;
            orbLearned=false;
            TellStory(origin==Origin.Dark
                ?"紫石沉入右眼，微凉的脉动随之传来。你握紧剑柄：先去遗迹看看，或许旧碑上还留着驾驭这股力量的方法。"
                :"你把手按在胸前，掌心亮起一线暖光。那道光还很微弱，但足以照见通往遗迹的旧路。");
            RequestCheckpoint();SaveCheckpoint(false);
        }
        private bool HandleOriginChoice()
        {
            if(!OriginChoiceReady)return false;
            if(Pressed('a')||Pressed('d'))originChoice=1-originChoice;
            if(Pressed('f'))ChooseOrigin(originChoice);
            return true;
        }
        private void DrawOriginChoice(float w,float h)
        {
            HudRect(new Rect(0,0,w,h),new Color(.015f,.025f,.04f,.72f));
            Rect box=new Rect(w/2-400,h/2-190,800,380);HudRect(box,C("10232d"));
            CenterLabel(new Rect(box.x+25,box.y+22,750,44),"登岛抉择 · 选择你的主属性",journeyTitleStyle);
            string light="光之力\n\nL 元气弹 · 单颗追踪\n光之力自然恢复\n适合稳定持续的旅程";
            string dark="暗之力\n\nL 流星 · 两颗紫色追踪球\n击败有灵生物为贤者之石充能\n机械没有灵魂，武器仍用元素之力";
            if(FlatButton(new Rect(box.x+28,box.y+94,357,210),light,originChoice==0))ChooseOrigin(0);
            if(FlatButton(new Rect(box.x+415,box.y+94,357,210),dark,originChoice==1))ChooseOrigin(1);
            CenterLabel(new Rect(box.x+20,box.y+323,760,35),"A / D 选择 · F 确认；本轮确定后固定，存档会保留选择",journeySmallStyle);
        }
        private void ChargeDarkPower(Enemy enemy)
        {
            if(!enableDesertJourney||origin!=Origin.Dark||woodlandMode&&(enemy.kind==EnemyKind.SpearGuard||enemy.kind==EnemyKind.Tumbleweed||enemy.kind==EnemyKind.StormGuard)||enemy.kind==EnemyKind.StoneSentinel||enemy.kind==EnemyKind.Colossus||enemy.kind==EnemyKind.CopperRoller||enemy.kind==EnemyKind.ThunderStone||IsChapterBoss(enemy.kind))return;
            darkPower=Mathf.Min(100,darkPower+(enemy.boss?24:12));
        }
        private void CastDarkMeteor()
        {
            if(!orbLearned){JourneyHint("贤者之石尚未回应 · 留意遗迹中的古井碑");return;}
            foreach(Shot old in shots)if(old.orb)return;
            float strength=1+darkPower/100*.35f;
            if(clock<nextSpell||!SpendPower(PowerPool.Dark,8))return;
            nextSpell=clock+1;
            Enemy target;Vector2 aim=AutoAim(10,7,0,out target);
            for(int i=0;i<2;i++)
            {
                Vector2 side=new Vector2(-aim.y,aim.x)*(i==0?-.16f:.16f);
                int before=shots.Count;SpawnShot(position+aim*.4f+side,aim,false,target,position);
                if(shots.Count>before){Shot s=shots[shots.Count-1];s.orb=true;s.element=Element.Dark;s.damage=SkillDamage(10*strength);s.life=1.8f;s.speed=7;s.tracking=target!=null;s.root.transform.localScale=Vector3.one*.24f;s.root.GetComponent<SpriteRenderer>().color=C("bc7cf4");AddShotRibbon(s,C("bc7cf4"),.09f);}
            }
            castDirection=aim;castPoseUntil=clock+.25f;
        }
    }
}
