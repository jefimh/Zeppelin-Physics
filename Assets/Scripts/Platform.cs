using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    [HideInInspector] public List<GameObject> platformContainers;

    BoxCollider boxCollider;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        platformContainers = new List<GameObject>();
    }

    public Bounds GetPlatformBounds()
    {
        return boxCollider.bounds;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Container"))
        {
            platformContainers.Add(collision.gameObject);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Container"))
        {
            platformContainers.Remove(collision.gameObject);
        }
    }
}
