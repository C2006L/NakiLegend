using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype {
 public sealed partial class NakiPrototype {
  private readonly Sprite[] islandStairSprites=new Sprite[4];
  private Sprite IslandStairArt(){int island=CurrentIsland.ordinal;if(island==1)return stairsV12??windRingSprite;int index=island==3?1:island==5?2:island==4?3:0;if(islandStairSprites[index]!=null)return islandStairSprites[index];var t=Resources.Load<Texture2D>("NakiArt/IslandTransitions/stairs");if(t==null)return windRingSprite;t.filterMode=FilterMode.Point;var sprite=Sprite.Create(t,new Rect(index*t.width/4f,0,t.width/4f,t.height),Vector2.one*.5f,t.height/2,0,SpriteMeshType.FullRect);ownedAssets.Add(sprite);islandStairSprites[index]=sprite;return sprite;}
  private sealed class WinchTurn {public GameObject root;public float start;}
  private readonly List<WinchTurn> winchTurns=new List<WinchTurn>();
  private void PlayWinchTurn(Vector2 at){var root=new GameObject("Turning winch handwheel");root.transform.SetParent(world,false);root.transform.position=at+Vector2.up*.42f;
   var back=Draw("Winch wheel hub shadow",disc,root.transform.position,Vector2.one*.3f,C("58432f"),DepthAt(at.y)+3);back.transform.SetParent(root.transform,true);
   var rim=Draw("Winch turning brass rim",windRingSprite??disc,root.transform.position,Vector2.one*.42f,C("d3af68"),DepthAt(at.y)+4);rim.transform.SetParent(root.transform,true);
   for(int i=0;i<3;i++){var spoke=Stretch("Moving winch spoke",square,root.transform.position,new Vector2(.32f,.035f),C("c5a668"),DepthAt(at.y)+5);spoke.transform.rotation=Quaternion.Euler(0,0,i*60);spoke.transform.SetParent(root.transform,true);}
   winchTurns.Add(new WinchTurn{root=root,start=Time.unscaledTime});
  }
  private void TickWinchMotions(){TickCoreEvacuation();TickBridgeLowerings();for(int i=winchTurns.Count-1;i>=0;i--){var turn=winchTurns[i];if(turn.root==null){winchTurns.RemoveAt(i);continue;}float phase=Mathf.Clamp01((Time.unscaledTime-turn.start)/.65f);turn.root.transform.rotation=Quaternion.Euler(0,0,Mathf.SmoothStep(0,270,phase));if(phase>=1){SpellSpark((Vector2)turn.root.transform.position,C("b0f6da"),.22f);Destroy(turn.root);winchTurns.RemoveAt(i);}}}
 }
}
