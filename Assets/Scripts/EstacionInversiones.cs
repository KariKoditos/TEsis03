using UnityEngine;

public class EstacionInversiones : MonoBehaviour
{
    [Header("Refs")]
    public FPSController controladorJugador;

    [Header("UX bloqueado")]
    [Tooltip("Mensaje cuando aún no se desbloquea inversiones.")]
    [TextArea]
    public string mensajeBloqueado =
        "Primero realiza una COMPRA, una VENTA y usa el AHORRO para desbloquear inversiones.";

    [Tooltip("Mostrar toast cuando esté bloqueado.")]
    public bool avisarConNotificacion = true;

    private bool dentro = false;

    void Start()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        dentro = true;

        
        if (JugadorFinanzas.instancia != null && JugadorFinanzas.instancia.PuedeInvertir()) //Revisa en Jugador Finanzas si ya puede invertir
        {
            AbrirPanel();
        }
        else
        {
            if (avisarConNotificacion)
                NotificationManager.Instancia?.Notify(mensajeBloqueado, NotificationType.Info);
            
        }
    }

    void AbrirPanel()
    {
        
        UIManager.instancia?.MostrarPanelInversion();
        if (controladorJugador != null) controladorJugador.habilitarMovimiento = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void CerrarPanel()
    {
        UIManager.instancia?.CerrarPanelInversiones();
        if (controladorJugador != null) controladorJugador.habilitarMovimiento = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
    }
}
