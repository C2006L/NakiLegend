using System.IO;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private static readonly string[] CapitalLetterTitles={"原始航海名册 · 被划去的归期","受封契约 · 无法退出的命令","仪式手稿 · 第七束光"};
  private static readonly string[][] CapitalLetterPages={
   new[]{"致航务总署：\n\n返航船除受命的少年外，还有十二名船员、两名医师，以及替失踪者送回遗物的居民。名单中的人都应按原定归期离港；不得以契约尚未结束为由，把家属继续扣在城里。\n\n鸣沙洲的古渠早在征召前便已失修，雷雨高原也有年久损坏的旧设施。海上的灾难并非都出自王宫。但船与粮有限时，为什么修渠、运药的申请总要让给取石的命令？\n\n请在正本上保留这些名字。若少年回来，先让他看见谁仍在等船。\n\n——港务官，原本第十七页","补记：\n\n暮钟岛的粮船曾三次获准出港，又三次被王印附令召回。原名册里的“归期”，在新誊本中一律改成“下一次出征”。失踪船员也没有销去姓名，依然被记作契约担保人。\n\n我比对了历任受封者的委任书。他们的姓名、来处和签字各不相同，最后一行却始终是同一句：仪式未成，征调不停。\n\n港口的欢迎是真的。居民不知道的是，替他们挂起旗帜的手，也在替下一位受命者收紧锁链。\n\n原件留下，誊本照常呈送。愿下一个归来的人有机会把两者放在一起。"},
   new[]{"受封契约·议事厅原正文\n\n碎片分存诸岛，用于维持水路、季候与生息。因紧急需要临时调往王都，须先留下替代设施与撤离通道；事毕归还，不得擅改为长期贡纳。\n\n受命者有权在知晓代价后退出。同行者、家属与尚未出生的人，均不得被列为生命担保。任何更改须公开宣读，由实际承担者重新签认。\n\n王印可调度物资，不可代替居民回答生死之事。\n\n——旧议事厅留存的正文","王印附款，另纸封缄\n\n仪式未成，征召视为延续；受封者及其封地继续供给。归来者不得退出，不得将碎片交还地方。\n\n页末朱批：替代设施未成以前，一片也不能放回。——王印核准。\n\n抄录员的私记：\n\n国王说，曾有一次放开调度，北区随即三日无水。从那以后，撤令草稿每年都写，每年都留在桌上。\n\n他不再相信别人能接住任何失误，于是每一道临时命令都没有了归期。正文没有改，城里遵行的却始终是这张附纸。"},
   new[]{"给最后一位修习者\n\n五种自然碎片与暮钟之石，共成六份外在的力量。右眼中的贤者之石是承载残响的人工容器，并非暮钟核心，不可再算作第七片。\n\n第七束光来自仍能选择的心。六片与这束光共同归位，NakiTong 方能重生。献出心中之光的人不会随神鸟重返人世；仪式不能拿旁人的生命抵付，也不能由国王替本人答应。\n\n若你读到这里，请先把原稿读完。知道力量从何而来，还不等于知道自己愿意为它付出什么。\n\n——未加王印的仪式原稿","原稿末页·两条后路\n\n若保留心中的光，须将六片分还诸岛。没有神鸟替人承担一切，旧泵需要有人修，断粮的航线需要有人重开。先恢复一处，再接通下一处；这些工作不能靠一纸新委任书完成。\n\n若自愿以心补光，重生的是 NakiTong，献光之人不能回来。余下的修复、议事与相互照料，仍由留下的人继续。\n\n手稿旁夹着一张旧船票，日期已经模糊。背面只有一句话：\n\n如果还有机会，愿你能回家。若你不能，也请让那些等船的人回家。"}
  };
  private static readonly string[] CapitalCgFiles={"01-return","02-lockdown","03-heart","04-victory","05-homeward","06-rebirth","07-defeat"};
  private static readonly string[] CapitalCgTitles={"归来者","未准的归期","第七束光","王印崩落","终于归航","让归途继续","未完成的归来"};
  private static readonly string[] CapitalCgText={"海上的灯终于连成归路。少年带着诸岛的记录返航，居民为他挂起灯和旗。他还不知道，自己的归期已被改成了下一次出征。","欢迎的旗还没有收起，港口的锁链已经落下。被扣下的船员、家属和粮车都在等同一张通行令。少年转身走向下城：先把他们的路打开。","三份原件终于对上：六片来自诸岛，第七束光就在自己心中。少年知道了献光的代价，也亲眼见过停下贡纳之后仍在转动的水轮。他把原稿收好，独自走向王座厅。","王印碎裂，国王的命令第一次没有得到回应。居民把撤离记录与原件放在同一张桌上，守军让开门口。少年仍站在台阶下，没有走向空出的王座。","六片分还诸岛以后，修渠的人留在渠边，运粮船重开旧航线。少年没有接下新的委任书。一个普通的傍晚，他背起行囊，搭上了终于准时出港的客船。","NakiTong 重生，少年没有回来。灯匠重新点灯，船员拉起船帆，林海的幼芽长出新叶。他的空斗篷挂在回航船舱里；这一次，没有人的归期被改成下一次出征。","王印的光压住了尚未完成的归途。书房里的原件仍在，泵站与港口的记录也没有丢失。从最近安全点重新整顿，再来完成这段路。"};
  private static readonly string[] CapitalKingLines={"国王：你拆开泵站时，可知道城里多少盏灯靠王印亮着？","少年：我留了供水的支路。旧泵在转，粮车也已经从下城出去了。","国王：今天能转。明天有人违约，海路再断，谁替他们承担？我不能把一座城交给侥幸。","少年：所以，他们连回家也要先等你的仪式完成？","国王：接过王印。等你需要替所有人守住明天，也会明白为什么不能放手。","少年把三份原件收入衣襟，没有走向王座。"};
  private Texture2D CapitalCgArt(int cg){return Resources.Load<Texture2D>("NakiArt/"+(cg>=0&&cg<=5?"CapitalStoryV26/":"CapitalV1/")+CapitalCgFiles[cg]);}
  private bool CapitalTerminal {get{return capitalMode&&chapterEnding&&capitalEnding>0&&capitalCg<0;}}
  private bool CapitalOverlay {get{return capitalMode&&(CapitalTerminal||capitalLetter>=0||capitalConfirm>0||capitalCg>=0&&!StoryActive);}}
  private void QueueCapitalCg(int id)
  {if((capitalSeen&(1<<id))!=0&&id<6)return;capitalCg=id;capitalArmed=false;capitalOpened=Time.unscaledTime;StopOriginAction();CancelBowCharge();BlockWorldInteraction();if(health>0&&id!=0)SaveChapterCheckpoint(false);}
  private bool TickCapitalOverlay()
  {
   if(!capitalMode)return false;
   if(health<=0&&capitalCg<0){capitalCg=6;capitalOpened=Time.unscaledTime;capitalArmed=false;}
   if(!Held('f'))capitalArmed=true;
   if(!CapitalOverlay)return false;
   if(CapitalTerminal)return true;
   bool ready=capitalArmed&&Time.unscaledTime>capitalOpened+.35f;
   if(capitalLetter>=0){
    if(Pressed('e')){CloseCapitalLetter(false);return true;}
    if(ready&&(Pressed('f')||Pressed('d')))AdvanceCapitalLetter();
    if(Pressed('a'))capitalPage=Mathf.Max(0,capitalPage-1);return true;
   }
   if(capitalConfirm>0){if(Pressed('e')){capitalConfirm=0;BlockWorldInteraction();return true;}if(ready&&Pressed('f'))ConfirmCapitalEnding();return true;}
   if(capitalCg>=0){if(ready&&(Pressed('f')||Pressed('e')))CloseCapitalCg();return true;}
   return false;
  }
  private void CloseCapitalLetter(bool complete)
  {if(complete)capitalLetters|=1<<capitalLetter;int next=complete&&capitalLetter<2?capitalLetter+1:-1;capitalLetter=next;capitalPage=0;capitalArmed=false;capitalOpened=Time.unscaledTime;BlockWorldInteraction();BuildChapterActions();UpdateExplorationDoors();RequestCheckpoint();SaveChapterCheckpoint(false);}
  private void AdvanceCapitalLetter()
  {if(capitalLetter<0)return;if(capitalPage+1<CapitalLetterPages[capitalLetter].Length){capitalPage++;capitalArmed=false;capitalOpened=Time.unscaledTime;}else CloseCapitalLetter(true);}
  private void ConfirmCapitalEnding()
  {if(capitalConfirm<1||capitalConfirm>2||capitalEnding!=0||!chapterStone||!journeyRooms[26].cleared)return;capitalEnding=capitalConfirm;capitalConfirm=0;QueueCapitalCg(capitalEnding==1?4:5);}
  private void CloseCapitalCg()
  {int id=capitalCg;if(id<0)return;capitalCg=-1;BlockWorldInteraction();if(id==6){if(!LoadChapterCheckpoint(false))BeginCapital(false);return;}capitalSeen|=1<<id;if(id==4||id==5){chapterEnding=journeyComplete=true;RememberReplayEnding();}BuildChapterActions();UpdateExplorationDoors();RequestCheckpoint();SaveChapterCheckpoint(false);}
  private void DrawCapitalOverlay(float w,float h)
  {
   HudRect(new Rect(0,0,w,h),new Color(.015f,.02f,.035f,.95f));
   if(capitalLetter>=0){
    float bw=Mathf.Min(w-48,870),bh=Mathf.Min(h-40,700),x=(w-bw)*.5f,y=(h-bh)*.5f;HudRect(new Rect(x,y,bw,bh),C("e7d7ad"));HudRect(new Rect(x+10,y+10,bw-20,bh-20),C("f3e7c9"));
    var title=new GUIStyle(journeyTitleStyle){normal={textColor=C("41342b")}};var text=new GUIStyle(journeyTextStyle){normal={textColor=C("3c352d")},wordWrap=true,fontSize=19};
    FittedLabel(new Rect(x+38,y+26,bw-76,48),CapitalLetterTitles[capitalLetter],title);
    Color previousInk=GUI.color;GUI.color=C("3c352d");text.normal.textColor=Color.white;
    FittedLabel(new Rect(x+38,y+88,bw-76,bh-166),CapitalLetterPages[capitalLetter][capitalPage],text,true);
    FittedLabel(new Rect(x+38,y+bh-62,bw-76,40),"A 上一页    F / D "+(capitalPage+1<CapitalLetterPages[capitalLetter].Length?"下一页":"读毕并收录")+"    ESC 暂放    "+(capitalPage+1)+" / "+CapitalLetterPages[capitalLetter].Length,text,true);GUI.color=previousInk;return;
   }
   if(capitalConfirm>0){FittedLabel(new Rect(w*.15f,h*.22f,w*.7f,55),capitalConfirm==1?"保留第七束光，分还六片，踏上归途":"以心补光，让 NakiTong 重生",journeyTitleStyle);
    FittedLabel(new Rect(w*.15f,h*.36f,w*.7f,h*.30f),capitalConfirm==1?"六片将分还诸岛，修渠与运粮由留下的人逐步接续。少年不接任王印，待归路重开，乘普通客船回家。\n\n这会记录为本次王都旅程的结局。":"少年将自愿献出心中的光，不能回来。NakiTong 重生；诸岛居民继续修复水路、通航与共同议事。回航船舱里只留下他的空斗篷。\n\n这会记录为本次王都旅程的结局。",journeyTextStyle,true);
    FittedLabel(new Rect(w*.15f,h*.75f,w*.7f,35),"F 确认选择    ESC 返回 · 可以继续探索",journeyTextStyle);return;}
   int cg=CapitalTerminal?(capitalEnding==1?4:5):Mathf.Clamp(capitalCg,0,6);var art=CapitalCgArt(cg);if(art!=null)GUI.DrawTexture(new Rect(0,0,w,h-175),art,ScaleMode.ScaleToFit);
   HudRect(new Rect(0,h-175,w,175),C("10232c"));FittedLabel(new Rect(32,h-164,w-64,36),CapitalCgTitles[cg],journeyTitleStyle);FittedLabel(new Rect(32,h-123,w-64,76),CapitalCgText[cg],journeyTextStyle,true);FittedLabel(new Rect(32,h-36,w-64,28),CapitalTerminal?"终章完 · F / N 重玩与其他结局 · G 回忆航图":cg==6?"F 返回最近安全点 · R 也可直接重试":"F 继续 · ESC 继续",journeySmallStyle);
   if(CapitalTerminal)DrawReplayEndButton(w,h);
  }
  private void ValidateCapitalCheckpoint(ChapterCheckpoint s)
  {
   if(!capitalMode)return;
   if(s.capitalFlags<0||s.capitalFlags>15||s.capitalLetters<0||s.capitalLetters>7||s.capitalSeen<0||s.capitalSeen>127||s.capitalEnding<0||s.capitalEnding>2||s.capitalCg< -1||s.capitalCg>5)throw new InvalidDataException("Invalid capital story state");
   if(s.rooms[8].cleared&&(s.capitalFlags&1)==0||s.rooms[17].cleared&&(!s.rooms[8].cleared||(s.capitalFlags&6)!=6)||s.rooms[26].cleared&&(!s.rooms[17].cleared||s.capitalLetters!=7||(s.capitalFlags&8)==0)||s.capitalEnding>0&&!s.stone||s.ending&&s.capitalEnding==0||(s.capitalFlags&8)!=0&&s.capitalLetters!=7)throw new InvalidDataException("Incomplete capital investigation");
   if(s.capitalEnding==1&&s.capitalCg==5||s.capitalEnding==2&&s.capitalCg==4||s.ending&&(s.capitalSeen&(1<<(s.capitalEnding==1?4:5)))==0)throw new InvalidDataException("Invalid capital conclusion");
  }
  private void RestoreCapitalCheckpoint(ChapterCheckpoint s)
  {if(!capitalMode)return;capitalFlags=s.capitalFlags;capitalLetters=s.capitalLetters;capitalSeen=s.capitalSeen;capitalEnding=s.capitalEnding;capitalCg=s.capitalCg;capitalLetter=-1;capitalConfirm=0;capitalOpened=Time.unscaledTime;capitalArmed=false;}
 }
}
