using UnityEngine;

namespace Deslab.UI.FX
{
    public class FXUIObject : FXObject
    {
        [SerializeField] private FXUIAnimation m_animation;
        [Space] [SerializeField] private bool randomOffsetOnStart;
        [SerializeField] private float offsetMin = 0.05f;
        [SerializeField] private float offsetMax = 0.1f;

        public new FXUIAnimation animation => this.m_animation;

        public void Go(Vector3 startPosition, Transform targetPoint)
        {
            var finalStartPosition = startPosition;

            if (this.randomOffsetOnStart)
            {
                var rX = Random.Range(this.offsetMin, this.offsetMax) * Math.RandomSign();
                var rY = Random.Range(this.offsetMin, this.offsetMax) * Math.RandomSign();
                finalStartPosition = startPosition + new Vector3(rX, rY, 0f);
            }

            this.transform.position = finalStartPosition;
            this.animation.Play(finalStartPosition, targetPoint);
        }
    }
}