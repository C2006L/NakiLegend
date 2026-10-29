using System.Collections.Generic;
using UnityEngine;

namespace NakiLegend.Prototype
{
    public sealed partial class NakiPrototype
    {
        private enum EnemyKind { SandScorpion, SpearGuard, Tumbleweed, SandKite, StoneSentinel, WindScorpion, Colossus, ApeKing, ChargeMoth, StormGuard, CopperRoller, ThunderStone, CopperWarden, ThunderSentinel, StormStag, WoodlandDeer, IceGuard, IceWisp, IceBeast, FrostHound, IceArcher, IceRunecaster }
        private readonly Dictionary<EnemyKind, Sprite[]> enemyActionArt = new Dictionary<EnemyKind, Sprite[]>();
        private sealed class FloatingHit { public string text; public Vector2 at; public float expires; public Color color; }
        private sealed class FadingBody { public GameObject root; public SpriteRenderer renderer; public float expires; }
        private readonly List<FloatingHit> floatingHits = new List<FloatingHit>();
        private readonly List<FadingBody> fadingBodies = new List<FadingBody>();
        private AudioSource journeyAudio;
        private AudioClip hitSound, hurtSound;
        private float nextSoundAt, cameraShakeUntil;
        private static readonly string[] EnemyNames = { "蓄露砂蝎", "断渠枪卫", "荆冠滚蓬", "逐风沙鹞", "枯井石卫", "噬雨风蝎", "古渠巨像", "沙原猿王", "蓄雷蛾", "巡雷枪卫", "铜壳滚兽", "引雷石卫", "守路铜卫", "引雷石卫长", "巡雷巨鹿", "寄枝鹿", "晶甲守卫", "浮冰晶灵", "裂冰甲兽", "霜牙猎犬", "寒弓游猎者", "刻霜祭司" };

        private void LoadEnemyActionArt()
        {
            string[] names = { "scorpion-actions", "skeleton-actions", "tumbleweed-actions", "kite-actions", "sentinel-actions", "wind-scorpion-actions", "colossus-actions", "ape-king-actions" };
            for (int kind = 0; kind < names.Length; kind++)
            {
                Texture2D texture = Resources.Load<Texture2D>("NakiArt/journey-" + names[kind]);
                if (texture == null) continue;
                texture.filterMode = FilterMode.Point;
                var frames = new Sprite[8];
                for (int i = 0; i < frames.Length; i++)
                {
                    frames[i] = Sprite.Create(texture, new Rect(i % 4 * 512, (1 - i / 4) * 512, 512, 512), new Vector2(.5f, 32f / 512), 130, 0, SpriteMeshType.FullRect);
                    frames[i].name = names[kind] + "_" + i; ownedAssets.Add(frames[i]);
                }
                enemyActionArt[(EnemyKind)kind] = frames;
            }
        }

        private Enemy SpawnJourneyEnemy(EnemyKind kind, Vector2 at, int depth)
        {
            bool boss = kind>=EnemyKind.WindScorpion&&kind<=EnemyKind.ApeKing||IsChapterBoss(kind);
            float hp = boss ? 390 + depth * 7 : kind == EnemyKind.StoneSentinel ? 95 + depth * 4 : kind == EnemyKind.SandKite ? 25 + depth : 28 + depth * 1.4f;
            float radius = boss ? .85f : kind == EnemyKind.StoneSentinel ? .55f : .32f;
            var enemy = new Enemy { kind = kind, position = at, hp = hp, maximum = hp, radius = radius, boss = boss,
                speed = (kind == EnemyKind.StoneSentinel ? .8f : boss ? kind == EnemyKind.ApeKing ? 1.65f : .95f : kind == EnemyKind.SandKite ? 1.7f : 1.2f) + Mathf.Min(.3f, depth * .025f), nextAttack = clock + 1.2f };
            Sprite[] frames;
            Sprite sprite = enemyActionArt.TryGetValue(kind, out frames) ? frames[0] : boss ? bossSprite : kind == EnemyKind.SpearGuard ? skeletonSprite : smallEnemySprite;
            enemy.root = Draw(EnemyNames[(int)kind], sprite, at, Vector2.one, Color.white, DepthAt(at.y)); enemy.body = enemy.root.GetComponent<SpriteRenderer>();
            enemy.shadow = Draw("Enemy shadow", softDisc ?? disc, at, new Vector2(boss ? 1.8f : .7f, boss ? .45f : .2f), new Color(.08f, .06f, .04f, .35f), -799);
            enemy.affinity = lightningTrialMode && boss ? Element.Lightning : boss ? Element.Wind : Element.Physical; enemy.attackElement = lightningTrialMode && boss ? Element.Lightning : Element.Physical; if(kind==EnemyKind.StoneSentinel){enemy.elite=true;enemy.eliteKind=1;}if(IsThunderEnemy(kind))ConfigureThunderEnemy(enemy);if(woodlandMode)ConfigureWoodEnemy(enemy);enemy.soulEligible=woodlandMode?(ArrayIndexWood(kind)==0||ArrayIndexWood(kind)==1||ArrayIndexWood(kind)==6||ArrayIndexWood(kind)==7||ArrayIndexWood(kind)==8||ArrayIndexWood(kind)==9||ArrayIndexWood(kind)==12):kind!=EnemyKind.StoneSentinel&&kind!=EnemyKind.Colossus&&kind!=EnemyKind.CopperRoller&&kind!=EnemyKind.ThunderStone&&!IsChapterBoss(kind);if(IsIceEnemy(kind))ConfigureIceEnemy(enemy);enemies.Add(enemy); return enemy;
        }

