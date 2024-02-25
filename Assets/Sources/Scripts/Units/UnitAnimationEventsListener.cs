using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnimationEventsListener : MonoBehaviour
{
    private Unit _unit;

    private void Awake()
    {
        _unit = GetComponentInParent<Unit>();
    }

    public void HandlerDieAnimationEnd()
    {
        _unit.OnDead?.Invoke(_unit);
    }
}
