using UnityEngine;

public interface IEfectoDeItem
{
    // Aplica el efecto en el contexto del juego
    void Aplicar(EfectoContexto ctx);

    // Texto breve para UI / depuraci�n
    string Descripcion { get; }
}

// Contexto que se pasa a cada efecto (evita dependencias r�gidas)
public class EfectoContexto
{
    public JugadorFinanzas finanzas;
    public NeedsSystem needs;
    public EventsManager eventos;

    public ItemEspacial itemFuente;
    public GameObject usuarioGO; // si alg�n efecto necesita saber qui�n lo us� (no obligatorio)
}
