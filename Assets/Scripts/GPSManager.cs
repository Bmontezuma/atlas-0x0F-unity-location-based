using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GPSManager : MonoBehaviour
{
    public Text latitudeText;  // Assign this Text in the Inspector
    public Text longitudeText; // Assign this Text in the Inspector
    public Text altitudeText;  // Assign this Text in the Inspector

    void Start()
    {
        // Start location service
        StartCoroutine(StartLocationService());
    }

    IEnumerator StartLocationService()
    {
        // Ensure location services are enabled
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogError("Location services are disabled. Please enable them in your device settings.");
            yield break;
        }

        // Start the location service
        Input.location.Start();

        // Wait until the location service initializes
        while (Input.location.status == LocationServiceStatus.Initializing)
        {
            yield return new WaitForSeconds(1);
        }

        // Handle failed initialization
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("Failed to initialize location services.");
            yield break;
        }

        // Update UI with GPS data every second
        InvokeRepeating(nameof(UpdateLocationUI), 0, 1f);
    }

    void UpdateLocationUI()
    {
        var location = Input.location.lastData;

        // Update the UI with latitude, longitude, and altitude
        latitudeText.text = $"Latitude: {location.latitude}";
        longitudeText.text = $"Longitude: {location.longitude}";
        altitudeText.text = $"Altitude: {location.altitude} meters";

        // Log to console for debugging
        Debug.Log($"Latitude: {location.latitude}, Longitude: {location.longitude}, Altitude: {location.altitude}");
    }

    private void OnDestroy()
    {
        // Stop location service when the script is destroyed
        Input.location.Stop();
    }
}
