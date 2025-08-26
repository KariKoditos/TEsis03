using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryManager : MonoBehaviour
{
    [Header("Escena de Victoria")]
    public string escenaVictoria = "Victoria";
    public float delaySegundos = 0f;

    [Header("Meta financiera")]
    public bool usarMetaFinanciera = true;
    public int metaFinanciera = 5000;
    public bool contarCreditos = true;
    public bool contarAhorro = true;
    public bool contarInventario = false;

    [Header("Supervivencia por tiempo")]
    public bool usarSupervivencia = true;
    public float tiempoSupervivenciaSeg = 300f; // 5 min
    public bool mostrarAvisosProgreso = true;

    

    private bool victoriaDisparada = false;
    private float tiempoAcumulado = 0f;
    private float chequeoFinanzasCada = 0.5f;
    private float relojChequeo = 0f;
    bool aviso80Meta = false;
    bool aviso30s = false;

    void OnEnable()
    {
        
        if (NeedsSystem.Instancia != null) NeedsSystem.Instancia.OnAnyNeedZero += CancelarPorGameOver;
    }

    void OnDisable()
    {
        if (NeedsSystem.Instancia != null) NeedsSystem.Instancia.OnAnyNeedZero -= CancelarPorGameOver;
    }

    void Update()
    {
        if (victoriaDisparada) return;

       
        if (usarSupervivencia)
        {
            tiempoAcumulado += Time.deltaTime;
            if (tiempoAcumulado >= tiempoSupervivenciaSeg)
            {
                DispararVictoria("Has completado la supervivencia.");
                return;
            }
        }

        
        if (usarMetaFinanciera)
        {
            relojChequeo += Time.deltaTime;
            if (relojChequeo >= chequeoFinanzasCada)
            {
                relojChequeo = 0f;
                if (CalcularRiqueza() >= metaFinanciera)
                {
                    DispararVictoria($"¡Alcanzaste tu meta de {metaFinanciera} créditos!");
                    return;
                }
            }
        }
    }

    

    int CalcularRiqueza()
    {
        int total = 0;
        if (JugadorFinanzas.instancia != null)
        {
            if (contarCreditos) total += Mathf.Max(0, JugadorFinanzas.instancia.creditos);
            if (contarAhorro) total += Mathf.Max(0, JugadorFinanzas.instancia.saldoAhorro);

            if (contarInventario && JugadorFinanzas.instancia.inventario != null)
            {
                foreach (var it in JugadorFinanzas.instancia.inventario)
                    if (it != null) total += Mathf.Max(0, it.valorVenta);
            }
        }
        return total;
    }

    void DispararVictoria(string motivo)
    {
        if (victoriaDisparada) return;
        victoriaDisparada = true;

        
        NotificationManager.Instancia?.Notify(motivo, NotificationType.Success);

        
        Time.timeScale = 1f;

        if (delaySegundos > 0f) Invoke(nameof(CargarEscenaVictoria), delaySegundos);
        else CargarEscenaVictoria();
    }

    void CargarEscenaVictoria()
    {
        if (string.IsNullOrEmpty(escenaVictoria))
        {
            Debug.LogError("VictoryManager: escenaVictoria no asignada.");
            return;
        }
        SceneManager.LoadScene(escenaVictoria, LoadSceneMode.Single);
    }

    void CancelarPorGameOver()
    {
        
        enabled = false;
    }

    string FormatearTiempo(float s)
    {
        int seg = Mathf.CeilToInt(s);
        int m = seg / 60;
        int r = seg % 60;
        return $"{m:00}:{r:00}";
    }

    
    

    void AvisosProgresoMeta(int total)
    {
        if (metaFinanciera <= 0) return;
        if (!aviso80Meta && total >= metaFinanciera * 0.8f)
        {
            aviso80Meta = true;
            NotificationManager.Instancia?.Notify( "¡Vas al 80% de tu meta financiera! ¡Sigue así!", NotificationType.Info);
        }
    }

    void AvisosProgresoTiempo(float restante)
    {
        if (!aviso30s && restante <= 30f && restante > 0f)
        {
            aviso30s = true;
            NotificationManager.Instancia?.Notify("¡30 segundos para completar la supervivencia!", NotificationType.Info);
        }
    }
}
