using UnityEngine;
namespace NakiLegend.Prototype {
 public sealed partial class NakiPrototype {
  private Sprite hostileBulletSprite;
  private Sprite HostileBulletSprite {get {if(hostileBulletSprite==null)hostileBulletSprite=MakeSprite(new[]{"...xxxx...","..xxxxxx..",".xxxxxxxx.","xxxxxxxxxx","xxxxxxxxxx","xxxxxxxxxx","xxxxxxxxxx",".xxxxxxxx.","..xxxxxx..","...xxxx..."},new System.Collections.Generic.Dictionary<char,Color>{{'x',Color.white}},10);return hostileBulletSprite;}}
  private void DefineHostileBullet(Shot shot){var sr=shot.root.GetComponent<SpriteRenderer>();sr.sprite=HostileBulletSprite;GameObject outline=Draw("Hostile bullet outline",HostileBulletSprite,shot.position,Vector2.one*.33f,C("302238"),sr.sortingOrder-1);outline.transform.SetParent(shot.root.transform,true);GameObject core=Draw("Hostile bullet core",HostileBulletSprite,shot.position,Vector2.one*.10f,C("fff6dc"),sr.sortingOrder+1);core.transform.SetParent(shot.root.transform,true);}
  private static readonly float[] AttackByIsland={1f,1f,1f,1f,1f};
  private bool CombatShieldRecovery {get{return enableDesertJourney&&currentRoom>=0&&currentRoom<journeyRooms.Count&&IsCombatRoom(journeyRooms[currentRoom])&&!journeyRooms[currentRoom].cleared;}}
  private float HostileProjectileSpeedScale {get{return (LearningEncounter?1.05f:1.12f+.0325f*Mathf.Clamp(CurrentIsland.ordinal-1,0,4))*(enemies.Exists(e=>e.boss&&e.hp>0)?(manorCourtyardMode?.5f:.75f):1);}}
  private static readonly float[] RecoveryByIsland={.92f,.90f,.82f,.86f,.85f},CounterIntervalByIsland={2.9f,2.8f,2.7f,2.6f,2.5f};
  private float EncounterRecoveryScale {get{return LearningEncounter?1:RecoveryByIsland[Mathf.Clamp(CurrentIsland.ordinal-1,0,4)];}}
  private int TacticalCompanionCount {get{return LearningEncounter?0:journeyRooms[currentRoom].kind==RoomKind.Elite?2:1;}}
  private float BossCounterInterval {get{return CounterIntervalByIsland[Mathf.Clamp(CurrentIsland.ordinal-1,0,4)];}}
  private float EnemyHealthScale(bool boss){return manorCourtyardMode&&boss?.65f:1;}
  private float EnemyAttackScale {get{return manorCourtyardMode?1.75f:AttackByIsland[Mathf.Clamp(CurrentIsland.ordinal-1,0,4)];}}
  private void DensifyFan(ref int count,ref float spacing){int old=count;count=Mathf.Min(count,bossAttackEmission?20:5);if(count>1)spacing*=Mathf.Max(1,old-1)/(float)(count-1);}
  private Color SpellTint(Element element){return element==Element.Dark?C("ba7df4"):element==Element.Ice?C("b7f2ff"):element==Element.Fire?C("ffbb73"):element==Element.Wind?C("83efc8"):element==Element.Lightning?C("9ee5ff"):element==Element.Wood?C("b3de8f"):C("fff0ad");}
  private int SpawnSpellBurst(Element element,string sound,float damage,float speed,float lifetime,bool tracking=false,bool pierce=false,bool primary=false){Enemy target;Vector2 aim=AutoAim(11,speed,0,out target);int beforeAll=shots.Count,group=++nextMeteorGroup;float spread=element==Element.Wind?9:element==Element.Lightning?7:8;for(int i=-2;i<=2;i++){Vector2 dir=Quaternion.Euler(0,0,i*spread)*aim;int before=shots.Count;SpawnShot(position+dir*.45f,dir,false,tracking?target:null,position);if(shots.Count==before)continue;Shot shot=shots[shots.Count-1];shot.element=element;shot.skillAudio=sound;shot.meteorGroup=group;shot.orb=primary;shot.damage=SkillDamage(damage);shot.speed=speed;shot.life=lifetime;shot.tracking=tracking&&target!=null;shot.hitsLeft=pierce?2:1;var renderer=shot.root.GetComponent<SpriteRenderer>();renderer.color=SpellTint(element);if(element==Element.Wind){renderer.sprite=pixelSlash??disc;shot.root.transform.localScale=new Vector3(.32f,.55f,1);shot.root.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(dir.y,dir.x)*Mathf.Rad2Deg);}else if(element==Element.Ice){shot.root.transform.localScale=new Vector3(.18f,.36f,1);shot.root.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(dir.y,dir.x)*Mathf.Rad2Deg-90);}else{shot.root.transform.localScale=Vector3.one*(primary?.31f:.27f);var halo=Draw("Spell halo",windRingSprite??disc,shot.position,Vector2.one*.42f,new Color(renderer.color.r,renderer.color.g,renderer.color.b,.45f),renderer.sortingOrder-1);halo.transform.SetParent(shot.root.transform,true);}AddShotRibbon(shot,renderer.color,primary?.1f:.07f);if(element==Element.Lightning)shot.chapterArc=true;}
   if(shots.Count>beforeAll){if(primary)RecordEncounterCast();SkillSound(sound,"cast");castDirection=aim;castPoseUntil=clock+.28f;SpellSpark(position+aim*.45f,SpellTint(element),.3f);}return shots.Count-beforeAll;
  }
  private bool PrimaryVolleyPresent(Element element,int limit){meteorGroups.Clear();foreach(Shot shot in shots)if(!shot.hostile&&shot.orb&&shot.element==element)meteorGroups.Add(shot.meteorGroup);return meteorGroups.Count>=limit;}
  private void CastLightVolley(){BeginLightChannel();}
  private void CastMeteorVolley(){if(!orbLearned){JourneyHint("右眼的力量尚未领悟 · 留意遗迹中的碑文");return;}if(clock<nextSpell||clock<dashUntil||PrimaryVolleyPresent(Element.Dark,3)||darkPower<1)return;float strength=DarkStrength;if(SpawnSpellBurst(Element.Dark,"meteor",MeteorBaseDamage*OriginTrainingScale*strength,7,1.8f,true,false,true)==0)return;SpendPower(PowerPool.Dark,1);nextSpell=clock+.8f;}
  private void CastBladeVolley(){if(!bladeLearned){JourneyHint("风之刃尚未学会 · 继续探索旧渠");return;}if(clock<bladeReadyAt||clock<dashUntil||elementPower<SkillElementCost(6))return;if(SpawnSpellBurst(Element.Wind,"blade",(windEmpowered?26:22)*.65f,9,1.4f,false,true)==0)return;SpendSkillPower(6);bladeReadyAt=clock+1.2f;}
  private void CastFireVolley(){if(!fireBoltKnown){JourneyHint("先在安全训练炉学习火种投射");return;}if(clock<fireBoltReady||clock<dashUntil||elementPower<SkillElementCost(6))return;if(SpawnSpellBurst(Element.Fire,"meteor",22*.7f,9,1.2f)==0)return;SpendSkillPower(6);fireBoltReady=clock+1.2f;}
  private void CastIceVolley(){if(elementPower<SkillElementCost(6))return;if(SpawnSpellBurst(Element.Ice,"ice_spike",20*.7f,10,1.1f)==0)return;SpendSkillPower(6);iceSpikeReady=clock+1.4f;}
  private void CastArcVolley(){if(elementPower<SkillElementCost(10))return;if(SpawnSpellBurst(Element.Lightning,"arc",20*.7f,9,1.4f)==0)return;SpendSkillPower(10);chapterArcReady=clock+2.6f;}
  private void UseStaffVolley(){
   WeaponDef w=CurrentWeapon;if(!CanUseWeapon(w)){nextSword=clock+.2f;return;}
   Enemy target;Vector2 aim=AutoAim(w.range,8,w.scatter,out target);int start=shots.Count,volley=++nextMeteorGroup;
   int count=StaffVolleyCount(w);
   for(int i=0;i<count;i++){
    Vector2 dir=Quaternion.Euler(0,0,(i-(count-1)*.5f)*6)*aim;int before=shots.Count;SpawnShot(position+dir*.5f,dir,false,null,position);if(shots.Count==before)continue;
    Shot shot=shots[shots.Count-1];shot.meteorGroup=volley;shot.root.name="Staff weapon projectile";shot.element=w.element;shot.damage=w.damage*StaffPelletScale(w,count);shot.speed=8;shot.life=w.range/8;shot.tracking=false;
    shot.root.transform.localScale=Vector3.one*.14f;shot.root.GetComponent<SpriteRenderer>().color=SpellTint(w.element);ArsenalShot(shot);if(w.variant==2){shot.hitsLeft=2;shot.speed*=1.2f;}AddShotRibbon(shot,SpellTint(w.element),.025f);StyleIdentityProjectile(shot,HeldWeaponIdentity);
   }
   if(shots.Count==start){nextSword=clock+.1f;return;}SpendPower(w.pool,w.cost);WeaponAudio();nextSword=clock+w.interval;castDirection=aim;castPoseUntil=clock+.2f;
  }
 }
}
