using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private const float StaffDamageScale=.8f;
  private int weaponImpactSource=-1;
  private void ApplyWeaponDamage(Enemy target,float amount,Element element,int id)
  {int old=weaponImpactSource;weaponImpactSource=id;try{ApplyElementDamage(target,amount*WeaponImpactScale(id,element,target.affinity),element);}finally{weaponImpactSource=old;}}
  private void OriginRangedFan(Shot shot)
  {if(shot.identityWeapon!=128)return;shot.damage*=.6f;foreach(int side in new[]{-1,1}){Vector2 aim=Quaternion.Euler(0,0,side*7)*shot.direction;RelicBullet(shot.position,aim,shot.damage,shot.element,shot.speed,shot.life,WeaponEffect.None,128);}}
  private void AppendOriginWeapons(List<WeaponDef> items)
  {
   AddOriginWeapon(items,"光之杖",WeaponType.Staff,Element.Light,22,.72f,11,4,1.25f,PowerPool.Light,1);
   AddOriginWeapon(items,"光剑",WeaponType.Sword,Element.Light,25,.44f,1.7f,3,1.15f,PowerPool.Light,1);
   AddOriginWeapon(items,"暗黑低语匕首",WeaponType.Sword,Element.Dark,17,.24f,1.1f,1,.75f,PowerPool.Dark,1);
   AddOriginWeapon(items,"曙光长枪",WeaponType.Spear,Element.Light,33,.78f,2.8f,6,1.95f,PowerPool.Light,2);
   AddOriginWeapon(items,"晨星弓",WeaponType.Bow,Element.Light,20,.75f,12,5,1.2f,PowerPool.Light,2);
   AddOriginWeapon(items,"棱镜弩",WeaponType.Bow,Element.Light,19,.88f,11,6,1.1f,PowerPool.Light,3);
   AddOriginWeapon(items,"贤者魂灯",WeaponType.Staff,Element.Dark,19,.85f,10,2,1.25f,PowerPool.Dark,2);
   AddOriginWeapon(items,"缄魂弩",WeaponType.Bow,Element.Dark,22,.82f,12,2,1.1f,PowerPool.Dark,3);
   AddOriginWeapon(items,"渡魂镰",WeaponType.Sword,Element.Dark,31,.83f,2.2f,2,1.6f,PowerPool.Dark,4);
  }
  private void AddOriginWeapon(List<WeaponDef> items,string name,WeaponType type,Element element,float damage,float interval,float range,float cost,float length,PowerPool pool,int island)
  {
   WeaponDef w=Weapon(name,type,element,damage,interval,range,cost,length,null,"ffffff");w.pool=pool;w.sourceIsland=island;w.family=items.Count;w.maxHits=type==WeaponType.Sword||type==WeaponType.Spear?4:1;items.Add(w);
  }
  private float WeaponCost(WeaponDef w,float extra=0){return w.pool==PowerPool.Elemental?ElementCost(w.cost+extra):w.cost+extra;}
  private string WeaponPoolName(WeaponDef w){return w.pool==PowerPool.Light?"光之力":w.pool==PowerPool.Dark?"贤者之石能量":"元素之力";}
  private string WeaponCostLabel(WeaponDef w){return WeaponCost(w).ToString("0.##")+" "+WeaponPoolName(w);}
  private bool OriginWeaponAllowed(WeaponDef w){return w.pool==PowerPool.Elemental||w.pool==PowerPool.Light&&origin==Origin.Light||w.pool==PowerPool.Dark&&origin==Origin.Dark;}
  private bool CanUseWeapon(WeaponDef w,float extra=0)
  {
   if(!OriginWeaponAllowed(w)){JourneyHint("这把武器需要选择"+(w.pool==PowerPool.Light?"光之力":"贤者之石")+"路线 · H 切换备用武器");return false;}
   if(WeaponCost(w,extra)<=0)return true;float power=w.pool==PowerPool.Light?energy:w.pool==PowerPool.Dark?darkPower:elementPower;
   if(power+.001f<WeaponCost(w,extra)){JourneyHint(WeaponPoolName(w)+"不足 · H 切换备用武器");return false;}return true;
  }
  private int StaffVolleyCount(WeaponDef w){return w.pool==PowerPool.Light?1:w.pool==PowerPool.Dark?2:w.effect==WeaponEffect.Needle||w.variant==2?1:w.variant==1||w.effect==WeaponEffect.FlameSpray?5:3;}
  private float StaffPelletScale(WeaponDef w,int count){return StaffDamageScale*(w.pool!=PowerPool.Elemental?1:w.effect==WeaponEffect.FlameSpray?.5625f:1)*(w.pool!=PowerPool.Elemental?1:count==1?(w.effect==WeaponEffect.Needle?1.5f:2.1f):count==5?.5f:.72f);}
  private string WeaponDamageLabel(WeaponDef w){return w.type==WeaponType.Staff?"每弹 "+(w.damage*StaffPelletScale(w,StaffVolleyCount(w))*WeaponOutputScale(w)).ToString("0.#")+" ×"+StaffVolleyCount(w):"伤害 "+((w.type==WeaponType.Bow?BowCardDamage(w,0):w.damage*MeleeDamageScale(w))*WeaponOutputScale(w)).ToString("0.#");}
  private string OriginWeaponDescription(int id)
  {
   switch(id){
    case 123:return "单发日芒 · 贯穿两名敌人，遇掩体停止";
    case 124:return "光刃挥斩 · 短距离月牙剑气，最多命中两名敌人";
    case 125:return "快速近刺 · 同一目标连续命中四次触发低语追加伤害";
    case 126:return "直线长枪 · 贯穿近身敌人并带出一道曙光";
    case 127:return "晨星箭 · 满蓄贯穿三名敌人";
    case 128:return "三道棱光 · 窄幅散射，满蓄可贯穿";
    case 129:return "两盏魂火 · 缓慢追踪，受掩体阻挡";
    case 130:return "缄魂箭 · 命中标记四秒，再次命中引爆并短暂减速";
    case 131:return "宽幅镰斩 · 对低生命普通敌人追加伤害，首领免疫处决";
    default:return "";
   }
  }
  private readonly Dictionary<Enemy,int> whisperHits=new Dictionary<Enemy,int>();
  private readonly Dictionary<Enemy,float> soulCrossMarks=new Dictionary<Enemy,float>();
  private void OriginMeleeHit(Enemy enemy)
  {
   TriggerSoulSeal(enemy);
   int id=HeldWeaponIdentity;if(id==125&&enemy.hp>0){int hits;whisperHits.TryGetValue(enemy,out hits);if(++hits>=4){whisperHits.Remove(enemy);IdentityImpact(id,enemy.position,swingDirection);ApplyElementDamage(enemy,12,Element.Dark);}else whisperHits[enemy]=hits;}
   if(id==131&&enemy.hp>0&&!enemy.boss&&enemy.hp<enemy.maximum*.25f)ApplyElementDamage(enemy,18,Element.Dark);
  }
  private void OriginProjectileHit(Shot s,Enemy target,Vector2 contact)
  {
   if(s.identityWeapon>=0&&weaponCatalog[s.identityWeapon].type==WeaponType.Staff&&s.element==Element.Wood&&target.hp>0){target.woodPoisonUntil=Mathf.Max(target.woodPoisonUntil,clock+3);target.woodPoisonNext=Mathf.Max(target.woodPoisonNext,clock+.7f);}
   if(s.identityWeapon!=130||target.hp<=0)return;float until;if(soulCrossMarks.TryGetValue(target,out until)&&until>=clock){soulCrossMarks.Remove(target);ApplyElementDamage(target,8,Element.Dark);target.iceSlowUntil=Mathf.Max(target.iceSlowUntil,clock+(target.boss?.25f:.8f));IdentityImpact(130,contact,s.direction);}else soulCrossMarks[target]=clock+4;
  }
  private void OriginMeleeWave()
  {
   int id=HeldWeaponIdentity;if(id!=124&&id!=126)return;RelicBullet(position+swingDirection*.55f,swingDirection,CurrentWeapon.damage*.3f,Element.Light,11,.32f,WeaponEffect.None,id);
   if(shots.Count>0&&shots[shots.Count-1].identityWeapon==id)shots[shots.Count-1].hitsLeft=2;
  }
 }
}
