using UnityEngine;
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

    [Header("Debug Settings")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private bool simulateLocationInEditor = false;
    [SerializeField] private Vector2 editorSimulatedLocation = new Vector2(37.7749f, -122.4194f);

    private ARSession arSession;
    private ARCameraManager arCameraManager;

    private Vector2 currentCoordinates;
    private bool isLocationInitialized = false;

    private void Awake()
    {
        InitializeComponents();
        RequestPermissions();
    }

    private void InitializeComponents()
    {
        arSession = FindObjectOfType<ARSession>();
        arCameraManager = FindObjectOfType<ARCameraManager>();

        if (arSession == null || arCameraManager == null)
        {
            DebugLog("AR components not fully configured!", true);
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

            UpdateUITexts(location);
            ConvertToUnityCoordinates();
        }
        catch (System.Exception e)
        {
            DebugLog($"Location update error: {e.Message}", true);
        }
    }

    private void UpdateUITexts(LocationInfo location)
    {
        if (latitudeText != null)
            latitudeText.text = $"Latitude: {location.latitude:F6}";
        if (longitudeText != null)
            longitudeText.text = $"Longitude: {location.longitude:F6}";
        if (altitudeText != null)
            altitudeText.text = $"Altitude: {location.altitude:F1}m";
        if (arStatusText != null)
            arStatusText.text = $"Status: AR {ARSession.state}, GPS Active";
    }

    private void ConvertToUnityCoordinates()
    {
        Vector3 unityPosition = GPSEncoder.GPSToUCS(currentCoordinates);
        DebugLog($"Unity Coordinates: {unityPosition}");
    }

    public Vector2 GetCurrentCoordinates() => currentCoordinates;

    private void DebugLog(string message, bool isError = false)
    {
        if (!showDebugLogs) return;

        if (isError)
            Debug.LogError($"[UnifiedGPSManager] {message}");
        else
            Debug.Log($"[UnifiedGPSManager] {message}");
    }

    private void OnDestroy()
    {
        if (Input.location.status == LocationServiceStatus.Running)
        {
            CancelInvoke(nameof(UpdateLocationUI));
            Input.location.Stop();
        }
    }
}