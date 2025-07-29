using UnityEngine;

public class TiendaEspacial : MonoBehaviour
{
    public FPSController controladorJugador;

    [Header("Items en Venta")]
    public ItemEspacial[] itemsEnVenta;

    [Header("Inflación")]
    public float incrementoPorciento = 5f; 
    public float tiempoInflacion = 60f;    
         

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

    private void AplicarInflacion()
    {
        foreach (var item in itemsEnVenta)
        {
            // Incrementar precio y valor de venta
            item.costo += Mathf.RoundToInt(item.costo * (incrementoPorciento / 100f));
            item.valorVenta += Mathf.RoundToInt(item.valorVenta * (incrementoPorciento / 100f));
        }

        Debug.Log($" Inflación aplicada: +{incrementoPorciento}% a todos los precios.");
        UIManager.instancia.ActualizarPrecios(itemsEnVenta);
    }

}
