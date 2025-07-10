using System.Collections.Generic;
using UnityEngine;

public class JugadorFinanzas : MonoBehaviour
{
    public int creditos = 200;
    public List<ItemEspacial> inventario = new List<ItemEspacial>();

    public void Comprar(ItemEspacial item)
    {
        if (creditos >= item.costo)
        {
            creditos -= item.costo;
            inventario.Add(item);
            Debug.Log("Compraste: " + item.nombre);
        }
        else
        {
            Debug.Log("No tienes suficientes créditos");
        }
    }

    public void Vender(ItemEspacial item)
    {
        if (inventario.Contains(item))
        {
            creditos += item.valorVenta;
            inventario.Remove(item);
            Debug.Log($"Vendiste: {item.nombre}");
        }
    }
}
