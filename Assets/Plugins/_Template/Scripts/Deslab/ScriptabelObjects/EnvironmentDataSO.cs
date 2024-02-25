using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "EnvironmentData", menuName = "Deslab/ScriptableObjects/EnvironmentData")]
public class EnvironmentDataSO : ScriptableObject
{
    [field: SerializeField] public ColorScheme ColorScheme { get; private set; }
    [field: SerializeField] public GameObject Environment { get; private set; }

    public void ApplyColorScheme()
    {
        foreach (MaterialColor matCol in ColorScheme.MaterialsColors)
        {
            matCol.SetColors();
        }
    }
}

[Serializable]
public struct ColorScheme
{
    [field: SerializeField] public MaterialColor[] MaterialsColors { get; private set; }
}

[Serializable]
public struct MaterialColor
{
    [field: SerializeField] public Material ChangingMaterial{ get; private set; }
    [field: SerializeField] public Color Color { get; private set; }
    [field: SerializeField] public Color HighlightColor { get; private set; }
    [field: SerializeField] public Color ShadowColor { get; private set; }

    public void SetColors()
    {
        ChangingMaterial.SetColor("_Color", Color);
        ChangingMaterial.SetColor("_HColor", HighlightColor);
        ChangingMaterial.SetColor("_SColor", ShadowColor);
    }
}
