using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Deslab.UI.FX;

namespace Deslab.UI
{
    public class WinScreen : CanvasGroupWindow
    {
        [SerializeField] private Button claimButton;
        [SerializeField] private Header _header;
        [SerializeField] private Reward _reward;
        [SerializeField] private TMP_Text earningsMoneyCount;
        [SerializeField] private ExampleFXUIGenerator fXUIGenerator;

        public List<EndGameStars> endGameStars = new List<EndGameStars>();

        public static Action OnCongrats;
        public static Action OnRewardsClaim;

        private void Awake()
        {
            _header.Init();
            _reward.Init();
            claimButton.onClick.AddListener(() =>
            {
                if (!StaticManager.Instance.debugMode)
                    StaticManager.AddMoney(StaticManager.moneyCollectedOnLevel);
                claimButton.interactable = false;
                claimButton.transform.localScale = Vector3.zero;
                StaticManager.victory = true;
                StaticManager.LoadNextLevel();
                UIManager.Instance.ShowScreen(ScreenType.Menu);
                //StaticManager.SavePlayerData();
                StaticManager.Restart();
                OnRewardsClaim?.Invoke();
            });
        }

        public override void ShowWindow(Action onCompleted = null)
        {
            earningsMoneyCount.text = StaticManager.Instance.playerData.money.ToString();
            fXUIGenerator.OnParticleReachedTargetEvent += OnRewardMoneyReachedEarnings;
            ResetParameters();

            base.ShowWindow(() =>
            {
                _header.PlayAnimation();
                _reward.PlayAnimation(ClaimCoins);
                HapticsManager.Instance.SuccessVibrate();
                claimButton.interactable = true;
            });

            void ResetParameters()
            {
                _header.ResetValues();
                _reward.ResetValues();
                claimButton.transform.localScale = Vector3.zero;
                multiplierPanel.transform.localScale = Vector3.zero;
            }
        }

        public override void HideWindow()
        {
            base.HideWindow(() =>
            {
                claimButton.transform.localScale = Vector3.zero;
                for (int i = 0; i <= endGameStars.Count; i++)
                {
                    endGameStars[i].ResetStar();
                }
            });
        }

        private void ClaimCoins()
        {
            SetEarnings(StaticManager.moneyCollectedOnLevel, true);
            OnCongrats?.Invoke();
        }

        private float totalEarnedMoney;
        private void SetEarnings(int moneyValue, bool applyMultiplier)
        {
            if (moneyValue == 0)
                totalEarnedMoney = StaticManager.minMoneyCount;
            else if (moneyValue > 0)
                totalEarnedMoney = moneyValue;

            
            if (applyMultiplier && StaticManager.currentMoneyMultiplier > 1)
                TweenText(_reward.rewardLabel, totalEarnedMoney, 0.3f, ApplyMultiplier);
            else
            {
                TweenText(_reward.rewardLabel, totalEarnedMoney, 0.3f, DoMoneyFlow);
                StaticManager.moneyCollectedOnLevel = (int)totalEarnedMoney;
            }
        }

        [SerializeField] private Transform multiplierPanel;
        [SerializeField] private TMP_Text multiplierText;
        //[SerializeField] private ParticleSystem multiplierParticles;
        private void ApplyMultiplier()
        {
            multiplierText.text = "x" + StaticManager.currentMoneyMultiplier;
            totalEarnedMoney *= StaticManager.currentMoneyMultiplier;
            multiplierPanel.DOScale(1, 0.5f).SetEase(Ease.OutBounce).onComplete += () =>
            {
                TweenText(_reward.rewardLabel, totalEarnedMoney, 0.3f, DoMoneyFlow);
                //multiplierParticles.Play();
            };
            StaticManager.moneyCollectedOnLevel = (int)totalEarnedMoney;
        }

        private void DoMoneyFlow()
        {
            fXUIGenerator.MakeMoneyFX();
        }

        private void OnRewardMoneyReachedEarnings(FXUIGenerator fXUI)
        {
            fXUIGenerator.OnParticleReachedTargetEvent -= OnRewardMoneyReachedEarnings;
            float targetMoneyValue = StaticManager.Instance.playerData.money + totalEarnedMoney;
            TweenText(earningsMoneyCount, targetMoneyValue, 0.3f, ShowClaimButton);
        }

        private void ShowClaimButton()
        {
            claimButton.transform.DOScale(1, 0.5f).SetEase(Ease.OutBack);
        }

        private void TweenText(TMP_Text tMP, float targetValue, float smoothness, Action onComplete = null)
        {
            float currentValue = float.Parse(tMP.text);

            DOTween.To(() => currentValue, x => currentValue = x, targetValue, smoothness).OnUpdate(() =>
            {
                currentValue = (int)Mathf.Round(currentValue);
                tMP.text = currentValue.ToString();
            }).onComplete += () => onComplete?.Invoke();
        }
    }
}