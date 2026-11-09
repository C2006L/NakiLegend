from pathlib import Path
r=Path('D:/NakiLegend/Repository/Assets/NakiPrototype')
def edit(name,old,new):
 p=r/('NakiPrototype.'+name+'.cs');s=p.read_text(encoding='utf-8-sig');assert old in s,(name,old[:50]);p.write_text(s.replace(old,new),encoding='utf-8')
edit('CombatMastery','1.5f+CombatMasteryTier*.1f','3f+CombatMasteryTier*.2f')
edit('Starfall','StarfallRadius=1.2f','StarfallRadius=1.65f')
edit('OriginCombat','originMarkUntil=clock+6','originMarkUntil=clock+8')
edit('OriginCombat','originMarkPulses>=5','originMarkPulses>=6')
edit('OriginCombat','originMarkProcReady=clock+.6f','originMarkProcReady=clock+.45f')
edit('OriginCombat','<1.2f+e.radius','<1.8f+e.radius')
edit('OriginCombat','OriginDamage(28)*DarkStrength','OriginDamage(34)*DarkStrength')
edit('OriginCombat','originMarkPulses+" / 5"','originMarkPulses+" / 6"')
edit('OriginCombat','  private void TickOriginVisuals()','''  private void TransferSoulSeal()
  {
   if(originMarkedTarget==null||originMarkedTarget.hp>0&&enemies.Contains(originMarkedTarget)||clock>=originMarkUntil||originMarkPulses>=6)return;
   Vector2 at=originMarkedTarget.position;Enemy next=null;float range=3.8f;
   foreach(Enemy e in enemies)if(e.hp>0&&Vector2.Distance(e.position,at)<range&&!ProjectilePathBlocked(at,e.position,.04f)){next=e;range=Vector2.Distance(e.position,at);}
   if(next!=null){originMarkedTarget=next;AddFloatingHit("蚀魂印转移",next.position+Vector2.up*1.15f,C("d0a5f4"));}
  }
  private void TickOriginVisuals()''')
edit('OriginCombat','   TickStarfall();','   TickStarfall();TransferSoulSeal();')
edit('OriginCombat','if(clock>=originMarkUntil||originMarkedTarget==null','if(clock>=originMarkUntil||originMarkPulses>=6||originMarkedTarget==null')
edit('OriginLegacy','先给敌人挂印；6秒内用武器、流星或星陨命中触发范围爆炸，最多5次','挂印8秒；武器、流星或星陨命中引爆，最多6次，击杀后转移邻敌')
edit('OriginLegacy','三处落点预警后坠星，造成范围伤害并引爆蚀魂印','三处大圈落点后坠星，范围与敌方祭师相同，可引爆蚀魂印')
edit('BossDuels','dir,11,2.4f','dir,11,3f')
edit('BossDuels','?0:profile==11?-1:1)*18','?0:profile==11?-1:1)*22')
edit('BossDuels','left=(profile==0?2:3)+(e.difficultyPhase>=2?1:0)','left=(profile==0?3:4)+(e.difficultyPhase>=2?1:0)')
edit('BossDuels','int before=shots.Count;Vector2 aim=s.aim;','''int before=shots.Count;Vector2 aim=s.aim;
    // Re-aim once between beats, rather than tracking every bullet in a fan.
    // Constant circular movement no longer escapes the entire committed sequence.
    if(s.index==2){Vector2 lead=IntentLead(.2f)-e.position;if(lead.sqrMagnitude>.01f)s.aim=aim=lead.normalized;}''')
edit('BossDuels','int count=(s.profile==0?12:16)+e.difficultyPhase*2','int count=(s.profile==0?14:18)+e.difficultyPhase*2')
edit('BossDuels','    for(int n=before;n<shots.Count;n++){','''    if(s.left==1&&s.profile!=0&&s.profile!=6&&s.profile!=11&&s.profile!=12){
     Vector2 flank=Quaternion.Euler(0,0,(s.index%2==0?1:-1)*65)*s.aim;
     PressureFan(e.position+flank*(e.radius+.2f),flank,5,10,4.0f,e.attackElement,1.05f);
    }
    for(int n=before;n<shots.Count;n++){''')
print('Updated skill utility and active boss combinations; health and projectile speed unchanged')
