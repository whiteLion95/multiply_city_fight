using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Arystan.Math;
using System;
using Deslab.Level;

[Serializable]
public class Unit : MonoBehaviour
{
    [SerializeField] private UnitDataSO data;
    [SerializeField] private Animator anim;
    [SerializeField] private Transform[] scalingTransforms;

    private RoadBlock _curRoadBlock;
    private RoadBlock _nextRoadBlock;
    private Vector3 _currentDirection;
    private Vector3 _targetPoint;
    private bool _leftLastRoadBlock;
    private Transform _targetObject;
    private MovingState _movingState;
    private Unit _enemyUnit;

    #region Animation
    private const string IS_MOVING_BOOL = "IsMoving";
    private const string DIE_TRIGGER = "Die";
    #endregion

    public Action OnPointReached;
    public Action OnObjectReached;
    public Action<Unit, Tower> OnOtherTower;
    public Action<Unit> OnDead;
    public Action<Unit> OnDestroyedRoad;
    public Action<Unit> OnMaxUnits;
    public Action<RoadBlock> OnWaitingRoadBlock;
    public static Action<Unit> OnUnitDisabled;
    public static Action<Tile, Unit> OnUnitOnTile;
    public static Action<Unit, Unit> OnCollisionWithEnemy;
    
    public Tower MyTower { get; set; }
    public float XOffset { get; set; }
    public bool CanBeMultiplied { get; set; }
    public RoadBlock CurRoadBlock { get => _curRoadBlock; }
    public RoadBlock NextRoadBlock { get => _nextRoadBlock; }
    public Vector3 CurrentDirection { get => _currentDirection; }
    public Vector3 TargetPoint { get => _targetPoint; }
    public Fraction Fraction { get => MyTower.FractionData.Fraction; }
    public bool IsDead { get; private set; } = false;
    [field:SerializeField] public bool IsFighting { get; private set; } = false;
    public Unit EnemyUnit { get => _enemyUnit; }

    // Update is called once per frame
    void Update()
    {
        Move();
        LookToMovementDirection();
    }

    private void OnEnable()
    {
        _currentDirection = transform.forward;
        SetMovingByDirection(_currentDirection);
        OnWaitingRoadBlock += HandlerOnWaitingRoadBlock;
        OnObjectReached += HandlerOnObjectReached;
        Player.OnRoadCut += HandlerOnRoadCut;
        LevelManager.OnLevelLoaded += HandlerOnLevelLoaded;
    }

    private void OnDisable()
    {
        ResetValues();
        RoadBuilder.OnTileRoaded -= HandlerOnTileRoaded;
        RoadBuilder.OnRoadLinked -= HandlerOnRoadLinked;
        OnUnitDisabled?.Invoke(this);
        Player.OnRoadCut -= HandlerOnRoadCut;
        LevelManager.OnLevelLoaded -= HandlerOnLevelLoaded;
    }

    public void OnSpawn(Tower myTower, float xOffset)
    {
        MyTower = myTower;
        XOffset = xOffset;
        _nextRoadBlock = MyTower.MyRoad[0];
        CanBeMultiplied = true;
    }

    private void ResetValues()
    {
        _nextRoadBlock = null;
        CanBeMultiplied = false;
        _leftLastRoadBlock = false;
        _contributed = false;
        _reachedRandPoint = false;
        IsDead = false;
        IsFighting = false;
        _targetObject = null;
        _onLastTile = false;
    }

