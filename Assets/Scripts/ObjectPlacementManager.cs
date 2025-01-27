using UnityEngine;
using UnityEngine.UI;

public class ObjectPlacementManager : MonoBehaviour
{
    [SerializeField] private Button drawObjectButton; // Assign your Draw button here in the Inspector
    [SerializeField] private GameObject meshPrefab;  // Assign your model prefab here in the Inspector
    [SerializeField] private Camera mainCamera;      // Assign your Main Camera here in the Inspector

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

        if (meshPrefab == null)
        {
            Debug.LogError("Mesh Prefab is not assigned!");
        }
    }

    private void SpawnObjectInFrontOfCamera()
    {
        if (meshPrefab == null || mainCamera == null)
        {
            Debug.LogError("Cannot spawn object: Mesh Prefab or Main Camera is missing!");
            return;
        }

        // Calculate the position in front of the camera
        Vector3 spawnPosition = mainCamera.transform.position + mainCamera.transform.forward * 2f; // 2 meters in front
        Quaternion spawnRotation = Quaternion.identity;

        // Spawn the object
        Instantiate(meshPrefab, spawnPosition, spawnRotation);
        Debug.Log("Spawned object in front of the camera.");
    }
}
