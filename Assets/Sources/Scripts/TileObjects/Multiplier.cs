using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EZ_Pooling;
using System;
using TMPro;

public class Multiplier : TileObject
{
    [SerializeField] private int multiplyValue;
    [Tooltip("Двухсторонний диапазон рандомной координаты x для спавна клона. 0 - центр тайла, 0.5 - край")]
    [SerializeField] [Range(0f, 0.5f)] private float spawnPosRange;
    [Tooltip("Дистанция между диапазонами спавна в процентах")]
    [SerializeField] [Range(0f, 1f)] private float minDistanceFromOriginal = 0.2f;
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text multiplyText;
    [SerializeField] private UnitDataSO unitsData;

    public static Action<Unit, Unit> OnUnitMultiplied;
    public static Action<Unit, Multiplier> OnUnitOnMultiplier;

    private Color _originalColor;
    private GameManager _gameManager;

    public int MultiplyValue { get => multiplyValue; }

    private void Awake()
    {
        _originalColor = image.color;
        multiplyText.text = "x" + multiplyValue;
    }

    private void Start()
    {
        _gameManager = GameManager.Instance;
    }

    public void Discolor()
    {
        Color tempColor = image.color;
        tempColor.a = 0;
        image.color = tempColor;
    }

    public void ResetColor()
    {
        image.color = _originalColor;
    }

    public void MultiplyUnit(Unit unit)
    {
        if (_gameManager.GetFractionUnitsOnLevel(unit.MyTower).Count < _gameManager.UnitsPerFraction)
        {
            OnUnitOnMultiplier?.Invoke(unit, this);
            Unit sourceUnit = unit;
            float rangePiece = (unit.MyTower.SpawnPosRange * 2) / multiplyValue;
            float gap = minDistanceFromOriginal * rangePiece;
            float leftX, rightX = 0f;
            int clonesCount = 0;

            for (int i = 0; i < multiplyValue; i++)
            {
                if (_gameManager.GetFractionUnitsOnLevel(unit.MyTower).Count < _gameManager.UnitsPerFraction)
                {
                    if (unit.ReachedMaxUnitsOnTile(MyTile))
                    {
                        unit.MyTower.HandleMaxUnitsOnTile(MyTile, unit, false);
                        return;
                    }

                    if (clonesCount < (multiplyValue - 1))
                    {
                        if (i == 0)
                            leftX = -unit.MyTower.SpawnPosRange;
                        else
                            leftX = rightX + gap;

                        if (i == (multiplyValue - 1))
                            rightX = unit.MyTower.SpawnPosRange;
                        else
                            rightX = (leftX + rangePiece) - (gap / 2);

                        if (unit.XOffset >= leftX && unit.XOffset <= (leftX + rangePiece))
                            continue;

                        if (sourceUnit != null)
                        {
                            //Calculating spawn position for clone
                            float xOffset = GetXOffset(leftX, rightX);
                            Vector3 spawnPos = transform.position + (sourceUnit.CurrentDirection * (-0.6f)) + (Quaternion.AngleAxis(90f, Vector3.up) * sourceUnit.CurrentDirection * xOffset);
                            Debug.DrawRay(spawnPos, Vector3.up, Color.red, 5f);

                            //spawning clone and assigning values
                            Unit newUnit = EZ_PoolManager.Spawn(sourceUnit.transform, spawnPos, sourceUnit.transform.rotation).
                                GetComponent<Unit>();
                            newUnit.CopyValuesFromUnit(sourceUnit);
                            newUnit.XOffset = xOffset;
                            clonesCount++;
                            //newUnit.PlayTween();

                            OnUnitMultiplied?.Invoke(sourceUnit, newUnit);
                            sourceUnit = newUnit;
                        }
                    }
                }
            }
        }
    }

    private float GetXOffset(float leftX, float rightX)
    {
        float xOffset = UnityEngine.Random.Range(leftX, rightX);
        return xOffset;
    }
}
