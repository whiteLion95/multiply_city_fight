using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class KinematicBodyTrigger : MonoBehaviour
{
    public List<Rigidbody> rigidbodies = new List<Rigidbody>();
    public List<Collider> colliders = new List<Collider>();
    public float explosionForce;
    public float explosionRadius;

    [HideInInspector] public Brick hittedBrick;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            for (int i = 0; i < rigidbodies.Count; i++)
            {
                colliders[i].isTrigger = false;
                rigidbodies[i].useGravity = true;
                rigidbodies[i].isKinematic = false;
            }
            StartCoroutine(DisableObject());
        }
    }

    IEnumerator DisableObject()
    {
        yield return new WaitForSeconds(3.5f);
        gameObject.SetActive(false);
    }
}