    public void ResetScale()
    {
        foreach (Transform transform in scalingTransforms)
        {
            transform.localScale = Vector3.one;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("RoadBlock"))
        {
            RoadBlock rBlock = other.GetComponentInParent<RoadBlock>();
            int rBlockIndex = MyTower.MyRoad.IndexOf(rBlock);

            // Если это не следующий блок, ничего не делать
            if (!rBlock.Equals(_nextRoadBlock) && CanBeMultiplied)
                return;

            // Для клонов
            if (!CanBeMultiplied && !rBlock.Equals(_curRoadBlock))
                return;

            if (rBlockIndex == (MyTower.MyRoad.Count - 1))
            {
                _nextRoadBlock = null;
            }
            else
            {
                _nextRoadBlock = MyTower.MyRoad[rBlockIndex + 1];
            }

            _curRoadBlock = rBlock;

            if (rBlock.BaseTower.Equals(MyTower))
            {
                float delayBeforeActions = 0f;

                if (rBlock.MyTile.MyTileObject is Multiplier)
                {
                    if (!ReachedMaxUnitsOnTile(rBlock.MyTile))
                    {
                        if (CanBeMultiplied)
                        {
                            Multiplier multiplier = rBlock.MyTile.MyTileObject as Multiplier;
                            multiplier.MultiplyUnit(this);
                        }
                        else
                        {
                            delayBeforeActions = 0.1f;
                        }
                    }
                    else
                    {
                        Vector3 middlePoint;
                        if (PointFound(out middlePoint, transform.position, rBlock.transform.position))
                        {
                            SetMovingToPoint(middlePoint, () => MyTower.HandleMaxUnitsOnTile(rBlock.MyTile, this));
                        }
                    }
                }
                
                StartCoroutine(ActionsOnRoadBlockAfterDelay(rBlock, delayBeforeActions));
            }
        }
    }

    private bool _contributed;
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Tower") && _leftLastRoadBlock)
        {
            Tower triggeredTower = other.GetComponent<Tower>();

            if (!_contributed)
                StopMoving();

            if (triggeredTower.Equals(MyTower.MyRoadBuilder.LinkedTower))
            {
                _leftLastRoadBlock = false;
                OnOtherTower?.Invoke(this, MyTower.MyRoadBuilder.LinkedTower);
                _contributed = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("RoadBlock"))
        {
            RoadBlock leftRoadBlock = other.GetComponentInParent<RoadBlock>();

            // если покинутый блок является последним блоком на дороге башни юнита
            // и у башни есть связанная башня
            if (leftRoadBlock.Equals(MyTower.MyRoad[MyTower.MyRoad.Count - 1]))
            {
                _leftLastRoadBlock = true;
            }
        }
    }

    private IEnumerator ActionsOnRoadBlockAfterDelay(RoadBlock rBlock, float delay)
    {
        yield return new WaitForSeconds(delay);
        CanBeMultiplied = true;
        ActionsOnRoadBlock(rBlock);
    }

    private void ActionsOnRoadBlock(RoadBlock rBlock)
    {
        int roadBlockIndex = MyTower.MyRoad.IndexOf(rBlock);

        // Если на конце дороги и нет соединённой башни, то начать кучковаться на тайле
        if (roadBlockIndex == (MyTower.MyRoad.Count - 1) && MyTower.MyRoadBuilder.LinkedTower == null)
        {
            OnWaitingRoadBlock?.Invoke(rBlock);
        }
        else // Иначе двигаться в текущем направлении либо до точки поворота
        {
            KeepGoing(_curRoadBlock.transform.forward, _curRoadBlock.transform);
        }

        //Искать врага на тайле и идти к нему, если был найден
        Tile curTile = _curRoadBlock.MyTile;
        OnUnitOnTile?.Invoke(curTile, this);
        _enemyUnit = SearchForClosestEnemyUnit(curTile.UnitsOnTile);
        Fight(_enemyUnit);
    }

    #region EventHandlers
    private void HandlerOnLevelLoaded(Level level)
    {
        if (gameObject.activeSelf)
            Destroy(gameObject);
    }

    private void HandlerOnObjectReached()
    {
        if (_targetObject != null && _targetObject.CompareTag("Unit"))
        {
            Unit targetUnit = _targetObject.GetComponent<Unit>();

            if (targetUnit.Fraction != this.Fraction)
            {
                Die();

                if (MyTower.MyRoadBuilder is Player)
                {
                    OnCollisionWithEnemy?.Invoke(this, targetUnit);
                }
            }

            OnObjectReached -= HandlerOnObjectReached;
        }
    }

