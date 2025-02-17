using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float cameraSensitivity = 3.0f;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float positionLerp = 0.01f;
    [SerializeField] private float rotationLerp = 0.02f;

    private Vector3 cameraOffset = new Vector3(0, 50, -100);
    private Vector3 turn;

    private float rotationX;
    private float rotationY;

    void Update()
    {
        //CameraFollow();
        CameraRotation();
    }

    void CameraRotation()
    {
        turn.x += Input.GetAxis("Mouse X") * cameraSensitivity;
        turn.y += Input.GetAxis("Mouse Y") * cameraSensitivity;
        transform.localRotation = Quaternion.Euler(-turn.y, turn.x, 0);
    }

    void CameraFollow()
    {
        transform.position = Vector3.Lerp(transform.position, cameraTarget.position + cameraOffset, positionLerp);
        transform.rotation = Quaternion.Lerp(transform.rotation, cameraTarget.rotation, rotationLerp);
    }
}
