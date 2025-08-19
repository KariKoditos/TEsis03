using UnityEngine;

public class BotonCompra : MonoBehaviour
{
    public ItemEspacial item;

    public void Comprar()
    {
        JugadorFinanzas.instancia.Comprar(item);
    }
}
