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
    [SerializeField] private bool simulateLocationInEditor = false;

    private Vector2 currentCoordinates;
    private Vector2 storedCoordinates;
    private bool isDestinationSet = false;

    private ARSession arSession;
    private ARCameraManager arCameraManager;
    private Camera arCamera;

    private Vector2 editorSimulatedLocation = new Vector2(37.7749f, -122.4194f); // Example: San Francisco

    private void Awake()
    {
        DebugLog("Initializing...");

#if UNITY_EDITOR
        if (simulateLocationInEditor)
        {
            DebugLog("Running in Editor with simulated location");
            GPSEncoder.SetLocalOrigin(editorSimulatedLocation);
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
        DebugLog("Requesting permissions...");

        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            DebugLog("Requesting Camera permission...");
            Permission.RequestUserPermission(Permission.Camera);
        }

        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            DebugLog("Requesting Fine Location permission...");
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
        }

        if (arCameraManager == null)
        {
            DebugLog("ARCameraManager not found! Please add ARCamera to scene.", true);
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
            GPSEncoder.SetLocalOrigin(editorSimulatedLocation);
            yield return new WaitForSeconds(1);
            StartCoroutine(StartLocationService());
            yield break;
        }
#endif

        while (ARSession.state == ARSessionState.None || ARSession.state == ARSessionState.SessionInitializing)
        {
            DebugLog($"Waiting for ARSession to initialize. Current state: {ARSession.state}");
            yield return null;
        }

        if (ARSession.state == ARSessionState.Unsupported)
        {
            DebugLog("AR is not supported on this device!", true);
            yield break;
        }

        if (ARSession.state != ARSessionState.Ready)
        {
            DebugLog($"AR failed to initialize. Current state: {ARSession.state}", true);
            yield break;
        }

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

        if (!ArePermissionsGranted())
        {
            DebugLog("Permissions not granted for location services.", true);
            yield break;
        }

        if (!Input.location.isEnabledByUser)
        {
            DebugLog("Location services are disabled. Please enable them in device settings.", true);
            yield break;
        }

        Input.location.Start(1f, 1f);
        DebugLog("Waiting for location initialization...");

        int maxWait = 30;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            DebugLog($"Initializing location services... {maxWait}s remaining.");
            yield return new WaitForSeconds(1);
            maxWait--;
        }

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
            DebugLog("Simulating location updates in the Editor...");
            return;
        }
#endif

        if (Input.location.status == LocationServiceStatus.Running)
        {
            UpdateLocation();
        }
        else
        {
            DebugLog("Location service is not running.");
        }
    }

    private void UpdateLocation()
    {
        var location = Input.location.lastData;
        currentCoordinates = new Vector2(location.latitude, location.longitude);
        DebugLog($"Current Location - Lat: {location.latitude}, Long: {location.longitude}, Alt: {location.altitude}");

        // Convert GPS to Unity coordinates
        Vector3 unityPosition = GPSEncoder.GPSToUCS(currentCoordinates);
        DebugLog($"Converted to Unity Coordinates: {unityPosition}");
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

    private bool ArePermissionsGranted()
    {
#if PLATFORM_ANDROID
        return Permission.HasUserAuthorizedPermission(Permission.Camera) &&
               Permission.HasUserAuthorizedPermission(Permission.FineLocation);
#else
        return true;
#endif
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
