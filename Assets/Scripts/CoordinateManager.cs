using System.Collections;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class CoordinateManager : MonoBehaviour
{
    private Vector2 currentCoordinates;
    private Vector2 storedCoordinates;
    private bool isDestinationSet = false;

    [SerializeField] private GameObject currentLocationMeshPrefab;
    [SerializeField] private GameObject destinationLocationMeshPrefab;
    
    private ARSession arSession;
    private ARCameraManager arCameraManager;
    private Camera arCamera;

    private void Awake()
    {
        Debug.Log("[CoordinateManager] Initializing...");
        
        // Check and request Android permissions
        #if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }
        #endif

        // Initialize AR components
        arSession = FindObjectOfType<ARSession>();
        arCameraManager = FindObjectOfType<ARCameraManager>();
        
        if (arSession == null)
        {
            Debug.LogError("[CoordinateManager] ARSession not found! Please add ARSession to scene.");
            return;
        }

        if (arCameraManager == null)
        {
            Debug.LogError("[CoordinateManager] ARCameraManager not found! Please add ARCamera to scene.");
            return;
        }
    }

    private void Start()
    {
        Debug.Log("[CoordinateManager] Starting...");
        StartCoroutine(InitializeAR());
    }

    private IEnumerator InitializeAR()
    {
        Debug.Log("[CoordinateManager] Initializing AR...");
        
        // Wait for AR session to be ready
        yield return new WaitUntil(() => arSession.state == ARSessionState.Ready);
        
        // Get AR Camera reference
        arCamera = arCameraManager.GetComponent<Camera>();
        if (arCamera == null)
        {
            Debug.LogError("[CoordinateManager] Failed to get AR Camera reference!");
            yield break;
        }

        Debug.Log("[CoordinateManager] AR initialized successfully.");
        StartCoroutine(StartLocationService());
    }

    private IEnumerator StartLocationService()
    {
        Debug.Log("[CoordinateManager] Starting location service...");

        // Check if location service is enabled
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogError("[CoordinateManager] Location services are disabled. Please enable them in device settings.");
            yield break;
        }

        // Start location service
        Input.location.Start(1f, 1f);
        Debug.Log("[CoordinateManager] Waiting for location initialization...");

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Check if service initialized successfully
        if (maxWait < 1)
        {
            Debug.LogError("[CoordinateManager] Location service initialization timed out!");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("[CoordinateManager] Unable to determine device location!");
            yield break;
        }

        Debug.Log("[CoordinateManager] Location services initialized successfully.");
    }

    private void Update()
    {
        if (Input.location.status == LocationServiceStatus.Running)
        {
            UpdateLocation();
        }
    }

    private void UpdateLocation()
    {
        var location = Input.location.lastData;
        currentCoordinates = new Vector2(location.latitude, location.longitude);
        Debug.Log($"[CoordinateManager] Current Location - Lat: {location.latitude}, Long: {location.longitude}, Alt: {location.altitude}");
    }

    public void GetCurrentCoordinates()
    {
        Debug.Log($"[CoordinateManager] Current Coordinates: Lat {currentCoordinates.x}, Long {currentCoordinates.y}");
    }

    public void SetDestinationCoordinates()
    {
        storedCoordinates = currentCoordinates;
        isDestinationSet = true;
        Debug.Log($"[CoordinateManager] Destination Set: Lat {storedCoordinates.x}, Long {storedCoordinates.y}");
    }

    public void DrawObject()
    {
        if (!isDestinationSet)
        {
            Debug.LogError("[CoordinateManager] No destination set!");
            return;
        }

        if (arCamera == null)
        {
            Debug.LogError("[CoordinateManager] AR Camera not initialized!");
            return;
        }

        try
        {
            // Place current location marker in front of the camera
            Vector3 cameraForward = arCamera.transform.forward;
            Vector3 currentPosition = arCamera.transform.position + (cameraForward * 2.0f);
            
            if (currentLocationMeshPrefab != null)
            {
                Instantiate(currentLocationMeshPrefab, currentPosition, Quaternion.identity);
                Debug.Log($"[CoordinateManager] Current location marker placed at: {currentPosition}");
            }

            // Place destination marker offset from current position
            Vector3 destinationPosition = currentPosition + (Vector3.right * 2.0f);
            if (destinationLocationMeshPrefab != null)
            {
                Instantiate(destinationLocationMeshPrefab, destinationPosition, Quaternion.identity);
                Debug.Log($"[CoordinateManager] Destination marker placed at: {destinationPosition}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[CoordinateManager] Error placing markers: {e.Message}");
        }
    }

    private void OnDestroy()
    {
        Debug.Log("[CoordinateManager] Cleaning up...");
        if (Input.location.status == LocationServiceStatus.Running)
        {
            Input.location.Stop();
        }
    }

    private float CalculateDistance(Vector2 coord1, Vector2 coord2)
    {
        const float R = 6371000; // Earth's radius in meters
        float lat1 = coord1.x * Mathf.Deg2Rad;
        float lon1 = coord1.y * Mathf.Deg2Rad;
        float lat2 = coord2.x * Mathf.Deg2Rad;
        float lon2 = coord2.y * Mathf.Deg2Rad;

        float dLat = lat2 - lat1;
        float dLon = lon2 - lon1;

        float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
                  Mathf.Cos(lat1) * Mathf.Cos(lat2) *
                  Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);
        float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));

        return R * c;
    }
}