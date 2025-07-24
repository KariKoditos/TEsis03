using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instancia;

    [Header("UI General")]
    public TMP_Text textoCreditos;        // Texto para créditos
    public GameObject canvasTienda;       // Panel completo de la tienda

    [Header("Inventario")]
    public GameObject panelInventario;    // Panel de inventario
    public TMP_Text[] textosSlots;        // Textos de los 5 botones de inventario

    [Header("Jugador")]
    public FPSController controladorJugador;

    private bool inventarioAbierto = false;

    private void Awake()
    {
        if (instancia == null)
            instancia = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (canvasTienda != null)
            canvasTienda.SetActive(false);

        if (panelInventario != null)
            panelInventario.SetActive(false);

        ActualizarCreditos(JugadorFinanzas.instancia.creditos);
        ActualizarInventarioUI(JugadorFinanzas.instancia.inventario);
    }

    private void Update()
    {
        // Alternar Inventario con la tecla "I"
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventario();
        }
    }

    // --- Créditos ---
    public void ActualizarCreditos(int cantidad)
    {
        if (textoCreditos != null)
            textoCreditos.text = $"CRÉDITOS: {cantidad}";
    }

    // --- Inventario ---
    public void ActualizarInventarioUI(List<ItemEspacial> inventario)
    {
        for (int i = 0; i < textosSlots.Length; i++)
        {
            if (i < inventario.Count)
                textosSlots[i].text = inventario[i].nombre;
            else
                textosSlots[i].text = "- Vacío -";
        }
    }

    public void VenderSlot(int slotIndex)
    {
        JugadorFinanzas.instancia.Vender(slotIndex);
    }

    public void ToggleInventario()
    {
        inventarioAbierto = !inventarioAbierto;
        panelInventario.SetActive(inventarioAbierto);

        if (inventarioAbierto)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
            if (controladorJugador != null)
                controladorJugador.habilitarMovimiento = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
            if (controladorJugador != null)
                controladorJugador.habilitarMovimiento = true;
        }
    }

    // --- Tienda ---
    public void MostrarTienda()
    {
        if (canvasTienda != null)
            canvasTienda.SetActive(true);
    }

    public void OcultarTienda()
    {
        if (canvasTienda != null)
            canvasTienda.SetActive(false);
    }

    // --- Inflación ---
    public void ActualizarPrecios(ItemEspacial[] items)
    {
        ButtonPrecio[] botones = canvasTienda.GetComponentsInChildren<ButtonPrecio>();

        for (int i = 0; i < botones.Length && i < items.Length; i++)
        {
            botones[i].ActualizarTexto(items[i]);
        }
    }

}
