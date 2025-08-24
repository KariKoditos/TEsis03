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
    [Range(0, 100)] public int valorInicial = 100;
    [Range(0, 10)] public int decaimientoPorTick = 1; // cuánto baja cada tick
}

public class NeedsSystem : MonoBehaviour
{
    public static NeedsSystem Instancia;

    [Header("Config")]
    public NeedStatusConf comida = new NeedStatusConf { tipo = NeedType.Comida, valorInicial = 100, decaimientoPorTick = 1 };
    public NeedStatusConf salud = new NeedStatusConf { tipo = NeedType.Salud, valorInicial = 100, decaimientoPorTick = 1 };
    public NeedStatusConf energia = new NeedStatusConf { tipo = NeedType.Energia, valorInicial = 100, decaimientoPorTick = 1 };

    [Tooltip("Cada cuántos segundos aplica el decaimiento")]
    public float tickSeconds = 3f;

    int _comida, _salud, _energia;
    float _t;

    public bool debugLogs = true;

    // Eventos para UI/derrota
    public event Action<NeedType, int> OnNeedChanged;
    public event Action OnAnyNeedZero;

    void Awake()
    {
        Instancia = this;
        _comida = Mathf.Clamp(comida.valorInicial, 0, 100);
        _salud = Mathf.Clamp(salud.valorInicial, 0, 100);
        _energia = Mathf.Clamp(energia.valorInicial, 0, 100);
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
        _t += Time.deltaTime;
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

    void Modificar(NeedType t, int delta)
    {
        int v = GetValor(t) + delta;
        v = Mathf.Clamp(v, 0, 100);

        switch (t)
        {
            case NeedType.Comida: _comida = v; break;
            case NeedType.Salud: _salud = v; break;
            case NeedType.Energia: _energia = v; break;
        }

        OnNeedChanged?.Invoke(t, v);

        if (v <= 0)
            OnAnyNeedZero?.Invoke(); // aquí disparas derrota si quieres
    }


    


}
