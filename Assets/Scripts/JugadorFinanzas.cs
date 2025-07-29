using System.Collections.Generic;
using UnityEngine;

public class JugadorFinanzas : MonoBehaviour
{
    public static JugadorFinanzas instancia;

    public int creditos = 200;
    public int maxInventario = 5;  // L�mite de objetos
    public List<ItemEspacial> inventario = new List<ItemEspacial>();

    private void Awake()
    {
        if (instancia == null) instancia = this;
        else Destroy(gameObject);
    }

    public void Comprar(ItemEspacial item)
    {
        if (inventario.Count >= maxInventario)
        {
            Debug.Log("Inventario lleno. No puedes comprar m�s objetos.");
            return;
        }

        if (creditos >= item.costo)
        {
            creditos -= item.costo;
            inventario.Add(item);  // Agregar al inventario
            Debug.Log($"Compraste: {item.nombre}. Cr�ditos restantes: {creditos}");

            // Actualizar la UI
            UIManager.instancia.ActualizarCreditos(creditos);
            UIManager.instancia.ActualizarInventarioUI(inventario);
        }
        else
        {
            Debug.Log("No tienes suficientes cr�ditos para comprar este �tem.");
        }
    }


    public void Vender(int index)
    {
        if (index < 0 || index >= inventario.Count)
        {
            Debug.Log(" No hay �tem en este slot para vender.");
            return;
        }

        ItemEspacial item = inventario[index];
        creditos += item.valorVenta;
        inventario.RemoveAt(index);

        Debug.Log($" Vendiste: {item.nombre}. Cr�ditos actuales: {creditos}");
        UIManager.instancia.ActualizarCreditos(creditos);
        UIManager.instancia.ActualizarInventarioUI(inventario);
    }
}
