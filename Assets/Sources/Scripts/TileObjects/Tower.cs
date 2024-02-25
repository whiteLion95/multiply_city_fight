using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Deslab.Level;
using EZ_Pooling;
using DG.Tweening;

public class Tower : TileObject
{
    [Header("Фракция")]
    [SerializeField] private FractionsDataSO fractionsData;
    [SerializeField] private Fraction fraction;
    [Header("Генерация и спавн юнитов")]
    [SerializeField] private TowerDataSO data;
    [SerializeField] private UnitDataSO unitsData;
    [SerializeField] [Tooltip("Коэффициент, на который умножается базовая длительность спавна")] [Range(1f, 10f)] private float spawnDurationKoeff = 1f;
    [Tooltip("Начальное кол-во юнитов")] [SerializeField] private int basicUnitsCount;
    [SerializeField] private Unit unitPrefab;
    [SerializeField] private Transform spawnPoint;
    [Tooltip("Двухсторонний диапазон рандомной координаты x для спавна. 0 - центр тайла, 0.5 - край")]
    [SerializeField] [Range(0f, 0.5f)] private float spawnPosRange;
    [SerializeField] private GameObject maxUnitsTextPrefabFlash;
    //[SerializeField] private GameObject maxUnitsTextPrefabStatic;

    public Action<Tower, int, bool> OnUnitsCountChanged;
    public static Action<Tower, Tower, Tower> OnNewTowerBuilded;
    public static Action<Tower> OnRoadDestroyed;
    public static Action<Tower, Unit> OnUnitConsumed;
    public static Action<Unit> OnUnitSpawned;
    public static Action<Unit> OnUnitDeSpawned;

    private List<RoadBlock> _myRoad;
    private RoadBuilder _myRoadBuilder;
    private List<Unit> _myUnits;
    private GameManager _gameManager;
    private DOTweenAnimation _tween;
    
    public FractionData FractionData { get; private set; }
    [field: SerializeField] public int UnitsCount { get; private set; }
    public int MaxUnitsCount { get => data.MaxUnitsCount; }
    public List<RoadBlock> MyRoad { get => _myRoad; }
    public RoadBuilder MyRoadBuilder { get => _myRoadBuilder; }
    public float SpawnPosRange { get => spawnPosRange; }
    public List<Unit> MyUnits { get => _myUnits; }

    private void Awake()
    {
        FractionData = fractionsData.GetFractionData(fraction);
        _myRoad = new List<RoadBlock>();
        _myUnits = new List<Unit>();
        UnitsCount = basicUnitsCount;
        _myRoadBuilder = GetComponent<RoadBuilder>();
        _tween = GetComponentInChildren<DOTweenAnimation>();

        if (basicUnitsCount > MaxUnitsCount)
            basicUnitsCount = MaxUnitsCount;
    }

    private void Start()
    {
        _gameManager = GameManager.Instance;
    }

    private void OnEnable()
    {
        RoadBuilder.OnTileRoaded += HandlerOnTileRoaded;
        LevelManager.OnLevelStarted += HandlerOnLevelStarted;
        Multiplier.OnUnitMultiplied += HandlerOnUnitMultiplied;
    }

    private void OnDisable()
    {
        RoadBuilder.OnTileRoaded -= HandlerOnTileRoaded;
        LevelManager.OnLevelStarted -= HandlerOnLevelStarted;
        Multiplier.OnUnitMultiplied -= HandlerOnUnitMultiplied;
    }

    public void Init()
    {
        StartCoroutine(GenerateUnits());
    }

    #region EventHandlers
    private void HandlerOnLevelStarted()
    {
        if (MyRoadBuilder is Bot)
        {
            if (StaticManager.levelID > botsData.LastEasyLevel)
            {
                spawnDurationKoeff = botsData.SpawnHardKoeff;
            }
        }

        if (fraction != Fraction.Neutral)
            StartCoroutine(GenerateUnits());
    }

