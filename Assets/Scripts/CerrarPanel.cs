using UnityEngine;

public class CerrarPanel : MonoBehaviour
{
    // Este método se conecta al botón
    public void Cerrar(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(false);

            // Opcional: reanudar el juego si lo pausaste
            Time.timeScale = 1f;

            // Bloquear el cursor de nuevo (si es un panel de juego)
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Si tienes FPSController, lo reactivamos
            if (UIManager.instancia != null && UIManager.instancia.controladorJugador != null)
                UIManager.instancia.controladorJugador.habilitarMovimiento = true;
        }
    }
}
