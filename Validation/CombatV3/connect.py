from pathlib import Path
root=Path(__file__).resolve().parents[2]
base=root/'Assets/NakiPrototype'
def read(name):return (base/name).read_text(encoding='utf-8-sig')
def write(name,s):(base/name).write_text(s,encoding='utf-8')
def method(s,signature,body):
    start=s.index(signature);a=s.index('{',start);level=1;b=a+1
    while level:
        if s[b]=='{':level+=1
        elif s[b]=='}':level-=1
        b+=1
    return s[:a+1]+body+s[b-1:]
s=read('NakiPrototype.Elements.cs')
s=method(s,'private void CastJourneyOrb()', '\n            if(origin==Origin.Dark){CastDarkMeteor();return;}CastLightVolley();\n        ')
s=method(s,'private void CastWindBlade()', '\n            CastBladeVolley();\n        ')
s=s.replace('float before = shield,','amount*=EnemyAttackScale;float before = shield,').replace('消耗8光之力，单颗追踪','消耗8光之力，五弹齐射')
write('NakiPrototype.Elements.cs',s)
s=method(read('NakiPrototype.Origin.cs'),'private void CastDarkMeteor()', '\n            CastMeteorVolley();\n        ')
write('NakiPrototype.Origin.cs',s)
s=method(read('NakiPrototype.FireCombat.cs'),'private void CastFireBolt()', 'CastFireVolley();')
s=s.replace('private void FireFan(Enemy e,int count,float spacing,float speed){','private void FireFan(Enemy e,int count,float spacing,float speed){DensifyFan(ref count,ref spacing);')
s=s.replace('int count=e.boss?(e.secondPhase?13:9):3;for(', 'int count=e.boss?(e.secondPhase?13:9):3;float spacing=e.boss?16:17;DensifyFan(ref count,ref spacing);for(').replace('(i-(count-1)*.5f)*(e.boss?16:17)','(i-(count-1)*.5f)*spacing')
s=s.replace('(e.secondPhase?.95f:1.3f):1.8f','(e.secondPhase?.8f:1.1f):1.45f')
write('NakiPrototype.FireCombat.cs',s)
s=read('NakiPrototype.Enemies.cs').replace('enemies.Add(enemy); return enemy;', 'enemy.hp*=EnemyHealthScale(enemy.boss);enemy.maximum=enemy.hp;enemies.Add(enemy); return enemy;')
s=s.replace('Vector2 direction = enemy.attackDirection;','DensifyFan(ref count,ref spacing);Vector2 direction = enemy.attackDirection;')
write('NakiPrototype.Enemies.cs',s)
s=read('NakiPrototype.IceChapterCombat.cs').replace('float offset=0){for(', 'float offset=0){DensifyFan(ref count,ref spacing);for(')
write('NakiPrototype.IceChapterCombat.cs',s)
s=read('NakiPrototype.IceCombat.cs')
start=s.index('    Enemy target;Vector2 aim=AutoAim(10,10,0,out target);')
end=s.index('\n   }',start)
s=s[:start]+'    CastIceVolley();'+s[end:]
s=s.replace('for(int i=0;i<count;i++){Vector2 direction=Quaternion.Euler(0,0,(i-(count-1)*.5f)*(e.boss?16:12))', 'float spacing=e.boss?16:12;DensifyFan(ref count,ref spacing);for(int i=0;i<count;i++){Vector2 direction=Quaternion.Euler(0,0,(i-(count-1)*.5f)*spacing)')
write('NakiPrototype.IceCombat.cs',s)
s=read('NakiPrototype.ThunderSkills.cs')
a=s.index('                int count=shots.Count;SpawnShot(position+aim*.45f,aim,false,null,position);')
b=s.index('\n            }',a)
s=s[:a]+'                CastArcVolley();'+s[b:]
write('NakiPrototype.ThunderSkills.cs',s)
s=read('NakiPrototype.ForgeCombat.cs').replace('private GameObject ForgeFanWarning(Enemy e,Vector2 aim,int count,float spacing){','private GameObject ForgeFanWarning(Enemy e,Vector2 aim,int count,float spacing){DensifyFan(ref count,ref spacing);')
s=s.replace('(e.secondPhase?1.35f:1.8f):2','(e.secondPhase?1.05f:1.4f):1.7f')
write('NakiPrototype.ForgeCombat.cs',s)
s=read('NakiPrototype.Equipment.cs').replace('WeaponDef weapon=CurrentWeapon;', 'WeaponDef weapon=CurrentWeapon;if(weapon.type==WeaponType.Staff){UseStaffVolley();return;}')
s=s.replace('(weapon.name.Contains("弩")?2.5f:2)*charge*charge','(weapon.name.Contains("弩")?3.5f:3)*charge*charge')
write('NakiPrototype.Equipment.cs',s)
s=read('NakiPrototype.BowCharge.cs')
s=s.replace('private float bowChargeStarted,bowShotCharge;', 'private float bowChargeStarted,bowShotCharge,bowPausedTime,bowDashPauseAt=-1,bowDashPauseEnd;\n  private void PauseBowChargeForDodge(){if(!bowCharging)return;if(bowDashPauseAt>=0)bowPausedTime+=Mathf.Clamp(clock-bowDashPauseAt,0,bowDashPauseEnd-bowDashPauseAt);bowDashPauseAt=clock;bowDashPauseEnd=dashUntil;}')
s=s.replace('(clock-bowChargeStarted)/BowChargeTime', '(clock-bowChargeStarted-bowPausedTime-(bowDashPauseAt>=0?Mathf.Clamp(clock-bowDashPauseAt,0,bowDashPauseEnd-bowDashPauseAt):0))/BowChargeTime')
s=s.replace('chargingWeapon=-1;if(bowChargeBar', 'chargingWeapon=-1;bowPausedTime=0;bowDashPauseAt=-1;if(bowChargeBar')
s=s.replace('if(clock<dashUntil||clock<castPoseUntil||clock<pixelHurtUntil)', 'if(clock<castPoseUntil||clock<pixelHurtUntil)')
s=s.replace('if(held&&!bowCharging&&clock>=nextSword)', 'if(clock<dashUntil){if(bowCharging)UpdateBowChargeBar();return;}\n   if(held&&!bowCharging&&clock>=nextSword)')
s=s.replace('bowChargeStarted=clock;chargingWeapon=', 'bowChargeStarted=clock;bowPausedTime=0;bowDashPauseAt=-1;chargingWeapon=')
write('NakiPrototype.BowCharge.cs',s)
s=read('NakiPrototype.PixelHero.cs').replace('dashUntil = clock + .17f;', 'dashUntil = clock + .17f;PauseBowChargeForDodge();')
write('NakiPrototype.PixelHero.cs',s)
s=read('NakiPrototype.cs').replace('var shot = new Shot { position = at,', 'int live=0;foreach(Shot active in shots)if(active.hostile==hostile)live++;if(live>=(hostile?128:64))return;\n            var shot = new Shot { position = at,')
write('NakiPrototype.cs',s)
print('Connected volleys, charge preservation and derived combat balance')
