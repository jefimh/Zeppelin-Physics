using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine : MonoBehaviour
{
    [SerializeField]
    private bool isLeftEngine = false;
    [SerializeField]
    float speed = 100f;
    Zeppelin zeppelinScript;
    Rigidbody rigidbody;
    [SerializeField]
    private GameObject zeppelin;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isLeftEngine)
        {
            if (Input.GetKey(KeyCode.D))
            {
                rigidbody.AddForce(Vector3.forward * speed, ForceMode.Force);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                rigidbody.AddForce(Vector3.forward * speed, ForceMode.Force);
            }
        }
    }
}
