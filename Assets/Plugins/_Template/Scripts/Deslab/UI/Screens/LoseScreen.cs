using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Deslab.UI
{
    public class LoseScreen : CanvasGroupWindow
    {
        [SerializeField] private Button claimButton;
        [SerializeField] private Header _header;
        [SerializeField] private Image skullImage;

        public static Action OnLoseImage;
        
        private void Awake()
        {
            _header.Init();
            
            claimButton.onClick.AddListener(() =>
            {
                if (!StaticManager.Instance.debugMode)
                    StaticManager.AddMoney(StaticManager.moneyCollectedOnLevel);
                claimButton.interactable = false;
                claimButton.transform.localScale = Vector3.zero;
                StaticManager.victory = false;
                StaticManager.Restart();
                StaticManager.LoadNextLevel();
                //UIManager.instance.ShowMenu();
                //StaticManager.SavePlayerData();
                //DeslyticsManager.LevelRestart();
            });
        }

        public override void ShowWindow(Action onCompleted = null)
        {
            ResetParameters();

            base.ShowWindow(() =>
            {
                _header.PlayAnimation();
                ShowSkullWithClaimButton();
                claimButton.interactable = true;
            });

            void ResetParameters()
            {
                _header.ResetValues();
                skullImage.color = new Color(255, 255, 255, 0);
                skullImage.transform.localScale = Vector3.zero;
                claimButton.transform.localScale = Vector3.zero;
            }
        }

        private void ShowSkullWithClaimButton()
        {
            var extraDuration = 1f;
            
            skullImage.DOColor(Color.white, 0.5f).SetDelay(extraDuration).SetEase(Ease.OutQuad);
            skullImage.transform.DOScale(1, 0.5f).SetDelay(extraDuration).SetEase(Ease.OutQuad);
            claimButton.transform.DOScale(1, 0.2f).SetDelay(fadeTime + extraDuration).SetEase(Ease.OutQuad);

            OnLoseImage?.Invoke();
        }

        public override void HideWindow()
        {
            base.HideWindow(() =>
            {
                claimButton.transform.localScale = Vector3.zero;
            });
        }
    }
}
