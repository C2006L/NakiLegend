using UnityEngine;
namespace NakiLegend.Prototype {
 public sealed partial class NakiPrototype {
  private float damageFeedbackUntil,damageFeedbackDuck=1;
  private static bool IsPlayerDamageCue(string key){return key=="shield"||key=="hurt"||key=="break";}
  private void PlayPlayerDamageFeedback(float shieldBefore,float absorbed,float lostHealth){
   if(absorbed>.001f)Sfx(shieldBefore>.001f&&shield<=.001f?"break":"shield",true,shield<=.001f?1:.85f);
   // Overflow damage must report actual injury as well as the shield breaking.
   if(lostHealth>.001f)Sfx("hurt",true,1);
  }
 }
}
