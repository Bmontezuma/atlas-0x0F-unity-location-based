using UnityEngine;
using UnityEngine.UI; // Required for Button
using UnityEngine.Android;
using UnityEngine.XR.ARFoundation;
using TMPro;
using System.Collections;

public class UnifiedGPSManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI latitudeText;
    [SerializeField] private TextMeshProUGUI longitudeText;
    [SerializeField] private TextMeshProUGUI altitudeText;
    [SerializeField] private TextMeshProUGUI arStatusText;
    [SerializeField] private TextMeshProUGUI distanceText; // UI for distance display
    [SerializeField] private Button storeLocationButton; // Button to store first location
    [SerializeField] private Button calculateDistanceButton; // Button to trigger distance calculation

    [Header("Debug Settings")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private bool simulateLocationInEditor = false;
    [SerializeField] private Vector2 editorSimulatedLocation = new Vector2(37.7749f, -122.4194f);

    private ARSession arSession;
    private ARCameraManager arCameraManager;
    private ObjectPlacementManager objectPlacementManager;
    private Vector2 currentCoordinates;
    private Vector2 storedCoordinates;
    private bool isLocationInitialized = false;
    private bool hasStoredCoordinates = false;

    private void Awake()
    {
        InitializeComponents();
        RequestPermissions();
    }

    private void InitializeComponents()
    {
        arSession = FindObjectOfType<ARSession>();
        arCameraManager = FindObjectOfType<ARCameraManager>();
        objectPlacementManager = FindObjectOfType<ObjectPlacementManager>();

        if (arSession == null || arCameraManager == null)
        {
            DebugLog("AR components not fully configured!", true);
        }

        if (objectPlacementManager == null)
        {
            DebugLog("ObjectPlacementManager not found in scene!", true);
        }

        if (storeLocationButton != null)
        {
            storeLocationButton.onClick.AddListener(StoreCurrentLocation);
        }

        if (calculateDistanceButton != null)
        {
            calculateDistanceButton.onClick.AddListener(CalculateDistance);
        }
    }

    private void RequestPermissions()
    {
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
    }

    private void Start()
    {
        StartCoroutine(InitializeARAndLocation());
    }

    private IEnumerator InitializeARAndLocation()
    {
#if UNITY_EDITOR
        if (simulateLocationInEditor)
        {
            yield return SimulateEditorLocation();
            yield break;
        }
#endif
        yield return InitializeAR();
        yield return StartLocationServices();
    }

    private IEnumerator InitializeAR()
    {
        if (arSession == null) yield break;

        while (ARSession.state == ARSessionState.None || ARSession.state == ARSessionState.SessionInitializing)
        {
            DebugLog($"Waiting for AR initialization. State: {ARSession.state}");
            yield return null;
        }

        if (ARSession.state == ARSessionState.Unsupported)
        {
            DebugLog("AR not supported on this device!", true);
            yield break;
        }

        if (ARSession.state != ARSessionState.Ready)
        {
            DebugLog($"AR initialization failed. State: {ARSession.state}", true);
            yield break;
        }

        DebugLog("AR initialized successfully.");
    }

#if UNITY_EDITOR
    private IEnumerator SimulateEditorLocation()
    {
        currentCoordinates = editorSimulatedLocation;
        UpdateLocationUI();
        yield return new WaitForSeconds(1);
    }
#endif

    private IEnumerator StartLocationServices()
    {
        if (!Input.location.isEnabledByUser)
        {
            DebugLog("Location services disabled!", true);
            yield break;
        }

        Input.location.Start(1f, 1f);
        int maxWait = 30;

        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            DebugLog($"Initializing location services... {maxWait}s remaining");
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1 || Input.location.status == LocationServiceStatus.Failed)
        {
            DebugLog("Location services initialization failed!", true);
            yield break;
        }

        isLocationInitialized = true;
        InvokeRepeating(nameof(UpdateLocationUI), 0, 1f);
    }

    private void UpdateLocationUI()
    {
        if (!isLocationInitialized)
        {
            DebugLog("Location services not running", true);
            return;
        }

        try
        {
            var location = Input.location.lastData;
            currentCoordinates = new Vector2(location.latitude, location.longitude);

            latitudeText.text = $"Latitude: {location.latitude:F6}";
            longitudeText.text = $"Longitude: {location.longitude:F6}";
            altitudeText.text = $"Altitude: {location.altitude:F1}m";
            arStatusText.text = $"Status: AR {ARSession.state}, GPS Active";
        }
        catch (System.Exception e)
        {
            DebugLog($"Location update error: {e.Message}", true);
        }
    }

    public void StoreCurrentLocation()
    {
        storedCoordinates = currentCoordinates;
        hasStoredCoordinates = true;
        DebugLog($"Stored location: {storedCoordinates}");

        // Spawn first prefab at the stored location
        objectPlacementManager.SpawnObjectAtGPSLocation(storedCoordinates, objectPlacementManager.firstMeshPrefab);
    }

    public void CalculateDistance()
    {
        if (!hasStoredCoordinates)
        {
            DebugLog("No stored location available to calculate distance!", true);
            return;
        }

        float distance = HaversineDistance(storedCoordinates, currentCoordinates);
        if (distanceText != null)
        {
            distanceText.text = $"Distance: {distance:F2} meters";
        }
        DebugLog($"Distance between stored location and current location: {distance} meters");

        // Spawn second prefab at the current location
        objectPlacementManager.SpawnObjectAtGPSLocation(currentCoordinates, objectPlacementManager.secondMeshPrefab);
    }

    private float HaversineDistance(Vector2 coord1, Vector2 coord2)
    {
        float R = 6371000f; // Radius of Earth in meters
        float dLat = Mathf.Deg2Rad * (coord2.x - coord1.x);
        float dLon = Mathf.Deg2Rad * (coord2.y - coord1.y);

        float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
                  Mathf.Cos(Mathf.Deg2Rad * coord1.x) * Mathf.Cos(Mathf.Deg2Rad * coord2.x) *
                  Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);

        float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
        return R * c; // Distance in meters
    }

    private void DebugLog(string message, bool isError = false)
    {
        if (!showDebugLogs) return;

        if (isError)
            Debug.LogError($"[UnifiedGPSManager] {message}");
        else
            Debug.Log($"[UnifiedGPSManager] {message}");
    }
}
