using UnityEngine;
using System.Collections.Generic;

public class TiendaEspacial : MonoBehaviour
{
    public FPSController controladorJugador;

    [Header("Pool de Ítems disponibles")]
    public ItemEspacial[] poolDeItems;
    public bool autoCargarResources = true; 

    [Header("Slots de venta por rotación")]
    [Range(1, 12)] public int cantidadSlots = 4;   
    public bool forzarCategorias = true;           
    public ShopItem[] slots;                       

    [Header("Inflación")]
    public float incrementoPorciento = 5f;
    public float tiempoInflacion = 60f;
    public bool inflacionEnTiempoReal = true;

    [Header("Rotación automática")]
    public float tiempoRotacion = 50f; 
    public bool rotacionEnTiempoReal = true;

    // rotación actual (COPIAS, para no tocar los assets originales)
    public ItemEspacial[] itemsEnVenta;

    void Start()
    {
        if ((poolDeItems == null || poolDeItems.Length == 0) && autoCargarResources)
        {
            poolDeItems = Resources.LoadAll<ItemEspacial>("Items");
            Debug.Log($"TiendaEspacial: cargados {poolDeItems.Length} items desde Resources/Items");
        }

        RotarItems();

        if (inflacionEnTiempoReal)
            StartCoroutine(InflacionRealtime());
        else
            InvokeRepeating(nameof(AplicarInflacion), tiempoInflacion, tiempoInflacion);
    }

    
    public void AbrirTienda()
    {
        // objetos nuevos cada q se abra la tienda
        RotarItems();

        UIManager.instancia.MostrarTienda();
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (controladorJugador) controladorJugador.habilitarMovimiento = false;

        UIManager.instancia.ActualizarCreditos(JugadorFinanzas.instancia.creditos);
        RefrescarUI();
    }

    public void CerrarTienda()
    {
        UIManager.instancia.OcultarTienda();
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (controladorJugador) controladorJugador.habilitarMovimiento = true;
    }

    
    public void RotarItems()
    {
        if (poolDeItems == null || poolDeItems.Length == 0)
        {
            Debug.LogWarning("TiendaEspacial: no hay ítems en el pool para rotar.");
            return;
        }

        var seleccion = new List<ItemEspacial>(cantidadSlots);

        if (forzarCategorias)
        {
            TryAddRandomDeTipo(seleccion, TipoItem.Necesidad);
            if (seleccion.Count < cantidadSlots) TryAddRandomDeTipo(seleccion, TipoItem.Lujo);
            if (seleccion.Count < cantidadSlots) TryAddRandomDeTipo(seleccion, TipoItem.Prevención);
            if (seleccion.Count < cantidadSlots) TryAddRandomDeTipo(seleccion, TipoItem.Inversión);
        }

        // ruleta sin duplicar items en tienda
        var usados = new HashSet<ItemEspacial>(seleccion);
        int guard = 200;
        while (seleccion.Count < cantidadSlots && guard-- > 0)
        {
            var pick = poolDeItems[Random.Range(0, poolDeItems.Length)];
            if (!usados.Contains(pick)) { seleccion.Add(pick); usados.Add(pick); }
        }

        // es para las copias y no moverle a los assets
        itemsEnVenta = new ItemEspacial[seleccion.Count];
        for (int i = 0; i < seleccion.Count; i++)
            itemsEnVenta[i] = ScriptableObject.Instantiate(seleccion[i]);

        Debug.Log($"TiendaEspacial: rotación aplicada ({itemsEnVenta.Length} ítems).");
        RefrescarUI();
    }

    void TryAddRandomDeTipo(List<ItemEspacial> lista, TipoItem tipo)
    {
        var cand = new List<ItemEspacial>();
        foreach (var it in poolDeItems) if (it && it.tipo == tipo) cand.Add(it);
        if (cand.Count == 0) return;
        var pick = cand[Random.Range(0, cand.Count)];
        if (!lista.Contains(pick)) lista.Add(pick);
    }

    
    public void RefrescarUI()
    {
        if (slots == null || slots.Length == 0 || itemsEnVenta == null) return;

        
        for (int i = 0; i < slots.Length; i++)
        {
            //
            var it = (itemsEnVenta.Length > 0) ? itemsEnVenta[i % itemsEnVenta.Length] : null;

            if (it != null) slots[i].Setup(it, OnClickComprar);
            else slots[i].SetEmpty(); 
        }
    }

    void OnClickComprar(ItemEspacial item)
    {
        JugadorFinanzas.instancia.Comprar(item);
        UIManager.instancia.ActualizarCreditos(JugadorFinanzas.instancia.creditos);

        
        foreach (var s in slots) s.ActualizarInteractividad();

        
        NotificationManager.Instancia?.Notify($"Compraste {item.nombre} por ${item.costo}", NotificationType.Success);
    }

    
    System.Collections.IEnumerator InflacionRealtime()
    {
        var wait = new WaitForSecondsRealtime(tiempoInflacion);
        while (true) { yield return wait; AplicarInflacion(); }
    }

    void AplicarInflacion()
    {
        if (itemsEnVenta == null || itemsEnVenta.Length == 0) return;

        foreach (var it in itemsEnVenta)
        {
            if (!it) continue;
            it.costo += Mathf.RoundToInt(it.costo * (incrementoPorciento / 100f));
            it.valorVenta += Mathf.RoundToInt(it.valorVenta * (incrementoPorciento / 100f));
        }

        
        foreach (var s in slots) s.RefrescarCosto();

        Debug.Log($"TiendaEspacial: inflación +{incrementoPorciento}% aplicada a la rotación actual.");
    }
}
