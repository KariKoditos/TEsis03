using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class NotificationInboxUI : MonoBehaviour
{
    [Header("Refs")]
    public GameObject panelHistorial;      // PanelNotificaciones
    public Transform contenedor;           // ScrollNotifs/Viewport/Content
    public GameObject prefabFila;          // NotifRow (prefab)

    [Header("Controles")]
    public KeyCode teclaToggle = KeyCode.N;      // abrir/cerrar panel
    public KeyCode teclaBorrarTodo = KeyCode.Delete; // borrar historial si el panel está abierto

    void Start()
    {
        if (panelHistorial) panelHistorial.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(teclaToggle))
            TogglePanel();

        if (panelHistorial && panelHistorial.activeSelf && Input.GetKeyDown(teclaBorrarTodo))
        {
            BorrarTodo();
        }
    }

    public void TogglePanel()
    {
        if (!panelHistorial) return;

        bool activo = !panelHistorial.activeSelf;
        panelHistorial.SetActive(activo);

        if (activo)
        {
            RefrescarLista();
            if (NotificationManager.Instancia != null)
                NotificationManager.Instancia.MarcarLeidas(); // apaga "badge" de nuevas
        }
    }

    public void RefrescarLista()
    {
        if (!contenedor || !prefabFila) return;
        // limpiar contenido actual
        for (int i = contenedor.childCount - 1; i >= 0; i--)
            Destroy(contenedor.GetChild(i).gameObject);

        if (NotificationManager.Instancia == null) return;

        // tu manager devuelve una lista de tuplas: (msg, tipo, ts)
        List<(string msg, NotificationType tipo, System.DateTime ts)> data =
            NotificationManager.Instancia.GetHistorialSnapshot();

        foreach (var n in data)
        {
            var go = Instantiate(prefabFila, contenedor);
            var msg = go.transform.Find("Mensaje")?.GetComponent<TMP_Text>();
            var hora = go.transform.Find("Hora")?.GetComponent<TMP_Text>();

            if (msg) msg.text = n.msg;
            if (hora) hora.text = n.ts.ToString("HH:mm");

            
        }

        // (Opcional) si quieres forzar actualización de layout:
        // Canvas.ForceUpdateCanvases();
    }

    
    public void BorrarTodo()
    {
        if (NotificationManager.Instancia != null)
        {
            NotificationManager.Instancia.ClearHistorial();
        }
        RefrescarLista();
    }
}
