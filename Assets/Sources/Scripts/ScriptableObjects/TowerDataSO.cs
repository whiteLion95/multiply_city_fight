using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TowerData", menuName = "ScriptableObjects/TowerData")]
public class TowerDataSO : ScriptableObject
{
    [SerializeField] private int maxUnitsCount;
    [Tooltip("Время в секундах, необходимое на генерацию одного юнита")] [SerializeField] private float generationTime;
    [Tooltip("Время в секундах, необходимое на спавн одного юнита")] [SerializeField] private float spawnTime;

    public int MaxUnitsCount { get => maxUnitsCount; }
    /// <summary>
    /// Время, необходимое на генерацию одного юнита
    /// </summary>
    public float GenerationTime { get => generationTime; }
    /// <summary>
    /// Время, необходимое на спавн одного юнита
    /// </summary>
    public float SpawnTime { get => spawnTime; }
}
