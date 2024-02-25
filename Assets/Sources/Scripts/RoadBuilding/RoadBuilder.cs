using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using EZ_Pooling;

/// <summary>
/// Абстрактный класс для построения дорог
/// От одной башни можно построить только одну дорогу +
/// Начать строить дорогу можно только с тайла, на котором расположена башня строящего +
/// Продолжить строить дорогу можно только с тайла, на котором дорога остановилась +
/// Строить дорогу можно только по горизонтали и вертикали +
/// Нельзя построить дорогу через препятствия И через тайл, на котором построена эта дорога +
/// Постройка дороги заканчивается либо на нейтральной башне либо на башне противника
/// </summary>
[RequireComponent(typeof(Tower))]
public abstract class RoadBuilder : MonoBehaviour
{
    [SerializeField] private RoadBlock rBlockPrefab;

    public static Action<RoadBuilder, Tile, RoadBlock, Tile> OnTileRoaded;
    public static Action<RoadBuilder, Tower, Tile> OnRoadLinked;
    public static Action<RoadBuilder> OnRoadBuilderCreated;
    public static Action<RoadBuilder> OnRoadBuilderDisabled;

    public Tower BaseTower { get; private set; }
    public Tower LinkedTower { get; set; }
    public Tile CurrentTile { get; private set; }

    private RoadBlock _prevRoadBlock;

    protected virtual void Awake()
    {
        BaseTower = GetComponent<Tower>();
        BaseTower.OnTileSet += (Tile tile) => CurrentTile = tile;
    }

    protected virtual void Start()
    {
        OnRoadBuilderCreated?.Invoke(this);
    }

    protected virtual void OnEnable()
    {
        Tower.OnNewTowerBuilded += HandlerOnNewTowerBuilded;
        Tower.OnRoadDestroyed += HandlerOnRoadDestroyed;
    }

    protected virtual void OnDisable()
    {
        Tower.OnNewTowerBuilded -= HandlerOnNewTowerBuilded;
        Tower.OnRoadDestroyed -= HandlerOnRoadDestroyed;
        OnRoadBuilderDisabled?.Invoke(this);
    }

    /// <summary>
    /// Строит дорогу на одном тайле
    /// </summary>
    /// <param name="targetTile">Тайл для постройки дороги</param>
    protected void BuildRoadOnTile(Tile sourceTile, Tile targetTile)
    {
        if (sourceTile.IsAdjacentTo(targetTile))
        {
            if (targetTile.State == TileState.TileObject)
            {
                if (targetTile.MyTileObject is Obstacle || targetTile.MyTileObject.Equals(BaseTower))
                    return;
                else if (targetTile.MyTileObject is Tower)
                {
                    LinkedTower = targetTile.MyTileObject as Tower;
                    _prevRoadBlock.transform.LookAt(targetTile.transform);
                    OnRoadLinked?.Invoke(this, LinkedTower, sourceTile);
                    CurrentTile = targetTile;
                    return;
                }
            }

            // Не строить дорогу на своей дороге
            if (targetTile.BaseTowers != null && targetTile.BaseTowers.Contains(BaseTower))
            {
                return;
            }

            RoadBlock rBlock = EZ_PoolManager.Spawn(rBlockPrefab.transform, targetTile.transform.position, Quaternion.identity).GetComponent<RoadBlock>();
            rBlock.BaseTower = BaseTower;
            targetTile.SetRoad(rBlock);
            CurrentTile = targetTile;

            if (BaseTower.MyRoad.Count > 0 && _prevRoadBlock)
                _prevRoadBlock.transform.LookAt(CurrentTile.transform);

            OnTileRoaded?.Invoke(this, targetTile, rBlock, sourceTile);
            _prevRoadBlock = rBlock;
        }
    }

    protected virtual void HandlerOnNewTowerBuilded(Tower conqueredTower, Tower newTower, Tower baseTower)
    {
        if (baseTower.Equals(BaseTower))
        {
            LinkedTower = newTower;
        }
        else if (conqueredTower.Equals(LinkedTower))
        {
            LinkedTower = newTower;
        }
    }

    protected void HandlerOnRoadDestroyed(Tower tower)
    {
        if (tower.Equals(BaseTower))
        {
            CurrentTile = BaseTower.MyTile;
        }
    }
}
