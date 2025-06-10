using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TiendaEspacial : MonoBehaviour
{
    public GameObject panelTienda;
    public JugadorFinanzas jugador;
    public TMP_Text textoCreditos;
    public ItemEspacial[] itemsEnVenta;
    public Transform contenedorBotones;
    public GameObject botonPrefab;

    

    void Start()
    {
        panelTienda.SetActive(false);
        ActualizarCreditos();
        GenerarBotones();
    }

    void GenerarBotones()
    {
        foreach (ItemEspacial item in itemsEnVenta)
        {
            GameObject boton = Instantiate(botonPrefab, contenedorBotones);
            boton.GetComponentInChildren<Text>().text = $"{item.nombre} - ${item.costo}";
            boton.GetComponent<Button>().onClick.AddListener(() => Comprar(item));
        }
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
    }

    public void CerrarTienda()
    {
        panelTienda.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1;
    }

    void ActualizarCreditos()
    {
        textoCreditos.text = $"Créditos: {jugador.creditos}";
    }
}
