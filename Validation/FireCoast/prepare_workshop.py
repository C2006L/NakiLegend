from pathlib import Path
import shutil
root=Path(__file__).resolve().parents[2]
generated=Path('C:/Users/15853/.codex/generated_images/01a1764d-2b16-7840-a400-e14f3ffe5ffd')
dest=root/'Assets/NakiPrototype/Resources/NakiArt/FireWorkshop'
dest.mkdir(parents=True,exist_ok=True)
for name,source in [('creatures','exec-e9daa1bc-d5dc-47af-a302-e852c2571083.png'),('environment','exec-1ab3a0b7-80b8-45ce-b329-9625450c39f8.png')]:shutil.copy2(generated/source,dest/(name+'.png'))
p=root/'Assets/NakiPrototype/NakiPrototype.FireWorkshop.cs'
s=p.read_text(encoding='utf-8').replace('if(index==0)r=new Rect(w*.08f,h*1.08f,w*.84f,h*.84f);','if(index==0)r=new Rect(t.width*120f/1536,t.height*(1024-507f)/1024,t.width*568f/1536,t.height*498f/1024);')
a='for(int i=0;i<4;i++){var r=new Rect(i%2*t.width/2f,(1-i/2)*t.height/2f,t.width/2f,t.height/2f);'
b='Rect[] regions={new Rect(40,40,675,600),new Rect(720,150,490,500),new Rect(35,650,560,550),new Rect(600,755,650,450)};for(int i=0;i<4;i++){Rect r=regions[i];r=new Rect(r.x*t.width/1254,(1254-r.yMax)*t.height/1254,r.width*t.width/1254,r.height*t.height/1254);'
assert a in s
p.write_text(s.replace(a,b),encoding='utf-8')
