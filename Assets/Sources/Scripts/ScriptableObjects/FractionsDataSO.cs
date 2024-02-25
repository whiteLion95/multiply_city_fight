using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FractionsData", menuName = "ScriptableObjects/FractionsData")]
public class FractionsDataSO : ScriptableObject
{
    [field: SerializeField] public List<FractionData> Fractions { get; private set; }

    public FractionData GetFractionData(Fraction fraction)
    {
        FractionData fData = default;

        foreach (FractionData data in Fractions)
        {
            if (data.Fraction == fraction)
                fData = data;
        }

        return fData;
    }

    /// <summary>
    /// The greater the index of fraction data in Fractions array, the greater the fraction data
    /// </summary>
    public int CompareFractionsByPriority(FractionData frData1, FractionData frData2)
    {
        if (!Fractions.Contains(frData1))
        {
            if (!Fractions.Contains(frData2))
            {
                // If Fractions don't contain frData1 and frData2,
                // they're equal
                return 0;
            }
            else
            {
                // If Fractions don't contain frData1 but contain frData2,
                // frData2 is greater
                return -1;
            }
        }
        else
        {
            // If Fraction contain frData1...
            if (!Fractions.Contains(frData2))
            {
                // ... and don't contain frData2,
                // frData1 is greater
                return 1;
            }
            else
            {
                // ... and contain frData2,
                // compare the indexes of two fractions data
                // in the Fractions list
                int index1 = Fractions.IndexOf(frData1);
                int index2 = Fractions.IndexOf(frData2);

                return index1.CompareTo(index2);
            }
        }
    }
}

[System.Serializable]
public class FractionData
{
    [field: SerializeField] public Fraction Fraction { get; private set; }
    [field: SerializeField] public Color TowerColor { get; private set; }
    [field: SerializeField] public Color UnitsColor { get; private set; }
    [field: SerializeField] public Color RoadColor { get; private set; }
}

public enum Fraction
{
    Player,
    RedBot,
    GreenBot,
    YellowBot,
    Neutral
}
