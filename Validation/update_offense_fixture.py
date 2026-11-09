from pathlib import Path
r=Path('D:/NakiLegend/Repository');p=r/'Validation/prepare_boss_offense.py';s=p.read_text(encoding='utf-8-sig').replace("p.with_suffix('.cs.v9-disabled')","p.with_suffix('.cs.v9-final-disabled')")
insert='''s=s.replace('Call("TickBossBreathing",boss);','Call("TickBossBreathing",boss,.01f);')
s=s.replace('Check((bool)Get("BossResting"),"Boss enters output opening island "+island);','''+repr('''Check((bool)Get("BossResting"),"Boss enters output opening island "+island);
 Vector2 prior=(Vector2)Get(boss,"position");Call("TickBossBreathing",boss,.4f);Check(Vector2.Distance(prior,(Vector2)Get(boss,"position"))>.05f,"Resting movable boss repositions without attacking");
 Check((int)Get("additionalCoverCount")==1,"Boss room gains one cover with reserved walking clearance");''')+''')
s=s.replace('float speed=(float)Get("HostileProjectileSpeedScale");','float speed=(float)Get("HostileProjectileSpeedScale");if(island==5)Check(speed<.7f,"Captain projectiles separately slowed below previous boss arena speed");')
'''
s=s.replace("(o/'Smoke.cs').write_text(s,encoding='utf-8');qa=",insert+"(o/'Smoke.cs').write_text(s,encoding='utf-8');qa=");p.write_text(s,encoding='utf-8')
