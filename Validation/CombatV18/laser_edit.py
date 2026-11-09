from pathlib import Path
import json,shutil
r=Path('D:/NakiLegend/Repository');root=r/'Assets/NakiPrototype';out=r/'Validation/CombatV18';manifest=out/'files.json';files=set(json.loads(manifest.read_text()))
def edit(name,old,new):
 p=root/name;s=p.read_text(encoding='utf-8-sig')
 if new in s:return
 assert old in s,(name,old[:70]);rel=str(p.relative_to(r)).replace('\\','/');bak=out/'backup-source'/rel;bak.parent.mkdir(parents=True,exist_ok=True)
 if not bak.exists():shutil.copy2(p,bak)
 p.write_text(s.replace(old,new),encoding='utf-8');files.add(rel);manifest.write_text(json.dumps(sorted(files)),encoding='utf-8')
edit('NakiPrototype.CombatV3.cs','AttackByIsland={1.12f,1.28f,1.43f,1.48f,1.58f}','AttackByIsland={1f,1f,1f,1f,1f}')
# First meteor still does full damage; only repeated hits on one boss are reduced.
edit('NakiPrototype.CombatV3.cs','MeteorBaseDamage*OriginTrainingScale*strength*.8f','MeteorBaseDamage*OriginTrainingScale*strength')
edit('NakiPrototype.EncounterDirector.cs','damage*=hits==0?1:hits==1?.8f:hits==2?.65f:.5f;','damage*=shot.orb&&shot.element==Element.Dark?(hits==0?1:hits==1?.65f:hits==2?.45f:.33f):(hits==0?1:hits==1?.8f:hits==2?.65f:.5f);')
edit('NakiPrototype.EncounterDirector.cs','float factor=shot.hitsLeft>1||shot.element==Element.Wind?.8f:.4f;','float factor=shot.hitsLeft>1||shot.element==Element.Wind?.8f:e.boss?.4f:.65f;')
# Clear pictures and the collision ray share the same muzzle and target centre.
edit('NakiPrototype.OriginCombat.cs','start+=visualOffset;Vector2 end=CoverBeamEnd','start+=visualOffset;if(!solar&&lightBeamTarget!=null&&lightBeamTarget.hp>0)dir=(lightBeamTarget.position+Vector2.up*.3f-start).normalized;Vector2 end=CoverBeamEnd')
edit('NakiPrototype.OriginCombat.cs','DistanceToSegment(e.position,start,end)','DistanceToSegment(e.position+Vector2.up*.3f,start,end)')
edit('NakiPrototype.OriginCombat.cs','!ProjectilePathBlocked(position,e.position,.04f)','!ProjectilePathBlocked(start,e.position+Vector2.up*.3f,.05f)')
edit('NakiPrototype.Elements.cs','StopOriginAction();if(!committed)','if(!lightChannel)StopOriginAction();if(!committed)')
edit('NakiPrototype.BossSignatures.cs','GameObject root,left,right;','GameObject root,left,right,beamEdge,beamTip,beamMuzzle;')
edit('NakiPrototype.BossDuels.cs','if(profile==1||profile==4||','if(profile==4||')
edit('NakiPrototype.BossDuels.cs','?0:profile==11?-1:1)*22','?0:profile==11?-1:1)*14')
edit('NakiPrototype.BossSignatures.cs','UpdateSignatureVisuals(z);if(z.mode==2||z.mode==8)ClipSignatureBeam(z,z.dir,.65f);','if(z.mode==8){Vector2 beamDir=Quaternion.Euler(0,0,Mathf.Max(0,clock-z.active)*z.gap)*z.dir;z.at=z.owner.position+beamDir*(z.owner.radius+.3f);ClipSignatureBeam(z,beamDir,.32f);}UpdateSignatureVisuals(z);if(z.mode==2)ClipSignatureBeam(z,z.dir,.65f);')
start='   ClipSignatureBeam(z,dir,.36f);float length=z.left.transform.localScale.x;'
edit('NakiPrototype.BossDuels.cs',start,'   z.at=z.owner.position+dir*(z.owner.radius+.3f);ClipSignatureBeam(z,dir,.32f);float length=z.left.transform.localScale.x;')
edit('NakiPrototype.BossDuels.cs','beam.sprite=square;beam.color=SpellTint(z.owner.attackElement);','beam.sprite=square;Color tint=SpellTint(z.owner.attackElement);float fade=Mathf.Clamp01((clock-z.active)/.16f)*Mathf.Clamp01((z.end-clock)/.2f);beam.color=new Color(tint.r,tint.g,tint.b,.9f*fade);')
edit('NakiPrototype.BossDuels.cs','new Vector3(length,.36f,1)','new Vector3(length,.32f,1)')
edit('NakiPrototype.BossDuels.cs','   // The emitter starts outside the body: moving in to the flank remains useful for melee.','''   if(z.beamEdge==null){z.beamEdge=Stretch("Beam contrast outline",square,z.at,Vector2.one,C("282032"),619);z.beamTip=Draw("Beam clipped contact",disc,end,Vector2.one*.22f,tint,622);z.beamMuzzle=Draw("Beam visible muzzle",windRingSprite??disc,z.at,Vector2.one*.46f,tint,622);foreach(GameObject part in new[]{z.beamEdge,z.beamTip,z.beamMuzzle})part.transform.SetParent(z.root.transform,true);}
   z.beamEdge.transform.position=z.left.transform.position;z.beamEdge.transform.rotation=z.left.transform.rotation;z.beamEdge.transform.localScale=new Vector3(length,.39f,1);
   z.beamTip.transform.position=end;z.beamMuzzle.transform.position=z.at;
   foreach(GameObject part in new[]{z.beamEdge,z.beamTip,z.beamMuzzle,z.right}){var sr=part.GetComponent<SpriteRenderer>();Color c=sr.color;c.a=length>.01f?fade:0;sr.color=c;}
   // Damage begins after the beam has visibly formed. Cover stops both the picture and hit path.''')
edit('NakiPrototype.BossDuels.cs','if(length>.01f&&clock>=z.nextHit&&DistanceToSegment(position,z.at,end)<.38f','if(length>.01f&&clock>=z.active+.16f&&clock>=z.nextHit&&DistanceToSegment(position,z.at,end)<.38f')
edit('NakiPrototype.BossDuels.cs','z.nextHit=clock+.7f;TakeJourneyDamage(1.25f,z.owner.attackElement)','z.nextHit=clock+.8f;TakeJourneyDamage(.9f/EnemyAttackScale,z.owner.attackElement)')
print('Updated laser clipping, formation feedback, player aim and damage readability')
