using UnityEngine;
using UnityEngine.UI;

public class BotonInventario : MonoBehaviour
{
    public int indice;
    private Button boton;

    void Start()
    {
        boton = GetComponent<Button>();
        boton.onClick.AddListener(Seleccionar);
    }

    void Seleccionar()
    {
        var inventario = JugadorFinanzas.instancia.inventario;

        if (indice >= 0 && indice < inventario.Count)
        {
            var item = inventario[indice];
            UIManager.instancia.MostrarDetallesItem(item, indice);


        }
    }
}
