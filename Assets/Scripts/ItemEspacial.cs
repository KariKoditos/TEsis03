using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Nuevo Item", menuName = "Items/ItemEspacial")] //fabrica de datos de items
public class ItemEspacial : ScriptableObject
{
    public string nombre;
    public int costo;
    public int valorVenta;
    public Sprite icono;
    public TipoItem tipo;
   

    [TextArea(2, 4)]    
    public string descripcion;

    [Header("Uso general")]
    public bool consumible = true;

    [Header("Prevención / Seguro")]
    public bool esSeguroNave = false;

    [Header("Prevención")]
    public string tagPrevencion;

    [Header("Efectos de Necesidad (solo si tipo = Necesidad)")]
    public NecesidadTipo satisface = NecesidadTipo.Ninguna;
    public int efectoNecesidad = 0;

    [Header("Efectos (Decorator)")]
    [SerializeReference] public List<IEfectoDeItem> efectos = new List<IEfectoDeItem>(); 
}


public enum TipoItem
{
    Necesidad,
    Lujo,
    Inversión,
    Prevención
}


public enum NecesidadTipo
{
    Ninguna,
    Comida,
    Salud,
    Energia
}