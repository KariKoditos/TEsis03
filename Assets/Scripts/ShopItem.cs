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

    public void Setup(ItemEspacial data, Action<ItemEspacial> onBuy)
    {
        item = data;

        if (icono) icono.sprite = data.icono;
        if (textoNombre) textoNombre.text = data.nombre;
        if (textoDescripcion) textoDescripcion.text = data.descripcion;
        if (textoCosto) textoCosto.text = $"${data.costo}";

        botonComprar.onClick.RemoveAllListeners();
        botonComprar.onClick.AddListener(() => onBuy?.Invoke(item));

        ActualizarInteractividad();
    }

    public void ActualizarInteractividad()
    {
        bool tieneEspacio = JugadorFinanzas.instancia.inventario.Count < JugadorFinanzas.instancia.maxInventario;
        bool puedePagar = UIManager.instancia.GetCreditos() >= item.costo;

        botonComprar.interactable = (tieneEspacio && puedePagar);
    }
}
