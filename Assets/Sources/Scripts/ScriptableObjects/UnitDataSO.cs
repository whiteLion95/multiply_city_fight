using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitData", menuName = "ScriptableObjects/UnitData")]
public class UnitDataSO : ScriptableObject
{
    [SerializeField] [Tooltip("Максимальное кол-во видимых юнитов в толпе")] private int maxUnitsInCrowd;
    [SerializeField] [Tooltip("Максимальное кол-во юнитов на уровне")] private int maxUnitsOnLevel;

    [field: SerializeField] public float MovementSpeed { get; private set; }
    [field: SerializeField] public float TurnSpeed { get; private set; }
    public int MaxUnitsInCrowd { get => maxUnitsInCrowd; }
    public int MaxUnitsOnLevel { get => maxUnitsOnLevel; }
}
