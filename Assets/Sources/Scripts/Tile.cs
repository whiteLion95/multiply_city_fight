using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Tile : MonoBehaviour
{
    [SerializeField] private GameObject pointer;
    [SerializeField] private FractionsDataSO fractionsDataSO;
    [SerializeField] private List<Renderer> cubesRenderers;
    [Header("Tutorial dim image")]
    [SerializeField] private Image dimImage;
    [SerializeField] [Range(0, 1)] private float dimTargetTransparency;
    [SerializeField] private float dimFadeDuration;

    private Collider _tileCollider;
    private int _tileLayerMask;
    private List<RoadBlock> _rBlocks;
    private Color _originalColor;
    private List<FractionData> _fractionsDataOnTile;
    private TileState _originalState;

    public TileState State { get; private set; } = TileState.Empty;
    public TileObject MyTileObject { get; private set; }
    /// <summary>
    /// Список башен, которые провели дорогу на этом тайле
    /// </summary>
    public List<Tower> BaseTowers { get; private set; }
    public List<RoadBlock> RoadBlocksOnMe { get => _rBlocks; }
    public PathNode PathNode { get; private set; }
    public List<Unit> UnitsOnTile { get; private set; }

    private void Awake()
    {
        _tileCollider = gameObject.GetComponentInChildren<Collider>();
        _tileLayerMask = 1 << LayerMask.NameToLayer("Tile");
        BaseTowers = new List<Tower>();
        PathNode = GetComponent<PathNode>();
        _fractionsDataOnTile = new List<FractionData>();
        UnitsOnTile = new List<Unit>();
        _originalState = State;

        EnvironmentManager.OnEnvironmentSet += InitMaterial;
        Unit.OnUnitOnTile += HandlerOnUnitOnTile;
        Unit.OnUnitDisabled += RemoveUnit;
        TutorialCore.OnTutorialStepPlay += HandlerOnTutorialStepPlay;
        TutorialCore.OnTutorialCompleted += HideDimImage;
        TutorialCore.OnTutorialStepHide += HideDimImage;
    }

    private void OnDisable()
    {
        EnvironmentManager.OnEnvironmentSet -= InitMaterial;
        Unit.OnUnitOnTile -= HandlerOnUnitOnTile;
        Unit.OnUnitDisabled -= RemoveUnit;
        TutorialCore.OnTutorialStepPlay -= HandlerOnTutorialStepPlay;
        TutorialCore.OnTutorialCompleted -= HideDimImage;
        TutorialCore.OnTutorialStepHide -= HideDimImage;
    }

    private void InitMaterial()
    {
        _originalColor = cubesRenderers[0].material.color;
    }

    private void OnTriggerEnter(Collider other)
    {
        TileObject tileObject = other.GetComponentInParent<TileObject>();

        if (tileObject)
        {
            SetTileObject(tileObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Убрать юнита из списка юнитов на тайле
        if (other.CompareTag("Unit"))
        {
            Unit leftUnit = other.GetComponent<Unit>();
            RemoveUnit(leftUnit);
        }
    }

    private void HandlerOnUnitOnTile(Tile tile, Unit unit)
    {
        if (tile.Equals(this))
        {
            if (!UnitsOnTile.Contains(unit))
            {
                UnitsOnTile.Add(unit);
            }
        }
    }

    private void RemoveUnit(Unit unit)
    {
        if (UnitsOnTile != null && UnitsOnTile.Contains(unit))
        {
            UnitsOnTile.Remove(unit);
        }
    }

    private void SetTileObject(TileObject tileObject)
    {
        State = TileState.TileObject;
        MyTileObject = tileObject;
        tileObject.SetTile(this);
        _originalState = State;

        if (!(tileObject is Obstacle))
            GameManager.Instance.AddTargetableTile(this);
    }

    public void SetRoad(RoadBlock rBlock)
    {
        State = TileState.Roaded;
        rBlock.MyTile = this;

        if (_rBlocks == null)
            _rBlocks = new List<RoadBlock>();

        _rBlocks.Add(rBlock);
        BaseTowers.Add(rBlock.BaseTower);
        RefreshFractionsDataOnTile();
        RefreshColor();

        if (TutorialLevel.Instance != null)
            HideDimImage();
    }

    private void RefreshFractionsDataOnTile()
    {
        _fractionsDataOnTile.Clear();

        foreach (var tower in BaseTowers)
        {
            if (!_fractionsDataOnTile.Contains(tower.FractionData))
            {
                _fractionsDataOnTile.Add(tower.FractionData);
            }
        }

        if (_fractionsDataOnTile.Count > 1)
            _fractionsDataOnTile.Sort(fractionsDataSO.CompareFractionsByPriority);
    }

    private void RefreshColor()
    {
        Multiplier myMultiplier = null;
        if (MyTileObject != null && MyTileObject is Multiplier)
        {
            myMultiplier = (MyTileObject as Multiplier);
        }

        if (_fractionsDataOnTile.Count > 0)
        {
            int divider = Mathf.CeilToInt((float)cubesRenderers.Count / _fractionsDataOnTile.Count);
            int remainder = cubesRenderers.Count % _fractionsDataOnTile.Count;
            int currentCube = 0;

            if (cubesRenderers.Count >= _fractionsDataOnTile.Count)
            {
                for (int i = 0; i < _fractionsDataOnTile.Count; i++)
                {
                    for (int j = currentCube; j < divider; j++)
                    {
                        cubesRenderers[j].material.color = _fractionsDataOnTile[i].RoadColor;
                        currentCube++;
                    }

                    if (remainder == 0)
                        divider += divider;
                    else
                        divider += remainder;
                }
            }
            else
            {
                Debug.LogError("Number of fractions on tile exceeds number of cube renderers on tile");
            }

            if (myMultiplier != null)
                myMultiplier.Discolor();
        }
        else
        {
            foreach (var cubeRenderer in cubesRenderers)
            {
                cubeRenderer.material.color = _originalColor;
            }

            if (myMultiplier != null)
                myMultiplier.ResetColor();
        }
    }

    /// <summary>
    /// Убирает Roadblock и меняет цвет если необходимо
    /// </summary>
    public void RemoveRoadBlock(RoadBlock rBlock)
    {
        if (_rBlocks != null)
        {
            BaseTowers.Remove(rBlock.BaseTower);
            _rBlocks.Remove(rBlock);
            RefreshFractionsDataOnTile();
            RefreshColor();

            if (_rBlocks.Count == 0)
                State = _originalState;

            if (TutorialLevel.Instance != null)
            {
                if (State != TileState.Roaded)
                {
                    ShowDimImage();
                }
            }
        }
    }

    /// <summary>
    /// Возвращает true, если проверяемый тайл находится возле вызывающего этот метод тайла по горизонтали или вертикали
    /// </summary>
    /// <param name="tile">Проверяемый тайл</param>
    /// <returns></returns>
    public bool IsAdjacentTo(Tile tile)
    {
        if ((tile.transform.position.x != transform.position.x) && (tile.transform.position.z != transform.position.z))
        {
            return false;
        }

        if (!IsNearby(tile))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Пуляет лучи в четыре стороны по горизонтали и вертикали и возвращает true, если
    /// хотя бы один из ударов луча пришёлся на указываемый аргумент tile
    /// </summary>
    /// <returns></returns>
    private bool IsNearby(Tile tile)
    {
        RaycastHit hit;
        Vector3 castDir = Vector3.forward;

        // Cast ray to forward, right, back and left (4 directions)
        for (int i = 0; i < 4; i++)
        {
            if (Physics.Raycast(_tileCollider.transform.position, castDir, out hit, 10f, layerMask:_tileLayerMask))
            {
                if (hit.collider.GetComponentInParent<Tile>().Equals(tile))
                    return true;
            }

            castDir = Quaternion.AngleAxis(90f, Vector3.up) * castDir;
        }

        return false;
    }

    private void ShowDimImage()
    {
        dimImage.DOFade(dimTargetTransparency, dimFadeDuration);
    }

    private void HideDimImage()
    {
        dimImage.DOFade(0f, dimFadeDuration);
    }

    private void HandlerOnTutorialStepPlay(int curStep)
    {
        if (TutorialLevel.Instance != null)
        {
            if (!TutorialLevel.Instance.Steps[curStep].AvailableTiles.Contains(this))
            {
                if (State != TileState.Roaded)
                    ShowDimImage();
            }
            else
            {
                HideDimImage();
            }
        }
    }
}

public enum TileState
{
    Empty,
    TileObject,
    Roaded
}
