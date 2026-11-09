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
        private bool professionFromSave;
        private readonly System.Collections.Generic.HashSet<Enemy> ramHits=new System.Collections.Generic.HashSet<Enemy>();
        private bool ProfessionReady { get { return enableDesertJourney&&profession==Profession.Unchosen&&!openingActive&&!StoryActive&&!showRainCG&&!chapterSetup&&!thunderSetupMenu&&health>0; } }
        private string ProfessionName { get { return profession==Profession.Warrior?"战士":profession==Profession.Mage?"法师":profession==Profession.Archer?"弓箭手":"待选专长"; } }
        private void ChooseProfession(int choice)
        {
            bool starting=profession==Profession.Unchosen&&!professionFromSave;
            profession=(Profession)(Mathf.Clamp(choice,0,2)+1);
            int starter=profession==Profession.Mage?3:profession==Profession.Archer?2:0;
            if(starting&&starter!=0)
            {
                foreach(int id in carriedWeapons)if(id>0&&id!=starter&&!reserveWeapons.Contains(id)&&reserveWeapons.Count<4)reserveWeapons.Add(id);
                carriedWeapons[0]=starter;carriedWeapons[1]=0;equippedSlot=0;
            }
            else if(profession!=Profession.Warrior&&carriedWeapons[1]<0){carriedWeapons[1]=starter;equippedSlot=1;}
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
            CenterLabel(new Rect(w/2-420,h/2-195,840,45),"战斗专长 · 武器与光之力与贤者之石选择不受限制",journeyTitleStyle);
            string[] cards={"战士\n\n初始武器：旅人短剑\nK 冲撞伤敌并轻微击退\n近战出手减伤30%，不易打断","法师\n\n开局持旧木法杖\n元素消耗降低15%\nH 可切换备用短剑","弓箭手\n\n开局持猎沙短弓\n弓类更准，射速提高10%\nH 可切换备用短剑"};
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
        private int nextMeteorGroup;
        private readonly System.Collections.Generic.HashSet<int> meteorGroups=new System.Collections.Generic.HashSet<int>();
        private bool OriginChoiceReady { get { return enableDesertJourney&&!ThunderActive&&!gatehouseSampleMode&&!bossPracticeMode&&origin==Origin.Unchosen&&!openingActive&&!StoryActive&&!showRainCG&&health>0; } }
        private void ChooseOrigin(int choice)
        {
            if(origin!=Origin.Unchosen)return;
            origin=choice==1?Origin.Dark:Origin.Light;
            energy=origin==Origin.Light?LightMaximum:0;darkPower=origin==Origin.Dark?6:0;
            orbLearned=false;
            TellStory(origin==Origin.Dark
                ?"紫石沉入右眼，微凉的脉动随之传来。你握紧手中的武器：先去遗迹看看，或许旧碑上还留着驾驭这股力量的方法。"
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
            string light="光之力\n\nL 辉光束 · 按住自动锁定照射\n停手1.5秒后，每秒恢复4光能\n逐步领悟 O 光幕与 P 别·天神";
            string dark="贤者之石能量\n\nL 流星 · 五颗紫色追踪球\n击败有灵生物为贤者之石充能\n机械没有灵魂；普通武器用元素之力，魂器消耗石中能量";
            if(FlatButton(new Rect(box.x+28,box.y+94,357,210),light,originChoice==0))ChooseOrigin(0);
            if(FlatButton(new Rect(box.x+415,box.y+94,357,210),dark,originChoice==1))ChooseOrigin(1);
            CenterLabel(new Rect(box.x+20,box.y+323,760,35),"A / D 选择 · F 确认；本轮确定后固定，存档会保留选择",journeySmallStyle);
        }
        private void ChargeDarkPower(Enemy enemy)
        {
            if(!enableDesertJourney||origin!=Origin.Dark||enemy.hp>0||!enemy.soulEligible||enemy.summoned||enemy.soulClaimed)return;
            enemy.soulClaimed=true;int amount=enemy.boss||enemy.elite?3:2;
            float added=amount;darkPower+=added;if((originLoreBits&32)==0){originLoreBits|=32;AddFloatingHit("右眼掠过陌生的归途",enemy.position+Vector2.up*1.4f,C("bfa0e8"));}
            if(added>0)AddFloatingHit("灵魂能量 +"+added.ToString("0"),enemy.position+Vector2.up,C("b88ae8"));
        }
        private void CastDarkMeteor()
        {
            CastMeteorVolley();
        }
    }
}
