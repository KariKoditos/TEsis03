using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public enum NotificationType { Info, Success, Warning, Error }

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instancia;

    [Header("Contenedores")]
    [Tooltip("Content del overlay (fuera del panel). Debe tener VerticalLayoutGroup + ContentSizeFitter.")]
    public Transform contenedorToasts;           // SIEMPRE visible (overlay)
    [Tooltip("Opcional: Content del Scroll del panel/inbox (si luego quieres poblarlo).")]
    public Transform contenedorInbox;            // DENTRO del panel (opcional)

    [Header("Prefab")]
    [Tooltip("Prefab raíz del toast (CanvasGroup requerido, TMP_Text opcional en hijos).")]
    public GameObject prefabToast;

    [Header("Límites / Tiempos")]
    [Range(1, 10)] public int maxVisibles = 10;
    public bool usarTiempoNoEscalado = true;
    [Min(0.1f)] public float vidaSegundosDefault = 3.5f;
    [Min(0.01f)] public float fadeIn = 0.18f;
    [Min(0.01f)] public float fadeOut = 0.20f;

    [Header("Icono de aviso (campanita/punto)")]
    public Image imagenAviso;

    [Header("Pool (opcional)")]
    public Transform poolRoot;                   // vacío desactivado; si es null, se crea

    // Pool + estado
    readonly Queue<GameObject> pool = new Queue<GameObject>();
    readonly Dictionary<GameObject, Coroutine> corutinas = new Dictionary<GameObject, Coroutine>();

    // Historial
    [SerializeField] int maxHistorial = 200;
    readonly List<(string msg, NotificationType tipo, System.DateTime ts)> historial =
        new List<(string, NotificationType, System.DateTime)>(128);

    void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;

        if (!poolRoot)
        {
            var go = new GameObject("ToastPoolRoot");
            go.transform.SetParent(transform, false);
            go.SetActive(false);
            poolRoot = go.transform;
        }
    }

    void OnDisable()
    {
        // Si desactivas la UI/escena, corta corutinas para evitar MissingReference
        foreach (var kv in corutinas)
            if (kv.Value != null) StopCoroutine(kv.Value);
        corutinas.Clear();
    }

    // ===== API principal =====
    public void Notify(string mensaje, NotificationType tipo = NotificationType.Info, float? duracion = null)
    {
        // 1) Historial
        historial.Add((mensaje, tipo, System.DateTime.Now));
        if (historial.Count > maxHistorial) historial.RemoveAt(0);

        // 2) Badge de aviso
        if (imagenAviso) imagenAviso.gameObject.SetActive(true);

        // 3) Toast visual en overlay
        if (!contenedorToasts || !prefabToast)
        {
            Debug.LogWarning("[NotificationManager] Falta contenedorToasts o prefabToast.");
            return;
        }

        // Si está lleno, recicla el más antiguo
        while (contenedorToasts.childCount >= maxVisibles)
        {
            var viejo = contenedorToasts.GetChild(0).gameObject;
            ForzarDevolverAlPool(viejo);
        }

        // Tomar del pool o instanciar
        GameObject go = pool.Count > 0 ? pool.Dequeue() : Instantiate(prefabToast);
        go.transform.SetParent(contenedorToasts, false);
        go.SetActive(true);

        // CanvasGroup para el fade
        var cg = go.GetComponent<CanvasGroup>();
        if (!cg) cg = go.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        // Texto (si existe)
        var txt = go.GetComponentInChildren<TMP_Text>(true);
        if (txt) txt.text = mensaje;

        // Cancelar corutina previa si estaba corriendo
        if (corutinas.TryGetValue(go, out var co) && co != null)
            StopCoroutine(co);

        // Duración final (mínimo + por tipo + por longitud)
        float vida = CalcularDuracionFinal(mensaje, tipo, duracion);
        corutinas[go] = StartCoroutine(RutinaToast(go, cg, vida));
    }

    float CalcularDuracionFinal(string mensaje, NotificationType tipo, float? overrideDur)
    {
        if (overrideDur.HasValue)
            return Mathf.Max(2.0f, overrideDur.Value); // mínimo 2s si viene override

        float vidaBase = vidaSegundosDefault;

        // Asegurar mínimos por tipo (ajusta a gusto)
        switch (tipo)
        {
            case NotificationType.Success: vidaBase = Mathf.Max(vidaBase, 3.0f); break;
            case NotificationType.Warning: vidaBase = Mathf.Max(vidaBase, 4.0f); break;
            case NotificationType.Error: vidaBase = Mathf.Max(vidaBase, 5.0f); break;
                // Info: usa vidaSegundosDefault
        }

        // Sumar un poco por longitud (máx +3s)
        int len = string.IsNullOrEmpty(mensaje) ? 0 : mensaje.Length;
        vidaBase += Mathf.Min(3.0f, len * 0.03f);

        return vidaBase;
    }

    IEnumerator RutinaToast(GameObject go, CanvasGroup cg, float vida)
    {
        // Fade in
        float t = 0f;
        while (t < fadeIn)
        {
            if (!go || !cg) yield break;
            t += usarTiempoNoEscalado ? Time.unscaledDeltaTime : Time.deltaTime;
            cg.alpha = Mathf.SmoothStep(0f, 1f, t / fadeIn);
            yield return null;
        }
        if (!go || !cg) yield break;
        cg.alpha = 1f;

        // Vida
        t = 0f;
        while (t < vida)
        {
            if (!go || !cg) yield break;
            t += usarTiempoNoEscalado ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }

        // Fade out
        t = 0f;
        while (t < fadeOut)
        {
            if (!go || !cg) yield break;
            t += usarTiempoNoEscalado ? Time.unscaledDeltaTime : Time.deltaTime;
            cg.alpha = Mathf.SmoothStep(1f, 0f, t / fadeOut);
            yield return null;
        }
        if (cg) cg.alpha = 0f;

        DevolverAlPool(go);
    }

    void ForzarDevolverAlPool(GameObject go)
    {
        if (!go) return;
        if (corutinas.TryGetValue(go, out var co) && co != null)
        {
            StopCoroutine(co);
            corutinas[go] = null;
        }
        var cg = go.GetComponent<CanvasGroup>();
        if (cg) cg.alpha = 0f;
        DevolverAlPool(go);
    }

    void DevolverAlPool(GameObject go)
    {
        if (!go) return;
        go.SetActive(false);
        go.transform.SetParent(poolRoot ? poolRoot : transform, false);
        pool.Enqueue(go);
    }

    // ===== Historial / Badge =====
    public List<(string, NotificationType, System.DateTime)> GetHistorial()
        => new List<(string, NotificationType, System.DateTime)>(historial);

    // Compatibilidad con código anterior
    public List<(string, NotificationType, System.DateTime)> GetHistorialSnapshot() => GetHistorial();

    public void ClearHistorial() => historial.Clear();

    /// <summary> Llama esto al abrir el panel de notificaciones para apagar la campanita. </summary>
    public void MarcarLeidas()
    {
        if (imagenAviso) imagenAviso.gameObject.SetActive(false);
    }
}
