using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CoordinateManager : MonoBehaviour
{
    private Vector2 currentCoordinates;
    private Vector2 storedCoordinates;

    [SerializeField] private Text gpsLog; // Assign a Text UI element to display the log

    private void Start()
    {
        StartCoroutine(StartLocationService());
    }

    private IEnumerator StartLocationService()
    {
        if (!Input.location.isEnabledByUser)
        {
            LogToUI("Location services are not enabled.");
            yield break;
        }

        Input.location.Start();

        while (Input.location.status == LocationServiceStatus.Initializing && Input.location.status != LocationServiceStatus.Failed)
        {
            yield return new WaitForSeconds(1);
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            LogToUI("Failed to initialize location service.");
        }
        else
        {
            LogToUI("Location service started successfully.");
        }
    }

    private void Update()
    {
        if (Input.location.status == LocationServiceStatus.Running)
        {
            var location = Input.location.lastData;
            currentCoordinates = new Vector2(location.latitude, location.longitude);
        }
    }

    public void StoreCurrentCoordinates()
    {
        storedCoordinates = currentCoordinates;
        LogToUI($"Stored Coordinates: Lat {storedCoordinates.x}, Lon {storedCoordinates.y}");
    }

    public void CalculateDistance()
    {
        if (storedCoordinates != Vector2.zero)
        {
            float distance = HaversineDistance(storedCoordinates, currentCoordinates);
            LogToUI($"Distance between points: {distance:F2} meters");
        }
        else
        {
            LogToUI("No stored coordinates to calculate distance from.");
        }
    }

    private float HaversineDistance(Vector2 coord1, Vector2 coord2)
    {
        const float R = 6371000; // Radius of the Earth in meters
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

    private void LogToUI(string message)
    {
        Debug.Log(message);
        if (gpsLog != null)
        {
            gpsLog.text += $"{message}\n";
        }
    }
}
