using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CartaInversion : MonoBehaviour
{
    public string nombreInversion;
    public int gananciaEsperada; // en porcentaje (ej. 20%)
    public bool esRiesgosa;
    public float probabilidadFracaso = 0.2f;

    public TMP_InputField inputCantidad;
    public Button btnInvertir;

    void Start()
    {
        btnInvertir.onClick.AddListener(ProcesarInversion);
    }

    void ProcesarInversion()
    {
        if (!int.TryParse(inputCantidad.text, out int cantidadInvertida))
        {
            Debug.Log("Ingreso inválido.");
            return;
        }

        if (JugadorFinanzas.instancia.creditos < cantidadInvertida)
        {
            Debug.Log("No tienes suficientes créditos.");
            return;
        }

        // Restar créditos al jugador
        JugadorFinanzas.instancia.creditos -= cantidadInvertida;

        // Calcular ganancia
        int ganancia = Mathf.RoundToInt(cantidadInvertida * (gananciaEsperada / 100f));
        JugadorFinanzas.instancia.creditos += cantidadInvertida + ganancia;

        // Actualizar UI
        UIManager.instancia.ActualizarCreditos(JugadorFinanzas.instancia.creditos);

        Debug.Log($"Invertiste {cantidadInvertida} en {nombreInversion} y ganaste {ganancia}.");
    }
}
