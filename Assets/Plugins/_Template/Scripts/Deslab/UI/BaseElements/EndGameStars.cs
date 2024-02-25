using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Deslab.UI
{
    [Serializable]
    public class EndGameStars
    {
        public ParticleSystem sparkParticle;
        public Image coloredStarImage;
        public float showDuration = 0.5f;
        public float showDelay = 0.5f;

        public void ShowStar()
        {
            coloredStarImage.transform.DOScale(1, showDuration)
                .SetEase(Ease.OutBounce)
                .SetDelay(showDelay)
                .OnComplete(() =>
                {
                    sparkParticle.Play();
                    HapticsManager.Instance.LightVibrate();
                });
        }

        public void ResetStar()
        {
            coloredStarImage.transform.localScale = Vector3.zero;
        }
    }
}