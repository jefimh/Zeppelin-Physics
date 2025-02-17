using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI altitudeText;
    [SerializeField] private TextMeshProUGUI massText;
    [SerializeField] private TextMeshProUGUI downwardForceText;
    [SerializeField] private TextMeshProUGUI liftPowerText;
    [SerializeField] private TextMeshProUGUI resultantForceText;
    [SerializeField] private TextMeshProUGUI volumeText;

    [SerializeField] private Zeppelin zeppelinScript;
    [SerializeField] private Transform zeppelinTransform;
    [SerializeField] private Rigidbody zeppelinRigidbody;

    private float zeppelinMass;

    private void Start()
    {
        zeppelinScript = zeppelinScript.GetComponent<Zeppelin>();
        zeppelinTransform = zeppelinTransform.GetComponent<Transform>();
        zeppelinRigidbody = zeppelinRigidbody.GetComponent<Rigidbody>();

        altitudeText = altitudeText.GetComponent<TextMeshProUGUI>();
        massText = massText.GetComponent<TextMeshProUGUI>();
        liftPowerText = liftPowerText.GetComponent<TextMeshProUGUI>();
        downwardForceText = downwardForceText.GetComponent<TextMeshProUGUI>();
        resultantForceText = resultantForceText.GetComponent<TextMeshProUGUI>();
        volumeText = volumeText.GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        float zeppelinY = zeppelinTransform.position.y;
        float zeppelinMass = (zeppelinRigidbody.mass);
        float zeppelinLiftPower = zeppelinScript.liftPower.magnitude;
        float zeppelinDownwardForce = zeppelinScript.downwardForce.magnitude;
        float resultantForce = zeppelinScript.resultant.magnitude;
        float volume = zeppelinScript.volume;

        altitudeText.text = "Altitud: " + zeppelinY.ToString("n") + " M";
        massText.text = "Massa: " + zeppelinMass.ToString("n") + " KG";
        liftPowerText.text = "Lyftkraft: " + zeppelinLiftPower.ToString("n") + " N";
        downwardForceText.text = "Nedåtgående kraft: " + zeppelinDownwardForce.ToString("n") + " N";
        resultantForceText.text = "Resultantkraft: " + resultantForce.ToString("n") + " N";
        volumeText.text = "Volym: " + volume.ToString("n") + " m<sup>3</sup>";
    }
}
