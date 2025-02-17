using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropellerSound : MonoBehaviour
{
    [SerializeField] private float minimumPitch = 0.3f;
    [SerializeField] private float maximumPitch = 2f;
    [SerializeField] private Zeppelin zeppelinScript;
    [SerializeField] private Rigidbody zeppelinRigidbody;
    AudioSource audioSource;

    private float pitchModifier;
    private float maxSpeed = 40f;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.pitch = minimumPitch;
    }

    private void Update()
    {
        float currentSpeed = zeppelinRigidbody.velocity.magnitude;
        float soundPitch = minimumPitch + (currentSpeed / maxSpeed) * pitchModifier;
        soundPitch = Mathf.Clamp(soundPitch, 0, maximumPitch);

        pitchModifier = maximumPitch - minimumPitch;
        audioSource.pitch = soundPitch;
    }
}
