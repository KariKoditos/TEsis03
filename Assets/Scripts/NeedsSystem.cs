using UnityEngine;
using System;



public enum NeedType
{ 

    Comida, 
    Salud, 
    Energia 

}

[Serializable]
public class NeedStatusConf
{
    public NeedType tipo;
    [Range(0, 200)] public int valorInicial = 200;
    [Range(0, 20)] public int decaimientoPorTick = 1; 
}

public class NeedsSystem : MonoBehaviour
{
    public static NeedsSystem Instancia;

    [Header("Config")]
    public NeedStatusConf comida = new NeedStatusConf { tipo = NeedType.Comida, valorInicial = 200, decaimientoPorTick = 1 };
    public NeedStatusConf salud = new NeedStatusConf { tipo = NeedType.Salud, valorInicial = 200, decaimientoPorTick = 1 };
    public NeedStatusConf energia = new NeedStatusConf { tipo = NeedType.Energia, valorInicial = 200, decaimientoPorTick = 1 };

    [Header("Alertas")]
    [Range(1, 99)] public int umbralAlerta = 40;
    
    public int resetAlertaOffset = 5;


    public float tickSeconds = 3f;

    int _comida, _salud, _energia;
    float _t;
    public bool usarTiempoReal = true;

    public bool debugLogs = true;
    bool alertComidaEnviada, alertSaludEnviada, alertEnergiaEnviada;


    // Eventos para UI/derrota
    public event Action<NeedType, int> OnNeedChanged;
    public event Action OnAnyNeedZero;

    void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;

        _comida = Mathf.Clamp(comida.valorInicial, 0, 200);
        _salud = Mathf.Clamp(salud.valorInicial, 0, 200);
        _energia = Mathf.Clamp(energia.valorInicial, 0, 200);
    }

    void Start()
    {
        // dispara valores iniciales a la UI
        OnNeedChanged?.Invoke(NeedType.Comida, _comida);
        OnNeedChanged?.Invoke(NeedType.Salud, _salud);
        OnNeedChanged?.Invoke(NeedType.Energia, _energia);
    }

    void Update()
    {
        _t += usarTiempoReal ? Time.unscaledDeltaTime : Time.deltaTime; // <-- clave
        if (_t >= tickSeconds)
        {
            _t = 0f;
            Tick();
        }
    }

    void Tick()
    {
        Modificar(NeedType.Comida, -comida.decaimientoPorTick);
        Modificar(NeedType.Salud, -salud.decaimientoPorTick);
        Modificar(NeedType.Energia, -energia.decaimientoPorTick);

        if (debugLogs)
        {
            Debug.Log($"[NeedsSystem] C:{_comida} S:{_salud} E:{_energia}  (timeScale={Time.timeScale})");
        }
            

    }

    public void AplicarEfecto(NecesidadTipo necesidad, int cantidad)
    {
        if (necesidad == NecesidadTipo.Ninguna || cantidad == 0) return;

        switch (necesidad)
        {
            case NecesidadTipo.Comida: Modificar(NeedType.Comida, cantidad); break;
            case NecesidadTipo.Salud: Modificar(NeedType.Salud, cantidad); break;
            case NecesidadTipo.Energia: Modificar(NeedType.Energia, cantidad); break;
        }
    }

    public int GetValor(NeedType t)
    {
        return t switch
        {
            NeedType.Comida => _comida,
            NeedType.Salud => _salud,
            NeedType.Energia => _energia,
            _ => 0
        };
    }

    public int GetValor(NecesidadTipo n)
    {
        return n switch
        {
            NecesidadTipo.Comida => GetValor(NeedType.Comida),
            NecesidadTipo.Salud => GetValor(NeedType.Salud),
            NecesidadTipo.Energia => GetValor(NeedType.Energia),
            _ => 0
        };
    }

    public void Modificar(NeedType t, int delta)
    {
        int v = GetValor(t) + delta;
        v = Mathf.Clamp(v, 0, 200);

        switch (t)
        {
            case NeedType.Comida: _comida = v; break;
            case NeedType.Salud: _salud = v; break;
            case NeedType.Energia: _energia = v; break;
        }

        OnNeedChanged?.Invoke(t, v);

        if (v <= 0)
            OnAnyNeedZero?.Invoke();

        ChecarAlertas(t, v);
    }

    void ChecarAlertas(NeedType t, int v)
    {
        ref bool flag = ref alertComidaEnviada;
        if (t == NeedType.Salud) flag = ref alertSaludEnviada;
        if (t == NeedType.Energia) flag = ref alertEnergiaEnviada;

        if (v <= umbralAlerta && !flag)
        {
            flag = true;

            // texto amigable + sugerencia rápida
            string sugerencia = t switch
            {
                NeedType.Comida => "Compra/usa comida.",
                NeedType.Salud => "Compra/usa kit médico.",
                NeedType.Energia => "Compra/usa una Soda.",
                _ => "Revisa la tienda."
            };

            NotificationManager.Instancia?.Notify(
                $" {t} baja: {v}%. {sugerencia}",
                NotificationType.Warning
            );
        }

        // si recupera por encima del umbral + offset, permitimos volver a alertar en el futuro
        if (flag && v >= umbralAlerta + resetAlertaOffset)
            flag = false;
    }



}
