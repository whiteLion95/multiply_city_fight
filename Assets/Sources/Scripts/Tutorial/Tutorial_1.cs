using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Deslab.Level;

public class Tutorial_1 : Tutorial
{
    protected override void Init()
    {
        base.Init();
        RoadBuilder.OnRoadLinked += HandlerOnRoadLinked;
        LevelManager.OnLevelStarted += HandlerOnLevelStarted;
    }

    protected virtual void OnDisable()
    {
        RoadBuilder.OnRoadLinked -= HandlerOnRoadLinked;
        LevelManager.OnLevelStarted -= HandlerOnLevelStarted;
    }

    protected virtual void HandlerOnRoadLinked(RoadBuilder rBuilder, Tower linkedTower, Tile sourceTile)
    {
        if (rBuilder is Player)
        {
            HideCurrentStep();
            RoadBuilder.OnRoadLinked -= HandlerOnRoadLinked;
        }
    }

    private void HandlerOnLevelStarted()
    {
        PlayNextStep();
    }
}