using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialLevel : MonoBehaviour
{
    [field: SerializeField] public List<StepTiles> Steps { get; private set; }

    public static TutorialLevel Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDisable()
    {
        Instance = null;
    }
}

[System.Serializable]
public struct StepTiles
{
    [field: SerializeField] public List<Tile> AvailableTiles { get; private set; }
}