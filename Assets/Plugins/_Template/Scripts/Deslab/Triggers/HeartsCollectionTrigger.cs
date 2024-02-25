using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartsCollectionTrigger : CollectionTrigger
{
    //[SerializeField] private int regenAmount = 20;

    protected override void ActionsOnTrigger()
    {
        //Player.OnHealthChanged?.Invoke(regenAmount);
    }
}
