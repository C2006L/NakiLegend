using UnityEngine;

namespace NakiLegend.Prototype
{
    public sealed partial class NakiPrototype
    {
        private enum Element { Physical, Light, Wind, Lightning, Ice, Fire, Wood, Dark }
        [Header("Player shield")]
        public float maximumShield = 4;
        public bool lightningTrialMode;
        private float shield, lastDamageAt = -10, windGuardUntil, waveReadyAt, bladeReadyAt;
        private bool orbLearned, bladeLearned, waveLearned, windEmpowered;
        private int clearedCombatRooms, pendingLesson;
        private float hintReadyAt;
        private GameObject lessonAltar, activeWindWave;
        private Vector2 lessonPosition;
        private float waveStartedAt;
        private Sprite windRingSprite;

        private void ResetPlayerProgression()
        {
            shield = EffectiveMaximumShield; lastDamageAt = -10; windGuardUntil = waveReadyAt = bladeReadyAt = 0;
            orbLearned = bladeLearned = waveLearned = windEmpowered = false; clearedCombatRooms = pendingLesson = 0;
            hintReadyAt = 0; journeyEnergyRegen = 6;journeyToast=null;journeyToastUntil=0;
            if (bossPracticeMode) { orbLearned = bladeLearned = waveLearned = true; clearedCombatRooms = 12; }
            ResetEquipment();ResetPowers();ResetBackpack();
            if(gatehouseSampleMode){orbLearned=bladeLearned=waveLearned=true;carriedWeapons[1]=6;}
        }

        private void TickPlayerDefenses(float dt)
        {
            bool recovering=clock-lastDamageAt>=5&&shield<EffectiveMaximumShield;
            if(recovering&&!shieldWasRecovering)Sfx("recover",false,.35f);shieldWasRecovering=recovering;
            if(recovering)shield=Mathf.Min(EffectiveMaximumShield,shield+dt);
            if (activeWindWave != null)
            {
                float phase = (clock - waveStartedAt) / .35f;
                if (phase >= 1) { Destroy(activeWindWave); activeWindWave = null; }
                else
                {
                    float radius = Mathf.Lerp(.15f, 2.3f, phase);
                    activeWindWave.transform.localScale = Vector3.one * radius * 2;
                    activeWindWave.GetComponent<SpriteRenderer>().color = new Color(.4f, 1, .85f, 1 - phase * .7f);
                    DispelProjectiles((Vector2)activeWindWave.transform.position, radius);
                }
            }
        }

        private void TakeJourneyDamage(float amount, Element element)
        {
            if (clock < invulnerableUntil || health <= 0) return;
            if (element == Element.Lightning && clock < windGuardUntil) amount *= WindGuardMultiplier;
            float before = shield, absorbed = Mathf.Min(shield, amount);
            shield -= absorbed; health = Mathf.Max(0, health - (amount - absorbed)); lastDamageAt = clock;
            invulnerableUntil = clock + .8f; flashUntil = clock + .13f; pixelHurtUntil = clock + .16f;
            swordHitPending = false; swordUntil = castPoseUntil = clock;
            if (before > 0 && shield <= .001f) AddFloatingHit("护盾破裂", position + Vector2.up, C("71cde5"));
            else if (absorbed > 0) AddFloatingHit("-" + amount.ToString("0.#"), position + Vector2.up, C("7bd6ee"));
            Sfx(before>0&&shield<=.001f?"break":absorbed>0?"shield":"hurt",true,.85f);if(health<=0)Sfx("death",true);
        }

        private static float ElementMultiplier(Element attack, Element target)
        {
            if (attack == Element.Wind && target == Element.Lightning) return 1.3f;
            if ((attack==Element.Lightning&&target==Element.Ice)||(attack==Element.Ice&&target==Element.Fire)||(attack==Element.Fire&&target==Element.Wood)||(attack==Element.Wood&&target==Element.Wind)) return 1.3f;
            return 1;
        }

