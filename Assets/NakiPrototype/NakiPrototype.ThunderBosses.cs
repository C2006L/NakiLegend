using UnityEngine;

namespace NakiLegend.Prototype
{
    public sealed partial class NakiPrototype
    {
        private static bool IsChapterBoss(EnemyKind kind){return kind>=EnemyKind.CopperWarden&&kind<=EnemyKind.StormStag;}
        private void ConfigureChapterEnemy(Enemy e)
        {
            int stage=thunderChapterMode?journeyRooms[currentRoom].stage:0;e.affinity=e.attackElement=Element.Lightning;e.nextAttack=clock+1.6f;
            e.boss=IsChapterBoss(e.kind);e.radius=e.boss?.9f:e.kind==EnemyKind.ThunderStone?.5f:.34f;e.speed=e.kind==EnemyKind.StormStag?1.5f:e.kind==EnemyKind.CopperRoller?1.5f:.85f;
            float hp=e.kind==EnemyKind.CopperWarden?360:e.kind==EnemyKind.ThunderSentinel?620:e.kind==EnemyKind.StormStag?1250:e.kind==EnemyKind.CopperRoller?70+stage*10:92+stage*12;
            if(!e.boss)e.speed*=1.15f;
            e.hp=e.maximum=hp;if(e.kind==EnemyKind.ThunderStone)e.root.transform.localScale=Vector3.one*.45f;
        }
        private void UpdateChapterEnemy(Enemy e,float dt)
        {
            if(IsChapterBoss(e.kind)){UpdateChapterBoss(e,dt);return;}
            e.position=MoveEnemyWithinRoom(e.position,Vector2.zero,e.radius);Vector2 old=e.position,off=position-e.position;
            if(e.chargeUntil>0)
            {
                if(clock<e.chargeUntil)
                {
                    e.position=MoveEnemyWithinRoom(e.position,e.attackDirection*6.2f*dt,e.radius);
                    if((e.position-old).sqrMagnitude<.00001f)e.chargeUntil=clock;
                    if(DistanceToSegment(position,old,e.position)<e.radius+.3f)TakeJourneyDamage(1.5f,woodlandMode?Element.Wood:Element.Lightning);
                }
                else{e.chargeUntil=0;e.recoveryUntil=clock+.9f;}
            }
            else if(e.windup>0)
            {
                e.windup-=dt;if(e.windup<=0){if(e.warning!=null)Destroy(e.warning);e.actionStarted=clock;if(e.kind==EnemyKind.CopperRoller)e.chargeUntil=clock+.7f;else e.recoveryUntil=clock+.9f;}
            }
            else if(clock>=e.recoveryUntil)
            {
                int winding=0;foreach(Enemy other in enemies)if(other.windup>0)winding++;
                if(clock>=e.nextAttack&&clock>=thunderNextWindup&&off.magnitude<7&&winding<2)
                {
                    e.actionStarted=clock;e.attackNumber++;e.attackDirection=off.sqrMagnitude>.01f?off.normalized:Vector2.down;e.windup=e.kind==EnemyKind.CopperRoller?.8f:1;
                    e.nextAttack=clock+e.windup+2;thunderNextWindup=clock+.65f;
                    if(e.kind==EnemyKind.ThunderStone){e.strikePoint=position;AddChapterStrike(e.strikePoint,1);}
                    else e.warning=ChapterLineWarning(e.position,e.attackDirection,4.6f,.7f);
                }
                else if(off.magnitude>2.8f)e.position=MoveEnemyWithinRoom(e.position,off.normalized*e.speed*dt,e.radius);
            }
            e.velocity=dt>0?(e.position-old)/dt:Vector2.zero;UpdateChapterEnemyPose(e);
        }
        private void UpdateChapterBoss(Enemy e,float dt)
        {
            e.position=MoveEnemyWithinRoom(e.position,Vector2.zero,e.radius);Vector2 old=e.position;
            if(!e.secondPhase&&e.hp<=e.maximum*(e.kind==EnemyKind.CopperWarden?.5f:.45f))
            {
                e.secondPhase=true;e.phaseUntil=clock+1.5f;e.windup=e.chargeUntil=0;e.nextAttack=e.phaseUntil+.35f;e.recoveryUntil=e.phaseUntil;
                if(e.warning!=null)Destroy(e.warning);ClearThunderHazards();
                for(int i=shots.Count-1;i>=0;i--)if(shots[i].hostile){Destroy(shots[i].root);shots.RemoveAt(i);}
                AddFloatingHit("雷流失衡 · 第二阶段",e.position+Vector2.up*2,C("b6dcff"));
            }
            if(clock<e.phaseUntil){UpdateChapterEnemyPose(e);return;}
            if(e.chargeUntil>0)
            {
                if(clock<e.chargeUntil)
                {
                    e.position=MoveEnemyWithinRoom(e.position,e.attackDirection*7.2f*dt,e.radius);
                    if((e.position-old).sqrMagnitude<.00001f)e.chargeUntil=clock;
                    if(DistanceToSegment(position,old,e.position)<e.radius+.3f)TakeJourneyDamage(2,woodlandMode?Element.Wood:Element.Lightning);
                }
                else
                {
                    e.chargeUntil=0;e.recoveryUntil=clock+1;
                    if(e.chapterCombo>0)
                    {e.chapterCombo=0;e.windup=.7f;e.actionStarted=clock;e.attackDirection=(position-e.position).normalized;e.warning=ChapterLineWarning(e.position,e.attackDirection,4.8f,1.6f);e.nextAttack=clock+2.5f;}
                }
            }
            else if(e.windup>0)
            {
                e.windup-=dt;if(e.windup<=0){if(e.warning!=null)Destroy(e.warning);ResolveChapterBossMove(e);}
            }
            else if(clock>=e.recoveryUntil)
            {
                if(clock>=e.nextAttack)BeginChapterBossMove(e);
                else
                {
                    Vector2 off=position-e.position;float desired=e.kind==EnemyKind.CopperWarden?2.5f:4;
                    if(off.magnitude>desired)e.position=MoveEnemyWithinRoom(e.position,off.normalized*e.speed*dt,e.radius);
                }
            }
            e.velocity=dt>0?(e.position-old)/dt:Vector2.zero;UpdateChapterEnemyPose(e);
        }
        private GameObject ChapterLineWarning(Vector2 at,Vector2 direction,float length,float width)
        {
            GameObject o=Stretch("Locked attack path",square,at+direction*length*.5f,new Vector2(length,width),new Color(.95f,.61f,.32f,.26f),-640);
            o.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg);return o;
        }
        private void BeginChapterBossMove(Enemy e)
        {
            Sfx("warn",true,.4f);e.attackNumber++;int pattern=(e.attackNumber-1)%3;e.attackDirection=(position-e.position).sqrMagnitude>.01f?(position-e.position).normalized:Vector2.down;
            e.actionStarted=clock;e.windup=e.kind==EnemyKind.CopperWarden?(pattern==0?.7f:pattern==1?.9f:.85f):e.kind==EnemyKind.ThunderSentinel?(pattern==1?1.2f:1):pattern==2?1.4f:.9f;
            e.nextAttack=clock+e.windup+(e.secondPhase?1.05f:1.3f);e.chapterCombo=e.kind==EnemyKind.StormStag&&e.secondPhase&&pattern==0?1:0;
            if(e.kind==EnemyKind.CopperWarden)
            {
                if(pattern==0)e.warning=ChapterLineWarning(e.position,e.attackDirection,3.2f,.65f);
                if(pattern==2)e.warning=WarningSector(e.position,e.attackDirection,2.1f);
            }
            else if(e.kind==EnemyKind.ThunderSentinel)
            {
                if(pattern==0){AddChapterStrike(position,1);if(e.secondPhase)AddChapterStrike(SafeChapterLightning(position+Vector2.right*1.9f),1.7f);}
                if(pattern==2){AddChapterStrike(SafeChapterLightning(position+Vector2.left*1.4f),1);AddChapterStrike(SafeChapterLightning(position+Vector2.right*1.4f),1.8f);if(e.secondPhase)AddChapterStrike(SafeChapterLightning(position+Vector2.up*1.8f),2.5f);}
            }
            else
            {
                if(pattern==0)e.warning=ChapterLineWarning(e.position,e.attackDirection,4.8f,1.6f);
                if(pattern==1)
                {
                    int count=e.secondPhase?5:e.attackNumber==2?2:3;
                    for(int i=0;i<count;i++)AddChapterStrike(SafeChapterLightning(position+new Vector2((i%2==0?1:-1)*i*.8f,i*.35f)),1+i*.55f);
                }
            }
        }
        private Vector2 SafeChapterLightning(Vector2 desired){return NearestEnemyFooting(desired,.85f);}
        private void AddChapterStrike(Vector2 at,float delay)
        {
            Sfx(woodlandMode?"root":"warn",true,.55f);
            GameObject warning=WarningCircle(at,1.1f);
            GameObject timer=Draw("Lightning countdown",windRingSprite??disc,at,Vector2.one*2.2f,C("ffce73"),-637);
            timer.transform.SetParent(warning.transform,true);
            thunderStrikes.Add(new ThunderStrike{at=at,born=clock,when=clock+delay,root=warning,timer=timer});
        }
        private void ResolveChapterBossMove(Enemy e)
        {
            int move=(e.attackNumber-1)%3;e.actionStarted=clock;e.recoveryUntil=clock+(move==2?1.2f:1)-(e.secondPhase?.2f:0);
            if(e.kind==EnemyKind.CopperWarden)
            {
                if(move==0){if(DistanceToSegment(position,e.position,e.position+e.attackDirection*3.2f)<.48f&&!ProjectilePathBlocked(e.position,position,.05f))TakeJourneyDamage(2,woodlandMode?Element.Wood:Element.Lightning);}
                else if(move==1)ChapterBossFan(e,e.secondPhase?7:5);
                else{Vector2 off=position-e.position;if(off.magnitude<2.4f&&Vector2.Dot(off.normalized,e.attackDirection)>Mathf.Cos(55*Mathf.Deg2Rad)&&!ProjectilePathBlocked(e.position,position,.04f))TakeJourneyDamage(2,woodlandMode?Element.Wood:Element.Lightning);}
            }
            else if(e.kind==EnemyKind.ThunderSentinel){if(move==1)ChapterBossFan(e,e.secondPhase?9:7);}
            else if(move==0)e.chargeUntil=clock+.65f;else if(move==2)ChapterBossFan(e,e.secondPhase?11:9);
        }
        private void ChapterBossFan(Enemy e,int count)
        {
            int start=shots.Count;JourneyFan(e,count,16);
            for(int i=start;i<shots.Count;i++){shots[i].damage=2;if(woodlandMode)shots[i].element=Element.Wood;shots[i].speed=e.secondPhase?4.4f:4;shots[i].root.GetComponent<SpriteRenderer>().color=C(woodlandMode?"a7c974":"b1e6ff");}
        }
        private void UpdateChapterEnemyPose(Enemy e)
        {
            Sprite[] a;if(!enemyActionArt.TryGetValue(e.kind,out a)||a.Length==0)return;
            int frame=e.velocity.sqrMagnitude>.001f?Mathf.FloorToInt(clock*8)%Mathf.Min(4,a.Length):0;
            if(e.windup>0)frame=4;if(e.chargeUntil>clock||clock<e.recoveryUntil&&clock-e.actionStarted<.25f)frame=5;
            if(clock<e.hurtUntil)frame=6;
            e.body.sprite=a[Mathf.Min(frame,a.Length-1)];e.root.transform.position=e.position;
            e.body.sortingOrder=DepthAt(e.position.y);e.body.flipX=(e.windup>0||e.chargeUntil>0?e.attackDirection.x:position.x-e.position.x)<0;
            if(e.shadow!=null)e.shadow.transform.position=e.position;UpdateEliteSigil(e);
        }
        private string ChapterBossMove(Enemy e)
        {
            int move=Mathf.Max(0,(e.attackNumber-1)%3);
            return e.kind==EnemyKind.CopperWarden?new[]{"拦路叉","巡路雷哨","横叉扫路"}[move]:e.kind==EnemyKind.ThunderSentinel?new[]{"点地引雷","铜环放电","错区巡雷"}[move]:new[]{"雷角突进","巡山落雷","聚雷冠"}[move];
        }
    }
}
