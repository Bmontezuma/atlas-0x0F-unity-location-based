using System.Collections;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.XR.ARFoundation;
using TMPro;

public class GPSManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI latitudeText;
    [SerializeField] private TextMeshProUGUI longitudeText;
    [SerializeField] private TextMeshProUGUI altitudeText;
    [SerializeField] private TextMeshProUGUI arPositionText;

    [Header("Debug Settings")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private bool simulateLocationInEditor = false;
    [SerializeField] private Vector2 editorSimulatedLocation = new Vector2(37.7749f, -122.4194f); // Example: San Francisco

    private ARSession arSession;
    private ARCameraManager arCameraManager;

    private void Awake()
    {
        DebugLog("[GPSManager] Initializing...");

        if (latitudeText == null || longitudeText == null || altitudeText == null || arPositionText == null)
        {
            DebugLog("[GPSManager] One or more TextMeshProUGUI fields are not assigned in the Inspector!", true);
            return;
        }

        arSession = FindObjectOfType<ARSession>();
        arCameraManager = FindObjectOfType<ARCameraManager>();

        if (arSession == null)
        {
            DebugLog("[GPSManager] ARSession not found! Please add ARSession to the scene.", true);
        }

        if (arCameraManager == null)
        {
            DebugLog("[GPSManager] ARCameraManager not found! Please add AR Camera to the scene.", true);
        }

#if PLATFORM_ANDROID
        RequestPermissions();
#endif
    }

    private void RequestPermissions()
    {
        DebugLog("[GPSManager] Requesting permissions...");
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }

        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }
    }

    private void Start()
    {
        DebugLog("[GPSManager] Starting...");
        StartCoroutine(InitializeAR());
    }

    private IEnumerator InitializeAR()
    {
        DebugLog("[GPSManager] Initializing AR...");

#if UNITY_EDITOR
        if (simulateLocationInEditor)
        {
            DebugLog("[GPSManager] Simulating AR session in the Editor...");
            yield return new WaitForSeconds(1);
            StartCoroutine(StartLocationService());
            yield break;
        }
#endif

        while (ARSession.state == ARSessionState.None || ARSession.state == ARSessionState.SessionInitializing)
        {
            DebugLog($"[GPSManager] Waiting for ARSession to initialize. Current state: {ARSession.state}");
            yield return null;
        }

        if (ARSession.state == ARSessionState.Unsupported)
        {
            DebugLog("[GPSManager] AR is not supported on this device!", true);
            yield break;
        }

        if (ARSession.state != ARSessionState.Ready)
        {
            DebugLog($"[GPSManager] AR failed to initialize. Current state: {ARSession.state}", true);
            yield break;
        }

        DebugLog("[GPSManager] AR initialized successfully.");
        StartCoroutine(StartLocationService());
    }

    private IEnumerator StartLocationService()
    {
        DebugLog("[GPSManager] Starting location service...");

#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            DebugLog("[GPSManager] Location permission not granted!", true);
            yield break;
        }
#endif

        if (!Input.location.isEnabledByUser)
        {
            DebugLog("[GPSManager] Location services are disabled. Please enable them in device settings.", true);
            yield break;
        }

        Input.location.Start(1f, 1f);
        DebugLog("[GPSManager] Waiting for location initialization...");

        int maxWait = 30;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            DebugLog($"[GPSManager] Initializing location services... {maxWait}s left.");
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1 || Input.location.status == LocationServiceStatus.Failed)
        {
            DebugLog("[GPSManager] Location services failed to initialize.", true);
            yield break;
        }

        DebugLog("[GPSManager] Location services started successfully.");
        InvokeRepeating(nameof(UpdateLocationUI), 0, 1f);
    }

    private void UpdateLocationUI()
    {
        if (simulateLocationInEditor)
        {
            DebugLog("[GPSManager] Simulating GPS location in Editor...");
            latitudeText.text = $"Latitude: {editorSimulatedLocation.x:F6}";
            longitudeText.text = $"Longitude: {editorSimulatedLocation.y:F6}";
            altitudeText.text = $"Altitude: Simulated";
            return;
        }

        if (Input.location.status != LocationServiceStatus.Running)
        {
            DebugLog("[GPSManager] Location services are not running.", true);
            return;
        }

        try
        {
            var location = Input.location.lastData;
            DebugLog($"[GPSManager] GPS Raw Data - Lat: {location.latitude}, Lon: {location.longitude}, Alt: {location.altitude}");

            latitudeText.text = $"Latitude: {location.latitude:F6}";
            longitudeText.text = $"Longitude: {location.longitude:F6}";
            altitudeText.text = $"Altitude: {location.altitude:F1}m";

            if (arCameraManager != null)
            {
                Vector3 arPosition = arCameraManager.transform.position;
                arPositionText.text = $"AR Position: {arPosition:F2}";
            }
        }
        catch (System.Exception e)
        {
            DebugLog($"[GPSManager] Error updating location: {e.Message}", true);
        }
    }

    private void DebugLog(string message, bool isError = false)
    {
        if (!showDebugLogs) return;

        if (isError)
            Debug.LogError(message);
        else
            Debug.Log(message);
    }
}
