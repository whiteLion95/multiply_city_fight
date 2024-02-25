using System;
using Deslab.UI.FX;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Deslab.UI
{
    [Serializable]
    public class Reward
    {
        public Transform rewardPanel;
        public CanvasGroup rewardGroup;
        public TMP_Text rewardLabel;
        
        private Vector3 _startPos;

        [SerializeField] private ParticleSystem _confetti;
        [SerializeField] private ExampleFXUIGenerator _fxuiGenerator;

        [SerializeField] private Ease _ease;
        [SerializeField] private float _baseDuration;
        [SerializeField] private float _delay;

        public void Init()
        {
            _startPos = rewardPanel.position;
        }

        public void PlayAnimation(Action onCompleted = null)
        {
            rewardGroup.DOFade(1, _baseDuration).SetDelay(_delay).SetEase(_ease);
            rewardPanel.transform.DOLocalMoveY(0, _baseDuration).SetDelay(_delay).SetEase(_ease).OnComplete(() =>
            {
                _confetti.Play();
                //_fxuiGenerator.MakeMoneyFX();
                onCompleted?.Invoke();
            });
        }

        public void ResetValues()
        {
            rewardLabel.text = "0";
            rewardPanel.position = _startPos;
            rewardGroup.alpha = 0;
        }
    }
}