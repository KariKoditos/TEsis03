using UnityEngine;

[CreateAssetMenu(fileName = "Nuevo Item", menuName = "Items/ItemEspacial")]
public class ItemEspacial : ScriptableObject
{
    public string nombre;
    public int costo;
    public int valorVenta;
    public Sprite icono;
    public TipoItem tipo;
   

    [TextArea(2, 4)]    
    public string descripcion;

    [Header("Prevenci�n / Seguro")]
    public bool esSeguroNave = false;

    [Header("Efectos de Necesidad (solo si tipo = Necesidad)")]
    public NecesidadTipo satisface = NecesidadTipo.Ninguna;
    public int efectoNecesidad = 0;
}


public enum TipoItem
{
    Necesidad,
    Lujo,
    Inversi�n,
    Prevenci�n
}


public enum NecesidadTipo
{
    Ninguna,
    Comida,
    Salud,
    Energia
}