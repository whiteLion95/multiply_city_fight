using UnityEngine;

[CreateAssetMenu(fileName = "ColorsCoreData", menuName = "Deslab/ScriptableObjects/ColorsCoreData")]

public class ColorsCoreData : ScriptableObject
{
    [SerializeField] private Material _changingMaterial;
    [SerializeField] private float _changingColorSmoothness;

    public Material ChangingMaterial { get { return _changingMaterial; } }
    public float ChangingColorSmoothness { get { return _changingColorSmoothness; } }
}