        private void UpdateJourneyEnemy(Enemy enemy, float dt)
        {
            if(IsIceEnemy(enemy.kind)){UpdateIceEnemy(enemy,dt);return;}
            if(IsThunderEnemy(enemy.kind)){UpdateThunderEnemy(enemy,dt);return;}
            enemy.position=MoveEnemyWithinRoom(enemy.position,Vector2.zero,enemy.radius);
            Vector2 previous = enemy.position, offset = position - enemy.position;
            if (enemy.boss && !enemy.secondPhase && enemy.hp <= enemy.maximum * .45f)
            {
                BeginBossPhase(enemy);
            }
            if(enemy.phaseUntil>clock){UpdateJourneyEnemyPose(enemy,false);return;}
            if (enemy.chargeUntil > clock)
            {
                enemy.position = MoveEnemyWithinRoom(enemy.position, enemy.attackDirection * (enemy.kind == EnemyKind.Tumbleweed ? 6.2f : 5.2f) * dt, enemy.radius);
                if ((enemy.position - previous).sqrMagnitude < .0001f) enemy.chargeUntil = clock;
                if (DistanceToSegment(position, previous, enemy.position) < enemy.radius + .3f) TakeJourneyDamage(enemy.boss ? 2 : 1, enemy.attackElement);
            }
            else if (enemy.windup > 0)
            {
                enemy.windup -= dt;
                if (enemy.windup <= 0) { if (enemy.warning != null) Destroy(enemy.warning); ResolveJourneyAttack(enemy); }
            }
            else if (clock >= enemy.recoveryUntil)
            {
                bool ranged = enemy.kind == EnemyKind.SandKite;
                float reach = enemy.boss ? 20 : ranged ? 6 : enemy.kind == EnemyKind.StoneSentinel ? 2.4f : enemy.kind == EnemyKind.SpearGuard ? 2.1f : 2.0f;
                int winding = 0; foreach (Enemy other in enemies) if (other.windup > 0) winding++;
                bool clearLine = !ProjectilePathBlocked(enemy.position, position, .06f);
                bool spearAligned = enemy.kind != EnemyKind.SpearGuard || Mathf.Abs(offset.y) < .55f && Mathf.Abs(offset.x) > .8f;
                if (clock >= enemy.nextAttack && offset.magnitude < reach && spearAligned && (!ranged && enemy.kind != EnemyKind.SpearGuard || clearLine) && winding < (enemy.boss ? 3 : 2)) BeginJourneyAttack(enemy);
                else if (offset.magnitude > enemy.radius + .45f || ranged && offset.magnitude < 2.7f || !spearAligned)
                {
                    Vector2 direction = offset.normalized * (ranged && offset.magnitude < 2.7f ? -1 : 1);
                    if (enemy.kind == EnemyKind.SpearGuard)
                    {
                        float side = Mathf.Abs(offset.x) > .1f ? Mathf.Sign(offset.x) : 1;
                        direction = (position - Vector2.right * side * 1.6f - enemy.position).normalized;
                    }
                    if (!clearLine && (ranged || enemy.kind == EnemyKind.SpearGuard)) direction = new Vector2(-offset.normalized.y, offset.normalized.x);
                    if (ranged && offset.magnitude >= 3 && offset.magnitude < 5) direction = new Vector2(-direction.y, direction.x) * .55f;
                    Vector2 next = MoveEnemyWithinRoom(enemy.position, direction * enemy.speed * dt, enemy.radius);
                    if ((next - enemy.position).sqrMagnitude < .00001f) next = MoveEnemyWithinRoom(enemy.position, new Vector2(-direction.y, direction.x) * enemy.speed * dt, enemy.radius);
                    enemy.position = next;
                }
            }
            enemy.velocity=dt>0?(enemy.position-previous)/dt:Vector2.zero;
            UpdateJourneyEnemyPose(enemy, (enemy.position - previous).sqrMagnitude > .000001f);UpdateEliteSigil(enemy);
        }

