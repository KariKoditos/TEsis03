using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;
    public float distance = 5f;
    public float height = 2f;
    public float rotationDamping = 3f;
    public float heightDamping = 2f;

    void LateUpdate()
    {
        if (!player) return;

        // Calcular la rotaci�n deseada
        float wantedRotationAngle = player.eulerAngles.y;
        float wantedHeight = player.position.y + height;

        // Posici�n actual de la c�mara
        float currentRotationAngle = transform.eulerAngles.y;
        float currentHeight = transform.position.y;

        // Suavizar la rotaci�n
        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);

        // Suavizar la altura
        currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

        // Convertir �ngulo a rotaci�n
        Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

        // Posicionar la c�mara detr�s del jugador
        transform.position = player.position;
        transform.position -= currentRotation * Vector3.forward * distance;

        // Ajustar altura
        transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);

        // Mirar siempre al jugador
        transform.LookAt(player);
    }
}