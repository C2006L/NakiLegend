using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype {
 public sealed partial class NakiPrototype {
  private readonly Sprite[] iceMechanicSprites=new Sprite[2];
  private Sprite BossPropSprite(int index){if(iceMechanicSprites[index]!=null)return iceMechanicSprites[index];var t=Resources.Load<Texture2D>("NakiArt/IceChapter/boss-props");if(t==null)return IceSprite(index==0?4:5);t.filterMode=FilterMode.Point;var s=Sprite.Create(t,new Rect(index*t.width/2f,0,t.width/2f,t.height),new Vector2(.5f,.08f),t.height/(index==0?1.65f:1.25f),0,SpriteMeshType.FullRect);ownedAssets.Add(s);iceMechanicSprites[index]=s;return s;}
  private sealed class MirrorVolley { public Enemy lens; public Vector2 aim; public float at; public GameObject warning; }
  private readonly List<MirrorVolley> mirrorVolleys=new List<MirrorVolley>();
  private int iceBattleWinches;
  private float tideCoreUntil;
  private void AddIceFanWarning(GameObject root,Vector2 at,Vector2 aim,int count,float spacing,float offset=0){EnemyMuzzleTell(at,aim).transform.SetParent(root.transform,true);}

  private GameObject NewIceWarning(Enemy e){var root=new GameObject("Locked ice attack telegraph");root.transform.SetParent(world,false);int move=e.attackNumber%3;
   if(!e.boss){if(e.kind==EnemyKind.SnowMoth)AddIceFanWarning(root,e.position,e.attackDirection,3,18);else{var line=ChapterLineWarning(e.position,e.attackDirection,2,.55f);line.transform.SetParent(root.transform,true);}return root;}
   if(move==0){var circle=WarningCircle(e.strikePoint,1.25f);circle.transform.SetParent(root.transform,true);AddIceFanWarning(root,e.position,e.attackDirection,e.secondPhase?9:7,20);}
   else if(move==1&&e.kind==EnemyKind.FrostMirror){var line=ChapterLineWarning(e.position,e.attackDirection,5,1.8f);line.transform.SetParent(root.transform,true);AddIceFanWarning(root,e.position,e.attackDirection,e.secondPhase?9:7,14);}
   else{AddIceFanWarning(root,e.position,e.attackDirection,e.secondPhase?13:9,e.kind==EnemyKind.TideGuardian?23:13);if(e.kind==EnemyKind.TideGuardian)AddIceFanWarning(root,e.position,e.attackDirection,e.secondPhase?8:5,32,16);}return root;
  }
  private void SpawnFrostMirrors(Enemy owner){
   foreach(Vector2 preferred in new[]{new Vector2(-3.8f,1),new Vector2(3.8f,1)}){
    Vector2 at=NearestEnemyFooting(FindFreeInteractionPoint(preferred),.4f);var lens=SpawnJourneyEnemy(EnemyKind.FrostLens,at,0);lens.iceOwner=owner;lens.summoned=true;
   }
   AddFloatingHit("霜镜凝结 · 可击碎",owner.position+Vector2.up*2,C("bef4ff"));
  }
  private void QueueMirrorVolleys(Enemy owner){foreach(Enemy lens in enemies)if(lens.kind==EnemyKind.FrostLens&&lens.iceOwner==owner&&lens.hp>0){Vector2 aim=(position-lens.position).normalized;mirrorVolleys.Add(new MirrorVolley{lens=lens,aim=aim,at=clock+1.1f,warning=MirrorRayWarning(lens.position,aim)});}}
  private GameObject MirrorRayWarning(Vector2 at,Vector2 aim){var root=new GameObject("Locked mirror rays");root.transform.SetParent(world,false);AddIceFanWarning(root,at,aim,3,9);return root;}
  private void TickMirrorVolleys(){for(int i=mirrorVolleys.Count-1;i>=0;i--){var v=mirrorVolleys[i];if(v.lens==null||v.lens.hp<=0||!enemies.Contains(v.lens)||v.lens.iceOwner==null||v.lens.iceOwner.hp<=0){if(v.warning!=null)Destroy(v.warning);mirrorVolleys.RemoveAt(i);continue;}if(clock<v.at)continue;if(v.warning!=null)Destroy(v.warning);v.lens.attackDirection=v.aim;IceFan(v.lens,3,9,5);SkillSound("ice_spike","cast");mirrorVolleys.RemoveAt(i);}}
  private void FrostMirrorBroken(Enemy lens){Enemy owner=lens.iceOwner;if(owner==null||owner.hp<=0)return;
   for(int i=mirrorVolleys.Count-1;i>=0;i--)if(mirrorVolleys[i].lens==lens){if(mirrorVolleys[i].warning!=null)Destroy(mirrorVolleys[i].warning);mirrorVolleys.RemoveAt(i);}
   SpellSpark(lens.position,C("c5f6ff"),.7f);SkillSound("ice_spike","hit");bool remaining=false;foreach(Enemy other in enemies)if(other.kind==EnemyKind.FrostLens&&other.iceOwner==owner&&other.hp>0)remaining=true;
   if(!remaining){owner.windup=owner.chargeUntil=0;if(owner.warning!=null)Destroy(owner.warning);owner.iceArmor=0;owner.recoveryUntil=clock+2;owner.nextAttack=clock+2.2f;AddFloatingHit("霜镜尽碎 · 反击窗口",owner.position+Vector2.up*2,C("aff9e2"));}
  }
  private Enemy ActiveTideGuardian(){foreach(Enemy e in enemies)if(e.kind==EnemyKind.TideGuardian&&e.hp>0)return e;return null;}
  private void DrawBattleWinches(){foreach(int side in new[]{0,1}){Vector2 at=FindFreeInteractionPoint(new Vector2(side==0?-5.5f:5.5f,-1));int bit=1<<side;var boss=ActiveTideGuardian();bool enabled=boss!=null&&boss.secondPhase;bool used=(iceBattleWinches&bit)!=0;
    AddChapterAction(330+side,at,used?"回流绞盘已开启":enabled?"转动回流绞盘 · 打断寒潮":"回流绞盘 · 等待冰壳松动");
    var wheel=Draw("Battle tide winch",BossPropSprite(1),at,Vector2.one,used?C("a4f6dc"):enabled?Color.white:C("7b94a3"),DepthAt(at.y));wheel.transform.SetParent(chapterFocus.transform,true);

   }}
  private void ActivateBattleWinch(int side){Enemy boss=ActiveTideGuardian();int bit=1<<side;if(boss==null||!boss.secondPhase){JourneyHint("晶壳尚未松动，先应对守望者的攻击");return;}if((iceBattleWinches&bit)!=0){JourneyHint("这一侧绞盘已经开启");return;}
   Vector2 winchAt=new Vector2(side==0?-5.5f:5.5f,-1);foreach(var action in chapterActions)if(action.kind==330+side)winchAt=action.at;iceBattleWinches|=bit;boss.windup=boss.chargeUntil=0;if(boss.warning!=null)Destroy(boss.warning);boss.recoveryUntil=clock+2;boss.nextAttack=clock+2.1f;tideCoreUntil=clock+3;
   if(iceBattleWinches==3)boss.iceArmor=0;
   for(int i=shots.Count-1;i>=0;i--)if(shots[i].hostile){if(shots[i].root!=null)Destroy(shots[i].root);shots.RemoveAt(i);}
   SpellSpark(boss.position,C("81e8ee"),1.4f);SkillSound("wave","cast");AddFloatingHit("回流冲开寒潮 · 核心暴露",boss.position+Vector2.up*2,C("8cefe4"));BuildChapterActions();PlayWinchTurn(winchAt);
  }
  private void ClearIceBossMechanics(){foreach(var v in mirrorVolleys)if(v.warning!=null)Destroy(v.warning);mirrorVolleys.Clear();iceBattleWinches=0;tideCoreUntil=0;}

  private readonly Sprite[] iceSluiceStates=new Sprite[2];
  private Sprite IceSluiceSprite(bool open){int i=open?0:1;if(iceSluiceStates[i]!=null)return iceSluiceStates[i];var t=Resources.Load<Texture2D>("NakiArt/IceChapter/sluice-states");var sprite=Sprite.Create(t,new Rect(i*t.width/2f,0,t.width/2f,t.height),Vector2.one*.5f,t.height/2.2f,0,SpriteMeshType.FullRect);ownedAssets.Add(sprite);iceSluiceStates[i]=sprite;return sprite;}
  private readonly List<GameObject> iceReturnRipples=new List<GameObject>();
  private void BuildIceSluiceRoom(){iceReturnRipples.Clear();

   for(int side=0;side<2;side++){bool opened=side==0?chapterWest:chapterEast;float x=side==0?-3:3;
    var water=Stretch("Stone-lined return sluice",IceSluiceSprite(opened),new Vector2(side==0?-1.7f:1.7f,2.4f),new Vector2(3.4f,2.2f),Color.white,-980);water.transform.SetParent(chapterFocus.transform,true);
    var wheel=Draw("Repaired story winch",BossPropSprite(1),new Vector2(x,1),Vector2.one,opened?C("aff2da"):Color.white,DepthAt(1));wheel.transform.SetParent(chapterFocus.transform,true);
    AddChapterAction(312+side,new Vector2(x,.75f),opened?(side==0?"西绞盘已复位":"东绞盘已复位"):(side==0?"清理西闸叶 · 转动绞盘":"清理东闸叶 · 转动绞盘"));

    for(int i=0;i<6;i++){var ripple=Stretch("Return tide ripple",square,new Vector2(side==0?-3+i*.5f:.2f+i*.5f,2.4f),new Vector2(.18f,.035f),C("c3f7ff"),-978);ripple.transform.SetParent(chapterFocus.transform,true);ripple.SetActive(opened);iceReturnRipples.Add(ripple);}
   }
   AddChapterAction(314,new Vector2(0,-1.4f),"开启回流总闸",BossPropSprite(1));
  }
  private void TickIceReturnWater(){if(!iceSampleMode||!iceFullChapter||currentRoom!=41)return;for(int i=0;i<iceReturnRipples.Count;i++){var ripple=iceReturnRipples[i];if(ripple==null||!ripple.activeSelf)continue;float left=i<6?-3.2f:.2f;ripple.transform.position=new Vector2(left+Mathf.Repeat(i*.5f+clock*.5f,3),2.4f+.1f*Mathf.Sin(i+clock*2));}}
  private void BuildIceRescueWinch(){var winch=Draw("Expedition gate winch",BossPropSprite(1),new Vector2(-3.8f,-.4f),Vector2.one,chapterRanger?C("aff2da"):Color.white,DepthAt(-.4f));winch.transform.SetParent(chapterFocus.transform,true);}
 }
}
