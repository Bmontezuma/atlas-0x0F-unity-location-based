using System.Collections;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class CoordinateManager : MonoBehaviour
{
    [Header("AR Objects")]
    [SerializeField] private GameObject currentLocationMeshPrefab;
    [SerializeField] private GameObject destinationLocationMeshPrefab;

    [Header("Debug Settings")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private bool simulateLocationInEditor = true;

    private Vector2 currentCoordinates;
    private Vector2 storedCoordinates;
    private bool isDestinationSet = false;

    private ARSession arSession;
    private ARCameraManager arCameraManager;
    private Camera arCamera;

    // Editor simulation coordinates (only used in Unity Editor)
    private Vector2 editorSimulatedLocation = new Vector2(37.7749f, -122.4194f); // San Francisco coordinates

    private void Awake()
    {
        DebugLog("Initializing...");
        
        #if UNITY_EDITOR
        if (simulateLocationInEditor)
        {
            DebugLog("Running in Editor with simulated location");
            return;
        }
        #endif

        #if PLATFORM_ANDROID
        RequestPermissions();
        #endif

        InitializeARComponents();
    }

    private void RequestPermissions()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }
    }

    private void InitializeARComponents()
    {
        arSession = FindObjectOfType<ARSession>();
        arCameraManager = FindObjectOfType<ARCameraManager>();

        if (arSession == null)
        {
            DebugLog("ARSession not found! Please add ARSession to scene.", true);
            return;
        }

        if (arCameraManager == null)
        {
            DebugLog("ARCameraManager not found! Please add ARCamera to scene.", true);
            return;
        }
    }

    private void Start()
    {
        DebugLog("Starting...");
        StartCoroutine(InitializeAR());
    }

    private IEnumerator InitializeAR()
    {
        DebugLog("Initializing AR...");

        #if UNITY_EDITOR
        if (simulateLocationInEditor)
        {
            DebugLog("Simulating AR session in Editor");
            yield return new WaitForSeconds(1);
            StartCoroutine(StartLocationService());
            yield break;
        }
        #endif

        while (ARSession.state == ARSessionState.None || 
               ARSession.state == ARSessionState.SessionInitializing)
        {
            yield return null;
        }

        if (ARSession.state == ARSessionState.Unsupported)
        {
            DebugLog("AR is not supported on this device!", true);
            yield break;
        }

        // Only proceed if we're in a valid state
        if (ARSession.state != ARSessionState.Ready)
        {
            DebugLog($"AR failed to initialize. State: {ARSession.state}", true);
            yield break;
        }
        
        // Get AR Camera reference
        arCamera = arCameraManager.GetComponent<Camera>();
        if (arCamera == null)
        {
            DebugLog("Failed to get AR Camera reference!", true);
            yield break;
        }

        DebugLog("AR initialized successfully.");
        StartCoroutine(StartLocationService());
    }

    private IEnumerator StartLocationService()
    {
        DebugLog("Starting location service...");

        #if UNITY_EDITOR
        if (simulateLocationInEditor)
        {
            currentCoordinates = editorSimulatedLocation;
            DebugLog($"Using simulated location: Lat {currentCoordinates.x}, Long {currentCoordinates.y}");
            yield break;
        }
        #endif

        // Check if location service is enabled
        if (!Input.location.isEnabledByUser)
        {
            DebugLog("Location services are disabled. Please enable them in device settings.", true);
            yield break;
        }

        // Start location service
        Input.location.Start(1f, 1f);
        DebugLog("Waiting for location initialization...");

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
            DebugLog("Location service initialization timed out!", true);
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            DebugLog("Unable to determine device location!", true);
            yield break;
        }

        DebugLog("Location services initialized successfully.");
    }

    private void Update()
    {
        #if UNITY_EDITOR
        if (simulateLocationInEditor)
        {
            return;
        }
        #endif

        if (Input.location.status == LocationServiceStatus.Running)
        {
            UpdateLocation();
        }
    }

    private void UpdateLocation()
    {
        var location = Input.location.lastData;
        currentCoordinates = new Vector2(location.latitude, location.longitude);
        DebugLog($"Current Location - Lat: {location.latitude}, Long: {location.longitude}, Alt: {location.altitude}");
    }

    public void GetCurrentCoordinates()
    {
        DebugLog($"Current Coordinates: Lat {currentCoordinates.x}, Long {currentCoordinates.y}");
    }

    public void SetDestinationCoordinates()
    {
        storedCoordinates = currentCoordinates;
        isDestinationSet = true;
        DebugLog($"Destination Set: Lat {storedCoordinates.x}, Long {storedCoordinates.y}");
    }

    public void DrawObject()
    {
        if (!isDestinationSet)
        {
            DebugLog("No destination set!", true);
            return;
        }

        #if UNITY_EDITOR
        if (simulateLocationInEditor)
        {
            PlaceSimulatedObjects();
            return;
        }
        #endif

        if (arCamera == null)
        {
            DebugLog("AR Camera not initialized!", true);
            return;
        }

        try
        {
            PlaceARObjects();
        }
        catch (System.Exception e)
        {
            DebugLog($"Error placing markers: {e.Message}", true);
        }
    }

    private void PlaceSimulatedObjects()
    {
        // In editor, place objects in front of main camera
        Vector3 cameraPosition = Camera.main.transform.position;
        Vector3 cameraForward = Camera.main.transform.forward;

        // Place current location marker
        Vector3 currentPosition = cameraPosition + (cameraForward * 2.0f);
        if (currentLocationMeshPrefab != null)
        {
            Instantiate(currentLocationMeshPrefab, currentPosition, Quaternion.identity);
            DebugLog($"Current location marker placed at: {currentPosition}");
        }

        // Place destination marker
        Vector3 destinationPosition = currentPosition + (Vector3.right * 2.0f);
        if (destinationLocationMeshPrefab != null)
        {
            Instantiate(destinationLocationMeshPrefab, destinationPosition, Quaternion.identity);
            DebugLog($"Destination marker placed at: {destinationPosition}");
        }
    }

    private void PlaceARObjects()
    {
        // Place current location marker in front of the AR camera
        Vector3 cameraForward = arCamera.transform.forward;
        Vector3 currentPosition = arCamera.transform.position + (cameraForward * 2.0f);
        
        if (currentLocationMeshPrefab != null)
        {
            Instantiate(currentLocationMeshPrefab, currentPosition, Quaternion.identity);
            DebugLog($"Current location marker placed at: {currentPosition}");
        }

        // Place destination marker offset from current position
        Vector3 destinationPosition = currentPosition + (Vector3.right * 2.0f);
        if (destinationLocationMeshPrefab != null)
        {
            Instantiate(destinationLocationMeshPrefab, destinationPosition, Quaternion.identity);
            DebugLog($"Destination marker placed at: {destinationPosition}");
        }
    }

    private void OnDestroy()
    {
        DebugLog("Cleaning up...");
        #if !UNITY_EDITOR
        if (Input.location.status == LocationServiceStatus.Running)
        {
            Input.location.Stop();
        }
        #endif
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

    private void DebugLog(string message, bool isError = false)
    {
        if (!showDebugLogs) return;

        if (isError)
            Debug.LogError($"[CoordinateManager] {message}");
        else
            Debug.Log($"[CoordinateManager] {message}");
    }
}