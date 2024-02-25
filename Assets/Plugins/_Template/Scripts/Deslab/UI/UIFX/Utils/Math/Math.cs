using UnityEngine;

namespace Deslab.UI.FX
{
    public static class Math
    {
        public static int RandomSign()
        {
            return Random.Range(0, 2) * 2 - 1;
        }
    }
}