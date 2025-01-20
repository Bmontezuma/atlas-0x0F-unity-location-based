using System.Collections;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.XR.ARFoundation;
using TMPro;

public class GPSLogger : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI latitudeText;
    [SerializeField] private TextMeshProUGUI longitudeText;
    [SerializeField] private TextMeshProUGUI altitudeText;
    [SerializeField] private TextMeshProUGUI arStatusText;

    private ARSession arSession;

    private void Awake()
    {
        Debug.Log("[GPSLogger] Initializing...");
        
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

        arSession = FindObjectOfType<ARSession>();
        if (arSession == null)
        {
            Debug.LogError("[GPSLogger] ARSession not found!");
            return;
        }
    }

    private void Start()
    {
        Debug.Log("[GPSLogger] Starting...");
        StartCoroutine(InitializeAR());
    }

    private IEnumerator InitializeAR()
    {
        Debug.Log("[GPSLogger] Waiting for AR initialization...");
        yield return new WaitUntil(() => ARSession.state == ARSessionState.Ready);
        
        Debug.Log("[GPSLogger] AR initialized successfully.");
        StartCoroutine(StartLocationService());
    }

    private IEnumerator StartLocationService()
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogError("[GPSLogger] Location services are not enabled.");
            UpdateStatusText("Location services disabled");
            yield break;
        }

        Input.location.Start();
        Debug.Log("[GPSLogger] Initializing location services...");
        UpdateStatusText("Initializing location...");

        while (Input.location.status == LocationServiceStatus.Initializing)
        {
            yield return new WaitForSeconds(1);
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("[GPSLogger] Unable to determine device location.");
            UpdateStatusText("Location services failed");
            yield break;
        }

        Debug.Log("[GPSLogger] Location services initialized. Starting updates...");
        UpdateStatusText("Location services active");
        InvokeRepeating(nameof(UpdateLocationUI), 0, 1);
    }

    private void UpdateLocationUI()
    {
        if (Input.location.status != LocationServiceStatus.Running)
        {
            UpdateStatusText("Location services not running");
            return;
        }

        try
        {
            var location = Input.location.lastData;
            
            if (latitudeText != null)
                latitudeText.text = $"Latitude: {location.latitude:F6}";
            if (longitudeText != null)
                longitudeText.text = $"Longitude: {location.longitude:F6}";
            if (altitudeText != null)
                altitudeText.text = $"Altitude: {location.altitude:F1}m";

            UpdateStatusText($"AR: {ARSession.state}, GPS: Active");
            Debug.Log($"[GPSLogger] Location - Lat: {location.latitude:F6}, Long: {location.longitude:F6}, Alt: {location.altitude:F1}m");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GPSLogger] Error updating UI: {e.Message}");
            UpdateStatusText("Error updating location");
        }
    }

    private void UpdateStatusText(string status)
    {
        if (arStatusText != null)
            arStatusText.text = $"Status: {status}";
    }

    private void OnDestroy()
    {
        if (Input.location.status == LocationServiceStatus.Running)
        {
            CancelInvoke(nameof(UpdateLocationUI));
            Input.location.Stop();
        }
        Debug.Log("[GPSLogger] Cleaned up location services.");
    }
}
