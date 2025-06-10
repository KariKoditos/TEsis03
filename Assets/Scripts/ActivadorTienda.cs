using UnityEngine;

public class ActivadorTienda : MonoBehaviour
{
    public TiendaEspacial tienda;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            tienda.AbrirTienda();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            tienda.CerrarTienda();
        }
    }
}
