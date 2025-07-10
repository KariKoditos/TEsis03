using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TiendaEspacial : MonoBehaviour
{
    public GameObject panelTienda;
    public JugadorFinanzas jugador;
    public TMP_Text textoCreditos;
    public ItemEspacial[] itemsEnVenta;
    public GameObject botonPrefab;
    public FPSController controladorJugador;



    void Start()
    {
        panelTienda.SetActive(false);
        ActualizarCreditos();
        
    }

   
    public void Comprar(ItemEspacial item)
    {
        jugador.Comprar(item);
        ActualizarCreditos();
    }

    public void AbrirTienda()
    {
        panelTienda.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0;
        controladorJugador.habilitarMovimiento = false;
    }

    public void CerrarTienda()
    {
        panelTienda.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1;
        controladorJugador.habilitarMovimiento = true;
    }

    void ActualizarCreditos()
    {
        textoCreditos.text = $"Créditos: {jugador.creditos}";
    }
}
