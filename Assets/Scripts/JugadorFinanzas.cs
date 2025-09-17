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
    public bool inversionesDesbloqueadas = false;

    [Header("Interés de Ahorro")]
    public bool ahorroGeneraInteres = true;
    [Tooltip("Porcentaje de interés por periodo (ej. 1 = 1%)")]
    public float interesPorPeriodo = 1f;
    [Tooltip("Segundos entre cada interés")]
    public float periodoInteresSegundos = 60f;

    [Header("Sistema de Inversión (progresión extra)")]
    public int inversionesSegurasRealizadas = 0;
    public bool inversionesRiesgosasDesbloqueadas = false;

    [Header("Prevención / Seguro (si lo usas)")]
    public bool tieneSeguro = false;

    [Header("Notificador de ahorro (opcional)")]
    [SerializeField] float intervaloAhorroSegundos = 60f;
    int ultimoAhorroNotificado = -1;

    Coroutine rutinaAhorroNotif;
    Coroutine rutinaInteres;

    void Awake()
    {
        if (instancia != null && instancia != this) { Destroy(gameObject); return; }
        instancia = this;
    }

    void Start()
    {
        UIManager.instancia?.ActualizarCreditos(creditos);
        UIManager.instancia?.ActualizarAhorro(saldoAhorro);
        UIManager.instancia?.ActualizarInventarioUI(inventario);
    }

    void OnEnable()
    {
        if (rutinaAhorroNotif == null)
            rutinaAhorroNotif = StartCoroutine(NotificadorAhorro());

        if (ahorroGeneraInteres && rutinaInteres == null)
            rutinaInteres = StartCoroutine(RutinaInteresAhorro());
    }

    void OnDisable()
    {
        if (rutinaAhorroNotif != null)
        {
            StopCoroutine(rutinaAhorroNotif);
            rutinaAhorroNotif = null;
        }
        if (rutinaInteres != null)
        {
            StopCoroutine(rutinaInteres);
            rutinaInteres = null;
        }
    }
    

    public void AgregarCreditos(int cantidad)
    {
        if (cantidad == 0) return;
        creditos = Mathf.Max(0, creditos + cantidad);
        UIManager.instancia?.ActualizarCreditos(creditos);
        
    }

    public bool TryGastarCreditos(int monto)
    {
        if (monto <= 0) return false;
        if (creditos < monto) return false;

        creditos -= monto;
        UIManager.instancia?.ActualizarCreditos(creditos);

      

        return true;
    }

    
    public void Depositar(int cantidad)
    {
        if (cantidad <= 0) return;

        if (!TryGastarCreditos(cantidad))
        {
            NotificationManager.Instancia?.Notify("Créditos insuficientes para depositar.", NotificationType.Warning, 2.5f);
            return;
        }

        saldoAhorro += cantidad;
        UIManager.instancia?.ActualizarAhorro(saldoAhorro);

        usoAhorro = true;
        VerificarDesbloqueoInversiones();

        NotificationManager.Instancia?.Notify($"Depositaste {cantidad}. Ahorro: {saldoAhorro}", NotificationType.Success, 2f);
    }

    public void Retirar(int cantidad)
    {
        if (cantidad <= 0 || cantidad > saldoAhorro) return;

        saldoAhorro -= cantidad;
        UIManager.instancia?.ActualizarAhorro(saldoAhorro);

        AgregarCreditos(cantidad);

        usoAhorro = true;
        VerificarDesbloqueoInversiones();

        NotificationManager.Instancia?.Notify($"Retiraste {cantidad}. Ahorro: {saldoAhorro}", NotificationType.Info, 2f);
    }

    System.Collections.IEnumerator RutinaInteresAhorro()
    {
        var wait = new WaitForSecondsRealtime(periodoInteresSegundos);
        while (true)
        {
            yield return wait;

            if (!ahorroGeneraInteres) continue;
            if (saldoAhorro <= 0) continue;

            int interes = Mathf.Max(1, Mathf.FloorToInt(saldoAhorro * (interesPorPeriodo / 100f)));
            saldoAhorro += interes;
            UIManager.instancia?.ActualizarAhorro(saldoAhorro);

            NotificationManager.Instancia?.Notify(
                $"Tu ahorro generó +{interes}. Total: {saldoAhorro}",
                NotificationType.Success, 2.5f
            );
        }
    }

    System.Collections.IEnumerator NotificadorAhorro()
    {
        var wait = new WaitForSecondsRealtime(intervaloAhorroSegundos);
        while (true)
        {
            yield return wait;

            if (saldoAhorro != ultimoAhorroNotificado)
            {
                ultimoAhorroNotificado = saldoAhorro;
                NotificationManager.Instancia?.Notify(
                    $"Ahorro actualizado: {saldoAhorro} créditos.",
                    NotificationType.Info, 2f
                );
            }
        }
    }

    // ================== Comprar ==================
    public bool Comprar(ItemEspacial item, int precioOverride)
    {
        if (item == null) return false;

        if (inventario.Count >= maxInventario)
        {
            NotificationManager.Instancia?.Notify("Inventario lleno.", NotificationType.Warning, 2.5f);
            return false;
        }

        if (!TryGastarCreditos(precioOverride))
        {
            NotificationManager.Instancia?.Notify($"Créditos insuficientes ({precioOverride}).", NotificationType.Warning, 2.5f);
            return false;
        }

        var copia = ScriptableObject.Instantiate(item);
        inventario.Add(copia);

        UIManager.instancia?.ActualizarInventarioUI(inventario);
       

        hizoCompra = true;
        VerificarDesbloqueoInversiones();
        return true;
    }

    public bool Comprar(ItemEspacial item)
    {
        if (item == null) return false;
        return Comprar(item, item.costo);
    }

    
    public void UsarItemPorIndice(int index)
    {
        if (index < 0 || index >= inventario.Count) return;

        var item = inventario[index];

        // 1) Efecto sobre necesidades 
        if (item.satisface != NecesidadTipo.Ninguna &&
            item.efectoNecesidad > 0 &&
            NeedsSystem.Instancia != null)
        {
            NeedsSystem.Instancia.AplicarEfecto(item.satisface, item.efectoNecesidad);
        }
        var ctx = new EfectoContexto
        {
            finanzas = this,
            needs = NeedsSystem.Instancia,
            eventos = EventsManager.Instancia,
            itemFuente = item,
            usuarioGO = this.gameObject
        };



        if (item.efectos != null)
        {
            for (int i = 0; i < item.efectos.Count; i++)
            {
                var ef = item.efectos[i];
                if (ef != null) ef.Aplicar(ctx);
            }
        }

        // 3) Intento de resolver evento con on item used
        bool resuelto = false;
        if (EventsManager.Instancia != null)
        {
            resuelto = EventsManager.Instancia.OnItemUsed(item);
        }

        inventario.RemoveAt(index);
        UIManager.instancia?.ActualizarInventarioUI(inventario);

        NotificationManager.Instancia?.Notify($"Usaste {item.nombre}.", NotificationType.Info, 2f);
    }

    
    public void Vender(int index)
    {
        if (index < 0 || index >= inventario.Count) return;

        var item = inventario[index];
        inventario.RemoveAt(index);

        AgregarCreditos(item.valorVenta);
        UIManager.instancia?.ActualizarInventarioUI(inventario);

        hizoVenta = true;
        VerificarDesbloqueoInversiones();

        NotificationManager.Instancia?.Notify(
            $"Vendiste {item.nombre}. +{item.valorVenta} créditos.",
            NotificationType.Success, 2f
        );
    }

    // ================== Inversiones / Progresión ==================
    public bool PuedeInvertir()
    {
        return inversionesDesbloqueadas;
    }

    public void VerificarDesbloqueoInversiones()
    {
        if (!inversionesDesbloqueadas && hizoCompra && hizoVenta && usoAhorro)
        {
            inversionesDesbloqueadas = true;
            NotificationManager.Instancia?.Notify(
                "¡Has desbloqueado las inversiones!",
                NotificationType.Success, 2.5f
            );
        }
    }

    public void RegistrarInversionSegura()
    {
        inversionesSegurasRealizadas++;
        if (inversionesSegurasRealizadas >= 2 && !inversionesRiesgosasDesbloqueadas)
        {
            inversionesRiesgosasDesbloqueadas = true;
            Debug.Log("Inversiones riesgosas desbloqueadas");
            
        }
    }
}
