using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private enum PowerPool { Light,Elemental,Dark }
  private float elementPower=100,darkPower;
  private sealed class StoredEssence { public Vector2 at;public float value,born; }
  private sealed class EssenceDrop { public StoredEssence data;public GameObject root; }
  private readonly List<EssenceDrop> essenceDrops=new List<EssenceDrop>();
  private Sprite essenceSprite;
  private void ResetPowers(){elementPower=100;darkPower=0;essenceDrops.Clear();}
  private bool SpendPower(PowerPool pool,float cost)
  {
   float available=pool==PowerPool.Light?energy:pool==PowerPool.Elemental?elementPower:darkPower;
   if(available+.001f<cost){JourneyHint(pool==PowerPool.Light?"光之力不足 · 等待内心的光恢复":pool==PowerPool.Dark?"暗之力不足 · 击败有灵生物给贤者之石充能":"元素之力不足 · 拾取结晶或使用元素药剂");return false;}
   if(pool==PowerPool.Light)energy-=cost;else if(pool==PowerPool.Elemental)elementPower-=cost;else darkPower-=cost;return true;
  }
  private void BuildEssenceSprite()
  {
   if(essenceSprite!=null)return;
   essenceSprite=MakeSprite(new[]{"....kk....","...kIIk...","..kIwwIk..",".kITttTIk.","kITttttTIk","kITttttTIk",".kITttTIk.","..kITIk...","...kIk....","....k....."},new Dictionary<char,Color>{{'k',C("24535f")},{'I',C("c1fff0")},{'w',Color.white},{'T',C("53d9ca")},{'t',C("2b9fbc")}},30);
  }
  private void SpawnEssence(Enemy enemy)
  {
   if(!enableDesertJourney)return;BuildEssenceSprite();
   Vector2 at=FindFreeInteractionPoint(enemy.position);
   var data=new StoredEssence{at=at,value=enemy.boss?40:enemy.elite?20:8,born=clock};
   journeyRooms[currentRoom].essence.Add(data);DrawEssence(data);
  }
  private void DrawEssence(StoredEssence data)
  {BuildEssenceSprite();essenceDrops.Add(new EssenceDrop{data=data,root=Draw("Element crystal +"+data.value,essenceSprite,data.at,Vector2.one,Color.white,DepthAt(data.at.y)+1)});}
  private void RestoreEssence(RoomNode node){essenceDrops.Clear();foreach(var data in node.essence)DrawEssence(data);}
  private void TickEssence(float dt)
  {
   for(int i=essenceDrops.Count-1;i>=0;i--)
   {
    EssenceDrop drop=essenceDrops[i];float d=Vector2.Distance(position,drop.data.at);
    if(elementPower<100&&clock>drop.data.born+.12f&&d<1.9f)drop.data.at=Vector2.MoveTowards(drop.data.at,position,dt*5.5f);
    if(elementPower<100&&Vector2.Distance(position,drop.data.at)<.43f)
    {float added=Mathf.Min(100-elementPower,drop.data.value);elementPower+=added;AddFloatingHit("元素 +"+added.ToString("0"),position+Vector2.up*.8f,C("73e6cf"));journeyRooms[currentRoom].essence.Remove(drop.data);Destroy(drop.root);essenceDrops.RemoveAt(i);RequestCheckpoint();}
    else if(drop.root!=null)drop.root.transform.position=drop.data.at+Vector2.up*(.07f+.035f*Mathf.Sin(clock*4+i));
   }
  }
 }
}
