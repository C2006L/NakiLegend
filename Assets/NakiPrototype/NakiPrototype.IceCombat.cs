using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private void ConfigureIceEnemy(Enemy e)
  {
   bool boss=e.kind==EnemyKind.IceBeast;e.boss=boss;e.radius=boss?.9f:e.kind==EnemyKind.IceGuard?.4f:.3f;
   e.hp=e.maximum=boss?780:e.kind==EnemyKind.IceGuard?82:56;e.iceArmor=boss?4:e.kind==EnemyKind.IceGuard?2:0;
   e.affinity=e.attackElement=Element.Ice;e.speed=boss?1.1f:e.kind==EnemyKind.IceGuard?1.3f:.9f;e.nextAttack=clock+1.6f;e.soulEligible=e.kind==EnemyKind.IceGuard;
  }
  private float IceArmorDamage(Enemy e,float amount,Element element)
  {
   if(!IsIceEnemy(e.kind)||e.iceArmor<=0)return amount;
   if(element==Element.Lightning)
   {
    e.iceArmor--;SpellSpark(e.position,C("d7f4ff"),.65f);
    if(e.iceArmor==0){if(e.warning!=null)Destroy(e.warning);e.windup=e.chargeUntil=0;e.recoveryUntil=clock+1;e.nextAttack=clock+1.2f;AddFloatingHit("晶甲破裂 · 输出窗口",e.position+Vector2.up*1.4f,C("c8f5ff"));Sfx("break",false,.5f);return amount;}
    AddFloatingHit("晶甲裂纹 "+e.iceArmor,e.position+Vector2.up*1.3f,C("a7e6ff"));
   }
   return amount*.65f;
  }
  private void UpdateIceEnemy(Enemy e,float dt)
  {
   Vector2 old=e.position,to=position-e.position;bool boss=e.kind==EnemyKind.IceBeast;
   if(boss&&!e.secondPhase&&e.hp<=e.maximum*.5f){e.secondPhase=true;e.iceArmor=3;e.windup=e.chargeUntil=0;e.phaseUntil=clock+1.1f;e.nextAttack=e.phaseUntil+.4f;if(e.warning!=null)Destroy(e.warning);AddFloatingHit("碎冰暴走 · 新晶甲凝结",e.position+Vector2.up*1.6f,C("bfe8ff"));}
   float slow=clock<e.iceSlowUntil?(boss?.8f:.45f):1;
   if(clock>=e.phaseUntil)
   {
    if(e.chargeUntil>0)
    {
     if(clock<e.chargeUntil){e.position=MoveEnemyWithinRoom(e.position,e.attackDirection*6.8f*slow*dt,e.radius);if((e.position-old).sqrMagnitude<.00001f)e.chargeUntil=clock;if(DistanceToSegment(position,old,e.position)<e.radius+.3f)TakeJourneyDamage(2,Element.Ice);}
     else{e.chargeUntil=0;e.recoveryUntil=clock+.85f;}
    }
    else if(e.windup>0)
    {
     e.windup-=dt;if(e.windup<=0){if(e.warning!=null)Destroy(e.warning);ResolveIceMove(e);}
    }
    else if(clock>=e.recoveryUntil)
    {
     if(clock>=e.nextAttack&&to.magnitude<(boss?9:e.kind==EnemyKind.IceGuard?2:8))
     {
      e.attackNumber++;e.attackDirection=to.sqrMagnitude>.01f?to.normalized:Vector2.down;e.actionStarted=clock;
      int move=boss?(e.attackNumber-1)%3:e.kind==EnemyKind.IceGuard?3:1;e.windup=move==0?1:move==2?1.1f:.8f;e.nextAttack=clock+e.windup+(e.secondPhase?1.3f:1.8f);
      if(move==0||move==3)e.warning=ChapterLineWarning(e.position,e.attackDirection,move==0?5:2.1f,move==0?1.65f:.65f);
      else if(move==2){e.strikePoint=position;e.warning=WarningCircle(position,1.2f);}else e.warning=WarningSector(e.position,e.attackDirection,2);
      if(boss)Sfx("warn",true,.4f);
     }
     else if(to.magnitude>(e.kind==EnemyKind.IceWisp?4:boss?3:1.3f))e.position=MoveEnemyWithinRoom(e.position,to.normalized*e.speed*slow*dt,e.radius);
    }
   }
   e.velocity=dt>0?(e.position-old)/dt:Vector2.zero;e.root.transform.position=e.position+Vector2.up*(e.kind==EnemyKind.IceWisp?.22f+.04f*Mathf.Sin(clock*5):e.velocity.sqrMagnitude>.01f?.025f*Mathf.Sin(clock*12):0);
   e.body.flipX=to.x<0;e.body.sortingOrder=DepthAt(e.position.y);e.body.color=clock<e.hurtUntil?C("ffe5dd"):e.iceArmor>0?C("b9d9ff"):Color.white;if(e.shadow!=null)e.shadow.transform.position=e.position;
  }
  private void ResolveIceMove(Enemy e)
  {
   int move=e.boss?(e.attackNumber-1)%3:e.kind==EnemyKind.IceGuard?3:1;e.recoveryUntil=clock+.8f;
   if(move==0){e.chargeUntil=clock+.72f;return;}
   if(move==3){if(DistanceToSegment(position,e.position,e.position+e.attackDirection*2.1f)<.5f&&!ProjectilePathBlocked(e.position,position,.04f))TakeJourneyDamage(1.5f,Element.Ice);return;}
   if(move==2){SpellSpark(e.strikePoint,C("c8f6ff"),1.5f);SkillSound("ice_ring","hit");if(Vector2.Distance(position,e.strikePoint)<1.2f)TakeJourneyDamage(2,Element.Ice);return;}
   int count=e.boss?(e.secondPhase?7:5):3;
   for(int i=0;i<count;i++){Vector2 direction=Quaternion.Euler(0,0,(i-(count-1)*.5f)*(e.boss?16:12))*e.attackDirection;int before=shots.Count;SpawnShot(e.position+direction*.45f,direction,true,null,e.position);if(shots.Count>before){Shot s=shots[shots.Count-1];s.element=Element.Ice;s.damage=e.boss?2:1;s.speed=e.boss?4.1f:4.5f;s.life=3;s.root.GetComponent<SpriteRenderer>().color=C("b7f2ff");}}
  }
  private void CastIceSkill(int slot)
  {
   int skill=chapterSkillSlots[slot];if(!ChapterSkillKnown(skill)){JourneyHint("冰法术尚未领悟 · 寻找破冰队记录");return;}
   if(clock<dashUntil||clock<ChapterSkillReady(skill))return;
   if(skill==4)
   {
    Enemy target;Vector2 aim=AutoAim(10,10,0,out target);int before=shots.Count;
    if(elementPower<ElementCost(6)){JourneyHint("元素之力不足");return;}SpawnShot(position+aim*.45f,aim,false,null,position);if(shots.Count==before)return;
    SpendPower(PowerPool.Elemental,6);iceSpikeReady=clock+1.4f;Shot shot=shots[shots.Count-1];shot.tracking=false;shot.skillAudio="ice_spike";shot.element=Element.Ice;shot.damage=SkillDamage(20);shot.speed=10;shot.life=1.1f;shot.root.transform.localScale=new Vector3(.16f,.35f,1);shot.root.GetComponent<SpriteRenderer>().color=C("c5f6ff");AddShotRibbon(shot,C("a5e7f5"),.045f);SkillSound("ice_spike","cast");castDirection=aim;castPoseUntil=clock+.25f;
   }
   else
   {
    if(!SpendPower(PowerPool.Elemental,12))return;iceRingReady=clock+5;iceRingBorn=clock;if(iceRingVisual!=null)Destroy(iceRingVisual);iceRingVisual=Draw("Expanding frost ring",windRingSprite??disc,position,Vector2.one*.2f,C("c1f3ff"),-630);SkillSound("ice_ring","cast");
    foreach(Enemy e in enemies.ToArray())if(Vector2.Distance(e.position,position)<3+e.radius&&!ProjectilePathBlocked(position,e.position,.03f)){e.iceSlowUntil=clock+(e.boss?1:3);ApplySkillHit(e,SkillDamage(14),Element.Ice,"ice_ring");}
   }
  }
  private void TickIceEffects()
  {
   if(iceRingVisual==null)return;float phase=(clock-iceRingBorn)/.45f;if(phase>=1){Destroy(iceRingVisual);iceRingVisual=null;return;}
   iceRingVisual.transform.localScale=Vector3.one*Mathf.Lerp(.2f,6,phase);var renderer=iceRingVisual.GetComponent<SpriteRenderer>();renderer.color=new Color(.75f,.95f,1,1-phase);
  }
  private void DrawIceSkills(float w,float h)
  {
   HudRect(new UnityEngine.Rect(w/2-455,h/2-320,910,640),C("10232d"));CenterLabel(new Rect(w/2-430,h/2-300,860,45),"冰港元素配置 · U / I",journeyTitleStyle);
   string[] detail={"快速风刃","拦截普通飞弹","雷击晶甲，打开破甲窗口","雷弧可跳向近处敌人","直线冰锥，命中缓速","身边霜环，短时减速；Boss 效果较弱"};
   for(int i=0;i<6;i++){float y=h/2-225+i*76;FittedLabel(new Rect(w/2-420,y,550,28),IceSkills[i]+" · "+(ChapterSkillKnown(i)?ElementCost(ChapterSkillCost(i)).ToString("0.##")+" 元素":"尚未领悟"),journeyTextStyle);FittedLabel(new Rect(w/2-420,y+29,550,27),detail[i],journeySmallStyle);if(FlatButton(new Rect(w/2+160,y,115,52),"装备 U",chapterSkillSlots[0]==i))EquipChapterSkill(i,0);if(FlatButton(new Rect(w/2+290,y,115,52),"装备 I",chapterSkillSlots[1]==i))EquipChapterSkill(i,1);}
   CenterLabel(new Rect(w/2-430,h/2+260,860,35),"战斗中可查看，清房后调整 · ESC 返回",journeySmallStyle);
  }
 }
}
