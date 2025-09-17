using UnityEngine;
using System;

[Serializable]
public class EfectoNecesidad : IEfectoDeItem
{
    public NecesidadTipo tipo = NecesidadTipo.Comida;
    public int cantidad = 20;

    public string Descripcion => $"+{cantidad} a {tipo}";

    public void Aplicar(EfectoContexto ctx)
    {
        if (ctx?.needs == null) return;
        ctx.needs.AplicarEfecto(tipo, cantidad);
    }
}
