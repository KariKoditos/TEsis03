using UnityEngine;

public class BotonCompra : MonoBehaviour
{
    public ItemEspacial item;
    public JugadorFinanzas jugador;

    public void Comprar()
    {
        jugador.Comprar(item);
    }
}
