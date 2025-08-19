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
}

public enum TipoItem
{
    Necesidad,
    Lujo,
    Inversi�n,
    Prevenci�n
}
