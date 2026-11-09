using System.Collections.Generic;
using UnityEngine;

namespace NakiLegend.Prototype
{
    public sealed partial class NakiPrototype
    {
        private enum WeaponType { Sword, Spear, Bow, Staff }
        private sealed class WeaponDef
        {
            public string name; public WeaponType type; public Element element;
            public float damage, interval, range, cost, length, scatter;
            public Sprite sprite; public Color color;public bool bowFacesLeft;public int maxHits;
            public PowerPool pool=PowerPool.Elemental;public WeaponEffect effect;public int variant,family,sourceIsland;public bool signature;
        }
        private sealed class StoredWeapon { public int id; public Vector2 at; }
        private sealed class WeaponDrop { public StoredWeapon data; public GameObject root; }
        private WeaponDef[] weaponCatalog;
        private readonly int[] carriedWeapons = { 0, -1 };
        private readonly List<WeaponDrop> worldWeapons = new List<WeaponDrop>();
        private int equippedSlot;
        private float meleeActionDuration = .32f;
        private Sprite lessonArtwork, arrowSprite;
        private GameObject rewardChest;
        private Vector2 chestPosition;
        private bool layeredHeroReady;
        private WeaponDef CurrentWeapon { get { return weaponCatalog[carriedWeapons[equippedSlot]]; } }

        private void LoadEquipmentArt()
        {
            Sprite blade = swordArtwork;
            if (blade != null)
            {
                blade = Sprite.Create(blade.texture, blade.rect, new Vector2(.165f, .5f), blade.rect.width, 0, SpriteMeshType.FullRect); ownedAssets.Add(blade);
            }
            Sprite spear = EquipmentSprite("equipment-spear-v9", new Vector2(.30f, .5f), false) ?? blade;
            Sprite bow = EquipmentSprite("equipment-bow-v9", new Vector2(.32f,.5f), true) ?? blade;
            Sprite staff = EquipmentSprite("equipment-staff-v9", new Vector2(.105f, .5f), false) ?? blade;
            weaponCatalog = new[] {
                Weapon("旅人短剑",WeaponType.Sword,Element.Physical,18,.36f,1.45f,0,1,blade,"ffffff"),
                Weapon("断渠长矛",WeaponType.Spear,Element.Physical,20,.55f,2.3f,1,1.85f,spear,"ffffff"),
                Weapon("猎沙短弓",WeaponType.Bow,Element.Physical,12,.55f,10,1,1.05f,bow,"ffffff"),
                Weapon("旧木法杖",WeaponType.Staff,Element.Light,12,.65f,10,1,1.3f,staff,"ffffff"),
                Weapon("风纹剑",WeaponType.Sword,Element.Wind,18,.32f,1.55f,1,1.05f,blade,"85e7d2"),
                Weapon("裂岩重剑",WeaponType.Sword,Element.Physical,28,.65f,1.8f,2,1.35f,blade,"d4baa0"),
                Weapon("风牙法杖",WeaponType.Staff,Element.Wind,14,.7f,10,2,1.4f,staff,"85e7d2"),
                Weapon("守渠风矛",WeaponType.Spear,Element.Wind,22,.65f,2.5f,1,2.05f,spear,"85e7d2")
            };
            weaponCatalog[2].bowFacesLeft=true;weaponCatalog[2].scatter=3.5f;weaponCatalog[3].scatter=1.2f;weaponCatalog[6].scatter=.65f;
            lessonArtwork = WorldPropSprite(2);
            arrowSprite = MakeSprite(new[] { "..........kk...", "kkkkkkkkkkIIkk.", "kLLLLLLLLLIIIwk", "kkkkkkkkkkIIkk.", "..........kk..." },
                new Dictionary<char, Color> { { 'k', C("383147") }, { 'L', C("bd8749") }, { 'I', C("e6dac1") }, { 'w', Color.white } }, 18);
            BuildWindRing();
        }

