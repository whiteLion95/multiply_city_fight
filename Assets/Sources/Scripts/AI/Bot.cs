using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Deslab.Level;

/// <summary>
/// Начинает действовать спустя определённое время после старта уровня либо после появления во время игры +
/// Определить точку назначения: башню врага, нейтральную башню или множитель
/// Начать строить дорогу
/// Процесс постройки дороги:
///     Ищутся самые оптимальные тайлы от текущего
///     Выбирается один из этих тайлов рандомно
///     Если можно за следующее действие дойти до точки назначения, идти до точки назначения
/// </summary>
public class Bot : RoadBuilder
{
    [SerializeField] private BotsDataSO data;

    private PathNode[] _curPath;
    private GameManager _gameManager;
    private Grid _grid;
    private PathFinding _pathFinding;

    protected override void Awake()
    {
        base.Awake();

        _grid = GetComponentInParent<Grid>();
        _pathFinding = new PathFinding(this);

        LevelManager.OnLevelStarted += Init;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        LevelManager.OnLevelStarted -= Init;
        OnRoadLinked -= HandlerOnRoadLinked;
    }

    protected override void Start()
    {
        base.Start();

        _gameManager = GameManager.Instance;

        if (LevelManager.Instance.LevelStarted)
            Invoke(nameof(Init), 0.1f);
    }

    private void Init()
    {
        //If tutorial - start actions after player linked first tower
        if (TutorialLevel.Instance != null)
        {
            OnRoadLinked += HandlerOnRoadLinked;
        }
        else
            StartActions();
    }

    private void HandlerOnRoadLinked(RoadBuilder rB, Tower tower, Tile tile)
    {
        if (rB is Player)
        {
            StartActions();
            OnRoadLinked -= HandlerOnRoadLinked;
        }
    }

    private void StartActions()
    {
        SetCurrentPath();
        StartCoroutine(ActionsRoutine());
    }

    /// <summary>
    /// Start doing actions
    /// </summary>
    private IEnumerator ActionsRoutine()
    {
        yield return new WaitForSeconds(GetDelayWithInaccuracy(data.DelayBeforeActions));

        while (LinkedTower == null)
        {
            DoAction();
            yield return new WaitForSeconds(GetDelayWithInaccuracy(data.DelayBeforeNextAction));
        }
    }

    /// <summary>
    /// Возвращает задержку с погрешностью
    /// </summary>
    private float GetDelayWithInaccuracy(float delay)
    {
        float resultDelay = Random.Range(delay - (data.ActionsInaccuracy * delay), delay + (data.ActionsInaccuracy * delay));
        return resultDelay;
    }

    private int _curNode = 0;
    /// <summary>
    /// Do one action
    /// </summary>
    private void DoAction()
    {
        if (_curPath != null && _curPath.Length > 0)
        {
            if (_curNode < (_curPath.Length - 1))
            {
                _curNode++;
                BuildRoadOnTile(CurrentTile, _curPath[_curNode].MyTile);
            }
            else
            {
                SetCurrentPath();
                _curNode = 0;
                DoAction();
            }
        }
    }

    #region PathFinding
    private void SetCurrentPath()
    {
        List<Tile> targetTiles = GetTargetTiles();

        if (targetTiles != null && targetTiles.Count > 0)
        {
            List<PathNode[]> shortestPaths = GetShortestPaths(targetTiles);

            if (_curPath != null && shortestPaths.Count == 0)
                System.Array.Clear(_curPath, 0, _curPath.Length - 1);
            else if (shortestPaths.Count == 1)
                _curPath = shortestPaths[0];
            else
            {
                List<PathNode[]> highestPriorPaths = GetPathsWithHighestPriority(shortestPaths);

                if (highestPriorPaths != null)
                {
                    if (highestPriorPaths.Count == 1)
                        _curPath = highestPriorPaths[0];
                    else
                        _curPath = GetRandomPath(highestPriorPaths);
                }
            }
        }
    }
    private List<Tile> GetTargetTiles()
    {
        List<Tile> targetTiles = new List<Tile>();
        bool containsEnemyTower = false;

        foreach (Tile tile in _gameManager.TargetableTiles)
        {
            bool validTile = true;

            if (!tile.Equals(BaseTower.MyTile) && !tile.Equals(CurrentTile) && !tile.BaseTowers.Contains(BaseTower))
            {
                if (tile.State == TileState.TileObject && tile.MyTileObject is Tower
                    && (tile.MyTileObject as Tower).FractionData.Fraction == BaseTower.FractionData.Fraction)
                {
                    continue;
                }

                //Avoid tiles with the same fraction
                //foreach (Tower tower in tile.BaseTowers)
                //{
                //    if (tower.FractionData.Fraction == BaseTower.FractionData.Fraction)
                //        validTile = false;
                //}

                if (validTile)
                    targetTiles.Add(tile);

                if (!containsEnemyTower && tile.State == TileState.TileObject && tile.MyTileObject is Tower
                    && (tile.MyTileObject as Tower).FractionData.Fraction != Fraction.Neutral)
                {
                    containsEnemyTower = true;
                }
            }
        }

        if (!containsEnemyTower)
            return null;

        return targetTiles;
    }
    private List<PathNode[]> GetShortestPaths(List<Tile> tiles)
    {
        List<PathNode[]> allPaths = new List<PathNode[]>();
        List<PathNode[]> shortestPaths = new List<PathNode[]>();
        int shortestDistance = 0;

        for (int i = 0; i < tiles.Count; i++)
        {
            List<PathNode> path = _pathFinding.FindPath(CurrentTile.PathNode, tiles[i].PathNode);

            if (path != null)
            {
                allPaths.Add(path.ToArray());

                if (i == 0)
                    shortestDistance = path.Count;
                else if (path.Count < shortestDistance)
                    shortestDistance = path.Count;
            }
        }

        for (int i = 0; i < allPaths.Count; i++)
        {
            if (allPaths[i].Length == shortestDistance)
            {
                shortestPaths.Add(allPaths[i]);
            }
        }

        return shortestPaths;
    }
    private List<PathNode[]> GetPathsWithHighestPriority(List<PathNode[]> paths)
    {
        if (paths.Count > 0)
        {
            List<PathNode[]> highestPriorPaths = new List<PathNode[]>();
            int highestPriority = GetPathPriority(paths[0]);

            //Нахожу высший приоритет
            for (int i = 1; i < paths.Count; i++)
            {
                if (GetPathPriority(paths[i]) < highestPriority)
                    highestPriority = GetPathPriority(paths[i]);
            }

            //Добавляю пути с высшим приоритетом в список
            for (int i = 0; i < paths.Count; i++)
            {
                if (GetPathPriority(paths[i]) == highestPriority)
                    highestPriorPaths.Add(paths[i]);
            }

            return highestPriorPaths;
        }

        return null;
    }
    private int GetPathPriority(PathNode[] path)
    {
        Tile lastTile = GetLastTile(path);

        if (lastTile.State == TileState.TileObject)
            return lastTile.MyTileObject.Priority;

        return -1;
    }
    private Tile GetLastTile(PathNode[] path)
    {
        return path[path.Length - 1].MyTile;
    }
    private PathNode[] GetRandomPath(List<PathNode[]> paths)
    {
        int randIndex = Random.Range(0, paths.Count);
        return paths[randIndex];
    }
    #endregion
}
