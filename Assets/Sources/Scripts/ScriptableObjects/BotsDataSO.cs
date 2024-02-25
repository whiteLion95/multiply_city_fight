using UnityEngine;

[CreateAssetMenu(fileName = "BotsData", menuName = "ScriptableObjects/BotsData")]
public class BotsDataSO : ScriptableObject
{
    [SerializeField] [Tooltip("Задержка перед тем, как начинать действовать")] private float delayBeforeActions = 1f;
    [SerializeField] [Tooltip("Задержка перед следующим действием")] private float delayBeforeNextAction = 1f;
    [SerializeField] [Tooltip("Множитель на который умножаются задержки в туториалах")] private float tutorialKoeff = 1.5f;
    [SerializeField] [Tooltip("После какого уровня ускорить ботов")] private int lastEasyLevel = 8;
    [SerializeField] [Tooltip("Множитель на который умножаются задержки после последнего лёгкого уровня")] private float delayHardKoeff = 0.5f;
    [SerializeField] [Tooltip("Множитель на который умножается базовая длительность спавна ботов после последнего лёгкого уровня")] private float spawnHadrKoeff = 2f;
    [SerializeField] [Tooltip("Погрешность задержки действий ботов в процентах")] [Range(0f, 1f)] private float actionsInaccuracy = 0.1f;
    [Space(20f)]
    [SerializeField] [Tooltip("Объекты, которые могут служить целями для ботов. Чем выше объект в списке, тем выше его приоритет")] 
    private TargetObject[] targetObjects;

    private float GetDelay(float originDelay)
    {
        float tempKoeff = 1;

        if (TutorialLevel.Instance != null)
            tempKoeff = tutorialKoeff;
        if (StaticManager.levelID > lastEasyLevel)
            tempKoeff = delayHardKoeff;

        return originDelay * tempKoeff;
    }

    public int GetPriority(TargetObject targetObject)
    {
        int priority = -1;

        for (int i = 0; i < targetObjects.Length; i++)
        {
            if (targetObjects[i] == targetObject)
                priority = i;
        }

        return priority;
    }

    public float DelayBeforeActions { get => GetDelay(delayBeforeActions); }
    public float DelayBeforeNextAction { get => GetDelay(delayBeforeNextAction); }
    public TargetObject[] TargetObjects { get => targetObjects; }
    public int LastEasyLevel { get => lastEasyLevel; }
    public float SpawnHardKoeff { get => spawnHadrKoeff; }
    public float ActionsInaccuracy { get => actionsInaccuracy; }
}

public enum TargetObject
{
    EnemyTower,
    NeutralTower,
    Multiplier
}
