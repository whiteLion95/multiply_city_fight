using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZ_Pooling;

/// <summary>
/// Чтобы точно не оставались заспавненные юниты с прошлого уровня
/// </summary>
public class PoolManagerExtension : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform child in EZ_PoolManager.Instance.transform)
        {
            if (child.gameObject.activeSelf)
                EZ_PoolManager.Despawn(child);
        }
    }
}
