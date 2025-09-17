using UnityEngine;
using System.Collections.Generic;
using System;

public class EventsManager : MonoBehaviour
{
    public static EventsManager Instancia;

    [Header("Config de Eventos Aleatorios")]
    [Tooltip("Lista de eventos posibles que puede lanzar la nave.")]
    public List<RandomEventDef> eventos = new List<RandomEventDef>();

    [Tooltip("Tiempo mínimo (segundos) entre eventos aleatorios")]
    public float intervaloMinSeg = 60f;

    [Tooltip("Tiempo máximo (segundos) entre eventos aleatorios")]
    public float intervaloMaxSeg = 120f;

    private RandomEventDef eventoActivo;

    // Bus / Contratos
    public static event Action<string> OnIncidentSpawnedRequiredTag;
    public static event Func<ItemEspacial, bool> OnTryResolveIncidentWithItem;

    void Awake() //Singleton instancia global: EventsManager UI, finanzas, inventario usan el gestor de eventos sin referencias manuales
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
    }

    void Start()
    {
        // Iniciar ciclo de eventos aleatorios (usa Random de Unity)
        Invoke(nameof(LanzarEventoAleatorio),
               UnityEngine.Random.Range(intervaloMinSeg, intervaloMaxSeg));
    }

    void LanzarEventoAleatorio()
    {
        if (eventos.Count == 0) return;

        eventoActivo = eventos[UnityEngine.Random.Range(0, eventos.Count)];
        Debug.Log($"[EVENTO] {eventoActivo.titulo} lanzado.");

        NotificationManager.Instancia?.Notify(
            $"Incidente: {eventoActivo.titulo}\n{eventoActivo.descripcion}",
            NotificationType.Warning
        );

        // Volver a agendar otro evento
        Invoke(nameof(LanzarEventoAleatorio),
               UnityEngine.Random.Range(intervaloMinSeg, intervaloMaxSeg));
    }

    // checa si el item q uso si arregla el evento
    public bool OnItemUsed(ItemEspacial item)
    {
        if (eventoActivo == null || item == null) return false;

        // Caso 1: requiere asset exacto
        if (eventoActivo.solucion == EventSolutionType.ItemByAsset &&
            eventoActivo.itemRequerido == item)
        {
            ResolverEvento();
            return eventoActivo.consumirItemAlUsar;
        }

        // Caso 2: requiere tipo
        if (eventoActivo.solucion == EventSolutionType.ItemByTipo &&
            eventoActivo.tipoRequerido == item.tipo)
        {
            ResolverEvento();
            return eventoActivo.consumirItemAlUsar;
        }

        return false; 
    }

    void ResolverEvento()
    {
        Debug.Log($"Evento {eventoActivo.titulo} resuelto.");
        NotificationManager.Instancia?.Notify(
            $"Incidente resuelto: {eventoActivo.titulo}", NotificationType.Success
        );
        eventoActivo = null;
    }

    public bool PuedeUsarseParaEvento(ItemEspacial item)
    {
        if (eventoActivo == null || item == null) return false;

        switch (eventoActivo.solucion)
        {
            case EventSolutionType.ItemByAsset:
                return eventoActivo.itemRequerido == item;
            case EventSolutionType.ItemByTipo:
                return eventoActivo.tipoRequerido == item.tipo;
            default:
                return false;
        }
    }


    public static void EmitIncidentSpawned(string requiredTag)
    {
        if (OnIncidentSpawnedRequiredTag != null)
        {
            OnIncidentSpawnedRequiredTag.Invoke(requiredTag);
        }
    }

    public static bool TryResolveIncidentWithItem(ItemEspacial item)
    {
        if (OnTryResolveIncidentWithItem != null)
        {
            return OnTryResolveIncidentWithItem.Invoke(item);
        }
        else
        {
            return false;
        }
    }

}
