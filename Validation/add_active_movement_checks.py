from pathlib import Path
r=Path('D:/NakiLegend/Repository');p=r/'Validation/prepare_boss_offense.py';s=p.read_text(encoding='utf-8-sig').replace("p.with_suffix('.cs.v9-movement-disabled')","p.with_suffix('.cs.v9-verified-disabled')")
needle="(o/'Smoke.cs').write_text(s,encoding='utf-8');qa="
addition='''s=s.replace('Quiet();object boss=List("enemies").Cast<object>().First(e=>(bool)Get(e,"boss"));', '''+repr('''Quiet();object boss=List("enemies").Cast<object>().First(e=>(bool)Get(e,"boss"));
 Set(boss,"nextAttack",F("clock")+100);Set(boss,"recoveryUntil",F("clock")+100);Set(boss,"windup",0f);Vector2 activeBefore=(Vector2)Get(boss,"position");Call("UpdateTacticalEnemy",boss,.2f);Check(Vector2.Distance(activeBefore,(Vector2)Get(boss,"position"))>.05f,"Active boss repositions during short weapon recovery instead of standing still island "+island);
 if(island<5){Set(boss,"recoveryUntil",0f);Call("TickBossSignature",boss,.01f);IDictionary signatures=(IDictionary)Get("bossSignatures");object sig=signatures[boss];Check((float)Get(sig,"next")-F("clock")<2.2f,"Identity control skill starts well before breathing window");}
''')+''')
'''
s=s.replace(needle,addition+needle);p.write_text(s,encoding='utf-8')
