using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private int manorLetters,manorLetter=-1,manorLetterPage;
  private bool manorVictorySeen,manorVictoryCg;
  private static readonly string[] ManorLetterTitles={"未寄出的家书","被退回的求援信","王印原令与最后的手记"};
  private static readonly string[][] ManorLetterPages={
   new[]{"母亲：\n\n船终于靠岸了。他们在港口叫我勇者，给我披了一条很重的绶带。我一路想了许多话，真到那时候，却只想问回家的船还要等几天。\n\n鸣沙洲的水已经重新流进村里。雷雨高原的灯匠替我补过灯，霜潮群岛的船员说，等春天冰薄了，可以顺路送我一程。赤炉的工匠托我带种子给北方的人。我都记着。", "国王赐给我一座府邸，说最后一片源石还不能离开这里。村里缺粮，我先把自己的口粮分了出去。这里的人并不认识我，可他们愿意替我留一盏灯。\n\n等事情做完，我就回去。院子不要急着种满，给我留一小块。\n\n——你的孩子\n\n信封没有地址。封口的蜡始终没有融过。"},
   new[]{"致府邸的主人：\n\n送药的船到了，药却被扣在转运间。守粮人说，车上的封条不能拆，工坊的批令还没下来。屋里咳嗽的人已经不愿再问日期了。\n\n我们知道你也被困在这里。你刚来时，亲手把粮袋放在门前；后来巡卫不许我们再敲府邸的门。孩子们仍问，那个带灯的少年什么时候回来。", "这封信第三次被送回。前两次说印章不齐，这一次只有四个字：不得离庭。\n\n我们不要封赏，也不求谁替我们打仗。只想让运粮的人走过街口，让病人先用上已经送到的药。倘若你还能看见这封信，开一次门就好。\n\n——旧村医师与守粮人\n\n页脚有另一人的批注：‘我听见了。可这副甲，已经不肯让我走向门外。’"},
   new[]{"王印原令·密存副本\n\n受封者须镇守元素源石，不得擅离。其右眼所藏之石为承载残响的人工容器；真正源石封于誓甲之内。契约不随肉身终止，须由继任者承接，或剥离三处王印方可解除。\n\n战斗所得余烬可供术式运转，不等同于完整意志。不得以累积余烬之数，代替承接契约所需的生命。", "最后的手记\n\n我以为再强一点，就不会有人挨饿、有人等不到药。我把所有请求都背在身上，直到有一天，竟开始问他们，谁更值得被救。\n\n倘若后来的人看见这些字，请先去村里。别急着接过我的剑，也别相信下一封委任书已经替你选好了路。\n\n若你能把光落在王印上，就试着拆开它。若你眼中也有那块石头，请记住：甲会倒下，命令却会等着穿上下一具身体。\n\n——我曾经也只是想回家。"}
  };
  private void BuildManorStudy()
  {
   if(currentRoom!=32)return;
   var t=Resources.Load<Texture2D>("NakiArt/ManorVillageV4/bookcase");
   if(t!=null){var sprite=Sprite.Create(t,new Rect(0,0,t.width,t.height),new Vector2(.5f,.1f),t.width/3.1f,0,SpriteMeshType.FullRect);ownedAssets.Add(sprite);for(int i=0;i<3;i++){Vector2 at=new Vector2(-4+i*4,2.7f);Draw("Manor study bookcase",sprite,at,Vector2.one,Color.white,DepthAt(at.y)).transform.SetParent(chapterFocus.transform,true);TrackCampCollision(at+Vector2.up*.2f,new Vector2(2.3f,.45f),true);}}
   for(int i=0;i<3;i++)AddChapterAction(730+i,new Vector2(-4+i*4,.6f),((manorLetters&(1<<i))!=0?"重读 · ":"阅读 · ")+ManorLetterTitles[i],ManorVillageProp(4,1.8f));
  }
  private void FinishManorLetterPage()
  {
   if(manorLetterPage<ManorLetterPages[manorLetter].Length-1){manorLetterPage++;return;}
   manorLetters|=1<<manorLetter;manorLetter=-1;BuildChapterActions();UpdateExplorationDoors();UpdateFlowGuide();RequestCheckpoint();SaveChapterCheckpoint(false);BlockWorldInteraction();
  }
  private bool TickManorReading()
  {
   if(!manorCourtyardMode)return false;
   if(manorVictoryCg){if(Pressed('f')||Pressed('e')){manorVictoryCg=false;manorVictorySeen=true;BuildChapterActions();RequestCheckpoint();SaveChapterCheckpoint(false);BlockWorldInteraction();}return true;}
   if(manorLetter<0)return false;
   if(Pressed('e')){manorLetter=-1;BlockWorldInteraction();return true;}
   if(Pressed('a'))manorLetterPage=Mathf.Max(0,manorLetterPage-1);
   if(Pressed('f')||Pressed('d'))FinishManorLetterPage();return true;
  }
  private void DrawManorReading(float w,float h)
  {
   if(manorVictoryCg){HudRect(new Rect(0,0,w,h),Color.black);var art=Resources.Load<Texture2D>("NakiArt/ManorVillageV4/knight-defeated");if(art!=null)GUI.DrawTexture(new Rect(0,0,w,h-100),art,ScaleMode.ScaleToFit);FittedLabel(new Rect(35,h-92,w-70,42),"誓甲已破，命令未熄",journeyTitleStyle);FittedLabel(new Rect(35,h-48,w-70,40),"F 继续 · 回到内庭，查看仍在发亮的王印",journeyTextStyle);return;}
   HudRect(new Rect(0,0,w,h),new Color(.02f,.025f,.04f,.94f));Rect paper=new Rect(w*.15f,h*.07f,w*.7f,h*.86f);HudRect(paper,C("eadbb8"));HudRect(new Rect(paper.x+8,paper.y+8,paper.width-16,3),C("927553"));
   var title=new GUIStyle(journeyTitleStyle);title.normal.textColor=C("483729");var body=new GUIStyle(journeyTextStyle){wordWrap=true,fontSize=Mathf.Clamp((int)(h/38),18,26)};body.normal.textColor=C("3f352a");
   FittedLabel(new Rect(paper.x+40,paper.y+28,paper.width-80,48),ManorLetterTitles[manorLetter],title);
   Color ink=GUI.color;GUI.color=C("3f352a");body.normal.textColor=Color.white;GUI.Label(new Rect(paper.x+40,paper.y+90,paper.width-80,paper.height-180),ManorLetterPages[manorLetter][manorLetterPage],body);GUI.color=ink;body.normal.textColor=C("3f352a");
   body.fontSize=18;GUI.color=C("3f352a");body.normal.textColor=Color.white;GUI.Label(new Rect(paper.x+40,paper.yMax-68,paper.width-80,52),"A 上页 · F / D "+(manorLetterPage==1?"读毕收起":"下一页")+" · ESC 暂时收起（未读完不记为已读）    "+(manorLetterPage+1)+" / 2",body);GUI.color=ink;
  }
 }
}
