using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brick : MonoBehaviour
{
    private KinematicBodyTrigger _bodyTrigger;

    private void Awake()
    {
        _bodyTrigger = GetComponentInParent<KinematicBodyTrigger>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && _bodyTrigger.hittedBrick == null)
        {
            _bodyTrigger.hittedBrick = this;
            _bodyTrigger.hittedBrick.GetComponent<Rigidbody>().AddExplosionForce(_bodyTrigger.explosionForce, _bodyTrigger.hittedBrick.transform.position, _bodyTrigger.explosionRadius, 1f, ForceMode.Impulse);
        }
    }
}
