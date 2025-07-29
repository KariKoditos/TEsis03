using UnityEngine;

public class BancoEspacial : MonoBehaviour
{
    public GameObject panelAhorro;       // Panel UI de Ahorro
    public FPSController controladorJugador;

    private void Start()
    {
        if (panelAhorro != null)
            panelAhorro.SetActive(false); // Asegurar que inicie desactivado
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AbrirAhorro();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CerrarAhorro();
        }
    }

    public void AbrirAhorro()
    {
        if (panelAhorro == null) return;

        panelAhorro.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (controladorJugador != null)
            controladorJugador.habilitarMovimiento = false;

        UIManager.instancia.ActualizarAhorro(JugadorFinanzas.instancia.saldoAhorro);
        Debug.Log("Panel de ahorro abierto.");
    }

    public void CerrarAhorro()
    {
        if (panelAhorro == null) return;

        panelAhorro.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (controladorJugador != null)
            controladorJugador.habilitarMovimiento = true;

        Debug.Log("Panel de ahorro cerrado.");
    }
}
