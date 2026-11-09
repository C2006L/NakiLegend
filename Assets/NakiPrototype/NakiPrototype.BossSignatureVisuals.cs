using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private readonly Dictionary<int,Sprite> signatureGlyphs=new Dictionary<int,Sprite>();
  private Sprite SignatureGlyph(int kind)
  {
   Sprite sprite;if(signatureGlyphs.TryGetValue(kind,out sprite))return sprite;
   string[] rows=kind==1?new[]{"....xx......","...xIxx.....","..xIIx......",".xIIx.......","xxIxxxxxxx..","....xIIx....","...xIIx.....","..xIIx......","..xxx.......","..xx........"}:kind==2?new[]{".....x......","....xIx.....","....xIIx....","..x.xIIx....",".xIxxIIIx...",".xIIIIIIx...","xIIIIIIIIx..","xIIIIIIIIx..",".xIIIIIIx...","..xxxxxx...."}:kind==3?new[]{"...xxxxxx...","..xxIIIIxx..",".xxI....Ixx.","xxI......Ixx","xxI......Ixx",".xxI....Ixx.","..xxIIIIxx..","...xxxxxx..."}:kind==4?new[]{"....xx......","...xIIxx....","..xIIIIxx...",".xIIxxIIxx..","xIIx..xIIxx.","xIIx..xIIxx.",".xIIxxIIxx..","..xIIIIxx...","...xxxx....."}:kind==5?new[]{".....xx.....","....xIIx....","....xIIx....","...xIIIIx...","...xIIIIx...","..xIIIIIIx..","..xIIIIIIx..",".xIIIIIIIIx.","..xxxxxxxx.."}:new[]{"....xxxx....","..xxIIIIxx..",".xIIIIIIIIx.","xIII....IIIx","xII......IIx",".xII....IIx.","..xxIIIIxx..","....xxxx...."};
   sprite=MakeSprite(rows,new Dictionary<char,Color>{{'x',C("805a32")},{'I',Color.white}},12);sprite.name="Boss signature glyph "+kind;signatureGlyphs[kind]=sprite;return sprite;
  }
  private Color SignatureZoneTint(SignatureZone z)
  {return z.mode==4&&IsCapitalBoss(z.owner.kind)?C("e77d58"):z.mode==0?C("e5c18d"):z.mode==3?C("7ce4ff"):z.mode==7?C("a3d474"):z.mode==6?C("ffc16a"):z.owner.attackElement==Element.Fire?C("ff9946"):SpellTint(z.owner.attackElement);}
  private void BuildSignatureVisuals(SignatureZone z)
  {
   int count=z.mode==8?0:z.mode==2?20:z.mode==5?2:z.mode==6?7:12;z.motifs=new SpriteRenderer[count];
   for(int i=0;i<count;i++){int glyph=z.mode==2?0:z.mode==3?1:z.mode==7?4:z.mode==1?5:z.mode==6?3:z.mode==4&&z.owner.attackElement==Element.Fire?2:0;
    var part=Draw("Signature active motif "+z.mode+" / "+i,SignatureGlyph(glyph),z.at,Vector2.one*.1f,Color.clear,z.mode==2?-610:z.mode==6?DepthAt(z.at.y)+7:-612);part.transform.SetParent(z.root.transform,true);z.motifs[i]=part.GetComponent<SpriteRenderer>();}
  }
  private void UpdateSignatureVisuals(SignatureZone z)
  {
   bool active=clock>=z.active;float ready=Mathf.Clamp01((clock-z.born)/.85f),fade=Mathf.Clamp01((z.end-clock)/.3f),radius=z.mode==7?Mathf.Lerp(z.radius,1.9f,Mathf.Clamp01((clock-z.active)/3)):z.radius;
   Color color=SignatureZoneTint(z);if(z.left!=null&&z.mode!=1&&z.mode!=5){foreach(var r in z.left.GetComponentsInChildren<SpriteRenderer>()){Color c=color;c.a=(r.gameObject==z.left?.12f:.65f)*(active?1:ready)*fade;r.color=c;}}
   for(int i=0;i<z.motifs.Length;i++){
    var r=z.motifs[i];float angle=i*Mathf.PI*2/z.motifs.Length,spin=(clock-z.active)*1.3f,pulse=.85f+.15f*Mathf.Sin(clock*13+i),size=.3f;Vector2 at=z.at;
    if(z.mode==2){float t=i/(float)(z.motifs.Length-1);at+=(CoverBeamEnd(z.at,z.dir,z.radius,.325f)-z.at)*t;size=active?.38f:.16f;angle=Mathf.Atan2(z.dir.y,z.dir.x)+Mathf.PI*.5f;}
    else if(z.mode==5){float y=z.at.y+Mathf.Max(0,clock-z.active)*1.8f;at=new Vector2(z.gap+(i==0?-1:1)*1.4f,y);size=.28f;angle=Mathf.PI*.5f;}
    else if(z.mode==6){angle=Mathf.Atan2(z.dir.y,z.dir.x)+(i-3)*.35f;at=z.owner.position+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*1.4f;size=.42f;}
    else if(z.mode==3){at+=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius*(i%2==0?.72f:.3f);size=active?.52f:.2f;angle+=spin*.3f;}
    else if(z.mode==1){at+=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*.7f;size=active?.26f:.14f;}
    else if(z.mode==4&&z.owner.attackElement==Element.Fire){at+=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius*(i%3==0?.25f:.65f);at+=Vector2.up*(.15f+.07f*Mathf.Sin(clock*16+i));size=active?.58f:.18f;angle=Mathf.PI*.5f;}
    else{angle+=spin;float orbit=z.mode==0?Mathf.Lerp(.25f,.85f,i/(float)z.motifs.Length):.65f;at+=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius*orbit;size=active?(z.mode==7?.48f:.32f):.13f;}
    r.transform.position=at;r.transform.rotation=Quaternion.Euler(0,0,angle*Mathf.Rad2Deg-90);r.transform.localScale=Vector3.one*size*pulse;Color tint=color;tint.a=(active?.95f:ready*.5f)*fade;r.color=tint;
   }
  }
  private void BeginBossCastVisual(Enemy e,SignatureState state)
  {
   if(state.castFx!=null)Destroy(state.castFx);state.castFx=new GameObject("Boss signature preparation aura");state.castFx.transform.SetParent(world,false);state.glyphs=new SpriteRenderer[6];
   Color tint=e.attackElement==Element.Fire?C("ffb064"):e.attackElement==Element.Ice?C("a9edff"):woodlandMode?C("a6df7b"):SpellTint(e.attackElement);
   for(int i=0;i<6;i++){var obj=Draw("Gathering boss power",SignatureGlyph(BossIdentity(e)==5?1:BossIdentity(e)==7?5:3),e.position,Vector2.one*.3f,tint,-608);obj.transform.SetParent(state.castFx.transform,true);state.glyphs[i]=obj.GetComponent<SpriteRenderer>();}
  }
  private void TickBossCastVisuals()
  {
   foreach(var pair in bossSignatures){Enemy e=pair.Key;var state=pair.Value;if(state.castFx==null)continue;
    if(e.hp<=0||!enemies.Contains(e)||clock>=state.hold+.25f){Destroy(state.castFx);state.castFx=null;continue;}
    float t=Mathf.Clamp01(1-(state.hold-clock)/.8f);for(int i=0;i<state.glyphs.Length;i++){var r=state.glyphs[i];float angle=i*Mathf.PI/3+clock*.65f;r.transform.position=e.position+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*(e.radius+.5f-t*.18f);r.transform.localScale=Vector3.one*(.25f+.14f*t);Color c=r.color;c.a=.4f+.5f*t;r.color=c;}
   }
  }
  private string ActiveSignatureTitle(Enemy e)
  {SignatureState state;return bossSignatures.TryGetValue(e,out state)&&clock<state.visibleUntil?SignatureNames[BossIdentity(e)]+(clock<state.hold?" · 蓄势":" · 已生效"):null;}
  private string BossCombatHint(Enemy e)
  {
   if(IsCapitalBoss(e.kind)){if(RecoveryWindow(e))return "收招移动 · 命中可压制下一轮起手";return CapitalMoveName(e,CapitalState(e).move)+" · 机关可击破，光束可用石柱阻挡";}string beamTitle=ActiveDuelBeamTitle(e);if(beamTitle!=null)return beamTitle;
   string active=ActiveSignatureTitle(e);if(active!=null){SignatureState state=bossSignatures[e];if(clock<state.hold)return active;
    return SignatureNames[BossIdentity(e)]+" · "+new[]{"离开涡心，留意旋转风弹","绕过升起的石柱","提防石刃返程","走出链路，避免被拉近","绕开两座雷场","落雷依次抵达，改变走向","避开晶柱与冻结地带","击碎镜体，解除夹击","穿过缺口，避开两侧浪墙","离开持续燃烧的火区","避开两条交错锻链","正面护壳，绕侧后反击","清理新生虫群","离开扩张的孢床","先拆寄生根结，解除护阵"}[BossIdentity(e)];}
   return e.phaseUntil>clock?"力量涌动 · 即将变招":e.windup>0?"蓄力 · "+EnemyMoveName(e):clock<e.recoveryUntil?"收招 · 反击机会":"观察前摇，留意下一次攻击";
  }
 }
}
