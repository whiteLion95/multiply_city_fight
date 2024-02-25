using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TutorialCore : MonoBehaviour
{
    [SerializeField] [Tooltip("Canvas Group который блокируется при воспроизведении шагов туториала")]
    protected CanvasGroup blockCanvasGroup;

    protected TutorialStep[] tutorialSteps;
    protected int curStep = -1;

    public static Action OnTutorialCompleted;
    public static Action<int> OnTutorialStepPlay;
    public static Action OnTutorialStepHide;
    public static TutorialCore Instance;

    public int CurStep { get { return curStep; } }
    public bool Completed { get; private set; }

    protected virtual void Awake()
    {
        tutorialSteps = GetComponentsInChildren<TutorialStep>(true);
    }

    protected virtual void Start()
    {
        TurnOffAllSteps();
    }

    private void TurnOffAllSteps()
    {
        foreach (var step in tutorialSteps)
        {
            step.gameObject.SetActive(false);
        }
    }

    protected void PlayNextStep()
    {
        if (curStep < (tutorialSteps.Length - 1))
        {
            curStep++;
            tutorialSteps[curStep].DoStep();
            OnTutorialStepPlay?.Invoke(curStep);

            if (curStep > 0)
                tutorialSteps[curStep - 1].gameObject.SetActive(false);
        }
        else
        {
            OnTutorialCompleted?.Invoke();
            Completed = true;
            HideCurrentStep();
        }
    }

    public void HideCurrentStep()
    {
        tutorialSteps[curStep].gameObject.SetActive(false);
        OnTutorialStepHide?.Invoke();
    }
}
