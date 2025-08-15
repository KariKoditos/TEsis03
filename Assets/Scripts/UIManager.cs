using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class UIManager : MonoBehaviour
{
    public static UIManager instancia;

    [Header("UI General")]
    public TMP_Text textoCreditos;
    public TMP_Text textoCreditosAhorro;
    public GameObject canvasTienda;       

    [Header("Inventario")]
    public GameObject panelInventario;    
    public TMP_Text[] textosSlots;
    public Image[] iconosSlots;
    

    [Header("Panel de Detalles del Inventario")]
    public GameObject panel;
    public Image imagenGrande;
    public TMP_Text textoNombre;
    public TMP_Text textoDescripcion;
    public TMP_Text textoValorVenta;
    public Button botonVender;
    public Button botonUsar;

    [Header("Jugador")]
    public FPSController controladorJugador;

    [Header("Ahorro")]
    public TMP_Text textoAhorro;

    [Header("Ahorro UI")]
    public TMP_InputField inputCantidadAhorro;

    private bool inventarioAbierto = false;
    private ItemEspacial itemSeleccionado;
    private int indexSeleccionado;
    


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
        
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventario();
        }
    }


    public void ActualizarCreditos(int cantidad)
    {
        if (textoCreditos != null)
            textoCreditos.text = $"CRÉDITOS: {cantidad}";

        if (textoCreditosAhorro != null)
            textoCreditosAhorro.text = $"CRÉDITOS: {cantidad}";
    }



    public void ActualizarInventarioUI(List<ItemEspacial> inventario)
    {
        for (int i = 0; i < textosSlots.Length; i++)
        {
            if (i < inventario.Count)
            {
                
                textosSlots[i].text = inventario[i].nombre;


                textoValorVenta.text = $"Venta: ${inventario[i].valorVenta}";


                if (iconosSlots[i] != null)
                {
                    iconosSlots[i].sprite = inventario[i].icono;
                    iconosSlots[i].color = Color.white;
                }
            }
            else
            {
                
                textosSlots[i].text = "- Vacío -";
                //textosValorVenta[i].text = "";


                if (iconosSlots[i] != null)
                {
                    iconosSlots[i].sprite = null;
                    iconosSlots[i].color = new Color(1, 1, 1, 0);
                }
            }
        }
    }

    public void MostrarDetalles(ItemEspacial item, int indice)
    {
        if (panel != null) panel.SetActive(true);

        if (imagenGrande != null) imagenGrande.sprite = item.icono;
        if (textoDescripcion != null) textoDescripcion.text = item.descripcion;
        if (textoValorVenta != null) textoValorVenta.text = $"Venta: ${item.valorVenta}";

        itemSeleccionado = item;

        // Configurar botón Vender
        botonVender.onClick.RemoveAllListeners();
        botonVender.onClick.AddListener(() => VenderSlot(indexSeleccionado));

        // Configurar botón Usar
        botonUsar.onClick.RemoveAllListeners();
        botonUsar.onClick.AddListener(() => UsarItem(itemSeleccionado));
    }

    public void UsarItem(ItemEspacial item)
    {
        // Ejemplo de efecto: aumentar barra de comida (esto lo implementarás después)
        Debug.Log($"Usaste el item: {item.nombre}");

        // También puedes quitarlo del inventario si deseas
        JugadorFinanzas.instancia.inventario.Remove(item);

        // Actualizar UI después de usar
        ActualizarInventarioUI(JugadorFinanzas.instancia.inventario);
        ActualizarCreditos(JugadorFinanzas.instancia.creditos);

        // Ocultar panel de detalles
        if (panel != null)
            panel.SetActive(false);

    }


    public void VenderDesdeDetalle()
    {
        JugadorFinanzas.instancia.Vender(indexSeleccionado);
        ActualizarCreditos(JugadorFinanzas.instancia.creditos);
        ActualizarInventarioUI(JugadorFinanzas.instancia.inventario);

        if (panel != null)
            panel.SetActive(false);
    }




    public void ActualizarAhorro(int cantidad)
    {
        if (textoAhorro != null)
            textoAhorro.text = $"AHORRO: {cantidad}";
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

    
    public void ActualizarPrecios(ItemEspacial[] items)
    {
        ButtonPrecio[] botones = canvasTienda.GetComponentsInChildren<ButtonPrecio>();

        for (int i = 0; i < botones.Length && i < items.Length; i++)
        {
            botones[i].ActualizarTexto(items[i]);
        }
    }


    public void DepositarDesdeUI()
    {
        int cantidad;
        if (int.TryParse(inputCantidadAhorro.text, out cantidad))
            JugadorFinanzas.instancia.Depositar(cantidad);
    }

    public void RetirarDesdeUI()
    {
        int cantidad;
        if (int.TryParse(inputCantidadAhorro.text, out cantidad))
            JugadorFinanzas.instancia.Retirar(cantidad);
    }

}
