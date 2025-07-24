using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ButtonPrecio : MonoBehaviour
{
    public TMP_Text texto;   // Texto que muestra nombre y precio
    private ItemEspacial item;

    public void Configurar(ItemEspacial nuevoItem)
    {
        item = nuevoItem;
        ActualizarTexto(item);
    }

    public void ActualizarTexto(ItemEspacial item)
    {
        texto.text = $"{item.nombre} - ${item.costo}";
    }

    public void Comprar()
    {
        JugadorFinanzas.instancia.Comprar(item);
        UIManager.instancia.ActualizarCreditos(JugadorFinanzas.instancia.creditos);
    }
}
