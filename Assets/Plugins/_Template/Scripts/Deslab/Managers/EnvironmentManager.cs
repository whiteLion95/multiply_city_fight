using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Deslab.Level;
using Arystan.Saving;
using System;
using Deslab.Utils;
using Deslab.UI;

/// <summary>
/// С запуска игры нужно менять окружение по порядку
/// После того, как пришло время менять последенее окружение по порядку, начинать менять оркужения рандомно без повтора
/// Следующее окружение не может равняться текущему окружению
/// Необходимо сохранять данные о текущем окружении и о том, менять ли окружения рандомно
/// Процесс применения окружения:
///     поменять цвета для каждого меняющегося материала
///     заспавнить объекты окружения и применить им родителя, указанного в уровне
/// </summary>
public class EnvironmentManager : Singleton<EnvironmentManager>, ISaveable
{
    [SerializeField] private EnvironmentDataSO[] environmentsData;
    [SerializeField] [Tooltip("Окружение с каким минимальным индексом загружать при рандомной загрузке")] private int minEnvironmentIndex;

    public static Action OnEnvironmentSet;

    [Serializable]
    private struct SavingData
    {
        public int currentEnvironmentIndex;
        public bool randomEnvironments;
    }

    private SavingData _savingData;
    private SavingManager _savingManager;
    private RandomNoRepeate _rndNoReapeate;

    protected override void Awake()
    {
        base.Awake();

        InitRandNoRepeat();

        LevelManager.OnLevelLoaded += HandlerOnLevelLoaded;
        WinScreen.OnRewardsClaim += HandlerOnRewardsClaim;
    }

    private void Start()
    {
        AddMeToSavingManager();
    }

    private void OnDisable()
    {
        LevelManager.OnLevelLoaded -= HandlerOnLevelLoaded;
        WinScreen.OnRewardsClaim -= HandlerOnRewardsClaim;

    }

    private void InitRandNoRepeat()
    {
        _rndNoReapeate = new RandomNoRepeate();
        _rndNoReapeate.SetCount(environmentsData.Length);
    }

    #region Event Handlers
    private void HandlerOnLevelLoaded(Level level)
    {
        EnvironmentDataSO curEData = environmentsData[_savingData.currentEnvironmentIndex];
        curEData.ApplyColorScheme();

        if (curEData.Environment != null)
            Instantiate(curEData.Environment, Vector3.zero, Quaternion.identity, level.EnvironmentParent);
        
        SaveData();
        OnEnvironmentSet?.Invoke();
    }

    private void HandlerOnRewardsClaim()
    {
        if (!StaticManager.Instance.debugMode)
        {
            if (!_savingData.randomEnvironments)
            {
                _savingData.currentEnvironmentIndex++;

                if (_savingData.currentEnvironmentIndex == (environmentsData.Length - 1))
                {
                    _savingData.randomEnvironments = true;
                }
            }
            else
            {
                int randId = _rndNoReapeate.GetAvailableExcept(_savingData.currentEnvironmentIndex, minEnvironmentIndex);
                _savingData.currentEnvironmentIndex = randId;
            }
        }
    }
    #endregion

    #region SavingLoading
    private const string SAVING_KEY = "Environments Data";

    public void SaveData()
    {
        _savingManager.Save(SAVING_KEY, _savingData);
    }

    public void LoadData()
    {
        _savingData = _savingManager.Load<SavingData>(SAVING_KEY);
    }

    public void AddMeToSavingManager()
    {
        _savingManager = SavingManager.Instance;
        _savingManager.AddToSaveableObjects(this);
        LoadData();
    }
    #endregion
}
