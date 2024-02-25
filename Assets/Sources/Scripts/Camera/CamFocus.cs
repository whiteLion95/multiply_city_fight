using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFocus : CamFocusCore
{
    private void Awake()
    {
        //Player.OnLoaded += OnPlayerLoadedHandler;
    }

    private void OnPlayerLoadedHandler()
    {
        //StartFollowing(Player.Instance.transform);
    }
}
