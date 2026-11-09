using System;
using System.IO;
using UnityEngine;
namespace NakiLegend.Prototype {
 public sealed partial class NakiPrototype {
  [HideInInspector] public string journeyTestProfile="";
  private string JourneyRootDirectory {get{return Application.isBatchMode&&!string.IsNullOrEmpty(chapterBatchSaveDirectory)?chapterBatchSaveDirectory:Path.Combine(Application.persistentDataPath,"NakiLegend");}}
  private string JourneySaveDirectory {get{
   string root=JourneyRootDirectory;
   if(string.IsNullOrEmpty(journeyTestProfile))return root;
   foreach(char c in journeyTestProfile)if(!char.IsLetterOrDigit(c)&&c!='-'&&c!='_')throw new InvalidOperationException("Invalid journey test profile");
   return Path.Combine(root,"playthroughs",journeyTestProfile);
  }}
  private bool FullJourneyTravelAllowed(int island,int floor){
   if(string.IsNullOrEmpty(journeyTestProfile))return true;
   RefreshAtlasProgress();for(int i=0;i<island;i++)if(!atlasFinished[i])return false;
   return floor<=0;
  }
  private int CapitalMoveCycle(Enemy e){int index=Mathf.Max(0,e.attackNumber-1);
   if(e.kind==EnemyKind.CapitalMagistrate)return new[]{0,1,2,3,1,2}[index%6];
   if(e.kind==EnemyKind.CapitalKing)return new[]{0,2,3,1,2,0,3,2}[index%8];
   return index%4;
  }
  private Sprite royalGroundRing;
  private Sprite RoyalGroundRing {
   get{
    if(royalGroundRing!=null)return royalGroundRing;
    var texture=new Texture2D(64,64,TextureFormat.RGBA32,false);texture.filterMode=FilterMode.Point;texture.wrapMode=TextureWrapMode.Clamp;
    for(int y=0;y<64;y++)for(int x=0;x<64;x++){float distance=new Vector2(x-31.5f,y-31.5f).magnitude/31f;texture.SetPixel(x,y,distance<=1&&distance>=.91f?Color.white:Color.clear);}
    texture.Apply();royalGroundRing=Sprite.Create(texture,new Rect(0,0,64,64),Vector2.one*.5f,64,0,SpriteMeshType.FullRect);ownedAssets.Add(texture);ownedAssets.Add(royalGroundRing);return royalGroundRing;
   }
  }
  private void CapitalGroundSeal(Enemy owner,Vector2 desired,float radius,float duration)
  {
   int count=0;foreach(var z in signatureZones)if(z.owner==owner&&z.mode==4)count++;
   if(count>=(owner.kind==EnemyKind.CapitalKing?3:2)||RecoveryWindow(owner))return;
   Vector2 at=desired;bool found=false;
   // Shift a rejected landing inward instead of silently losing the whole move near a doorway or pillar.
   for(int ring=0;ring<=2&&!found;ring++)for(int i=0;i<(ring==0?1:8)&&!found;i++){
    Vector2 candidate=NearestEnemyFooting(desired+(Vector2)(Quaternion.Euler(0,0,i*45)*Vector2.right)*(ring*1.25f),radius*.5f);
    if(Blocked(candidate,radius*.5f)||IntersectsTransit(candidate,Vector2.one*(radius*2)))continue;
    bool overlap=false;foreach(var z in signatureZones)if(z.mode==4&&Vector2.Distance(z.at,candidate)<z.radius+radius+.35f){overlap=true;break;}
    if(!overlap){at=candidate;found=true;}
   }
   if(!found)return;
   int before=signatureZones.Count;AddSignatureZone(owner,4,at,Vector2.zero,radius,duration);
   if(signatureZones.Count>before){var zone=signatureZones[signatureZones.Count-1];zone.root.name="Royal ground seal · "+owner.kind;zone.active=clock+1.1f;zone.end=zone.active+duration;zone.nextHit=zone.active;foreach(var edge in zone.left.GetComponentsInChildren<SpriteRenderer>())if(edge.gameObject!=zone.left)edge.sprite=RoyalGroundRing;}
  }
  private void CapitalControlGround(Enemy owner,int move)
  {
   // Never place ground seals during a laser: the player must retain a readable escape route.
   if(CapitalBeamActive(owner)||move==0||move==2&&owner.kind==EnemyKind.CapitalKing||move==1&&owner.kind==EnemyKind.CapitalMagistrate)return;
   Vector2 aim=owner.attackDirection.sqrMagnitude>.01f?owner.attackDirection:Vector2.down,side=new Vector2(-aim.y,aim.x),center=IntentLead(.25f);
   if(owner.kind==EnemyKind.CapitalCommander){if(move==1||move==2)foreach(int sign in new[]{-1,1})CapitalGroundSeal(owner,center+side*sign*2.5f,1.05f,2.8f);}
   else if(owner.kind==EnemyKind.CapitalMagistrate){foreach(int sign in new[]{-1,1})CapitalGroundSeal(owner,center+side*sign*2.6f,1.15f,3.2f);}
   else{CapitalGroundSeal(owner,center,1.15f,3.2f);CapitalGroundSeal(owner,center+side*2.8f,1.15f,3.2f);if(owner.difficultyPhase>0)CapitalGroundSeal(owner,center-side*2.8f,1.15f,3.2f);}
  }
  private void QueueCapitalTrapPair(Enemy e,bool king){
   Vector2 aim=e.attackDirection.sqrMagnitude>.001f?e.attackDirection:Vector2.down,side=new Vector2(-aim.y,aim.x),center=IntentLead(.25f);
   if(king){CapitalGroundSeal(e,center+side*2.8f,1.05f,2.8f);QueueManorImpact(e,NearestEnemyFooting(center,.4f),1f,1.05f,1.15f,false);QueueManorImpact(e,NearestEnemyFooting(center+side*2.8f,.4f),.9f,1.05f,1.65f,false);}
   else foreach(int sign in new[]{-1,1})QueueManorImpact(e,NearestEnemyFooting(center+side*sign*2.5f,.4f),.85f,1f,sign<0?1.15f:1.6f,false);
  }
 }
}
