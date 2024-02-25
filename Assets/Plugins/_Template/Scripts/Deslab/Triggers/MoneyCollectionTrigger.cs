using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Deslab.Level
{
    public class MoneyCollectionTrigger : CollectionTrigger
    {
        [SerializeField] private int moneyCount = 10;

        protected override void ActionsOnTrigger()
        {
            StaticManager.AddMoneyOnLevel(moneyCount);
            gameObject.SetActive(false);
        }
    }
}
