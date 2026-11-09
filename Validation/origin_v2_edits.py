from pathlib import Path
root=Path(__file__).resolve().parents[1]/'Assets/NakiPrototype'
def edit(file,old,new):
 p=root/file;s=p.read_text(encoding='utf-8-sig');assert old in s,(file,old[:90]);p.write_text(s.replace(old,new),encoding='utf-8')
p=root/'NakiPrototype.OriginLegacy.cs';s=p.read_text(encoding='utf-8');s=s.replace('originGuardReturnAt,originBeamUntil,originBeamNext','originGuardReturnAt')
a=s.index('  private string PrimarySkillName');b=s.index('  private bool ValidOriginLegacy',a)
s=s[:a]+'''  private string PrimarySkillName {get{return OriginSkillName(0);}}
  private float PrimarySkillCost {get{return OriginSkillCost(0);}}
  private float PrimarySkillCooldown {get{return OriginSkillCooldown(0);}}
  private void RestoreOriginLegacy(int lore,int knowledge,int skill,int queued)
  {
   originLoreBits=lore&63;originKnowledge=(knowledge|(lore&31))&31;queuedOriginChapter=Mathf.Clamp(queued,0,5);primarySkill=Mathf.Clamp(skill,0,2);
   originAbilityMenu=false;originGuardUntil=originGuardReturnAt=0;ResetOriginCombat();
  }
  private bool OriginAbilityKnown(int id)
  {
   if(id<0||id>2||!orbLearned)return false;
   int known=originKnowledge|(originLoreBits&31);if(bossPracticeMode||lightningTrialMode||thunderChapterPracticeBoss>=0)known|=10;
   return id==0||id==1&&(known&2)!=0||id==2&&(known&10)==10;
  }
''' +s[b:]
s=s.replace('primarySkill=OriginAbilityKnown(source.primary)?source.primary:0;','primarySkill=Mathf.Clamp(source.primary,0,2);')
s=s.replace('TellStory(VoyagerLines(i));if(i==1||i==3)AnnounceDiscovery("主属性战技已领悟 · B 行囊中配置");','TellStory(VoyagerLines(i));if(i==1||i==3)AnnounceDiscovery(OriginAbilityKnown(2)?"已领悟 O "+OriginSkillName(1)+" · P "+OriginSkillName(2):OriginAbilityKnown(1)?"已领悟 O "+OriginSkillName(1):"主属性记录已收好 · 先在鸣沙洲古井碑领悟基础战技");')
a=s.index('   if(originBeam!=null){');b=s.index('  private void CastEmpoweredMeteor()',a)
s=s[:a]+'   TickOriginVisuals();\n  }\n'+s[b:]
s=s.replace('   if(PrimaryVolleyPresent(Element.Dark,3)||darkPower<21)','   if(!OriginAbilityKnown(2)||clock<originThirdReady||clock<dashUntil)return;\n   if(PrimaryVolleyPresent(Element.Dark,3)||darkPower<21)')
s=s.replace('SpendPower(PowerPool.Dark,21);nextSpell=clock+1.2f;','SpendPower(PowerPool.Dark,21);originThirdReady=clock+1.2f;')
a=s.index('  private void DrawOriginAbilityMenu(')
s=s[:a]+'''  private void DrawOriginAbilityMenu(float w,float h)
  {
   HudRect(new Rect(0,0,w,h),C("10212a"));CenterLabel(new Rect(w/2-430,h/2-265,860,45),origin==Origin.Dark?"贤者之石 · 残响与意志":"掌心之光 · 凝聚、守护、贯虹",journeyTitleStyle);
   for(int i=0;i<3;i++){
    Rect card=new Rect(w/2-400,h/2-195+i*112,800,96);bool known=OriginAbilityKnown(i);HudRect(card,known?C("1c3b40"):C("202e38"));
    FittedLabel(new Rect(card.x+18,card.y+12,760,30),char.ToUpperInvariant(OriginKeys[i])+" · "+OriginSkillName(i)+(known?"":" · 尚未领悟"),journeyTextStyle);
    string description=origin==Origin.Dark?new[]{"按住释放五颗追踪流星 · 每组 1 暗能","标记可见敌人 6 秒，流星命中触发蚀魂爆发 · 8 暗能","五颗强化追踪球 · 21 暗能"}[i]:new[]{"按住 L 持续照射，松开或 K 闪避停止 · 启动 4，每秒 18 光能","1.5 秒减伤光幕，真实抵挡返还光能，上限 8 · 消耗 20","短暂凝聚后发射宽光束，贯穿敌人，遇掩体停止 · 消耗 35"}[i];
    FittedLabel(new Rect(card.x+18,card.y+49,760,37),known?description:OriginLearningPlace(i),journeySmallStyle,true);
   }
   CenterLabel(new Rect(w/2-420,h/2+145,840,62),origin==Origin.Dark?"储能无上限；残响来自击败有灵生物。\nL / O / P 独立释放，U / I 保留元素技能。":"施法期间暂停回光；停手 1.5 秒后，每秒恢复 4。\nL / O / P 独立释放，U / I 保留元素技能。",journeySmallStyle);
   if(FlatButton(new Rect(w/2-110,h/2+225,220,42),"B / ESC 返回"))originAbilityMenu=false;
  }
 }
}
''';p.write_text(s,encoding='utf-8')
p=root/'NakiPrototype.CombatV3.cs';s=p.read_text(encoding='utf-8');a=s.index('  private void CastLightVolley()');b=s.index('  private void CastMeteorVolley()',a);s=s[:a]+'  private void CastLightVolley(){BeginLightChannel();}\n'+s[b:];s=s.replace('private void CastMeteorVolley(){if(CastOriginAbility())return;','private void CastMeteorVolley(){');p.write_text(s,encoding='utf-8')
edit('NakiPrototype.Powers.cs','originGuardUntil=originBeamUntil=0;if(originBeam!=null)Destroy(originBeam);','originGuardUntil=originGuardReturnAt=0;ResetOriginCombat();')
edit('NakiPrototype.Powers.cs','if(pool==PowerPool.Light)energy-=cost;','if(pool==PowerPool.Light){energy-=cost;lightRegenReady=Mathf.Max(lightRegenReady,clock+LightRegenDelay);}')
edit('NakiPrototype.Elements.cs','if(origin==Origin.Light&&clock<originGuardUntil){amount*=.3f;if(clock>=originGuardReturnAt){energy=Mathf.Min(100,energy+2);originGuardReturnAt=clock+.6f;}}','if(origin==Origin.Light&&clock<originGuardUntil){amount*=.3f;RefundGuardLight();}')
edit('NakiPrototype.Elements.cs','swordHitPending = false; swordUntil = castPoseUntil = clock;','StopOriginAction();swordHitPending = false; swordUntil = castPoseUntil = clock;')
edit('NakiPrototype.Elements.cs','凝成一颗明亮的光球。','凝成一线沿石路延伸的光。')
edit('NakiPrototype.Elements.cs','已领悟 L 元气弹 · 消耗6光之力，五弹齐射','已领悟 L 辉光束 · 按住照射，启动4＋每秒18光能；松开或K停止')
edit('NakiPrototype.Origin.cs','L 元气弹 · 五颗追踪光球\\n光之力自然恢复\\n适合稳定持续的旅程','L 辉光束 · 按住自动锁定照射\\n停手1.5秒后，每秒恢复4光能\\n逐步领悟 O 光幕与 P 贯虹')
edit('NakiPrototype.CombatPolish.cs','RelicHit(shot,target,contact);ResolveChapterArc(shot, target);','TrySoulSealHit(shot,target);RelicHit(shot,target,contact);ResolveChapterArc(shot, target);')
edit('NakiPrototype.Enemies.cs','CancelBowCharge();ResetEncounterDirector();','StopOriginAction();ResetOriginRoomEffects();CancelBowCharge();ResetEncounterDirector();')
edit('NakiPrototype.Checkpoints.cs','&&health>0&&!challengeActive','&&health>0&&!OriginActionActive&&!challengeActive')
edit('NakiPrototype.Audio.cs','bool suspended=AudioSuspended;','bool suspended=AudioSuspended;UpdateOriginAudio();')
edit('NakiPrototype.cs','if(AudioSuspended||StoryActive||openingActive||health<=0||doorTransition>0)CancelBowCharge();','if(AudioSuspended||StoryActive||openingActive||health<=0||doorTransition>0){StopOriginAction();CancelBowCharge();}')
edit('NakiPrototype.cs','if(HandleWorldAtlas())return;','if(HandleWorldAtlas()){StopOriginAction();return;}')
edit('NakiPrototype.cs','if(HandleIceVoyage())return;','if(HandleIceVoyage()){StopOriginAction();return;}')
edit('NakiPrototype.cs','if(HandleTalents())return;','if(HandleTalents()){StopOriginAction();return;}')
edit('NakiPrototype.cs','if (enableDesertJourney && HandleJourneyOverlayInput()){CancelBowCharge();return;}','if (enableDesertJourney && HandleJourneyOverlayInput()){StopOriginAction();CancelBowCharge();return;}')
edit('NakiPrototype.cs','if (paused){CancelBowCharge();return;}','if (paused){StopOriginAction();CancelBowCharge();return;}')
edit('NakiPrototype.cs','if(origin!=Origin.Dark)energy = Mathf.Min(100, energy + (enableDesertJourney ? journeyEnergyRegen : 14) * dt);else energy=0;','if(enableDesertJourney)TickLightRecovery(dt);else energy=Mathf.Min(100,energy+14*dt);')
edit('NakiPrototype.cs','BeginDodge(input);','StopOriginAction();BeginDodge(input);')
edit('NakiPrototype.cs',"if(enableDesertJourney)TickBowInput(Held('j'));","if(enableDesertJourney){TickOriginInput(dt,Held('l'),Pressed('o'),Pressed('p'));if(OriginActionActive)CancelBowCharge();else TickBowInput(Held('j'));}")
edit('NakiPrototype.cs',"&& Held('j') && clock >= nextSword","&& !OriginActionActive && Held('j') && clock >= nextSword")
edit('NakiPrototype.cs',"if (Held('l') && clock >= nextSpell && (enableDesertJourney || energy >= 25)","if (!enableDesertJourney && Held('l') && clock >= nextSpell && energy >= 25")
edit('NakiPrototype.cs',"case 'u': return keyboard.uKey.wasPressedThisFrame;","case 'u': return keyboard.uKey.wasPressedThisFrame;\n                case 'o': return keyboard.oKey.wasPressedThisFrame;\n                case 'p': return keyboard.pKey.wasPressedThisFrame;")
edit('NakiPrototype.JourneyHud.cs','   SkillBox(w-576,y,origin==Origin.Unchosen?"L 主属性":"L "+PrimarySkillName,orbLearned,nextSpell,PrimarySkillCost,true);','   for(int slot=0;slot<3;slot++)SkillBox(w-950+slot*188,y,char.ToUpperInvariant(OriginKeys[slot])+" "+OriginSkillName(slot),OriginAbilityKnown(slot),OriginSkillReady(slot),OriginSkillCost(slot),true);')
edit('NakiPrototype.JourneyHud.cs','w-386+i*190','w-386+i*190')
edit('NakiPrototype.JourneyHud.cs','(origin==Origin.Dark?"流星":"元气弹")','(origin==Origin.Dark?"流星":"辉光束")')
edit('NakiPrototype.JourneyHud.cs','   if(!light)cost=SkillElementCost(cost);','   int originSlot=light&&label.Length>0?label[0]==\'O\'?1:label[0]==\'P\'?2:0:0;\n   if(!light)cost=SkillElementCost(cost);')
edit('NakiPrototype.JourneyHud.cs','   FittedLabel(new Rect(x+14,y+29,164,23),detail,','   if(light){if(!learned)detail=originSlot==0?"鸣沙洲领悟":originSlot==1?"雷雨高原领悟":"赤炉群岛领悟";else if(origin==Origin.Light&&originSlot==0)detail=lightChannel?"照射中 · 18 / 秒":lightNeedsRelease?"松开 L 后再次照射":"启动4 ＋ 18 / 秒";else if(originSlot==2&&solarCharging)detail="正在凝聚";}\n   FittedLabel(new Rect(x+14,y+29,164,23),detail,')
edit('NakiPrototype.JourneyHud.cs','light?PrimarySkillCooldown:','light?OriginSkillCooldown(originSlot):')
edit('NakiPrototype.VoyagerStory.cs','少年依照边注，将紫光收成一束。','少年依照铜纹边注，学着让残响在目标身上留下一枚印记。')
print('Connected independent L/O/P actions, recovery, learning gates, projectiles, HUD and cleanup')
