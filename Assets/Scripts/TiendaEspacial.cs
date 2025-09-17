using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TiendaEspacial : MonoBehaviour
{
    public FPSController controladorJugador;

    [Header("Pool de Ítems disponibles")]
    public ItemEspacial[] poolDeItems;                 // si está vacío y autoCargarResources = true, se cargan desde Resources/Items
    public bool autoCargarResources = true;

    [Header("Slots de venta por rotación (opcional)")]
    [Range(1, 12)] public int cantidadSlots = 4;
    public bool forzarCategorias = true;
    public ShopItem[] slots;                           // si no lo usas, puedes dejarlo vacío

    [Header("Inflación (por tiempo) para la rotación clásica")]
    public float incrementoPorciento = 5f;
    public float tiempoInflacion = 60f;
    public bool inflacionEnTiempoReal = true;

    [Header("Rotación automática (clásica)")]
    public float tiempoRotacion = 50f;
    public bool rotacionEnTiempoReal = true;

    [Header("UI Scroll (Tienda Completa)")]
    [Tooltip("Asigna el Content del ScrollView (no el Viewport).")]
    public Transform scrollContent;                  
    [Tooltip("Prefab de la tarjeta (debe tener componente ShopItem).")]
    public GameObject prefabShopItem;

    [Header("Economía dinámica (por compras)")]
    [Tooltip("Cada compra del mismo ítem añade este costo extra permanente.")]
    public int inflacionPorCompra = 5;                
    public int inflacionTopeExtra = 200;             

    
    private readonly Dictionary<ItemEspacial, int> comprasPorItem = new Dictionary<ItemEspacial, int>();

    private readonly Dictionary<string, ItemEspacial> originalPorNombre = new Dictionary<string, ItemEspacial>();

    // Rotación clásica (copias)
    public ItemEspacial[] itemsEnVenta;

    void Start()
    {
        if ((poolDeItems == null || poolDeItems.Length == 0) && autoCargarResources)
        {
            poolDeItems = Resources.LoadAll<ItemEspacial>("Items");
            Debug.Log($"TiendaEspacial: cargados {poolDeItems.Length} ítems desde Resources/Items");
        }

       
        originalPorNombre.Clear();
        foreach (var it in poolDeItems)
            if (it && !originalPorNombre.ContainsKey(it.nombre))
                originalPorNombre.Add(it.nombre, it);

        

        // Inflación temporal 
        if (inflacionEnTiempoReal) StartCoroutine(InflacionRealtime());
        else InvokeRepeating(nameof(AplicarInflacion), tiempoInflacion, tiempoInflacion);
    }

    
    public void AbrirTienda()
    {
        if (scrollContent && prefabShopItem)
            PoblarTiendaCompleta();

        UIManager.instancia.MostrarTienda();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (controladorJugador) controladorJugador.habilitarMovimiento = false;

        UIManager.instancia.ActualizarCreditos(JugadorFinanzas.instancia.creditos);

      
        if (slots != null && slots.Length > 0)
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
        if (poolDeItems == null || poolDeItems.Length == 0) return;

        var seleccion = new List<ItemEspacial>(cantidadSlots);

        if (forzarCategorias)
        {
            TryAddRandomDeTipo(seleccion, TipoItem.Necesidad);
            if (seleccion.Count < cantidadSlots) TryAddRandomDeTipo(seleccion, TipoItem.Lujo);
            if (seleccion.Count < cantidadSlots) TryAddRandomDeTipo(seleccion, TipoItem.Prevención);
            if (seleccion.Count < cantidadSlots) TryAddRandomDeTipo(seleccion, TipoItem.Inversión);
        }

        var usados = new HashSet<ItemEspacial>(seleccion);
        int guard = 200;
        while (seleccion.Count < cantidadSlots && guard-- > 0)
        {
            var pick = poolDeItems[UnityEngine.Random.Range(0, poolDeItems.Length)];
            if (!usados.Contains(pick)) { seleccion.Add(pick); usados.Add(pick); }
        }

        itemsEnVenta = new ItemEspacial[seleccion.Count];
        for (int i = 0; i < seleccion.Count; i++)
            itemsEnVenta[i] = ScriptableObject.Instantiate(seleccion[i]);

        RefrescarUI();
    }

    void TryAddRandomDeTipo(List<ItemEspacial> lista, TipoItem tipo)
    {
        var cand = new List<ItemEspacial>();
        foreach (var it in poolDeItems) if (it && it.tipo == tipo) cand.Add(it);
        if (cand.Count == 0) return;
        var pick = cand[UnityEngine.Random.Range(0, cand.Count)];
        if (!lista.Contains(pick)) lista.Add(pick);
    }

    public void RefrescarUI()
    {
        if (slots == null || slots.Length == 0 || itemsEnVenta == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            var s = slots[i];
            if (s == null) continue;  

            var it = (itemsEnVenta.Length > 0) ? itemsEnVenta[i % itemsEnVenta.Length] : null;
            if (it != null) s.Setup(it, OnClickComprarSlot);
            else s.SetEmpty();
        }
    }

    void OnClickComprarSlot(ItemEspacial item)
    {
        
        if (JugadorFinanzas.instancia.Comprar(item, item.costo))
        {
            foreach (var s in slots) s.ActualizarInteractividad();
            //NotificationManager.Instancia?.Notify($"Compraste {item.nombre} por ${item.costo}", NotificationType.Success);
        }
    }

    IEnumerator InflacionRealtime()
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

 
    public void PoblarTiendaCompleta()
    {
        if (!scrollContent || !prefabShopItem)
        {
            Debug.LogWarning("[Tienda] Falta asignar scrollContent o prefabShopItem.");
            return;
        }

        
        for (int i = scrollContent.childCount - 1; i >= 0; i--)
            Destroy(scrollContent.GetChild(i).gameObject);

        
        foreach (var original in poolDeItems)
        {
            if (!original) continue;

            
            int precio = GetPrecioConInflacion(original);

            
            var copia = ScriptableObject.Instantiate(original);
            copia.costo = precio;

            var go = Instantiate(prefabShopItem, scrollContent);
            var card = go.GetComponent<ShopItem>();
            card.Setup(copia, OnComprarClickScroll); 
        }
    }

    
    void OnComprarClickScroll(ItemEspacial itemCopiaMostrada)
    {
        if (itemCopiaMostrada == null) return;

        // Recuperar el original por nombre para aplicar inflación real
        ItemEspacial original = itemCopiaMostrada;
        if (originalPorNombre.TryGetValue(itemCopiaMostrada.nombre, out var o))
            original = o;

        int precio = GetPrecioConInflacion(original);

        if (JugadorFinanzas.instancia.Comprar(itemCopiaMostrada, precio))
        {
            RegistrarCompra(original);   // registra compras para subir precio la próxima vez
            PoblarTiendaCompleta();      // refresca las cards con el nuevo precio
            //NotificationManager.Instancia?.Notify($"Compraste {itemCopiaMostrada.nombre} por {precio} créditos.", NotificationType.Success, 3.5f);
        }
        else
        {
            NotificationManager.Instancia?.Notify($"No tienes créditos suficientes para {itemCopiaMostrada.nombre}.", NotificationType.Warning, 4f);
        }
    }

    void RegistrarCompra(ItemEspacial original)
    {
        if (!comprasPorItem.ContainsKey(original)) comprasPorItem[original] = 0;
        comprasPorItem[original]++; // cada compra del mismo ítem incrementa el precio futuro
    }

    // costo base + (nCompras * inflacionPorCompra) con tope máximo
    int GetPrecioConInflacion(ItemEspacial original)
    {
        int n = comprasPorItem.TryGetValue(original, out var c) ? c : 0;
        int extra = Mathf.Clamp(n * inflacionPorCompra, 0, inflacionTopeExtra);
        return original.costo + extra;
    }
}
