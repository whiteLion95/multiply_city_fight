using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CollectionTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ActionsOnTrigger();
        }
    }

    /// <summary>
    /// Actions needed to be done when OnTriggerEnter is being called
    /// </summary>
    protected abstract void ActionsOnTrigger();
}
