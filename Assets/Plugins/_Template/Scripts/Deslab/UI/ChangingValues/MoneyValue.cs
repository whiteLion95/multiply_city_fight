using DG.Tweening;
using TMPro;
using UnityEngine;

public class MoneyValue : MonoBehaviour
{
    public bool collectedOnLevel = false;
    public TMP_Text MoneyText;
    int currentMoney;

    private void Start()
    {
        StaticManager.onMoneyChange += SetMoneyValue;
        StaticManager.onRestart += Restart;

        if (!collectedOnLevel)
            currentMoney = StaticManager.Instance.playerData.money;
        else
            currentMoney = StaticManager.moneyCollectedOnLevel;
        MoneyText.text = currentMoney + "";
    }

    public void SetMoneyValue()
    {
        if (!collectedOnLevel)
        {
            MoneyText.text = StaticManager.Instance.playerData.money.ToString();
        }
        else
        {
            if (StaticManager.Instance.gameStatus != GameStatus.Menu)
                DOTween.To(() => currentMoney, x => currentMoney = x, StaticManager.moneyCollectedOnLevel, 0.6f)
                .OnUpdate(() => MoneyText.text = currentMoney + "");
        }
    }

    public void Restart()
    {
        currentMoney = 0;
        if (collectedOnLevel)
            MoneyText.text = "0";
    }
}
