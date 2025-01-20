using System.Collections;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.XR.ARFoundation;
using TMPro;

public class GPSManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI latitudeText;
    [SerializeField] private TextMeshProUGUI longitudeText;
    [SerializeField] private TextMeshProUGUI altitudeText;
    [SerializeField] private TextMeshProUGUI arPositionText;

    private Vector2 referenceCoordinates = new Vector2(36.154f, -95.931f);
    private ARSession arSession;
    private ARCameraManager arCameraManager;

    private void Awake()
    {
        Debug.Log("[GPSManager] Initializing...");
        
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
        arCameraManager = FindObjectOfType<ARCameraManager>();

        if (arSession == null || arCameraManager == null)
        {
            Debug.LogError("[GPSManager] Required AR components not found!");
            return;
        }
    }

    private void Start()
    {
        Debug.Log("[GPSManager] Starting...");
        
        if (!IsValidCoordinate(referenceCoordinates))
        {
            Debug.LogError("[GPSManager] Invalid reference coordinates!");
            return;
        }

        StartCoroutine(InitializeAR());
    }

    private IEnumerator InitializeAR()
    {
        Debug.Log("[GPSManager] Waiting for AR initialization...");
        yield return new WaitUntil(() => ARSession.state == ARSessionState.Ready);
        
        Debug.Log("[GPSManager] AR initialized successfully.");
        StartCoroutine(StartLocationService());
    }

    private IEnumerator StartLocationService()
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogError("[GPSManager] Location services disabled.");
            yield break;
        }

        Input.location.Start();
        Debug.Log("[GPSManager] Initializing location services...");

        while (Input.location.status == LocationServiceStatus.Initializing)
        {
            yield return new WaitForSeconds(1);
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("[GPSManager] Failed to initialize location services.");
            yield break;
        }

        Debug.Log("[GPSManager] Location services initialized. Starting updates...");
        InvokeRepeating(nameof(UpdateLocationUI), 0, 1f);
    }

    private void UpdateLocationUI()
    {
        if (Input.location.status != LocationServiceStatus.Running)
        {
            Debug.LogWarning("[GPSManager] Location services not running!");
            return;
        }

        try
        {
            var location = Input.location.lastData;
            Vector2 currentCoordinates = new Vector2(location.latitude, location.longitude);
            
            if (latitudeText != null)
                latitudeText.text = $"Latitude: {location.latitude:F6}";
            if (longitudeText != null)
                longitudeText.text = $"Longitude: {location.longitude:F6}";
            if (altitudeText != null)
                altitudeText.text = $"Altitude: {location.altitude:F1}m";

            // Get AR Camera position for reference
            if (arCameraManager != null && arPositionText != null)
            {
                Vector3 arPosition = arCameraManager.transform.position;
                arPositionText.text = $"AR Position: {arPosition:F2}";
            }

            Debug.Log($"[GPSManager] Location updated - Lat: {location.latitude:F6}, Long: {location.longitude:F6}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GPSManager] Error updating location UI: {e.Message}");
        }
    }

    private bool IsValidCoordinate(Vector2 coord)
    {
        return coord.x >= -90f && coord.x <= 90f && // latitude
               coord.y >= -180f && coord.y <= 180f;  // longitude
    }

    private void OnDestroy()
    {
        if (Input.location.status == LocationServiceStatus.Running)
        {
            CancelInvoke(nameof(UpdateLocationUI));
            Input.location.Stop();
        }
        Debug.Log("[GPSManager] Cleaned up location services.");
    }
}