using System;
using System.Collections;
using UnityEngine;

namespace Deslab.UI.FX
{
    public abstract class FXUIGenerator : FXGenerator<FXUIObject>
    {
        #region EVENTS

        public event Action<FXUIGenerator> OnFXReachedTargetEvent;
        public event Action<FXUIGenerator> OnParticleReachedTargetEvent;

        #endregion

        [SerializeField] protected Transform transformTarget;
        [Space] [SerializeField] protected int packCountMin = 3;
        [SerializeField] protected int packCountMax = 5;
        [SerializeField] protected float timeBetweenParts = 0.1f;
        [SerializeField] protected float startDelay = 0;

        protected void MakeFX(Vector3 validPosition)
        {
            this.MakeFXMany(validPosition, 1);
        }

        protected void MakeFXMany(Vector3 validPosition)
        {
            var rCount = UnityEngine.Random.Range(this.packCountMin, this.packCountMax + 1);
            this.MakeFXMany(validPosition, rCount);
        }

        protected void MakeFXMany(Vector3 validPosition, int count)
        {
            this.StartCoroutine(this.FXWorkRoutine(validPosition, count));
        }

        int finishedParts = 0;

        void OnParticleReachedTarget(FXUIAnimation fxuiAnimation)
        {
            OnParticleReachedTargetEvent?.Invoke(this);
            fxuiAnimation.OnAnimationFinishedEvent -= OnParticleReachedTarget;
            fxuiAnimation.gameObject.SetActive(false);
            finishedParts++;
        }

        protected virtual IEnumerator FXWorkRoutine(Vector3 validPosition, int count)
        {
            yield return new WaitForSeconds(startDelay);

            for (int i = 0; i < count; i++)
            {
                var fx = this.fxPool.GetFreeElement();
                fx.Go(validPosition, this.transformTarget);
                fx.animation.OnAnimationFinishedEvent += OnParticleReachedTarget;
                yield return new WaitForSeconds(this.timeBetweenParts);
            }

            while (finishedParts < count)
                yield return null;

            this.OnFXReachedTargetEvent?.Invoke(this);
        }
    }
}