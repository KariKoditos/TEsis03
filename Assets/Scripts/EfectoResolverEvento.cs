using UnityEngine;
using System;

[Serializable]
public class EfectoResolverEvento : IEfectoDeItem
{
    // Modo 1: por tipo de item (ej: “cualquier Prevención”)
    public bool resolverPorTipo = true;
    public TipoItem tipoRequerido = TipoItem.Prevención;

    // Modo 2: por asset exacto (ej: “SO Extintor”)
    public bool resolverPorAsset = false;

    public string Descripcion => resolverPorAsset ? "Resuelve evento (por asset)" : $"Resuelve evento (tipo {tipoRequerido})";

    public void Aplicar(EfectoContexto ctx)
    {
        if (ctx?.eventos == null || ctx.itemFuente == null) return;

        // El EventsManager ya decide si este item resuelve o no:
        // Decorator aquí es “intento” de resolver (no consume aquí)
        ctx.eventos.OnItemUsed(ctx.itemFuente);
    }
}
