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
    [Tooltip("Nombres clave que usarás como 'reparación' típica. No tienen que coincidir exacto con los assets.")]
    public string[] etiquetasPrevencion = { "ToolBox", "Seguro", "Repuesto", "Kit de reparación" };

    [Header("Sesgo a tienda (opcional)")]
    public TiendaEspacial tienda;         // (si la asignas) le pedimos prevenir en la próxima rotación
    public bool forzarPrevencionEnSiguienteRotacion = true;

    // Incidente en curso (uno a la vez, simple)
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
        StartCoroutine(LoopIncidentes());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator LoopIncidentes()
    {
        // Espera inicial aleatoria para que no caiga siempre al mismo tiempo
        yield return new WaitForSecondsRealtime(Random.Range(5f, 12f));

        while (true)
        {
            // Espera aleatoria entre incidentes
            float espera = Random.Range(intervaloMinSeg, intervaloMaxSeg);
            yield return new WaitForSecondsRealtime(espera);

            // Si ya hay uno activo, salta este ciclo
            if (incidenteActivo != null) continue;

            // Crea un incidente
            string titulo = _plantillas[Random.Range(0, _plantillas.Length)];
            string sugerido = etiquetasPrevencion.Length > 0
                ? etiquetasPrevencion[Random.Range(0, etiquetasPrevencion.Length)]
                : "herramienta de prevención";

            int multa = Random.Range(multaMin, multaMax);

            incidenteActivo = new PendingIncident
            {
                titulo = titulo,
                sugerencia = sugerido,
                multa = multa,
                deadlineRealtime = Time.realtimeSinceStartup + tiempoParaResolverSeg,
                resuelto = false
            };

            // Notificación
            NotificationManager.Instancia?.Notify(
                $" Incidente: {titulo}\n" +
                $"Sugerido: {sugerido}. Tienes {Mathf.RoundToInt(tiempoParaResolverSeg)}s para atenderlo.\n" +
                $"Multa si se ignora: {multa} créditos, -{dañoSalud} Salud, -{dañoEnergia} Energía.",
                NotificationType.Warning
            );

            // Sesgo: pedir a la tienda un ítem de prevención la próxima vez
            if (forzarPrevencionEnSiguienteRotacion && tienda != null)
                tienda.forzarCategorias = true; // ya favorece Prevención en tu lógica

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
                // Aplicar consecuencias
                AplicarConsecuencias(incidenteActivo);
                incidenteActivo = null;
                yield break;
            }
            yield return null;
        }
    }

    void AplicarConsecuencias(PendingIncident inc)
    {
        // 1) Multa de créditos
        bool cobrado = false;
        if (JugadorFinanzas.instancia != null && inc.multa > 0)
        {
            cobrado = JugadorFinanzas.instancia.TryGastarCreditos(inc.multa);
        }

        // 2) Daño a necesidades si no alcanzó (o igual quieres aplicarlo siempre)
        if (NeedsSystem.Instancia != null)
        {
            if (!cobrado) // si no pudo pagar, castigo más duro (opcional)
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

    // ===== API pública para resolver con un ítem de Prevención =====

    public bool ResolverConItem(string nombreItem)
    {
        if (incidenteActivo == null || incidenteActivo.resuelto) return false;

        // Simple: marcar resuelto por cualquier ítem de Prevención
        incidenteActivo.resuelto = true;

        NotificationManager.Instancia?.Notify(
            $" Incidente resuelto con {nombreItem}. ¡Buen trabajo!",
            NotificationType.Success
        );
        incidenteActivo = null;
        return true;
    }

    // Resolver pagando manualmente (si quisieras un botón o acción directa)
    public bool ResolverPagando(int costo)
    {
        if (incidenteActivo == null || incidenteActivo.resuelto) return false;

        if (JugadorFinanzas.instancia != null && JugadorFinanzas.instancia.TryGastarCreditos(costo))
        {
            incidenteActivo.resuelto = true;
            NotificationManager.Instancia?.Notify(
                $" Pagaste {costo} créditos y se resolvió el incidente.",
                NotificationType.Info
            );
            incidenteActivo = null;
            return true;
        }
        else
        {
            NotificationManager.Instancia?.Notify(
                $"No tienes créditos suficientes para pagar la reparación ({costo}).",
                NotificationType.Warning
            );
            return false;
        }
    }

    // Estructura interna
    private class PendingIncident
    {
        public string titulo;
        public string sugerencia;
        public int multa;
        public float deadlineRealtime;
        public bool resuelto;
    }
}
