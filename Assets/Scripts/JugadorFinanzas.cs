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

    [Header("Interés de Ahorro")]
    public bool ahorroGeneraInteres = true;
    public float interesPorPeriodo = 1f;     
    public float periodoInteresSegundos = 60f;

    [Header("Sistema de Inversión")]
    public int inversionesSegurasRealizadas = 2;
    public bool inversionesRiesgosasDesbloqueadas = false;

    [Header("Prevención / Seguro")]
    public bool tieneSeguro = false;

    [SerializeField] 
    float intervaloAhorroSegundos = 60f;
    int ultimoAhorroNotificado = -1;
    Coroutine rutinaAhorro;
    private Coroutine rutinaInteres;



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

    void OnEnable()
    {
        if (rutinaAhorro == null)
            rutinaAhorro = StartCoroutine(NotificadorAhorro());

        if (ahorroGeneraInteres && rutinaInteres == null)
            rutinaInteres = StartCoroutine(RutinaInteresAhorro());
    }

    void OnDisable()
    {
        if (rutinaAhorro != null)
        {
            StopCoroutine(rutinaAhorro);
            rutinaAhorro = null;
        }

        if (rutinaInteres != null)
        {
            StopCoroutine(rutinaInteres);
            rutinaInteres = null;
        }
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
        NotificationManager.Instancia?.Notify($" Depositaste {cantidad}. Ahorro: {saldoAhorro}", NotificationType.Success);

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

        NotificationManager.Instancia?.Notify($" Retiraste {cantidad}. Ahorro: {saldoAhorro}", NotificationType.Info);

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

        ItemEspacial copia = ScriptableObject.Instantiate(item);
        inventario.Add(copia);

        // Usa helper: valida fondos, descuenta y refresca UI automáticamente
        if (!TryGastarCreditos(item.costo))
        {
            Debug.Log("No tienes suficientes créditos para comprar este ítem.");
            return;
        }

        if (copia.tipo == TipoItem.Prevención)
        {
            ShipIncidentManager.Instancia?.ResolverConItem(copia.nombre);
            NotificationManager.Instancia?.Notify($"El incidente ha sido resuelto gracias a {copia.nombre}.",NotificationType.Success);


        }



        
        UIManager.instancia.ActualizarInventarioUI(inventario); //refresh creditos inv

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

        if (creditos <= 0)
        {
            Debug.Log("¡Game Over por créditos!");
            GameOverUI.TriggerGameOver();
        }
    }

    public bool TryGastarCreditos(int monto)
    {
        if (monto <= 0) return false;
        if (creditos < monto) return false;

        creditos -= monto;
        RefrescarUIEconomia();

        if (creditos <= 0)
        {
            Debug.Log("¡Game Over por créditos!");
            GameOverUI.TriggerGameOver();
        }

        return true;
    }

    System.Collections.IEnumerator RutinaInteresAhorro()
    {
        var wait = new WaitForSecondsRealtime(periodoInteresSegundos);
        while (true)
        {
            yield return wait;

            if (!ahorroGeneraInteres) continue;
            if (saldoAhorro <= 0) continue;

            // calcula interés (mínimo 1 si hay saldo)
            int interes = Mathf.Max(1, Mathf.FloorToInt(saldoAhorro * (interesPorPeriodo / 100f)));
            saldoAhorro += interes;

            // refresca UI
            UIManager.instancia?.ActualizarAhorro(saldoAhorro);

            // notificación (si tienes tu manager)
            NotificationManager.Instancia?.Notify(
                $" Tu ahorro generó +{interes} créditos. Total: {saldoAhorro}",
                NotificationType.Success
            );
        }
    }

    private void RefrescarUIEconomia()
    {
        UIManager.instancia?.ActualizarCreditos(creditos);
        UIManager.instancia?.ActualizarAhorro(saldoAhorro);
    }

    System.Collections.IEnumerator NotificadorAhorro()
    {
        var wait = new WaitForSecondsRealtime(intervaloAhorroSegundos);
        while (true)
        {
            yield return wait;

            // notifica sólo si cambió desde la última vez (si quieres siempre, elimina el if)
            if (saldoAhorro != ultimoAhorroNotificado)
            {
                ultimoAhorroNotificado = saldoAhorro;
                NotificationManager.Instancia?.Notify(
                    $"Ahorro actualizado: {saldoAhorro} créditos.",
                    NotificationType.Info
                );
            }
        }
    }

}
