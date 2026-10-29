namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private enum IslandId { Desert,Thunder,Ice,Fire,Woodland,Manor }
  private sealed class IslandProfile
  {
   public readonly IslandId id;public readonly int ordinal,shieldBonus;public readonly string name,saveKey;public readonly float skillScale;public readonly bool available;
   public IslandProfile(IslandId id,int ordinal,string name,string saveKey,int shieldBonus,float skillScale,bool available){this.id=id;this.ordinal=ordinal;this.name=name;this.saveKey=saveKey;this.shieldBonus=shieldBonus;this.skillScale=skillScale;this.available=available;}
  }
  private static readonly IslandProfile[] IslandProfiles={
   new IslandProfile(IslandId.Desert,1,"沙漠岛","journey-v1.json",0,1,true),
   new IslandProfile(IslandId.Thunder,2,"雷雨高原","thunder-chapter-v1.json",1,1.15f,true),
   new IslandProfile(IslandId.Ice,3,"冰山岛","ice-sample-v1.json",1,1.22f,true),
   new IslandProfile(IslandId.Fire,4,"烈焰岛","fire-chapter-v1.json",2,1.30f,false),
   new IslandProfile(IslandId.Woodland,5,"青栖林海","woodland-chapter-v1.json",2,1.38f,true),
   new IslandProfile(IslandId.Manor,6,"大骑士的府邸","manor-chapter-v1.json",3,1.45f,false)};
  // Legacy scene flags remain an adapter so existing scenes and saves keep opening.
  private IslandProfile CurrentIsland {get{return IslandProfiles[(int)(iceSampleMode?IslandId.Ice:woodlandMode?IslandId.Woodland:ThunderActive?IslandId.Thunder:IslandId.Desert)];}}
 }
}
