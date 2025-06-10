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
            Debug.Log($"Compraste: {item.nombre}");

            switch (item.tipo)
            {
                case TipoItem.Necesidad:
                    Debug.Log("Has cubierto una necesidad.");
                    break;
                case TipoItem.Lujo:
                    Debug.Log("Compraste un lujo.");
                    break;
                case TipoItem.Inversion:
                    Debug.Log("Hiciste una inversión.");
                    break;
                case TipoItem.Prevencion:
                    Debug.Log("Te preparaste para emergencias.");
                    break;
            }
        }
        else
        {
            Debug.Log("No tienes suficientes créditos.");
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
