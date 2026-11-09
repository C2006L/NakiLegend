using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype {
 public sealed partial class NakiPrototype {
  private readonly Dictionary<int,Sprite> learningEmblems=new Dictionary<int,Sprite>();
  private void DrawLearningEmblem(Transform parent,Vector2 at,int icon,float size,int order){
   Sprite sprite;if(!learningEmblems.TryGetValue(icon,out sprite)||sprite==null){Texture2D texture=CompactIcon(icon);sprite=Sprite.Create(texture,new Rect(0,0,64,64),Vector2.one*.5f,64,0,SpriteMeshType.FullRect);sprite.name="Learning emblem / "+icon;ownedAssets.Add(sprite);learningEmblems[icon]=sprite;}
   GameObject plate=Stretch("Learning emblem backing",square,at,Vector2.one*(size+.08f),C("283b42"),order);plate.transform.SetParent(parent,true);
   GameObject glyph=Draw("Learning emblem / "+icon,sprite,at,Vector2.one*size,Color.white,order+1);glyph.transform.SetParent(parent,true);
  }
  private int LearningSiteIcon(int kind,string label){
   if(label.Contains("整理元素技能"))return 18;
   if(ThunderStoryRoute)return kind==5?8:kind==6?9:-1;
   if(iceSampleMode&&kind==302)return 10;
   if(fireChapterMode)return kind==404?12:kind==410&&!forgeRescued?13:-1;
   if(woodlandMode)return kind==202?16:kind==205?17:-1;
   return -1;
  }
  private bool BuildLearningSite(int kind,Vector2 at,string label,Sprite original){
   int icon=LearningSiteIcon(kind,label);if(icon<0)return false;
   // A real furnace, NPC or woodland relic keeps its authored body; only its learning glyph is added.
   if(fireChapterMode||woodlandMode||capitalMode||manorCourtyardMode){DrawLearningEmblem(chapterFocus.transform,at+Vector2.up*.95f,icon,.38f,DepthAt(at.y)+25);return false;}
   Sprite art=DesertTeachingSprite;Color stone=iceSampleMode?C("bbdfe6"):C("a3b6c0");
   GameObject stand=Draw("Themed learning stand / "+kind,art,at,Vector2.one*.9f,stone,DepthAt(at.y));stand.transform.SetParent(chapterFocus.transform,true);
   if(iceSampleMode&&kind==302){DrawLearningEmblem(chapterFocus.transform,at+new Vector2(-.19f,.58f),10,.3f,DepthAt(at.y)+2);DrawLearningEmblem(chapterFocus.transform,at+new Vector2(.19f,.58f),11,.3f,DepthAt(at.y)+2);}
   else DrawLearningEmblem(chapterFocus.transform,at+Vector2.up*.58f,icon,.45f,DepthAt(at.y)+2);
   return true;
  }
 }
}
