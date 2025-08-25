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
    public int da�oSalud = 5;
    public int da�oEnergia = 10;

    [Header("Sugerencias / �tems prevenci�n")]
    [Tooltip("Nombres clave que usar�s como 'reparaci�n' t�pica. No tienen que coincidir exacto con los assets.")]
    public string[] etiquetasPrevencion = { "ToolBox", "Seguro", "Repuesto", "Kit de reparaci�n" };

    [Header("Sesgo a tienda (opcional)")]
    public TiendaEspacial tienda;         // (si la asignas) le pedimos prevenir en la pr�xima rotaci�n
    public bool forzarPrevencionEnSiguienteRotacion = true;

    // Incidente en curso (uno a la vez, simple)
    private PendingIncident incidenteActivo = null;

    // Plantillas de incidente
    private readonly string[] _plantillas = new string[]
    {
        "Vibraci�n an�mala en el motor secundario",
        "Panel solar con rendimiento irregular",
        "Filtro de aire saturado",
        "Fuga menor en sello de compuerta",
        "Descalibraci�n del giroscopio",
        "Sobrecalentamiento en microreactor auxiliar",
        "Firmware del m�dulo de navegaci�n desactualizado"
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
                : "herramienta de prevenci�n";

            int multa = Random.Range(multaMin, multaMax);

            incidenteActivo = new PendingIncident
            {
                titulo = titulo,
                sugerencia = sugerido,
                multa = multa,
                deadlineRealtime = Time.realtimeSinceStartup + tiempoParaResolverSeg,
                resuelto = false
            };

            // Notificaci�n
            NotificationManager.Instancia?.Notify(
                $" Incidente: {titulo}\n" +
                $"Sugerido: {sugerido}. Tienes {Mathf.RoundToInt(tiempoParaResolverSeg)}s para atenderlo.\n" +
                $"Multa si se ignora: {multa} cr�ditos, -{da�oSalud} Salud, -{da�oEnergia} Energ�a.",
                NotificationType.Warning
            );

            // Sesgo: pedir a la tienda un �tem de prevenci�n la pr�xima vez
            if (forzarPrevencionEnSiguienteRotacion && tienda != null)
                tienda.forzarCategorias = true; // ya favorece Prevenci�n en tu l�gica

            // Esperar resoluci�n o deadline
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
        // 1) Multa de cr�ditos
        bool cobrado = false;
        if (JugadorFinanzas.instancia != null && inc.multa > 0)
        {
            cobrado = JugadorFinanzas.instancia.TryGastarCreditos(inc.multa);
        }

        // 2) Da�o a necesidades si no alcanz� (o igual quieres aplicarlo siempre)
        if (NeedsSystem.Instancia != null)
        {
            if (!cobrado) // si no pudo pagar, castigo m�s duro (opcional)
            {
                NeedsSystem.Instancia.Modificar(NeedType.Salud, -da�oSalud * 2);
                NeedsSystem.Instancia.Modificar(NeedType.Energia, -da�oEnergia * 2);
            }
            else
            {
                NeedsSystem.Instancia.Modificar(NeedType.Salud, -da�oSalud);
                NeedsSystem.Instancia.Modificar(NeedType.Energia, -da�oEnergia);
            }
        }

        NotificationManager.Instancia?.Notify(
            $"No se atendi� el incidente a tiempo. Se aplic� multa y bajaron barras.",
            NotificationType.Error
        );
    }

    // ===== API p�blica para resolver con un �tem de Prevenci�n =====

    public bool ResolverConItem(string nombreItem)
    {
        if (incidenteActivo == null || incidenteActivo.resuelto) return false;

        // Simple: marcar resuelto por cualquier �tem de Prevenci�n
        incidenteActivo.resuelto = true;

        NotificationManager.Instancia?.Notify(
            $" Incidente resuelto con {nombreItem}. �Buen trabajo!",
            NotificationType.Success
        );
        incidenteActivo = null;
        return true;
    }

    // Resolver pagando manualmente (si quisieras un bot�n o acci�n directa)
    public bool ResolverPagando(int costo)
    {
        if (incidenteActivo == null || incidenteActivo.resuelto) return false;

        if (JugadorFinanzas.instancia != null && JugadorFinanzas.instancia.TryGastarCreditos(costo))
        {
            incidenteActivo.resuelto = true;
            NotificationManager.Instancia?.Notify(
                $" Pagaste {costo} cr�ditos y se resolvi� el incidente.",
                NotificationType.Info
            );
            incidenteActivo = null;
            return true;
        }
        else
        {
            NotificationManager.Instancia?.Notify(
                $"No tienes cr�ditos suficientes para pagar la reparaci�n ({costo}).",
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
