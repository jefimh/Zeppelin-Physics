using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Autopilot : MonoBehaviour
{
    [SerializeField] private Zeppelin zeppelinScript;
    [SerializeField] private Transform platformA;
    [SerializeField] private Transform platformB;
    [SerializeField] private GameObject emptySpotGameObject;
    [SerializeField] private float minimumDistanceBetweenCargos = 40f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float zeppelinMinimumAscendAltitude = 500f;
    [SerializeField] private float minimumDistanceToPlatformBorder = 5f;
    [SerializeField] private LineRenderer lineRenderer;

    private Transform currentDestination;
    private GameObject lastDeliveredContainer = null;

    [HideInInspector] public bool isAutopilotEnabled = false;

    private enum AutopilotBehaviourState
    {
        FlyingToDestination,
        Descending,
        Docking,
        Ascending,
        RotatingTowardsTarget
    }

    private AutopilotBehaviourState currentState;

    /// <summary>
    /// Logik som exekveras en g�ng per bildruta, och lyssnar efter ifall 
    /// anv�ndare interagerade med "TAB" tangenten. Ifall den har det,
    /// s� aktiveras autopiloten. Om den redan var aktiverad, s� deaktiveras autopiloten. 
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isAutopilotEnabled = !isAutopilotEnabled;

            if (isAutopilotEnabled)
            {
                if (zeppelinScript.GetAttachedContainer() == null)
                {
                    Transform nearestPlatform = GetNearestContainerPlatform();
                    currentDestination = GetRandomContainerOnPlatform(nearestPlatform);
                }
                else if (zeppelinScript.GetAttachedContainer() != null)
                {
                    if(zeppelinScript.platformWhereContainerGotAttached.name == "PlatformA")
                    {
                        currentDestination = FindEmptyContainerSpotOnPlatform(platformB);
                    }
                    else
                    {
                        currentDestination = FindEmptyContainerSpotOnPlatform(platformA);

                    }
                }

                currentState = AutopilotBehaviourState.RotatingTowardsTarget;
            }
            else
            {
                zeppelinScript.isMovingUpward = false;
                zeppelinScript.isMovingVertically = false;
            }
        }

        if (isAutopilotEnabled && currentDestination != null)
        {
            VisualizeZeppelinRouteToDestination();
        }
        else
        {
            lineRenderer.positionCount = 0;
        }
    }

    private void VisualizeZeppelinRouteToDestination()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, zeppelinScript.transform.position);
        lineRenderer.SetPosition(1, currentDestination.position);
    }

    private void FixedUpdate()
    {
        if (isAutopilotEnabled)
        {
            HandleAutopilot();
        }
    }

    /// <summary>
    /// Huvudlogiken - anropar till respektive metod beroende p� l�get som AI-zeppelinaren befinner sig i.
    /// </summary>
    private void HandleAutopilot()
    {
        switch (currentState)
        {
            case AutopilotBehaviourState.FlyingToDestination:
                FlyZeppelinToTarget();
                break;
            case AutopilotBehaviourState.Descending:
                DescendZeppelin();
                break;
            case AutopilotBehaviourState.Docking:
                PickUpContainer();
                break;
            case AutopilotBehaviourState.Ascending:
                AscendZeppelin();
                break;
            case AutopilotBehaviourState.RotatingTowardsTarget:
                RotateZeppelin();
                break;
        }
    }

    /// <summary>
    /// Hitta den n�rmsta platformen till zeppelinaren. 
    /// </summary>
    /// 
    /// <returns>
    /// Returnerar en Unity inbyggd transform-komponent som inneh�ller platformens koordinater
    /// </returns>
    public Transform GetNearestContainerPlatform()
    {
        Transform nearestFoundPlatform;
        float distanceToPlatformA = Vector3.Distance(zeppelinScript.transform.position, platformA.position);
        float distanceToPlatformB = Vector3.Distance(zeppelinScript.transform.position, platformB.position);

        if(distanceToPlatformA < distanceToPlatformB)
        {
            nearestFoundPlatform = platformA;
        }
        else
        {
            nearestFoundPlatform = platformB;
        }

        return nearestFoundPlatform;
    }

    /// <summary>
    /// Hitta den l�ngsta platformen till zeppelinaren. 
    /// </summary>
    /// 
    /// <returns>
    /// Returnerar en Unity inbyggd transform-komponent som inneh�ller plattformens koordinater
    /// </returns>
    private Transform GetFurthestContainerPlatform()
    {
        Transform furthestFoundPlatform;
        float distanceToPlatformA = Vector3.Distance(zeppelinScript.transform.position, platformA.position);
        float distanceToPlatformB = Vector3.Distance(zeppelinScript.transform.position, platformB.position);

        if(distanceToPlatformA > distanceToPlatformB)
        {
            furthestFoundPlatform = platformA;
        }
        else
        {
            furthestFoundPlatform = platformB;
        }

        return furthestFoundPlatform;
    }

    /// <summary>
    /// V�ljer ut en slumpm�ssig flyglast p� en av plattformarna, dock inte en flyglast
    /// som nyligen blev avlastad. 
    /// </summary>
    /// 
    /// <param name="platform">
    /// Plattformen varp� en slumpm�ssig flyglast ska v�ljas ut.
    /// </param>
    /// 
    /// <returns>
    /// Returnerar en Unity inbyggd transform-komponent som inneh�ller plattformens koordinater
    /// </returns>
    private Transform GetRandomContainerOnPlatform(Transform platform)
    {
        Platform platformScript = platform.GetComponent<Platform>();
        List<GameObject> availableContainersOnPlatform = new List<GameObject>();
        Transform containerTransform = null;

        foreach (GameObject container in platformScript.platformContainers)
        {
            if (container != lastDeliveredContainer)
            {
                availableContainersOnPlatform.Add(container);
            }
        }

        int randomIndex = Random.Range(0, availableContainersOnPlatform.Count);
        containerTransform = availableContainersOnPlatform[randomIndex].transform;

        return containerTransform;
    }

    /// <summary>
    /// Hittar en slumpm�ssig plats p� en av plattformarna varp� flyglasten ska avlastas, och samtidigt
    /// ser till att flyglasten inte kommer i kontakt med redan befintliga flyglaster p� plattformen.
    /// </summary>
    /// 
    /// <param name="platform">
    /// Plattformen som en tillg�nglig avlastningsplats ska s�kas p�.
    /// </param>
    /// 
    /// <returns>
    /// Returnerar efter 100 f�rs�k koordinater f�r den fria avlastningaplatsen, givet 
    /// att datorn lyckas hitta en fri plats. 
    /// Anledningen till att datorn har endast 100 f�rs�k 
    /// p� att hitta en fri avlastningsplats �r f�r att undvika programmet fr�n att 
    /// fastna i en o�ndlig loop ifall den lyckas aldrig att hitta en fri plats. 
    /// Dock b�r detta utfall aldrig intr�ffa.
    /// </returns>
    private Transform FindEmptyContainerSpotOnPlatform(Transform platform)
    {
        Transform emptySpot = emptySpotGameObject.transform;
        Platform platformScript = platform.GetComponent<Platform>();

        int maxAttempts = 100;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            float xMin = platformScript.GetPlatformBounds().min.x + minimumDistanceToPlatformBorder;
            float xMax = platformScript.GetPlatformBounds().max.x - minimumDistanceToPlatformBorder;
            float randomXPositionOnPlatform = Random.Range(xMin, xMax);

            float zMin = platformScript.GetPlatformBounds().min.z + minimumDistanceToPlatformBorder;
            float zMax = platformScript.GetPlatformBounds().max.z - minimumDistanceToPlatformBorder;
            float randomZPositionOnPlatform = Random.Range(zMin, zMax);

            Vector3 randomPositionOnPlatform = new Vector3(randomXPositionOnPlatform, platform.position.y, randomZPositionOnPlatform);

            bool isSpotEmpty = true;

            foreach (GameObject container in platformScript.platformContainers)
            {
                float distanceFromRandomPositionToContainer = Vector3.Distance(randomPositionOnPlatform, container.transform.position);

                if (distanceFromRandomPositionToContainer < minimumDistanceBetweenCargos)
                {
                    isSpotEmpty = false;
                    break;
                }
            }

            if (isSpotEmpty)
            {
                emptySpot.position = randomPositionOnPlatform;
                return emptySpot;
            }

            attempts++;
        }

        return emptySpot;
    }

    /// <summary>
    /// Ser till att zeppelinaren flyger till sin destination.
    /// </summary>
    private void FlyZeppelinToTarget()
    {
        Vector2 zeppelinPositionIn2D = new Vector2(zeppelinScript.transform.position.x, zeppelinScript.transform.position.z);
        Vector2 destinationPositionIn2D = new Vector2(currentDestination.position.x, currentDestination.position.z);
        float distanceToDestinationPointIn2D = Vector2.Distance(zeppelinPositionIn2D, destinationPositionIn2D);

        float slowDownFactor = 1.35f * Mathf.Pow(10, -7);
        float distanceToSlowDownAt = Mathf.Clamp(zeppelinScript.GetForwardForceValue() * slowDownFactor, 40, Mathf.Infinity);
        float descendAtDistance = Mathf.Clamp(0.375f * distanceToSlowDownAt, 10, Mathf.Infinity);
        Debug.Log("Descend at: " + descendAtDistance);

        Debug.Log("Distance to slow down at: " + distanceToSlowDownAt);
        Debug.Log("Distance to point: " + distanceToDestinationPointIn2D);

        if (distanceToDestinationPointIn2D > distanceToSlowDownAt)
        {
            zeppelinScript.AddForwardForce(Vector3.forward);
        }
        else
        {
            float zeppelinSlowdownFactorOnApproach = Mathf.Clamp01(distanceToDestinationPointIn2D / 10000f);
            zeppelinScript.AddForwardForce(Vector3.forward * zeppelinSlowdownFactorOnApproach);

            if (distanceToDestinationPointIn2D < descendAtDistance)
            {
                zeppelinScript.AddForwardForce(Vector3.forward * 0);
                currentState = AutopilotBehaviourState.Descending;
            }
        }
    }

    /// <summary>
    /// F�r zeppelinaren att stiga till en l�gsta avsedd h�jd (zeppelinMinimumAscendAltitude)
    /// efter att den har av -eller p�lastat flyglasten fr�n en av plattformarna.
    /// </summary>
    private void AscendZeppelin()
    {
        if (zeppelinScript.transform.position.y < zeppelinMinimumAscendAltitude)
        {
            zeppelinScript.isMovingVertically = true;
            zeppelinScript.isMovingUpward = true;
        }
        else
        {
            zeppelinScript.isMovingVertically = false;
            currentState = AutopilotBehaviourState.RotatingTowardsTarget;
        }
    }


    /// <summary>
    /// F�r zeppelinaren att nedstiga d� den har anl�nt vid destination.
    /// </summary>
    private void DescendZeppelin()
    {
        if (zeppelinScript.transform.position.y > currentDestination.position.y + 40f)
        {
            zeppelinScript.isMovingVertically = true;
            zeppelinScript.isMovingUpward = false;
        }
        else
        {
            zeppelinScript.isMovingVertically = false;

            if (zeppelinScript.GetAttachedContainer() != null)
            {
                DeliverContainer();
                currentState = AutopilotBehaviourState.Ascending;
            }
            else
            {
                currentState = AutopilotBehaviourState.Docking;
            }
        }
    }

    /// <summary>
    /// F�r zeppelinaren att avlasta zeppelinarens flyglast 
    /// och hittar en ny slumpm�ssig last p� samma plattform
    /// och st�ller in lastens position som sin nya destination.
    /// </summary>
    private void DeliverContainer()
    {
        lastDeliveredContainer = zeppelinScript.GetAttachedContainer().gameObject;
        zeppelinScript.DetachContainer();

        Transform targetPlatform = GetNearestContainerPlatform();
        currentDestination = GetRandomContainerOnPlatform(targetPlatform);
    }

    /// <summary>
    /// F�r zeppelinaren att p�lasta flyglasten givet att den inte har en p�lastad last. 
    /// </summary>
    private void PickUpContainer()
    {
        if (zeppelinScript.GetAttachedContainer() != null)
        {
            Transform targetPlatform = GetFurthestContainerPlatform();
            zeppelinScript.platformWhereContainerGotAttached = GetNearestContainerPlatform();
            currentDestination = FindEmptyContainerSpotOnPlatform(targetPlatform);
            currentState = AutopilotBehaviourState.Ascending;
        }
    }

    /// <summary>
    /// F�r zeppelinaren att rotera s� att dess framsida �r riktad mot destination innan den b�rjar f�rdas fram�t.
    /// </summary>
    private void RotateZeppelin()
    {
        float thresholdAngle = 0.3f;
        Vector3 directionToFaceDestination = currentDestination.position - zeppelinScript.transform.position;

        //Ignorera h�jden (y-koordinaten)
        directionToFaceDestination.y = 0f;

        Quaternion targetRotation = Quaternion.LookRotation(directionToFaceDestination); 
        float angle = Quaternion.Angle(zeppelinScript.transform.rotation, targetRotation);

        if (angle > thresholdAngle)
        {
            Quaternion rotation = Quaternion.Slerp(zeppelinScript.transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            zeppelinScript.transform.rotation = rotation;
        }
        else
        {
            currentState = AutopilotBehaviourState.FlyingToDestination;
        }
    }
}