using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZ_Pooling;
using TMPro;

public class MaxUnitsText : MonoBehaviour
{
    private TMP_Text _text;

    private void Awake()
    {
        _text = GetComponentInChildren<TMP_Text>();
    }

    private void OnEnable()
    {
        Invoke(nameof(Despawn), 3f);
    }

    public void Despawn()
    {
        EZ_PoolManager.Despawn(transform);
    }
}