        private void ApplyElementDamage(Enemy enemy, float amount, Element element)
        {
            amount=IceArmorDamage(enemy,amount,element);float multiplier = ElementMultiplier(element, enemy.affinity);
            if (multiplier > 1) AddFloatingHit(ElementName(element)+"克"+ElementName(enemy.affinity), enemy.position + Vector2.up * 1.1f, C("83edcf"));
            DamageEnemy(enemy, amount * multiplier);
            if (enemy.hp > 0 && element == Element.Wind && enemy.affinity == Element.Lightning && clock >= enemy.poiseReadyAt)
            {
                enemy.poise += 1;
                if (enemy.poise >= 3)
                {
                    enemy.poise = 0; enemy.poiseReadyAt = clock + 6;
                    if (enemy.windup > 0) { enemy.windup = 0; if (enemy.warning != null) Destroy(enemy.warning); enemy.recoveryUntil = clock + .7f; AddFloatingHit("破势", enemy.position + Vector2.up, C("a8f3dc")); }
                }
            }
        }

        private void CastJourneyOrb()
        {
            if(origin==Origin.Dark){CastDarkMeteor();return;}
            if (!orbLearned) { JourneyHint("掌心的光尚未凝聚 · 留意遗迹中的古井碑"); nextSpell = clock + .35f; return; }
            foreach (Shot shot in shots) if (shot.orb) return;
            if (clock < nextSpell || !SpendPower(PowerPool.Light,8)) return;
             nextSpell = clock + 1.2f;SkillSound("orb","cast");
            Enemy target; Vector2 direction=AutoAim(10,7,0,out target);
            int before = shots.Count; SpawnShot(position + direction * .4f, direction, false, target, position);
            if (shots.Count > before)
            {
                Shot shot = shots[shots.Count - 1];shot.skillAudio="orb"; shot.orb = true; shot.element = Element.Light; shot.damage = SkillDamage(24); shot.life = 1.8f; shot.speed = 7; shot.tracking = target != null;
                shot.root.transform.localScale = Vector3.one * .29f;
                shot.root.GetComponent<SpriteRenderer>().color=C("fff1b2");AddShotRibbon(shot,C("ecd18e"),.13f);SpellSpark(position+direction*.4f,C("fff1b2"),.28f);
            }
            castDirection = direction; castPoseUntil = clock + .32f;
        }

        private void CastWindBlade()
        {
            if (!bladeLearned) { JourneyHint("风之刃尚未学会 · 继续探索旧渠"); return; }
            if (clock < bladeReadyAt || clock < dashUntil || !SpendPower(PowerPool.Elemental,6)) return;
            Enemy target; Vector2 direction=AutoAim(10,9,0,out target); bladeReadyAt = clock + 1.2f;SkillSound("blade","cast");
            int before = shots.Count; SpawnShot(position + direction * .45f, direction, false, null, position);
            if (shots.Count > before)
            {
                Shot shot = shots[shots.Count - 1];shot.skillAudio="blade"; shot.tracking = false; shot.element = Element.Wind; shot.damage = SkillDamage(windEmpowered ? 26 : 22); shot.life = 1.4f; shot.speed = 9; shot.hitsLeft = 2;
                shot.root.GetComponent<SpriteRenderer>().sprite = pixelSlash ?? disc; shot.root.GetComponent<SpriteRenderer>().color = C("7ce7c4");
                shot.root.transform.localScale = new Vector3(.32f, .55f, 1); shot.root.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
                AddShotRibbon(shot,C("82e6cb"),.07f);
            }
            castDirection = direction; castPoseUntil = clock + .32f;
        }

        private void CastWindWave()
        {
            if (!waveLearned) { JourneyHint("风波尚未学会 · 寻找第二层的守渠试炼"); return; }
            if (clock < waveReadyAt || clock < dashUntil || !SpendPower(PowerPool.Elemental,10)) return;
             waveReadyAt = clock + (windEmpowered ? 4 : 4.5f); windGuardUntil = clock + WindGuardDuration; waveStartedAt = clock;SkillSound("wave","cast");
            if (activeWindWave != null) Destroy(activeWindWave);
            activeWindWave = Draw("Wind wave", windRingSprite ?? disc, position + Vector2.up * .25f, Vector2.one * .3f, C("8aefd0"), 800);
            DispelProjectiles(position, .4f); castDirection = facing; castPoseUntil = clock + .32f;
        }

