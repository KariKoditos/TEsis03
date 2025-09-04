using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShipIncidentManager : MonoBehaviour
{
    public static ShipIncidentManager Instancia;

    [Header("Frecuencia de incidentes (tiempo real)")]
    public float intervaloMinSeg = 45f;
    public float intervaloMaxSeg = 90f;

    [Header("Ventana para resolver")]
    public float tiempoParaResolverSeg = 60f;

    [Header("Consecuencias si se ignora")]
    public int multaMin = 50;
    public int multaMax = 200;
    public int dañoSalud = 5;
    public int dañoEnergia = 10;

    [Header("Sugerencias / Ítems prevención")]
    [Tooltip("Nombres clave/etiquetas que usará el incidente como 'requiredTag' (ej: Extintor, Seguro...).")]
    public string[] etiquetasPrevencion = { "Extintor", "Seguro", "Repuesto", "Kit de reparación" };

    [Header("Sesgo a tienda (opcional)")]
    public TiendaEspacial tienda;
    public bool forzarPrevencionEnSiguienteRotacion = true;

    // Incidente en curso (uno a la vez)
    private PendingIncident incidenteActivo = null;

    // Plantillas de incidente
    private readonly string[] _plantillas = new string[]
    {
        "Vibración anómala en el motor secundario",
        "Panel solar con rendimiento irregular",
        "Filtro de aire saturado",
        "Fuga menor en sello de compuerta",
        "Descalibración del giroscopio",
        "Sobrecalentamiento en microreactor auxiliar",
        "Firmware del módulo de navegación desactualizado"
    };

    void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);
    }

    void OnEnable()
    {
        // Suscribir resolver a través del bus de eventos
        EventsManager.OnTryResolveIncidentWithItem += ResolverConItem;
        StartCoroutine(LoopIncidentes());
    }

    void OnDisable()
    {
        EventsManager.OnTryResolveIncidentWithItem -= ResolverConItem;
        StopAllCoroutines();
    }

    IEnumerator LoopIncidentes()
    {
        // Espera inicial aleatoria
        yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(5f, 12f));

        while (true)
        {
            // Espera aleatoria entre incidentes (tiempo real)
            float espera = UnityEngine.Random.Range(intervaloMinSeg, intervaloMaxSeg);
            yield return new WaitForSecondsRealtime(espera);

            if (incidenteActivo != null) continue;

            // Crea un incidente
            string titulo = _plantillas[UnityEngine.Random.Range(0, _plantillas.Length)];
            string required = (etiquetasPrevencion != null && etiquetasPrevencion.Length > 0)
                ? etiquetasPrevencion[UnityEngine.Random.Range(0, etiquetasPrevencion.Length)]
                : "Extintor"; // fallback sensato

            int multa = UnityEngine.Random.Range(multaMin, multaMax);

            incidenteActivo = new PendingIncident
            {
                titulo = titulo,
                requiredTag = required, // <- clave
                multa = multa,
                deadlineRealtime = Time.realtimeSinceStartup + tiempoParaResolverSeg,
                resuelto = false
            };

            // Notificación
            NotificationManager.Instancia?.Notify(
                $" Incidente: {titulo}\n" +
                $"Requiere: {required}. Tienes {Mathf.RoundToInt(tiempoParaResolverSeg)}s para atenderlo.\n" +
                $"Multa si se ignora: {multa} créditos, -{dañoSalud} Salud, -{dañoEnergia} Energía.",
                NotificationType.Warning
            );

            // Anuncia por el bus (por si UI quiere mostrar pista)
            EventsManager.EmitIncidentSpawned(required);

            // Sesgo tienda
            if (forzarPrevencionEnSiguienteRotacion && tienda != null)
                tienda.forzarCategorias = true;

            // Esperar resolución o deadline
            yield return StartCoroutine(EsperarResolucionODefault());
        }
    }

    IEnumerator EsperarResolucionODefault()
    {
        while (incidenteActivo != null && !incidenteActivo.resuelto)
        {
            if (Time.realtimeSinceStartup >= incidenteActivo.deadlineRealtime)
            {
                AplicarConsecuencias(incidenteActivo);
                incidenteActivo = null;
                yield break;
            }
            yield return null;
        }
    }

    void AplicarConsecuencias(PendingIncident inc)
    {
        // 1) Multa
        bool cobrado = false;
        if (JugadorFinanzas.instancia != null && inc.multa > 0)
            cobrado = JugadorFinanzas.instancia.TryGastarCreditos(inc.multa);

        // 2) Daño a necesidades
        if (NeedsSystem.Instancia != null)
        {
            if (!cobrado)
            {
                NeedsSystem.Instancia.Modificar(NeedType.Salud, -dañoSalud * 2);
                NeedsSystem.Instancia.Modificar(NeedType.Energia, -dañoEnergia * 2);
            }
            else
            {
                NeedsSystem.Instancia.Modificar(NeedType.Salud, -dañoSalud);
                NeedsSystem.Instancia.Modificar(NeedType.Energia, -dañoEnergia);
            }
        }

        NotificationManager.Instancia?.Notify(
            $"No se atendió el incidente a tiempo. Se aplicó multa y bajaron barras.",
            NotificationType.Error
        );
    }

    // ===== Resolver vía bus (handler) =====
    private bool ResolverConItem(ItemEspacial item)
    {
        if (incidenteActivo == null || incidenteActivo.resuelto) return false;
        if (item == null || item.tipo != TipoItem.Prevención) return false;

        if (!string.IsNullOrEmpty(incidenteActivo.requiredTag) &&
            item.tagPrevencion != incidenteActivo.requiredTag)
        {
            NotificationManager.Instancia?.Notify(
                $"Necesitas {incidenteActivo.requiredTag} para este incidente.",
                NotificationType.Warning, 4f
            );
            return false;
        }

        incidenteActivo.resuelto = true;
        NotificationManager.Instancia?.Notify(
            $"Incidente resuelto con {item.nombre}.",
            NotificationType.Success, 4f
        );
        incidenteActivo = null;
        return true;
    }

    // Estructura interna
    private class PendingIncident
    {
        public string titulo;
        public string requiredTag;     // <--- añadido
        public int multa;
        public float deadlineRealtime;
        public bool resuelto;
    }
}
