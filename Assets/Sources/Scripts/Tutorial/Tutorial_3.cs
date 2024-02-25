using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial_3 : Tutorial
{
    protected override void Init()
    {
        base.Init();
        Tower.OnNewTowerBuilded += HandlerOnNewTowerBuilded;
    }

    private void OnDisable()
    {
        Tower.OnNewTowerBuilded -= HandlerOnNewTowerBuilded;
        Tower.OnRoadDestroyed -= HandlerOnRoadDestroyed;
    }

    protected virtual void HandlerOnNewTowerBuilded(Tower conqueredTower, Tower newTower, Tower baseTower)
    {
        if (baseTower.MyRoadBuilder is Player)
        {
            PlayNextStep();

            Tower.OnNewTowerBuilded -= HandlerOnNewTowerBuilded;
            Tower.OnRoadDestroyed += HandlerOnRoadDestroyed;
        }
    }

    private void HandlerOnRoadDestroyed(Tower tower)
    {
        if (tower.MyRoadBuilder is Player)
        {
            PlayNextStep();
            Tower.OnRoadDestroyed -= HandlerOnRoadDestroyed;
        }
    }
}
