using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InvestmentManager : MonoBehaviour
{
    public static InvestmentManager Instancia;

    private class PendingInvestment
    {
        public bool esRiesgosa;
        public int cantidad;
        public int gananciaBase;
        public float duracion;
        public string nombreInversion;
        public System.Action onResolvedUI; // opcional: para avisar a la carta si sigue visible
    }

    private readonly List<PendingInvestment> enCurso = new List<PendingInvestment>();

    void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
        // Este objeto debe estar SIEMPRE activo (no lo metas dentro de paneles que se desactivan)
        // Si quieres que sobreviva de escena a escena:
        // DontDestroyOnLoad(gameObject);
    }

    public void IniciarInversion(string nombreInversion, bool esRiesgosa, int cantidad, int gananciaBase, float duracion, System.Action onResolvedUI = null)
    {
        var inv = new PendingInvestment
        {
            esRiesgosa = esRiesgosa,
            cantidad = cantidad,
            gananciaBase = gananciaBase,
            duracion = Mathf.Max(0f, duracion),
            nombreInversion = nombreInversion,
            onResolvedUI = onResolvedUI
        };
        enCurso.Add(inv);
        StartCoroutine(RutinaProcesar(inv));
    }

    private IEnumerator RutinaProcesar(PendingInvestment inv)
    {
        // Espera en tiempo REAL para que funcione aunque el juego esté pausado por UI
        float t = 0f;
        while (t < inv.duracion)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        // Resolver inversión
        if (inv.esRiesgosa)
        {
            bool gano = Random.value < 0.5f;
            if (!gano)
            {
                // Ya se descontó la cantidad al confirmar; aquí no se devuelve.
                NotificationManager.Instancia?.Notify(
                    $"{inv.nombreInversion}: Perdiste {inv.cantidad} créditos.",
                    NotificationType.Warning
                );
            }
            else
            {
                int mult = Random.Range(2, 5);
                int retorno = inv.cantidad * mult;
                UIManager.instancia?.AgregarCreditos(retorno);
                NotificationManager.Instancia?.Notify(
                    $"{inv.nombreInversion}: ¡Ganaste! +{retorno} (x{mult}).",
                    NotificationType.Success
                );
            }
        }
        else
        {
            int retorno = inv.cantidad + Mathf.Max(0, inv.gananciaBase);
            UIManager.instancia?.AgregarCreditos(retorno);
            UIManager.instancia?.RegistrarInversionSeguraExitosa();
            UIManager.instancia?.RefrescarInteractividadCartas();
            NotificationManager.Instancia?.Notify(
                $"{inv.nombreInversion}: Retorno +{retorno} (+{inv.gananciaBase}).",
                NotificationType.Info
            );
        }

        inv.onResolvedUI?.Invoke(); // por si la carta sigue visible y quiere resetear su UI
        enCurso.Remove(inv);
    }
}
