using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GPSLogger : MonoBehaviour
{
    public Text latitudeText;  // Assign in Inspector
    public Text longitudeText; // Assign in Inspector
    public Text altitudeText;  // Assign in Inspector

    void Start()
    {
        StartCoroutine(StartLocationService());
    }

    IEnumerator StartLocationService()
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogError("Location services are not enabled on this device.");
            yield break;
        }

        Input.location.Start();

        // Wait until location service initializes
        while (Input.location.status == LocationServiceStatus.Initializing)
        {
            yield return new WaitForSeconds(1);
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("Unable to determine device location.");
            yield break;
        }

        // Start updating UI with location data
        InvokeRepeating(nameof(UpdateLocationUI), 0, 1);
    }

    void UpdateLocationUI()
    {
        var location = Input.location.lastData;

        latitudeText.text = $"Latitude: {location.latitude}";
        longitudeText.text = $"Longitude: {location.longitude}";
        altitudeText.text = $"Altitude: {location.altitude} meters";
    }
}
