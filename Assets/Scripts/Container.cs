using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
    private float containerMass = 1000f;
    Rigidbody rigidbody;
    Zeppelin zeppelinScript;

    public void ApplyContainerPhysics()
    {
        rigidbody = GetComponent<Rigidbody>();
        Vector3 containerDownwardForce = new Vector3(0, containerMass * 9.81f, 0);
        zeppelinScript.resultant += containerDownwardForce;
    }
}
