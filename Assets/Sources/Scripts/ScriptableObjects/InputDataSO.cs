using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InputData", menuName = "ScriptableObjects/InputData")]
public class InputDataSO : ScriptableObject
{
    [SerializeField] [Tooltip("Сколько нужно держать палец на башне, чтобы удалить её дорогу")] private float deleteTowerDuration;

    public float DeleteTowerDuration { get => deleteTowerDuration; }
}
