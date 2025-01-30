using UnityEngine;

public class EarthRotation : MonoBehaviour
{
    [SerializeField] private float speedMultiplier = 1000f; // Adjust this to control speed

    private const float RealRotationSpeed = 360f / 86164f; // Real Earth's rotation (degrees per second)

    private void Update()
    {
        float adjustedSpeed = RealRotationSpeed * speedMultiplier; // Speed up the rotation
        transform.Rotate(Vector3.up, adjustedSpeed * Time.deltaTime, Space.World);
    }
}
