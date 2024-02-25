using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Deslab.UI.FX
{
    public class ExampleFXUIGenerator : FXUIGenerator
    {
        [SerializeField] private bool manyFXs;
        
        #region CALLBACKS

        private void OnClick()
        {
            var startPosition = this.transform.position;
            if (this.manyFXs)
                this.MakeFXMany(startPosition);
            else
                this.MakeFX(startPosition);
        }


        public void MakeMoneyFX()
        {
            OnParticleReachedTargetEvent += ParticleReachedTargetEvent;
            OnFXReachedTargetEvent += FXReachedTargetEvent;
            var startPosition = this.transform.position;
            MakeFXMany(startPosition);
        }

        public DOTweenAnimation tweenAnimation;

        private void ParticleReachedTargetEvent(FXUIGenerator fXUIGenerator)
        {
            tweenAnimation.DORestart();
            tweenAnimation.DORewind();
            tweenAnimation.DOPlay();
        }

        private void FXReachedTargetEvent(FXUIGenerator fXUIGenerator)
        {
            OnParticleReachedTargetEvent -= ParticleReachedTargetEvent;
            OnFXReachedTargetEvent -= FXReachedTargetEvent;
        }

        #endregion
    }
}