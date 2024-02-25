using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerUnitsText : ChangingValueText
{
    private Tower _myTower;

    protected override void Awake()
    {
        base.Awake();
        _myTower = GetComponentInParent<Tower>();
    }

    protected override void InitChangingValue()
    {
        changingValue = _myTower.UnitsCount;
    }

    protected override void SubscribeToEvents()
    {
        _myTower.OnUnitsCountChanged += HandlerOnValueChanged;
    }

    protected override void UnSubscribeFromEvents()
    {
        if (_myTower != null) 
            _myTower.OnUnitsCountChanged -= HandlerOnValueChanged;
    }

    // Start is called before the first frame update
    void Start()
    {
        InitValue();
    }

    private void Update()
    {
        if (_maxReached && _increment)
        {
            valueText.text = "MAX";
            _increment = false;
        }
    }

    private bool _maxReached;
    private bool _increment;
    private void HandlerOnValueChanged(Tower tower, int value, bool increment)
    {
        _increment = increment;

        if (_maxReached)
        {
            if (increment)
                return;
            else
                _maxReached = false;
        }

        if (value < _myTower.MaxUnitsCount)
        {
            ChangeValueText(value);
        }
        else
        {
            valueText.text = "MAX";
            _maxReached = true;
        }
    }
}
