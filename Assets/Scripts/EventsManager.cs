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

    void Awake() //singleton
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

    void LanzarEventoAleatorio() //gestionar el incidente
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

    
    public bool OnItemUsed(ItemEspacial item) //Ya con efecto resuelve el evento 
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

    void ResolverEvento() //Marca el incidente como resuelto
    {
        Debug.Log($"Evento {eventoActivo.titulo} resuelto.");
        NotificationManager.Instancia?.Notify(
            $"Incidente resuelto: {eventoActivo.titulo}", NotificationType.Success
        );
        eventoActivo = null;
    }

    public bool PuedeUsarseParaEvento(ItemEspacial item) //Verificación solo lectura
    {
        if (eventoActivo == null || item == null) return false; //checa si el item funciona para el evento

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


    public static void EmitIncidentSpawned(string requiredTag) //Incidente con item que tiene tag
    {
        if (OnIncidentSpawnedRequiredTag != null)
        {
            OnIncidentSpawnedRequiredTag.Invoke(requiredTag);
        }
    }

}
