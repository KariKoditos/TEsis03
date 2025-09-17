using UnityEngine;

public class InversionSeguraStrategy : IInvestmentStrategy
{
    private readonly int _gananciaFija;
    public InversionSeguraStrategy(int gananciaFija) => _gananciaFija = gananciaFija;

    public float Riesgo => 0.1f;
    public string Nombre => "Segura";

    public int CalcularRetorno(int montoInvertido) 
    {
        return Mathf.Max(0, _gananciaFija);
    }
}

public class InversionRiesgosaStrategy : IInvestmentStrategy
{   
    private readonly float _minPct;
    private readonly float _maxPct;

    public InversionRiesgosaStrategy(float minPct = -0.10f, float maxPct = 0.20f)
    {
        _minPct = minPct; _maxPct = maxPct;
    }

    public float Riesgo => 0.6f;
    public string Nombre => "Riesgosa";

    public int CalcularRetorno(int montoInvertido)
    {
        float r = UnityEngine.Random.Range(_minPct, _maxPct);
        return Mathf.RoundToInt(montoInvertido * r); // puede q pierda
    }
}
