using UnityEngine;
using UnityEngine.UI;

public class NeedBarUI : MonoBehaviour
{
    public NeedType tipo;
    [Header("Usa uno u otro")]
    public Slider slider;
    bool _subscrito = false;


    void Start()
    {
        IntentarSuscribir();
        // fuerza valor inicial aunque el evento aún no haya disparado
        if (NeedsSystem.Instancia != null)
            ActualizarVisual(NeedsSystem.Instancia.GetValor(tipo));
    }

    void OnEnable()
    { 
        IntentarSuscribir(); 

    }

    void OnDisable()
    {
        if (NeedsSystem.Instancia != null)
            NeedsSystem.Instancia.OnNeedChanged -= OnNeedChanged;
    }

    void LateUpdate()
    {
        // plan B: si por alguna razón no llegan eventos, sincroniza por frame
        if (NeedsSystem.Instancia != null && !_subscrito)
            ActualizarVisual(NeedsSystem.Instancia.GetValor(tipo));
    }

    void IntentarSuscribir()
    {
        if (NeedsSystem.Instancia != null && !_subscrito)
        {
            NeedsSystem.Instancia.OnNeedChanged += OnNeedChanged;
            _subscrito = true;
        }
    }

    void OnNeedChanged(NeedType t, int valor)
    {
        if (t != tipo) return;
        ActualizarVisual(valor);
    }

    void ActualizarVisual(int valor)
    {
        float norm = Mathf.InverseLerp(0, 100, valor);

        if (slider != null)
        {
            slider.minValue = 0;
            slider.maxValue = 100;
            slider.value = valor;
        }

        

    }
}