        private void BeginJourneyAttack(Enemy enemy)
        {
            if(enemy.boss||enemy.elite)Sfx("warn",true,.4f);
            int depth = journeyRooms[currentRoom].depth;
            enemy.attackDirection = (position - enemy.position).sqrMagnitude < .01f ? Vector2.down : (position - enemy.position).normalized;
            enemy.attackNumber++; enemy.actionStarted = clock;
            float windup = enemy.boss ? .9f : enemy.kind == EnemyKind.SpearGuard ? .68f : enemy.kind == EnemyKind.Tumbleweed ? .72f : .7f;
            enemy.windup = windup;
            enemy.nextAttack = clock + windup + (enemy.boss ? enemy.secondPhase ? 1.25f : 1.8f : Mathf.Max(.85f, 1.5f - depth * .035f));
            int pattern = (enemy.attackNumber - 1) % 3;
            bool targetMark = enemy.kind == EnemyKind.ApeKing && pattern == 0 || enemy.kind == EnemyKind.WindScorpion && pattern == 0;
            enemy.strikePoint = targetMark ? position : enemy.position + enemy.attackDirection * (enemy.kind == EnemyKind.SpearGuard ? 2.1f : 1.2f);
            if(enemy.warning!=null)Destroy(enemy.warning);
            if(enemy.eliteKind==2){enemy.strikePoint=position;enemy.warning=WarningCircle(enemy.strikePoint,.95f);enemy.windup=1.0f;}
            else if(enemy.kind==EnemyKind.SpearGuard){Vector2 center=enemy.position+enemy.attackDirection*1.05f;enemy.warning=Stretch("Spear thrust warning",square,center,new Vector2(2.3f,.55f),new Color(1,.32f,.12f,.38f),-638);enemy.warning.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(enemy.attackDirection.y,enemy.attackDirection.x)*Mathf.Rad2Deg);}
            else if(enemy.kind==EnemyKind.WindScorpion&&pattern==1){enemy.warning=WarningSector(enemy.position,enemy.attackDirection,2.4f);enemy.windup=.95f;}
            else if(enemy.kind==EnemyKind.Colossus&&pattern==0){enemy.strikePoint=enemy.position;enemy.warning=WarningCircle(enemy.position,enemy.secondPhase?4.5f:3.8f);enemy.windup=1.1f;}
            else if(enemy.kind==EnemyKind.StoneSentinel){enemy.strikePoint=enemy.position;enemy.warning=WarningCircle(enemy.position,1.6f);enemy.windup=1.0f;}
            else if(targetMark){enemy.warning=WarningCircle(enemy.strikePoint,enemy.kind==EnemyKind.ApeKing?1.25f:1.15f);enemy.windup=1.1f;}
            else enemy.warning=WarningSector(enemy.position,enemy.attackDirection,enemy.boss?4:2.0f);
            enemy.nextAttack=Mathf.Max(enemy.nextAttack,clock+enemy.windup+(enemy.boss?1.7f:1.2f));

        }

