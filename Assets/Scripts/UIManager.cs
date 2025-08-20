using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

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

    [Header("Detalles de Item")]
    public GameObject panelDetalles;
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

    [Header("Inversiones")]
    public Button botonInversion;
    public GameObject panelInversion;
    public TMP_InputField inputCantidadInversion;
    public TMP_Text textoCreditosInversion;

    [Header("Desbloqueo Inversiones")]
    [Tooltip("Inversiones seguras exitosas requeridas para desbloquear las riesgosas.")]
    public int segurasParaDesbloquear = 2;
    private int inversionesSegurasExitosas = 0;
    private bool riesgosDesbloqueados = false;

    [Header("Panel de Inversión Individual")]
    public GameObject panelInputInversion;
    public TMP_InputField inputCantidad;
    public TMP_Text textoNombreInversion;
    public Button botonConfirmarInversion;

    // Datos temporales de la carta seleccionada
    //private DatosInversion inversionSeleccionada;


    private bool inventarioAbierto = false;
    private int indexItemSeleccionado = -1;
    public static bool panelInversionAbierto = false;


    public void Start()
    {
        botonInversion.gameObject.SetActive(false);

    }



    private void Awake()
    {
        if (instancia == null) instancia = this;
        else Destroy(gameObject);
    }



    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (inventarioAbierto)
                OcultarInventario();
            else
                MostrarInventario();
        }

        if (Input.GetKeyDown(KeyCode.P)) 
        {
            if (!panelInversion.activeSelf)
            {
                MostrarPanelInversion();
            }
            else
            {
                CerrarPanelInversiones();
            }

        }

    }

    void TogglePanelInversiones()
    {
        bool activo = panelInversion.activeSelf;
        panelInversion.SetActive(!activo);

        // Mostrar o esconder el cursor dependiendo del estado
        Cursor.lockState = activo ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !activo;
    }

    public void MostrarInventario()
    {
        inventarioAbierto = true;
        panelInventario.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
        if (controladorJugador != null)
            controladorJugador.habilitarMovimiento = false;
        ActualizarInventarioUI(JugadorFinanzas.instancia.inventario);
    }

    public void OcultarInventario()
    {
        inventarioAbierto = false;
        panelInventario.SetActive(false);
        panelDetalles.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
        if (controladorJugador != null)
            controladorJugador.habilitarMovimiento = true;
    }

    public void ActualizarInventarioUI(List<ItemEspacial> inventario)
    {
        for (int i = 0; i < textosSlots.Length; i++)
        {
            if (i < inventario.Count)
            {
                textosSlots[i].text = inventario[i].nombre;
                iconosSlots[i].sprite = inventario[i].icono;
                iconosSlots[i].color = Color.white;
            }
            else
            {
                textosSlots[i].text = "- Vacío -";
                iconosSlots[i].sprite = null;
                iconosSlots[i].color = new Color(1, 1, 1, 0);
            }
        }
    }

    public void MostrarDetallesItem(ItemEspacial item, int index)
    {
        panelDetalles.SetActive(true);
        imagenGrande.sprite = item.icono;
        textoNombre.text = item.nombre;
        Debug.Log($"Descripción cargada: {item.descripcion}");
        textoDescripcion.text = item.descripcion;
        textoValorVenta.text = $"Venta: ${item.valorVenta}";

        indexItemSeleccionado = index;

        botonVender.onClick.RemoveAllListeners();
        botonVender.onClick.AddListener(() => VenderItem());
    }

    public void VenderItem()
    {
        if (indexItemSeleccionado >= 0)
        {
            JugadorFinanzas.instancia.Vender(indexItemSeleccionado);
            ActualizarCreditos(JugadorFinanzas.instancia.creditos);
            ActualizarInventarioUI(JugadorFinanzas.instancia.inventario);
            panelDetalles.SetActive(false);
        }
    }

    public void ActualizarCreditos(int cantidad)
    {
        if (textoCreditos != null)
            textoCreditos.text = $"CRÉDITOS: {cantidad}";

        if (textoCreditosAhorro != null)
            textoCreditosAhorro.text = $"CRÉDITOS: {cantidad}";
    }

    public void ActualizarAhorro(int cantidad)
    {
        if (textoAhorro != null)
            textoAhorro.text = $"AHORRO: {cantidad}";
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

    public void ActivarBotonInversion()
    {
        if (botonInversion != null)
        {
            botonInversion.gameObject.SetActive(true);
            Debug.Log("Botón de inversión activado");
        }
        else
        {
            Debug.LogWarning("No se asignó el botón de inversión en el inspector");
        }
    }

    public void MostrarPanelInversion()
    {
        panelInversion.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        controladorJugador.habilitarMovimiento = false;

        RefrescarUIEconomia();
        RefrescarInteractividadCartas();
    }

    public void CerrarPanelInversiones()
    {
        panelInversion.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        controladorJugador.habilitarMovimiento = true;
    }

    public int GetCreditos()
    {
        return JugadorFinanzas.instancia != null ? JugadorFinanzas.instancia.creditos : 0;
    }

    public bool TryGastarCreditos(int monto)
    {
        if (monto <= 0) return false;
        if (JugadorFinanzas.instancia == null) return false;
        if (JugadorFinanzas.instancia.creditos < monto) return false;

        JugadorFinanzas.instancia.creditos -= monto;
        RefrescarUIEconomia();
        return true;
    }

    public void AgregarCreditos(int cantidad)
    {
        if (cantidad == 0 || JugadorFinanzas.instancia == null) return;
        JugadorFinanzas.instancia.creditos = Mathf.Max(0, JugadorFinanzas.instancia.creditos + cantidad);
        RefrescarUIEconomia();
    }


    private void RefrescarUIEconomia()
    {
        // Refresca textos principales
        ActualizarCreditos(JugadorFinanzas.instancia.creditos);

        // Refresca el texto de créditos visible dentro del panel de inversiones
        if (textoCreditosInversion != null)
            textoCreditosInversion.text = $"CRÉDITOS: {JugadorFinanzas.instancia.creditos}";
    }

    // === Desbloqueo de riesgosas ===
    public void RegistrarInversionSeguraExitosa()
    {
        inversionesSegurasExitosas++;
        if (!riesgosDesbloqueados && inversionesSegurasExitosas >= segurasParaDesbloquear)
        {
            riesgosDesbloqueados = true;
            Debug.Log("¡Riesgosas desbloqueadas!");
            RefrescarInteractividadCartas(); //  habilita el botón Invertir de las riesgosas
        }
    }

    public bool RiesgosDesbloqueados()
    {
        return riesgosDesbloqueados;
    }

    public void RefrescarInteractividadCartas()
    {
        // Incluye inactivos en la escena (por si el panel está cerrado)
        var cartas = GameObject.FindObjectsOfType<CartaInversion>(true);
        foreach (var carta in cartas)
            carta.ActualizarInteractividad();
    }

}
