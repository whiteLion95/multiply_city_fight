using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Абстрактный родительский класс для обектов на тайлах
/// </summary>
public abstract class TileObject : MonoBehaviour
{
    [SerializeField] protected BotsDataSO botsData;

    public Action<Tile> OnTileSet;

    public Tile MyTile { get; protected set; }
    public int Priority { get; private set; }

    public void SetTile(Tile tile)
    {
        MyTile = tile;
        OnTileSet?.Invoke(tile);

        SetPriority();
    }

    private void SetPriority()
    {
        if (this is Tower)
        {
            Tower thisTower = this as Tower;

            if (thisTower.FractionData.Fraction == Fraction.Neutral)
                Priority = botsData.GetPriority(TargetObject.NeutralTower);
            else
                Priority = botsData.GetPriority(TargetObject.EnemyTower);
        }
        else if (this is Multiplier)
        {
            Priority = botsData.GetPriority(TargetObject.Multiplier);
        }
    }
}
