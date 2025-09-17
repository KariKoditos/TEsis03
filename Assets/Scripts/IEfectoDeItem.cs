using UnityEngine;

public interface IEfectoDeItem
{
    // Aplica el efecto en el contexto del juego
    void Aplicar(EfectoContexto ctx);

    // Texto breve para UI / depuración
    string Descripcion { get; }
}

// Contexto que se pasa a cada efecto (evita dependencias rígidas)
public class EfectoContexto
{
    public JugadorFinanzas finanzas;
    public NeedsSystem needs;
    public EventsManager eventos;

    public ItemEspacial itemFuente;
    public GameObject usuarioGO; // si algún efecto necesita saber quién lo usó (no obligatorio)
}
