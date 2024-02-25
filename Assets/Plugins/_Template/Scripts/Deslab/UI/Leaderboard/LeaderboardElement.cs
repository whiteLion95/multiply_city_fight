using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Deslab.UI
{
    public class LeaderboardElement : MonoBehaviour
    {
        public TMP_Text entityPlace;
        public TMP_Text entityName;
        public Transform playerIndicator;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void SetPlacementID(int placementID, LeaderboardListElement leaderboardListElement)
        {
            placementID++;
            entityPlace.text = placementID.ToString();
            entityName.text = leaderboardListElement.entityName;
            if (leaderboardListElement.isPlayer)
                EnablePlayerVisual();
            else
                DisablePlayerVisual();
        }

        private void EnablePlayerVisual()
        {
            DOTween.To(() => _rectTransform.sizeDelta, x => _rectTransform.sizeDelta = x,
                new Vector2(_rectTransform.sizeDelta.x, 100), 0.5f).SetEase(Ease.InOutQuart);
            playerIndicator.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutQuart);
        }

        private void DisablePlayerVisual()
        {
            DOTween.To(() => _rectTransform.sizeDelta, x => _rectTransform.sizeDelta = x,
                new Vector2(_rectTransform.sizeDelta.x, 70), 0.5f).SetEase(Ease.InOutQuart);
            playerIndicator.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InOutQuart);
        }
    }
}