    private void HandlerOnTileRoaded(RoadBuilder roadBuilder, Tile tile, RoadBlock rBlock, Tile sourceTile)
    {
        if (roadBuilder.Equals(_myRoadBuilder))
        {
            _myRoad.Add(rBlock);

            if (_myRoad.Count == 1)
            {
                StartCoroutine(SpawnUnits());
            }

            _maxUnitsTextWasShown = false;
        }
    }

    private void HandlerOnUnitInOtherTower(Unit unit, Tower otherTower)
    {
        otherTower.HandleIncomingUnit(unit, this);
        unit.OnOtherTower -= HandlerOnUnitInOtherTower;
    }

    private void HandlerOnUnitDie(Unit unit)
    {
        DespawnUnit(unit);
    }

    private void HandlerOnUnitMultiplied(Unit sourceUnit, Unit newUnit)
    {
        if (_myUnits.Contains(sourceUnit))
        {
            _myUnits.Add(newUnit);

            newUnit.OnOtherTower += HandlerOnUnitInOtherTower;
            newUnit.OnDead += HandlerOnUnitDie;
            newUnit.OnDestroyedRoad += HandlerOnUnitOnDestroyedRoad;
        }
    }

    private void HandlerOnUnitOnDestroyedRoad(Unit unit)
    {
        DespawnUnit(unit);

        if (MyTile.UnitsOnTile.Count < unitsData.MaxUnitsInCrowd)
        {
            SpawnUnit(true);
        }
    }
    #endregion

    #region Coroutines
    private IEnumerator GenerateUnits()
    {
        while (_myRoad.Count == 0)
        {
            yield return new WaitForSeconds(data.GenerationTime);
            GenerateUnit();
        }
    }

    private IEnumerator SpawnUnits()
    {
        spawnPoint.LookAt(_myRoad[0].transform.position);

        while (_myRoad.Count > 0)
        {
            yield return new WaitForSecondsRealtime(data.SpawnTime * spawnDurationKoeff);
            if (_myRoad.Count > 0)
            {
                SpawnUnit();
            }
        }
    }
    #endregion

    private void GenerateUnit()
    {
        if (UnitsCount < data.MaxUnitsCount)
        {
            OnUnitsCountChanged?.Invoke(this, ++UnitsCount, true);
        }
    }

    private void SpawnUnit(bool randZ = false)
    {
        if (_gameManager.GetFractionUnitsOnLevel(this).Count < _gameManager.UnitsPerFraction)
        {
            float xOffset;
            Vector3 spawnPos = GetRandSpawnPos(out xOffset, randZ);
            Unit spawnedUnit = EZ_PoolManager.Spawn(unitPrefab.transform, spawnPos, spawnPoint.rotation).GetComponent<Unit>();
            _myUnits.Add(spawnedUnit);
            spawnedUnit.OnSpawn(this, xOffset);
            Unit.OnUnitOnTile?.Invoke(MyTile, spawnedUnit);
            OnUnitSpawned?.Invoke(spawnedUnit);

            spawnedUnit.OnOtherTower += HandlerOnUnitInOtherTower;
            spawnedUnit.OnDead += HandlerOnUnitDie;
            spawnedUnit.OnDestroyedRoad += HandlerOnUnitOnDestroyedRoad;
        }
    }

    private Vector3 GetRandSpawnPos(out float xOffset, bool randZ)
    {
        xOffset = UnityEngine.Random.Range(-spawnPosRange, spawnPosRange);
        float zOffset = 0f;

        if (randZ)
            zOffset = UnityEngine.Random.Range(-spawnPosRange, spawnPosRange);

        Vector3 spawnPos = spawnPoint.TransformPoint(new Vector3(xOffset, 0f, zOffset));
        return spawnPos;
    }

    private void TakeDamage(Unit attackingUnit)
    {
        if (UnitsCount > 0)
        {
            OnUnitsCountChanged?.Invoke(this, --UnitsCount, false);

            if (UnitsCount == 0)
            {
                attackingUnit.MyTower.ConquerTower(this);
            }
        }
    }

