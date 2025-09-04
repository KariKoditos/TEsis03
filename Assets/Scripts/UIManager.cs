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
    public UnityEngine.UI.Image[] iconosSlots;  // tamaño 5
    public UnityEngine.UI.Button[] botonesUsar;   // tamaño 5
    public UnityEngine.UI.Button[] botonesVender; // tamaño 5
    

    [Header("Detalles de Item")]
   
    public Button botonVender;
    public Button botonUsar;

    [Header("Jugador")]
    public FPSController controladorJugador;

    [Header("Ahorro")]
    public TMP_Text textoAhorro;

    [Header("Ahorro UI")]
    public TMP_InputField inputCantidadAhorro;

    [Header("Inversiones")]
    public GameObject panelInversion;
    public TMP_InputField inputCantidadInversion;
    public TMP_Text textoCreditosInversion;

    [Header("Desbloqueo Inversiones")]
    [Tooltip("Inversiones seguras exitosas requeridas para desbloquear las riesgosas.")]
    public int segurasParaDesbloquear = 2;
    private int inversionesSegurasExitosas = 0;
    private bool riesgosDesbloqueados = false;



    System.Collections.Generic.List<ItemEspacial> _invCache;

    [Header("PaneleNotificaciones")]
    public GameObject panelNotificaciones;

    // Datos temporales de la carta seleccionada
    //private DatosInversion inversionSeleccionada;


    private bool inventarioAbierto = false;
    private int indexItemSeleccionado = -1;
    public static bool panelInversionAbierto = false;
    private bool abierto = false;


    public void Start()
    {
        

    }



    private void Awake()
    {
        if (instancia != null && instancia != this) { Destroy(gameObject); return; }
        instancia = this;
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

        if (Input.GetKeyDown(KeyCode.N))
        {
            if (!abierto)
                MostrarNotificaciones();
            else
                CerrarNotificaciones();
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


    public void MostrarNotificaciones()
    {
        panelNotificaciones.SetActive(true);
        abierto = true;

        // activar cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // bloquear movimiento
        if (controladorJugador != null)
            controladorJugador.habilitarMovimiento = false;

        // opcional: pausar juego
        Time.timeScale = 0f;
    }

    public void OcultarInventario()
    {
        inventarioAbierto = false;
        panelInventario.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
        if (controladorJugador != null)
            controladorJugador.habilitarMovimiento = true;
    }

    public void CerrarNotificaciones()
    {
        panelNotificaciones.SetActive(false);
        abierto = false;

        // ocultar cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (controladorJugador != null)
            controladorJugador.habilitarMovimiento = true;

        // reanudar juego
        Time.timeScale = 1f;

    }

    public void ActualizarInventarioUI(List<ItemEspacial> inventario)
    {
        int n = textosSlots.Length; // asume 5
        for (int i = 0; i < n; i++)
        {
            bool hayItem = (i < inventario.Count && inventario[i] != null);

            // Pintar nombre + icono
            if (hayItem)
            {
                var it = inventario[i];
                textosSlots[i].text = it.nombre;

                iconosSlots[i].sprite = it.icono;
                iconosSlots[i].preserveAspect = true;
                iconosSlots[i].color = Color.white;
            }
            else
            {
                textosSlots[i].text = "- Vacío -";
                iconosSlots[i].sprite = null;
                iconosSlots[i].color = new Color(1, 1, 1, 0);
            }

            // Asegurar listeners correctos
            if (i < botonesUsar.Length) botonesUsar[i].onClick.RemoveAllListeners();
            if (i < botonesVender.Length) botonesVender[i].onClick.RemoveAllListeners();

            if (hayItem)
            {
                int idx = i;
                var it = inventario[idx];

                if (i < botonesUsar.Length && botonesUsar[i])
                {
                    botonesUsar[i].onClick.AddListener(() =>
                        JugadorFinanzas.instancia.UsarItemPorIndice(idx));
                    bool sePuedeUsar = it.tipo == TipoItem.Necesidad || it.tipo == TipoItem.Prevención;
                    botonesUsar[i].interactable = sePuedeUsar;
                }

                if (i < botonesVender.Length && botonesVender[i])
                {
                    botonesVender[i].onClick.AddListener(() =>
                        JugadorFinanzas.instancia.Vender(idx));
                    botonesVender[i].interactable = true;
                }
            }
            else
            {
                if (i < botonesUsar.Length && botonesUsar[i]) botonesUsar[i].interactable = false;
                if (i < botonesVender.Length && botonesVender[i]) botonesVender[i].interactable = false;
            }
        }
    }

  

    public void VenderItem()
    {
        if (indexItemSeleccionado >= 0)
        {
            JugadorFinanzas.instancia.Vender(indexItemSeleccionado);
            ActualizarCreditos(JugadorFinanzas.instancia.creditos);
            ActualizarInventarioUI(JugadorFinanzas.instancia.inventario);
            
        }
    }

    public void ActualizarCreditos(int cantidad)
    {
        if (textoCreditos != null)
            textoCreditos.text = $"CRÉDITOS: {cantidad}";
        
        // if (textoCreditosAhorro != null) textoCreditosAhorro.text = $"CRÉDITOS: {cantidad}";
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


    public void OnClickUsarItem()
    {
        if (indexItemSeleccionado < 0 || JugadorFinanzas.instancia == null) return;

        JugadorFinanzas.instancia.UsarItemPorIndice(indexItemSeleccionado);

       
        indexItemSeleccionado = -1;
    }

    private bool EsUsable(ItemEspacial item)
    {
        // 1) Necesidades: solo si la barra no está llena
        if (item.tipo == TipoItem.Necesidad && item.efectoNecesidad > 0 && item.satisface != NecesidadTipo.Ninguna)
        {
            if (NeedsSystem.Instancia == null) return true;
            return NeedsSystem.Instancia.GetValor(item.satisface) < 100;
        }

        // 2) Si hay un evento activo que se resuelve con este item o su tipo
        if (EventsManager.Instancia != null && EventsManager.Instancia.PuedeUsarseParaEvento(item))
            return true;

        // 3) Ítems de prevención: si quieres permitir activarlos manualmente
        if (item.tipo == TipoItem.Prevención)
            return true;

        return false;
    }


}
