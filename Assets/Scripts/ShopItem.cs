using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ShopItem : MonoBehaviour
{
    [Header("Refs")]
    public Image icono;
    public TMP_Text textoNombre;
    public TMP_Text textoDescripcion;
    public TMP_Text textoCosto;
    public Button botonComprar;

    private ItemEspacial item;
    private Action<ItemEspacial> onBuy;

    public void Setup(ItemEspacial data, Action<ItemEspacial> onBuy)
    {
        item = data;
        this.onBuy = onBuy;

        if (icono) icono.sprite = data.icono;
        if (textoNombre) textoNombre.text = data.nombre;
        if (textoDescripcion) textoDescripcion.text = data.descripcion;
        if (textoCosto) textoCosto.text = "$" + data.costo;

        botonComprar.onClick.RemoveAllListeners();
        botonComprar.onClick.AddListener(() =>
        {
            var cmd = new ComprarCommand(item, item.costo);
            if (cmd.CanExecute())
            {
                cmd.Execute();

                onBuy?.Invoke(item);
            }
        });

        gameObject.SetActive(true);
        ActualizarInteractividad();
    }



    public void ActualizarInteractividad()
    {

        if (!gameObject.activeInHierarchy || item == null) return;

        bool tieneEspacio = JugadorFinanzas.instancia.inventario.Count < JugadorFinanzas.instancia.maxInventario;
        bool puedePagar = UIManager.instancia.GetCreditos() >= item.costo;

        botonComprar.interactable = (tieneEspacio && puedePagar);
    }

    public void RefrescarCosto()
    {
        if (textoCosto && item != null) textoCosto.text = $"${item.costo}";
        ActualizarInteractividad();
    }

    public void SetEmpty()
    {
        // deja visible si quieres, pero sin interactuar
        if (icono) icono.sprite = null;
        if (textoNombre) textoNombre.text = "-";
        if (textoDescripcion) textoDescripcion.text = "Sin producto";
        if (textoCosto) textoCosto.text = "-";
        if (botonComprar) botonComprar.interactable = false;
    }


}