    public void HandleIncomingUnit(Unit unit, Tower sourceTower)
    {
        OnUnitConsumed?.Invoke(this, unit);
        _tween.DORestart();

        if (unit.MyTower.FractionData.Fraction == fraction)
        {
            if (UnitsCount < data.MaxUnitsCount)
            {
                GenerateUnit();
            }
            else if (MyRoad != null && MyRoad.Count > 0)
            {
                TransferUnitFromSourceTower(unit, sourceTower);
                return;
            }
        }
        else
        {
            TakeDamage(unit);
        }

        sourceTower.DespawnUnit(unit);
    }

    private void TransferUnitFromSourceTower(Unit unit, Tower sourceTower)
    {
        sourceTower.MyUnits.Remove(unit);
        this.MyUnits.Add(unit);

        unit.KeepGoingInTower(this, spawnPoint);

        unit.OnOtherTower -= sourceTower.HandlerOnUnitInOtherTower;
        unit.OnDead -= sourceTower.HandlerOnUnitDie;
        unit.OnDestroyedRoad -= sourceTower.HandlerOnUnitOnDestroyedRoad;

        unit.OnOtherTower += HandlerOnUnitInOtherTower;
        unit.OnDead += HandlerOnUnitDie;
        unit.OnDestroyedRoad += HandlerOnUnitOnDestroyedRoad;
    }

    /// <summary>
    /// Если у завоёванной башни есть дорога, 
    ///     уничтожить её
    /// Поставить на место завоёванной башни копию башни завоевателя
    /// Уничтожить завоёванную башню
    /// </summary>
    /// <param name="tower"></param>
    public void ConquerTower(Tower tower)
    {
        if (tower.MyRoad.Count > 0)
            tower.DestroyMyPath();

        Tower newTower = Instantiate(gameObject, tower.transform.position, tower.transform.rotation, transform.parent).GetComponent<Tower>();

        OnNewTowerBuilded?.Invoke(tower, newTower, this);

        Destroy(tower.gameObject);
        newTower.Init();
    }

    public void DestroyMyPath(RoadBlock from = null, bool included = false)
    {
        if (_myRoad.Count > 0)
        {
            int fromIndex;

            if (from == null)
                fromIndex = 0;
            else if (_myRoad.Contains(from))
            {
                fromIndex = _myRoad.IndexOf(from);

                if (!included)
                {
                    fromIndex++;

                    if (fromIndex == _myRoad.Count)
                    {
                        MyRoadBuilder.LinkedTower = null;
                        return;
                    }
                }
            } 
            else
                return;

            if (MyRoadBuilder.LinkedTower)
                MyRoadBuilder.LinkedTower = null;

            for (int i = fromIndex ; i < _myRoad.Count; i++)
            {
                EZ_PoolManager.Despawn(_myRoad[i].transform);
            }

            _myRoad.RemoveRange(fromIndex, _myRoad.Count - fromIndex);

            if (fromIndex == 0)
            {
                for (int i = 0; i < _myUnits.Count; i++)
                {
                    EZ_PoolManager.Despawn(_myUnits[i].transform);
                }

                _myUnits.Clear();
                StopAllCoroutines();
                StartCoroutine(GenerateUnits());
            }

            _maxUnitsTextWasShown = false;
            OnRoadDestroyed?.Invoke(this);
        }
    }

    private void DespawnUnit(Unit unit)
    {
        _myUnits.Remove(unit);
        EZ_PoolManager.Despawn(unit.transform);

        unit.OnOtherTower -= HandlerOnUnitInOtherTower;
        unit.OnDead -= HandlerOnUnitDie;
        unit.OnDestroyedRoad -= HandlerOnUnitOnDestroyedRoad;

        OnUnitDeSpawned?.Invoke(unit);
    }

    private bool _maxUnitsTextWasShown = false;
    public void HandleMaxUnitsOnTile(Tile tile, Unit unit, bool despawnUnit = true)
    {
        if (_myUnits.Contains(unit))
        {
            if (despawnUnit)
                DespawnUnit(unit);

            if (MyRoadBuilder is Player && !_maxUnitsTextWasShown)
            {
                EZ_PoolManager.Spawn(maxUnitsTextPrefabFlash.transform, tile.transform.position, Quaternion.identity);
                _maxUnitsTextWasShown = true;
            }
        }
    }
}
