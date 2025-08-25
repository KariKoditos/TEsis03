using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public enum NotificationType { Info, Success, Warning, Error }

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instancia;

    [Header("Toasts (esquina)")]
    public Transform contenedor;           // Panel con Vertical Layout Group
    public GameObject prefabToast;         // Prefab: CanvasGroup + Image fondo + (opcional) Image hijo "Icon" + TMP_Text
    [Range(1, 10)] public int maxVisibles = 5;
    public float duracionPorDefecto = 4f;
    [Tooltip("Si es true, los toasts expiran aunque el juego esté 'pausado' por UI.")]
    public bool usarTiempoNoEscalado = true;

    

    [Header("Icono de aviso (campanita/punto)")]
    public Image imagenAviso;  

    // --- Pool de instancias para evitar GC
    readonly Queue<GameObject> pool = new Queue<GameObject>();

    // --- Historial persistente para el panel
    [SerializeField] int maxHistorial = 200;
    readonly List<(string msg, NotificationType tipo, System.DateTime ts)> historial =
        new List<(string, NotificationType, System.DateTime)>(128);

    // Badge de "nuevas no leídas" (opcional)
    public bool tieneNuevasNoLeidas { get; private set; } = false;

    void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
    }

    public void Notify(string mensaje, NotificationType tipo = NotificationType.Info, float? duracion = null)
    {
        // 1) Guardar en historial (persiste hasta que el jugador borre)
        historial.Add((mensaje, tipo, System.DateTime.Now));
        if (historial.Count > maxHistorial) historial.RemoveAt(0);
        tieneNuevasNoLeidas = true;

        if (imagenAviso) imagenAviso.gameObject.SetActive(true);

        // 2) Mostrar toast visual (temporal)
        if (!contenedor || !prefabToast) { Debug.Log(mensaje); return; }

        // Limitar visibles
        while (contenedor.childCount >= maxVisibles)

        {
            var child = contenedor.GetChild(0).gameObject;
            // Encolar de forma segura en el pool
            child.SetActive(false);
            child.transform.SetParent(null);
            pool.Enqueue(child);
        }

        // Reusar del pool o instanciar
        GameObject go = pool.Count > 0 ? pool.Dequeue() : Instantiate(prefabToast);
        go.transform.SetParent(contenedor, false);
        go.SetActive(true);

        // Referencias del prefab
        var cg = go.GetComponent<CanvasGroup>(); if (!cg) cg = go.AddComponent<CanvasGroup>();
        var bg = go.GetComponent<Image>(); // fondo (opcional, para color por tipo)
        var icon = go.transform.Find("Icon") ? go.transform.Find("Icon").GetComponent<Image>() : null;
        var txt = go.GetComponentInChildren<TMP_Text>();

        if (txt) txt.text = mensaje;
       

        if (bg) // color según tipo (opcional)
        {
            switch (tipo)
            {
                case NotificationType.Success: bg.color = new Color(0.15f, 0.6f, 0.25f, 0.9f); break;
                case NotificationType.Warning: bg.color = new Color(0.8f, 0.55f, 0.1f, 0.9f); break;
                case NotificationType.Error: bg.color = new Color(0.7f, 0.15f, 0.15f, 0.9f); break;
                default: bg.color = new Color(0.15f, 0.25f, 0.55f, 0.9f); break;
            }
        }

        float vida = duracion ?? duracionPorDefecto;
        StartCoroutine(RutinaToast(go, cg, vida));
    }

    

    IEnumerator RutinaToast(GameObject go, CanvasGroup cg, float vida)
    {
        // Fade in
        cg.alpha = 0f;
        float t = 0f;
        while (t < 0.18f)
        {
            t += (usarTiempoNoEscalado ? Time.unscaledDeltaTime : Time.deltaTime);
            cg.alpha = Mathf.SmoothStep(0f, 1f, t / 0.18f);
            yield return null;
        }
        cg.alpha = 1f;

        // Visible
        t = 0f;
        while (t < vida)
        {
            t += (usarTiempoNoEscalado ? Time.unscaledDeltaTime : Time.deltaTime);
            yield return null;
        }

        // Fade out
        t = 0f;
        while (t < 0.2f)
        {
            t += (usarTiempoNoEscalado ? Time.unscaledDeltaTime : Time.deltaTime);
            cg.alpha = Mathf.SmoothStep(1f, 0f, t / 0.2f);
            yield return null;
        }

        // Devolver al pool
        go.SetActive(false);
        go.transform.SetParent(null);

        var rt = go.GetComponent<RectTransform>();
        if (rt)
        {
            rt.localScale = Vector3.one;
            rt.sizeDelta = Vector2.zero;
        }

        pool.Enqueue(go);



    }

    // === API para el panel de historial ===
    public List<(string msg, NotificationType tipo, System.DateTime ts)> GetHistorialSnapshot()
        => new List<(string, NotificationType, System.DateTime)>(historial); //Preguntar para que es =>

    public void ClearHistorial()
    {
        historial.Clear();
    }

    // Llama esto cuando el jugador abre el panel (para apagar el “badge”)
    public void MarcarLeidas()
    {
        tieneNuevasNoLeidas = false;
        if (imagenAviso) imagenAviso.gameObject.SetActive(false);
    }
}
