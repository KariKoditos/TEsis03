using UnityEngine;
using TMPro;

public class UICreditos : MonoBehaviour
{
    public TMP_Text textoCreditos;

    private void OnEnable()
    {
        Actualizar(JugadorFinanzas.instancia != null ? JugadorFinanzas.instancia.creditos : 0);
    }

    public void Actualizar(int cantidad)
    {
        if (textoCreditos != null)
            textoCreditos.text = $"CRÉDITOS: {cantidad}";
    }

    private void Update()
    {
        // opcional: refrescar en cada frame si no quieres llamar manualmente
        if (JugadorFinanzas.instancia != null)
            Actualizar(JugadorFinanzas.instancia.creditos);
    }
}
