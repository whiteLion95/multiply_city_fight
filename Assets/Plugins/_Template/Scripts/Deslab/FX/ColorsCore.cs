using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

namespace Deslab.FX
{
    [Serializable]
    public class ColorsCore
    {
        [SerializeField] protected ColorsCoreData _data;

        private Color _startingColor;

        /// <summary>
        /// Sets starting color. Invoke it in your Awake or Start method
        /// </summary>
        public void SetStartingColor()
        {
            try
            {
                _startingColor = _data.ChangingMaterial.color;
            }
            catch (NullReferenceException)
            {
                Debug.LogError("You need to apply changing material in Inspector");
            }
        }

        /// <summary>
        /// Sets color of changing material
        /// </summary>
        /// <param name="color"></param>
        public void SetColor(Color color)
        {
            try
            {
                if (_data.ChangingMaterial.color != color)
                {
                    _data.ChangingMaterial.DOColor(color, _data.ChangingColorSmoothness);
                }
            }
            catch (NullReferenceException)
            {
                Debug.LogError("You need to apply changing material in Inspector");
            }
        }

        /// <summary>
        /// Resets color of changing material to the starting color. It can be invoked in your OnDisable method
        /// </summary>
        public void ResetColor()
        {
            try
            {
                _data.ChangingMaterial.color = _startingColor;
            }
            catch (NullReferenceException)
            {
                Debug.LogError("You need to apply changing material in Inspector");
            }
        }

        public static bool CompareColors(Color32 c1, Color32 c2)
        {
            return c1.r == c2.r && c1.g == c2.g && c1.b == c2.b && c1.a == c2.a;
        }

        public Color CurrentColor { get { return _data.ChangingMaterial.color; } }
    }
}
