using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private sealed class GroundShock { public Vector2 at;public float born,radius,last;public Element element;public GameObject root; }
  private readonly System.Collections.Generic.List<GroundShock> groundShocks=new System.Collections.Generic.List<GroundShock>();
  private Sprite sectorWarning;
  private void MakeChampion(Enemy e)
  {
   if(e.elite)return;e.elite=true;e.hp=e.maximum*=1.35f;e.speed*=1.08f;
   e.eliteKind=e.kind==EnemyKind.SandKite&&journeyRooms[currentRoom].stage>0?2:1;
   if(e.eliteKind==2){e.affinity=Element.Lightning;e.attackElement=Element.Lightning;JourneyHint("雷印精英：风技能可破势；地面雷区需要躲避");}
  }
  private string EnemyTitle(Enemy e)
  {string identity=woodlandMode&&ArrayIndexWood(e.kind)>=0?WoodNames[ArrayIndexWood(e.kind)]:EnemyNames[(int)e.kind];if(e.tacticRole==5)return "盾阵 · "+identity;if(e.tacticRole==3)return "护阵者 · "+identity;if(e.tacticRole==4)return "狙击手 · "+identity;if(woodlandMode){int k=ArrayIndexWood(e.kind);if(k>=0)return WoodNames[k];}return e.boss?EnemyNames[(int)e.kind]:e.elite?(e.eliteKind==2?"雷印 · ":e.kind==EnemyKind.StoneSentinel?"重甲 · ":"追猎 · ")+EnemyNames[(int)e.kind]:EnemyNames[(int)e.kind];}
  private string EnemyMoveName(Enemy e)
  {
   if(IsManorEnemy(e.kind))return e.kind==EnemyKind.ManorCaptain?new[]{"枪芒齐射","破盾冲锋 · 绕侧反击","长枪横扫 · 收招破绽","错时誓印 · 离开落点"}[e.attackNumber%4]:e.kind==EnemyKind.ManorPriest?"暗黑星陨 · 单次落点":e.kind==EnemyKind.ManorHammer?"重锤震地":e.kind==EnemyKind.ManorAxe?"断旗横扫":ArmedIntentName(e);
   if(e.boss){string signature=ActiveSignatureTitle(e);if(signature!=null)return signature;}
   if(!e.boss&&!CombatFixture(e)&&EncounterTactics)return ArmedIntentName(e);
   if(woodlandMode)return e.kind==EnemyKind.WoodParasite?"寄生种子":new[]{"分层种阵","锁向冲撞","孢雾落点"}[e.attackNumber%3];
   if(IsCoreEnemy(e.kind))return e.boss?new[]{"双段潮焰","开口熔环","错时喷口"}[e.attackNumber%3]:e.kind==EnemyKind.AshPackbeast?"蓄势冲撞":"引焰扇射";
   if(IsWorkshopEnemy(e.kind))return e.kind==EnemyKind.ForgeMaster?new[]{"错时锤焰","链钩牵模","炉芯折射"}[e.attackNumber%3]:e.kind==EnemyKind.ForgeGuard?"长矛刺线":e.kind==EnemyKind.MoltenCrab?"横钳冲扑":e.kind==EnemyKind.FurnaceCore?"炉芯扇射":"折射铸模";
   if(IsFireEnemy(e.kind))return e.kind==EnemyKind.RedSpineLizard?new[]{"延迟熔爆","赤脊冲扑","火团扇射"}[e.attackNumber%3]:e.kind==EnemyKind.CoalThrower?"投掷延迟煤包":e.kind==EnemyKind.AshBat?"俯冲扑击":e.kind==EnemyKind.EmberLizard?"三扇火团":"短刃突刺";
   if(e.kind==EnemyKind.FrostMirror)return e.attackNumber%3==1?"滑扑与碎镜":"霜镜扇射";if(e.kind==EnemyKind.TideGuardian)return e.attackNumber%3==0?"冻潮落点":"分层寒潮弹幕";if(e.kind==EnemyKind.SnowCrab)return "雪钳突刺";if(e.kind==EnemyKind.SnowMoth)return "风道碎冰";
   if(IsExpandedIceEnemy(e.kind))return e.kind==EnemyKind.FrostHound?"霜牙扑击":e.kind==EnemyKind.IceArcher?"双矢连射":"刻霜落点";
   if(IsIceEnemy(e.kind))return e.boss?new[]{"晶角冲锋","碎冰扇射","霜裂落点"}[Mathf.Max(0,(e.attackNumber-1)%3)]:e.kind==EnemyKind.IceGuard?"冰斧突刺":"冰晶扇射";
   if(IsChapterBoss(e.kind))return ChapterBossMove(e);
   int move=(e.attackNumber-1)%3;
   if(e.kind==EnemyKind.WindScorpion)return new[]{"尾刺落点","风钳横扫","风刃齐射"}[Mathf.Max(0,move)];
   if(e.kind==EnemyKind.Colossus)return new[]{"扩散震波","碎石齐射","召唤守卫"}[Mathf.Max(0,move)];
   if(e.kind==EnemyKind.ApeKing)return new[]{"跃击落点","投石扇射","呼唤荆蓬"}[Mathf.Max(0,move)];
   return e.eliteKind==2?"雷印落点":e.kind==EnemyKind.StoneSentinel?"重锤震地":"蓄力攻击";
  }
  private GameObject WarningCircle(Vector2 at,float radius)
  {
   GameObject root=Draw("Attack area",disc,at,Vector2.one*radius*2,new Color(1,.22f,.1f,.16f),-640);
   GameObject edge=Draw("Attack area edge",windRingSprite??disc,at,Vector2.one*radius*2,new Color(1,.48f,.22f,.85f),-638);edge.transform.SetParent(root.transform,true);return root;
  }
  private GameObject WarningSector(Vector2 at,Vector2 direction,float radius)
  {
   if(sectorWarning==null)
   {
    var t=new Texture2D(128,128,TextureFormat.RGBA32,false);t.filterMode=FilterMode.Point;
    for(int y=0;y<128;y++)for(int x=0;x<128;x++){Vector2 p=new Vector2(x-63.5f,y-63.5f)/62;float r=p.magnitude,a=Mathf.Abs(Mathf.Atan2(p.y,p.x)*Mathf.Rad2Deg);t.SetPixel(x,y,r<=1&&a<=55?new Color(1,1,1,r>.94f||a>51?1:.22f):Color.clear);}t.Apply();sectorWarning=Sprite.Create(t,new Rect(0,0,128,128),Vector2.one*.5f,128,0,SpriteMeshType.FullRect);ownedAssets.Add(t);ownedAssets.Add(sectorWarning);
   }
   GameObject obj=Draw("Sweep warning",sectorWarning,at,Vector2.one*radius*2,C("f27d51"),-638);obj.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg);return obj;
  }
  private void BeginBossPhase(Enemy e)
  {
   e.secondPhase=true;e.phaseUntil=clock+1.2f;e.windup=0;e.chargeUntil=clock;e.recoveryUntil=e.phaseUntil;e.nextAttack=e.phaseUntil+.45f;
   if(e.warning!=null)Destroy(e.warning);AddFloatingHit("风石共鸣 · 第二阶段",e.position+Vector2.up*1.3f,C("e5bd7d"));
   for(int i=shots.Count-1;i>=0;i--)if(shots[i].hostile){Destroy(shots[i].root);shots.RemoveAt(i);}
  }
  private void SpawnGroundShock(Enemy e)
  {
   var shock=new GroundShock{at=e.position,born=clock,radius=e.secondPhase?4.5f:3.8f,element=e.attackElement};shock.root=Draw("Expanding ground shock",windRingSprite??disc,shock.at,Vector2.one*.2f,C("e8ac71"),-620);groundShocks.Add(shock);
  }
  private void TickGroundShocks()
  {
   for(int i=groundShocks.Count-1;i>=0;i--){GroundShock g=groundShocks[i];float t=(clock-g.born)/1.05f;float radius=Mathf.Lerp(.1f,g.radius,t),d=Vector2.Distance(position,g.at);
    if(t>=1){Destroy(g.root);groundShocks.RemoveAt(i);continue;}
    g.root.transform.localScale=Vector3.one*radius*2;
    if(d>=g.last-.25f&&d<=radius+.25f&&!ProjectilePathBlocked(g.at,position,.02f))TakeJourneyDamage(2,g.element);g.last=radius;
   }
  }
  private Vector2 SafeEnemyLanding(Enemy e,Vector2 requested)
  {
   return NearestEnemyFooting(requested,e.radius);
  }
  private void UpdateEliteSigil(Enemy e)
  {
   if(!e.elite)return;
   if(e.armorSigil==null)e.armorSigil=Draw("Elite identity ring",windRingSprite??disc,e.position,new Vector2(1.1f,.5f),e.eliteKind==2?C("e3bc77"):C("caa86b"),-610);
   e.armorSigil.transform.position=e.position+Vector2.up*.05f;
   e.armorSigil.GetComponent<SpriteRenderer>().color=e.kind==EnemyKind.StoneSentinel&&clock<e.recoveryUntil?C("7ed8c7"):e.eliteKind==2?C("e3bc77"):C("caa86b");
  }
 }
}
