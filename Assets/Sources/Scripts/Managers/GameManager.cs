using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Deslab.Level;
using TMPro;
using DG.Tweening;

public class GameManager : Singleton<GameManager>
{
    [System.Serializable]
    private class FractionOnLevel
    {
        public FractionOnLevel(FractionData data, List<Tower> towers, List<Unit> units)
        {
            this.data = data;
            this.towers = towers;
            this.units = units;
        }

        public FractionData data;
        public List<Tower> towers;
        public List<Unit> units; 
    }

    [SerializeField] private MultiSlider multiSlider;
    [SerializeField] private List<FractionOnLevel> _fractionsOnLevel;
    [SerializeField] private TMP_Text maxUnitsLabel;
    [SerializeField] private UnitDataSO unitsData;

    public List<RoadBuilder> PlayerRoadBuilders { get; private set; }
    public List<RoadBuilder> BotsRoadBuilders { get; private set; }
    public List<Tile> TargetableTiles { get; private set; }
    public int UnitsPerFraction 
    { 
        get
        {
            return unitsData.MaxUnitsOnLevel / _fractionsOnLevel.Count;
        } 
    }

    protected override void Awake()
    {
        base.Awake();

        PlayerRoadBuilders = new List<RoadBuilder>();
        BotsRoadBuilders = new List<RoadBuilder>();
        TargetableTiles = new List<Tile>();
        _fractionsOnLevel = new List<FractionOnLevel>();

        DOTween.SetTweensCapacity(1000, 500);

        LevelManager.OnLevelEnded += HandlerOnLevelEnded;
        LevelManager.OnLevelLoaded += HandlerOnLevelLoaded;
        RoadBuilder.OnRoadBuilderCreated += HandlerOnRoadbuilderCreated;
        RoadBuilder.OnRoadBuilderDisabled += HandlerOnRoadBuilderDisabled;
        LevelManager.OnNewLevel += HandlerOnNewLevel;
        Tower.OnUnitSpawned += HandlerOnUnitSpawned;
        Tower.OnUnitDeSpawned += HandlerOnUnitDespawned;
        Multiplier.OnUnitMultiplied += HandlerOnUnitMultiplied;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (var fraction in _fractionsOnLevel)
            {
                foreach (var tower in fraction.towers)
                {
                    foreach (var unit in tower.MyUnits)
                    {
                        unit.StopMoving();
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            foreach (var fraction in _fractionsOnLevel)
            {
                foreach (var tower in fraction.towers)
                {
                    foreach (var unit in tower.MyUnits)
                    {
                        unit.KeepMoving();
                    }
                }
            }
        }
    }

    private void OnDisable()
    {
        LevelManager.OnLevelEnded -= HandlerOnLevelEnded;
        LevelManager.OnLevelLoaded -= HandlerOnLevelLoaded;
        RoadBuilder.OnRoadBuilderCreated -= HandlerOnRoadbuilderCreated;
        RoadBuilder.OnRoadBuilderDisabled -= HandlerOnRoadBuilderDisabled;
        LevelManager.OnNewLevel -= HandlerOnNewLevel;
        Tower.OnUnitSpawned -= HandlerOnUnitSpawned;
        Tower.OnUnitDeSpawned -= HandlerOnUnitDespawned;
        Multiplier.OnUnitMultiplied -= HandlerOnUnitMultiplied;
    }

    private void ResetValues()
    {
        PlayerRoadBuilders.Clear();
        BotsRoadBuilders.Clear();
        TargetableTiles.Clear();
        _fractionsOnLevel = new List<FractionOnLevel>();
    }

    public void AddTargetableTile(Tile targetTile)
    {
        if (!TargetableTiles.Contains(targetTile))
        {
            TargetableTiles.Add(targetTile);
        }
    }

    private void AddTowerToFractionsOnLevel(Tower tower)
    {
        FractionOnLevel curFractionOnLevel = GetFractionOnLevel(tower);

        if (curFractionOnLevel == null)
        {
            curFractionOnLevel = new FractionOnLevel(tower.FractionData, new List<Tower>(), new List<Unit>());

            if (tower.MyRoadBuilder is Player)
                _fractionsOnLevel.Insert(0, curFractionOnLevel);
            else
                _fractionsOnLevel.Add(curFractionOnLevel);
        }

        if (curFractionOnLevel != null)
        {
            curFractionOnLevel.towers.Add(tower);
        }

        tower.OnUnitsCountChanged += HandlerOnTowerCountChanged;
    }

    private FractionOnLevel GetFractionOnLevel(Tower tower)
    {
        return _fractionsOnLevel.Find((x) => x.data.Fraction == tower.FractionData.Fraction);
    }

    private int GetTotalHealthInFraction(FractionOnLevel fractionOnLevel)
    {
        int totalHealth = 0;

        foreach (Tower tower in fractionOnLevel.towers)
        {
            totalHealth += tower.UnitsCount;
        }

        return totalHealth;
    }

    private int GetTotalHealthOnLevel()
    {
        int totalHealth = 0;

        foreach (FractionOnLevel fractionOnLevel in _fractionsOnLevel)
        {
            totalHealth += GetTotalHealthInFraction(fractionOnLevel);
        }

        return totalHealth;
    }

    private void AddUnitToFractionOnLevel(Unit unit)
    {
        FractionOnLevel curFractionOnLevel = GetFractionOnLevel(unit.MyTower);
        curFractionOnLevel.units.Add(unit);
    }

    private void RemoveUnitFromFractionOnLevel(Unit unit)
    {
        FractionOnLevel curFractionOnLevel = GetFractionOnLevel(unit.MyTower);

        if (curFractionOnLevel.units.Contains(unit))
            curFractionOnLevel.units.Remove(unit);
    }

    /// <summary>
    /// Returns total units of specified tower's fraction on current level
    /// </summary>
    public List<Unit> GetFractionUnitsOnLevel(Tower tower)
    {
        FractionOnLevel curFractionOnLevel = GetFractionOnLevel(tower);
        return curFractionOnLevel.units;
    }

    private void InitMultiSlider()
    {
        multiSlider.Init(_fractionsOnLevel.Count);

        for (int i = 0; i < _fractionsOnLevel.Count; i++)
        {
            multiSlider.SetBlockColor(multiSlider.Blocks[i], _fractionsOnLevel[i].data.TowerColor);
        }

        if (GetTotalHealthOnLevel() > 0)
        {
            foreach (FractionOnLevel fractionOnLevel in _fractionsOnLevel)
            {
                ChangeMultiSlider(fractionOnLevel);
            }
        }
    }

    private void StopRefreshingMultiSlider()
    {
        foreach (FractionOnLevel fractionOnLevel in _fractionsOnLevel)
        {
            foreach (Tower tower in fractionOnLevel.towers)
            {
                tower.OnUnitsCountChanged -= HandlerOnTowerCountChanged;
            }
        }
    }

    private IEnumerator ChangeMultiSlider(FractionOnLevel fractionOnLevel)
    {
        yield return new WaitForSeconds(0.01f);
        float fractionPct = (float)GetTotalHealthInFraction(fractionOnLevel) / GetTotalHealthOnLevel();
        int blockIndex = _fractionsOnLevel.IndexOf(fractionOnLevel);
        multiSlider.RefreshSlider(blockIndex, fractionPct);
    }

    private void DestroyAllUnits()
    {
        Unit[] allUnits = FindObjectsOfType<Unit>();

        if (allUnits.Length > 0)
        {
            foreach (Unit unit in allUnits)
            {
                Destroy(unit.gameObject);
            }
        }
    }

    #region EventHandlers
    private void HandlerOnLevelLoaded(Level level)
    {
        Invoke(nameof(InitMultiSlider), 0.1f);
        Invoke(nameof(DestroyAllUnits), 0.1f);
    }

    private void HandlerOnLevelEnded()
    {
        StopRefreshingMultiSlider();
    }

    private void HandlerOnNewLevel()
    {
        ResetValues();
    }

    private void HandlerOnRoadbuilderCreated(RoadBuilder rBuilder)
    {
        if (rBuilder is Player)
            PlayerRoadBuilders.Add(rBuilder as Player);
        else
            BotsRoadBuilders.Add(rBuilder as Bot);

        AddTowerToFractionsOnLevel(rBuilder.BaseTower);
    }

    private void HandlerOnRoadBuilderDisabled(RoadBuilder rBuilder)
    {
        if (rBuilder is Player)
        {
            PlayerRoadBuilders.Remove(rBuilder as Player);

            if (PlayerRoadBuilders.Count == 0)
                StaticManager.OnLose?.Invoke();
        }
        else
        {
            BotsRoadBuilders.Remove(rBuilder as Bot);

            if (BotsRoadBuilders.Count == 0)
                StaticManager.OnWin?.Invoke();
        }
    }

    private void HandlerOnTowerCountChanged(Tower tower, int value, bool increment)
    {
        FractionOnLevel curFractionOnLevel = GetFractionOnLevel(tower);
        StartCoroutine(ChangeMultiSlider(curFractionOnLevel));
    }

    private void HandlerOnUnitSpawned(Unit unit)
    {
        AddUnitToFractionOnLevel(unit);
    }

    private void HandlerOnUnitMultiplied(Unit sourceUnit, Unit newUnit)
    {
        AddUnitToFractionOnLevel(newUnit);
    }

    private void HandlerOnUnitDespawned(Unit unit)
    {
        RemoveUnitFromFractionOnLevel(unit);
    }
    #endregion
}
