using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Deslab.Level;
using Lean.Touch;
//using Deslab.Scripts.Deslytics;

namespace Deslab.UI
{
    [Serializable]
    public struct UpgradePanel
    {
        public TMP_Text level;
        public TMP_Text cost;
    }

    [Serializable]
    public enum ScreenType
    {
        Menu,
        Game,
        Settings,
        Win,
        Lose
    }

    public class UIManager : Singleton<UIManager>
    {
        public LevelManager levelManager;
        [SerializeField] private List<CanvasGroupWindow> screens;
        public List<UpgradePanel> upgradePanels = new List<UpgradePanel>();

        public Action<ScreenType> OnScreenShowed;

        public CanvasGroupWindow ActiveScreen { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            
        }

        private void OnEnable()
        {
            StaticManager.OnWin += ShowWinScreen;
            StaticManager.OnLose += ShowLoseScreen;
            LevelManager.OnLevelLoaded += OnLevelLoadedHandler;
        }

        private void OnDisable()
        {
            StaticManager.OnWin -= ShowWinScreen;
            StaticManager.OnLose -= ShowLoseScreen;
            LevelManager.OnLevelLoaded -= OnLevelLoadedHandler;
        }

        /// <summary>
        /// Start game by click UI element
        /// </summary>
        public void StartGame()
        {
            StaticManager.reloadLevel = false;
            ShowScreen(ScreenType.Game);
            levelManager.StartLevel();
            ShowTutorial();

            //DeslyticsManager.LevelStart(StaticManager.levelID);
        }

        private void OnLevelLoadedHandler(Level.Level level)
        {
            ShowOnlyOneScreen(ScreenType.Menu);
        }

        /// <summary>
        /// Shows screen 'screenType'
        /// </summary>
        public void ShowScreen(ScreenType screenType, bool hideActiveScreen = true, bool disableActiveScreen = false, Action onShowCompleted = null, Action onHideCompleted = null)
        {
            if (ActiveScreen != null)
            {
                if (hideActiveScreen)
                    ActiveScreen.HideWindow(onHideCompleted);

                if (disableActiveScreen)
                    ActiveScreen.DisableWindow();
            }
            
            foreach (CanvasGroupWindow screen in screens)
            {
                if (screen.ScreenType == screenType)
                {
                    screen.ShowWindow(onShowCompleted);
                    ActiveScreen = screen;
                    return;
                }
            }
        }

        /// <summary>
        /// Shows screen 'screenType' and hides all other screens
        /// </summary>
        private void ShowOnlyOneScreen(ScreenType screenType, Action onShowCompleted = null, Action onHideCompleted = null)
        {
            foreach (CanvasGroupWindow screen in screens)
            {
                if (screen.ScreenType == screenType)
                {
                    ShowScreen(screenType, onShowCompleted: onShowCompleted);
                    OnScreenShowed?.Invoke(screenType);
                }
                else
                {
                    screen.HideWindow(onHideCompleted);
                }
            }
        }

        public void ShowGameScreen()
        {
            ShowScreen(ScreenType.Game);
        }

        public void ShowSettingsScreen()
        {
            ShowScreen(ScreenType.Settings, false, true);
        }

        /// <summary>
        /// Show Win Screen after complete level
        /// </summary>
        public void ShowWinScreen()
        {
            ShowScreen(ScreenType.Win);
        }

        /// <summary>
        /// Ident to Win Screen but lose
        /// </summary>
        public void ShowLoseScreen()
        {
            ShowScreen(ScreenType.Lose);
            //DeslyticsManager.LevelFailed();
        }

        //////////////////////////////////
        //
        //  That region for tutorial objects.
        //
        //////////////////////////////////

        public GameObject tutorial;
        public void HideTutorial(LeanFinger finger)
        {
            if (tutorial != null)
            {
                tutorial.SetActive(false);
                LeanTouch.OnFingerUp -= HideTutorial;
            }
        }

        public void ShowTutorial()
        {
            if (tutorial != null)
            {
                tutorial.SetActive(true);
                LeanTouch.OnFingerUp += HideTutorial;
            }
        }

        /// <summary>
        /// Upgrade some Player stats by unique upgrade ID
        /// </summary>
        /// <param name="upgradeID"></param>
        public void Upgrade(int upgradeID)
        {
            PlayerUpgradeData pUpgradeData = StaticManager.Instance.playerData.playerUpgradeData[upgradeID];
            if (StaticManager.Instance.playerData.money >= pUpgradeData.upgradeCost)
            {
                StaticManager.SubstractMoney(pUpgradeData.upgradeCost);
                pUpgradeData.Upgrade();
                upgradePanels[upgradeID].cost.text = pUpgradeData.upgradeCost.ToString();
                upgradePanels[upgradeID].level.text = "LVL " + pUpgradeData.upgradeLevel;

                StaticManager.Instance.playerData.playerUpgradeData[upgradeID] = pUpgradeData;
                StaticManager.Instance.SavePlayerData();
            }
        }

        /// <summary>
        /// Set value from loaded Player Stats to Upgrade UI elements
        /// </summary>
        /// <param name="playerUpgradeData"></param>
        public void SetUpgradesStats(List<PlayerUpgradeData> playerUpgradeData)
        {
            List<PlayerUpgradeData> pUpgradeData = playerUpgradeData;
            for (int i = 0; i < pUpgradeData.Count; i++)
            {
                upgradePanels[i].cost.text = pUpgradeData[i].upgradeCost.ToString();
                upgradePanels[i].level.text = "LVL " + pUpgradeData[i].upgradeLevel;
            }
        }

        //For debugging
        public void OnWin()
        {
            StaticManager.OnWin?.Invoke();
        }

        public void OnLose()
        {
            StaticManager.OnLose?.Invoke();
        }
    }

    #region InDev


    //[Serializable]
    //public class LevelStars
    //{
    //    public Image starImage;
    //    Sequence starSequence;
    //    public void ShowStar()
    //    {
    //        starSequence = DOTween.Sequence();

    //        starSequence.Append(starImage.transform.transform.DOLocalMoveY(-650, 1.3f)
    //                                                         .SetEase(Ease.InBack))
    //            .Join(
    //                    starImage.transform.DOLocalRotate(new Vector3(0, 359, 0), 0.7f, RotateMode.FastBeyond360)
    //                                       .SetLoops(-1))
    //            .Join(
    //                    starImage.DOFade(0, 0.7f)
    //                             .SetDelay(0.8f));
    //    }

    //    public void ResetStar()
    //    {
    //        starSequence.Kill();
    //        starImage.color = new Color32(255, 255, 255, 255);
    //        starImage.transform.localRotation = Quaternion.Euler(Vector3.zero);
    //        starImage.transform.localPosition = new Vector3(starImage.transform.localPosition.x, -36, 0);
    //    }
    //}
    #endregion
}
