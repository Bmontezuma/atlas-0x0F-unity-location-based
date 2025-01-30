using UnityEngine;
using UnityEngine.UI;

public class ObjectPlacementManager : MonoBehaviour
{
    [SerializeField] private Button drawObjectButton; // Assign your Draw button in the Inspector
    [SerializeField] public GameObject firstMeshPrefab;
    [SerializeField] public GameObject secondMeshPrefab;
    [SerializeField] private Camera mainCamera;      // Assign your Main Camera in the Inspector

    private Vector2 firstSpawnPoint;
    private Vector2 secondSpawnPoint;
    private bool hasFirstSpawn = false;
    private bool hasSecondSpawn = false;

    private void Start()
    {
        // Ensure the button and prefab are set
        if (drawObjectButton != null)
        {
            drawObjectButton.onClick.AddListener(SpawnObjectInFrontOfCamera);
        }
        else
        {
            Debug.LogError("Draw Object Button is not assigned!");
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main; // Automatically assign the Main Camera if not set
        }

        if (firstMeshPrefab == null || secondMeshPrefab == null)
        {
            Debug.LogError("One or both Mesh Prefabs are not assigned!");
        }
    }

    private void SpawnObjectInFrontOfCamera()
    {
        if (firstMeshPrefab == null || mainCamera == null)
        {
            Debug.LogError("Cannot spawn object: Mesh Prefab or Main Camera is missing!");
            return;
        }

        // Calculate the position in front of the camera
        Vector3 spawnPosition = mainCamera.transform.position + mainCamera.transform.forward * 2f; // 2 meters in front
        Quaternion spawnRotation = Quaternion.identity;

        // Spawn the object
        Instantiate(firstMeshPrefab, spawnPosition, spawnRotation);
        Debug.Log("Spawned object in front of the camera.");
    }

    public void SetFirstSpawnPoint(Vector2 gpsCoords)
    {
        firstSpawnPoint = gpsCoords;
        hasFirstSpawn = true;
        Debug.Log($"First spawn point set at: {firstSpawnPoint.x}, {firstSpawnPoint.y}");
    }

    public void SetSecondSpawnPoint(Vector2 gpsCoords)
    {
        secondSpawnPoint = gpsCoords;
        hasSecondSpawn = true;
        Debug.Log($"Second spawn point set at: {secondSpawnPoint.x}, {secondSpawnPoint.y}");
    }

    public void SpawnObjectAtGPSLocation(Vector2 gpsCoords, GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("Cannot spawn object: Prefab is missing!");
            return;
        }

        Vector3 unityPosition = GPSEncoder.GPSToUCS(gpsCoords);
        Instantiate(prefab, unityPosition, Quaternion.identity);
        Debug.Log($"Spawned {prefab.name} at GPS location: {gpsCoords.x}, {gpsCoords.y}");
    }

    public void SpawnObjectsAtGPSLocations()
    {
        if (firstMeshPrefab == null || secondMeshPrefab == null)
        {
            Debug.LogError("Cannot spawn objects: One or both Mesh Prefabs are missing!");
            return;
        }

        if (hasFirstSpawn)
        {
            Vector3 unityPosition1 = GPSEncoder.GPSToUCS(firstSpawnPoint);
            Instantiate(firstMeshPrefab, unityPosition1, Quaternion.identity);
            Debug.Log($"Spawned {firstMeshPrefab.name} at first GPS location: {firstSpawnPoint.x}, {firstSpawnPoint.y}");
        }

        if (hasSecondSpawn)
        {
            Vector3 unityPosition2 = GPSEncoder.GPSToUCS(secondSpawnPoint);
            Instantiate(secondMeshPrefab, unityPosition2, Quaternion.identity);
            Debug.Log($"Spawned {secondMeshPrefab.name} at second GPS location: {secondSpawnPoint.x}, {secondSpawnPoint.y}");
        }
    }
}
