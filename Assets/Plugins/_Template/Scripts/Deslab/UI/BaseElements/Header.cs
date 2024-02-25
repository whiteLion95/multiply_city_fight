using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Deslab.UI.FX;

namespace Deslab.UI
{
    [Serializable]
    public struct Header
    {
        public TMP_Text levelLabel;
        public TMP_Text actionLabel;
        [SerializeField] private Color32 _fromColorLevelLabel;
        [SerializeField] private Color32 _fromColorActionLabel;
        [SerializeField] private Color32 _toColorLevelLabel;
        [SerializeField] private Color32 _toColorActionLabel;
        [SerializeField] private Ease _ease;

        [SerializeField] private float _baseDuration;
        [SerializeField] private float _delay;

        private Vector3 _startLevelLabelPos;
        private Vector3 _startActionLabelPos;

        public void Init()
        {
            _startLevelLabelPos = levelLabel.transform.position;
            _startActionLabelPos = actionLabel.transform.position;
        }

        public void PlayAnimation(Action onCompleted = null)
        {
            levelLabel.transform.DOLocalMoveX(0, _baseDuration).SetDelay(_delay).SetEase(_ease);
            actionLabel.transform.DOLocalMoveX(0, _baseDuration).SetDelay(_delay + _delay / 2).SetEase(_ease);
            levelLabel.DOColor(_toColorLevelLabel, _baseDuration).SetDelay(_delay).SetEase(_ease);
            actionLabel.DOColor(_toColorActionLabel, _baseDuration).SetDelay(_delay + _delay / 2).SetEase(_ease).OnComplete((() =>
            {
                onCompleted?.Invoke();
            }));
        }

        public void ResetValues()
        {
            levelLabel.transform.position = _startLevelLabelPos;
            actionLabel.transform.position = _startActionLabelPos;
            levelLabel.color =  _fromColorLevelLabel;
            actionLabel.color = _fromColorActionLabel;
        }
    }
}