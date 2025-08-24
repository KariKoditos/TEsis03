using System.Collections.Generic;
using UnityEngine;

public class JugadorFinanzas : MonoBehaviour
{
    public static JugadorFinanzas instancia;

    [Header("Economía")]
    public int creditos = 1000;
    public int saldoAhorro = 0;

    [Header("Inventario")]
    public int maxInventario = 5;
    public List<ItemEspacial> inventario = new List<ItemEspacial>();

    [Header("Progresión / Desbloqueos")]
    public bool hizoCompra = false;
    public bool hizoVenta = false;
    public bool usoAhorro = false;

    [Header("Sistema de Inversión")]
    public int inversionesSegurasRealizadas = 2;
    public bool inversionesRiesgosasDesbloqueadas = false;

    [Header("Prevención / Seguro")]
    public bool tieneSeguro = false;

    private void Awake()
    {
        if (instancia == null) instancia = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        
        UIManager.instancia?.ActualizarCreditos(creditos);
        UIManager.instancia?.ActualizarAhorro(saldoAhorro);
        UIManager.instancia?.ActualizarInventarioUI(inventario);
    }


    public void Depositar(int cantidad)
    {
        if (cantidad <= 0)
        {
            Debug.Log("Cantidad inválida para depositar.");
            return;
        }

        // Intenta gastar desde créditos (valida fondos y refresca UI)
        if (!TryGastarCreditos(cantidad))
        {
            Debug.Log("No tienes suficientes créditos para depositar esa cantidad.");
            return;
        }

        // Si sí alcanzó, mueve al ahorro
        saldoAhorro += cantidad;

        // Refresca ahorro en UI (créditos ya se refrescaron en TryGastarCreditos)
        RefrescarUIEconomia();

        Debug.Log($"Depositaste {cantidad} créditos. Saldo ahorro: {saldoAhorro}");

    }

    public void Retirar(int cantidad)
    {
        if (cantidad <= 0)
        {
            Debug.Log("Cantidad inválida para retirar.");
            return;
        }

        if (cantidad > saldoAhorro)
        {
            Debug.Log("No tienes suficiente saldo en ahorro para retirar esa cantidad.");
            return;
        }

        saldoAhorro -= cantidad;

        // Suma a créditos y refresca UI con helper
        AgregarCreditos(cantidad);

        usoAhorro = true;
        VerificarDesbloqueoInversiones();

        Debug.Log($"Retiraste {cantidad} créditos. Saldo ahorro: {saldoAhorro}");
    }

    public void Comprar(ItemEspacial item)
    {
        if (item == null)
        {
            Debug.LogWarning("Intentaste comprar un item nulo.");
            return;
        }

        if (inventario.Count >= maxInventario)
        {
            Debug.Log("Inventario lleno. No puedes comprar más objetos.");
            return;
        }

        // Usa helper: valida fondos, descuenta y refresca UI automáticamente
        if (!TryGastarCreditos(item.costo))
        {
            Debug.Log("No tienes suficientes créditos para comprar este ítem.");
            return;
        }

        // Instancia segura del ScriptableObject (evitas compartir la referencia de la tienda)
        ItemEspacial copia = ScriptableObject.Instantiate(item);
        inventario.Add(copia);

        // Solo refrescamos inventario; créditos ya se refrescaron
        UIManager.instancia?.ActualizarInventarioUI(inventario);

        hizoCompra = true;
        VerificarDesbloqueoInversiones();

        Debug.Log($"Compraste: {item.nombre}. Créditos restantes: {creditos}");

    }

    public void Vender(int index)
    {
        if (index < 0 || index >= inventario.Count)
        {
            Debug.Log("No hay ítem en este slot para vender.");
            return;
        }

        ItemEspacial item = inventario[index];
        inventario.RemoveAt(index);

        // Suma créditos y refresca UI con helper
        AgregarCreditos(item.valorVenta);

        // Refresca inventario en UI
        UIManager.instancia?.ActualizarInventarioUI(inventario);

        hizoVenta = true;
        VerificarDesbloqueoInversiones();

        Debug.Log($"Vendiste: {item.nombre}. Créditos actuales: {creditos}");
    }


    public void EliminarItem(int index)
    {
        if (index < 0 || index >= inventario.Count) return;
        var item = inventario[index];
        inventario.RemoveAt(index);
        UIManager.instancia?.ActualizarInventarioUI(inventario);
        Debug.Log($"Consumiste/Eliminaste: {item.nombre}");
    }

    public void UsarItemPorIndice(int index)
    {
        if (index < 0 || index >= inventario.Count)
        {
            Debug.LogWarning("Índice fuera de rango al intentar usar ítem.");
            return;
        }
        UsarItem(inventario[index], index);
    }

    public void UsarItem(ItemEspacial item, int indexEnInventario = -1)
    {
        if (item == null)
        {
            Debug.LogWarning("Item nulo al intentar usar.");
            return;
        }

        // === NECESIDAD: rellena barra correspondiente ===
        if (item.tipo == TipoItem.Necesidad && item.efectoNecesidad > 0 && item.satisface != NecesidadTipo.Ninguna)
        {
            if (NeedsSystem.Instancia != null)
            {
                NeedsSystem.Instancia.AplicarEfecto(item.satisface, item.efectoNecesidad);
                Debug.Log($"Usaste {item.nombre}: +{item.efectoNecesidad} a {item.satisface}");
            }
            else
            {
                Debug.LogWarning("NeedsSystem.Instancia es null. Asegúrate de tener el NeedsSystem en la escena.");
            }

            // Consumible -> se elimina del inventario
            if (indexEnInventario >= 0) EliminarItem(indexEnInventario);
            return;
        }

        if (item.tipo == TipoItem.Prevención)
        {
            tieneSeguro = true;
            Debug.Log(" Seguro de nave activado.");
            if (indexEnInventario >= 0) EliminarItem(indexEnInventario); // si es consumible
            return;
        }

        if (item.tipo == TipoItem.Inversión)
        {
            Debug.Log("Este ítem es de inversión. Usa tu panel/sistema de inversiones para gestionarlo.");
            return;
        }

        
        Debug.Log($"'{item.nombre}' no tiene efecto de necesidad. Tipo: {item.tipo}");
    }

    void VerificarDesbloqueoInversiones()
    {
        if (hizoCompra && hizoVenta && usoAhorro)
        {
            UIManager.instancia.ActivarBotonInversion();
            Debug.Log("¡Mecánica de inversión desbloqueada!");
        }
    }

    public void RegistrarInversionSegura()
    {
        inversionesSegurasRealizadas++;

        if (inversionesSegurasRealizadas >= 2 && !inversionesRiesgosasDesbloqueadas)
        {
            inversionesRiesgosasDesbloqueadas = true;
            Debug.Log("Inversiones riesgosas desbloqueadas");
           // UIManager.instancia.MostrarMensajeDesbloqueo(); // si quieres mostrar algo visual
        }
    }

    public void AgregarCreditos(int cantidad)
    {
        if (cantidad == 0) return;
        creditos = Mathf.Max(0, creditos + cantidad);
        RefrescarUIEconomia();
    }

    public bool TryGastarCreditos(int monto)
    {
        if (monto <= 0) return false;
        if (creditos < monto) return false;

        creditos -= monto;
        RefrescarUIEconomia();
        return true;
    }

    private void RefrescarUIEconomia()
    {
        UIManager.instancia?.ActualizarCreditos(creditos);
        UIManager.instancia?.ActualizarAhorro(saldoAhorro);
    }



}
