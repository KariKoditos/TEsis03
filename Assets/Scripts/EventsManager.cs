using UnityEngine;
using System.Collections.Generic;

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

    void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
    }

    void Start()
    {
        // iniciar ciclo de eventos
        Invoke(nameof(LanzarEventoAleatorio), Random.Range(intervaloMinSeg, intervaloMaxSeg));
    }

    void LanzarEventoAleatorio()
    {
        if (eventos.Count == 0) return;

        eventoActivo = eventos[Random.Range(0, eventos.Count)];
        Debug.Log($"[EVENTO] {eventoActivo.titulo} lanzado.");

        NotificationManager.Instancia?.Notify(
            $"Incidente: {eventoActivo.titulo}\n{eventoActivo.descripcion}",
            NotificationType.Warning
        );

        // volver a agendar otro evento
        Invoke(nameof(LanzarEventoAleatorio), Random.Range(intervaloMinSeg, intervaloMaxSeg));
    }

    // cuando el jugador usa un ítem, se pregunta si resuelve el evento
    public bool OnItemUsed(ItemEspacial item)
    {
        if (eventoActivo == null) return false;

        // caso 1: necesita ítem exacto
        if (eventoActivo.solucion == EventSolutionType.ItemByAsset &&
            eventoActivo.itemRequerido == item)
        {
            ResolverEvento();
            return eventoActivo.consumirItemAlUsar;
        }

        // caso 2: necesita tipo
        if (eventoActivo.solucion == EventSolutionType.ItemByTipo &&
            eventoActivo.tipoRequerido == item.tipo)
        {
            ResolverEvento();
            return eventoActivo.consumirItemAlUsar;
        }

        return false; // no resuelve nada
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
        if (eventoActivo == null) return false;

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

}
