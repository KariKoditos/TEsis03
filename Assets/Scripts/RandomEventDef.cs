using UnityEngine;

public enum EventSolutionType
{
    ItemByAsset,   // requiere un ItemEspacial específico
    ItemByTipo,    // requiere un tipo de ítem (Prevención, etc.)
    PagarCreditos  // se resuelve pagando
}

[CreateAssetMenu(fileName = "Nuevo Evento", menuName = "Eventos/RandomEventDef")]
public class RandomEventDef : ScriptableObject
{
    [Header("Definición")]
    public string titulo;
    [TextArea(2, 4)] public string descripcion;

    [Header("Resolución requerida")]
    public EventSolutionType solucion;
    public ItemEspacial itemRequerido;  // si solucion = ItemByAsset
    public TipoItem tipoRequerido;      // si solucion = ItemByTipo
    public int costoReparacion;         // si solucion = PagarCreditos

    [Header("Reglas de uso del ítem")]
    public bool consumirItemAlUsar = true; // ¿se elimina del inventario al resolver?

    [Header("Tiempo para resolver (segundos, tiempo real)")]
    public float tiempoLimite = 60f;

    [Header("Consecuencias si expira")]
    public int multaCreditos = 0;
    public int dañoSalud = 0;
    public int dañoEnergia = 0;

    [Header("Recompensas (opcional)")]
    public int recompensaCreditos = 0;
}
