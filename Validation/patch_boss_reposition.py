from pathlib import Path
r=Path('D:/NakiLegend/Repository');a=r/'Assets/NakiPrototype'
p=a/'NakiPrototype.EncounterDirector.cs';s=p.read_text(encoding='utf-8-sig').replace('TickBossBreathing(e)','TickBossBreathing(e,dt)').replace('if(bossAttackEmission){int previous=count;','if(bossAttackEmission&&count>1){int previous=count;');p.write_text(s,encoding='utf-8')
p=a/'NakiPrototype.BossBreathing.cs';s=p.read_text(encoding='utf-8-sig').replace('TickBossBreathing(Enemy e)','TickBossBreathing(Enemy e,float dt)');s=s.replace('   e.velocity=Vector2.zero;e.windup=0;', '''   Vector2 before=e.position;
   if(e.boss&&!RootedBoss(e)&&dt>0){Vector2 to=position-e.position;Vector2 aim=to.sqrMagnitude>.01f?to.normalized:Vector2.down;Vector2 step=to.magnitude<2.6f?-aim:new Vector2(-aim.y,aim.x)*((e.attackNumber&1)==0?1:-1);e.position=BossStep(e,step*.85f*dt);e.attackDirection=aim;}
   e.velocity=dt>0?(e.position-before)/dt:Vector2.zero;e.windup=0;''').replace('UpdateJourneyEnemyPose(e,false);return true;','UpdateJourneyEnemyPose(e,e.velocity.sqrMagnitude>.00001f);return true;');p.write_text(s,encoding='utf-8')
p=a/'NakiPrototype.CombatV3.cs';s=p.read_text(encoding='utf-8-sig').replace('e.hp>0)?.75f:1','e.hp>0)?(manorCourtyardMode?.5f:.75f):1');p.write_text(s,encoding='utf-8')
p=a/'NakiPrototype.EncounterRefresh.cs';s=p.read_text(encoding='utf-8-sig').replace('additionalCoverCount=0;if(manorCourtyardMode','additionalCoverCount=0;if(room.kind==RoomKind.Boss){AddBossFightCover(room);return;}if(manorCourtyardMode');idx=s.index('  private void AddEncounterCover(');s=s[:idx]+'''  private void AddBossFightCover(RoomNode room)
  {
   foreach(Vector2 at in new[]{new Vector2(-3,1.8f),new Vector2(3,-1.8f),new Vector2(-4,-1.8f),new Vector2(4,1.8f)}){
    Rect foot=new Rect(at.x-.5f,at.y-.1f,1,.5f),lane=Rect.MinMaxRect(foot.xMin-1,foot.yMin-1,foot.xMax+1,foot.yMax+1);
    if(IntersectsTransit(lane.center,lane.size))continue;bool blocked=false;foreach(Rect other in obstacles)if(lane.Overlaps(other)){blocked=true;break;}if(blocked)continue;
    Sprite art=manorCourtyardMode?ManorSprite("prop-3",2):woodlandMode?WoodlandNature(1):fireChapterMode?(room.stage==2?CoreArt(3):room.stage==1?WorkshopProp(1):FireProp(1)):iceSampleMode?IceSprite(4):ThunderActive?PressureProp(1):pillarArtwork;
    if(art==null)continue;Draw("Boss refuge cover",art,at,Vector2.one*.7f,Color.white,DepthAt(at.y));obstacles.Add(foot);projectileObstacles.Add(foot);additionalCoverCount++;break;
   }
  }
'''+s[idx:];p.write_text(s,encoding='utf-8')
f=r/'Validation/boss-offense-files.txt';f.write_text(f.read_text(encoding='utf-8')+'\nAssets/NakiPrototype/NakiPrototype.EncounterRefresh.cs',encoding='utf-8')
