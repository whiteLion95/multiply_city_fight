using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesManager : ParticlesManagerCore
{
    protected override void Awake()
    {
        base.Awake();

        RoadBuilder.OnTileRoaded += HandlerOnTileRoaded;
        Tower.OnNewTowerBuilded += HandlerOnNewTowerBuilded;
        RoadBuilder.OnRoadLinked += HandlerOnRoadLinked;
    }

    private void OnDisable()
    {
        RoadBuilder.OnTileRoaded -= HandlerOnTileRoaded;
        Tower.OnNewTowerBuilded -= HandlerOnNewTowerBuilded;
        RoadBuilder.OnRoadLinked -= HandlerOnRoadLinked;
    }

    private void HandlerOnTileRoaded(RoadBuilder rBuilder, Tile tile, RoadBlock rBlock, Tile sourceTile)
    {
        PlayParticle(Particles.RoadBuilded, tile.transform.position, Quaternion.identity, rBuilder.BaseTower.FractionData.RoadColor);
    }

    private void HandlerOnNewTowerBuilded(Tower conqueredTower, Tower newTower, Tower baseTower)
    {
        PlayParticle(Particles.TowerConquered, newTower.transform.position, Quaternion.identity, baseTower.FractionData.TowerColor);
    }

    private void HandlerOnRoadLinked(RoadBuilder rBuilder, Tower linkedTower, Tile sourceTile)
    {
        PlayParticle(Particles.TowerLinked, linkedTower.transform.position, Quaternion.identity, rBuilder.BaseTower.FractionData.RoadColor);
    }
}
