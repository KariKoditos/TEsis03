using UnityEngine;

public enum EventSolutionType
{
    ItemByAsset,   // requiere un ItemEspacial espec�fico
    ItemByTipo,    // requiere un tipo de �tem (Prevenci�n, etc.)
    PagarCreditos  // se resuelve pagando
}

[CreateAssetMenu(fileName = "Nuevo Evento", menuName = "Eventos/RandomEventDef")]
public class RandomEventDef : ScriptableObject
{
    [Header("Definici�n")]
    public string titulo;
    [TextArea(2, 4)] public string descripcion;

    [Header("Resoluci�n requerida")]
    public EventSolutionType solucion;
    public ItemEspacial itemRequerido;  // si solucion = ItemByAsset
    public TipoItem tipoRequerido;      // si solucion = ItemByTipo
    public int costoReparacion;         // si solucion = PagarCreditos

    [Header("Reglas de uso del �tem")]
    public bool consumirItemAlUsar = true; // �se elimina del inventario al resolver?

    [Header("Tiempo para resolver (segundos, tiempo real)")]
    public float tiempoLimite = 60f;

    [Header("Consecuencias si expira")]
    public int multaCreditos = 0;
    public int da�oSalud = 0;
    public int da�oEnergia = 0;

    [Header("Recompensas (opcional)")]
    public int recompensaCreditos = 0;
}
