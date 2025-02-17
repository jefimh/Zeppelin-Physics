using System.Collections;
using UnityEngine;

public class Zeppelin : MonoBehaviour
{
    Rigidbody rigidbody;
    [SerializeField] Transform massCenter;
    [SerializeField] Platform[] platformScript = new Platform[2];
    [SerializeField] float thrustForce = 100f;
    public float yawForce = 100f;
    [SerializeField] float containerAttachForce = 100f;
    [SerializeField] Transform containerAttachPoint;
    private float zeppelinLengthScaleFactor = 0.3347f;
    public Transform platformWhereContainerGotAttached;
    Autopilot autopilotScript;
    Vector3 inputForce;
    [SerializeField] private PauseMenu pauseMenuScript;

    private GameObject attachedContainer = null;

    [HideInInspector] public bool isAttached;

    private bool isRecentlyDetached = false;
    private bool hasDownwardForceChanged = false;
    public bool isMovingVertically = false;
    public bool isMovingUpward = false;

    private float zeppelinWidth = 82f;
    private float zeppelinHeight = 82f;
    private float zeppelinLength = 245f;

    const float speedMultiplier = 10f;
    const float maxSpeed = 45.7f;

    public float volume;
    float envelopeMass;
    float skeletonMass;

    [HideInInspector] public Vector3 liftPower;
    [HideInInspector] public Vector3 downwardForce;
    [HideInInspector] public Vector3 resultant;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        autopilotScript = GetComponent<Autopilot>();

