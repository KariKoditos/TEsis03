using UnityEngine;

public class TiendaEspacial : MonoBehaviour
{
    public FPSController controladorJugador;

    public void AbrirTienda()
    {
        UIManager.instancia.MostrarTienda();
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (controladorJugador != null)
            controladorJugador.habilitarMovimiento = false;

        UIManager.instancia.ActualizarCreditos(JugadorFinanzas.instancia.creditos);
    }

    public void CerrarTienda()
    {
        UIManager.instancia.OcultarTienda();
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (controladorJugador != null)
            controladorJugador.habilitarMovimiento = true;
    }
}