        private Sprite EquipmentSprite(string name, Vector2 pivot, bool vertical)
        {
            Texture2D texture = Resources.Load<Texture2D>("NakiArt/" + name); if (texture == null) return null;
            texture.filterMode = FilterMode.Point;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), pivot, vertical ? texture.height : texture.width, 0, SpriteMeshType.FullRect);
            ownedAssets.Add(sprite); return sprite;
        }

        private static WeaponDef Weapon(string name, WeaponType type, Element element, float damage, float interval, float range, float cost, float length, Sprite sprite, string color)
        { return new WeaponDef { name = name, type = type, element = element, damage = damage, interval = interval, range = range, cost = cost, length = length, sprite = sprite, color = C(color) }; }

        private void ResetEquipment()
        {
            profession=Profession.Unchosen;professionFromSave=false;ramHits.Clear();
            equippedSlot = 0; carriedWeapons[0] = 0; carriedWeapons[1] = bossPracticeMode ? 4 : -1;
        }

        private void SwitchWeapon()
        {
            int other = 1 - equippedSlot;
            if (carriedWeapons[other] < 0) { JourneyHint("尚未携带第二把武器 · 靠近武器按 F 拾取"); return; }
            if (clock < dashUntil) return;
            CancelBowCharge();equippedSlot = other; swordHitPending = false; swordUntil = castPoseUntil = clock;
            nextSword = Mathf.Max(nextSword, clock + .15f);
        }

        private void UseEquippedWeapon()
        {
            WeaponDef weapon=CurrentWeapon;if(weapon.type==WeaponType.Staff){UseStaffVolley();return;}
            if(weapon.type==WeaponType.Sword||weapon.type==WeaponType.Spear){if(!CanUseWeapon(weapon)||!SpendPower(weapon.pool,weapon.cost)){nextSword=clock+.2f;return;}WeaponAudio();Swing();return;}
            bool bow=weapon.type==WeaponType.Bow,archer=profession==Profession.Archer&&bow;
            float charge=bow?Mathf.Clamp01(bowShotCharge):0,cost=weapon.cost+(weapon.cost>0&&charge>.15f?1:0);
            if(!CanUseWeapon(weapon,cost-weapon.cost)){nextSword=clock+.2f;return;}
            Enemy target;Vector2 aim=AutoAim(weapon.range,bow?11:8,weapon.scatter*(archer?.5f:1)*(1-charge*.8f),out target);
            int before=shots.Count;SpawnShot(position+aim*.5f,aim,false,null,position);
            if(shots.Count==before){nextSword=clock+.1f;return;}
            SpendPower(weapon.pool,cost);WeaponAudio();nextSword=clock+weapon.interval/(archer?1.1f:1);
            Shot shot=shots[shots.Count-1];shot.tracking=false;shot.damage=BowReleaseDamage(weapon,charge);shot.element=weapon.element;shot.speed=bow?11+6*charge:8;shot.life=weapon.range*(1+charge*.15f)/shot.speed;
            var renderer=shot.root.GetComponent<SpriteRenderer>();
            if(bow){renderer.sprite=arrowSprite;shot.root.transform.localScale=(weapon.name.Contains("弩")?new Vector3(.48f,.34f,1):new Vector3(.65f,.45f,1))*(1+charge*.2f);shot.root.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(aim.y,aim.x)*Mathf.Rad2Deg);renderer.color=charge>=.999f?C("e2fff4"):Color.white;}
            else renderer.color=weapon.element==Element.Wind?C("82e6cb"):C("f7e8b3");
            if(weapon.element==Element.Lightning)renderer.color=C("b4eaff");
            castDirection=aim;castPoseUntil=clock+.24f;
            ArsenalShot(shot);AddShotRibbon(shot,bow?(charge>=.999f?C("a2f5dc"):C("d0c4a8")):weapon.element==Element.Wind?C("82e6cb"):C("b4eaff"),bow?.025f+charge*.025f:.07f);StyleIdentityProjectile(shot,HeldWeaponIdentity);OriginRangedFan(shot);
        }

        private float CurrentMeleeDamage { get { return CurrentWeapon.damage*MeleeDamageScale(CurrentWeapon) + (swordDamage - initialSwordDamage); } }

        private void AddShotRibbon(Shot shot,Color tint,float width)
        {
            if(shot.root==null)return;
            var trail=shot.root.GetComponent<TrailRenderer>();
            if(trail==null)trail=shot.root.AddComponent<TrailRenderer>();
            trail.emitting=true;trail.sharedMaterial=shot.root.GetComponent<SpriteRenderer>().sharedMaterial;
            trail.time=.13f;trail.minVertexDistance=.035f;trail.startWidth=width;trail.endWidth=0;
            trail.startColor=new Color(tint.r,tint.g,tint.b,.65f);trail.endColor=new Color(tint.r,tint.g,tint.b,0);
            trail.sortingOrder=shot.root.GetComponent<SpriteRenderer>().sortingOrder-1;trail.numCapVertices=2;
        }
        private void SpellSpark(Vector2 at,Color tint,float size)
        {
            GameObject glow=Draw("Brief spell glow",softDisc??disc,at,Vector2.one*size,tint,560);
            fadingBodies.Add(new FadingBody{root=glow,renderer=glow.GetComponent<SpriteRenderer>(),expires=clock+.2f});
        }

        private void DrawHeldEquipment(HeroAction action, int direction, int frameIndex, float phase, Vector2 root, Vector2 aim)
        {
            bool visible=action!=HeroAction.Defeated;
            sword.SetActive(visible);
            if(!visible) { if(heldHandCover!=null) heldHandCover.gameObject.SetActive(false); return; }
            HeroFrame pose=heroFrames[action][direction][frameIndex];
            WeaponDef weapon=CurrentWeapon;SpriteRenderer renderer=sword.GetComponent<SpriteRenderer>();
            renderer.sprite=weapon.sprite??pixelSword;renderer.color=weapon.color;renderer.flipX=false;
            bool left=direction==3;
            float angle=direction==0?-80:direction==2?70:left?198:-18;
            if(action==HeroAction.Attack)
            {
                float[] arcs={-60,-105,-35,20,55,0};
                // Aim is already in world space; mirroring it again reverses left attacks.
                angle=Mathf.Atan2(aim.y,aim.x)*Mathf.Rad2Deg;
                if(weapon.type!=WeaponType.Spear)angle+=arcs[Mathf.Clamp(frameIndex,0,5)]*(left?-1:1);
            }
            if(action==HeroAction.Cast || action==HeroAction.Dodge || bowCharging || OriginActionActive)
                angle=Mathf.Atan2(aim.y,aim.x)*Mathf.Rad2Deg;
            if(weapon.type==WeaponType.Bow&&weapon.bowFacesLeft)angle+=180; // Source bow faces local -X; arrows and aim face +X.
            Vector2 grip=root+pose.grip*pixelHeroScale;
            if(action==HeroAction.Attack&&weapon.type==WeaponType.Spear)
            {float t=Mathf.Clamp01((clock-swingStarted)/meleeActionDuration);float reach=t<.34f?Mathf.Lerp(-.12f,.55f,t/.34f):Mathf.Lerp(.55f,0,(t-.34f)/.66f);grip+=aim*reach*pixelHeroScale;}
            if(action==HeroAction.Cast&&weapon.type==WeaponType.Bow)grip-=aim*.12f*Mathf.Clamp01((castPoseUntil-clock)/.24f);
            if(bowCharging&&weapon.type==WeaponType.Bow)grip-=aim*.1f*BowChargeFraction;
            sword.transform.position=grip;sword.transform.rotation=Quaternion.Euler(0,0,angle);
            sword.transform.localScale=Vector3.one*weapon.length*pixelHeroScale;
            renderer.sortingOrder=heroBody.sortingOrder+(direction==2?-1:1);
            DrawGripCover(pose,grip);
        }

        private void DrawRoomLoot(RoomNode node)
        {
            foreach (WeaponDrop old in worldWeapons) if(old.root!=null) {old.root.SetActive(false);Destroy(old.root);}
            worldWeapons.Clear(); rewardChest = lessonAltar = activeWindWave = null;
            foreach (StoredWeapon stored in node.drops) CreateWeaponDropVisual(stored);
            if (node.kind == RoomKind.Reward && !node.claimed)
            {
                chestPosition = FindFreeInteractionPoint(new Vector2(2.4f,.1f));
                rewardChest = Draw("Island supply cache", capitalMode?CapitalProp(5):!ThunderActive?PressureProp(0):thunderChapterMode?(WorldPropSprite(6)??journeyRock??square):(journeyRock??square), chestPosition, Vector2.one*(!ThunderActive?1:thunderChapterMode?.9f:.55f), thunderChapterMode?(fireChapterMode?C("ebc49b"):woodlandMode?C("b6d89e"):iceSampleMode?C("c2e7f2"):Color.white):C("e5c991"), DepthAt(chestPosition.y));
            }
            if (node.cleared) CheckPendingLesson();
        }

        private void SeedRoomLoot(RoomNode node)
        {
            if(thunderChapterMode)return;
            if(node.kind==RoomKind.Reward||node.kind==RoomKind.Boss||node.kind==RoomKind.Guardian)return;
            if (node.lootSeeded) return; node.lootSeeded = true;
            bool drop = node.kind == RoomKind.Elite || node.kind == RoomKind.Reward || node.kind == RoomKind.Guardian || clearedCombatRooms > 0 && clearedCombatRooms % 2 == 0;
            if (!drop) return;
            int[] offers=clearedCombatRooms<=2&&node.kind!=RoomKind.Elite?null:RollSupplyOffers(node);
            var lootRandom=new System.Random(activeSeed^node.id*499^0x2789);
            int id=offers==null?1:offers[lootRandom.Next(10)<7?0:1+lootRandom.Next(2)];
            var stored = new StoredWeapon { id = id, at = FindFreeInteractionPoint(new Vector2(1.8f,-1.5f)) }; node.drops.Add(stored); CreateWeaponDropVisual(stored);
            if(clearedCombatRooms<=2)AnnounceDiscovery("战后补给 · "+weaponCatalog[id].name+" · 靠近按 F 拾取");
        }

        private void CreateWeaponDropVisual(StoredWeapon stored)
        {
            stored.at=FindSafeLootPoint(stored.at);
            WeaponDef weapon = weaponCatalog[stored.id];
            GameObject root = Draw("Dropped " + weapon.name, weapon.sprite ?? pixelSword, stored.at, Vector2.one * .55f, weapon.color, DepthAt(stored.at.y));
            worldWeapons.Add(new WeaponDrop { data=stored,root=root });
        }

        private WeaponDrop NearestWeaponDrop()
        {
            WeaponDrop best=null;float distance=1.25f;
            foreach(WeaponDrop drop in worldWeapons) {float d=Vector2.Distance(position,drop.data.at);if(d<distance){best=drop;distance=d;}}return best;
        }

        private void PickUpWeapon(WeaponDrop drop)
        {
            RequestCheckpoint();
            RoomNode node=journeyRooms[currentRoom];
            int emptySlot=carriedWeapons[0]<0?0:carriedWeapons[1]<0?1:-1;
            if(emptySlot<0){if(reserveWeapons.Count<4){reserveWeapons.Add(drop.data.id);node.drops.Remove(drop.data);worldWeapons.Remove(drop);Destroy(drop.root);JourneyHint("已收入背包 · B 查看");return;}pendingWeaponPickup=drop;bagSelection=equippedSlot;inventoryMenu=true;return;}
            int slot=emptySlot;
            int old=carriedWeapons[slot];carriedWeapons[slot]=drop.data.id;equippedSlot=slot;
            node.drops.Remove(drop.data);worldWeapons.Remove(drop);Destroy(drop.root);
            if(old>=0){var stored=new StoredWeapon{id=old,at=position+Vector2.right*.4f};node.drops.Add(stored);CreateWeaponDropVisual(stored);}
            swordHitPending=false;swordUntil=castPoseUntil=clock;nextSword=Mathf.Max(nextSword,clock+.2f);
            JourneyHint("已装备 "+CurrentWeapon.name+" · H 切换");
        }

        private bool TryRoomInteraction()
        {
            if(!WorldInteractionReady)return false;if(TryOptionalChallengeInteraction())return true;
            if(ExplainNearbyWorkshopGate())return true;if(thunderChapterMode&&TryChapterInteraction())return true;
            if(!thunderChapterMode&&GatehouseActive&&!gatehouseSampleMode&&canalEpilogueComplete&&journeyRooms[currentRoom].gatehouseIndex==4&&Vector2.Distance(position,ThunderDockAt)<1.4f){OpenWorldAtlas(1);return true;}
            if(thunderPrototypeMode&&TryThunderInteraction())return true;
            if(GatehouseActive&&TryGatehouseInteraction())return true;
            if(stairs!=null&&journeyRooms[currentRoom].cleared&&Vector2.Distance(position,stairPosition)<1.6f){if(ThunderStoryRoute&&currentRoom==6&&!chapterMarkKnown){JourneyHint("先研读铜纹台学习雷标，再沿石阶进入引雷遗址");return true;}if(fireChapterMode&&currentRoom==29&&!journeyRooms[27].cleared){JourneyHint("先平息锻链巨匠，再进入地心炉门");return true;}if(iceSampleMode&&iceFullChapter&&((currentRoom==2&&!iceSpikeKnown)||(currentRoom==26&&!chapterRanger))){JourneyHint(currentRoom==2?"先读取破冰队记录，学习冰法术":"先去受困营地，打开救援门");return true;}BeginDoorTransition(journeyRooms[currentRoom].stairsTo,-1);return true;}
            if(lessonAltar!=null&&Vector2.Distance(position,lessonPosition)<1.35f){LearnPendingLesson();return true;}
            if(TryDesertLandmarkInteraction())return true;
            WeaponDrop drop=NearestWeaponDrop();if(drop!=null){PickUpWeapon(drop);return true;}
            if(rewardChest!=null&&Vector2.Distance(position,chestPosition)<1.4f){chapterRewardMenu=true;chapterRewardChoice=0;return true;}
            return false;
        }
    }
}
