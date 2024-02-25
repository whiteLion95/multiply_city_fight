using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadBlock : MonoBehaviour
{
    public Tower BaseTower { get; set; }
    public Tile MyTile { get; set; }

    private void OnDisable()
    {
        if (MyTile)
            MyTile.RemoveRoadBlock(this);
    }
}
