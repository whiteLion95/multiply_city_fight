using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using EZ_Pooling;

public class FadingDecal : MonoBehaviour, IPoolable
{
    [SerializeField] private float fadingSmoothness = 1f;

    private SpriteRenderer _sprite;

    private void Awake()
    {
        _sprite = GetComponentInChildren<SpriteRenderer>();
    }

    public void OnSpawned()
    {
        StartFading();
    }

    public void OnDespawned()
    {
        ResetSpriteTransparency();
    }

    private void StartFading()
    {
        _sprite.DOFade(0f, fadingSmoothness).onComplete +=
            () => EZ_PoolManager.Despawn(transform);
    }

    private void ResetSpriteTransparency()
    {
        Color tempColor = _sprite.color;
        tempColor.a = 1;
        _sprite.color = tempColor;
    }
}
