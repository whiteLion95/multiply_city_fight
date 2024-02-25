using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial_2 : Tutorial_1
{
    protected override void Init()
    {
        base.Init();
        Tower.OnNewTowerBuilded += HandlerOnNewTowerBuilded;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        Tower.OnNewTowerBuilded -= HandlerOnNewTowerBuilded;
    }

    protected override void HandlerOnRoadLinked(RoadBuilder rBuilder, Tower linkedTower, Tile sourceTile)
    {
        if (rBuilder is Player)
        {
            if (curStep == 0)
                HideCurrentStep();
            else
            {
                PlayNextStep();
            }
        }
    }

    protected virtual void HandlerOnNewTowerBuilded(Tower conqueredTower, Tower newTower, Tower baseTower)
    {
        if (baseTower.MyRoadBuilder is Player)
        {
            if (curStep == 0)
                PlayNextStep();
        }
    }
}