    private void HandlerOnRoadCut(RoadBlock cutBlock, Tower baseTower)
    {
        if (baseTower.Equals(MyTower))
        {
            if (cutBlock != null)
            {
                if (cutBlock.Equals(_curRoadBlock))
                {
                    KeepGoing(cutBlock.transform.forward, cutBlock.transform);
                    int cutBlockIndex = baseTower.MyRoad.IndexOf(cutBlock);

                    if (cutBlockIndex < (baseTower.MyRoad.Count - 1))
                        _nextRoadBlock = baseTower.MyRoad[cutBlockIndex + 1];
                    else
                        _nextRoadBlock = null;
                }
                else if (!MyTower.MyRoad.Contains(_curRoadBlock) || MyTower.MyRoad.IndexOf(_curRoadBlock) > MyTower.MyRoad.IndexOf(cutBlock))
                {
                    OnDestroyedRoad?.Invoke(this);
                }
            }
            else
            {
                OnDestroyedRoad?.Invoke(this);
            }
        }
    }
    #endregion

    #region Moving

    private enum MovingState
    {
        Direction,
        Point,
        Object
    }
    private void Move()
    {
        if (_currentDirection != Vector3.zero)
        {
            if (!anim.GetBool(IS_MOVING_BOOL))
                anim.SetBool(IS_MOVING_BOOL, true);

            switch (_movingState)
            {
                case MovingState.Direction:
                    transform.Translate(_currentDirection * data.MovementSpeed * Time.deltaTime, Space.World);
                    break;
                case MovingState.Point:
                    transform.position = Vector3.MoveTowards(transform.position, _targetPoint, data.MovementSpeed * Time.deltaTime);
                    // Определить когда дошёл до цели и вызвать событие onPointReached
                    if (transform.position == _targetPoint)
                        OnPointReached?.Invoke();
                    break;
                case MovingState.Object:
                    if (_targetObject != null)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, _targetObject.transform.position, data.MovementSpeed * Time.deltaTime);
                        if (Vector3.Distance(transform.position, _targetObject.transform.position) <= 0.1f)
                        {
                            OnObjectReached?.Invoke();
                        }
                        if (_targetObject != null)
                            _currentDirection = _targetObject.transform.position - transform.position;
                    }
                    break;
            }
        }
    }
    public void StopMoving()
    {
        _currentDirection = Vector3.zero;
        anim.SetBool(IS_MOVING_BOOL, false);
    }

    public void KeepMoving()
    {
        ActionsOnRoadBlock(_curRoadBlock);
    }

    private void LookToMovementDirection()
    {
        if (_currentDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(_currentDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, data.TurnSpeed * Time.deltaTime);
        }
    }

    private bool _onLastTile;
    private void KeepGoing(Vector3 nextDirection, Transform rBlockTrans)
    {
        if (_currentDirection == nextDirection)
        {
            SetMovingByDirection(_currentDirection);
        }
        else if (_currentDirection == -nextDirection)
        {
            Vector3 middlePoint;
            if (PointFound(out middlePoint, transform.position, rBlockTrans.position))
            {
                // Идти до точки поворота, повернуться и идти в новом направлении
                SetMovingToPoint(middlePoint, () => TurnAndGo(nextDirection));
            }
        }
        else
        {
            Vector3 turnPoint;
            if (TurnPointFound(out turnPoint, nextDirection, rBlockTrans))
            {
                // Идти до точки поворота, повернуться и идти в новом направлении
                SetMovingToPoint(turnPoint, () => TurnAndGo(nextDirection));
            }
        }
    }

    public void KeepGoingInTower(Tower tower, Transform spawnTrans)
    {
        if (tower.MyRoad != null && tower.MyRoad.Count > 0)
        {
            MyTower = tower;
            _nextRoadBlock = tower.MyRoad[0];
            _currentDirection = tower.transform.position - _curRoadBlock.transform.position;
            Vector3 nextDirection = _nextRoadBlock.transform.position - tower.transform.position;
            KeepGoing(nextDirection, spawnTrans);
        }
    }

    private void SetMovingToPoint(Vector3 point, Action onPointReached = null)
    {
        _movingState = MovingState.Point;
        _targetPoint = point;
        OnPointReached = onPointReached;
    }
    private void SetMovingByDirection(Vector3 direction)
    {
        _movingState = MovingState.Direction;
        _currentDirection = direction;
    }
    private void SetMovingToObject(Transform targetObject)
    {
        _movingState = MovingState.Object;
        _targetObject = targetObject;
        _currentDirection = targetObject.transform.position - transform.position;
    }
    private void TurnAndGo(Vector3 nextDirection)
    {
        _currentDirection = nextDirection;
        SetMovingByDirection(_currentDirection);
    }
    private bool TurnPointFound(out Vector3 turnPoint, Vector3 nextDirection, Transform rBlockTrans)
    {
        Vector3 linePoint1 = rBlockTrans.position + (nextDirection * (-0.5f)) + (rBlockTrans.right * XOffset);
        Vector3 linePoint2 = rBlockTrans.position + (_currentDirection * (-0.5f)) + ((Quaternion.AngleAxis(90f, Vector3.up) * _currentDirection) * XOffset);

        if (MathFormulas.LineLineIntersection(out turnPoint, linePoint1, nextDirection, linePoint2, _currentDirection))
        {
            return true;
        }

        return false;
    }

    private bool PointFound(out Vector3 point, Vector3 linePoint1, Vector3 middleLinePoint2)
    {
        Vector3 linePoint2 = middleLinePoint2 + (Quaternion.AngleAxis(90f, Vector3.up) * _currentDirection * (-0.5f));

        if (MathFormulas.LineLineIntersection(out point, linePoint1, _currentDirection, linePoint2, Quaternion.AngleAxis(90f, Vector3.up) * _currentDirection))
        {
            return true;
        }

        return false;
    }
    #endregion

    #region Crowd
    private bool _reachedRandPoint;
    private Vector3 _randPoint;
    /// <summary>
    /// Ожидать дальнейшего построения пути
    /// Взять рандомное значение в пределах _spawnRange моей башни
    /// Найти исходя из полученного значения точку на тайле (должна быть перед юнитом)
    /// Установить движение до этой точки
    /// При достижении полученной точки, остановить движение
    /// </summary>
    private void HandlerOnWaitingRoadBlock(RoadBlock rBlock)
    {
        if (!_onLastTile)
        {
            _onLastTile = true;

            RoadBuilder.OnTileRoaded += HandlerOnTileRoaded;
            RoadBuilder.OnRoadLinked += HandlerOnRoadLinked;

            float randRange = MyTower.SpawnPosRange;
            float randZ = UnityEngine.Random.Range(-randRange, randRange);

            if (PointFound(out _randPoint, transform.position, _curRoadBlock.transform.position + (_currentDirection * randZ)))
            {
                Action onPointReached = delegate { };

                if (!ReachedMaxUnitsOnTile(rBlock.MyTile))
                {
                    onPointReached = () =>
                    {
                        StopMoving();
                        _reachedRandPoint = true;
                    };
                }
                else
                {
                    onPointReached = () =>
                    {
                        if (_onLastTile)
                            MyTower.HandleMaxUnitsOnTile(rBlock.MyTile, this);
                    };

                    Vector3 middlePoint;
                    if (PointFound(out middlePoint, transform.position, rBlock.transform.position))
                    {
                        _randPoint = middlePoint;
                    }
                }

                SetMovingToPoint(_randPoint, onPointReached);
            }
        }
    }

    private void HandlerOnTileRoaded(RoadBuilder rBuilder, Tile tile, RoadBlock rBlock, Tile sourceTile)
    {
        if (rBlock.BaseTower.Equals(MyTower) && sourceTile.Equals(_curRoadBlock.MyTile))
        {
            MoveFromWaiting(rBlock);
        }
    }

    private void HandlerOnRoadLinked(RoadBuilder rBuilder, Tower linkedTower, Tile sourceTile)
    {
        if (MyTower.MyRoadBuilder.Equals(rBuilder) && sourceTile.Equals(_curRoadBlock.MyTile))
        {
            MoveFromWaiting();
        }
    }

    private void MoveFromWaiting(RoadBlock rBlock = null)
    {
        if (MyTower.MyRoad.Contains(_curRoadBlock))
        {
            _onLastTile = false;
            _nextRoadBlock = rBlock;
            Vector3 nextDirection = _curRoadBlock.transform.forward;

            if (_nextRoadBlock != null && nextDirection != _currentDirection)
            {
                XOffset = _curRoadBlock.transform.InverseTransformPoint(_randPoint).x;
            }

            if (_reachedRandPoint)
            {
                TurnAndGo(nextDirection);
            }
            else
            {
                SetMovingToPoint(_randPoint, () => TurnAndGo(nextDirection));
            }

            _reachedRandPoint = false;

            RoadBuilder.OnTileRoaded -= HandlerOnTileRoaded;
            RoadBuilder.OnRoadLinked -= HandlerOnRoadLinked;
        }
    }

    public bool ReachedMaxUnitsOnTile(Tile tile)
    {
        List<Unit> myFractionUnitsOnTile = new List<Unit>();
        bool reachedMax = false;

        for (int i = 0; i < tile.UnitsOnTile.Count; i++)
        {
            if (tile.UnitsOnTile[i].Fraction == this.Fraction)
                myFractionUnitsOnTile.Add(tile.UnitsOnTile[i]);
        }

        if (myFractionUnitsOnTile.Count >= data.MaxUnitsInCrowd)
        {
            reachedMax = true;
        }

        return reachedMax;
    }
    #endregion

    #region Multiplying
    public void CopyValuesFromUnit(Unit sourceUnit)
    {
        _currentDirection = sourceUnit.CurrentDirection;
        _curRoadBlock = sourceUnit.CurRoadBlock;
        _nextRoadBlock = sourceUnit.NextRoadBlock;
        _targetPoint = sourceUnit.TargetPoint;
        MyTower = sourceUnit.MyTower;
    }
    #endregion

    #region Battle
    private Unit SearchForClosestEnemyUnit(List<Unit> units)
    {
        if (units != null)
        {
            List<Unit> enemyUnits = new List<Unit>();

            foreach (Unit unit in units)
            {
                if (unit.Equals(this))
                    continue;

                if (unit.Fraction != this.Fraction && !unit.IsFighting && !unit.IsDead)
                {
                    enemyUnits.Add(unit);
                }
            }

            Unit closestEnemy = GetClosestUnit(enemyUnits);

            return closestEnemy;
        }

        return null;
    }
    private Unit GetClosestUnit(List<Unit> units)
    {
        if (units != null && units.Count > 0)
        {
            float shortestDistance = Mathf.Infinity;
            int closestUnitIndex = 0;

            for (int i = 0; i < units.Count; i++)
            {
                float distanceToUnit = Vector3.Distance(this.transform.position, units[i].transform.position);

                if (distanceToUnit < shortestDistance)
                {
                    shortestDistance = distanceToUnit;
                    closestUnitIndex = i;
                }
            }

            return units[closestUnitIndex];
        }

        return null;
    }

    /// <summary>
    /// Двигаться по направлению к врагу и спровоцировать врага
    /// </summary>
    public void Fight(Unit enemyUnit)
    {
        if (enemyUnit != null)
        {
            IsFighting = true;
            SetMovingToObject(enemyUnit.transform);

            if (_targetObject != null && !enemyUnit.IsFighting)
                enemyUnit.Fight(this);
        }
    }

    private void Die()
    {
        StopMoving();
        IsDead = true;
        IsFighting = false;
        anim.SetTrigger(DIE_TRIGGER);
    }
    #endregion
}