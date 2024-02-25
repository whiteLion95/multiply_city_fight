using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Deslab.UI;
using Deslab.Level;

public class AudioManager : AudioManagerCore
{
    #region String constants
    private const string STICKMAN_COLLISION_1 = "StickmanCollision_1";
    private const string STICKMAN_COLLISION_2 = "StickmanCollision_2";
    private const string STICKMAN_COLLISION_3 = "StickmanCollision_3";
    private const string LOSE = "Lose";
    private const string TAP = "Tap";
    private const string WIN = "Win";
    private const string TOWER_CONQUERED = "TowerConquered";
    private const string TILE_ROADED = "TileRoaded";
    private const string TOWER_LOST = "TowerLost";
    private const string STICKMAN_IN_TOWER = "StickmanInTower";
    private const string ROAD_DESTROYED = "RoadDestroyed";
    private const string STICKMAN_MULTIPLIED = "StickmanMultiplied";
    #endregion

    private readonly string[] _stickmanCollisionStrings = { STICKMAN_COLLISION_1, STICKMAN_COLLISION_2, STICKMAN_COLLISION_3 };

    protected override void Awake()
    {
        base.Awake();

        LevelManager.OnLevelStarted += HandlerOnLevelStarted;
    }

    private void OnDisable()
    {
        Unit.OnCollisionWithEnemy -= HandlerOnCollisionWithEnemy;
        LoseScreen.OnLoseImage -= HandlerOnLoseImage;
        WinScreen.OnCongrats -= HandlerOnCongrats;
        Player.OnTap -= HandlerOnTap;
        Tower.OnNewTowerBuilded -= HandlerOnNewTowerBuilded;
        RoadBuilder.OnTileRoaded -= HandlerOnTileRoaded;
        RoadBuilder.OnRoadLinked -= HandlerOnRoadLinked;
        Tower.OnUnitConsumed -= HandlerOnIncomingUnit;
        Tower.OnRoadDestroyed -= HandlerOnRoadDestroyed;
        Multiplier.OnUnitOnMultiplier -= HandlerOnUnitOnMultiplier;
        LevelManager.OnLevelEnded -= HandlerOnLevelEnded;
        LevelManager.OnLevelStarted -= HandlerOnLevelStarted;
    }

    #region EventHandlers
    private void HandlerOnLevelEnded()
    {
        LevelManager.OnLevelEnded -= HandlerOnLevelEnded;
        Unit.OnCollisionWithEnemy -= HandlerOnCollisionWithEnemy;
        Player.OnTap -= HandlerOnTap;
        Tower.OnNewTowerBuilded -= HandlerOnNewTowerBuilded;
        RoadBuilder.OnTileRoaded -= HandlerOnTileRoaded;
        RoadBuilder.OnRoadLinked -= HandlerOnRoadLinked;
        Tower.OnUnitConsumed -= HandlerOnIncomingUnit;
        Tower.OnRoadDestroyed -= HandlerOnRoadDestroyed;
        Multiplier.OnUnitOnMultiplier -= HandlerOnUnitOnMultiplier;
    }

    private void HandlerOnLevelStarted()
    {
        LevelManager.OnLevelEnded += HandlerOnLevelEnded;
        Unit.OnCollisionWithEnemy += HandlerOnCollisionWithEnemy;
        LoseScreen.OnLoseImage += HandlerOnLoseImage;
        WinScreen.OnCongrats += HandlerOnCongrats;
        Player.OnTap += HandlerOnTap;
        Tower.OnNewTowerBuilded += HandlerOnNewTowerBuilded;
        RoadBuilder.OnTileRoaded += HandlerOnTileRoaded;
        RoadBuilder.OnRoadLinked += HandlerOnRoadLinked;
        Tower.OnUnitConsumed += HandlerOnIncomingUnit;
        Tower.OnRoadDestroyed += HandlerOnRoadDestroyed;
        Multiplier.OnUnitOnMultiplier += HandlerOnUnitOnMultiplier;
    }

    private void HandlerOnCollisionWithEnemy(Unit unit, Unit enemyUnit)
    {
        int randIndex = Random.Range(0, _stickmanCollisionStrings.Length);
        PlayOneShot(_stickmanCollisionStrings[randIndex]);
    }

    private void HandlerOnLoseImage()
    {
        PlayOneShot(LOSE);
    }

    private void HandlerOnTap()
    {
        PlayOneShot(TAP);
    }

    private void HandlerOnCongrats()
    {
        PlayOneShot(WIN);
    }

    private void HandlerOnNewTowerBuilded(Tower conqueredTower, Tower newTower, Tower baseTower)
    {
        if (baseTower.MyRoadBuilder is Player)
            PlayOneShot(TOWER_CONQUERED);

        if (conqueredTower.MyRoadBuilder is Player)
            PlayOneShot(TOWER_LOST);
    }

    private void HandlerOnTileRoaded(RoadBuilder rBuilder, Tile tile, RoadBlock rBlock, Tile sourceTile)
    {
        PlayOneShot(TILE_ROADED);
    }

    private void HandlerOnRoadLinked(RoadBuilder rBuilder, Tower linkedTower, Tile sourceTile)
    {
        PlayOneShot(TILE_ROADED);
    }

    private void HandlerOnIncomingUnit(Tower tower, Unit unit)
    {
        PlayOneShot(STICKMAN_IN_TOWER);
    }

    private void HandlerOnRoadDestroyed(Tower tower)
    {
        PlayOneShot(ROAD_DESTROYED);
        HapticsManager.Instance.LightVibrate();
    }

    private void HandlerOnUnitOnMultiplier(Unit unit, Multiplier multiplier)
    {
        PlayOneShot(STICKMAN_MULTIPLIED);
    }
    #endregion
}
