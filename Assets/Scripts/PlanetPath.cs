using UnityEngine;

public class PlanetPath : MonoBehaviour
{
    public Transform[] waypoints;  // Puntos de la trayectoria
    public float speed = 2f;       // Velocidad de movimiento
    public bool loop = true;       // Si el movimiento se repite en bucle

    private int currentWaypoint = 0;

    void Update()
    {
        if (waypoints.Length == 0) return;

        // Mueve el planeta hacia el waypoint actual
        transform.position = Vector3.MoveTowards(
            transform.position,
            waypoints[currentWaypoint].position,
            speed * Time.deltaTime
        );

        // Si llega al waypoint, pasa al siguiente
        if (Vector3.Distance(transform.position, waypoints[currentWaypoint].position) < 0.1f)
        {
            currentWaypoint++;
            if (currentWaypoint >= waypoints.Length)
            {
                currentWaypoint = loop ? 0 : waypoints.Length - 1;
            }
        }
    }
}