        private void ResolveJourneyAttack(Enemy enemy)
        {
            enemy.actionStarted = clock; enemy.recoveryUntil = clock + (enemy.boss ? .75f : .65f);
            int pattern = (enemy.attackNumber - 1) % 3;
            switch (enemy.kind)
            {
                case EnemyKind.SandScorpion: enemy.chargeUntil = clock + .22f; break;
                case EnemyKind.Tumbleweed: enemy.chargeUntil = clock + .52f; break;
                case EnemyKind.SpearGuard:
                    if (DistanceToSegment(position, enemy.position, enemy.strikePoint) < .5f && !ProjectilePathBlocked(enemy.position, position, .04f)) TakeJourneyDamage(enemy.boss ? 2 : 1, enemy.attackElement);
                    break;
                case EnemyKind.SandKite: if(enemy.eliteKind==2)JourneyAreaHit(enemy,enemy.strikePoint,.95f);else JourneyFan(enemy,2,12); break;
                case EnemyKind.StoneSentinel:
                    JourneyAreaHit(enemy, enemy.position, 1.6f); if (enemy.attackNumber % 2 == 0) JourneyFan(enemy, 3, 19); break;
                case EnemyKind.WindScorpion:
                    if (pattern == 0) JourneyAreaHit(enemy, enemy.strikePoint, 1.15f);
                    else if (pattern == 1) {Vector2 off=position-enemy.position;if(off.magnitude<2.7f&&Vector2.Dot(off.normalized,enemy.attackDirection)>Mathf.Cos(55*Mathf.Deg2Rad)&&!ProjectilePathBlocked(enemy.position,position,.02f))TakeJourneyDamage(2,enemy.attackElement);}
                    else JourneyFan(enemy, enemy.secondPhase ? 7 : 5, 16);
                    break;
                case EnemyKind.Colossus:
                    if (pattern == 0) SpawnGroundShock(enemy);
                    else if (pattern == 1) JourneyFan(enemy, enemy.secondPhase ? 9 : 7, 14);
                    else SummonJourneyAdds(EnemyKind.SpearGuard, enemy.secondPhase ? 2 : 1);
                    break;
                case EnemyKind.ApeKing:
                    if (pattern == 0)
                    {
                        Vector2 landing = SafeEnemyLanding(enemy,enemy.strikePoint);
                        if (!Blocked(landing, enemy.radius)) enemy.position = landing;
                        JourneyAreaHit(enemy, enemy.position, 1.25f);
                    }
                    else if (pattern == 1) JourneyFan(enemy, enemy.secondPhase ? 5 : 3, 19);
                    else SummonJourneyAdds(EnemyKind.Tumbleweed, 2);
                    break;
            }
        }

        private void JourneyFan(Enemy enemy, int count, float spacing)
        {
            Vector2 direction = enemy.attackDirection;
            for (int i = 0; i < count; i++)
            {
                Vector2 aim = Quaternion.Euler(0, 0, (i - (count - 1) * .5f) * spacing) * direction;
                int before = shots.Count;
                SpawnShot(enemy.position + aim * (enemy.boss ? .95f : .45f), aim, true, null, enemy.position);
                if (shots.Count > before) { shots[shots.Count - 1].element = enemy.attackElement; shots[shots.Count - 1].damage = enemy.boss ? 2 : 1; }
                if (enemy.eliteKind==2&&shots.Count>before)shots[shots.Count-1].root.GetComponent<SpriteRenderer>().color=C("eec47b");
                if (enemy.kind == EnemyKind.ApeKing && shots.Count > before) shots[shots.Count - 1].root.GetComponent<SpriteRenderer>().color = C("b68a58");
            }
        }

        private void SummonJourneyAdds(EnemyKind kind, int count)
        {
            var random = new System.Random(activeSeed ^ (int)(clock * 100) ^ currentRoom);
            for (int i = 0; i < count && enemies.Count < 7; i++){QueueEnemy(kind, FindJourneySpawn(random, .34f));pendingSpawns[pendingSpawns.Count-1].summoned=true;}
        }

        private void JourneyAreaHit(Enemy enemy, Vector2 at, float radius)
        {
            if (Vector2.Distance(position, at) < radius + .3f && !ProjectilePathBlocked(at, position, .04f)) TakeJourneyDamage(enemy.boss ? 2 : 1, enemy.attackElement);
            GameObject pulse = Draw("Impact pulse", softDisc ?? disc, at, Vector2.one * radius * 2, new Color(1, .73f, .4f, .65f), -640);
            fadingBodies.Add(new FadingBody { root = pulse, renderer = pulse.GetComponent<SpriteRenderer>(), expires = clock + .25f });
        }

