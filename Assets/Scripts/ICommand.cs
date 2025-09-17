using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICommand
{
    bool CanExecute();
    void Execute();
}

public abstract class PlayerCommandBase : ICommand
{
    protected JugadorFinanzas jugador => JugadorFinanzas.instancia;
    public abstract bool CanExecute();
    public abstract void Execute();
}