        CalculateZeppelinScale(862576f);
        CalculateLiftPower();
        CalculateEnvelopeMass();
        CalculateSkeletonMass();
        SetZeppelinDownwardForce();
        CalculateInputForce();
        SetAttachPointPosition();
        pauseMenuScript.SetVolumeSlider(volume);
    }

    private void Update()
    {
        CalculateLiftPower();

        if (!autopilotScript.isAutopilotEnabled)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                isMovingVertically = true;
            }
            else if (Input.GetKeyUp(KeyCode.S))
            {
                isMovingVertically = false;
            }

            if (isMovingVertically && Input.GetKey(KeyCode.LeftShift))
            {
                isMovingUpward = true;
            }
            else
            {
                isMovingUpward = false;
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                DetachContainer();
            }
        }
    }

    //Används för fysiksimuleringar eftersom det är synkroniserat med Unitys fysikmotor. 
    void FixedUpdate()
    {
        HandleVerticalMovement();

        if (!autopilotScript.isAutopilotEnabled)
        {
            Throttle();
            Yaw();
        }
    }


    //Exekveras sist av alla andra updatemetoder
    private void LateUpdate()
    {
        //Förhindra rotation på x -och z-axeln. 
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
    }

    void CalculateInputForce()
    {
        float y = rigidbody.mass * 100;
        inputForce = new Vector3(0, y, 0);
    }

    void HandleVerticalMovement()
    {
        resultant = liftPower - downwardForce;

        if (attachedContainer != null && !hasDownwardForceChanged)
        {
            float containerDownwardForce = attachedContainer.GetComponent<Rigidbody>().mass * 9.81f;
            downwardForce += new Vector3(0, containerDownwardForce, 0);
            hasDownwardForceChanged = true;
        }

        if (isMovingVertically && !isMovingUpward)
        {
            resultant -= inputForce;
        }
        else if (isMovingVertically && isMovingUpward)
        {
            resultant += inputForce;
        }

        rigidbody.AddForce(resultant, ForceMode.Force);
    }

    void LimitSpeed()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(rigidbody.velocity);
        localVelocity.z = Mathf.Clamp(localVelocity.z, -maxSpeed, maxSpeed);
        rigidbody.velocity = transform.TransformDirection(localVelocity);
    }

    void SetZeppelinDownwardForce()
    {
        float totalMass = (envelopeMass + skeletonMass);
        Debug.Log("Total mass: " + totalMass);
        rigidbody.mass = totalMass;
        downwardForce = new Vector3(0, (totalMass * 9.81f), 0);
    }

    //Åk fram
    void Throttle()
    {
        if (Input.GetKey(KeyCode.W))
        {
            AddForwardForce(Vector3.forward);
        }
    }

    public void AddForwardForce(Vector3 direction)
    {
        rigidbody.AddRelativeForce(direction * thrustForce * speedMultiplier * (rigidbody.mass / 1000));
        LimitSpeed();
    }

    public float GetForwardForceValue()
    {
        return thrustForce * speedMultiplier * (rigidbody.mass / 1000);
    }

    //Sväng vänster/höger
    void Yaw()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        rigidbody.AddTorque(transform.up * horizontalInput * yawForce * speedMultiplier * (rigidbody.mass / 1000));
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Container") && attachedContainer == null && !isRecentlyDetached)
        {
            AttachContainer(other.attachedRigidbody.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Container") && attachedContainer == null && !isRecentlyDetached)
        {
            AttachContainer(other.attachedRigidbody.gameObject);
        }
    }

    public void AttachContainer(GameObject container)
    {
        attachedContainer = container;
        platformWhereContainerGotAttached = autopilotScript.GetNearestContainerPlatform();
        Rigidbody containerRigidbody = container.GetComponent<Rigidbody>();

        if (containerRigidbody != null)
        {
            StartCoroutine(ApplyMagneticForce(containerRigidbody));
        }
    }

    private IEnumerator ApplyMagneticForce(Rigidbody containerRigidbody)
    {
        containerRigidbody.useGravity = false;
        containerRigidbody.isKinematic = true;

        float duration = 1.0f;
        float timePassed = 0.0f;

        Vector3 startPosition = containerRigidbody.position;
        Quaternion startRotation = containerRigidbody.rotation;

        Quaternion targetRotation = Quaternion.LookRotation(containerAttachPoint.forward, containerAttachPoint.up);

        while (timePassed < duration)
        {
            timePassed += Time.deltaTime;
            float t = timePassed / duration;
            float easedT = Mathf.SmoothStep(0, 1, t);

            containerRigidbody.position = Vector3.Lerp(startPosition, containerAttachPoint.position, easedT);
            containerRigidbody.rotation = Quaternion.Slerp(startRotation, targetRotation, easedT);

            yield return null;
        }

        attachedContainer.transform.SetParent(containerAttachPoint);

        containerRigidbody.transform.localPosition = Vector3.zero;
        containerRigidbody.transform.localRotation = Quaternion.identity;
    }

    public void DetachContainer()
    {
        if (attachedContainer != null)
        {
            attachedContainer.transform.SetParent(null);

            Rigidbody containerRigidbody = attachedContainer.GetComponent<Rigidbody>();
            if (containerRigidbody != null)
            {
                containerRigidbody.isKinematic = false;
            }

            attachedContainer = null;

            containerRigidbody.useGravity = true;
            isRecentlyDetached = true;
            downwardForce -= new Vector3(0, containerRigidbody.mass * 9.81f, 0);
            hasDownwardForceChanged = false;
            platformWhereContainerGotAttached = null;

            StartCoroutine(ResetRecentlyDetachedAfterDelay(3f));
        }
    }

    private IEnumerator ResetRecentlyDetachedAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isRecentlyDetached = false;
    }

    void CalculateLiftPower()
    {
        float radiusX = zeppelinWidth / 2f;
        float radiusY = zeppelinHeight / 2f;
        float radiusZ = zeppelinLength / 2f;

        volume = GetEllipsoidVolume(radiusX, radiusY, radiusZ);

        float yPosition = transform.position.y;
        float powerAir = (volume * (101325 * Mathf.Pow(1f - (yPosition * 0.0000225577f), 5.2559f)) / (287.05f * (20 + 273.15f))) * 9.81f;
        float powerHydrogen = volume * 0.0815f * 9.81f; 
        float liftPowerY = powerAir - powerHydrogen;

        liftPower = new Vector3(0, liftPowerY, 0);
    }

    //void LockVolumeCalculator()
    //{
    //    float zeppelinScaleX = transform.localScale.x;
    //    float zeppelinScaleY = transform.localScale.y;
    //    float zeppelinScaleZ = transform.localScale.z;

    //    const float scaleConstantX = 0.00463f;
    //    const float scaleConstantY = 0.00146f;
    //    const float scaleConstantZ = 0.00114f;

    //    float lockObjectScaleX = scaleConstantX * zeppelinScaleX;
    //    float lockObjectScaleY = scaleConstantY * zeppelinScaleY;
    //    float lockObjectScaleZ = scaleConstantZ * zeppelinScaleZ;

    //    lockObject.transform.localScale = new Vector3(lockObjectScaleX, lockObjectScaleY, lockObjectScaleZ);
    //}

    public GameObject GetAttachedContainer()
    {
        return attachedContainer;
    }

    void CalculateEnvelopeMass()
    {
        float envelopeThickness = 0.002f;
        float radiusX = (zeppelinWidth / 2f) - envelopeThickness;
        float radiusY = (zeppelinHeight / 2f) - envelopeThickness;
        float radiusZ = (zeppelinLength / 2f) - envelopeThickness;
        float tempVolume = GetEllipsoidVolume(radiusX, radiusY, radiusZ);
        float polyMylar = 1390f;

        envelopeMass = (volume - tempVolume)  * polyMylar;
        Debug.Log("Envelope Mass: " + envelopeMass);
    }

    float GetEllipsoidVolume(float radiusX, float radiusY, float radiusZ)
    {
        return (4 / 3f) * Mathf.PI * radiusX * radiusY * (radiusY / zeppelinLengthScaleFactor);
    }

    public void CalculateZeppelinScale(float volume)
    {
        float numerator = volume * zeppelinLengthScaleFactor;
        float denominator = (4f / 3f) * Mathf.PI;
        zeppelinWidth = Mathf.Pow((numerator / denominator), 1f/3f) * 2;
        zeppelinHeight = zeppelinWidth;
        zeppelinLength = zeppelinHeight / zeppelinLengthScaleFactor;
        float visualLength = zeppelinHeight;

        CalculateLiftPower();
        CalculateEnvelopeMass();
        CalculateSkeletonMass();
        SetZeppelinDownwardForce();
        CalculateInputForce();
        SetAttachPointPosition();

        transform.localScale = new Vector3(zeppelinWidth, zeppelinHeight, visualLength);
    }

    public void ChangeContainerMass(float mass)
    {
        foreach (var script in platformScript)
        {
            foreach (var container in script.platformContainers)
            {
                container.GetComponent<Rigidbody>().mass = mass;
                Debug.Log("Container mass: " + container.GetComponent<Rigidbody>().mass);
            }
        }

        if(GetAttachedContainer() != null)
        {
            attachedContainer.GetComponent<Rigidbody>().mass = mass;
            Debug.Log("Container mass: " + attachedContainer.GetComponent<Rigidbody>().mass);
        }
    }

    void SetAttachPointPosition()
    {
        float fixedDistance = 11f;
        float scalingFactor = fixedDistance / transform.localScale.y;

        Vector3 newPosition = new Vector3(0, -scalingFactor, 0);
        containerAttachPoint.localPosition = newPosition;
    }

    void CalculateSkeletonMass()
    {
        //Aluminium
        float duralumDensity = 2710f;

        skeletonMass = volume * duralumDensity * 0.0000838f;
        Debug.Log("Skeleton Mass: " + skeletonMass);
    }
}
