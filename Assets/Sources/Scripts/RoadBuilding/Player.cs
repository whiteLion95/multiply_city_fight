using UnityEngine;
using Lean.Touch;
using System;
using Deslab.Level;
using System.Collections.Generic;
using System.Linq;

public class Player : RoadBuilder
{
    [SerializeField] private InputDataSO inputData;
    [SerializeField] private LayerMask myLayerMask;

    public static Action OnTap;
    public static Action<RoadBlock, Tower> OnRoadCut;

    private Camera _mainCamera;
    private bool _fingerDown;
    private LeanFinger _finger;
    private bool _fingerOnTower;

    protected override void Awake()
    {
        base.Awake();
        _mainCamera = Camera.main;
    }

    protected override void Start()
    {
        base.Start();
    }

    private void FixedUpdate()
    {
        if (_fingerDown)
        {
            RayCast(_finger.ScreenPosition, OnHitAction);
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        LevelManager.OnLevelStarted += OnLevelStartedHandler;
        LevelManager.OnLevelEnded += OnLevelEndedHandler;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        LevelManager.OnLevelStarted -= OnLevelStartedHandler;
        LevelManager.OnLevelEnded -= OnLevelEndedHandler;
        LeanTouch.OnFingerDown -= HandlerOnFingerDown;
        LeanTouch.OnFingerUp -= HandlerOnFingerUp;
        LeanTouch.OnFingerUpdate -= HandlerOnFigerUpdate;
    }

    private void OnLevelStartedHandler()
    {
        LeanTouch.OnFingerDown += HandlerOnFingerDown;
        LeanTouch.OnFingerUp += HandlerOnFingerUp;
        LeanTouch.OnFingerUpdate += HandlerOnFigerUpdate;
    }

    private void OnLevelEndedHandler()
    {
        LeanTouch.OnFingerDown -= HandlerOnFingerDown;
        LeanTouch.OnFingerUp -= HandlerOnFingerUp;
        LeanTouch.OnFingerUpdate -= HandlerOnFigerUpdate;
    }

    private void HandlerOnFingerDown(LeanFinger finger)
    {
        if (!finger.IsOverGui)
        {
            _fingerDown = true;
            _finger = finger;

            RayCast(finger.ScreenPosition, OnFirstHitAction);
        }

        OnTap?.Invoke();
    }

    private void HandlerOnFingerUp(LeanFinger finger)
    {
        _fingerDown = false;
        _fingerOnTower = false;
        _firstHittedTile = null;
    }

    private float _timeOnTower = 0f;
    private void HandlerOnFigerUpdate(LeanFinger finger)
    {
        if (_fingerOnTower)
        {
            _timeOnTower += Time.deltaTime;

            if (_timeOnTower >= inputData.DeleteTowerDuration)
            {
                BaseTower.DestroyMyPath();
                _fingerOnTower = false;
                _timeOnTower = 0f;
            }
        }
    }

    private void RayCast(Vector2 screenPos, Action<RaycastHit> onRayHit = null)
    {
        RaycastHit hit;
        Ray ray = _mainCamera.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, myLayerMask))
        {
            onRayHit?.Invoke(hit);
        }
    }

    private void OnHitAction(RaycastHit hit)
    {
        Tile hittedTile = hit.collider.GetComponentInParent<Tile>();
        Tower hittedTower = hit.collider.GetComponentInParent<Tower>();

        if (_firstHittedTile != null)
        {
            Tile targetTile = null;

            if (hittedTile != null && hittedTile.IsAdjacentTo(_firstHittedTile))
            {
                if (!TutorialCheck(hittedTile))
                {
                    return;
                }

                if (!hittedTile.Equals(_firstHittedTile))
                {
                    if (hittedTile.Equals(CurrentTile))
                        _firstHittedTile = CurrentTile;
                    else
                        targetTile = hittedTile;
                }

                if (_fingerOnTower)
                {
                    if (!(hittedTile.State == TileState.TileObject && hittedTile.MyTileObject.Equals(BaseTower)))
                    {
                        _fingerOnTower = false;
                        _timeOnTower = 0f;
                    }
                }
            }

            if (hittedTower != null && hittedTower.MyTile.IsAdjacentTo(_firstHittedTile))
            {
                if (!TutorialCheck(hittedTower.MyTile))
                {
                    return;
                }

                if (!hittedTower.Equals(BaseTower) && LinkedTower == null)
                {
                    targetTile = hittedTower.MyTile;
                }
                else if (hittedTower.FractionData.Fraction == Fraction.Player)
                {
                    (hittedTower.MyRoadBuilder as Player).FirstHittedTile = hittedTower.MyTile;
                }
            }

            if (targetTile != null)
            {
                if (targetTile.RoadBlocksOnMe != null && targetTile.RoadBlocksOnMe.Count > 0)
                {
                    if (targetTile.RoadBlocksOnMe.Any((x) => x.BaseTower.Equals(BaseTower)))
                        return;
                }
                
                if (_firstHittedTile.BaseTowers.Contains(BaseTower) || _firstHittedTile.Equals(BaseTower.MyTile))
                {
                    if (!_firstHittedTile.Equals(CurrentTile))
                    {
                        if (!_firstHittedTile.Equals(BaseTower.MyTile))
                        {
                            // Удалить все блоки дороги после блока, которого коснулся игрок первым
                            RoadBlock touchedRoadBlock = _firstHittedTile.RoadBlocksOnMe.Find((x) => x.BaseTower.Equals(BaseTower));
                            BaseTower.DestroyMyPath(touchedRoadBlock);
                            // И повернуть блок к новому блоку
                            touchedRoadBlock.transform.LookAt(targetTile.transform);
                            BuildRoadOnTile(_firstHittedTile, targetTile);
                            OnRoadCut?.Invoke(touchedRoadBlock, BaseTower);
                        }
                        else
                        {
                            BaseTower.DestroyMyPath();
                            BuildRoadOnTile(_firstHittedTile, targetTile);
                        }
                    }
                    else
                    {
                        BuildRoadOnTile(_firstHittedTile, targetTile);
                    }
                }
            }
        }
    }

    private Tile _firstHittedTile;
    public Tile FirstHittedTile { get => _firstHittedTile; set => _firstHittedTile = value; }
    private void OnFirstHitAction(RaycastHit hit)
    {
        Tile hittedTile = hit.collider.GetComponentInParent<Tile>();
        Player hittedPlayer = hit.collider.GetComponentInParent<Player>();

        if (hittedTile)
        {
            if (hittedTile.State == TileState.TileObject && hittedTile.MyTileObject.Equals(BaseTower)
                && BaseTower.MyRoad.Count > 0)
            {
                _fingerOnTower = true;
            }

            _firstHittedTile = hittedTile;

            if (_fingerOnTower)
            {
                if (!TutorialCheck(hittedTile))
                {
                    _fingerOnTower = false;
                }
            }
        }

        if (hittedPlayer)
        {
            if (hittedPlayer.Equals(this))
            {
                _firstHittedTile = hittedPlayer.BaseTower.MyTile;
                _fingerOnTower = true;
            }

            if (_fingerOnTower)
            {
                if (!TutorialCheck(hittedPlayer.BaseTower.MyTile))
                {
                    _fingerOnTower = false;
                }
            }
        }
    }

    protected override void HandlerOnNewTowerBuilded(Tower conqueredTower, Tower newTower, Tower baseTower)
    {
        base.HandlerOnNewTowerBuilded(conqueredTower, newTower, baseTower);
        LeanTouch.OnFingerDown += HandlerOnFingerDown;
        LeanTouch.OnFingerUp += HandlerOnFingerUp;
        LeanTouch.OnFingerUpdate += HandlerOnFigerUpdate;
    }

    /// <summary>
    /// On tutorial levels interact only with available tiles
    /// </summary>
    private bool TutorialCheck(Tile hittedTile)
    {
        if (TutorialLevel.Instance != null && TutorialCore.Instance != null)
        {
            TutorialLevel tutorialLevel = TutorialLevel.Instance;
            TutorialCore tutorial = TutorialCore.Instance;

            if (!tutorial.Completed)
            {
                if (tutorial.CurStep >= 0)
                {
                    List<Tile> availableTiles = tutorialLevel.Steps[tutorial.CurStep].AvailableTiles;

                    if (!availableTiles.Contains(hittedTile))
                        return false;
                    else
                    {
                        // remove previous available tile from available tiles
                        int tileIndex = availableTiles.IndexOf(hittedTile);

                        if (tileIndex == 1)
                        {
                            availableTiles.RemoveAt(tileIndex - 1);
                        }
                    }
                }
            }
        }

        return true;
    }
}
