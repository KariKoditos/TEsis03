using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComprarCommand : PlayerCommandBase
{
    readonly ItemEspacial _item;
    readonly int _precio;

    public ComprarCommand(ItemEspacial item, int precio)
    {
        _item = item; _precio = precio;
    }

    public override bool CanExecute()
    {
        return _item != null
            && jugador != null
            && jugador.inventario.Count < jugador.maxInventario
            && jugador.creditos >= _precio;
    }

    public override void Execute()
    {
        if (!CanExecute()) return;
        jugador.Comprar(_item, _precio);
    }
}

public class VenderCommand : PlayerCommandBase
{
    private readonly int _index;

    public VenderCommand(int index)
    {
        _index = index;
    }

    public override bool CanExecute()
    {
        return jugador != null &&
               _index >= 0 &&
               _index < jugador.inventario.Count;
    }

    public override void Execute()
    {
        if (CanExecute())
        {
            jugador.Vender(_index);
        }
    }
}

public class DepositarCommand : PlayerCommandBase
{
    private int _cantidad;

    public DepositarCommand(int cantidad)
    {
        _cantidad = cantidad;
    }

    public override bool CanExecute()
    {
        if (jugador == null)
        {
            return false;
        }

        if (_cantidad <= 0)
        {
            return false;
        }

        if (jugador.creditos < _cantidad)
        {
            return false;
        }

        return true;
    }

    public override void Execute()
    {
        if (CanExecute())
        {
            jugador.Depositar(_cantidad);
        }
    }
}

public class RetirarCommand : PlayerCommandBase
{
    private int _cantidad;

    public RetirarCommand(int cantidad)
    {
        _cantidad = cantidad;
    }

    public override bool CanExecute()
    {
        if (jugador == null)
        {
            return false;
        }

        if (_cantidad <= 0)
        {
            return false;
        }

        if (jugador.saldoAhorro < _cantidad)
        {
            return false;
        }

        return true;
    }

    public override void Execute()
    {
        if (CanExecute())
        {
            jugador.Retirar(_cantidad);
        }
    }
}

public class UsarItemCommand : PlayerCommandBase
{
    private readonly int _index;

    public UsarItemCommand(int index)
    {
        _index = index;
    }

    public override bool CanExecute()
    {
        return jugador != null &&
               _index >= 0 &&
               _index < jugador.inventario.Count;
    }

    public override void Execute()
    {
        if (CanExecute())
        {
            jugador.UsarItemPorIndice(_index);
        }
    }
}
