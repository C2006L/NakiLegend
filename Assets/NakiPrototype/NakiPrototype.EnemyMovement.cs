using UnityEngine;
namespace NakiLegend.Prototype
{
    public sealed partial class NakiPrototype
    {
        private Vector2 NearestEnemyFooting(Vector2 desired,float radius)
        {
            Rect b=CurrentRoomBounds;float edge=radius+.12f;
            Vector2 at=new Vector2(Mathf.Clamp(desired.x,b.xMin+edge,b.xMax-edge),Mathf.Clamp(desired.y,b.yMin+edge,b.yMax-edge));
            if(!Blocked(at,radius))return at;
            Vector2 best=at;float score=float.MaxValue;
            for(float y=b.yMin+edge;y<=b.yMax-edge;y+=.4f)for(float x=b.xMin+edge;x<=b.xMax-edge;x+=.4f)
            {Vector2 p=new Vector2(x,y);float d=(p-desired).sqrMagnitude;if(d<score&&!Blocked(p,radius)){best=p;score=d;}}
            return best;
        }
        private Vector2 MoveEnemyWithinRoom(Vector2 from,Vector2 delta,float radius)
        {
            Vector2 at=NearestEnemyFooting(from,radius);Rect b=CurrentRoomBounds;float edge=radius+.12f;
            int steps=Mathf.Max(1,Mathf.CeilToInt(delta.magnitude/.12f));Vector2 step=delta/steps;
            for(int i=0;i<steps;i++)
            {
                Vector2 p=new Vector2(Mathf.Clamp(at.x+step.x,b.xMin+edge,b.xMax-edge),at.y);
                if(!Blocked(p,radius))at.x=p.x;
                p=new Vector2(at.x,Mathf.Clamp(at.y+step.y,b.yMin+edge,b.yMax-edge));
                if(!Blocked(p,radius))at.y=p.y;
            }
            return at;
        }
    }
}
