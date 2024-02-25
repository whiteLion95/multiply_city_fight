using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Deslab.FX
{
    /// <summary>
    /// Area where incoming object changes his color. Object with this component must have collider component with isTrigger On
    /// </summary>
    public class ChangeColorArea : MonoBehaviour
    {
        [SerializeField] [Tooltip("Incoming object's tag")] private string[] _objectTags;
        private Color _areaColor;
        private bool _isOnColorArea;

        public static Action<Color> OnColorArea = delegate { };
        public static Action OnOutOfColorArea = delegate { };

        [Obsolete]
        private void Awake()
        {
            SetAreaColor();
        }

        private void OnTriggerEnter(Collider other)
        {
            //if (other.GetComponent<SpringEnd>() == SpringEnd.ForwardEnd && !_isOnColorArea)
            //{
            //    _isOnColorArea = true;
            //    OnColorArea?.Invoke(_areaColor);
            //}
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") && _isOnColorArea)
            {
                _isOnColorArea = false;
                OnOutOfColorArea?.Invoke();
            }
        }

        [Obsolete]
        private void SetAreaColor()
        {
            try
            {
                _areaColor = GetComponentInChildren<ParticleSystem>().startColor;
            }
            catch (NullReferenceException)
            {
                Debug.LogError("There must be a child object of the object with ChangeColorArea component on it with ParticleSystem component");
            }
        }

        public Color AreaColor { get { return _areaColor; } }
    }
}
