using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private bool iceEndingCgSeen,interactionNeedsRelease,voyageArmed;
  private float worldInteractionReady,voyageOpened;
  private int pendingIceVoyage=-1;
  private void BlockWorldInteraction(){worldInteractionReady=Time.unscaledTime+.4f;interactionNeedsRelease=true;}
  private void TickInteractionRelease(){if(!Held('f'))interactionNeedsRelease=false;}
  private bool WorldInteractionReady {get{return !interactionNeedsRelease&&Time.unscaledTime>=worldInteractionReady;}}
  private void RequestIceVoyage(int action)
  {
   if(!iceSampleMode||!WorldInteractionReady||!SafeCheckpointRoom||chapterCg||StoryActive)return;
   if(action!=304&&action!=340)return;
   if(action==340&&(!iceFullChapter||!chapterEnding||!iceEndingCgSeen))return;
   pendingIceVoyage=action;voyageArmed=false;voyageOpened=Time.unscaledTime;CancelBowCharge();
  }
  private bool HandleIceVoyage()
  {
   if(pendingIceVoyage<0)return false;
   if(!Held('f'))voyageArmed=true;
   if(Pressed('e'))CancelIceVoyage();
   else if(voyageArmed&&Pressed('f'))ConfirmIceVoyage();
   return true;
  }
  private void CancelIceVoyage(){pendingIceVoyage=-1;BlockWorldInteraction();}
  private void ConfirmIceVoyage()
  {
   if(pendingIceVoyage<0||Time.unscaledTime<voyageOpened+.25f)return;
   int action=pendingIceVoyage;pendingIceVoyage=-1;BlockWorldInteraction();
   if(action==304)ReturnFromIce();else if(action==340)EmbarkFireCoast();else if(action==507)EmbarkForestFromFire();else if(action==509)ReturnWoodlandToFire();else if(action==724)EmbarkManorReturn();
  }
  private void DrawIceVoyage(float w,float h)
  {
   HudRect(new Rect(0,0,w,h),new Color(.025f,.05f,.07f,.9f));
   Rect box=new Rect(w/2-370,h/2-160,740,320);HudRect(box,C("132630"));
   CenterLabel(new Rect(box.x+25,box.y+25,690,45),VoyageTitle,journeyTitleStyle);
   FittedLabel(new Rect(box.x+35,box.y+90,670,90),VoyageDescription,journeyTextStyle,true);
   if(FlatButton(new Rect(box.x+35,box.y+215,315,60),VoyageStay))CancelIceVoyage();
   if(FlatButton(new Rect(box.x+390,box.y+215,315,60),pendingIceVoyage==304||pendingIceVoyage==509||pendingIceVoyage==724?"确认返航 · F":"确认启程 · F"))ConfirmIceVoyage();
  }
 }
}
