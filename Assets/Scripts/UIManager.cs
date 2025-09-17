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
    public UnityEngine.UI.Image[] iconosSlots;  
    public UnityEngine.UI.Button[] botonesUsar;   
    public UnityEngine.UI.Button[] botonesVender; 
    

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

       
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        
        if (controladorJugador != null)
            controladorJugador.habilitarMovimiento = false;

       
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
        int n = textosSlots.Length;

        for (int i = 0; i < n; i++)
        {
            bool hayItem = (i < inventario.Count && inventario[i] != null);

            
            if (hayItem)
            {
                ItemEspacial it = inventario[i];
                if (textosSlots[i] != null) textosSlots[i].text = it.nombre;

                if (iconosSlots[i] != null)
                {
                    iconosSlots[i].sprite = it.icono;
                    iconosSlots[i].preserveAspect = true;
                    iconosSlots[i].color = Color.white;
                }
            }
            else
            {
                if (textosSlots[i] != null) textosSlots[i].text = "- Vacío -";
                if (iconosSlots[i] != null)
                {
                    iconosSlots[i].sprite = null;
                    iconosSlots[i].color = new Color(1f, 1f, 1f, 0f);
                }
            }

            // Limpiar listeners previos 
            if (i < botonesUsar.Length && botonesUsar[i] != null)
                botonesUsar[i].onClick.RemoveAllListeners();

            if (i < botonesVender.Length && botonesVender[i] != null)
                botonesVender[i].onClick.RemoveAllListeners();

            
            if (hayItem)
            {
                // USAR: solo si el tipo permite uso (Necesidad / Prevención)
                if (i < botonesUsar.Length && botonesUsar[i] != null)
                {
                    ItemEspacial it = inventario[i];
                    bool sePuedeUsar = (it.tipo == TipoItem.Necesidad || it.tipo == TipoItem.Prevención);
                    botonesUsar[i].interactable = sePuedeUsar;

                    if (sePuedeUsar)
                    {
                        // Sin lambdas: listener por índice fijo
                        if (i == 0) botonesUsar[i].onClick.AddListener(UsarSlot0);
                        else if (i == 1) botonesUsar[i].onClick.AddListener(UsarSlot1);
                        else if (i == 2) botonesUsar[i].onClick.AddListener(UsarSlot2);
                        else if (i == 3) botonesUsar[i].onClick.AddListener(UsarSlot3);
                        else if (i == 4) botonesUsar[i].onClick.AddListener(UsarSlot4);
                    }
                }

                // VENDER: siempre permitido si hay ítem
                if (i < botonesVender.Length && botonesVender[i] != null)
                {
                    botonesVender[i].interactable = true;

                    if (i == 0) botonesVender[i].onClick.AddListener(VenderSlot0);
                    else if (i == 1) botonesVender[i].onClick.AddListener(VenderSlot1);
                    else if (i == 2) botonesVender[i].onClick.AddListener(VenderSlot2);
                    else if (i == 3) botonesVender[i].onClick.AddListener(VenderSlot3);
                    else if (i == 4) botonesVender[i].onClick.AddListener(VenderSlot4);
                }
            }
            else
            {
                // No hay ítem: deshabilitar botones
                if (i < botonesUsar.Length && botonesUsar[i] != null)
                    botonesUsar[i].interactable = false;

                if (i < botonesVender.Length && botonesVender[i] != null)
                    botonesVender[i].interactable = false;
            }
        }
    }

    
    // USAR
    private void UsarSlot0() { EjecutarUsarCommand(0); }
    private void UsarSlot1() { EjecutarUsarCommand(1); }
    private void UsarSlot2() { EjecutarUsarCommand(2); }
    private void UsarSlot3() { EjecutarUsarCommand(3); }
    private void UsarSlot4() { EjecutarUsarCommand(4); }

    // VENDER
    private void VenderSlot0() { EjecutarVenderCommand(0); }
    private void VenderSlot1() { EjecutarVenderCommand(1); }
    private void VenderSlot2() { EjecutarVenderCommand(2); }
    private void VenderSlot3() { EjecutarVenderCommand(3); }
    private void VenderSlot4() { EjecutarVenderCommand(4); }

  
    private void EjecutarUsarCommand(int idx)
    {
        // encapsula la acción "Usar ítem"
        UsarItemCommand cmd = new UsarItemCommand(idx);

        if (cmd.CanExecute())
        {
            cmd.Execute(); // Internamente llama a JugadorFinanzas.UsarItemPorIndice(idx)  refrescamos UI para reflejar inventario/creditos actualizados

            ActualizarInventarioUI(JugadorFinanzas.instancia.inventario);
            ActualizarCreditos(JugadorFinanzas.instancia.creditos);
        }
        else
        {
            NotificationManager.Instancia?.Notify("No se puede usar ese slot.", NotificationType.Warning, 2.5f);
        }
    }

    private void EjecutarVenderCommand(int idx)
    {
        //encapsula la acción "Vender ítem"
        VenderCommand cmd = new VenderCommand(idx);

        if (cmd.CanExecute())
        {
            cmd.Execute(); // Internamente llama a JugadorFinanzas.Vender(idx) Refrescar UI tras la venta

            ActualizarInventarioUI(JugadorFinanzas.instancia.inventario);
            ActualizarCreditos(JugadorFinanzas.instancia.creditos);
        }
        else
        {
            NotificationManager.Instancia?.Notify("No se puede vender ese slot.", NotificationType.Warning, 2.5f);
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
        if (textoCreditos != null)  textoCreditos.text = $"CRÉDITOS: {cantidad}";

    }

    public void ActualizarAhorro(int cantidad)
    {
        if (textoAhorro != null) textoAhorro.text = $"AHORRO: {cantidad}";

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

        return false;
    }


}
