using UnityEngine;

public enum TipoItem
{
    Necesidad,
    Lujo,
    Inversion,
    Prevencion
}

[System.Serializable]
public class ItemEspacial
{
    public string nombre;
    public int costo;
    public int valorVenta;
    public TipoItem tipo;
    public Sprite icono;
}
