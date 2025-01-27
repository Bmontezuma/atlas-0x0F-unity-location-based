using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshSpawner : MonoBehaviour
{
    public GameObject meshPrefab; // Assign your 3D mesh prefab in the Inspector
    public Vector3 destinationPosition; // Set the desired destination in the Inspector

    private void Start()
    {
        if (meshPrefab == null)
        {
            Debug.LogError("Mesh Prefab is not assigned!");
            return;
        }

        // Instantiate the mesh at the current position
        InstantiateMeshAtPosition(transform.position);

        // Instantiate the mesh at the destination position
        InstantiateMeshAtPosition(destinationPosition);
    }

    private void InstantiateMeshAtPosition(Vector3 position)
    {
        // Instantiate the prefab at the given position with no rotation
        GameObject meshInstance = Instantiate(meshPrefab, position, Quaternion.identity);
        meshInstance.name = "MeshInstance_" + position; // Give it a unique name
    }
}

