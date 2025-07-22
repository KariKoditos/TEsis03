using UnityEngine;

public class PlanetRotation : MonoBehaviour
{
    [Header("Velocidad de rotación (grados por segundo)")]
    public float rotationSpeed = 10f;

    void Update()
    {
        // Rota el planeta sobre su eje Y
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.Self);
    }
}
