using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IntroPanelAutoHide : MonoBehaviour
{
    [Header("Panel de Controles / Ayuda")]
    [SerializeField] GameObject panel;           // Asigna el panel en el Inspector
    [SerializeField] float autoHideSeconds = 5f; // Solo en el primer arranque
    [SerializeField] bool usarTiempoNoEscalado = true;
    [SerializeField] KeyCode hotkey = KeyCode.T; // Tecla para abrir/cerrar

    

    const string SeenKey = "ControlsPanelSeen";  // PlayerPrefs flag
    Coroutine rutina;

 

    void Start()
    {
        // Mostrar solo la primera vez y auto-ocultar
        bool yaVisto = PlayerPrefs.GetInt(SeenKey, 0) == 1;

        if (!yaVisto)
        {
            Show();
            // auto-hide una sola vez
            if (rutina != null) StopCoroutine(rutina);
            rutina = StartCoroutine(AutoHideOnce());
            PlayerPrefs.SetInt(SeenKey, 1);
            PlayerPrefs.Save();
        }
        else
        {
            // En sesiones siguientes, arranca oculto pero disponible
            Hide();
        }
    }

    IEnumerator AutoHideOnce()
    {
        float t = 0f;
        while (t < autoHideSeconds)
        {
            t += usarTiempoNoEscalado ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }
        Hide();
        rutina = null;
    }

    void Update()
    {
        // Hotkey para abrir/cerrar en cualquier momento
        if (Input.GetKeyDown(hotkey))
            TogglePanel();
    }

    public void TogglePanel()
    {
        if (!panel) return;
        bool next = !panel.activeSelf;
        panel.SetActive(next);
        // Opcional: libera/bloquea cursor si tu juego es FPS
        Cursor.lockState = next ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = next;
    }

    public void Show()
    {
        if (!panel) return;
        panel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Hide()
    {
        if (!panel) return;
        panel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
