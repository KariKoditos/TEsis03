using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager instancia;

    [Header("UI General")]
    public TMP_Text textoCreditos;       // Texto de cr�ditos
    public GameObject canvasTienda;      // Canvas completo de la tienda

    [Header("UI Inventario")]
    public Transform contenedorInventario;   // Donde aparecer�n los items
    public GameObject prefabItemInventario;

    private void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (canvasTienda != null)
            canvasTienda.SetActive(false); // Aseguramos que est� apagado al inicio

        ActualizarCreditos(JugadorFinanzas.instancia.creditos);
    }

    public void ActualizarCreditos(int cantidad)
    {
        if (textoCreditos != null)
        {
            textoCreditos.text = $"CR�DITOS: {cantidad}";
        }
    }

    // M�todos para manejar el Canvas de la tienda
    public void MostrarTienda()
    {
        if (canvasTienda != null)
        {
            canvasTienda.SetActive(true);
            Debug.Log("Canvas de tienda activado desde UIManager");
        }
    }

    public void OcultarTienda()
    {
        if (canvasTienda != null)
        {
            canvasTienda.SetActive(false);
            Debug.Log("Canvas de tienda desactivado desde UIManager");
        }
    }

    public void ActualizarInventarioUI(List<ItemEspacial> inventario)
    {
        // Borrar elementos anteriores
        foreach (Transform child in contenedorInventario)
        {
            Destroy(child.gameObject);
        }

        // Agregar nuevos elementos
        foreach (ItemEspacial item in inventario)
        {
            GameObject nuevoItem = Instantiate(prefabItemInventario, contenedorInventario);
            nuevoItem.GetComponentInChildren<TMP_Text>().text = item.nombre;
        }
    }
    
}

