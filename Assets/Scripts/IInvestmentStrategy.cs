// InvestmentStrategy.cs
using UnityEngine;

public interface IInvestmentStrategy //Definetodas las estretegias 
{
    int CalcularRetorno(int montoInvertido);
    float Riesgo { get; } 
    string Nombre { get; }
}
