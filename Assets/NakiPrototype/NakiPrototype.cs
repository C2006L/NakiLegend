using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NakiLegend.Prototype
{
    // Attach to one empty GameObject. Generated art is loaded from Resources/NakiArt.
    public sealed partial class NakiPrototype : MonoBehaviour
    {
        [Header("Player tuning")]
        public float moveSpeed = 4.2f;
        public float dashSpeed = 12f;
        public float swordDamage = 18f;
        public float spellDamage = 15f;

        private sealed class Enemy
        {
            public GameObject root;
            public SpriteRenderer body;
            public Vector2 position;
            public float hp, maximum, radius, speed, nextAttack, windup;
            public bool boss;
            public Vector2 strikePoint;
            public GameObject warning;
        }

        private sealed class Shot
        {
            public GameObject root;
            public Vector2 position, direction;
            public Enemy target;
            public float life;
            public bool hostile;
        }

        private readonly List<Enemy> enemies = new List<Enemy>();
        private readonly List<Shot> shots = new List<Shot>();
        private readonly List<Rect> obstacles = new List<Rect>();
        private readonly List<Object> ownedAssets = new List<Object>();
        private Transform world;
        private Camera view;
        private bool ownsCamera;
        private GameObject hero, sword;
        private SpriteRenderer heroBody;
        private Sprite square, disc, heroSprite, smallEnemySprite, skeletonSprite, bossSprite;
        private Vector2 position, facing = Vector2.right, dashDirection;
        private float health, energy, clock, nextSword, nextSpell, nextDash, dashUntil, invulnerableUntil;
        private float swordUntil, flashUntil;
        private bool paused, won, bossSpawned;
        private int defeated;

        private static Color C(string hex) { Color color; ColorUtility.TryParseHtmlString("#" + hex, out color); return color; }

        private void Start()
        {
            square = MakeSprite(new[] { "x" }, new Dictionary<char, Color> { { 'x', Color.white } }, 1);
            disc = MakeSprite(new[] { "..xxxx..", ".xxxxxx.", "xxxxxxxx", "xxxxxxxx", "xxxxxxxx", "xxxxxxxx", ".xxxxxx.", "..xxxx.." },
                new Dictionary<char, Color> { { 'x', Color.white } }, 8);
            var palette = new Dictionary<char, Color> {
                { 'h', C("252435") }, { 's', C("efb88c") }, { 'b', C("304e79") },
                { 'i', C("eee0ba") }, { 'l', C("755034") }, { 'g', C("e8b455") },
                { 'd', C("382d2c") }, { 't', C("58d9d0") }, { 'r', C("a66a3e") }
            };
            heroSprite = MakeSprite(new[] {
                ".....hhhh.......", "....hhhhhh......", "...hhhhhhhh.....", "...hssssssh.....",
                "....shssh.......", "....ssss........", "...bbbbbb.......", "..bbbbbbgb......",
                ".bbbibiiibb.....", "..bsiiiiisb.....", "...liiiill......", "...lllllll......",
                "....dddd........", "...dd..dd.......", "...ll..ll.......", "..lll..lll......"
            }, palette, 16);
            smallEnemySprite = MakeSprite(new[] {
                "..........rr....", ".........r..r...", ".........r..t...", "..r......r......",
                ".rrr....rr......", "..rr.rrrrr......", "...rrrrrrrr.....", "..rrrtrtrrrr....",
                ".rr.rrrrrr.rr...", "....r.rr.r......", "...r..r...r.....", "................"
            }, palette, 16);
            skeletonSprite = MakeSprite(new[] {
                ".....iiii.......", "....iiiiii......", "....ididdi......", ".....iiii.......",
                "......ii....l...", "....bbbbb...l...", "...bibibib..l...", "...i.iii.i..l...",
                ".....iii....l...", ".....lil....l...", "....ii.ii...l...", "....dd.dd......."
            }, palette, 16);
            bossSprite = MakeSprite(new[] {
                "...............rrrrr....", "..............rr...rr...", "..............r.....r...", "..............r.....t...",
                "..............rr...ttt..", "...............rr...t...", "...rrr..........rr......", "..rrrrr...rrrr...rr.....",
                ".rr..rr..rrrrrr.rr......", ".rr...rrrrrbrrrrr.......", "..rr..rrrrbbrrrrr.......", "...rrrrrrrbbrrrrrr......",
                "....rrrrrtbbtrrrrrrr....", "...rr.rrrrbbrrrr..rrr...", "..rr..rrrrrrrrrr..r.rr..", ".rr....rrrrrrrr...r..rr.",
                ".......r.rr.r....rrrr...", "......r..rr..r..........", ".....r........r.........", "........................"
            }, palette, 8);
            LoadIllustratedAssets();
            LoadPixelHero();
            view = Camera.main;
            if (view == null) { var obj = new GameObject("Prototype Camera"); obj.tag = "MainCamera"; view = obj.AddComponent<Camera>(); ownsCamera = true; }
            view.orthographic = true;
            view.transform.position = new Vector3(0, 0, -10);
            view.transform.rotation = Quaternion.identity;
            view.clearFlags = CameraClearFlags.SolidColor;
            view.backgroundColor = C("252d36");
            view.cullingMask = ~0;
            ResetRoom();
        }

        private Sprite MakeSprite(string[] rows, Dictionary<char, Color> palette, float pixelsPerUnit)
        {
            int width = rows[0].Length;
            var texture = new Texture2D(width, rows.Length, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < rows.Length; y++)
                for (int x = 0; x < width; x++)
                {
                    Color value;
                    texture.SetPixel(x, rows.Length - y - 1, palette.TryGetValue(rows[y][x], out value) ? value : Color.clear);
                }
            texture.Apply();
            var sprite = Sprite.Create(texture, new Rect(0, 0, width, rows.Length), new Vector2(.5f, .5f), pixelsPerUnit);
            ownedAssets.Add(sprite); ownedAssets.Add(texture);
            return sprite;
        }

        private GameObject Draw(string label, Sprite sprite, Vector2 at, Vector2 scale, Color color, int order)
        {
            var obj = new GameObject(label);
            obj.transform.SetParent(world, false);
            obj.transform.position = at;
            obj.transform.localScale = scale;
            var renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite; renderer.color = color; renderer.sortingOrder = order;
            return obj;
        }

        private void ResetRoom()
        {
            if (world != null) { world.gameObject.SetActive(false); Destroy(world.gameObject); }
            world = new GameObject("Generated Prototype Room").transform;
            world.SetParent(transform, false);
            enemies.Clear(); shots.Clear(); obstacles.Clear();
            clock = 0; health = 6; energy = 100; defeated = 0;
            paused = false; won = false; bossSpawned = false;
            swordHitPending = false;
            nextSword = nextSpell = nextDash = dashUntil = invulnerableUntil = swordUntil = flashUntil = 0;
            position = new Vector2(0, -2.6f); facing = Vector2.right;
            if (illustratedArtReady) BuildIllustratedRoom();
            else
            {
            Draw("Sandstone floor", square, Vector2.zero, new Vector2(19, 11), C("b99461"), -100);
            for (int y = -5; y <= 5; y++)
                for (int x = -9; x <= 9; x++)
                    Draw("Floor tile", square, new Vector2(x, y), Vector2.one * .96f,
                        (x + y) % 2 == 0 ? C("c4a171") : C("c09c6a"), -99);
            Draw("North wall", square, new Vector2(0, 5.8f), new Vector2(20, .6f), C("725740"), -50);
            Draw("South wall", square, new Vector2(0, -5.8f), new Vector2(20, .6f), C("725740"), -50);
            Draw("West wall", square, new Vector2(-9.8f, 0), new Vector2(.6f, 12.2f), C("725740"), -50);
            Draw("East wall", square, new Vector2(9.8f, 0), new Vector2(.6f, 12.2f), C("725740"), -50);
            Draw("Sealed door", square, new Vector2(0, 5.7f), new Vector2(2, .7f), C("3e7178"), -49);
            foreach (Vector2 at in new[] { new Vector2(-6, 3), new Vector2(6, 3), new Vector2(-6, -3), new Vector2(6, -3) })
            {
                obstacles.Add(new Rect(at.x - .55f, at.y - .55f, 1.1f, 1.1f));
                Draw("Pillar base", square, at, Vector2.one * 1.1f, C("826649"), -20);
                Draw("Pillar top", square, at + Vector2.up * .16f, Vector2.one * .8f, C("dfbf87"), -19);
            }
            }
            hero = Draw("Young spellblade", heroSprite, position, Vector2.one, Color.white, 10);
            heroBody = hero.GetComponent<SpriteRenderer>();
            sword = Draw("Sword strike", square, position, new Vector2(1.25f, .13f), C("fff0ae"), 30);
            sword.SetActive(false);
            SetupIllustratedHero();
            SpawnEnemy(new Vector2(-3.5f, 1), false, false);
            SpawnEnemy(new Vector2(3.5f, 1.8f), false, true);
        }

        private void SpawnEnemy(Vector2 at, bool boss, bool skeleton)
        {
            var enemy = new Enemy { position = at, hp = boss ? 280 : 48, maximum = boss ? 280 : 48,
                radius = boss ? .95f : .35f, speed = boss ? 1.05f : skeleton ? 1.75f : 1.4f,
                boss = boss, nextAttack = clock + 1.5f };
            enemy.root = Draw(boss ? "Wind Scorpion Boss" : skeleton ? "Spear skeleton" : "Sand scorpion",
                boss ? bossSprite : skeleton ? skeletonSprite : smallEnemySprite, at, Vector2.one, Color.white, 8);
            enemy.body = enemy.root.GetComponent<SpriteRenderer>();
            enemies.Add(enemy);
        }

        private void Update()
        {
            if (world == null) return;
            view.orthographicSize = Mathf.Max(6.8f, 10.5f / Mathf.Max(view.aspect, .1f));
            if (Pressed('r')) { ResetRoom(); return; }
            if (Pressed('e')) paused = !paused;
            if (paused) return;
            if (health <= 0 || won) { UpdateIllustratedHero(Vector2.zero); return; }
            float dt = Mathf.Min(Time.deltaTime, .05f);
            clock += dt;
            energy = Mathf.Min(100, energy + 14 * dt);
            Vector2 input = new Vector2((Held('d') ? 1 : 0) - (Held('a') ? 1 : 0), (Held('w') ? 1 : 0) - (Held('s') ? 1 : 0)).normalized;
            if (input.sqrMagnitude > 0 && clock >= dashUntil) facing = input;
            if (Pressed('k') && clock >= nextDash)
            {
                BeginDodge(input);
            }
            position = Move(position, (clock < dashUntil ? dashDirection * dashSpeed : input * moveSpeed) * dt, .3f);
            hero.transform.position = position;
            heroBody.flipX = facing.x < -.1f;
            heroBody.color = clock < flashUntil ? C("ff7777") : clock < invulnerableUntil ? C("a9e9ff") : Color.white;
            if (Held('j') && clock >= nextSword && clock >= dashUntil && clock >= castPoseUntil) Swing();
            if (Held('l') && clock >= nextSpell && energy >= 25 && clock >= dashUntil && clock >= swordUntil) Cast();
            UpdateSwordImpact();
            sword.SetActive(clock < swordUntil);
            if (clock < swordUntil) sword.transform.position = position + (Vector2)sword.transform.right * .7f;
            UpdateIllustratedHero(input);

            for (int i = enemies.Count - 1; i >= 0; i--) UpdateEnemy(enemies[i], dt);
            for (int i = shots.Count - 1; i >= 0; i--) UpdateShot(i, dt);
            if (!bossSpawned && defeated >= 2)
            {
                bossSpawned = true;
                SpawnEnemy(new Vector2(0, 3.2f), true, false);
                energy = 100;
            }
        }

        private Vector2 Move(Vector2 from, Vector2 delta, float radius)
        {
            Vector2 next = from;
            Rect limits = illustratedArtReady ? new Rect(-9.4f, -3.85f, 18.8f, 7.65f) : new Rect(-9.4f, -5.4f, 18.8f, 10.8f);
            Vector2 candidate = new Vector2(Mathf.Clamp(from.x + delta.x, limits.xMin + radius, limits.xMax - radius), from.y);
            if (!Blocked(candidate, radius)) next.x = candidate.x;
            candidate = new Vector2(next.x, Mathf.Clamp(from.y + delta.y, limits.yMin + radius, limits.yMax - radius));
            if (!Blocked(candidate, radius)) next.y = candidate.y;
            return next;
        }

        private bool Blocked(Vector2 at, float radius)
        {
            foreach (Rect obstacle in obstacles)
            {
                Vector2 closest = new Vector2(Mathf.Clamp(at.x, obstacle.xMin, obstacle.xMax), Mathf.Clamp(at.y, obstacle.yMin, obstacle.yMax));
                if ((at - closest).sqrMagnitude < radius * radius) return true;
            }
            return false;
        }

        private Enemy FindTarget(float range, bool frontOnly)
        {
            Enemy best = null;
            float distance = range;
            foreach (Enemy enemy in enemies)
            {
                Vector2 offset = enemy.position - position;
                float current = offset.magnitude;
                if (current >= distance || (frontOnly && Vector2.Dot(facing, offset.normalized) < .15f)) continue;
                best = enemy; distance = current;
            }
            return best;
        }

        private void Swing()
        {
            nextSword = clock + .32f; swordUntil = clock + .32f;
            Enemy aim = FindTarget(2.1f, true);
            Vector2 direction = aim != null ? (aim.position - position).normalized : facing;
            swingDirection = direction; swingStarted = clock;
            sword.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            swordHitAt = clock + .105f;
            swordHitPending = true;
        }

        private void Cast()
        {
            energy -= 25; nextSpell = clock + .65f;
            Enemy target = FindTarget(14, false);
            Vector2 direction = target != null ? (target.position - position).normalized : facing;
            castDirection = direction; castPoseUntil = clock + .32f;
            for (int i = -1; i <= 1; i++)
            {
                Vector2 spread = Quaternion.Euler(0, 0, i * 15) * direction;
                SpawnShot(position + direction * .45f, spread, false, target);
            }
        }

        private void SpawnShot(Vector2 at, Vector2 direction, bool hostile, Enemy target)
        {
            var shot = new Shot { position = at, direction = direction, target = target, hostile = hostile, life = hostile ? 5 : 2.8f };
            shot.root = Draw(hostile ? "Wind projectile" : "Light orb", disc, at, Vector2.one * (hostile ? .27f : .22f),
                hostile ? C("38d6ce") : C("fff2ab"), illustratedArtReady ? 700 : 25);
            shots.Add(shot);
        }

        private void UpdateEnemy(Enemy enemy, float dt)
        {
            if (enemy.windup > 0)
            {
                enemy.windup -= dt;
                if (enemy.windup <= 0)
                {
                    if (Vector2.Distance(position, enemy.strikePoint) < (enemy.boss ? 1.55f : .85f) + .3f) Hurt();
                    if (enemy.warning != null) Destroy(enemy.warning);
                    if (enemy.boss)
                    {
                        Vector2 aim = (position - enemy.position).normalized;
                        for (int i = -2; i <= 2; i++) SpawnShot(enemy.position + aim * 1.1f,
                            Quaternion.Euler(0, 0, i * 18) * aim, true, null);
                    }
                }
                return;
            }
            Vector2 offset = position - enemy.position;
            if (clock >= enemy.nextAttack && (enemy.boss || offset.magnitude < 1.15f))
            {
                enemy.nextAttack = clock + (enemy.boss ? 2.5f : 1.5f);
                enemy.windup = enemy.boss ? .9f : .55f;
                enemy.strikePoint = enemy.boss ? enemy.position + offset.normalized * 1.4f : position;
                enemy.warning = Draw("Attack warning", disc, enemy.strikePoint, Vector2.one * (enemy.boss ? 3.1f : 1.7f),
                    new Color(1, .35f, .1f, .4f), -10);
            }
            else if (offset.magnitude > enemy.radius + .4f)
            {
                Vector2 direction = offset.normalized;
                Vector2 next = Move(enemy.position, direction * enemy.speed * dt, enemy.radius);
                // Basic wall-following fallback for the four test pillars.
                if ((next - enemy.position).sqrMagnitude < .00001f)
                    next = Move(enemy.position, new Vector2(-direction.y, direction.x) * enemy.speed * dt, enemy.radius);
                enemy.position = next;
            }
            enemy.root.transform.position = enemy.position;
            if (illustratedArtReady) enemy.body.sortingOrder = DepthAt(enemy.position.y - .3f);
            enemy.body.flipX = offset.x < 0;
        }

        private void UpdateShot(int index, float dt)
        {
            Shot shot = shots[index]; shot.life -= dt;
            if (!shot.hostile && (shot.target == null || shot.target.hp <= 0)) shot.target = FindTarget(14, false);
            if (!shot.hostile && shot.target != null)
            {
                Vector2 wanted = (shot.target.position - shot.position).normalized;
                shot.direction = Vector2.Lerp(shot.direction, wanted, 5 * dt).normalized;
            }
            shot.position += shot.direction * (shot.hostile ? 3.5f : 7f) * dt;
            bool remove = shot.life <= 0 || Mathf.Abs(shot.position.x) > 9.4f ||
                shot.position.y > (illustratedArtReady ? 3.8f : 5.4f) || shot.position.y < (illustratedArtReady ? -3.85f : -5.4f) || Blocked(shot.position, .1f);
            if (!remove && shot.hostile && Vector2.Distance(shot.position, position) < .4f) { Hurt(); remove = true; }
            if (!remove && !shot.hostile)
                for (int i = enemies.Count - 1; i >= 0; i--)
                    if (Vector2.Distance(shot.position, enemies[i].position) < enemies[i].radius + .15f)
                    { DamageEnemy(enemies[i], spellDamage); remove = true; break; }
            if (remove) { Destroy(shot.root); shots.RemoveAt(index); }
            else shot.root.transform.position = shot.position;
        }

        private void DamageEnemy(Enemy enemy, float amount)
        {
            enemy.hp -= amount;
            if (enemy.hp > 0) return;
            if (enemy.warning != null) Destroy(enemy.warning);
            Destroy(enemy.root); enemies.Remove(enemy); defeated++;
            energy = Mathf.Min(100, energy + 15);
            if (enemy.boss) won = true;
        }

        private void Hurt()
        {
            if (clock < invulnerableUntil || health <= 0) return;
            health = Mathf.Max(0, health - 1);
            invulnerableUntil = clock + .85f; flashUntil = clock + .15f;
            pixelHurtUntil = clock + .2f;
            swordHitPending = false; swordUntil = 0;
        }

        private static bool Held(char key)
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;
            switch (key) {
                case 'w': return keyboard.wKey.isPressed; case 'a': return keyboard.aKey.isPressed;
                case 's': return keyboard.sKey.isPressed; case 'd': return keyboard.dKey.isPressed;
                case 'j': return keyboard.jKey.isPressed; case 'l': return keyboard.lKey.isPressed;
            }
            return false;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKey(key.ToString());
#else
            return false;
#endif
        }

        private static bool Pressed(char key)
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;
            switch (key) {
                case 'k': return keyboard.kKey.wasPressedThisFrame;
                case 'r': return keyboard.rKey.wasPressedThisFrame;
                case 'e': return keyboard.escapeKey.wasPressedThisFrame;
            }
            return false;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(key == 'e' ? KeyCode.Escape : key == 'k' ? KeyCode.K : KeyCode.R);
#else
            return false;
#endif
        }

        private void OnGUI()
        {
            DrawPrototypeHud();
        }

        private void OnDestroy()
        {
            if (world != null) Destroy(world.gameObject);
            if (ownsCamera && view != null) Destroy(view.gameObject);
            foreach (Object asset in ownedAssets) if (asset != null) Destroy(asset);
        }
    }
}