        private static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            Vector2 line = b - a; float t = line.sqrMagnitude < .000001f ? 0 : Mathf.Clamp01(Vector2.Dot(point - a, line) / line.sqrMagnitude);
            return Vector2.Distance(point, a + t * line);
        }

        private void UpdateJourneyEnemyPose(Enemy enemy, bool moving)
        {
            Sprite[] art;
            if (enemyActionArt.TryGetValue(enemy.kind, out art))
            {
                int frame = clock < enemy.hurtUntil ? 6 : enemy.windup > 0 ? 4 : enemy.chargeUntil > clock || clock < enemy.recoveryUntil && clock - enemy.actionStarted < .22f ? 5 : moving ? Mathf.FloorToInt(clock * 8 + enemy.maximum) % 4 : 0;
                enemy.body.sprite = art[frame];
            }
            enemy.root.transform.position = enemy.position;
            enemy.body.sortingOrder = DepthAt(enemy.position.y);
            enemy.body.flipX = (enemy.windup > 0 ? enemy.attackDirection.x : position.x - enemy.position.x) < 0;
            enemy.body.color = clock < enemy.hurtUntil ? new Color(1, .7f, .6f) : Color.white;
            if (enemy.shadow != null) enemy.shadow.transform.position = enemy.position;
        }

        private void OnEnemyHit(Enemy enemy, float amount)
        {
            enemy.hurtUntil = clock + .13f; cameraShakeUntil = clock + .08f;
            AddFloatingHit(Mathf.CeilToInt(amount).ToString(), enemy.position + Vector2.up * .7f, C("fff0bb")); HitAudio(enemy);
            if (enemy.hp <= 0)
            {
                Sprite[] art; Sprite sprite = enemyActionArt.TryGetValue(enemy.kind, out art) ? art[IsThunderEnemy(enemy.kind)?art.Length-1:7] : enemy.body.sprite;
                GameObject body = Draw("Defeated " + EnemyNames[(int)enemy.kind], sprite, enemy.position, Vector2.one, Color.white, enemy.body.sortingOrder);
                // Both objects share the world parent: keep the actor's authored scale during the pose change.
                body.transform.localScale = enemy.root.transform.localScale;
                body.transform.localRotation = enemy.root.transform.localRotation;
                SpriteRenderer renderer = body.GetComponent<SpriteRenderer>(); renderer.flipX = enemy.body.flipX; renderer.flipY = enemy.body.flipY;
                fadingBodies.Add(new FadingBody { root = body, renderer = renderer, expires = clock + .55f });
            }
            else if ((int)enemy.kind < 4 && !enemy.elite && enemy.windup > 0)
            {
                enemy.windup = 0; if (enemy.warning != null) Destroy(enemy.warning); enemy.recoveryUntil = clock + .25f;
            }
        }

        private void AddFloatingHit(string text, Vector2 at, Color color)
        {
            if (floatingHits.Count > 63) floatingHits.RemoveAt(0);
            floatingHits.Add(new FloatingHit { text = text, at = at, color = color, expires = clock + .65f });
        }

        private void TickJourneyEffects(float dt)
        {
            for (int i = floatingHits.Count - 1; i >= 0; i--) { floatingHits[i].at += Vector2.up * dt * .5f; if (floatingHits[i].expires <= clock) floatingHits.RemoveAt(i); }
            for (int i = fadingBodies.Count - 1; i >= 0; i--)
            {
                FadingBody body = fadingBodies[i]; float left = body.expires - clock;
                if (left <= 0 || body.root == null) { if (body.root != null) Destroy(body.root); fadingBodies.RemoveAt(i); }
                else { Color c = body.renderer.color; c.a = Mathf.Clamp01(left / .4f); body.renderer.color = c; }
            }
            FollowExplorationCamera(dt);
        }

        private void ClearJourneyEffects()
        {
            ClearChapterSkillEffects();
            foreach (FadingBody body in fadingBodies) if (body.root != null) Destroy(body.root);
            foreach(var g in groundShocks)if(g.root!=null)Destroy(g.root);groundShocks.Clear();
            fadingBodies.Clear(); floatingHits.Clear(); nextSoundAt = cameraShakeUntil = 0;
        }

        private void SetupJourneyAudio()
        {
            if (journeyAudio == null) journeyAudio = gameObject.AddComponent<AudioSource>();
            journeyAudio.Stop(); journeyAudio.playOnAwake = false; journeyAudio.volume = 0;SetupSoundscape();
            hitSound = JourneyTone("Sword hit", 510, .07f); hurtSound = JourneyTone("Player hurt", 165, .16f);
        }

        private AudioClip JourneyTone(string name, float pitch, float duration)
        {
            int count = Mathf.RoundToInt(22050 * duration); float[] samples = new float[count];
            for (int i = 0; i < count; i++) { float t = i / 22050f; samples[i] = Mathf.Sin(t * pitch * Mathf.PI * 2) * Mathf.Pow(1 - i / (float)count, 2) * .4f; }
            AudioClip clip = AudioClip.Create(name, count, 1, 22050, false); clip.SetData(samples, 0); ownedAssets.Add(clip); return clip;
        }

        private void PlayJourneySound(bool hurt)
        {
            Sfx(hurt?"hurt":"flesh",hurt,.7f);
        }
    }
}