        private void DispelProjectiles(Vector2 at, float radius)
        {
            for (int i = shots.Count - 1; i >= 0; i--)
                if (shots[i].hostile && shots[i].dispellable && Vector2.Distance(shots[i].position, at) <= radius)
                { SkillSound("wave","hit");Destroy(shots[i].root); shots.RemoveAt(i); }
        }

        private void JourneyHint(string message)
        {
            if (clock < hintReadyAt) return; hintReadyAt = clock + 1.1f;journeyToast=message;journeyToastUntil=clock+2.6f;
        }

        private void CheckPendingLesson()
        {
            if(ThunderActive||origin==Origin.Unchosen)return;
            pendingLesson = !orbLearned && clearedCombatRooms >= 3 ? 1 : !bladeLearned && clearedCombatRooms >= 6 ? 2 : !waveLearned && clearedCombatRooms >= 10 ? 3 : 0;
            if (pendingLesson == 0 || lessonAltar != null) return;
            lessonPosition = FindFreeInteractionPoint(new Vector2(-2.2f, -1.6f));
            lessonAltar = Draw("Wind bell teaching altar", lessonArtwork ?? doorArtwork ?? square, lessonPosition, Vector2.one * (lessonArtwork != null ? 1 : .45f), Color.white, DepthAt(lessonPosition.y));
        }

        private void LearnPendingLesson()
        {
            if (pendingLesson == 1&&origin==Origin.Dark) { orbLearned=true;TellStory("碑上的刻痕像一道道坠落的星。你试着抬手，右眼的紫光便沿着指尖散开。","少年：原来，石头里那些不肯熄灭的微光，可以这样引出来。");AnnounceDiscovery("已领悟 L 流星 · 消耗1灵魂能量，自动追踪"); }
            else if (pendingLesson == 1) { orbLearned = true; TellStory("碑文已经模糊，只剩一行浅刻：莫追逐光，让它先在心里停下。","你放慢呼吸。掌中的暖光渐渐收拢，凝成一颗明亮的光球。");AnnounceDiscovery("已领悟 L 元气弹 · 消耗8光之力，单颗追踪"); }
            if (pendingLesson == 2) { bladeLearned = true; TellStory("断壁间的风把细沙裁成一道弧线。你顺着那道弧挥剑，剑锋外竟也留下一缕长风。","少年：若能让风沿着剑走，就能劈开前面的阻碍。");AnnounceDiscovery("已领悟 U 风之刃 · 消耗6元素之力，风克雷"); }
            if (pendingLesson == 3) { waveLearned = true; TellStory("风铃台四周，沙尘总在靠近铜环时向外散去。你站进环中，试着把气息送向四周。","一圈清风从脚下荡开。纷乱的沙粒，被推到了身外。");AnnounceDiscovery("已领悟 I 护身风波 · 消耗10元素，吹散普通飞弹"); }
            if (lessonAltar != null) Destroy(lessonAltar); lessonAltar = null; pendingLesson = 0; if(origin==Origin.Light)energy = Mathf.Min(100, energy + 20);RequestCheckpoint();
        }

        private Vector2 FindFreeInteractionPoint(Vector2 preferred)
        {
            if (!Blocked(preferred, .5f) && !ProjectilePathBlocked(preferred, preferred, .3f)) return preferred;
            for (float y = -2.5f; y <= 2.5f; y += 1)
                for (float x = -5; x <= 5; x += 1.4f) { Vector2 at = new Vector2(x, y); if (!Blocked(at, .5f) && !ProjectilePathBlocked(at, at, .3f)) return at; }
            return Vector2.zero;
        }

        private void BuildWindRing()
        {
            var texture = new Texture2D(96, 96, TextureFormat.RGBA32, false); texture.filterMode = FilterMode.Point;
            for (int y = 0; y < 96; y++) for (int x = 0; x < 96; x++)
            {
                float radius = Vector2.Distance(new Vector2(x, y), new Vector2(47.5f, 47.5f));
                texture.SetPixel(x, y, radius > 40 && radius < 46 ? new Color(.7f, 1, .9f, .85f) : Color.clear);
            }
            texture.Apply(); windRingSprite = Sprite.Create(texture, new Rect(0, 0, 96, 96), Vector2.one * .5f, 96); ownedAssets.Add(texture); ownedAssets.Add(windRingSprite);
        }
    }
}
