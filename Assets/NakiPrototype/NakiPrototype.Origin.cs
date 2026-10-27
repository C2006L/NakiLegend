using UnityEngine;

namespace NakiLegend.Prototype
{
    public sealed partial class NakiPrototype
    {
        private enum Origin { Unchosen, Light, Dark }
        private Origin origin;
        private int originChoice;
        private bool OriginChoiceReady { get { return enableDesertJourney&&!gatehouseSampleMode&&!bossPracticeMode&&origin==Origin.Unchosen&&!openingActive&&!StoryActive&&!showRainCG&&health>0; } }
        private void ChooseOrigin(int choice)
        {
            if(origin!=Origin.Unchosen)return;
            origin=choice==1?Origin.Dark:Origin.Light;
            energy=origin==Origin.Light?100:0;darkPower=origin==Origin.Dark?24:0;
            orbLearned=false;
            TellStory(origin==Origin.Dark
                ?"你选择暗之力。贤者之石将寄宿右眼，击败有灵生物可以充能。先用武器探索，三间战斗房后学习流星。"
                :"你选择光之力。内心的光会自然恢复。先用武器探索，三间战斗房后学习元气弹。");
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
            if(!enableDesertJourney||origin!=Origin.Dark||enemy.kind==EnemyKind.StoneSentinel||enemy.kind==EnemyKind.Colossus)return;
            darkPower=Mathf.Min(100,darkPower+(enemy.boss?24:12));
        }
        private void CastDarkMeteor()
        {
            if(!orbLearned){JourneyHint("流星尚未学会 · 清理3个战斗房后寻找古井碑");return;}
            foreach(Shot old in shots)if(old.orb)return;
            float strength=1+darkPower/100*.35f;
            if(clock<nextSpell||!SpendPower(PowerPool.Dark,8))return;
            nextSpell=clock+1;
            Enemy target;Vector2 aim=AutoAim(10,7,0,out target);
            for(int i=0;i<2;i++)
            {
                Vector2 side=new Vector2(-aim.y,aim.x)*(i==0?-.16f:.16f);
                int before=shots.Count;SpawnShot(position+aim*.4f+side,aim,false,target,position);
                if(shots.Count>before){Shot s=shots[shots.Count-1];s.orb=true;s.element=Element.Dark;s.damage=10*strength;s.life=1.8f;s.speed=7;s.tracking=target!=null;s.root.transform.localScale=Vector3.one*.24f;s.root.GetComponent<SpriteRenderer>().color=C("bc7cf4");}
            }
            castDirection=aim;castPoseUntil=clock+.25f;
        }
    }
}
