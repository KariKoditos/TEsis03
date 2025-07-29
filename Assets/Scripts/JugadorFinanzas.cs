using System.Collections.Generic;
using UnityEngine;

public class JugadorFinanzas : MonoBehaviour
{
    public static JugadorFinanzas instancia;

    public int creditos = 200;
    public int maxInventario = 5;
    public int saldoAhorro = 0;
    public List<ItemEspacial> inventario = new List<ItemEspacial>();

    private void Awake()
    {
        if (instancia == null) instancia = this;
        else Destroy(gameObject);
    }

    public void Depositar(int cantidad)
    {
        if (cantidad <= 0 || cantidad > creditos)
        {
            Debug.Log(" No tienes suficientes créditos para depositar esa cantidad.");
            return;
        }

        creditos -= cantidad;
        saldoAhorro += cantidad;

        UIManager.instancia.ActualizarCreditos(creditos);
        UIManager.instancia.ActualizarAhorro(saldoAhorro);
        Debug.Log($"Depositaste {cantidad} créditos. Saldo ahorro: {saldoAhorro}");
    }

    public void Retirar(int cantidad)
    {
        if (cantidad <= 0 || cantidad > saldoAhorro)
        {
            Debug.Log("No tienes suficiente saldo en ahorro para retirar esa cantidad.");
            return;
        }

        saldoAhorro -= cantidad;
        creditos += cantidad;

        UIManager.instancia.ActualizarCreditos(creditos);
        UIManager.instancia.ActualizarAhorro(saldoAhorro);
        Debug.Log($"Retiraste {cantidad} créditos. Saldo ahorro: {saldoAhorro}");
    }

    public void Comprar(ItemEspacial item)
    {
        if (inventario.Count >= maxInventario)
        {
            Debug.Log("Inventario lleno. No puedes comprar más objetos.");
            return;
        }

        if (creditos >= item.costo)
        {
            creditos -= item.costo;
            inventario.Add(item);  
            Debug.Log($"Compraste: {item.nombre}. Créditos restantes: {creditos}");

            

            UIManager.instancia.ActualizarCreditos(creditos);
            UIManager.instancia.ActualizarInventarioUI(inventario);
        }
        else
        {
            Debug.Log("No tienes suficientes créditos para comprar este ítem.");
        }
    }


    public void Vender(int index)
    {
        if (index < 0 || index >= inventario.Count)
        {
            Debug.Log(" No hay ítem en este slot para vender.");
            return;
        }

        ItemEspacial item = inventario[index];
        creditos += item.valorVenta;
        inventario.RemoveAt(index);

        Debug.Log($" Vendiste: {item.nombre}. Créditos actuales: {creditos}");
        UIManager.instancia.ActualizarCreditos(creditos);
        UIManager.instancia.ActualizarInventarioUI(inventario);
    }
}
