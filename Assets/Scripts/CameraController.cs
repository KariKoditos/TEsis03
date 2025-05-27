using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform cuerpoJugador; // El objeto del jugador
    public float sensibilidad = 100f;

    float rotacionX = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Oculta el cursor
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensibilidad * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidad * Time.deltaTime;

        rotacionX -= mouseY;
        rotacionX = Mathf.Clamp(rotacionX, -90f, 90f); // Evita rotación extrema

        transform.localRotation = Quaternion.Euler(rotacionX, 0f, 0f);
        cuerpoJugador.Rotate(Vector3.up * mouseX);
    }
}