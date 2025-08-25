using UnityEngine;
using System.Collections.Generic;

public class TiendaEspacial : MonoBehaviour
{
    public FPSController controladorJugador;

    [Header("Pool de Ítems disponibles")]
    [Tooltip("Arrastra aquí TODOS los ItemEspacial que quieres que la tienda pueda vender.\n" +
             "Alternativa: déjalo vacío y carga automáticamente desde Resources/Items con autoCargarResources.")]
    public ItemEspacial[] poolDeItems;

    [Tooltip("Si está activo y poolDeItems está vacío, cargará todos los items desde Resources/Items")]
    public bool autoCargarResources = true;

    [Header("Slots de venta por rotación")]
    [Range(1, 12)]
    public int cantidadSlots = 4;
    public Transform contenedorLista;
    public GameObject prefabFila;

    [Header("Sesgo por categoría (opcional)")]
    public bool forzarCategorias = true; // 1 Necesidad, 1 Lujo, 1 Prevención, 1 Inversión si hay

    [Header("Inflación")]
    public float incrementoPorciento = 5f;   // % por tick de inflación
    public float tiempoInflacion = 60f;      // cada cuánto aplicar inflación (segundos, tiempo normal)
    public bool inflacionEnTiempoReal = true; // si true, no se detiene con Time.timeScale = 0

    

    // Ítems de la rotación actual (¡siempre COPIAS!)
    public ItemEspacial[] itemsEnVenta;

    // ---- Ciclo ----
    void Start()
    {
        if ((poolDeItems == null || poolDeItems.Length == 0) && autoCargarResources)
        {
            poolDeItems = Resources.LoadAll<ItemEspacial>("Items"); // Assets/Resources/Items/*.asset
            Debug.Log($"TiendaEspacial: cargados {poolDeItems.Length} items desde Resources/Items");
        }

        // primera rotación al arrancar
        RotarItems();

        // inflación periódica
        if (inflacionEnTiempoReal)
            StartCoroutine(InflacionRealtime());
        else
            InvokeRepeating(nameof(AplicarInflacion), tiempoInflacion, tiempoInflacion);
    }

    // ---- Abrir / Cerrar ----
    public void AbrirTienda()
    {
        
        RotarItems();

        UIManager.instancia.MostrarTienda();
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (controladorJugador != null)
            controladorJugador.habilitarMovimiento = false;

        UIManager.instancia.ActualizarCreditos(JugadorFinanzas.instancia.creditos);
        UIManager.instancia.ActualizarPrecios(itemsEnVenta);
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

    // ---- Rotación ----
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
            // intenta cubrir categorías distintas si existen en el pool
            TryAddRandomDeTipo(seleccion, TipoItem.Necesidad);
            if (seleccion.Count < cantidadSlots) TryAddRandomDeTipo(seleccion, TipoItem.Lujo);
            if (seleccion.Count < cantidadSlots) TryAddRandomDeTipo(seleccion, TipoItem.Prevención);
            if (seleccion.Count < cantidadSlots) TryAddRandomDeTipo(seleccion, TipoItem.Inversión);
        }

        // completa random hasta llenar slots, evitando duplicados visibles
        var usados = new HashSet<ItemEspacial>(seleccion);
        int intentosSeguridad = 200;
        while (seleccion.Count < cantidadSlots && intentosSeguridad-- > 0)
        {
            var pick = poolDeItems[Random.Range(0, poolDeItems.Length)];
            if (!usados.Contains(pick))
            {
                seleccion.Add(pick);
                usados.Add(pick);
            }
        }

        // crea COPIAS para la venta (para no modificar assets originales con inflación)
        itemsEnVenta = new ItemEspacial[seleccion.Count];
        for (int i = 0; i < seleccion.Count; i++)
            itemsEnVenta[i] = ScriptableObject.Instantiate(seleccion[i]);

        UIManager.instancia.ActualizarPrecios(itemsEnVenta);
        Debug.Log("TiendaEspacial: rotación aplicada.");
    }

    void TryAddRandomDeTipo(List<ItemEspacial> lista, TipoItem tipo)
    {
        var candidatos = new List<ItemEspacial>();
        foreach (var it in poolDeItems)
            if (it != null && it.tipo == tipo)
                candidatos.Add(it);

        if (candidatos.Count > 0)
        {
            var pick = candidatos[Random.Range(0, candidatos.Count)];
            if (!lista.Contains(pick)) lista.Add(pick);
        }
    }

    // ---- Inflación ----

    // versión robusta que sigue corriendo aunque pauses con Time.timeScale = 0
    System.Collections.IEnumerator InflacionRealtime()
    {
        var wait = new WaitForSecondsRealtime(tiempoInflacion);
        while (true)
        {
            yield return wait;
            AplicarInflacion();
        }
    }

    private void AplicarInflacion()
    {
        if (itemsEnVenta == null) return;

        foreach (var item in itemsEnVenta)
        {
            if (item == null) continue;

            // Incrementa SOLO en la copia que se muestra ahora
            item.costo      += Mathf.RoundToInt(item.costo * (incrementoPorciento / 100f));
            item.valorVenta += Mathf.RoundToInt(item.valorVenta * (incrementoPorciento / 100f));
        }

        RefrescarUI();

        UIManager.instancia.ActualizarPrecios(itemsEnVenta);
        Debug.Log($"TiendaEspacial: inflación +{incrementoPorciento}% aplicada a la ROTACIÓN actual.");
    }


    public void RefrescarUI()
    {
        if (!contenedorLista || !prefabFila || itemsEnVenta == null) return;

        // limpia lista actual
        for (int i = contenedorLista.childCount - 1; i >= 0; i--)
            Destroy(contenedorLista.GetChild(i).gameObject);

        // crea filas nuevas
        foreach (var it in itemsEnVenta)
        {
            var go = Instantiate(prefabFila, contenedorLista);
            var row = go.GetComponent<ShopItem>();
            row.Setup(it, OnClickComprar);
        }
    }

    private void OnClickComprar(ItemEspacial item)
    {
        JugadorFinanzas.instancia.Comprar(item);
        // actualiza créditos y lista (por si cambió el poder de compra o inventario lleno)
        UIManager.instancia.ActualizarCreditos(JugadorFinanzas.instancia.creditos);
        RefrescarUI();
    }

}
