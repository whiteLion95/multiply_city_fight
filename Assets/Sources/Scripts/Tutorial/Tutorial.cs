using Deslab.Level;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : TutorialCore
{
    [SerializeField] private int myLevelID;

    protected override void Awake()
    {
        base.Awake();
        LevelManager.OnLevelLoaded += HandlerOnLevelLoaded;
    }

    private void OnDestroy()
    {
        LevelManager.OnLevelLoaded -= HandlerOnLevelLoaded;
    }

    private void HandlerOnLevelLoaded(Level level)
    {
        if (StaticManager.levelID == myLevelID)
        {
            gameObject.SetActive(true);
            Init();
        }
        else if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    protected virtual void Init() 
    {
        Instance = this;
    }
}