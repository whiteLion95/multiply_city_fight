using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A base class for cam focuses
/// </summary>
public class CamFocusCore : MonoBehaviour
{
    protected Transform target;
    private bool _toFollow;

    protected virtual void Update()
    {
        if (_toFollow)
        {
            FollowTargetPos(target);
        }
    }

    protected void StartFollowing(Transform target)
    {
        this.target = target;
        _toFollow = true;
    }

    protected void StopFollowing()
    {
        _toFollow = false;
    }

    protected void FollowTargetPos(Transform target, bool x = true, bool y = true, bool z = true)
    {
        if (target != null)
        {
            if (x && y && z)
            {
                transform.position = target.position;
            }
            else if (!z)
            {
                transform.position = new Vector3(target.position.x, target.position.y);
            }
            else if (!y)
            {
                transform.position = new Vector3(target.position.x, 0, target.position.z);
            }
            else
            {
                transform.position = new Vector3(0, target.position.y, target.position.z);
            }
        }
    }
}
