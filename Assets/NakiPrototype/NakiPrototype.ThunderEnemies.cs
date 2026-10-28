using System;
using System.Collections.Generic;
using UnityEngine;

namespace NakiLegend.Prototype
{
    public sealed partial class NakiPrototype
    {
        [Serializable] private sealed class ThunderFrame { public int x=0,y=0,width=0,height=0;public float pivotX=.5f,pivotY=0; }
        [Serializable] private sealed class ThunderAtlas { public ThunderFrame[] moth=Array.Empty<ThunderFrame>(),guard=Array.Empty<ThunderFrame>(); }
        private sealed class ThunderStrike { public Vector2 at;public float when;public GameObject root; }
        private readonly List<ThunderStrike> thunderStrikes=new List<ThunderStrike>();
        private static bool IsThunderEnemy(EnemyKind kind){return kind>=EnemyKind.ChargeMoth&&kind<=EnemyKind.StormStag;}
        private void LoadThunderEnemyArt()
        {
            TextAsset data=Resources.Load<TextAsset>("NakiArt/ThunderS1/atlas");if(data==null)return;
            ThunderAtlas atlas=JsonUtility.FromJson<ThunderAtlas>(data.text);
            LoadThunderFrames("moth",EnemyKind.ChargeMoth,atlas.moth,250);
            LoadThunderFrames("guard",EnemyKind.StormGuard,atlas.guard,230);
        }
        private void LoadThunderFrames(string name,EnemyKind kind,ThunderFrame[] specs,float ppu)
        {
            Texture2D t=Resources.Load<Texture2D>("NakiArt/ThunderS1/"+name);if(t==null||specs==null)return;t.filterMode=FilterMode.Point;
            var frames=new Sprite[specs.Length];
            for(int i=0;i<specs.Length;i++)
            {
                ThunderFrame f=specs[i];frames[i]=Sprite.Create(t,new Rect(f.x,t.height-f.y-f.height,f.width,f.height),new Vector2(f.pivotX,f.pivotY),ppu,0,SpriteMeshType.FullRect);ownedAssets.Add(frames[i]);
            }
            enemyActionArt[kind]=frames;
        }
        private void ConfigureThunderEnemy(Enemy e)
        {
            if(e.kind>=EnemyKind.CopperRoller){ConfigureChapterEnemy(e);return;}
            e.boss=e.elite=false;e.radius=.32f;e.hp=e.maximum=e.kind==EnemyKind.ChargeMoth?30:42;
            e.speed=e.kind==EnemyKind.ChargeMoth?1.5f:1.65f;e.affinity=e.attackElement=Element.Lightning;
            if(thunderChapterMode){int tier=journeyRooms[currentRoom].stage;e.hp=e.maximum=(e.kind==EnemyKind.ChargeMoth?42:58)+tier*(e.kind==EnemyKind.ChargeMoth?6:8);e.speed+=.2f+tier*.08f;}
            e.nextAttack=clock+1.25f;e.root.transform.localScale=Vector3.one;
        }
        private void UpdateThunderEnemy(Enemy e,float dt)
        {
            if(e.kind>=EnemyKind.CopperRoller){UpdateChapterEnemy(e,dt);return;}
            e.position=MoveEnemyWithinRoom(e.position,Vector2.zero,e.radius);Vector2 before=e.position,off=position-e.position;
            bool moth=e.kind==EnemyKind.ChargeMoth;
            if(e.windup>0)
            {
                e.windup-=dt;
                if(e.windup<=0)
                {
                    if(e.warning!=null)Destroy(e.warning);e.actionStarted=clock;e.recoveryUntil=clock+.55f;
                    if(moth||e.attackNumber%2==0)
                    {
                        int first=shots.Count;JourneyFan(e,moth?3:1,16);
                        for(int i=first;i<shots.Count;i++){shots[i].root.GetComponent<SpriteRenderer>().color=C("a7e6ff");shots[i].speed=thunderChapterMode?3.9f:3.4f;if(thunderChapterMode)shots[i].damage=1.25f;}
                    }
                    else if(DistanceToSegment(position,e.position,e.position+e.attackDirection*2.1f)<.48f&&!ProjectilePathBlocked(e.position,position,.05f))TakeJourneyDamage(thunderChapterMode?1.25f:1,Element.Lightning);
                }
            }
            else if(clock>=e.recoveryUntil)
            {
                bool ranged=moth||(e.attackNumber+1)%2==0;float reach=ranged?6.5f:2.15f;
                int winding=0;foreach(Enemy other in enemies)if(other.windup>0)winding++;
                bool clear=!ProjectilePathBlocked(e.position,position,.1f);
                if(clock>=e.nextAttack&&clock>=thunderNextWindup&&off.magnitude<reach&&clear&&winding<(thunderChapterMode?2:currentWave<3?1:2))
                {
                    e.attackNumber++;e.actionStarted=clock;e.attackDirection=off.sqrMagnitude>.001f?off.normalized:Vector2.down;
                    e.windup=moth?.65f:e.attackNumber%2==0?.75f:.55f;e.nextAttack=clock+e.windup+(thunderChapterMode?1.4f-journeyRooms[currentRoom].stage*.1f:1.65f);thunderNextWindup=clock+(thunderChapterMode?.5f:.6f);
                    if(!moth&&e.attackNumber%2!=0)
                    {
                        e.warning=Stretch("Copper spear thrust warning",square,e.position+e.attackDirection*1.05f,new Vector2(2.1f,.55f),new Color(1,.56f,.24f,.32f),-638);
                        e.warning.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(e.attackDirection.y,e.attackDirection.x)*Mathf.Rad2Deg);
                    }
                }
                else if(off.magnitude>(ranged?4.8f:1.6f)||moth&&off.magnitude<2.8f||!clear)
                {
                    Vector2 dir=off.normalized*(moth&&off.magnitude<2.8f?-1:1);
                    if(!clear)dir=new Vector2(-dir.y,dir.x);
                    Vector2 next=MoveEnemyWithinRoom(e.position,dir*e.speed*dt,e.radius);
                    if((next-e.position).sqrMagnitude<.00001f)next=MoveEnemyWithinRoom(e.position,new Vector2(-dir.y,dir.x)*e.speed*dt,e.radius);
                    e.position=next;
                }
            }
            e.velocity=dt>0?(e.position-before)/dt:Vector2.zero;UpdateThunderPose(e);
        }
        private void UpdateThunderPose(Enemy e)
        {
            Sprite[] a;if(!enemyActionArt.TryGetValue(e.kind,out a))return;bool moth=e.kind==EnemyKind.ChargeMoth;
            int frame=Mathf.FloorToInt(clock*9)%6;
            if(!moth&&e.velocity.sqrMagnitude<.001f)frame=0;
            if(e.windup>0)frame=6+Mathf.Min(2,Mathf.FloorToInt((clock-e.actionStarted)*4));
            if(clock<e.recoveryUntil&&clock-e.actionStarted<.23f)frame=moth?11:e.attackNumber%2==0?9:8;
            if(clock<e.hurtUntil)frame=moth?12:10;
            e.body.sprite=a[Mathf.Clamp(frame,0,a.Length-1)];e.root.transform.position=e.position+(moth?Vector2.up*.35f:Vector2.zero);
            e.body.flipX=(e.windup>0?e.attackDirection.x:position.x-e.position.x)<0;
            e.body.sortingOrder=DepthAt(e.position.y);e.body.color=Color.white;
            if(e.shadow!=null)e.shadow.transform.position=e.position;
        }
        private void QueueThunderStrike(Vector2 at)
        {
            GameObject warning=WarningCircle(at,.8f);var sr=warning.GetComponent<SpriteRenderer>();sr.color=new Color(.35f,.75f,1,.15f);
            foreach(SpriteRenderer child in warning.GetComponentsInChildren<SpriteRenderer>())if(child!=sr)child.color=C("9cdaff");
            thunderStrikes.Add(new ThunderStrike{at=at,when=clock+.9f,root=warning});
        }
        private void TickThunderStrikes()
        {
            for(int i=thunderStrikes.Count-1;i>=0;i--)
            {
                ThunderStrike s=thunderStrikes[i];if(clock<s.when)continue;
                if(Vector2.Distance(position,s.at)<1.1f)TakeJourneyDamage(ChapterGroundDamage,Element.Lightning);
                if(s.root!=null)Destroy(s.root);
                GameObject bolt=Stretch("Ground lightning impact",square,s.at+Vector2.up*.9f,new Vector2(.07f,1.8f),C("d5f3ff"),650);
                fadingBodies.Add(new FadingBody{root=bolt,renderer=bolt.GetComponent<SpriteRenderer>(),expires=clock+.18f});
                thunderStrikes.RemoveAt(i);
            }
        }
        private bool TryThunderWindInterrupt(Shot shot,Vector2 from,Vector2 to)
        {
            if(!ThunderActive||!HasThunderConductor||thunderRepaired||shot.hostile||shot.element!=Element.Wind)return false;
            if(DistanceToSegment(ThunderNodeAt+Vector2.up*.35f,from,to)>.7f)return false;
            ClearThunderHazards();thunderNextStrike=clock+3;JourneyHint("风刃打断聚雷 · 清房后可免费复位手柄");return true;
        }
    }
}
